using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCrawler.Core
{
	public interface IWebsiteCrawler
	{
		Task<CrawlResult> RunAsync(Website target);
		Task<CrawlResult[]> RunAsync(IEnumerable<Website> targets);
	}
}