using System;

namespace NetCrawler.Core
{
	public class Website
	{
		private Uri rootUri;
		private string rootUrl;

		public string RootUrl
		{
			get { return rootUrl; }
			set
			{
				rootUrl = value;
				rootUri = new Uri(rootUrl);
			}
		}

		public bool FollowExternalLinks { get; set; }
		public int MaxConcurrentConnections { get; set; }

		public DateTimeOffset LastVisit { get; set; }
		public TimeSpan IntervalBetweenVisits { get; set; }

		public DateTimeOffset LastCrawlStartedAt { get; set; }
		public DateTimeOffset LastCrawlEndedAt { get; set; }
		public int PagesCrawled { get; set; }

		public bool IsRelativeUrl(CrawlUrl crawlUrl)
		{
			return crawlUrl.Uri.AbsoluteUri.StartsWith(rootUri.AbsoluteUri);

//			return rootUri.IsBaseOf(crawlUrl.Uri);
		}
	}
}