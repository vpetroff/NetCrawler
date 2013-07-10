using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCrawler.Core
{
	public interface ICrawlScheduler
	{
		Task<CrawlResult> Schedule(Website website);
		int Schedule(IEnumerable<string> urls);
	}
}