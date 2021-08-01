using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameWer.Struct
{
	public class NetworkProcessesSystemReady : BaseNetworkPacket
	{
		public NetworkProcessesSystemReady()
		{
			this.Method = "processesReady";
		}

		internal override string ParseJSON()
		{
			return JsonConvert.SerializeObject(new Dictionary<string, object>
			{
				{
					"method",
					this.Method
				}
			});
		}

		internal static NetworkProcessesSystemReady ParseObject(string content)
		{
			return ParseObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(content));
		}

		internal static NetworkProcessesSystemReady ParseObject(Dictionary<string, object> json)
		{
			return new NetworkProcessesSystemReady();
		}
	}
}
