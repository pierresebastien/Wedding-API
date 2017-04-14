using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Wedding.Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("hosting.json", true)
				.Build();

			IWebHost host = new WebHostBuilder()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseKestrel()
				.UseConfiguration(config)
				.UseStartup<Startup>()
				.Build();

			host.Run();
		}
	}
}
