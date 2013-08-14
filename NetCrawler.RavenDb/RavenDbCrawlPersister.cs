using System;
using System.Net;
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
				var existing = session.Advanced.LuceneQuery<Page, PagesToCrawlByUrl>().Where(string.Format("Hash:\"{0}\"", pageCrawlResult.CrawlUrl.Hash)).FirstOrDefault() ?? new Page();

				existing.WebsiteUrl = pageCrawlResult.CrawlUrl.WebsiteDefinition.Website.RootUrl;
				existing.Url = pageCrawlResult.CrawlUrl.Url;
				existing.Hash = pageCrawlResult.CrawlUrl.Hash;
				existing.Contents = pageCrawlResult.Contents;
				existing.StatusCode = pageCrawlResult.StatusCode;
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
				existing.LastCrawlStartedAt = website.LastCrawlStartedAt;
				existing.LastCrawlEndedAt = website.LastCrawlEndedAt;
				existing.PagesCrawled = website.PagesCrawled;

				existing.MaxConcurrentConnections = website.MaxConcurrentConnections;
				existing.IntervalBetweenVisits = website.IntervalBetweenVisits;
				existing.FollowExternalLinks = website.FollowExternalLinks;

				session.Store(existing);
				session.SaveChanges();
			}
		}
	}

	public class Page
	{
		public string WebsiteUrl { get; set; }
		public string Url { get; set; }
		public string Hash { get; set; }
		public string Contents { get; set; }
		public DateTimeOffset CrawledAt { get; set; }
		public HttpStatusCode StatusCode { get; set; }
	}
}