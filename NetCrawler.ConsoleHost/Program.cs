using System;
using System.Linq;
using NetCrawler.Core;
using NetCrawler.Core.Configuration;
using NetCrawler.RavenDb;
using NetCrawler.RavenDb.Persistence;
using NetCrawler.Redis;
using log4net;
using log4net.Config;

namespace NetCrawler.ConsoleHost
{
	class Program
	{
		static void Main()
		{
			XmlConfigurator.Configure();

			var log = LogManager.GetLogger(typeof (Program));

			var configuration = new Configuration();
			var pageDownloader = new PageDownloader(configuration);
			var htmlParser = new HtmlParser();
			var pageCrawler = new SinglePageCrawler(htmlParser, pageDownloader);

			var urlHasher = new UrlHasher();

			var documentStore = new DocumentStoreInitializer("http://localhost:8080", "NetCrawler2").DocumentStore;
//			var documentStore = new DocumentStoreInitializer("http://SLB-4B6WZN1:8080", "NetCrawler2").DocumentStore;
			var persister = new RavenDbCrawlPersister(documentStore);

//			var crawlUrlRepository = new InMemoryCrawlUrlRepository();
			var crawlUrlRepository = new RedisCrawlUrlRepository();

			var websiteCrawler = new WebsiteCrawler(new CrawlScheduler(urlHasher, configuration, pageCrawler, crawlUrlRepository), persister);

			var task = websiteCrawler.RunAsync(new [] {
				new Website
				{
					RootUrl = "http://www.karenmillen.com/",
					MaxConcurrentConnections = 15
				},
				new Website
				{
					RootUrl = "http://uk.tommy.com/",
					MaxConcurrentConnections = 15
				},
				new Website
				{
					RootUrl = "http://www.houseoffraser.co.uk/",
					MaxConcurrentConnections = 15
				},
			});

			var result = task.Result;

			log.InfoFormat("Crawl completed: {0} urls crawled in {1}", result.Sum(x => x.NumberOfPagesCrawled), (result.Max(x => x.CrawlEnded) - result.Min(x => x.CrawlStarted)).ToString());
		}
	}
}
