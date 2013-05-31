using NetCrawler.Core.Configuration;

namespace NetCrawler.Core
{
	internal class Builder : IBuilder
	{
		internal Builder()
		{
		}

		private IConfiguration configuration;

		public IBuilder WithConfiguration(IConfiguration configuration)
		{
			this.configuration = configuration;
			return this;
		}

		public ICrawler Build()
		{
			return new Crawler(configuration);
		}
	}

	public static class Instance
	{
		public static IBuilder Bulder()
		{
			return new Builder();
		}
	}

	public interface IBuilder
	{
		IBuilder WithConfiguration(IConfiguration configuration);
		ICrawler Build();
	}
}