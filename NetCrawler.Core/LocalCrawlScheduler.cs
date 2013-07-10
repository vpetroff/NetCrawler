using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NetCrawler.Core.Configuration;

namespace NetCrawler.Core
{
	public class LocalCrawlScheduler : ICrawlScheduler
	{
		private readonly IConfiguration configuration;
		private readonly IPageDownloader pageDownloader;
		private readonly IHtmlParser htmlParser;
		private readonly ConcurrentDictionary<byte[], CrawlUrl> urls = new ConcurrentDictionary<byte[], CrawlUrl>(); // {hash, url}
		private readonly ConcurrentBag<WebsiteBlock> websites = new ConcurrentBag<WebsiteBlock>();
		private readonly object websiteLock = new object();
		private ActionBlock<PageCrawlResult> schedulingBlock;

		public LocalCrawlScheduler(IConfiguration configuration, IPageDownloader pageDownloader, IHtmlParser htmlParser)
		{
			this.configuration = configuration;
			this.pageDownloader = pageDownloader;
			this.htmlParser = htmlParser;

			schedulingBlock = new ActionBlock<PageCrawlResult>(result =>
				{
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

		public Task<CrawlResult> Schedule(Website website)
		{
			var existing = websites.FirstOrDefault(x => x.Website == website);
			if (existing != null)
				return existing.CompletionSource.Task;

			WebsiteBlock websiteBlock;
			lock (websiteLock)
			{
				var processingBlock = new TransformBlock<CrawlUrl, PageCrawlResult>(crawlUrl =>
					{
						Console.WriteLine(crawlUrl.Url);
						var result = new SinglePageCrawler(htmlParser, pageDownloader).Crawl(crawlUrl.Url);
						result.CrawlUrl = crawlUrl;

						Interlocked.Increment(ref crawlUrl.Website.ProcessedUrlsCount);
						return result;
					}, new ExecutionDataflowBlockOptions
						{
							MaxDegreeOfParallelism =
								website.MaxConcurrentConnections > 0
									? website.MaxConcurrentConnections
									: configuration.MaxConcurrentConnectionsPerWebsite
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

			Schedule(new[]{ website.RootUrl });

			return websiteBlock.CompletionSource.Task;
		}

		public int Schedule(IEnumerable<string> crawlUrls)
		{
			var hashes = new Dictionary<byte[], string>(crawlUrls.Select(x => x.Split('#')[0].TrimEnd('/')).Where(x => !string.IsNullOrWhiteSpace(x)).ToDictionary(CalculateHash));
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

		private byte[] CalculateHash(string s)
		{
			return SHA256Managed.Create().ComputeHash(Encoding.UTF8.GetBytes(s));
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