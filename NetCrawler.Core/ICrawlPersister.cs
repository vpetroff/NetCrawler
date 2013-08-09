namespace NetCrawler.Core
{
	public interface ICrawlPersister
	{
		void Save(PageCrawlResult pageCrawlResult);
		void Save(Website website);
	}
}