using System;
using Newtonsoft.Json;

namespace GameWer.Struct
{
	public class BaseNetworkPacket
	{
		[JsonProperty("method")]
		internal string Method;

		internal virtual string ParseJSON()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
