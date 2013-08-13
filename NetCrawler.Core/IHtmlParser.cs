using System;
using System.Collections.Generic;

namespace NetCrawler.Core
{
	public interface IHtmlParser
	{
		IEnumerable<string> ExtractLinks(Uri pageUri, string contents);
	}
}