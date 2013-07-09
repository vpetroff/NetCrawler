using System.Collections.Generic;
using CsQuery;

namespace NetCrawler.Core
{
	public class HtmlParser : IHtmlParser
	{
		public IEnumerable<string> ExtractLinks(string contents)
		{
			var dom = CQ.Create(contents);

			var pageLinks = new List<string>();
			dom["a[href]"].Each(a =>
				{
					string href;
					if (a.TryGetAttribute("href", out href) && !string.IsNullOrWhiteSpace(href))
					{
						if (!pageLinks.Contains(href))
							pageLinks.Add(href);
					}
				});

			return pageLinks;
		}
	}
}