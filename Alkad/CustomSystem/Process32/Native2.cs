using System;
using System.Runtime.InteropServices;

namespace GameWer.CustomSystem.Process32
{
	internal static class Native2
	{
		[Flags]
		public enum Enum1 : uint
		{
			e1 = 2035711U,
			e2 = 1U,
			e3 = 2U,
			e4 = 8U,
			e5 = 16U,
			e6 = 32U,
			e7 = 64U,
			e8 = 128U,
			e9 = 256U,
			e10 = 512U,
			e11 = 1024U,
			e12 = 4096U,
			e13 = 1048576U
		}

		[Flags]
		public enum Enum2
		{
			e1 = 4096,
			e2 = 8192,
			e3 = 16384,
			e4 = 32768,
			e5 = 524288,
			e6 = 4194304,
			e7 = 1048576,
			e8 = 2097152,
			e9 = 536870912
		}

		[Flags]
		public enum Enum3
		{
			e1 = 16,
			e2 = 32,
			e3 = 64,
			e4 = 128,
			e5 = 1,
			e6 = 2,
			e7 = 4,
			e8 = 8,
			e9 = 256,
			e10 = 512,
			e11 = 1024
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct Struct1
		{
			public int s1;

			public uint s2;

			public uint s3;

			public IntPtr s4;

			public uint s5;

			public uint s6;

			public uint s7;

			public IntPtr s8;

			public uint s9;
		}

		public delegate int Func1(out IntPtr threadHandle, uint desiredAccess, IntPtr objectAttributes, IntPtr processHandle, IntPtr lpStartAddress, IntPtr lpParameter, int createSuspended, uint stackZeroBits, uint sizeOfStackCommit, uint sizeOfStackReserve, IntPtr lpBytesBuffer);

		public const string kernel32 = "kernel32.dll";

		public const string ntdll = "ntdll.dll";

		public const string loadlib = "LoadLibraryA";

		public const string ntcte = "NtCreateThreadEx";

		public const uint AA = 4294967295U;

		public const uint AB = 128U;

		public const uint AC = 0U;

		public const uint AD = 258U;

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(Enum1 processAccess, bool bInheritHandle, int processId);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, Enum2 flAllocationType, Enum3 flProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32.dll")]
		public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
	}
}
