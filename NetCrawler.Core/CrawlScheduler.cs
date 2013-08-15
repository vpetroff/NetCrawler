using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NetCrawler.Core.Configuration;
using log4net;

namespace NetCrawler.Core
{
	public class CrawlScheduler : ICrawlScheduler
	{
		private readonly IUrlHasher urlHasher;
		private readonly IConfiguration configuration;
		private readonly ISinglePageCrawler pageCrawler;
		private readonly ICrawlUrlRepository crawlUrlRepository;
		private readonly ConcurrentBag<WebsiteDefinition> websiteDefinitions = new ConcurrentBag<WebsiteDefinition>();
		private readonly ConcurrentDictionary<WebsiteDefinition, WebsiteProcessingDefinition> websiteProcessingDefinitions = new ConcurrentDictionary<WebsiteDefinition, WebsiteProcessingDefinition>();
		private readonly object websiteLock = new object();
		private readonly ActionBlock<PageCrawlResult> schedulingBlock;

		private readonly ILog log = LogManager.GetLogger(typeof (CrawlScheduler));

		public CrawlScheduler(IUrlHasher urlHasher, IConfiguration configuration, ISinglePageCrawler pageCrawler, ICrawlUrlRepository crawlUrlRepository)
		{
			this.urlHasher = urlHasher;
			this.configuration = configuration;
			this.pageCrawler = pageCrawler;
			this.crawlUrlRepository = crawlUrlRepository;

			schedulingBlock = new ActionBlock<PageCrawlResult>(result =>
				{
					var websiteDefinition = result.CrawlUrl.WebsiteDefinition;

					Interlocked.Increment(ref websiteDefinition.ProcessedUrlsCount);

					RaisePageCrawled(result);
					
					if (result.Links.Any())
					{
						Schedule(result.Links);
					}

					if (websiteDefinition.UrlsToProcessCount == websiteDefinition.ProcessedUrlsCount)
					{
						websiteProcessingDefinitions[websiteDefinition].Complete();
					}

					ScheduleNext();
				});
		}

		private void ScheduleNext()
		{
			WebsiteDefinition websiteDefinition;

			do
			{
				websiteDefinition = null;
				var next = crawlUrlRepository.Next();
				if (next != null)
				{
					websiteDefinition = websiteDefinitions.FirstOrDefault(x => x.Website.IsRelativeUrl(next) && x.Website.MaxConcurrentConnections > x.UrlsInProcess);
					if (websiteDefinition != null)
					{
						var inputCount = websiteProcessingDefinitions[websiteDefinition].Post(next);
						log.DebugFormat("Process block has {0} pending messages", inputCount);

						Interlocked.Increment(ref websiteDefinition.UrlsInProcess);

						RaisePageProcessing(next);
					}
				}
			} while (websiteDefinition != null);
		}

		public event Action<CrawlUrl> PageScheduled;
		public event Action<CrawlUrl> PageProcessing;
		public event Action<PageCrawlResult> PageCrawled;
		public event Action<Website> WebsiteScheduled;
		
		private void RaisePageScheduled(CrawlUrl crawlUrl)
		{
			var handler = PageScheduled;
			if (handler != null)
				handler.Invoke(crawlUrl);
		}

		private void RaisePageProcessing(CrawlUrl crawlUrl)
		{
			var handler = PageProcessing;
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
			var existing = websiteDefinitions.FirstOrDefault(x => x.Website == website);
			if (existing != null)
				return websiteProcessingDefinitions[existing].CompletionSource.Task;

			if (website == null || string.IsNullOrWhiteSpace(website.RootUrl))
			{
				var cancelledTask = new TaskCompletionSource<CrawlResult>();
				cancelledTask.SetCanceled();

				return cancelledTask.Task;
			}

			website.RootUrl = website.RootUrl.Split('#')[0].TrimEnd('/');

			WebsiteProcessingDefinition websiteProcessingDefinition;
			lock (websiteLock)
			{
				var processingBlock = CreateProcessingBlock(website);

				var websiteDefinition = new WebsiteDefinition
					{
						Website = website, 
						CrawlResult = new CrawlResult(),
					};
				
				websiteProcessingDefinition = new WebsiteProcessingDefinition(websiteDefinition)
					{
						ProcessingBlock = processingBlock,
						CompletionSource = new TaskCompletionSource<CrawlResult>()
					};

				if (websiteProcessingDefinitions.TryAdd(websiteDefinition, websiteProcessingDefinition))
				{
					websiteDefinitions.Add(websiteDefinition);
				}
			}

			RaiseWebsiteScheduled(website);

			var outstandingLinks = Schedule(new[] { website.RootUrl });
			if (outstandingLinks > 0)
				ScheduleNext();

			return websiteProcessingDefinition.CompletionSource.Task;
		}

		private TransformBlock<CrawlUrl, PageCrawlResult> CreateProcessingBlock(Website website)
		{
			var processingBlock = new TransformBlock<CrawlUrl, PageCrawlResult>(crawlUrl =>
				{
					var result = pageCrawler.Crawl(crawlUrl.Uri);
					result.CrawlUrl = crawlUrl;

					return result;
				}, new ExecutionDataflowBlockOptions
					{
						MaxDegreeOfParallelism =
							website.MaxConcurrentConnections > 0
								? website.MaxConcurrentConnections
								: configuration.MaxConcurrentConnectionsPerWebsite,
					});

			var persistBlock = new TransformBlock<PageCrawlResult, PageCrawlResult>(result =>
				{
					crawlUrlRepository.Done(result.CrawlUrl.Hash);

					Interlocked.Decrement(ref result.CrawlUrl.WebsiteDefinition.UrlsInProcess);

					return result;
				});

			processingBlock.LinkTo(persistBlock);
			persistBlock.LinkTo(schedulingBlock);

			return processingBlock;
		}

		public int Schedule(IEnumerable<string> crawlUrls)
		{
			var hashes = crawlUrls.Select(x => x.Split('#')[0].TrimEnd('/')).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToDictionary(urlHasher.CalculateHashAsString);
			var scheduledLinksCount = 0;

			foreach (var hash in hashes)
			{
				if (crawlUrlRepository.IsKnown(hash.Key))
					continue;

				try
				{
					var crawlUrl = new CrawlUrl
						{
							Hash = hash.Key,
							Url = hash.Value,
						};

					var websiteDefinition = websiteDefinitions.FirstOrDefault(x => x.Website.IsRelativeUrl(crawlUrl));
					if (websiteDefinition != null)
					{
						crawlUrl.WebsiteDefinition = websiteDefinition;
						if (crawlUrlRepository.TryAdd(hash.Key, crawlUrl))
						{
							Interlocked.Increment(ref websiteDefinition.UrlsToProcessCount);
							Interlocked.Increment(ref scheduledLinksCount);

/*
							var inputCount = websiteProcessingDefinitions[websiteDefinition].Post(crawlUrl);
							log.DebugFormat("Process block has {0} pending messages", inputCount);

*/
							RaisePageScheduled(crawlUrl);
						}
					}
				}
				catch (Exception ex)
				{
					log.Error(ex);
				}
			}

			return scheduledLinksCount;
		}
	}
}