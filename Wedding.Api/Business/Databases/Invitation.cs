using System;
using SimpleStack.Orm;
using SimpleStack.Orm.Attributes;

namespace Wedding.Api.Business.Databases
{
	[Alias("invitations")]
	public class DbInvitation
	{
		[Alias("user_id")]
		[PrimaryKey]
		[ForeignKey(typeof(DbUser))]
		public int UserId { get; set; }

		[Alias("event_id")]
		[PrimaryKey]
		[ForeignKey(typeof(DbEvent))]
		public int EventId { get; set; }

		[Alias("is_attending")]
		public bool? IsAttending { get; set; }

		[Alias("last_update_date")]
		public DateTime? LastUpdateDate { get; set; }
	}

	public class InvitationView
	{
		[Alias("user_id")]
		public int UserId { get; set; }

		[Alias("is_attending")]
		public bool? IsAttending { get; set; }

		[Alias("event_id")]
		public int EventId { get; set; }

		public string EventName { get; set; }

		public string EventDescription { get; set; }

		[Alias("start_date")]
		public DateTime StartDate { get; set; }

		[Alias("place_id")]
		public int PlaceId { get; set; }

		public string PlaceName { get; set; }

		[Alias("description")]
		public string Description { get; set; }

		[Alias("url")]
		public string Url { get; set; }

		[Alias("latitude")]
		public decimal Latitude { get; set; }

		[Alias("longitude")]
		public decimal Longitude { get; set; }

		[Alias("street")]
		public string Street { get; set; }

		[Alias("number")]
		public string Number { get; set; }

		[Alias("zip_code")]
		public int ZipCode { get; set; }

		[Alias("city")]
		public string City { get; set; }

		public static JoinSqlBuilder<InvitationView, DbInvitation> GetViewBuilder(IDialectProvider provider)
		{
			return new JoinSqlBuilder<InvitationView, DbInvitation>(provider)
				.Join<DbInvitation, DbEvent>(x => x.EventId, x => x.Id)
				.Join<DbEvent, DbPlace>(x => x.PlaceId, x => x.Id)
				.Select<DbInvitation>(x => new {x.UserId, x.EventId, x.IsAttending})
				.Select<DbEvent>(x => new {EventName = x.Name, x.StartDate, x.PlaceId, EventDescription = x.Description })
				.Select<DbPlace>(
					x => new {PlaceName = x.Name, x.Description, x.Street, x.Number, x.ZipCode, x.City, x.Latitude, x.Longitude, x.Url});
		}
	}
}
