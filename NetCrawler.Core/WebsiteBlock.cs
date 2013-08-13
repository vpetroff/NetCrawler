using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NetCrawler.Core
{
	public class WebsiteBlock
	{
		public int UrlsToProcessCount;
		public int ProcessedUrlsCount;

		public Website Website { get; set; }
		public ITargetBlock<CrawlUrl> ProcessingBlock { get; set; }

		public TaskCompletionSource<CrawlResult> CompletionSource { get; set; }

		public CrawlResult CrawlResult { get; set; }

		public void Complete()
		{
			ProcessingBlock.Complete();

			CrawlResult.NumberOfPagesCrawled = ProcessedUrlsCount;
			CrawlResult.CrawlEnded = DateTime.Now;

			CompletionSource.SetResult(CrawlResult);
		}
	}
}