using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace NetCrawler.RavenDb.Indexes
{
	public class PagesToCrawlByUrl : AbstractIndexCreationTask<Page>
	{
		public override IndexDefinition CreateIndexDefinition()
		{
			return new IndexDefinitionBuilder<Page>
			{
				Map = pages => from page in pages select new { page.Url, page.Base64UrlHash, page.WebsiteUrl, page.CrawledAt },
				Indexes =
						{
							{x => x.Url, FieldIndexing.NotAnalyzed},
							{x => x.Base64UrlHash, FieldIndexing.NotAnalyzed},
							{x => x.WebsiteUrl, FieldIndexing.NotAnalyzed},
							{x => x.CrawledAt, FieldIndexing.NotAnalyzed},
						},
				Stores =
						{
							{x => x.Url, FieldStorage.Yes},
							{x => x.Base64UrlHash, FieldStorage.Yes},
							{x => x.WebsiteUrl, FieldStorage.Yes},
							{x => x.CrawledAt, FieldStorage.Yes},
						},
						SortOptions =
							{
								{x => x.CrawledAt, SortOptions.String}
							}
			}.ToIndexDefinition(Conventions);
		}
	}
}