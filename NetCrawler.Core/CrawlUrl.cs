using System;

namespace NetCrawler.Core
{
	public class CrawlUrl
	{
		public string Hash { get; set; }

		private string url;
		public string Url
		{
			get { return url; }
			set
			{
				url = value;
				Uri = new Uri(url);
			}
		}

		public Uri Uri { get; private set; }

		public WebsiteDefinition WebsiteDefinition { get; set; }
	}
}