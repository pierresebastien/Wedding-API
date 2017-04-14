using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Nancy.Owin;
using Wedding.Api.Tools;

namespace Wedding.Api
{
	public class Startup
	{
		private readonly Config _config;

		public Startup(IHostingEnvironment env)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("config.json", false)
				.AddJsonFile($"config.{env.EnvironmentName}.json", true)
				.Build();
			_config = new Config(configuration);

			DatabaseManager.SetUpDatabase(_config.ConnectionFactory);
			HtmlToPdfConverter.Setup(_config.WkHtmlToPdfPath, _config.WkHtmlToPdfTimeout);
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseOwin(x => x.UseNancy(new NancyOptions {Bootstrapper = new Bootstrapper(_config) }));
		}
	}
}
