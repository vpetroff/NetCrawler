using System.Net;

namespace NetCrawler.Core
{
	public class PageDownloadResponse
	{
		public HttpWebResponse HttpWebResponse { get; set; }
		public string Contents { get; set; }
		public long Latency { get; set; }

		public bool IsSuccessful { get; set; }
	}
}