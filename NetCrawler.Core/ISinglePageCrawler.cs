using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mime;
using NetCrawler.Core.Configuration;

namespace NetCrawler.Core
{
	public interface ISinglePageCrawler
	{
		 
	}

	public class SinglePageCrawler : ISinglePageCrawler
	{
		private readonly IConfiguration configuration;

		public SinglePageCrawler(IHtmlParser htmlParser,
			IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public PageCrawlResult Crawl(string url)
		{
			var pageUri = new Uri(url);
			var crawlResult = new PageCrawlResult();

			try
			{
				var downloadResponse = DownloadPage(url);

				if (downloadResponse.IsSuccessful)
				{
					crawlResult.Links = ExtractLinks(downloadResponse.Contents);
				}
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Exception while crawling {0}: {1}", pageToCrawl.Url, ex);

				pageToCrawl.Response = new CrawlResponse
				{
					RawResponse = rawContent
				};

				if (response != null)
				{
					SetResponse(pageToCrawl, latency, response, rawContent);
				}

				pageToCrawl.CrawlStatus = CrawlStatus.Error;
				pageToCrawl.LastCrawledAt = DateTimeOffset.UtcNow;
				pageToCrawl.ErrorMessage = ex.ToString();
			}

			crawlResult.CrawlEndedAt = DateTimeOffset.UtcNow;
			return crawlResult;
		}

		private void SetResponse(CrawlRequest pageToCrawl, long latency, HttpWebResponse response, string rawContent)
		{
/*
			pageToCrawl.Response.Latency = latency;
			pageToCrawl.Response.Code = response.StatusCode;
			pageToCrawl.Response.ContentType = response.ContentType;
			pageToCrawl.Response.ContentLength = response.ContentLength;
			pageToCrawl.Response.RawResponse = rawContent;

			pageToCrawl.LastCrawledAt = DateTimeOffset.UtcNow;
			pageToCrawl.CrawlStatus = ((int)response.StatusCode >= 400) ? CrawlStatus.Error : CrawlStatus.Crawled;
*/
		}

		private PageDownloadResponse DownloadPage(string url)
		{
			var downloadResponse = new PageDownloadResponse();
			var latencyTimer = new Stopwatch();
			latencyTimer.Start();


			var request = (HttpWebRequest)WebRequest.Create(url);
			request.AllowAutoRedirect = true;
			request.MaximumAutomaticRedirections = 7;
			request.UserAgent = configuration.UserAgent;
			request.Accept = MediaTypeNames.Text.Html;

			downloadResponse.HttpWebResponse = (HttpWebResponse)request.GetResponse();

			var responseStream = downloadResponse.HttpWebResponse.GetResponseStream();

			if (responseStream != null)
			{
				using (var sr = new StreamReader(responseStream))
				{
					downloadResponse.Contents = sr.ReadToEnd();
					sr.Close();
				}
			}

			latencyTimer.Stop();
			downloadResponse.Latency = latencyTimer.ElapsedMilliseconds;

			return downloadResponse;
		}

	}

	public interface IHtmlParser
	{
	}

	class HtmlParser : IHtmlParser
	{
	}

	internal class PageDownloadResponse
	{
		public HttpWebResponse HttpWebResponse { get; set; }
		public string Contents { get; set; }
		public long Latency { get; set; }

		public bool IsSuccessful
		{
			get { return true; }
		}
	}

	public class PageCrawlResult
	{
		public PageCrawlResult()
		{
			CrawlStartedAt = DateTimeOffset.UtcNow;
		}

		public DateTimeOffset CrawlStartedAt { get; set; }
		public DateTimeOffset CrawlEndedAt { get; set; }
	}
}