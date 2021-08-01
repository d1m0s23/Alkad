using System;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using GameWer.SDK;

namespace GameWer
{
	public class UIManager
	{
		internal static Thread UIThread;

		internal static AppDomain UIDomain = null;

		internal static GameWerUI GameWerUIInstance;

		internal static IGameWerForm ProxyForm;

		private static PermissionSet CreateDomainPermission()
		{
			PermissionSet permissionSet = new PermissionSet(PermissionState.None);
			permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.AllFlags));
			return permissionSet;
		}

		private static AppDomainSetup CreateDomainSetup()
		{
			return new AppDomainSetup
			{
				ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
			};
		}

		private static void InitThread()
		{
			try
			{
				Type typeFromHandle = typeof(GameWerUI);
                GameWerUIInstance = (GameWerUI)UIDomain.CreateInstanceAndUnwrap(typeFromHandle.Assembly.FullName, typeFromHandle.FullName);
                ProxyForm = new GameWerProxy(GameWerUIInstance, UIDomain);
                GameWerUIInstance.InitUI();
			}
			catch (Exception ex)
			{
				OutputManager.Log(nameof(UIManager), $"Exception in CreateInstanceAndUnwrap: {ex}");
			}
			OutputManager.Log("UIManager", "InitThread::Shutdown");
			ApplicationManager.Shutdown();
		}

		internal static void Init()
		{
			OutputManager.Log("UI", "UIManager.Init()");
			try
			{
				PermissionSet permissionSet = CreateDomainPermission();
				AppDomainSetup info = CreateDomainSetup();
                UIDomain = AppDomain.CreateDomain("UIDomain", null, info);
                UIThread = new Thread(new ThreadStart(InitThread));
                UIThread.SetApartmentState(ApartmentState.STA);
                UIThread.Start();
			}
			catch (Exception ex)
			{
				OutputManager.Log(nameof(UIManager), $"Exception in UIManager.Init(): {ex}");
				ApplicationManager.Shutdown();
			}
		}

		internal static void Shutdown()
		{
			OutputManager.Log("UI", "UIManager.Shutdown()");
			try
			{
				UIThread?.Abort();
			}
			catch
			{
			}
			try
			{
				AppDomain.Unload(UIDomain);
			}
			catch
			{
			}
		}
	}
}
