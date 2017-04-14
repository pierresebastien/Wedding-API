using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using SimpleStack.Orm;
using SimpleStack.Orm.PostgreSQL;

namespace Wedding.Api
{
	public class Config
	{
		public Config(IConfiguration configuration)
		{
			FileConfig = configuration;
			ConnectionFactory = new OrmConnectionFactory(new PostgreSQLDialectProvider(), ConnectionString);
			PublicUri = new Uri(PublicUriString);
			ApplicationPath = PlatformServices.Default.Application.ApplicationBasePath;
		}

		public IConfiguration FileConfig { get; }

		public string ConnectionString => FileConfig["Data:DefaultConnection:ConnectionString"];

		public OrmConnectionFactory ConnectionFactory { get; }

		public string PublicUriString => FileConfig["Api:PublicUri"];

		public Uri PublicUri { get; }

		public string WkHtmlToPdfPath => FileConfig["WkHtmlToPdf:Path"];

		public int WkHtmlToPdfTimeout => int.Parse(FileConfig["WkHtmlToPdf:Timeout"] ?? "30");

		public string SmtpDeliveryMethod => FileConfig["Smtp:Delivery"];

		public string EmailSenderAddress => FileConfig["Smtp:Sender:Email"];

		public string EmailSender => FileConfig["Smtp:Sender:DisplayName"];

		public string SmtpHost => FileConfig["Smtp:Network:Host"];

		public int SmtpPort => int.Parse(FileConfig["Smtp:Network:Port"] ?? "25");

		public string SmtpUsername=> FileConfig["Smtp:Network:User"];

		public string SmtpPassword => FileConfig["Smtp:Network:Password"];

		public string SmtpPickupDirectory => FileConfig["Smtp:Pickup:Directory"];

		public string ApplicationPath { get; }
	}
}
