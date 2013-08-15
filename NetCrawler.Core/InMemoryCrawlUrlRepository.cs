using System.Collections.Concurrent;
using System.Linq;

namespace NetCrawler.Core
{
	public class InMemoryCrawlUrlRepository : ICrawlUrlRepository
	{
		private const int MaxQueueLength = 50;

		private readonly ConcurrentDictionary<string, string> done = new ConcurrentDictionary<string, string>();
		private readonly ConcurrentDictionary<string, CrawlUrl> scheduled = new ConcurrentDictionary<string, CrawlUrl>(); // {hash, url}
		private readonly ConcurrentQueue<CrawlUrl> scheduledQueue = new ConcurrentQueue<CrawlUrl>();
		private readonly ConcurrentDictionary<string, CrawlUrl> working = new ConcurrentDictionary<string, CrawlUrl>(); // {hash, url}

		private readonly object loadingLock = new object();

		public bool IsKnown(string key)
		{
			return scheduled.ContainsKey(key) || done.ContainsKey(key) || working.ContainsKey(key);
		}

		public bool TryAdd(string key, CrawlUrl crawlUrl)
		{
			return scheduled.TryAdd(key, crawlUrl);
		}

		public void Done(string key)
		{
			if (done.TryAdd(key, ""))
			{
				CrawlUrl crawlUrl;
				scheduled.TryRemove(key, out crawlUrl);
			}
		}

		public CrawlUrl PeekNext()
		{
			CrawlUrl next = null;
			if (scheduledQueue.TryPeek(out next))
			{
				return next;
			}

			LoadMore();

			scheduledQueue.TryPeek(out next);
			return next;
		}

		private void LoadMore()
		{
			lock (loadingLock)
			{
				var newUrls = scheduled.Take(MaxQueueLength).ToList();

				foreach (var newUrl in newUrls)
				{
					scheduledQueue.Enqueue(newUrl.Value);
					CrawlUrl next;
					scheduled.TryRemove(newUrl.Key, out next);
				}
			}
		}

		public CrawlUrl Next()
		{
			CrawlUrl next;
			if (scheduledQueue.TryDequeue(out next))
			{
				working.TryAdd(next.Hash, next);
				return next;
			}
			
			LoadMore();

			if (scheduledQueue.TryDequeue(out next))
			{
				working.TryAdd(next.Hash, next);
				return next;
			}

			return null;
		}
	}
}