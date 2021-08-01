using System;
using System.IO;
using System.Threading;
using GameWer.Data;

namespace GameWer
{
	internal static class Bootstrap
	{
		private static Mutex MutexInstance;

		[STAThread]
		private static void Main(string[] args)
		{
			try
			{
                DoInit();
			}
			catch (Exception ex)
			{
				File.AppendAllText("./output.log", $"\n[{DateTime.Now}] [Main]: Exception: {ex}");
			}
		}

		private static void DoInit()
		{
			OutputManager.Log("Bootstrap", "Incoming Main : 4.1.6.0");
            MutexInstance = new Mutex(true, "GameWer");
			if (MutexInstance.WaitOne(TimeSpan.Zero, true) || AppInfo.TargetConnectIP == "127.0.0.1")
			{
				ApplicationManager.Init();
				UIManager.Init();
				ProcessManager.Init();
				KeyManager.Init();
				DiscordManager.Init();
				NetworkManager.Init();
				ApplicationManager.Start();
				ApplicationManager.StartApplicationWorker();
                MutexInstance.ReleaseMutex();
				OutputManager.Log("Bootstrap", "End of the main thread...");
			}
			else
			{
				OutputManager.Log("Bootstrap", "Detected duplicate application!");
			}
		}
	}
}
