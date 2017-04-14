using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SimpleStack.Orm;
using Wedding.Api.Business.Databases;
using Dapper;
using Wedding.Api.Exceptions;

namespace Wedding.Api.Tools
{
    public class InvitationGenerator
    {
	    private const string EventTemplate =
		    @"<li class=""content-item"" data-event=""{0}"">
			     <h3>{1}</h3>
			     <div class=""text"">{2}</div>
			     <div class=""infos"">
			        <div>
			           <i class=""fa fa-fw fa-calendar""></i> {3}
			        </div>
			        {4}
			     </div>
			  </li>";

	    private const string AddressTemplate =
			@"<div>
			     <i class=""fa fa-fw fa-map-marker""></i>
			     <a href=""https://maps.google.com/maps?q={0},{1}"">{2}</a>
			  </div>";

	    private readonly Config _config;
	    private readonly string _templatePath;

	    public InvitationGenerator(Config config)
	    {
		    _config = config;
			_templatePath = Path.Combine(config.ApplicationPath, "Content", "template_invitation.html");
		}

	    public string GenerateInvitationForUser(int userId)
	    {
		    IList<InvitationView> invitations;
		    using (OrmConnection connection = _config.ConnectionFactory.OpenConnection())
		    {
			    DbUser user = connection.First<DbUser>(x => x.Id == userId);
			    if (!user.IsRegistrationCompleted)
			    {
				    throw new WeddingException("L'utilisateur n'a pas encore répondu à l'invitation");
			    }

				JoinSqlBuilder<InvitationView, DbInvitation> query = InvitationView.GetViewBuilder(connection.DialectProvider);
				query.Where<DbInvitation>(x => x.UserId == user.Id && x.IsAttending == true);
				query.OrderBy<DbEvent>(x => x.StartDate);
				invitations = connection.Query<InvitationView>(query.ToSql(), query.Parameters).ToList();
			}

			IList<int> places = new List<int>();
			StringBuilder builder = new StringBuilder();
		    foreach (var invitation in invitations)
		    {
			    string address = string.Empty;
				if (!places.Contains(invitation.PlaceId))
			    {
				    address = $"{invitation.Street}";
				    if (!string.IsNullOrWhiteSpace(invitation.Number))
				    {
					    address += $" {invitation.Number}";
				    }
				    address += $", {invitation.ZipCode} {invitation.City}";
				    address = string.Format(AddressTemplate, invitation.Latitude.ToString(CultureInfo.InvariantCulture),
					    invitation.Longitude.ToString(CultureInfo.InvariantCulture), address);
					places.Add(invitation.PlaceId);
			    }
			    builder.AppendLine(string.Format(EventTemplate, invitation.EventId, invitation.EventName,
				    invitation.EventDescription, invitation.StartDate.ToString("dd/MM/yyyy HH:mm"), address));
		    }

			string template = File.ReadAllText(_templatePath);
		    string temporaryFile = Path.Combine(Path.GetDirectoryName(_templatePath), $"{Guid.NewGuid():N}.html");
			string pdfPath;
			try
		    {
			    File.WriteAllText(temporaryFile, string.Format(template, builder));
			    pdfPath = HtmlToPdfConverter.Convert(temporaryFile);
		    }
		    finally
		    {
			    File.Delete(temporaryFile);
		    }

		    return pdfPath;
	    }
    }
}
