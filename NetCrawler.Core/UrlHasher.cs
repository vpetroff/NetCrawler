using System;
using System.Security.Cryptography;
using System.Text;

namespace NetCrawler.Core
{
	public class UrlHasher : IUrlHasher
	{
		private readonly SHA256 Sha256Instance;

		public UrlHasher()
		{
			Sha256Instance = SHA256Managed.Create();
		}

		public byte[] CalculateHash(string url)
		{
			return Sha256Instance.ComputeHash(Encoding.UTF8.GetBytes(url));
		}

		public string CalculateHashAsString(string url)
		{
			return Convert.ToBase64String(CalculateHash(url)).Replace('+', '-').Replace('/', '_');
		}
	}
}