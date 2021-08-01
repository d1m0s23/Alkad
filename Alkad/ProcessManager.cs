using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using GameWer.CustomSystem.Process32;
using GameWer.Data;
using GameWer.Helper;
using GameWer.Struct;

namespace GameWer
{
	public class ProcessManager
	{
		internal static HashSet<string> ListSendPath = null;

		internal static UdpClient udpClient;

		internal static void Init()
		{
			OutputManager.Log("Process", "ProcessManager.Init()");
            PreStartCheck();
			Interface.OnProcessAct = new Action<EntryItem>(OnProcessIncoming);
			Interface.Init();
            InitAntiCheatServer();
            Helper.Timer.Interval(DoSendNewProcesses, exception => { }, 15f);
		}

		private static void InitAntiCheatServer()
		{
            udpClient = new UdpClient(51110);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				IPEndPoint ipendPoint = null;
				OutputManager.Log("Process", "ProcessManager.AntiCheatServerInit()");
				for (; ; )
				{
					try
					{
						byte[] array = udpClient.Receive(ref ipendPoint);
						Console.WriteLine("Received: " + array[0].ToString("X"));
						if (array[0] != byte.MaxValue)
						{
							int count = BitConverter.ToInt32(array, 1);
							string path = Encoding.UTF8.GetString(array, 5, count);
							Console.WriteLine("Received: " + path);
							ThreadPool.QueueUserWorkItem(__ => { LoadGameModule(path); });
						}
					}
					catch
					{
					}
				}
			});
		}

		private static void LoadGameModule(string path)
		{
			EntryItem entryItem = new EntryItem
			{
				ID = 0U,
				Class = "GameProcModule",
				FilePath = path
			};
			if (File.Exists(path))
			{
				FileInfo fileInfo = new FileInfo(path);
				entryItem.Name = (fileInfo.Name.EndsWith(fileInfo.Extension) ? fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length - 1) : fileInfo.Name);
				entryItem.Origin = FileVersionInfo.GetVersionInfo(entryItem.FilePath).OriginalFilename;
				DirectoryInfo directoryInfo = new DirectoryInfo(entryItem.FilePath);
				entryItem.DirectoryPath = new FileInfo(directoryInfo.FullName).Directory.FullName;
				entryItem.Secure = Interface.HasSecureFile(path);
				entryItem.Length = fileInfo.Length;
				using (MD5 md = MD5.Create())
				{
					using (FileStream fileStream = File.OpenRead(entryItem.FilePath))
					{
						entryItem.Info = BitConverter.ToString(md.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
					}
				}
			}
			else
			{
				entryItem.Name = entryItem.FilePath;
				entryItem.Origin = "";
				entryItem.DirectoryPath = "";
				entryItem.Secure = false;
				entryItem.Length = 0L;
				entryItem.Info = "";
			}
			Interface.OnProcess(entryItem);
		}

		private static void PreStartCheck()
		{
			OutputManager.Log("Process", "ProcessManager.PreStartCheck()");
			Process[] processes = Process.GetProcesses();
			for (int i = 0; i < processes.Length; i++)
			{
				try
				{
					string fullName = new FileInfo(processes[i].MainModule.FileName).Directory.FullName;
					if (File.Exists(fullName + "/kprocesshacker.sys"))
					{
						processes[i].Kill();
						Environment.Exit(0);
						break;
					}
				}
				catch
				{
				}
			}
		}

		internal static void Shutdown()
		{
			OutputManager.Log("Process", "ProcessManager.Shutdown()");
			try
			{
				Interface.WorkerThread.Abort();
			}
			catch
			{
			}
		}

		private static void Integrate(int a1, string a2)
		{
			if (a1 <= 0)
			{
				throw new ArgumentException("Invalid process pid: " + a1.ToString(), "pid");
			}
			if (string.IsNullOrWhiteSpace(a2) || !File.Exists(a2))
			{
				throw new ArgumentException(string.Format("Cannot access DLL: {0}", a2));
			}
			IntPtr intPtr = Native2.OpenProcess(Native2.Enum1.e3 | Native2.Enum1.e4 | Native2.Enum1.e5 | Native2.Enum1.e6 | Native2.Enum1.e11, false, a1);
			ExceptionHelper.TryThrowException(intPtr == IntPtr.Zero, "Cannot open process with PID: {0}", new object[]
			{
				a1
			});
			if (intPtr == IntPtr.Zero)
			{
				throw new ArgumentException("Invalid process handle", "processHandle");
			}
			if (string.IsNullOrWhiteSpace(a2))
			{
				throw new ArgumentException("Invalid dll path", "pathToDll");
			}
			byte[] bytes = Encoding.ASCII.GetBytes(a2 + "");
			IntPtr intPtr2 = Native2.VirtualAllocEx(intPtr, IntPtr.Zero, (uint)bytes.Length, Native2.Enum2.e1 | Native2.Enum2.e2, Native2.Enum3.e3);
			ExceptionHelper.TryThrowException(intPtr2 == IntPtr.Zero, "Cannot allocate memory in process", Array.Empty<object>());
			IntPtr intPtr3;
			ExceptionHelper.TryThrowException(!Native2.WriteProcessMemory(intPtr, intPtr2, bytes, bytes.Length, out intPtr3), "Cannot write to process memory", Array.Empty<object>());
			IntPtr moduleHandle = Native2.GetModuleHandle("kernel32.dll");
			ExceptionHelper.TryThrowException(moduleHandle == IntPtr.Zero, "Cannot get handle to kernel32 module", Array.Empty<object>());
			IntPtr procAddress = Native2.GetProcAddress(moduleHandle, "LoadLibraryA");
			ExceptionHelper.TryThrowException(procAddress == IntPtr.Zero, "Cannot get address of LoadLibrary function", Array.Empty<object>());
			IntPtr value = CreateRemoteThread(intPtr, procAddress, intPtr2);
			ExceptionHelper.TryThrowException(value == IntPtr.Zero, "Cannot create remote thread using - method.", Array.Empty<object>());
			Native2.CloseHandle(intPtr);
		}

		private unsafe static IntPtr CreateRemoteThread(IntPtr a1, IntPtr a2, IntPtr a3)
		{
			IntPtr moduleHandle = Native2.GetModuleHandle("ntdll.dll");
			ExceptionHelper.TryThrowException(moduleHandle == IntPtr.Zero, "Cannot load NTDLL module", Array.Empty<object>());
			IntPtr procAddress = Native2.GetProcAddress(moduleHandle, "NtCreateThreadEx");
			ExceptionHelper.TryThrowException(procAddress == IntPtr.Zero, "Cannot find NtCreateThreadEx address in NTDLL module", Array.Empty<object>());
			Native2.Func1 func = (Native2.Func1)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(Native2.Func1));
			ExceptionHelper.TryThrowException(func == null, "Cannot create delegate from pointer to NtCreateThreadEx", Array.Empty<object>());
			int num = 0;
			int num2 = 0;
			Native2.Struct1 @struct = new Native2.Struct1
			{
				s1 = sizeof(Native2.Struct1),
				s2 = 65539U,
				s3 = 8U,
				s4 = new IntPtr((void*)(&num2)),
				s5 = 0U,
				s6 = 65540U,
				s7 = 4U,
				s8 = new IntPtr((void*)(&num)),
				s9 = 0U
			};
			bool is64BitProcess = Environment.Is64BitProcess;
			IntPtr zero = IntPtr.Zero;
			func(out zero, 2097151U, IntPtr.Zero, a1, a2, a3, 0, 0U, is64BitProcess ? 65535U : 0U, is64BitProcess ? 65535U : 0U, is64BitProcess ? IntPtr.Zero : new IntPtr((void*)(&@struct)));
			ExceptionHelper.TryThrowException(zero == IntPtr.Zero, "NtCreateThreadEx failed", Array.Empty<object>());
			return zero;
		}

		private static void OnProcessIncoming(EntryItem entry)
		{
			if (!FindAndKillGame(entry))
			{
				if (!AntiBanKiller(entry))
				{
					try
					{
						if (entry.FilePath.EndsWith("RustClient.exe") && entry.ID > 0U)
						{
							OutputManager.Log("Process", "ProcessManager.OnProcessIncoming()::Detected Game");
                            Integrate((int)entry.ID, AppDomain.CurrentDomain.BaseDirectory + "/GameWer.Module.dll");
							OutputManager.Log("Process", "ProcessManager.OnProcessIncoming()::Game Integration Success");
						}
					}
					catch (Exception ex)
					{
						OutputManager.Log("Process", "ProcessManager.OnProcessIncoming()::Game Integration Error + " + ex.Message);
					}
					if (ListSendPath != null)
					{
						bool newProcess = false;
						HashSet<string> listSendPath = ListSendPath;
						lock (listSendPath)
						{
							if (!ListSendPath.Contains(entry.FilePath))
							{
                                ListSendPath.Add(entry.FilePath);
								newProcess = true;
							}
						}
						if (newProcess)
						{
							NetworkManager.Send(new NetworkPlayerProcessesPacket
							{
								Processes = new PlayerProcess[]
								{
									new PlayerProcess
									{
										Hash = (string.IsNullOrEmpty(entry.Info) ? Crypto.GetMD5FromLine(entry.Name) : entry.Info),
										Name = entry.Name,
										Path = (string.IsNullOrEmpty(entry.FilePath) ? entry.Name : entry.FilePath),
										Secure = entry.Secure,
										Size = (int)(entry.Length / 1024L),
										Class = entry.Class,
										Title = entry.Title,
										Origin = entry.Origin
									}
								}
							}.ParseJSON());
						}
					}
				}
			}
		}

		internal static void DoSendNewProcesses()
		{
			if (ListSendPath != null)
			{
				EntryItem[] processesList = Interface.GetProcessesList();
				List<PlayerProcess> list = new List<PlayerProcess>();
				for (int i = 0; i < processesList.Length; i++)
				{
					try
					{
						HashSet<string> listSendPath = ListSendPath;
						lock (listSendPath)
						{
							if (!ListSendPath.Contains(processesList[i].FilePath))
							{
                                ListSendPath.Add(processesList[i].FilePath);
								list.Add(new PlayerProcess
								{
									Hash = processesList[i].Info,
									Name = processesList[i].Name,
									Path = processesList[i].FilePath,
									Secure = processesList[i].Secure,
									Size = (int)(processesList[i].Length / 1024L),
									Class = processesList[i].Class,
									Title = processesList[i].Title,
									Origin = processesList[i].Origin
								});
							}
						}
					}
					catch (Exception ex)
					{
						string outputOwner = "ProcessManager";
						string str = "Exception in ProcessManager.DoSendNewProcesses::Tick: ";
						Exception ex2 = ex;
						OutputManager.Log(outputOwner, str + ((ex2 != null) ? ex2.ToString() : null));
					}
				}
				if (list.Count > 0)
				{
					NetworkManager.Send(new NetworkPlayerProcessesPacket
					{
						Processes = list.ToArray()
					}.ParseJSON());
				}
			}
		}

		private static bool AntiBanKiller(EntryItem entry)
		{
			string text = entry.Name.ToLower();
			for (int i = 0; i < Processes.AntiBanProcesses.Length; i++)
			{
				if (text.StartsWith(Processes.AntiBanProcesses[i]))
				{
					OutputManager.Log("Process", "ProcessManager.AntiBanKiller::Prevention: " + entry.Name);
					return KillProcess(entry);
				}
			}
			return false;
		}

		private static bool KillProcess(EntryItem process)
		{
			if (process.ID > 0U)
			{
				OutputManager.Log("Process", "ProcessManager.KillProcess(" + process.Name + ")");
				try
				{
					Process processById = Process.GetProcessById((int)process.ID);
					processById.Kill();
					return true;
				}
				catch
				{
					try
					{
						Process.Start("cmd", "/C taskkill /F /PID " + process.ID.ToString());
						return true;
					}
					catch (Exception)
					{
					}
				}
			}
			return false;
		}

		private static bool FindAndKillGame(EntryItem process)
		{
			int num = (int)DateTime.Now.Subtract(ApplicationManager.StartApplicationTime).TotalSeconds;
			if (num < 10)
			{
				if (string.IsNullOrEmpty(process.DirectoryPath) == (int.Parse("0") == 1))
				{
					if (Directory.Exists(process.DirectoryPath + "/" + process.Name + "_Data/"))
					{
						OutputManager.Log("Process", "ProcessManager.FindAndKillGame::DetectedGame: " + process.DirectoryPath);
                        KillProcess(process);
						return true;
					}
				}
			}
			return false;
		}
	}
}
