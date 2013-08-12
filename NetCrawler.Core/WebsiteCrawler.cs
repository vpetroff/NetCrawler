using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NetCrawler.Core
{
	public class WebsiteCrawler : IWebsiteCrawler
	{
		private readonly ICrawlScheduler crawlScheduler;
		private readonly ICrawlPersister crawlPersister;
		private readonly ConcurrentDictionary<byte[], CrawlUrl> urls = new ConcurrentDictionary<byte[], CrawlUrl>(); // {hash, url}

		public WebsiteCrawler(ICrawlScheduler crawlScheduler, ICrawlPersister crawlPersister)
		{
			this.crawlScheduler = crawlScheduler;
			this.crawlPersister = crawlPersister;

			crawlScheduler.PageScheduled += crawlUrl =>
				{
					urls.TryAdd(crawlUrl.Hash, crawlUrl);

					Console.WriteLine("Scheduled '{0}' - total '{1}'", crawlUrl.Url, urls.Count);
				};

			crawlScheduler.PageCrawled += crawlResult =>
				{
					try
					{
						CrawlUrl url;
						urls.TryRemove(crawlResult.CrawlUrl.Hash, out url);

						Console.WriteLine("Crawled '{0}' - left '{1}'", crawlResult.CrawlUrl.Url, urls.Count);
						crawlPersister.Save(crawlResult);

						crawlResult.CrawlUrl.Website.Website.LastVisit = DateTimeOffset.Now;
						crawlPersister.Save(crawlResult.CrawlUrl.Website.Website);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				};

			crawlScheduler.WebsiteScheduled += website =>
				{
					try
					{
						Console.WriteLine("Added website " + website.RootUrl);
						website.LastCrawlStartedAt = DateTimeOffset.Now;
						website.PagesCrawled = 0;

						crawlPersister.Save(website);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				};
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
				Console.WriteLine("Finished crawl for website " + website.RootUrl);
				website.LastCrawlEndedAt = website.LastVisit;
				website.PagesCrawled = result.NumberOfPagesCrawled;

				crawlPersister.Save(website);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}