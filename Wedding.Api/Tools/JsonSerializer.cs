using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Wedding.Api.Tools
{
	public class CamelCaseJsonSerializer : JsonSerializer
	{
		public CamelCaseJsonSerializer()
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver();
		}
	}
}
