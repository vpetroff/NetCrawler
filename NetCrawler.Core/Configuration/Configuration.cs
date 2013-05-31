namespace NetCrawler.Core.Configuration
{
	public class Configuration : IConfiguration
	{
		public Configuration()
		{
			UserAgent = "NetCrawler v1.0 alpha | admin@netcrawler.com";
			FollowExternalLinks = false;
		}

		public string UserAgent { get; set; }
		public bool FollowExternalLinks { get; set; }
	}
}