using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCrawler.Core
{
	public class PageCrawlResult
	{
		public PageCrawlResult()
		{
			CrawlStartedAt = DateTimeOffset.UtcNow;
			Links = Enumerable.Empty<string>();
		}

		public DateTimeOffset CrawlStartedAt { get; set; }
		public DateTimeOffset CrawlEndedAt { get; set; }

		public string Contents { get; set; }
		public IEnumerable<string> Links { get; set; }

		public CrawlUrl CrawlUrl { get; set; }
	}
}