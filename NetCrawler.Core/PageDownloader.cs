using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mime;
using NetCrawler.Core.Configuration;

namespace NetCrawler.Core
{
	public class PageDownloader : IPageDownloader
	{
		private readonly IConfiguration configuration;

		public PageDownloader(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public PageDownloadResponse Download(string url)
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

				downloadResponse.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			latencyTimer.Stop();
			downloadResponse.Latency = latencyTimer.ElapsedMilliseconds;

			return downloadResponse;
		}
	}
}