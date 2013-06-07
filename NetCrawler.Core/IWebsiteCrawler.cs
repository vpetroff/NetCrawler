namespace NetCrawler.Core
{
	public interface IWebsiteCrawler
	{
		CrawlResult Run(Website target);
	}
}