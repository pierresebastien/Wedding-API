using System;
using SimpleStack.Orm.Attributes;
using Wedding.Api.Interfaces;

namespace Wedding.Api.Business.Databases
{
	[Alias("users")]
	public class DbUser : IIdentiafiable<int>
	{
		[Alias("id")]
		[PrimaryKey]
		[AutoIncrement]
		public int Id { get; set; }

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

		[Alias("api_key")]
		[Index(Unique = true)]
		public Guid ApiKey { get; set; }

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
	}
}
