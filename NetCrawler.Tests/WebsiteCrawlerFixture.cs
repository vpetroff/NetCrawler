using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NetCrawler.Core;
using NetCrawler.Core.Configuration;
using FluentAssertions;
using NetCrawler.RavenDb;
using NetCrawler.RavenDb.Persistence;

namespace NetCrawler.Tests
{
	[TestFixture]
	public class WebsiteCrawlerFixture
	{
		[Test]
		public void Should_crawl_website()
		{
			var configuration = new Configuration();
			var pageDownloader = new PageDownloader(configuration);
			var htmlParser = new HtmlParser();
			var pageCrawler = new SinglePageCrawler(htmlParser, pageDownloader);

			var documentStore = new DocumentStoreInitializer("http://localhost:8080", "NetCrawler").DocumentStore;
			var persister = new RavenDbCrawlPersister(documentStore);

			var urlHasher = new UrlHasher();
			var websiteCrawler = new WebsiteCrawler(new LocalCrawlScheduler(urlHasher, configuration, pageCrawler), persister);

			var task = websiteCrawler.RunAsync(new Website
				{
					RootUrl = "http://www.karenmillen.com/",
					MaxConcurrentConnections = 100
				});

//			task.Wait(new TimeSpan(0, 10, 0));
//			task.Wait(new TimeSpan(0, 2, 0));
			task.Wait();

			task.Status.ShouldBeEquivalentTo(TaskStatus.RanToCompletion);

			var result = task.Result;

			Console.WriteLine("Crawl completed: {0} urls crawled in {1}", result.NumberOfPagesCrawled, (result.CrawlEnded - result.CrawlStarted).ToString());
		}
	}
}