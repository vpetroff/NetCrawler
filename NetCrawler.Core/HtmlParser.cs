using System;
using System.Collections.Generic;
using System.Diagnostics;
using CsQuery;

namespace NetCrawler.Core
{
	public class HtmlParser : IHtmlParser
	{
		public IEnumerable<string> ExtractLinks(string pageUrl, string contents)
		{
			var dom = CQ.Create(contents);

			var baseUri = new Uri(pageUrl, UriKind.Absolute);

			var pageLinks = new List<string>();
			dom["a[href]"].Each(a =>
				{
					string href;
					if (a.TryGetAttribute("href", out href) && !string.IsNullOrWhiteSpace(href))
					{
						try
						{
							var uri = new Uri(baseUri, href);
							if (!pageLinks.Contains(uri.AbsoluteUri))
								pageLinks.Add(uri.AbsoluteUri);
						}
						catch (Exception ex)
						{
							Debug.WriteLine(ex.Message);
						}
					}
				});

			return pageLinks;
		}
	}
}