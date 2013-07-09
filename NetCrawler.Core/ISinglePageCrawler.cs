namespace NetCrawler.Core
{
	public interface ISinglePageCrawler
	{
		PageCrawlResult Crawl(string url);
	}
}