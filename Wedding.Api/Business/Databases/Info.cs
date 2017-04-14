using SimpleStack.Orm.Attributes;

namespace Wedding.Api.Business.Databases
{
	[Alias("infos")]
	public class DbInfo
	{
		public const string BankAccountKey = "BankAccount";

		[Alias("key")]
		[PrimaryKey]
		public string Key { get; set; }

		[Alias("value")]
		public string Value { get; set; }
	}
}
