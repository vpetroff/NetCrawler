using NetCrawler.Core;
using ServiceStack.Redis;

namespace NetCrawler.Redis
{
	public class RedisCrawlUrlRepository : ICrawlUrlRepository
	{
		private readonly RedisClient client;

		public RedisCrawlUrlRepository()
		{
			client = new RedisClient("localhost");
			client.FlushAll();
		}

		public bool Contains(string key)
		{
			return client.Exists(key) == 1;
		}

		public bool TryAdd(string key, CrawlUrl crawlUrl)
		{
			return client.As<CrawlUrl>().SetEntryIfNotExists(key, crawlUrl);
		}
	}
}