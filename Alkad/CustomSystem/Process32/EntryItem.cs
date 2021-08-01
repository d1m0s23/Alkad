using System;

namespace GameWer.CustomSystem.Process32
{
	public class EntryItem
	{
		public uint ID;

		public bool Secure;

		public long Length;

		public string Name { get; set; } = "";

		public string FilePath { get; set; } = "";

		public string DirectoryPath { get; set; } = "";

		public string Info { get; set; } = "";

		public string Class { get; set; } = "";

		public string Title { get; set; } = "";

		public string Origin { get; set; } = "";
	}
}
