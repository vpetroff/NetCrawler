namespace NetCrawler.Core
{
	public class WebsiteCrawler : IWebsiteCrawler
	{
		private readonly ICrawlScheduler crawlScheduler;

		public WebsiteCrawler(ICrawlScheduler crawlScheduler)
		{
			this.crawlScheduler = crawlScheduler;
		}

		public CrawlResult Run(Website target)
		{
			crawlScheduler.Schedule(target);

			return new CrawlResult();
		}
	}
}