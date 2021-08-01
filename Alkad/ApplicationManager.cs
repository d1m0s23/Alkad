using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using GameWer.CustomSystem.Information;
using GameWer.CustomSystem.Steamwork;
using GameWer.Data;
using GameWer.Helper;

namespace GameWer
{
	public class ApplicationManager
	{
		internal static bool IsWork = true;

		private static readonly Queue<Action> ListTaskInMainThread = new Queue<Action>();

		internal static DateTime StartApplicationTime { get; } = DateTime.Now;

		internal static void Init()
		{
			try
			{
				OutputManager.Log("Application", "ApplicationManager.Init()");
                InitPreInitialization();
                InitPolicyAccess();
                CheckPrimitivePrivilege();
                CheckSteam();
                CustomSystem.Information.Interface.Init();
			}
			catch (Exception ex)
			{
				OutputManager.Log("Application", $"Exception in ApplicationManager.Init: {ex}");
			}
		}

		private static void InitPreInitialization()
		{
			OutputManager.Log("Application", "ApplicationManager.InitPreInitialization()");
			if (AppInfo.TargetConnectIP != "127.0.0.1")
			{
				string directoryName = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName;
				if (!ValidateObject(Path.Combine(directoryName, "steam_api64.dll"), "8E963EE4007350E6BF428B4D3296B6AF"))
				{
					OutputManager.Log("Application", "ApplicationManager.InitPreInitialization::InvalidSteamAPI");
                    Shutdown();
				}
				if (!ValidateObject(Path.Combine(directoryName, "Facepunch.Steamworks.Win64.dll"), "9d6df92b629331a2cbaf831accd7ec39"))
				{
					OutputManager.Log("Application", "ApplicationManager.InitPreInitialization::InvalidSteamworks");
                    Shutdown();
				}
				if (!ValidateObject(Path.Combine(directoryName, "discord_game_sdk.dll"), "98CF3C5869073EF4403F42DB93F08A08"))
				{
					OutputManager.Log("Application", "ApplicationManager.InitPreInitialization::InvalidDiscord");
                    Shutdown();
				}
				if (!ValidateObject(Path.Combine(directoryName, "GameWer.SDK.dll"), "e76d49ef22e8a3010ad556acb7bdff50"))
				{
					OutputManager.Log("Application", "ApplicationManager.InitPreInitialization::InvalidSDK");
                    Shutdown();
				}
				if (!ValidateObject(Path.Combine(directoryName, "websocket-sharp.dll"), "7EDE6550BC9B175146CEB10F2CCFF9A1"))
				{
					OutputManager.Log("Application", "ApplicationManager.InitPreInitialization::InvalidWebSocket");
                    Shutdown();
				}
				if (!ValidateObject(Path.Combine(directoryName, "Newtonsoft.Json.dll"), "871CD27688DD512C0E14EE075808993F"))
				{
					OutputManager.Log("Application", "ApplicationManager.InitPreInitialization::InvalidJSON");
                    Shutdown();
				}
				if (!ValidateObject(Path.Combine(directoryName, "GameWer.Module.dll"), "72c8962b6b87658db3e6f079e24d0392"))
				{
					OutputManager.Log("Application", "ApplicationManager.InitPreInitialization::InvalidModule");
                    Shutdown();
				}
			}
		}

		private static bool ValidateObject(string path, string valid)
		{
			if (!string.IsNullOrEmpty(valid))
			{
				try
				{
					using (MD5 md = MD5.Create())
					{
						using (FileStream fileStream = File.OpenRead(path))
						{
							if (BitConverter.ToString(md.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant() != valid.ToLower())
							{
								return false;
							}
						}
					}
				}
				catch
				{
					return false;
				}
			}
			try
			{
				new X509Certificate2(X509Certificate.CreateFromSignedFile(path));
			}
			catch
			{
				return false;
			}
			return true;
		}

		private static void InitBaseAccount()
		{
			try
			{
				OutputManager.Log("Application", "ApplicationManager.InitBaseAccount()");
				ulong steamID = CustomSystem.Steamwork.Interface.GetSteamID();
				OutputManager.Log("Application", "ApplicationManager.InitBaseAccount::ConnectedAccount: " + steamID.ToString());
				if (steamID == 0UL)
				{
					OutputManager.Log("Application", "ApplicationManager.InitBaseAccount::InvalidAccount");
					MessageBox.Show("Ошибка работы со Steam. Перезапустите Steam от имени админа и войдите в свою учетную запись!\n\nError working with Steam. Restart Steam as administrator and log in to your account!", "Ошибка эксплуатации / Incorrect operation", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    Shutdown();
				}
				UIManager.ProxyForm.OnIncomingSteamID(steamID);
			}
			catch (Exception ex)
			{
				OutputManager.Log("Application", $"Exception in ApplicationManager.InitBaseAccount: {ex}");
			}
		}

		private static void CheckPrimitivePrivilege()
		{
			try
			{
				OutputManager.Log("Application", "ApplicationManager.CheckPrimitivePrivilege()");
				bool hasAdminPrivilege = false;
				using (WindowsIdentity current = WindowsIdentity.GetCurrent())
					hasAdminPrivilege = new WindowsPrincipal(current).IsInRole(WindowsBuiltInRole.Administrator);
				if (hasAdminPrivilege == true)
					return;

				OutputManager.Log("Application", "ApplicationManager.CheckPrimitivePrivilege::NotPrivilege");
				MessageBox.Show("Для работы античита - необходимо запустить античит от имени администратора.\n\nFor anti-cheat to work, you must run anti-cheat on behalf of the administrator.", "Ошибка эксплуатации / Incorrect operation", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				Shutdown();
			}
			catch (Exception ex)
			{
				OutputManager.Log("Application", $"Exception in ApplicationManager.CheckPrimitivePrivilege: {ex}");
			}
		}

		private static void CheckSteam()
		{
			try
			{
				OutputManager.Log("Application", "ApplicationManager.CheckSteam()");
				if (CustomSystem.Steamwork.Interface.HasSteamRunned() == true)
					return;

				OutputManager.Log("Application", "ApplicationManager.CheckSteam::NotFoundSteam");
				MessageBox.Show("Для работы античита - необходимо запустить Steam и авторизоваться в нем. Если Steam у вас запущен, перезапустите его от имени администратора.\n\nFor anti-cheat to work, you need to start Steam and log in to it. If you have Steam running, restart it as administrator.", "Неправильная эксплуатация / Incorrect operation", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				Shutdown();
			}
			catch (Exception ex)
			{
				OutputManager.Log("Application", $"Exception in ApplicationManager.CheckSteam: {ex}");
			}
		}

		private static void InitPolicyAccess()
		{
			try
			{
				OutputManager.Log("Application", "ApplicationManager.InitPolicyAccess()");
				bool flag = false;
				try
				{
					if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "GameWerAcceptSafety.dat")))
						flag = true;
				}
				catch
				{
				}
				if (!flag)
				{
					OutputManager.Log("Application", "ApplicationManager.InitPolicyAccess::NotHavePolice");
					if (MessageBox.Show("Вы даете согласие на сбор и обработку ваших персональных данных и информации собраной с этого устройства для автоматического анализа и предоставления администраторам игровых серверов?\n\nDo you consent to the collection and processing of your personal data and information collected from this device to automatically analyze and provide game server administrators?", "Сообщение безопасности / Safety message", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
					{
						OutputManager.Log("Application", "ApplicationManager.InitPolicyAccess::PoliceDenied");
						MessageBox.Show("Данная программа является античитом, и без сбора информации к сожалению - не может работоать. Так как - вы не дали согласия, данная программа(античит) будет закрыта.\n\nThis program is an anti-cheat, and unfortunately it cannot work without collecting information. Since - you did not give consent, this program (anti-cheat) will be closed.", "Сообщение безопасности / Safety message", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        Shutdown();
					}
					else
					{
						OutputManager.Log("Application", "ApplicationManager.InitPolicyAccess::PoliceAccept");
						File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "GameWerAcceptSafety.dat"), DateTime.Now.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				OutputManager.Log("Application", $"Exception in ApplicationManager.InitPolicyAccess: {ex}");
			}
		}

		internal static void Start()
		{
            InitBaseAccount();
			UIManager.ProxyForm.OnApplicationState("appVersion:4.1.6.0");
		}

		internal static void StartApplicationWorker()
		{
			OutputManager.Log("Application", "ApplicationManager.StartApplicationWorker()");
            Helper.Timer.Timeout(new Action(OnApplicationInitialized), new Action<Exception>(OnApplicationInitializationException), 10f);
			while (IsWork)
			{
				try
				{
					Action taskInMainThread = null;
					while ((taskInMainThread = GetTaskInMainThread()) != null)
					{
						try
						{
							if (taskInMainThread != null)
								taskInMainThread();
						}
						catch (Exception ex)
						{
							OutputManager.Log("Application",
							  $"Exception in ApplicationWorker::CallActonInMainThread:{(taskInMainThread != null ? taskInMainThread.Method.ToString() : "null")}: {ex}");
						}
					}
				}
				catch (Exception ex3)
				{
					OutputManager.Log("Application", $"Exception in ApplicationManager.StartApplicationWorker:While: {ex3}");
				}
				Thread.Sleep(33);
			}
		}

		private static void OnApplicationInitializationException(Exception exception)
		{
			OutputManager.Log("Application", $"Exception in OnApplicationInitialized: {exception}");
		}

		private static void OnApplicationInitialized()
		{
			OutputManager.Log("Application", "ApplicationManager.OnApplicationInitialized()");
			NetworkManager.Start();
		}

		internal static void Shutdown()
		{
			try
			{
				OutputManager.Log("Application", "Finish...");
				IsWork = false;
				KeyManager.Shutdown();
				ProcessManager.Shutdown();
				DiscordManager.Shutdown();
				UIManager.Shutdown();
				NetworkManager.Shutdown();
			}
			catch (Exception ex)
			{
				OutputManager.Log("Application", $"Exception in ApplicationManager.Shutdown: {ex}");
			}
			Environment.Exit(0);
		}

		internal static void SetTaskInMainThread(Action action)
		{
			lock (ListTaskInMainThread)
                ListTaskInMainThread.Enqueue(action);
		}

		internal static Action GetTaskInMainThread()
		{
			Action action = null;
			lock (ListTaskInMainThread)
			{
				if (ListTaskInMainThread.Count > 0)
					action = ListTaskInMainThread.Dequeue();
			}
			
			return action;
		}
	}
}
