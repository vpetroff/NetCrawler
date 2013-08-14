using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NetCrawler.Core
{
	public class PageCrawlResult
	{
		public PageCrawlResult()
		{
			CrawlStartedAt = DateTimeOffset.Now;
			Links = Enumerable.Empty<string>();
		}

		public DateTimeOffset CrawlStartedAt { get; set; }
		public DateTimeOffset CrawlEndedAt { get; set; }

		public HttpStatusCode StatusCode { get; set; }

		public string Contents { get; set; }
		public IEnumerable<string> Links { get; set; }

		public CrawlUrl CrawlUrl { get; set; }

	}
}