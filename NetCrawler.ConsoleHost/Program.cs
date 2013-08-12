﻿using System;
using NetCrawler.Core;
using NetCrawler.Core.Configuration;
using NetCrawler.RavenDb;
using NetCrawler.RavenDb.Persistence;
using log4net;
using log4net.Config;

namespace NetCrawler.ConsoleHost
{
	class Program
	{
		static void Main(string[] args)
		{
			XmlConfigurator.Configure();

			var log = LogManager.GetLogger(typeof (Program));

			var url = "http://www.karenmillen.com/";
			if (args.Length > 1)
				url = args[1];

			var configuration = new Configuration();
			var pageDownloader = new PageDownloader(configuration);
			var htmlParser = new HtmlParser();
			var pageCrawler = new SinglePageCrawler(htmlParser, pageDownloader);

			var urlHasher = new UrlHasher();

//			var documentStore = new DocumentStoreInitializer("http://localhost:8080", "NetCrawler").DocumentStore;
			var documentStore = new DocumentStoreInitializer("http://SLB-4B6WZN1:8080", "NetCrawler").DocumentStore;
			var persister = new RavenDbCrawlPersister(documentStore);

			var websiteCrawler = new WebsiteCrawler(new LocalCrawlScheduler(urlHasher, configuration, pageCrawler), persister);

			var task = websiteCrawler.RunAsync(new Website
			{
				RootUrl = url,
				MaxConcurrentConnections = 100
			});

			var result = task.Result;

			log.InfoFormat("Crawl completed: {0} urls crawled in {1}", result.NumberOfPagesCrawled, (result.CrawlEnded - result.CrawlStarted).ToString());
		}
	}
}