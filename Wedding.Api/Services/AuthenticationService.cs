using System;
using Nancy;
using Nancy.Authentication.Forms;
using SimpleStack.Orm;
using Wedding.Api.Business.Databases;
using Wedding.Api.Exceptions;
using Wedding.Api.Tools;
using MimeKit;

namespace Wedding.Api.Services
{
	public sealed class AuthenticationService : NancyModule
	{
		private readonly Config _config;
		private readonly EmailSender _emailSender;

		public AuthenticationService(Config config, EmailSender emailSender) : base("auth")
		{
			_config = config;
			_emailSender = emailSender;
			Get("/{ApiKey}", args => LogIn(args.ApiKey));
			Post("/logout", args => LogOut());
			Post("/recover/{Email}", args => RecoverToken(args.Email));
		}

		private Response LogIn(string apiKey)
		{
			Guid key;
			if (!Guid.TryParse(apiKey, out key))
			{
				throw new BadRequestException("Invalid api key");
			}

			using (OrmConnection connection = _config.ConnectionFactory.OpenConnection())
			{
				DbUser user = connection.FirstOrDefault<DbUser>(x => x.ApiKey == key);
				if (user == null)
				{
					throw new BadRequestException("Invalid api key");
				}
				string redirectUrl = $"{_config.PublicUri.Scheme}://{_config.PublicUri.Host}/invitation";
				return this.LoginAndRedirect(user.ApiKey, DateTime.Now.AddYears(1), redirectUrl);
			}
		}

		private Response LogOut()
		{
			return this.LogoutWithoutRedirect();
		}

		private Response RecoverToken(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				throw new BadRequestException("Invalid email address");
			}
			using (OrmConnection connection = _config.ConnectionFactory.OpenConnection())
			{
				DbUser user = connection.FirstOrDefault<DbUser>(x => x.Mail == email);
				if (user != null)
				{
					string displayName = string.IsNullOrWhiteSpace(user.NickName)
						? user.FirstName
						: user.NickName;
					_emailSender.SendEmail(email, displayName, "Lien de connexion",
						$"{_config.PublicUri}/auth/{user.ApiKey}", new MimePart[0]);
				}
				return new Response().WithStatusCode(HttpStatusCode.OK);
			}
		}
	}
}
