using System.ServiceProcess;

namespace NetCrawler.ServiceHost
{
	public partial class NetCrawlerService : ServiceBase
	{
		public NetCrawlerService()
		{
			InitializeComponent();


		}

		protected override void OnStart(string[] args)
		{
		}

		protected override void OnStop()
		{
		}
	}
}
