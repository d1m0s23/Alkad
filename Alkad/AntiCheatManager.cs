using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using GameWer.CustomSystem.Information;
using GameWer.CustomSystem.Steamwork;
using GameWer.Helper;
using GameWer.Struct;
using WebSocketSharp;
using Timer = GameWer.Helper.Timer;

namespace GameWer
{
	public class AntiCheatManager
	{
		internal static string LastKeySession = string.Empty;

		internal static string LastPublicKey = string.Empty;

		internal static string PrivateKey = string.Empty;

		private static BannedPlayerResultPacket CurrentBannedPlayerResultPacket { get; set; } = null;

		internal static void OnNetworkAuthResultPacket(NetworkAuthResultPacket authPacket)
		{
			try
			{
				OutputManager.Log("AntiCheat", "AntiCheatManager.OnNetworkAuthResultPacket: " + authPacket.Result.ToString());
                LastKeySession = authPacket.SessionKey;
                PrivateKey = authPacket.PrivateKey;
				UIManager.ProxyForm.OnNetworkAuthSuccess();
				if (CurrentBannedPlayerResultPacket != null)
                    OnNetworkBannedPlayerPacket(CurrentBannedPlayerResultPacket);

				NetworkManager.Send(new NetworkDetailsPlayerPacket
				{
					Hwid_list = string.Join(",", CustomSystem.Information.Interface.GetHWIDList),
					Modle = CustomSystem.Information.Interface.Model,
					Driversname = CustomSystem.Information.Interface.DriversName,
					Driverssize = int.Parse(CustomSystem.Information.Interface.DriversSize),
					Machinename = CustomSystem.Information.Interface.MachineName,
					Manufacturer = CustomSystem.Information.Interface.Manufacturer,
					Memorysize = int.Parse(CustomSystem.Information.Interface.MemorySize),
					Organization = CustomSystem.Information.Interface.RegisteredOrganization,
					Owner = CustomSystem.Information.Interface.RegisteredOwner,
					Processorid = CustomSystem.Information.Interface.ProcessorID,
					Processorname = CustomSystem.Information.Interface.ProcessorName,
					Productname = CustomSystem.Information.Interface.ProductName,
					Systemroot = CustomSystem.Information.Interface.SystemRoot,
					Username = CustomSystem.Information.Interface.UserName,
					Videoid = CustomSystem.Information.Interface.VideocardID,
					Videoname = CustomSystem.Information.Interface.VideocardName,
					IsBit64 = CustomSystem.Information.Interface.IsBit64OS,
					PrivateKeyHash = Crypto.GetMD5FromLine(LastPublicKey + PrivateKey + ".2")
				}.ParseJSON());
			}
			catch (Exception ex)
			{
				OutputManager.Log("AntiCheat", $"Exception in AntiCheatManager.OnNetworkAuthResultPacket: {ex}");
			}
		}

		internal static void OnNetworkBadVersionPacket(NetworkBadVersionPacket packet)
		{
			try
			{
				OutputManager.Log("AntiCheat", "AntiCheatManager.OnNetworkBadVersionPacket");
				UIManager.ProxyForm.OnApplicationState("badVersion");
				NetworkManager.NotNeedReconnect = true;
			}
			catch (Exception ex)
			{
				OutputManager.Log("AntiCheat", $"Exception in AntiCheatManager.OnNetworkBadVersionPacket: {ex}");
			}
		}

		private static void OnNetworkProcessesSystemReady(NetworkProcessesSystemReady packet)
		{
			try
			{
				OutputManager.Log("AntiCheat", "AntiCheatManager.OnNetworkProcessesSystemReady");
				ProcessManager.ListSendPath = new HashSet<string>();
				ProcessManager.DoSendNewProcesses();
			}
			catch (Exception ex)
			{
				OutputManager.Log("AntiCheat", $"Exception in AntiCheatManager.OnNetworkProcessesSystemReady: {ex}");
			}
		}

		internal static void OnNetworkPacket(string method, string fullContent, Dictionary<string, object> packet)
		{
			try
			{
				switch (method)
				{
					case "authResult":
						var packet1 = NetworkAuthResultPacket.ParseObject(packet);
						if (packet1.Result)
						{
							OnNetworkAuthResultPacket(packet1);
							break;
						}
						NetworkManager.BaseSocket.CloseAsync(CloseStatusCode.UnsupportedData);
						break;
					case "badVersion":
						OnNetworkBadVersionPacket(new NetworkBadVersionPacket());
						break;
					case "processesReady":
						OnNetworkProcessesSystemReady(new NetworkProcessesSystemReady());
						break;
					case "bannedResult":
						OnNetworkBannedPlayerPacket(BannedPlayerResultPacket.ParseObject(packet));
						break;
					case "authDiscord":
						OnDiscordCodeReceived(packet["code"] as string);
                        break;
                    default:
						{
							NetworkAuthResultPacket networkAuthResultPacket = NetworkAuthResultPacket.ParseObject(packet);
							if (networkAuthResultPacket.Result)
								OnNetworkAuthResultPacket(networkAuthResultPacket);
							else
								NetworkManager.BaseSocket.CloseAsync(CloseStatusCode.UnsupportedData);
						}
						break;
				}
			}
			catch (Exception ex)
			{
				OutputManager.Log("AntiCheat", $"Exception in AntiCheatManager.OnNetworkPacket: {ex}");
			}
		}

		private static void OnDiscordCodeReceived(string code)
		{
			SystemSounds.Asterisk.Play();
			ThreadPool.QueueUserWorkItem(msg =>
			{
				MessageBox.Show("[RU] Теперь, для индификации пользователя используется DISOCRD, Впишите 4х значный код из статуса в чат канал #authentication и вы будете авторизованы!\n\n[EN] Now, DISOCRD is used to identify the user, Enter the 4-digit code from the status in the #authentication chat channel and you will be authorized!");
			});
			UIManager.ProxyForm.OnIncomingCode(code);
		}

		private static void OnNetworkBannedPlayerPacket(BannedPlayerResultPacket bannedPlayerResultPacket)
		{
			try
			{
				OutputManager.Log("AntiCheat",
				  $"AntiCheatManager.OnNetworkBannedResultPacket: {bannedPlayerResultPacket.Reason}, {Date.UnixTimeStampToDateTime(bannedPlayerResultPacket.FinishAt)}");
				var dateTime = Date.UnixTimeStampToDateTime(bannedPlayerResultPacket.FinishAt);
				CurrentBannedPlayerResultPacket = bannedPlayerResultPacket;
				UIManager.ProxyForm.OnIncomingBanned(bannedPlayerResultPacket.Reason, dateTime);
			}
			catch (Exception ex)
			{
				OutputManager.Log("AntiCheat", $"Exception in AntiCheatManager.OnNetworkBannedResultPacket: {ex}");
			}
		}

		internal static void OnNetworkDisconnected(string reason)
		{
			try
			{
				OutputManager.Log("AntiCheat", $"AntiCheatManager.OnNetworkDisconnected({reason})");
				ProcessManager.ListSendPath = null;
				CurrentBannedPlayerResultPacket = null;
				UIManager.ProxyForm.OnNetworkDisconnected(reason);
				if (!ApplicationManager.IsWork)
					return;
				Timer.Timeout(() =>
				{
					if (NetworkManager.NotNeedReconnect == (int.Parse("0") == 1))
						NetworkManager.BaseSocket.ConnectAsync();
					else
						OutputManager.Log("AntiCheat", "AntiCheatManager.OnNetworkDisconnected::DetectedNoReconnect");
				}, ex => OutputManager.Log("Network", $"NetworkManager.OnNetworkClose::ReconnectingException:{ex}"), 3f);
			}
			catch (Exception ex)
			{
				OutputManager.Log("AntiCheat", $"Exception in AntiCheatManager.OnNetworkDisconnected: {ex}");
			}
		}

		internal static void OnNetworkConnected()
		{
			if (DiscordManager.DSID == string.Empty)
			{
				UIManager.ProxyForm.OnApplicationState("Not found DISCORD");
				SystemSounds.Asterisk.Play();
				MessageBox.Show("[RU] Для работы GameWer, необходимо что бы - был запущен Discord. Хотя бы до окочания авторизации(Пока вы не увидите статус онлайн). А потом вы можете его смело закрыть, если он вам мешает. Ваш дискорд аккаунт должен быть связан с номером телефона! А вы, для авторизации - должны быть на нашем дискорд сервере! Кнопка присоеденится, в самом низу окошка античита.\n\n[EN] For GameWer to work, you need to have Discord running. At least until the end of the authorization (Until you see the online status). And then you can safely close it if it bothers you. Your discord account must be linked to a phone number! And you, for authorization, must be on our discord server! The button will connect, at the very bottom of the anti-cheat window.");
				OutputManager.Log("AntiCheatManager", " Not found Discord");
				ApplicationManager.Shutdown();
			}
			else
			{
				try
				{
					OutputManager.Log("AntiCheat", "AntiCheatManager.OnNetworkConnected()");
					UIManager.ProxyForm.OnNetworkConnected();
                    LastPublicKey = Crypto.GetMD5FromLine(DateTime.Now.ToString());
					NetworkManager.Send(new NetworkAuthPacket
					{
						Version = "4.1.6.0",
						SteamID = CustomSystem.Steamwork.Interface.GetSteamID().ToString().Substring(0, 17),
						HWID = "",
						PCID = CustomSystem.Information.Interface.apphash,
						DSID = DiscordManager.DSID,
						LastSessionKey = LastKeySession,
						PublicKey = LastPublicKey,
						PublicKeyHash = Crypto.GetMD5FromLine(LastPublicKey + ".2")
					}.ParseJSON());
				}
				catch (Exception ex)
				{
					OutputManager.Log("AntiCheat", $"Exception in AntiCheatManager.OnNetworkConnected: {ex}");
				}
			}
		}
	}
}
