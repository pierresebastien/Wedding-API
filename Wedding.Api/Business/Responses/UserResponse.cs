using System.Collections.Generic;
using System.Linq;
using Wedding.Api.Business.Databases;

namespace Wedding.Api.Business.Responses
{
    public abstract class BaseUserResponse
    {
	    protected BaseUserResponse()
	    {
		    
	    }

	    protected BaseUserResponse(DbUser user)
	    {
		    Id = user.Id;
		    FirstName = user.FirstName;
		    LastName = user.LastName;
		    AdditionnalInfos = user.AdditionnalInfos;
		    Street = user.Street;
		    Number = user.Number;
		    Box = user.Box;
		    ZipCode = user.ZipCode;
		    City = user.City;
		    IsRegistrationCompleted = user.IsRegistrationCompleted;
		    Nickname = string.IsNullOrWhiteSpace(user.NickName) ? user.FirstName : user.NickName;
	    }

		public int Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Nickname { get; set; }

		public string AdditionnalInfos { get; set; }
		
		public string Street { get; set; }
		
		public string Number { get; set; }
		
		public string Box { get; set; }
		
		public int ZipCode { get; set; }
		
		public string City { get; set; }

		public bool IsRegistrationCompleted { get; set; }
	}

	public class FamilyUserResponse : BaseUserResponse
	{
		public FamilyUserResponse()
		{
			
		}

		public FamilyUserResponse(DbUser user, DbUserLink userLink) : base(user)
		{
			Type = userLink.Type;
		}

		public FamilyUserResponse(FamilyUserView user)
		{
			Id = user.Id;
			FirstName = user.FirstName;
			LastName = user.LastName;
			AdditionnalInfos = user.AdditionnalInfos;
			Street = user.Street;
			Number = user.Number;
			Box = user.Box;
			ZipCode = user.ZipCode;
			City = user.City;
			Type = user.Type;
			IsRegistrationCompleted = user.IsRegistrationCompleted;
			Nickname = string.IsNullOrWhiteSpace(user.NickName) ? user.FirstName : user.NickName;
		}

		public UserLinkType Type { get; set; }
	}

	public class UserResponse : BaseUserResponse
	{
		public UserResponse()
		{
			Family = new FamilyUserResponse[0];
		}

		public UserResponse(DbUser user, IList<FamilyUserView> family) : base(user)
		{
			Family = family != null ? family.Select(x => new FamilyUserResponse(x)).ToArray() : new FamilyUserResponse[0];
		}

		public FamilyUserResponse[] Family { get; set; }
	}
}
