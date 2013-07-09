using System;

namespace NetCrawler.Core
{
	public class SinglePageCrawler : ISinglePageCrawler
	{
		private readonly IHtmlParser htmlParser;
		private readonly IPageDownloader pageDownloader;

		public SinglePageCrawler(IHtmlParser htmlParser, IPageDownloader pageDownloader)
		{
			this.htmlParser = htmlParser;
			this.pageDownloader = pageDownloader;
		}

		public PageCrawlResult Crawl(string url)
		{
			var crawlResult = new PageCrawlResult();

			var downloadResponse = pageDownloader.Download(url);

			if (downloadResponse.IsSuccessful)
			{
				crawlResult.Contents = downloadResponse.Contents;
				crawlResult.Links = htmlParser.ExtractLinks(downloadResponse.Contents);
			}

			crawlResult.CrawlEndedAt = DateTimeOffset.UtcNow;
			return crawlResult;
		}
	}
}