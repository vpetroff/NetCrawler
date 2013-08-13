namespace NetCrawler.Core
{
	public interface IUrlHasher
	{
		byte[] CalculateHash(string url);
		string CalculateHashAsString(string url);
	}
}