using System.Linq;
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

		public bool IsKnown(string key)
		{
			return client.Exists(key) == 1;
		}

		public bool TryAdd(string key, CrawlUrl crawlUrl)
		{
			var typedClient = client.As<CrawlUrl>();
			typedClient.AddItemToList(typedClient.Lists["scheduled"], crawlUrl);

			return true;
		}

		public void Done(string key, CrawlUrl crawlUrl)
		{
			var typedClient = client.As<CrawlUrl>();

			typedClient.RemoveItemFromList(typedClient.Lists["working"], crawlUrl);
			typedClient.AddItemToList(typedClient.Lists["done"], crawlUrl);
		}

		public CrawlUrl PeekNext()
		{
			var typedClient = client.As<CrawlUrl>();
			return typedClient.GetRangeFromList(typedClient.Lists["scheduled"], 0, 0).FirstOrDefault();
		}

		public CrawlUrl Next()
		{
			var typedClient = client.As<CrawlUrl>();
			return typedClient.PopAndPushItemBetweenLists(typedClient.Lists["scheduled"], typedClient.Lists["working"]);
		}
	}
}