using System;
using System.Linq;
using System.Security.Claims;
using Nancy;
using Nancy.Security;

namespace Wedding.Api.Services
{
	public abstract class BaseService : NancyModule
	{
		protected readonly Config Config;

		protected BaseService(string modulePath, Config config) : base(modulePath)
		{
			Config = config;

			this.RequiresAuthentication();
		}

		public int GetCurrentUserId()
		{
			Claim claim = Context.CurrentUser.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
			if (claim == null)
			{
				throw new UnauthorizedAccessException();
			}
			return int.Parse(claim.Value);
		}
	}
}