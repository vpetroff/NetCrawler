namespace NetCrawler.Core.Configuration
{
	public interface IConfiguration
	{
		string UserAgent { get; set; }
		bool FollowExternalLinks { get; set; }
	}
}