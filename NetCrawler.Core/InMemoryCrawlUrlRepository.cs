using System.Collections.Concurrent;

namespace NetCrawler.Core
{
	public class InMemoryCrawlUrlRepository : ICrawlUrlRepository
	{
		private readonly ConcurrentDictionary<string, CrawlUrl> urls = new ConcurrentDictionary<string, CrawlUrl>(); // {hash, url}

		public bool IsKnown(string key)
		{
			return urls.ContainsKey(key);
		}

		public bool TryAdd(string key, CrawlUrl crawlUrl)
		{
			return urls.TryAdd(key, crawlUrl);
		}
	}
}