using System;
using System.Collections.Generic;
using CsQuery;
using log4net;

namespace NetCrawler.Core
{
	public class HtmlParser : IHtmlParser
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (HtmlParser));

		public IEnumerable<string> ExtractLinks(Uri pageUri, string contents)
		{
			var dom = CQ.Create(contents);

			var pageLinks = new List<string>();
			dom["a[href]"].Each(a =>
				{
					string href;
					if (!a.TryGetAttribute("href", out href) || string.IsNullOrWhiteSpace(href)) 
						return;

					try
					{
						var uri = new Uri(pageUri, href);
						if (!pageLinks.Contains(uri.AbsoluteUri))
							pageLinks.Add(uri.AbsoluteUri);
					}
					catch (Exception ex)
					{
						Log.Error(ex);
					}
				});

			return pageLinks;
		}
	}
}