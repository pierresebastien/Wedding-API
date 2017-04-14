using SimpleStack.Orm;
using SimpleStack.Orm.Attributes;

namespace Wedding.Api.Business.Databases
{
	[Alias("user_links")]
	public class DbUserLink
	{
		[Alias("user_id")]
		[PrimaryKey]
		[ForeignKey(typeof(DbUser))]
		public int UserId { get; set; }

		[Alias("other_user_id")]
		[PrimaryKey]
		[ForeignKey(typeof(DbUser))]
		public int OtherUserId { get; set; }

		[Alias("type")]
		public UserLinkType Type { get; set; }
	}

	public class FamilyUserView
	{
		public int Id { get; set; }

		[Alias("type")]
		public UserLinkType Type { get; set; }

		[Alias("first_name")]
		public string FirstName { get; set; }

		[Alias("last_name")]
		public string LastName { get; set; }

		[Alias("nickname")]
		public string NickName { get; set; }

		[Alias("mail")]
		public string Mail { get; set; }

		[Alias("additionnal_infos")]
		public string AdditionnalInfos { get; set; }

		[Alias("street")]
		public string Street { get; set; }

		[Alias("number")]
		public string Number { get; set; }

		[Alias("box")]
		public string Box { get; set; }

		[Alias("zip_code")]
		public int ZipCode { get; set; }

		[Alias("city")]
		public string City { get; set; }

		[Alias("is_registration_completed")]
		public bool IsRegistrationCompleted { get; set; }

		public static JoinSqlBuilder<FamilyUserView, DbUserLink> GetViewBuilder(IDialectProvider provider)
		{
			return new JoinSqlBuilder<FamilyUserView, DbUserLink>(provider)
				.Join<DbUserLink, DbUser>(x => x.OtherUserId, x => x.Id)
				.Select<DbUserLink>(x => new { Id = x.OtherUserId, x.Type})
				.Select<DbUser>(x => new
				{
					x.FirstName,
					x.LastName,
					x.NickName,
					x.Mail,
					x.AdditionnalInfos,
					x.Street,
					x.Number,
					x.Box,
					x.ZipCode,
					x.City,
					x.IsRegistrationCompleted
				});
		}
	}
}
