using System;
using Nancy;
using Nancy.Security;
using SimpleStack.Orm;
using Wedding.Api.Business.Databases;
using Wedding.Api.Business.Responses;

namespace Wedding.Api.Services
{
	public sealed class InfoService : NancyModule
    {
	    private readonly Config _config;

        public InfoService(Config config) : base("/info")
        {
	        _config = config;
	        Get("/date", args => Negotiate.WithModel(GetWeddingDate()));
			Get("/account", args => Negotiate.WithModel(GetBankAccount()));
        }

		private DateResponse GetWeddingDate()
		{
			using (OrmConnection connection = _config.ConnectionFactory.OpenConnection())
			{
				return new DateResponse { Date = DateTime.SpecifyKind(connection.First<DbEvent>(x => x.Id == 2).StartDate, DateTimeKind.Local)};
			}
		}

		private InfoResponse GetBankAccount()
		{
			this.RequiresAuthentication();
			using (OrmConnection connection = _config.ConnectionFactory.OpenConnection())
			{
				return new InfoResponse { Message = connection.First<DbInfo>(x => x.Key == DbInfo.BankAccountKey).Value };
			}
		}
	}
}