using System;
using System.Diagnostics;
using Steamworks;

namespace GameWer.CustomSystem.Steamwork
{
	public class Interface
	{
		private static ulong SteamID = 2147483647UL;

		static Interface()
		{
			SteamClient.Init(480, true);
            SteamID += SteamClient.SteamId;
		}

		internal static bool HasSteamRunned()
		{
			try
			{
				return Process.GetProcessesByName("Steam").Length != 0;
			}
			catch (Exception ex)
			{
				OutputManager.Log("CustomSystem.Steamwork.Interface", $"Exception in HasSteamRunned: {ex}");
			}
			return false;
		}

		internal static ulong GetSteamID()
		{
			return SteamID - 2147483647UL;
		}
	}
}
