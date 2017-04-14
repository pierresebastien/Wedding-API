using System;
using System.Collections.Generic;
using System.IO;
using MailKit.Net.Smtp;
using MimeKit;

namespace Wedding.Api.Tools
{
	public class EmailSender
	{
		protected Action<MimeMessage> SendMail;
		protected string SenderDisplayName;
		protected string SenderEmailAddress;

		protected EmailSender(string senderDisplayName, string senderEmailAddress)
		{
			SenderDisplayName = senderDisplayName;
			SenderEmailAddress = senderEmailAddress;
		}

		public EmailSender(string senderDisplayName, string senderEmailAddress, string pickupDirectory)
			: this(senderDisplayName, senderEmailAddress)
		{
			ConfigurePickupDirectory(pickupDirectory);
		}

		public EmailSender(string senderDisplayName, string senderEmailAddress, string host, int port, string username,
			string password)
			: this(senderDisplayName, senderEmailAddress)
		{
			ConfigureNetwork(host, port, username, password);
		}

		public EmailSender(Config config) : this(config.EmailSender, config.EmailSenderAddress)
		{
			switch (config.SmtpDeliveryMethod)
			{
				case "Network":
					ConfigureNetwork(config.SmtpHost, config.SmtpPort, config.SmtpUsername, config.SmtpPassword);
					break;
				case "Pickup":
					ConfigurePickupDirectory(config.SmtpPickupDirectory);
					break;
				default:
					SendMail = x => { throw new ArgumentOutOfRangeException(nameof(config.SmtpDeliveryMethod)); };
					break;
			}
		}

		protected void ConfigurePickupDirectory(string pickupDirectory)
		{
			if (string.IsNullOrWhiteSpace(pickupDirectory))
			{
				throw new ArgumentNullException(nameof(pickupDirectory));
			}
			SendMail = x => {
				x.WriteTo(Path.Combine(pickupDirectory, $"{Guid.NewGuid():N}.eml"));
			};
		}

		protected void ConfigureNetwork(string host, int port, string username, string password)
		{
			SendMail = x =>
			{
				if (string.IsNullOrWhiteSpace(host))
				{
					throw new ArgumentNullException(nameof(host));
				}
				using (SmtpClient client = new SmtpClient())
				{
					client.Connect(host, port);
					if (!string.IsNullOrWhiteSpace(username) &&
					    !string.IsNullOrWhiteSpace(password))
					{
						client.Authenticate(username, password);
					}
					client.Send(x);
					client.Disconnect(true);
				}
			};
		}

		public void SendEmail(string toRecipient, string toDisplayName, string subject, string body,
			IEnumerable<MimePart> attachments)
		{
			if (string.IsNullOrWhiteSpace(toRecipient))
			{
				throw new ArgumentNullException(nameof(toRecipient));
			}
			MimeMessage mail = new MimeMessage
			{
				Subject = subject
			};
			mail.From.Add(new MailboxAddress(SenderDisplayName, SenderEmailAddress));
			mail.To.Add(new MailboxAddress(toDisplayName, toRecipient));
			var multipart = new Multipart("mixed")
			{
				new TextPart("html")
				{
					Text = body,
					ContentTransferEncoding = ContentEncoding.QuotedPrintable
				}
			};
			foreach (MimePart attachment in attachments)
			{
				multipart.Add(attachment);
			}
			mail.Body = multipart;
			SendMail(mail);
		}
	}
}
