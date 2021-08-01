using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameWer.Struct
{
	public class NetworkAuthPacket : BaseNetworkPacket
	{
		[JsonProperty("version")]
		internal string Version;

		[JsonProperty("steamid")]
		internal string SteamID;

		[JsonProperty("hwid")]
		internal string HWID;

		[JsonProperty("pcid")]
		internal string PCID;

		[JsonProperty("dsid")]
		internal string DSID;

		[JsonProperty("lastSessionKey")]
		internal string LastSessionKey;

		[JsonProperty("publicKey")]
		internal string PublicKey;

		[JsonProperty("publicKeyHash")]
		internal string PublicKeyHash;

		public NetworkAuthPacket()
		{
			this.Method = "client";
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
					"version",
					this.Version
				},
				{
					"steamid",
					this.SteamID
				},
				{
					"hwid",
					this.HWID
				},
				{
					"pcid",
					this.PCID
				},
				{
					"dsid",
					this.DSID
				},
				{
					"lastSessionKey",
					this.LastSessionKey
				},
				{
					"publicKey",
					this.PublicKey
				},
				{
					"publicKeyHash",
					this.PublicKeyHash
				}
			});
		}
	}
}
