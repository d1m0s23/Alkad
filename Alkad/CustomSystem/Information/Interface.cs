using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using GameWer.Helper;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;

namespace GameWer.CustomSystem.Information
{
	public class Interface
	{
		[CompilerGenerated]
		private sealed class StringCmp
		{
			public string val;

			internal bool IsEquals(string str)
			{
				return str == this.val;
			}

			internal bool IsEquals2(string str)
			{
				return str == this.val;
			}
		}

		[CompilerGenerated]
		[Serializable]
		private sealed class Network
		{
			public static readonly Network Instance = new Network();

			internal bool ContainsLocal(string str)
			{
				return str.Contains("192.168.1.1");
			}

			internal bool ContainsLocal2(string str)
			{
				return str.Contains("192.168.0.1");
			}

			internal bool IsUP(NetworkInterface @interface)
			{
				return @interface.OperationalStatus == OperationalStatus.Up && @interface.NetworkInterfaceType != NetworkInterfaceType.Loopback;
			}

			internal string GetPhysicalAddress(NetworkInterface @interface)
			{
				return @interface.GetPhysicalAddress().ToString();
			}
		}

		internal static string GetHWID = string.Empty;

		internal static string apphash = "85d2b63a384q948a59e3e11285348534";

		internal static string Model = string.Empty;

		internal static string Manufacturer = string.Empty;

		internal static string ProductName = string.Empty;

		internal static string RegisteredOrganization = string.Empty;

		internal static string RegisteredOwner = string.Empty;

		internal static string SystemRoot = string.Empty;

		internal static string MachineName = string.Empty;

		internal static string UserName = string.Empty;

		internal static bool IsBit64OS = false;

		internal static string MemorySize = string.Empty;

		internal static string ProcessorName = string.Empty;

		internal static string ProcessorID = string.Empty;

		internal static string VideocardName = string.Empty;

		internal static string VideocardID = string.Empty;

		internal static string DriversName = string.Empty;

		internal static string DriversSize = string.Empty;

		internal static List<string> GetHWIDList { get; } = new List<string>();

		internal static void Init()
		{
            InitHWIDList();
            InitHWID();
            InitOther();
		}

		private static void InitOther()
		{
            DriversName = string.Join(",", Environment.GetLogicalDrives());
			try
			{
				uint num = uint.Parse("0");
				DriveInfo[] drives = DriveInfo.GetDrives();
				for (int i = 0; i < drives.Length; i++)
				{
					try
					{
						num += (uint)(drives[i].TotalSize / (long)int.Parse("1024") / (long)int.Parse("1024"));
					}
					catch
					{
					}
				}
                DriversSize = num.ToString();
			}
			catch (Exception)
			{
			}
			try
			{
				foreach (ManagementBaseObject managementBaseObject in new ManagementClass("Win32_Processor").GetInstances())
				{
					foreach (PropertyData propertyData in managementBaseObject.Properties)
					{
						string name = propertyData.Name;
						if (name != null)
						{
							if (!(name == "Name"))
							{
								if (name == "ProcessorId" || name == "ProcessorType" || name == "ProcessorRevision")
								{
                                    ProcessorID += propertyData.Value.ToString();
								}
							}
							else
							{
                                ProcessorName = propertyData.Value.ToString();
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
			try
			{
				foreach (ManagementBaseObject managementBaseObject2 in new ManagementClass("Win32_VideoController").GetInstances())
				{
					try
					{
						foreach (PropertyData propertyData2 in managementBaseObject2.Properties)
						{
							try
							{
								string name2 = propertyData2.Name;
								if (name2 != null)
								{
									if (!(name2 == "Name"))
									{
										if (name2 == "PNPDeviceID")
										{
                                            VideocardID = propertyData2.Value.ToString();
										}
									}
									else
									{
                                        VideocardName = propertyData2.Value.ToString();
									}
								}
							}
							catch
							{
							}
						}
					}
					catch
					{
					}
				}
			}
			catch (Exception)
			{
				Console.WriteLine("GetGPU Crash");
			}
			try
			{
                Model = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OEMInformation\\", "Model", "null").ToString();
			}
			catch
			{
			}
			try
			{
                Manufacturer = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OEMInformation\\", "Manufacturer", "null").ToString();
			}
			catch
			{
			}
			try
			{
                ProductName = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", "null").ToString();
			}
			catch
			{
			}
			try
			{
                RegisteredOrganization = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "RegisteredOrganization", "null").ToString();
			}
			catch
			{
			}
			try
			{
                RegisteredOwner = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "RegisteredOwner", "null").ToString();
			}
			catch
			{
			}
			try
			{
                SystemRoot = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "SystemRoot", "null").ToString();
			}
			catch
			{
			}
			try
			{
                MachineName = Environment.MachineName;
			}
			catch
			{
			}
			try
			{
                UserName = Environment.UserName;
			}
			catch
			{
			}
			try
			{
                IsBit64OS = Environment.Is64BitOperatingSystem;
			}
			catch
			{
			}
			try
			{
                MemorySize = ((int)(new ComputerInfo().TotalPhysicalMemory / ulong.Parse("1024") / ulong.Parse("1024"))).ToString();
			}
			catch
			{
			}
		}

		private static void InitHWID()
		{
			if (GetHWIDList.Count > int.Parse("0"))
			{
                GetHWID = Crypto.GetMD5FromLine(GetHWIDList[int.Parse("0")] + ((GetHWIDList.Count > int.Parse("1")) ? GetHWIDList[int.Parse("1")] : ""));
			}
		}

		private static void InitHWIDList()
		{
			string[] source = new string[]
			{
				"ff-ff-ff-ff-ff-ff",
				"7a-79-19-00-00-01",
				"02-00-00-00-51-00"
			};
			try
			{
                StringCmp stringCmp = new StringCmp();
				StreamReader streamReader = ExecuteCommandLine("arp", "-a");
				for (int i = 0; i < int.Parse("3"); i++)
				{
					streamReader.ReadLine();
				}
				List<string> list = new List<string>();
				while (!streamReader.EndOfStream)
				{
					string item = streamReader.ReadLine();
					list.Add(item);
				}
				stringCmp.val = list.Find(new Predicate<string>(Network.Instance.ContainsLocal));
				if (string.IsNullOrEmpty(stringCmp.val) || source.Any(new Func<string, bool>(stringCmp.IsEquals)))
				{
					stringCmp.val = list.Find(new Predicate<string>(Network.Instance.ContainsLocal2));
				}
				if (string.IsNullOrEmpty(stringCmp.val) || source.Any(new Func<string, bool>(stringCmp.IsEquals2)))
				{
					for (int j = 0; j < list.Count; j++)
					{
						string text = list[j];
						if (text != null)
						{
							text = text.Trim();
							while (text.Contains("  "))
							{
								text = text.Replace("  ", " ");
							}
							string[] array = text.Trim().Split(new char[]
							{
								' '
							});
							if (array.Length == int.Parse("3"))
							{
								string text2 = array[int.Parse("0")];
								string text3 = array[int.Parse("1")];
								if (!source.Contains(text3))
								{
									string[] array2 = text2.Split(new char[]
									{
										'.'
									}, StringSplitOptions.RemoveEmptyEntries);
									if (array2.Length != int.Parse("4") || ((!(array2[1] == "255") || !(array2[2] == "255")) && (!(array2[1] == "0") || !(array2[2] == "0"))))
									{
                                        GetHWIDList.Add(Crypto.GetMD5FromLine(text3));
									}
								}
							}
						}
					}
				}
				else
				{
                    GetHWIDList.Add(stringCmp.val);
				}
			}
			catch
			{
			}
			string text4 = NetworkInterface.GetAllNetworkInterfaces().Where(new Func<NetworkInterface, bool>(Network.Instance.IsUP)).Select(new Func<NetworkInterface, string>(Network.Instance.GetPhysicalAddress)).FirstOrDefault();
			if (!source.Contains(text4))
			{
                GetHWIDList.Add(Crypto.GetMD5FromLine(text4));
			}
		}

		private static StreamReader ExecuteCommandLine(string file, string arguments = "")
		{
			Process process = Process.Start(new ProcessStartInfo
			{
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				FileName = file,
				Arguments = arguments
			});
			if (process == null)
			{
				return null;
			}
			return process.StandardOutput;
		}
	}
}
