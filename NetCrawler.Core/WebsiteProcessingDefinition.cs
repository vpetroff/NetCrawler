using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NetCrawler.Core
{
	public class WebsiteProcessingDefinition
	{
		public WebsiteDefinition WebsiteDefinition { get; private set; }

		public WebsiteProcessingDefinition(WebsiteDefinition websiteDefinition)
		{
			WebsiteDefinition = websiteDefinition;
		}

		public TransformBlock<CrawlUrl, PageCrawlResult> ProcessingBlock { get; set; }
		public TaskCompletionSource<CrawlResult> CompletionSource { get; set; }



		public void Complete()
		{
			ProcessingBlock.Complete();

			WebsiteDefinition.CrawlResult.NumberOfPagesCrawled = WebsiteDefinition.ProcessedUrlsCount;
			WebsiteDefinition.CrawlResult.CrawlEnded = DateTime.Now;

			CompletionSource.SetResult(WebsiteDefinition.CrawlResult);
		}

		public int Post(CrawlUrl crawlUrl)
		{
			ProcessingBlock.Post(crawlUrl);
			return ProcessingBlock.InputCount;
		}
	}
}