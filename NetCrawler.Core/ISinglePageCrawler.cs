using System;

namespace NetCrawler.Core
{
	public interface ISinglePageCrawler
	{
		PageCrawlResult Crawl(Uri url);
	}
}