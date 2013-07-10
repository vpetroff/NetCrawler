using NUnit.Framework;
using NetCrawler.Core;
using NetCrawler.Core.Configuration;

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

			var websiteCrawler = new WebsiteCrawler(new LocalCrawlScheduler(configuration, pageDownloader, htmlParser));

			var task = websiteCrawler.RunAsync(new Website
				{
					RootUrl = "http://vladpetroff.com"
				});

			task.Wait();

			var result = task.Result;
		}
	}
}