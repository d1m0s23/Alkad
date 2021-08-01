using System;
using System.Runtime.InteropServices;

namespace GameWer.Helper
{
	internal static class ExceptionHelper
	{
		public static void TryThrowException(bool shouldThrow, string text, params object[] args)
		{
			bool flag = !shouldThrow;
			if (flag)
			{
				return;
			}
			string arg = string.Format(text, args);
			string arg2 = string.Format("LastWinError: {0}", Marshal.GetLastWin32Error());
			string message = string.Format("{0} ({1})", arg, arg2);
			throw new Exception(message);
		}
	}
}
