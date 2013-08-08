using System;
using NetCrawler.Core;
using Raven.Client;

namespace NetCrawler.RavenDb
{
	public class RavenDbPagePersister : IPagePersister
	{
		private readonly IDocumentStore documentStore;

		public RavenDbPagePersister(IDocumentStore documentStore)
		{
			this.documentStore = documentStore;
		}

		public void Save(PageCrawlResult pageCrawlResult)
		{
			using (var session = documentStore.OpenSession())
			{
				session.Store(new Page
					{
						WebsiteUrl = pageCrawlResult.CrawlUrl.Website.Website.RootUrl,
						Url = pageCrawlResult.CrawlUrl.Url,
						Contents = pageCrawlResult.Contents,
						CrawledAt = pageCrawlResult.CrawlEndedAt,
					});
				session.SaveChanges();
			}
		}
	}

	public class Page
	{
		public string WebsiteUrl { get; set; }
		public string Url { get; set; }
		public string Contents { get; set; }
		public DateTimeOffset CrawledAt { get; set; }
	}
}