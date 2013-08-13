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
		private int urlsCount;

		private static readonly ILog Log = LogManager.GetLogger(typeof(WebsiteCrawler));

		public WebsiteCrawler(ICrawlScheduler crawlScheduler, ICrawlPersister crawlPersister)
		{
			this.crawlScheduler = crawlScheduler;
			this.crawlPersister = crawlPersister;

			crawlScheduler.PageScheduled += crawlUrl =>
				{
					Interlocked.Increment(ref urlsCount);

					Log.InfoFormat("Scheduled '{0}' - total '{1}'", crawlUrl.Url, urlsCount);
				};

			crawlScheduler.PageCrawled += crawlResult =>
				{
					try
					{
						Interlocked.Decrement(ref urlsCount);

						Log.InfoFormat("Crawled '{0}' - left '{1}'", crawlResult.CrawlUrl.Url, urlsCount);
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