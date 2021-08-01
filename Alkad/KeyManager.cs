using System;
using System.Threading;
using System.Windows.Forms;
using GameWer.CustomSystem.KeyLogger;

namespace GameWer
{
	public class KeyManager
	{
		internal static void Init()
		{
			OutputManager.Log("Key", "KeyManager.Init");
			Interface.OnKeyPress = new Action<Keys>(OnKeyState);
			Interface.Init();
		}

		internal static void Shutdown()
		{
			OutputManager.Log("Key", "KeyManager.Shutdown");
			try
			{
				Interface.WorkerThread?.Abort();
			}
			catch
			{
			}
		}

		private static void OnKeyState(Keys key)
		{
			ApplicationManager.SetTaskInMainThread(() => { });
		}
	}
}
