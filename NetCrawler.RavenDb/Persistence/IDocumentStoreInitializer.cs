using Raven.Client;

namespace NetCrawler.RavenDb.Persistence
{
    public interface IDocumentStoreInitializer
    {
        IDocumentStore DocumentStore { get; }
    }
}