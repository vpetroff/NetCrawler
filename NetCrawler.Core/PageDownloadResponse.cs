using System.Net;

namespace NetCrawler.Core
{
	public class PageDownloadResponse
	{
		public string Contents { get; set; }
		public long Latency { get; set; }

		public bool IsSuccessful { get; set; }

		public HttpStatusCode StatusCode { get; set; }
	}
}