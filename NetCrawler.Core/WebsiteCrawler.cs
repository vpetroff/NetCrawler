using System.Threading.Tasks;

namespace NetCrawler.Core
{
	public class WebsiteCrawler : IWebsiteCrawler
	{
		private readonly ICrawlScheduler crawlScheduler;

		public WebsiteCrawler(ICrawlScheduler crawlScheduler)
		{
			this.crawlScheduler = crawlScheduler;
		}

		public async Task<CrawlResult> RunAsync(Website target)
		{
			return await crawlScheduler.Schedule(target);
		}
	}
}