using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace NetCrawler.Core
{
	public class WebsiteCrawler : IWebsiteCrawler
	{
		private readonly ICrawlScheduler crawlScheduler;
		private readonly ICrawlPersister crawlPersister;
		private int scheduledUrlsCount;
		private int processingUrlsCount;

		private static readonly ILog Log = LogManager.GetLogger(typeof(WebsiteCrawler));

		public WebsiteCrawler(ICrawlScheduler crawlScheduler, ICrawlPersister crawlPersister)
		{
			this.crawlScheduler = crawlScheduler;
			this.crawlPersister = crawlPersister;

			crawlScheduler.PageScheduled += crawlUrl =>
				{
					Interlocked.Increment(ref scheduledUrlsCount);

					Log.DebugFormat("Scheduled '{0}' - scheduled '{1}', processing '{2}'", crawlUrl.Url, scheduledUrlsCount, processingUrlsCount);
				};

			crawlScheduler.PageProcessing += crawlUrl =>
				{
					Interlocked.Increment(ref processingUrlsCount);

					Log.InfoFormat("Processing '{0}' - scheduled '{1}', processing '{2}'", crawlUrl.Url, scheduledUrlsCount, processingUrlsCount);
				};

			crawlScheduler.PageCrawled += crawlResult =>
				{
					try
					{
						Interlocked.Decrement(ref processingUrlsCount);
						Interlocked.Decrement(ref scheduledUrlsCount);

						Log.InfoFormat("Crawled '{0}' - scheduled '{1}', processing '{2}'", crawlResult.CrawlUrl.Url, scheduledUrlsCount, processingUrlsCount);

						crawlPersister.Save(crawlResult);

						crawlResult.CrawlUrl.WebsiteDefinition.Website.LastVisit = DateTimeOffset.Now;
						crawlPersister.Save(crawlResult.CrawlUrl.WebsiteDefinition.Website);
					}
					catch (Exception ex)
					{
						Log.Error(ex);
					}
				};

			crawlScheduler.WebsiteScheduled += website =>
				{
					try
					{
						Log.InfoFormat("Added website {0}", website.RootUrl);

						website.LastCrawlStartedAt = DateTimeOffset.Now;
						website.PagesCrawled = 0;

						crawlPersister.Save(website);
					}
					catch (Exception ex)
					{
						Log.Error(ex);
					}
				};
		}

		public Task<CrawlResult[]> RunAsync(IEnumerable<Website> targets)
		{
			return Task.WhenAll(targets.Select(RunAsync));
		}

		public async Task<CrawlResult> RunAsync(Website target)
		{
			return await crawlScheduler.Schedule(target).ContinueWith(t =>
				{
					target.LastVisit = DateTimeOffset.Now;

					WebsiteCrawlFinished(target, t.Result);

					return t.Result;
				});
		}

		private void WebsiteCrawlFinished(Website website, CrawlResult result)
		{
			try
			{
				Log.InfoFormat("Finished crawl for website {0}", website.RootUrl);
				website.LastCrawlEndedAt = website.LastVisit;
				website.PagesCrawled = result.NumberOfPagesCrawled;

				crawlPersister.Save(website);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}
	}
}