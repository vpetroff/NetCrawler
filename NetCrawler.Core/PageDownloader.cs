using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mime;
using NetCrawler.Core.Configuration;
using log4net;

namespace NetCrawler.Core
{
	public class PageDownloader : IPageDownloader
	{
		private readonly IConfiguration configuration;
		private static readonly ILog Log = LogManager.GetLogger(typeof (PageDownloader));

		public PageDownloader(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public PageDownloadResponse Download(Uri url)
		{
			var downloadResponse = new PageDownloadResponse();
			var latencyTimer = new Stopwatch();
			latencyTimer.Start();

			try
			{
				var request = (HttpWebRequest)WebRequest.Create(url);
				request.AllowAutoRedirect = true;
				request.MaximumAutomaticRedirections = 7;
				request.UserAgent = configuration.UserAgent;
				request.Accept = MediaTypeNames.Text.Html;

				var httpWebResponse = (HttpWebResponse)request.GetResponse();

				var responseStream = httpWebResponse.GetResponseStream();

				if (responseStream != null)
				{
					using (var sr = new StreamReader(responseStream))
					{
						downloadResponse.Contents = sr.ReadToEnd();
						sr.Close();
					}
				}

				downloadResponse.StatusCode = httpWebResponse.StatusCode;
				downloadResponse.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}

			latencyTimer.Stop();
			downloadResponse.Latency = latencyTimer.ElapsedMilliseconds;

			return downloadResponse;
		}
	}
}