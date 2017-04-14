using SimpleStack.Orm.Attributes;
using Wedding.Api.Interfaces;

namespace Wedding.Api.Business.Databases
{
	[Alias("places")]
	public class DbPlace : IIdentiafiable<int>
	{
		[Alias("id")]
		[PrimaryKey]
		[AutoIncrement]
		public int Id { get; set; }

		[Alias("name")]
		public string Name { get; set; }

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
	}
}
