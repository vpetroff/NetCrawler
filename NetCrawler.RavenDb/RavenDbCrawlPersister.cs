using System;
using NetCrawler.Core;
using NetCrawler.RavenDb.Indexes;
using Raven.Client;
using Raven.Client.Linq;
using System.Linq;

namespace NetCrawler.RavenDb
{
	public class RavenDbCrawlPersister : ICrawlPersister
	{
		private readonly IDocumentStore documentStore;

		public RavenDbCrawlPersister(IDocumentStore documentStore)
		{
			this.documentStore = documentStore;
		}

		public void Save(PageCrawlResult pageCrawlResult)
		{
			using (var session = documentStore.OpenSession())
			{
				var existing = session.Query<Page, PagesToCrawlByUrl>().Where(x => x.Base64UrlHash == pageCrawlResult.CrawlUrl.Base64Hash).FirstOrDefault() ?? new Page();

				existing.WebsiteUrl = pageCrawlResult.CrawlUrl.Website.Website.RootUrl;
				existing.Url = pageCrawlResult.CrawlUrl.Url;
				existing.Base64UrlHash = pageCrawlResult.CrawlUrl.Base64Hash;
				existing.Contents = pageCrawlResult.Contents;
				existing.CrawledAt = pageCrawlResult.CrawlEndedAt;

				session.Store(existing);
				session.SaveChanges();
			}
		}

		public void Save(Website website)
		{
			using (var session = documentStore.OpenSession())
			{
				var existing = session.Query<Website>().Where(w => w.RootUrl == website.RootUrl).FirstOrDefault() ?? website;
				
				existing.LastVisit = website.LastVisit;

				session.Store(existing);
				session.SaveChanges();
			}
		}
	}

	public class Page
	{
		public string WebsiteUrl { get; set; }
		public string Url { get; set; }
		public string Base64UrlHash { get; set; }
		public string Contents { get; set; }
		public DateTimeOffset CrawledAt { get; set; }

	}
}