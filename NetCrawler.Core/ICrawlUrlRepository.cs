namespace NetCrawler.Core
{
	public interface ICrawlUrlRepository
	{
		bool Contains(string key);
		bool TryAdd(string key, CrawlUrl crawlUrl);
	}
}