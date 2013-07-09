using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace NetCrawler.Core
{
	public class LocalCrawlScheduler : ICrawlScheduler
	{
		private readonly List<Website> websites = new List<Website>();

		public LocalCrawlScheduler()
		{
		}

		public void Schedule(Website website)
		{
			if (websites.Contains(website))
				return;

			websites.Add(website);
		}
	}
}