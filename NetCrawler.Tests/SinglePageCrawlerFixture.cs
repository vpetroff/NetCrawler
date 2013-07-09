using NUnit.Framework;
using NetCrawler.Core;
using NetCrawler.Core.Configuration;

namespace NetCrawler.Tests
{
	[TestFixture, Category("Integration")]
	public class SinglePageCrawlerFixture
	{
		[Test]
		public void Should_extract_links_from_page()
		{
			var configuration = new Configuration();
			var pageDownloader = new PageDownloader(configuration);
			var htmlParser = new HtmlParser();

			var crawler = new SinglePageCrawler(htmlParser, pageDownloader);

			var result = crawler.Crawl("http://vladpetroff.com");
		}
	}
}