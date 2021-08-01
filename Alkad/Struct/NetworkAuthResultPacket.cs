using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameWer.Struct
{
	public class NetworkAuthResultPacket : BaseNetworkPacket
	{
		[JsonProperty("result")]
		internal bool Result;

		[JsonProperty("privateKey")]
		internal string PrivateKey;

		[JsonProperty("sessionKey")]
		public string SessionKey;

		public NetworkAuthResultPacket()
		{
			this.Method = "authResult";
		}

		internal override string ParseJSON()
		{
			return JsonConvert.SerializeObject(new Dictionary<string, object>
			{
				{
					"method",
					this.Method
				},
				{
					"result",
					this.Result
				},
				{
					"privateKey",
					this.PrivateKey
				},
				{
					"sessionKey",
					this.SessionKey
				}
			});
		}

		internal static NetworkAuthResultPacket ParseObject(string content)
		{
			return ParseObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(content));
		}

		internal static NetworkAuthResultPacket ParseObject(Dictionary<string, object> json)
		{
			return new NetworkAuthResultPacket
			{
				Result = (bool)json["result"],
				PrivateKey = (string)json["privateKey"],
				SessionKey = (string)json["sessionKey"]
			};
		}
	}
}
