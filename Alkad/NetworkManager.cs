using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GameWer.Data;
using GameWer.Helper;
using Newtonsoft.Json;
using WebSocketSharp;

namespace GameWer
{
	public class NetworkManager
	{
		internal static WebSocket BaseSocket;

		internal static bool HasConnected = false;

		internal static bool NotNeedReconnect = false;

		internal static void Init()
		{
			OutputManager.Log("Network", "NetworkManager.Init()");
            BaseSocket = new WebSocket(string.Format("ws://{0}:{1}/", AppInfo.TargetConnectIP, AppInfo.TargetConnectPort), Array.Empty<string>());
            BaseSocket.OnClose += OnNetworkClose;
            BaseSocket.OnMessage += OnNetworkMessage;
            BaseSocket.OnOpen += OnNetworkConnected;
            BaseSocket.OnError += OnSocketError;
		}

		internal static void Send(string packet)
		{
			string text = Cryptography.OpenSSLEncrypt(packet, "");
			BaseSocket?.SendAsync(text + Crypto.GetMD5FromLine(text), status => { });
		}

		private static void OnSocketError(object sender, ErrorEventArgs e)
		{
			OutputManager.Log("Network", $"NetworkManager.OnSocketError:{e.Message}\n{e.Exception}");
		}

		internal static void Start()
		{
			OutputManager.Log("Network", "NetworkManager.Start()");
            BaseSocket.ConnectAsync();
		}

		private static void OnNetworkConnected(object sender, EventArgs e)
		{
			try
			{
				OutputManager.Log("Network", "NetworkManager.OnNetworkConnected()");
				HasConnected = true;
				ApplicationManager.SetTaskInMainThread(new Action(AntiCheatManager.OnNetworkConnected));
			}
			catch (Exception ex)
			{
				OutputManager.Log("Network", $"Exception in NetworkManager.OnNetworkConnected: {ex}");
				BaseSocket?.CloseAsync(CloseStatusCode.InvalidData);
			}
		}

		private static void OnNetworkMessage(object sender, MessageEventArgs e)
		{
			try
			{
				var contentJson = e.Data;
				var list = JsonConvert.DeserializeObject<Dictionary<string, object>>(contentJson);
				ApplicationManager.SetTaskInMainThread(() =>
				{
					object obj;
					var method = string.Empty;
					var isMethodValid = false;

					if (list.TryGetValue("method", out obj))
					{
						method = obj as string;
						isMethodValid = method != null;
					}

					if (isMethodValid == false)
						return;

					AntiCheatManager.OnNetworkPacket(method, contentJson, list);
				});
			}
			catch (Exception ex)
			{
				OutputManager.Log("Network", $"Exception in NetworkManager.OnNetworkMessage: {ex}");
				BaseSocket?.CloseAsync(CloseStatusCode.InvalidData);
			}
		}

		private static void OnNetworkClose(object sender, CloseEventArgs e)
		{
			try
			{
				var reason = e.Reason;
				OutputManager.Log("Network", $"NetworkManager.OnNetworkClose({((CloseStatusCode)e.Code)}, {reason})");
				HasConnected = false;
				ApplicationManager.SetTaskInMainThread(() => AntiCheatManager.OnNetworkDisconnected(reason));
			}
			catch (Exception ex)
			{
				OutputManager.Log("Network", $"Exception in NetworkManager.OnNetworkClose: {ex}");
				BaseSocket?.CloseAsync(CloseStatusCode.InvalidData);
			}
		}

		internal static void Shutdown()
		{
			OutputManager.Log("Network", "NetworkManager.Shutdown()");
			try
			{
				BaseSocket.Close();
			}
			catch
			{
			}
		}
	}
}
