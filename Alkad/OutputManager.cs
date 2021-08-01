using System;
using System.IO;

namespace GameWer
{
	public class OutputManager
	{
		private static OutputManager Instance { get; } = new OutputManager();

		internal static void Log(string outputOwner, string line)
		{
			lock (Instance)
			{
				try
				{
					Console.WriteLine(string.Format("\n[{0}] [{1}]: ", DateTime.Now, outputOwner) + line);
					File.AppendAllText("./output.log", string.Format("\n[{0}] [{1}]: ", DateTime.Now, outputOwner) + line);
				}
				catch (Exception)
				{
				}
			}
		}
	}
}
