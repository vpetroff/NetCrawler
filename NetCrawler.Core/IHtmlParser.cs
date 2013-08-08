using System.Collections.Generic;

namespace NetCrawler.Core
{
	public interface IHtmlParser
	{
		IEnumerable<string> ExtractLinks(string pageUrl, string contents);
	}
}