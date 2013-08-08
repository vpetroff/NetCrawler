using System;
using System.Threading.Tasks;

namespace NetCrawler.Core
{
	public class WebsiteCrawler : IWebsiteCrawler
	{
		private readonly ICrawlScheduler crawlScheduler;

		public WebsiteCrawler(ICrawlScheduler crawlScheduler, IPagePersister pagePersister)
		{
			this.crawlScheduler = crawlScheduler;

			crawlScheduler.PageCrawledEventHandler += (sender, result) =>
				{
					try
					{
						pagePersister.Save(result);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				};
		}

		public async Task<CrawlResult> RunAsync(Website target)
		{
			return await crawlScheduler.Schedule(target);
		}
	}
}