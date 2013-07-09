using System;

namespace NetCrawler.Core
{
	public class CrawlResult
	{
		public DateTimeOffset CrawlStarted { get; set; }
		public DateTimeOffset CrawlEnded { get; set; }

		public int NumberOfPagesCrawled { get; set; }
	}
}