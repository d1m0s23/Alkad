using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameWer.Struct
{
	public class NetworkDetailsPlayerPacket : BaseNetworkPacket
	{
		[JsonProperty("hwid_list")]
		internal string Hwid_list;

		[JsonProperty("modle")]
		internal string Modle;

		[JsonProperty("manufacturer")]
		internal string Manufacturer;

		[JsonProperty("productname")]
		internal string Productname;

		[JsonProperty("organization")]
		internal string Organization;

		[JsonProperty("owner")]
		internal string Owner;

		[JsonProperty("systemroot")]
		internal string Systemroot;

		[JsonProperty("machinename")]
		internal string Machinename;

		[JsonProperty("username")]
		internal string Username;

		[JsonProperty("isbit64")]
		internal bool IsBit64;

		[JsonProperty("memorysize")]
		internal int Memorysize;

		[JsonProperty("processorname")]
		internal string Processorname;

		[JsonProperty("processorid")]
		internal string Processorid;

		[JsonProperty("videoname")]
		internal string Videoname;

		[JsonProperty("videoid")]
		internal string Videoid;

		[JsonProperty("driversname")]
		internal string Driversname;

		[JsonProperty("driverssize")]
		internal int Driverssize;

		[JsonProperty("privateKeyHash")]
		internal string PrivateKeyHash;

		public NetworkDetailsPlayerPacket()
		{
			this.Method = "detailsPlayer";
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
					"hwid_list",
					this.Hwid_list
				},
				{
					"modle",
					this.Modle
				},
				{
					"manufacturer",
					this.Manufacturer
				},
				{
					"productname",
					this.Productname
				},
				{
					"organization",
					this.Organization
				},
				{
					"owner",
					this.Owner
				},
				{
					"systemroot",
					this.Systemroot
				},
				{
					"machinename",
					this.Machinename
				},
				{
					"username",
					this.Username
				},
				{
					"isbit64",
					this.IsBit64
				},
				{
					"memorysize",
					this.Memorysize
				},
				{
					"processorname",
					this.Processorname
				},
				{
					"processorid",
					this.Processorid
				},
				{
					"videoname",
					this.Videoname
				},
				{
					"videoid",
					this.Videoid
				},
				{
					"driversname",
					this.Driversname
				},
				{
					"driverssize",
					this.Driverssize
				},
				{
					"privateKeyHash",
					this.PrivateKeyHash
				}
			});
		}
	}
}
