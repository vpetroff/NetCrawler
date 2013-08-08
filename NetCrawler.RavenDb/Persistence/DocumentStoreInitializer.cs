using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;

namespace NetCrawler.RavenDb.Persistence
{
    public class DocumentStoreInitializer : IDocumentStoreInitializer
    {
		private readonly string url;
	    private readonly string databaseName;
	    private IDocumentStore documentStore;

		public DocumentStoreInitializer(string url, string databaseName)
		{
			this.url = url;
			this.databaseName = databaseName;
		}

	    public IDocumentStore DocumentStore
        {
            get
            {
                if (documentStore == null)
                    Initialize();

                return documentStore;
            }
        }

        private void Initialize()
        {
	        try
	        {
				documentStore = new DocumentStore
				{
					Url = url,
					DefaultDatabase = databaseName
				};

				documentStore.Conventions.CustomizeJsonSerializer = serializer => serializer.TypeNameHandling = TypeNameHandling.Auto;
				documentStore.Conventions.DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites;

				documentStore.Initialize();

				SetupIndices();
			}
	        catch (Exception ex)
	        {
		        throw;
	        }
        }

        private void SetupIndices()
        {
			IndexCreation.CreateIndexes(typeof(DocumentStoreInitializer).Assembly, documentStore);
		}
    }
}