using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using Dapper;
using MimeKit;
using Nancy;
using Nancy.ModelBinding;
using SimpleStack.Orm;
using Wedding.Api.Business.Databases;
using Wedding.Api.Business.Requests;
using Wedding.Api.Business.Responses;
using Wedding.Api.Exceptions;
using Wedding.Api.Tools;

namespace Wedding.Api.Services
{
	// TODO: add validation on requests => bind and validate
    public sealed class UserService : BaseService
    {
	    private readonly InvitationGenerator _invitationGenerator;
	    private readonly EmailSender _emailSender;

	    public UserService(Config config, InvitationGenerator invitationGenerator, EmailSender emailSender) : base("/users", config)
	    {
		    _invitationGenerator = invitationGenerator;
		    _emailSender = emailSender;
		    Get("/me", args => Negotiate.WithModel(GetCurrentUser()));
			Get("/{Id}/invitations", args => GetInvitations(args.Id));
			Post("/{Id}/invitations", args => SaveParticipation(this.Bind<ParticipationRequest>()));
		}

	    private UserResponse GetCurrentUser()
	    {
		    using (OrmConnection connection = Config.ConnectionFactory.OpenConnection())
		    {
			    int userId = GetCurrentUserId();
			    DbUser user = connection.First<DbUser>(x => x.Id == userId);
			    JoinSqlBuilder<FamilyUserView, DbUserLink> query = FamilyUserView.GetViewBuilder(connection.DialectProvider);
			    query.Where<DbUserLink>(x => x.UserId == userId);
			    IList<FamilyUserView> family = connection.Query<FamilyUserView>(query.ToSql(), query.Parameters).ToList();
				return new UserResponse(user, family);
		    }
	    }

	    private InvitationResponse[] GetInvitations(string userIdString)
	    {
		    int userId;
		    if (!int.TryParse(userIdString, out userId))
		    {
			    throw new BadRequestException("Invalid user identifier");
		    }

		    using (OrmConnection connection = Config.ConnectionFactory.OpenConnection())
		    {
			    int currentUserId = GetCurrentUserId();
			    if (userId != currentUserId)
			    {
				    if (!connection.Select<DbUserLink>(x => x.UserId == currentUserId && x.OtherUserId == userId).Any())
				    {
					    throw new WeddingException("Vous n'avez pas le droit de voir l'invitation de cette personne");
				    }
			    }
			    JoinSqlBuilder<InvitationView, DbInvitation> query = InvitationView.GetViewBuilder(connection.DialectProvider);
			    query.Where<DbInvitation>(x => x.UserId == userId);
			    query.OrderBy<DbEvent>(x => x.StartDate);
			    return connection.Query<InvitationView>(query.ToSql(), query.Parameters).Select(x => new InvitationResponse(x)).ToArray();
		    }
	    }

		private Response SaveParticipation(ParticipationRequest request)
		{
			DbUser currentUser;
			DbUser invitationUser;

			using (OrmConnection connection = Config.ConnectionFactory.OpenConnection())
			using (DbTransaction transaction = connection.BeginTransaction())
			{
				try
				{
					int currentUserId = GetCurrentUserId();
					currentUser = connection.First<DbUser>(x => x.Id == currentUserId);

					if (request.UserId != currentUserId)
					{
						if (!connection.Select<DbUserLink>(x => x.UserId == currentUserId && x.OtherUserId == request.UserId).Any())
						{
							throw new WeddingException("Vous n'avez pas le droit de valider l'invitation de cette personne");
						}
					}

					invitationUser = connection.First<DbUser>(x => x.Id == request.UserId);
					invitationUser.Street = request.Street;
					invitationUser.Number = request.Number;
					invitationUser.Box = request.Box;
					invitationUser.ZipCode = request.ZipCode;
					invitationUser.City = request.City;
					invitationUser.AdditionnalInfos = request.AdditionnalInfos;
					invitationUser.IsRegistrationCompleted = true;
					connection.Update(invitationUser);

					if (request.Attendings != null)
					{
						foreach (var invitation in connection.Select<DbInvitation>(x => x.UserId == request.UserId))
						{
							AttendingRequest attending = request.Attendings.SingleOrDefault(x => x.EventId == invitation.EventId);
							if (attending != null)
							{
								invitation.IsAttending = attending.IsAttending;
								invitation.LastUpdateDate = DateTime.Now;
								connection.Update(invitation);
							}
						}
					}

					transaction.Commit();
				}
				catch (Exception)
				{
					transaction.Rollback();
					throw;
				}
			}

			bool isAttending = request.Attendings.Any(x => x.IsAttending.HasValue && x.IsAttending.Value);
			if (!string.IsNullOrWhiteSpace(currentUser.Mail) && isAttending)
			{
				string invitationPdf = string.Empty;
				Stream planStream = null;
				Stream invitationStream = null;
				IList<MimePart> attatchments = new List<MimePart>();

				try
				{
					invitationPdf = _invitationGenerator.GenerateInvitationForUser(request.UserId);
					invitationStream = File.OpenRead(invitationPdf);

					attatchments.Add(new MimePart("application", "pdf")
					{
						ContentObject = new ContentObject(invitationStream),
						ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
						ContentTransferEncoding = ContentEncoding.Base64,
						FileName = "invitation.pdf"
					});

					AttendingRequest ceremony = request.Attendings.SingleOrDefault(x => x.EventId == 2);
					if (ceremony?.IsAttending != null && ceremony.IsAttending.Value)
					{
						string planFilePath = Path.Combine(Config.ApplicationPath, "Content", "Plan Malmundarium.pdf");
						planStream = File.OpenRead(planFilePath);
						attatchments.Add(new MimePart("application", "pdf")
						{
							ContentObject = new ContentObject(planStream),
							ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
							ContentTransferEncoding = ContentEncoding.Base64,
							FileName = Path.GetFileName(planFilePath)
						});
					}

					string toDisplayName = string.IsNullOrWhiteSpace(currentUser.NickName)
						? currentUser.FirstName
						: currentUser.NickName;
					string displayName = string.IsNullOrWhiteSpace(invitationUser.NickName)
						? invitationUser.FirstName
						: invitationUser.NickName;
					_emailSender.SendEmail(currentUser.Mail, toDisplayName, $"Invitation de {displayName}", "Tu trouveras en pièce jointe ton invitation", attatchments);
				}
				finally
				{
					invitationStream?.Dispose();
					planStream?.Dispose();
					if (!string.IsNullOrWhiteSpace(invitationPdf))
					{
						File.Delete(invitationPdf);
					}
				}
			}

			return new Response().WithStatusCode(HttpStatusCode.OK);
		}
	}
}
