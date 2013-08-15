using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCrawler.Core
{
	public interface ICrawlScheduler
	{
		Task<CrawlResult> Schedule(Website website);
		int Schedule(IEnumerable<string> urls);

		event Action<PageCrawlResult> PageCrawled;
		event Action<Website> WebsiteScheduled;
		event Action<CrawlUrl> PageScheduled;
		event Action<CrawlUrl> PageProcessing;
	}
}