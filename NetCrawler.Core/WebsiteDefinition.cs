namespace NetCrawler.Core
{
	public class WebsiteDefinition
	{
		public int UrlsToProcessCount;
		public int ProcessedUrlsCount;

		public Website Website { get; set; }

		public CrawlResult CrawlResult { get; set; }
	}
}