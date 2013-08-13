using System;

namespace NetCrawler.Core
{
	public interface IPageDownloader
	{
		PageDownloadResponse Download(Uri url);
	}
}