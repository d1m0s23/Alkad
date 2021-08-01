using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using GameWer.Helper;

namespace GameWer.CustomSystem.Process32
{
	public class Interface
	{

		internal static Thread WorkerThread;

		private static bool HasInitialized = false;

		private static readonly HashSet<uint> ListViewPID = new HashSet<uint>();

		private static readonly Dictionary<string, EntryItem> InfoPath = new Dictionary<string, EntryItem>();

		private static readonly List<EntryItem> ListProcesses = new List<EntryItem>();

		private static readonly uint DWSize = (uint)Marshal.SizeOf(typeof(Native.ProcessEntry32));

		private static bool foundIMGUI = false;

		public static Action<EntryItem> OnProcessAct;

		public static void Init()
		{
			bool hasInitialized = HasInitialized;
			if (!hasInitialized)
			{
                HasInitialized = true;
                WorkerThread = new Thread(new ThreadStart(WorkerUpdater));
                WorkerThread.IsBackground = true;
                WorkerThread.Priority = ThreadPriority.Highest;
                WorkerThread.Start();
			}
		}

		private static void ProcessAnalytics(EntryItem entry)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					if (new FileInfo(entry.FilePath).Length >= 10485760L)
						return;
					var str = Encoding.UTF8.GetString(File.ReadAllBytes(entry.FilePath));
					if (str.Contains("dnSpy.Contracts.DnSpy"))
					{
						Process.GetProcessById((int)entry.ID)?.Kill();
						OutputManager.Log("Process32", "DnSpy Detected");
						ApplicationManager.Shutdown();
					}
					else if (str.Contains("Mega_Dumper"))
					{
						Process.GetProcessById((int)entry.ID)?.Kill();
						OutputManager.Log("Process32", "Dumber Detected");
						ApplicationManager.Shutdown();
					}
					else if (str.Contains("PhShellProcessHacker"))
					{
						Process.GetProcessById((int)entry.ID)?.Kill();
						OutputManager.Log("Process32", "ProcessHacker Detected");
						ApplicationManager.Shutdown();
					}
					else if (str.Contains("x64dbg"))
					{
						Process.GetProcessById((int)entry.ID)?.Kill();
						OutputManager.Log("Process32", "x64dbg Detected");
						ApplicationManager.Shutdown();
					}
					else if (str.Contains("IDA Pro"))
					{
						Process.GetProcessById((int)entry.ID)?.Kill();
						OutputManager.Log("Process32", "IDA Detected");
						ApplicationManager.Shutdown();
					}
					else if (str.Contains("https://wiki.cheatengine.org/index.php"))
					{
						Process.GetProcessById((int)entry.ID)?.Kill();
						OutputManager.Log("Process32", "CheatEngine Detected");
						ApplicationManager.Shutdown();
					}
					else
					{
						if (!str.Contains("WINDBG_NO_WORKSPACE_WINDOWS"))
							return;
						Process.GetProcessById((int)entry.ID)?.Kill();
						OutputManager.Log("Process32", "WinDBG Detected");
						ApplicationManager.Shutdown();
					}
				}
				catch
				{
					try
					{
						OutputManager.Log("Process32", "Catch Detected");
						ApplicationManager.Shutdown();
					}
					catch
					{
					}
				}
			});
		}

		public static EntryItem[] GetProcessesList()
		{
			List<EntryItem> listProcesses = ListProcesses;
			EntryItem[] array;
			lock (listProcesses)
			{
				array = new EntryItem[ListProcesses.Count];
                ListProcesses.CopyTo(array);
			}
			return array;
		}

		private static void WorkerUpdater()
		{
			while (ApplicationManager.IsWork)
			{
				try
				{
                    WorkerTick();
				}
				catch
				{
				}
				Thread.Sleep(int.Parse("300"));
			}
		}

		private static void WorkerTick()
		{
			Native.ProcessEntry32 processEntry = default(Native.ProcessEntry32);
			IntPtr intPtr = Native.CreateToolhelp32Snapshot(Native.TH32CS_SNAPPROCESS, uint.Parse("0"));
			bool flag = intPtr == Native.INVALID_HANDLE_VALUE;
			if (!flag)
			{
				processEntry.dwSize = DWSize;
				bool flag2 = !Native.Process32First(intPtr, ref processEntry);
				if (!flag2)
				{
					do
					{
						bool flag3 = !ListViewPID.Contains(processEntry.th32ProcessID);
						if (flag3)
						{
                            ListViewPID.Add(processEntry.th32ProcessID);
                            IncomingProcess(processEntry);
						}
					}
					while (Native.Process32Next(intPtr, ref processEntry) && ApplicationManager.IsWork);
					Native.CloseHandle(intPtr);
				}
			}
		}

		public static void OnProcess(EntryItem entry)
		{
			bool flag = entry.Name.EndsWith(".exe") || entry.Name.EndsWith(".dll");
			if (flag)
			{
				entry.Name = entry.Name.Substring(0, entry.Name.Length - 4);
			}
			List<EntryItem> listProcesses = ListProcesses;
			lock (listProcesses)
			{
                ListProcesses.Add(entry);
			}
			try
			{
				Action<EntryItem> onProcessAct = OnProcessAct;
				if (onProcessAct != null)
				{
					onProcessAct(entry);
				}
			}
			catch (Exception ex)
			{
				string outputOwner = "CustomSystem.Process32.Interface";
				string str = "Exception in OnProcess action: ";
				Exception ex2 = ex;
				OutputManager.Log(outputOwner, str + ((ex2 != null) ? ex2.ToString() : null));
			}
		}

		public static bool HasSecureFile(string path)
		{
			try
			{
				X509Certificate certificate = X509Certificate.CreateFromSignedFile(path);
				X509Certificate2 x509Certificate = new X509Certificate2(certificate);
				return true;
			}
			catch
			{
			}
			return false;
		}

		private static void DetailsScan(EntryItem entry)
        {
			try
			{
				bool flag = File.Exists(entry.FilePath);
				if (flag)
				{
					entry.Origin = FileVersionInfo.GetVersionInfo(entry.FilePath).OriginalFilename;
					DirectoryInfo directoryInfo = new DirectoryInfo(entry.FilePath);
					entry.DirectoryPath = new FileInfo(directoryInfo.FullName).Directory.FullName;
					bool flag2 = Directory.Exists(entry.DirectoryPath + "/" + entry.Name + "_Data/") && entry.FilePath.Contains("RustClient.exe");
					if (flag2)
					{
						List<FileInfo> list = new List<FileInfo>();
						Helper.Timer.Interval(() => {
							if (Directory.Exists(entry.DirectoryPath))
							{
								FileInfo[] files = new DirectoryInfo(entry.DirectoryPath).GetFiles();
								int i = 0;
								while (i < files.Length)
								{
									if (!files[i].FullName.EndsWith("imgui.ini"))
									{
										goto IL_7C;
									}
									if (foundIMGUI)
									{
										goto IL_7C;
									}
									foundIMGUI = true;
									try
									{
										files[i].Delete();
									}
									catch
									{
									}
								IL_168:
									i++;
									continue;
								IL_7C:
									bool flag4 = !InfoPath.ContainsKey(files[i].FullName) && !files[i].FullName.Contains("RustClient.exe");
									if (flag4)
									{
										EntryItem entryItem = new EntryItem();
										entryItem.Name = (files[i].Name.EndsWith(files[i].Extension) ? files[i].Name.Substring(0, files[i].Name.Length - files[i].Extension.Length - 1) : files[i].Name);
										entryItem.FilePath = files[i].FullName;
										entryItem.DirectoryPath = files[i].Directory.FullName;
										entryItem.ID = 0U;
										entryItem.Class = "GameDirFile";
										InfoPath.Add(files[i].FullName, entryItem);
										DetailsScan(entryItem);
									}
									goto IL_168;
								}
							}
						}, ex => { }, 15f);
						if (Directory.Exists(entry.DirectoryPath + "/" + entry.Name + "_Data/Managed/"))
						{
							FileInfo[] files = new DirectoryInfo(entry.DirectoryPath + "/" + entry.Name + "_Data/Managed/").GetFiles("*.dll");
							list.AddRange(files);
						}
						if (Directory.Exists(entry.DirectoryPath + "/" + entry.Name + "_Data/Plugins/"))
						{
							FileInfo[] files2 = new DirectoryInfo(entry.DirectoryPath + "/" + entry.Name + "_Data/Plugins/").GetFiles("*.dll");
							for (int i = 0; i < files2.Length; i++)
							{
								if (!HasSecureFile(files2[i].FullName))
								{
									OutputManager.Log("Process32", "Not Correct Game Files");
									ApplicationManager.Shutdown();
									return;
								}
							}
							list.AddRange(files2);
						}
						if (Directory.Exists(entry.DirectoryPath + "/" + entry.Name + "_Data/Plugins/x86_64/"))
						{
							FileInfo[] files3 = new DirectoryInfo(entry.DirectoryPath + "/" + entry.Name + "_Data/Plugins/x86_64/").GetFiles("*.dll");
							for (int j = 0; j < files3.Length; j++)
							{
								if (!HasSecureFile(files3[j].FullName))
								{
									OutputManager.Log("Process32", "Not Correct Game Files");
									ApplicationManager.Shutdown();
									return;
								}
							}
							list.AddRange(files3);
						}
						for (int k = 0; k < list.Count; k++)
						{
							FileInfo fileInfo = list[k];
							Dictionary<string, EntryItem> infoPath = InfoPath;
							lock (infoPath)
							{
								if (!InfoPath.ContainsKey(fileInfo.FullName))
								{
									EntryItem entryItem = new EntryItem();
									entryItem.Name = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length - 1);
									entryItem.FilePath = fileInfo.FullName;
									entryItem.DirectoryPath = fileInfo.Directory.FullName;
									entryItem.ID = 0U;
									entryItem.Class = "GameDllFile";
									InfoPath.Add(fileInfo.FullName, entryItem);
									DetailsScan(entryItem);
								}
							}
						}
					}
					try
					{
						entry.Length = new FileInfo(entry.FilePath).Length;
					}
					catch
					{
					}
					entry.Secure = HasSecureFile(entry.FilePath);
					using (MD5 md = MD5.Create())
					{
						using (FileStream fileStream = File.OpenRead(entry.FilePath))
						{
							entry.Info = BitConverter.ToString(md.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
						}
					}
				}
				else
				{
					entry.Secure = false;
					entry.Info = Crypto.GetMD5FromLine(entry.Name);
					entry.Length = 0L;
				}
			}
			catch (Exception ex)
			{
				string outputOwner = "CustomSystem.Process32.Interface";
				string str = "Exception in DetailsScan: ";
				Exception ex2 = ex;
				OutputManager.Log(outputOwner, str + ((ex2 != null) ? ex2.ToString() : null));
			}
			OnProcess(entry);
		}

		private static string GetPathFromNative(Native.ProcessEntry32 pe32)
		{
			string text = "";
			try
			{
				IntPtr intPtr = Native.OpenProcess(Native.PROCESS_QUERY_INFORMATION, 0, pe32.th32ProcessID);
				bool flag = intPtr != IntPtr.Zero && intPtr != Native.INVALID_HANDLE_VALUE;
				if (flag)
				{
					int num = int.Parse("1024");
					try
					{
						try
						{
							StringBuilder stringBuilder = new StringBuilder(num);
							Native.GetModuleFileNameEx(intPtr, IntPtr.Zero, stringBuilder, num);
							bool flag2 = stringBuilder.Length == int.Parse("0");
							if (flag2)
							{
								Native.GetProcessImageFileName(intPtr, stringBuilder, num);
							}
							text = stringBuilder.ToString();
							bool flag3 = string.IsNullOrEmpty(text) || !File.Exists(text);
							if (flag3)
							{
								throw new Exception("FilePath not found");
							}
						}
						catch
						{
							try
							{
								StringBuilder stringBuilder = new StringBuilder(num);
								Native.GetProcessImageFileName(intPtr, stringBuilder, num);
								text = stringBuilder.ToString();
								bool flag4 = string.IsNullOrEmpty(text) || !File.Exists(text);
								if (flag4)
								{
									throw new Exception("FilePath not found");
								}
							}
							catch
							{
							}
						}
					}
					catch (Exception)
                    {
					}
					Native.CloseHandle(intPtr);
				}
			}
			catch
			{
			}
			return text;
		}

		private static void IncomingProcess(Native.ProcessEntry32 pe32)
		{
			bool flag = (ulong)pe32.th32ProcessID < (ulong)((long)int.Parse("10"));
			if (!flag)
			{
				try
				{
					EntryItem entryItem = null;
					try
					{
						Process processById = Process.GetProcessById((int)pe32.th32ProcessID);
						entryItem = new EntryItem
						{
							ID = pe32.th32ProcessID,
							Name = (processById.ProcessName.EndsWith(".exe") ? processById.ProcessName.Substring(0, processById.ProcessName.Length - 4) : processById.ProcessName),
							FilePath = pe32.szExeFile
						};
						StringBuilder stringBuilder = new StringBuilder(512);
						Native.GetClassName(processById.MainWindowHandle, stringBuilder, 512);
						try
						{
							bool flag2 = !File.Exists(entryItem.FilePath);
							if (flag2)
							{
								string fileName = processById.MainModule.FileName;
								entryItem.FilePath = fileName;
							}
						}
						catch
						{
							try
							{
								entryItem.FilePath = processById.StartInfo.FileName;
							}
							catch
							{
							}
						}
						try
						{
							entryItem.Class = stringBuilder.ToString();
							entryItem.Title = processById.MainWindowTitle;
						}
						catch
						{
						}
					}
					catch (Exception)
					{
						string pathFromNative = GetPathFromNative(pe32);
						entryItem = new EntryItem
						{
							ID = pe32.th32ProcessID,
							Name = pe32.szExeFile,
							FilePath = pathFromNative
						};
					}
					bool flag3 = entryItem.Name.EndsWith(".exe");
					if (flag3)
					{
						entryItem.Name = entryItem.Name.Substring(0, entryItem.Name.Length - 4);
					}
					bool flag4 = string.IsNullOrEmpty(entryItem.FilePath) || !File.Exists(entryItem.FilePath);
					if (flag4)
					{
						entryItem.FilePath = entryItem.Name;
						entryItem.DirectoryPath = "";
						entryItem.Secure = false;
						entryItem.Info = Crypto.GetMD5FromLine(entryItem.Name);
						entryItem.Length = 0L;
                        OnProcess(entryItem);
					}
					else
					{
						string text = entryItem.FilePath.ToLower();
						if (!text.Contains("windows") && !text.Contains("program files"))
						{
                            ProcessAnalytics(entryItem);
						}
						Dictionary<string, EntryItem> infoPath = InfoPath;
						lock (infoPath)
						{
							bool flag7 = !InfoPath.ContainsKey(entryItem.FilePath);
							if (flag7)
							{
                                InfoPath[entryItem.FilePath] = entryItem;
                                DetailsScan(entryItem);
								return;
							}
							long num = 0L;
							try
							{
								num = new FileInfo(entryItem.FilePath).Length;
							}
							catch
							{
							}
							bool flag8 = num == InfoPath[entryItem.FilePath].Length;
							if (!flag8)
							{
                                InfoPath[entryItem.FilePath] = entryItem;
                                DetailsScan(entryItem);
								return;
							}
							entryItem.Length = num;
							entryItem.Info = InfoPath[entryItem.FilePath].Info;
							entryItem.Secure = InfoPath[entryItem.FilePath].Secure;
						}
                        OnProcess(entryItem);
					}
				}
				catch (Exception ex2)
				{
					string outputOwner = "CustomSystem.Process32.Interface";
					string str = "Exception in IncomingProcess: ";
					Exception ex3 = ex2;
					OutputManager.Log(outputOwner, str + ((ex3 != null) ? ex3.ToString() : null));
				}
			}
		}
	}
}
