using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NetCrawler.Core.Configuration;

namespace NetCrawler.Core
{
	public class LocalCrawlScheduler : ICrawlScheduler
	{
		private readonly IUrlHasher urlHasher;
		private readonly IConfiguration configuration;
		private readonly ISinglePageCrawler pageCrawler;
		private readonly ConcurrentDictionary<byte[], CrawlUrl> urls = new ConcurrentDictionary<byte[], CrawlUrl>(); // {hash, url}
		private readonly ConcurrentBag<WebsiteBlock> websites = new ConcurrentBag<WebsiteBlock>();
		private readonly object websiteLock = new object();
		private readonly ActionBlock<PageCrawlResult> schedulingBlock;

		public LocalCrawlScheduler(IUrlHasher urlHasher, IConfiguration configuration, ISinglePageCrawler pageCrawler)
		{
			this.urlHasher = urlHasher;
			this.configuration = configuration;
			this.pageCrawler = pageCrawler;

			schedulingBlock = new ActionBlock<PageCrawlResult>(result =>
				{
					Interlocked.Increment(ref result.CrawlUrl.Website.ProcessedUrlsCount);

					RaisePageCrawled(result);
					
					var scheduledLinksCount = 0;

					if (result.Links.Any())
					{
						scheduledLinksCount = Schedule(result.Links);
					}

					if (scheduledLinksCount == 0)
					{
						if (result.CrawlUrl.Website.UrlsToProcessCount == result.CrawlUrl.Website.ProcessedUrlsCount)
							result.CrawlUrl.Website.Complete();
					}
				});
		}

		public event Action<CrawlUrl> PageScheduled;
		public event Action<PageCrawlResult> PageCrawled;
		public event Action<Website> WebsiteScheduled;
		
		private void RaisePageScheduled(CrawlUrl crawlUrl)
		{
			var handler = PageScheduled;
			if (handler != null)
				handler.Invoke(crawlUrl);
		}

		private void RaisePageCrawled(PageCrawlResult result)
		{
			var handler = PageCrawled;
			if (handler != null)
				handler.Invoke(result);
		}

		private void RaiseWebsiteScheduled(Website website)
		{
			var handler = WebsiteScheduled;
			if (handler != null)
				handler.Invoke(website);
		}

		public Task<CrawlResult> Schedule(Website website)
		{
			var existing = websites.FirstOrDefault(x => x.Website == website);
			if (existing != null)
				return existing.CompletionSource.Task;

			if (website == null || string.IsNullOrWhiteSpace(website.RootUrl))
			{
				var cancelledTask = new TaskCompletionSource<CrawlResult>();
				cancelledTask.SetCanceled();

				return cancelledTask.Task;
			}

			website.RootUrl = website.RootUrl.Split('#')[0].TrimEnd('/');

			WebsiteBlock websiteBlock;
			lock (websiteLock)
			{
				var processingBlock = new TransformBlock<CrawlUrl, PageCrawlResult>(crawlUrl =>
					{
						var result = pageCrawler.Crawl(crawlUrl.Url);
						result.CrawlUrl = crawlUrl;

						return result;
					}, new ExecutionDataflowBlockOptions
						{
							MaxDegreeOfParallelism =
								website.MaxConcurrentConnections > 0
									? website.MaxConcurrentConnections
									: configuration.MaxConcurrentConnectionsPerWebsite,
						});

				processingBlock.LinkTo(schedulingBlock);
				
				websiteBlock = new WebsiteBlock
					{
						Website = website, 
						CrawlResult = new CrawlResult(),
						ProcessingBlock = processingBlock, 
						CompletionSource = new TaskCompletionSource<CrawlResult>()
					};
				websites.Add(websiteBlock);
			}

			RaiseWebsiteScheduled(website);

			Schedule(new[] { website.RootUrl });

			return websiteBlock.CompletionSource.Task;
		}

		public int Schedule(IEnumerable<string> crawlUrls)
		{
			var hashes = new Dictionary<byte[], string>(crawlUrls.Select(x => x.Split('#')[0].TrimEnd('/')).Where(x => !string.IsNullOrWhiteSpace(x)).ToDictionary(urlHasher.CalculateHash));
			var scheduledLinksCount = 0;

			foreach (var hash in hashes)
			{
				if (urls.Keys.Any(x => hash.Key.SequenceEqual(x)))
					continue;

				try
				{
					var crawlUrl = new CrawlUrl
						{
							Hash = hash.Key,
							Base64Hash = Convert.ToBase64String(hash.Key).Replace('+', '-').Replace('/', '_'),
							Url = hash.Value,
						};

					var website = websites.FirstOrDefault(x => x.Website.IsRelativeUrl(crawlUrl));
					if (website != null)
					{
						crawlUrl.Website = website;
						if (urls.TryAdd(hash.Key, crawlUrl))
						{
							Interlocked.Increment(ref website.UrlsToProcessCount);
							Interlocked.Increment(ref scheduledLinksCount);
							website.ProcessingBlock.Post(crawlUrl);

							RaisePageScheduled(crawlUrl);
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}

			return scheduledLinksCount;
		}
	}

	public class WebsiteBlock
	{
		public int UrlsToProcessCount;
		public int ProcessedUrlsCount;

		public Website Website { get; set; }
		public ITargetBlock<CrawlUrl> ProcessingBlock { get; set; }

		public TaskCompletionSource<CrawlResult> CompletionSource { get; set; }

		public CrawlResult CrawlResult { get; set; }

		public void Complete()
		{
			ProcessingBlock.Complete();

			CrawlResult.NumberOfPagesCrawled = ProcessedUrlsCount;
			CrawlResult.CrawlEnded = DateTime.Now;

			CompletionSource.SetResult(CrawlResult);
		}
	}

	public class CrawlUrl
	{
		public byte[] Hash { get; set; }
		public string Base64Hash { get; set; }

		private string url;
		public string Url
		{
			get { return url; }
			set
			{
				url = value;
				Uri = new Uri(url);
			}
		}

		public Uri Uri { get; private set; }

		public WebsiteBlock Website { get; set; }
	}
}