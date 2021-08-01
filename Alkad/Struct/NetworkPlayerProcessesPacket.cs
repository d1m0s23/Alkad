using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameWer.Struct
{
	public class NetworkPlayerProcessesPacket : BaseNetworkPacket
	{
		[JsonProperty("processes")]
		internal PlayerProcess[] Processes;

		public NetworkPlayerProcessesPacket()
		{
			this.Method = "playerProcesses";
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
					"processes",
					this.Processes
				}
			});
		}
	}
}
