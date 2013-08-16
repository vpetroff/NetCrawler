using System.Linq;
using NetCrawler.Core;
using ServiceStack.Redis;

namespace NetCrawler.Redis
{
	public class RedisCrawlUrlRepository : ICrawlUrlRepository
	{
		private const string WorkingHashId = "working:crawlUrl";
		private const string DoneHashId = "done:crawlUrl";
		private const string ScheduledHashId = "scheduled:crawlUrl";
		private const string ScheduledListId = "scheduled:hash";

		private readonly BasicRedisClientManager clientManager;

		public RedisCrawlUrlRepository()
		{
			clientManager = new BasicRedisClientManager("localhost:6379");
			Client.FlushAll();
		}

		public IRedisClient Client
		{
			get { return clientManager.GetClient(); }
		}

		public bool IsKnown(string key)
		{
			var typedClient = Client.As<CrawlUrl>();

			return typedClient.HashContainsEntry(typedClient.GetHash<string>(WorkingHashId), key)
			       || typedClient.HashContainsEntry(typedClient.GetHash<string>(DoneHashId), key)
			       || typedClient.HashContainsEntry(typedClient.GetHash<string>(ScheduledHashId), key);
		}

		public bool TryAdd(string key, CrawlUrl crawlUrl)
		{
			Client.As<string>().Lists[ScheduledListId].Add(key);

			var typedClient = Client.As<CrawlUrl>();
			typedClient.SetEntryInHashIfNotExists(typedClient.GetHash<string>(ScheduledHashId), key, crawlUrl);

			return true;
		}

		public void Done(string key, CrawlUrl crawlUrl)
		{
			var typedClient = Client.As<CrawlUrl>();

			if (typedClient.RemoveEntryFromHash(typedClient.GetHash<string>(WorkingHashId), crawlUrl.Hash))
			{
				typedClient.SetEntryInHashIfNotExists(typedClient.GetHash<string>(DoneHashId), crawlUrl.Hash, crawlUrl);
			}
		}

		public CrawlUrl PeekNext()
		{
			var key = Client.As<string>().Lists[ScheduledListId].GetRange(0, 0).FirstOrDefault();
			if (key == null)
				return null;

			CrawlUrl crawlUrl;
			Client.As<CrawlUrl>().GetHash<string>(ScheduledHashId).TryGetValue(key, out crawlUrl);

			return crawlUrl;
		}

		public CrawlUrl Next()
		{
			var typedClient = Client.As<CrawlUrl>();

			var key = Client.As<string>().Lists[ScheduledListId].Pop();
			if (key == null)
				return null;

			var crawlUrl = typedClient.GetValueFromHash(typedClient.GetHash<string>(ScheduledHashId), key);

			if (typedClient.RemoveEntryFromHash(typedClient.GetHash<string>(ScheduledHashId), key))
				typedClient.SetEntryInHashIfNotExists(typedClient.GetHash<string>(WorkingHashId), key, crawlUrl);

			return crawlUrl;
		}
	}
}