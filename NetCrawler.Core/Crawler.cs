using NetCrawler.Core.Configuration;

namespace NetCrawler.Core
{
	public class Crawler : ICrawler
	{
		internal Crawler(IConfiguration configuration)
		{
		}

		public CrawlResult Run(Website target)
		{
			return new CrawlResult();
		}
	}

	public interface ICrawler
	{
		CrawlResult Run(Website target);
	}

	public class Website
	{
		public string Url { get; set; }
	}

	public class CrawlResult
	{
	}
}