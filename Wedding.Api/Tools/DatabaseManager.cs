using SimpleStack.Orm;
using Wedding.Api.Business.Databases;

namespace Wedding.Api.Tools
{
	public class DatabaseManager
	{
		public static void SetUpDatabase(OrmConnectionFactory connectionFactory)
		{
			using (OrmConnection connection = connectionFactory.OpenConnection())
			{
				connection.CreateTableIfNotExists<DbInfo>();
				connection.CreateTableIfNotExists<DbPlace>();
				connection.CreateTableIfNotExists<DbEvent>();
				connection.CreateTableIfNotExists<DbUser>();
				connection.CreateTableIfNotExists<DbUserLink>();
				connection.CreateTableIfNotExists<DbInvitation>();
			}
		}
	}
}
