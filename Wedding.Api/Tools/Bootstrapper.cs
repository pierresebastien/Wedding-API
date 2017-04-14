using System;
using System.Security;
using System.Text;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Cryptography;
using Nancy.Diagnostics;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Wedding.Api.Business.Responses;
using Wedding.Api.Exceptions;

namespace Wedding.Api.Tools
{
	public class Bootstrapper : DefaultNancyBootstrapper
	{
		private readonly Config _config;
		private readonly CryptographyConfiguration _cryptographyConfiguration;

		public Bootstrapper(Config config)
		{
			_config = config;
			_cryptographyConfiguration = new CryptographyConfiguration(
				new AesEncryptionProvider(new PassphraseKeyGenerator(_config.FileConfig["Api:Security:Encryption:Passphrase"],
					Encoding.UTF8.GetBytes(_config.FileConfig["Api:Security:Encryption:Salt"]))),
				new DefaultHmacProvider(new PassphraseKeyGenerator(_config.FileConfig["Api:Security:Hmac:Passphrase"],
					Encoding.UTF8.GetBytes(_config.FileConfig["Api:Security:Hmac:Salt"]))));
		}

		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			container.Register(_config);
			container.Register<UserMapper>().AsSingleton();
			container.Register<JsonSerializer, CamelCaseJsonSerializer>();
			container.Register<InvitationGenerator>();
			container.Register<EmailSender>();

			SSLProxy.RewriteSchemeUsingForwardedHeaders(pipelines);

			// NOTE: for api => better to use stateless auth
			FormsAuthentication.Enable(pipelines, new FormsAuthenticationConfiguration
			{
				CryptographyConfiguration = _cryptographyConfiguration,
				UserMapper = container.Resolve<IUserMapper>(),
				DisableRedirect = true,
				Domain = _config.PublicUri.Host
			});

			pipelines.AfterRequest += ctx =>
			{
				ctx.Response.Headers.Add("Access-Control-Allow-Origin", _config.PublicUri.Host);
				ctx.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
				ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT");
				ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, Accept, Content-Type");
			};

			pipelines.OnError.AddItemToEndOfPipeline((ctx, e) => HandleException(ctx, e));
		}

		protected override CryptographyConfiguration CryptographyConfiguration => _cryptographyConfiguration;

		public override void Configure(INancyEnvironment environment)
		{
			environment.Diagnostics(true, "password", "/_Nancy", null, 30, _cryptographyConfiguration);
			environment.Tracing(true, true);
		}

		protected virtual dynamic HandleException(NancyContext context, Exception exception)
		{
			if (exception is UnauthorizedAccessException)
			{
				return new Response { StatusCode = HttpStatusCode.Unauthorized };
			}
			if (exception is SecurityException)
			{
				return new Response { StatusCode = HttpStatusCode.Forbidden };
			}

			HttpStatusCode status = HttpStatusCode.InternalServerError;
			if (exception is NotFoundException)
			{
				status = HttpStatusCode.NotFound;
			}
			else if (exception is BadRequestException || exception is WeddingException)
			{
				status = HttpStatusCode.BadRequest;
			}
			return new Negotiator(context).WithModel(new ErrorResponse(exception)).WithStatusCode(status);
		}
	}
}