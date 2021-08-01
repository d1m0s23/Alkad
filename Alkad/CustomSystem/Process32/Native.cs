using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GameWer.CustomSystem.Process32
{
	internal class Native
	{
		public struct ProcessEntry32
		{
			public uint dwSize;

			public uint cntUsage;

			public uint th32ProcessID;

			public IntPtr th32DefaultHeapID;

			public uint th32ModuleID;

			public uint cntThreads;

			public uint th32ParentProcessID;

			public int pcPriClassBase;

			public uint dwFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szExeFile;
		}

		private const string KERNEL32 = "kernel32.dll";

		private const string PSAPI = "psapi.dll";

		public static uint TH32CS_SNAPHEAPLIST = 1U;

		public static uint TH32CS_SNAPPROCESS = 2U;

		public static uint TH32CS_SNAPTHREAD = 4U;

		public static uint TH32CS_SNAPMODULE = 8U;

		public static uint TH32CS_SNAPMODULE32 = 16U;

		public static uint TH32CS_SNAPALL = 15U;

		public static uint TH32CS_INHERIT = 2147483648U;

		public static uint PROCESS_ALL_ACCESS = 2035711U;

		public static uint PROCESS_TERMINATE = 1U;

		public static uint PROCESS_CREATE_THREAD = 2U;

		public static uint PROCESS_VM_OPERATION = 8U;

		public static uint PROCESS_VM_READ = 16U;

		public static uint PROCESS_VM_WRITE = 32U;

		public static uint PROCESS_DUP_HANDLE = 64U;

		public static uint PROCESS_CREATE_PROCESS = 128U;

		public static uint PROCESS_SET_QUOTA = 256U;

		public static uint PROCESS_SET_INFORMATION = 512U;

		public static uint PROCESS_QUERY_INFORMATION = 1024U;

		public static uint PROCESS_SUSPEND_RESUME = 2048U;

		public static uint PROCESS_QUERY_LIMITED_INFORMATION = 4096U;

		public static uint SYNCHRONIZE = 1048576U;

		public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool Process32First(IntPtr hSnapshot, ref ProcessEntry32 lppe);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool Process32Next(IntPtr hSnapshot, ref ProcessEntry32 lppe);

		[DllImport("kernel32.dll")]
		public static extern int CloseHandle(IntPtr handle);

		[DllImport("psapi.dll")]
		public static extern uint GetProcessImageFileName(IntPtr hProcess, [Out] StringBuilder lpImageFileName, [MarshalAs(UnmanagedType.U4)] [In] int nSize);

		[DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [MarshalAs(UnmanagedType.U4)] [In] int nSize);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, [MarshalAs(UnmanagedType.LPTStr)] [Out] StringBuilder lpExeName, ref uint lpdwSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);
	}
}
