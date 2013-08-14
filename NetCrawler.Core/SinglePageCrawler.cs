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

		public PageCrawlResult Crawl(Uri url)
		{
			var crawlResult = new PageCrawlResult();

			var downloadResponse = pageDownloader.Download(url);
			crawlResult.StatusCode = downloadResponse.StatusCode;

			if (downloadResponse.IsSuccessful)
			{
				crawlResult.Contents = downloadResponse.Contents;
				crawlResult.Links = htmlParser.ExtractLinks(url, downloadResponse.Contents);
			}

			crawlResult.CrawlEndedAt = DateTimeOffset.Now;

			return crawlResult;
		}
	}
}