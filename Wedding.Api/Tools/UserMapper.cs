using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using Nancy;
using Nancy.Authentication.Forms;
using SimpleStack.Orm;
using Wedding.Api.Business.Databases;

namespace Wedding.Api.Tools
{
	public class UserMapper : IUserMapper
	{
		private readonly Config _config;
		private readonly IDictionary<Guid, DbUser> _cache;

		public UserMapper(Config config)
		{
			_config = config;
			_cache = new ConcurrentDictionary<Guid, DbUser>();
		}

		public ClaimsPrincipal GetUserFromIdentifier(Guid identifier, NancyContext context)
		{
			DbUser user;
			if (_cache.ContainsKey(identifier))
			{
				user = _cache[identifier];
			}
			else
			{
				using (OrmConnection connection = _config.ConnectionFactory.OpenConnection())
				{
					user = connection.FirstOrDefault<DbUser>(x => x.ApiKey == identifier);
					if (user != null)
					{
						try
						{
							_cache.Add(identifier, user);
						}
						catch (ArgumentException)
						{
							// NOTE: could happen when multiple calls in //
						}
					}
				}
			}
			if (user != null)
			{
				IList<Claim> claims = new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture))
				};
				return new ClaimsPrincipal(new ClaimsIdentity(claims, "PresharedKey"));
			}
			return null;
		}
	}
}
