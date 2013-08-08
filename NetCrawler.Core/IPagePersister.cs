namespace NetCrawler.Core
{
	public interface IPagePersister
	{
		void Save(PageCrawlResult pageCrawlResult);
	}
}