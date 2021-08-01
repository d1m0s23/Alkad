using System;

namespace GameWer.Struct
{
	public class NetworkBadVersionPacket : BaseNetworkPacket
	{
		public NetworkBadVersionPacket()
		{
			this.Method = "badVersion";
		}
	}
}
