using System;

namespace NetCrawler.Core
{
	public class Website
	{
		public string RootUrl { get; set; }

		public bool FollowExternalLinks { get; set; }
		public int MaxConcurrentConnections { get; set; }

		public DateTimeOffset LastVisit { get; set; }
		public TimeSpan IntervalBetweenVisits { get; set; }
	}
}