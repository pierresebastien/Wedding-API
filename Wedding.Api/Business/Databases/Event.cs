using System;
using SimpleStack.Orm.Attributes;
using Wedding.Api.Interfaces;

namespace Wedding.Api.Business.Databases
{
	[Alias("events")]
	public class DbEvent : IIdentiafiable<int>
	{
		[Alias("id")]
		[PrimaryKey]
		[AutoIncrement]
		public int Id { get; set; }

		[Alias("name")]
		public string Name { get; set; }

		[Alias("description")]
		public string Description { get; set; }

		[Alias("start_date")]
		public DateTime StartDate { get; set; }

		[Alias("place_id")]
		[ForeignKey(typeof(DbPlace))]
		[Index]
		public int PlaceId { get; set; }
	}
}
