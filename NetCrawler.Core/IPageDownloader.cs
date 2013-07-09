namespace NetCrawler.Core
{
	public interface IPageDownloader
	{
		PageDownloadResponse Download(string url);
	}
}