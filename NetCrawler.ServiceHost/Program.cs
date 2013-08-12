using System.ServiceProcess;

namespace NetCrawler.ServiceHost
{
	static class Program
	{
		static void Main()
		{
			var servicesToRun = new ServiceBase[] 
				{ 
					new NetCrawlerService() 
				};

			ServiceBase.Run(servicesToRun);
		}
	}
}
