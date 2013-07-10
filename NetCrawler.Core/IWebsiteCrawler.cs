using System.Threading.Tasks;

namespace NetCrawler.Core
{
	public interface IWebsiteCrawler
	{
		Task<CrawlResult> RunAsync(Website target);
	}
}