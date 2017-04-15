using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SimpleStack.Orm;
using SimpleStack.Orm.PostgreSQL;
using Wedding.Api.Business.Databases;
using Wedding.Api.Tools;
using Dapper;

namespace Wedding.Email
{
	public class Program
	{
		private const string MailSubject = "Judith & Sébastien se marient";

		private static CommandOption _command;
		private static CommandOption _type;
		private static CommandOption _recipient;

		private static OrmConnectionFactory _connectionFactory;

		private static EmailSender _emailSender;

		private static string _smtpHost;
		private static int _smtpPort = 25;
		private static string _smtpUser;
		private static string _smtpPassword;

		private static string _pickupDirectoryPath;

		private static string _senderDisplayName;
		private static string _senderEmail;

		private static string _apiPublicUri;

		public static void Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("config.json", false)
				.Build();

			_connectionFactory = new OrmConnectionFactory(new PostgreSQLDialectProvider(),
				configuration["Data:DefaultConnection:ConnectionString"]);

			_smtpHost = configuration["Smtp:Server:Host"];
			string port = configuration["Smtp:Server:Port"];
			if (!string.IsNullOrWhiteSpace(port))
			{
				if (!int.TryParse(port, out _smtpPort))
				{
					Console.WriteLine("Unable to read port in config file");
				}
			}
			_smtpUser = configuration["Smtp:Server:User"];
			_smtpPassword = configuration["Smtp:Server:Password"];

			_pickupDirectoryPath = configuration["Smtp:Pickup:Directory"];

			_senderDisplayName = configuration["Smtp:Sender:DisplayName"];
			_senderEmail = configuration["Smtp:Sender:Email"];

			_apiPublicUri = configuration["Api:PublicUri"];

			CommandLineApplication commandLineApplication = new CommandLineApplication(false);
			_command = commandLineApplication.Option("-c |--command <command>", "The command to execute (test or invitations)",
				CommandOptionType.SingleValue);
			_type = commandLineApplication.Option("-t |--type <type>", "The type of email eneration (pickup or server)",
				CommandOptionType.SingleValue);
			_recipient = commandLineApplication.Option("-r |--recipient <recipient>", "The recipient to use for the test email",
				CommandOptionType.SingleValue);
			commandLineApplication.OnExecute(() =>
			{
				ProcessCommand();
				return 0;
			});
			commandLineApplication.Execute(args);
		}

		private static void ProcessCommand()
		{
			if (_type.HasValue())
			{
				switch (_type.Value().ToLowerInvariant())
				{
					case "pickup":
						_emailSender = new EmailSender(_senderDisplayName, _senderEmail,_pickupDirectoryPath);
						break;
					case "server":
						_emailSender = new EmailSender(_senderDisplayName, _senderEmail, _smtpHost, _smtpPort, _smtpUser, _smtpPassword);
						break;
				}
			}
			else
			{
				Console.WriteLine("You must specify a type to send email with -t or --type");
			}

			if (_command.HasValue())
			{
				switch (_command.Value().ToLowerInvariant())
				{
					case "test":
						ProcessTestEmail();
						break;
					case "invitations":
						ProcessInvitationEmails();
						break;
					case "reminder":
						ProcessReminderEmails();
						break;
				}
			}
			else
			{
				Console.WriteLine("No command specified");
			}
		}

		private static void ProcessTestEmail()
		{
			if (_recipient.HasValue())
			{
				DbUser user;
				using (OrmConnection connection = _connectionFactory.OpenConnection())
				{
					user = connection.First<DbUser>(x => x.Mail == _recipient.Value());
				}
				string displayName = string.IsNullOrWhiteSpace(user.NickName) ? user.FirstName : user.NickName;
				_emailSender.SendEmail(user.Mail, displayName, MailSubject, GetInvitationMailBody(user), new MimePart[0]);
			}
			else
			{
				Console.WriteLine("You must specify a recipient with -r or --recipient to generate a test email");
			}
		}

		private static void ProcessInvitationEmails()
		{
			IList<DbUser> users;
			using (OrmConnection connection = _connectionFactory.OpenConnection())
			{
				users = connection.Select<DbUser>().ToList();
			}

			foreach (var user in users.Where(x => !string.IsNullOrWhiteSpace(x.Mail)))
			{
				string displayName = string.IsNullOrWhiteSpace(user.NickName) ? user.FirstName : user.NickName;
				_emailSender.SendEmail(user.Mail, displayName, MailSubject, GetInvitationMailBody(user), new MimePart[0]);
			}
		}

		private static string GetInvitationMailBody(DbUser user)
		{
			string link = $"{_apiPublicUri}/auth/{user?.ApiKey}";
			return string.Format(@"
				  <div style=""font-family: Edwardian Script ITC, Trebuchet MS; text-align: center;font-size: 40px;font-weight:bold;"">
				  <p>Tu trouveras plus d'informations et ton invitation en suivant ce lien :<br/>
				  <a style=""font-family: Trebuchet MS;font-size: 20px;"" href=""{0}"">{0}</a>
				  </p>
				  <p>Sur le site tu pourras aussi nous confirmer ta présence ainsi que celle de ta/ton conjoint(e) et celle de tes enfants.</p>
				  <p>En attendant de vous y voir,</p>
				  <p>Gros bisous</p>
				  </div>", link);
		}

		private static void ProcessReminderEmails()
		{
			IList<DbUser> users = new List<DbUser>();
			using (OrmConnection connection = _connectionFactory.OpenConnection())
			{
				IList<DbUser> allUsers = connection.Select<DbUser>().ToList();
				foreach (var user in allUsers)
				{
					if (!string.IsNullOrWhiteSpace(user.Mail))
					{
						bool mustSendMailForUser = false;
						if (!user.IsRegistrationCompleted)
						{
							if (CheckNeedToSendReminder(connection, user.Id))
							{
								mustSendMailForUser = true;
								users.Add(user);
							}
						}
						
						if(!mustSendMailForUser)
						{
							JoinSqlBuilder<FamilyUserView, DbUserLink> query = FamilyUserView.GetViewBuilder(connection.DialectProvider);
							query.Where<DbUserLink>(x => x.UserId == user.Id);
							IList<FamilyUserView> family = connection.Query<FamilyUserView>(query.ToSql(), query.Parameters).ToList();

							foreach (var familyUser in family.Where(x => !x.IsRegistrationCompleted && string.IsNullOrWhiteSpace(x.Mail)).ToList())
							{
								if (CheckNeedToSendReminder(connection, familyUser.Id))
								{
									users.Add(user);
									break;
								}
							}
						}
					}
				}
			}

			foreach (var user in users)
			{
				string displayName = string.IsNullOrWhiteSpace(user.NickName) ? user.FirstName : user.NickName;
				_emailSender.SendEmail(user.Mail, displayName, MailSubject, GetReminderMailBody(user), new MimePart[0]);
			}
		}

		private static bool CheckNeedToSendReminder(OrmConnection connection, int userId)
		{
			JoinSqlBuilder<InvitationView, DbInvitation> query = InvitationView.GetViewBuilder(connection.DialectProvider);
			query.Where<DbInvitation>(x => x.UserId == userId);
			return connection.Query<InvitationView>(query.ToSql(), query.Parameters).Any(x => x.EventId == 4);
		}

		private static string GetReminderMailBody(DbUser user)
		{
			string link = $"{_apiPublicUri}/auth/{user?.ApiKey}";
			return string.Format(@"
				  <div style=""font-family: Edwardian Script ITC, Trebuchet MS; text-align: center;font-size: 40px;font-weight:bold;"">
				  <p>Coucou chers retardataires,</p>
				  <p>Nous attendons encore vos réponses à notre invitation de mariage pour pouvoir terminer les préparatifs de notre côté.</p>
				  <p>Pour rappel, voici le lien vers l'invitation :<br/>
				  <a style=""font-family: Trebuchet MS;font-size: 20px;"" href=""{0}"">{0}</a>
				  </p>
				  <p>Un grand merci</p>
				  <p>PS: Le site fonctionne de façon optimale avec Firefox ou Chrome</p>
				  </div>", link);
		}
	}
}
