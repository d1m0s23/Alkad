using System;
using System.Text;

namespace GameWer.Helper
{
	public static class ChallengeExt
	{
		public static byte[] Challenge(this Encoding encoding, string text)
		{
			return encoding.GetBytes(text + "Chalenge");
		}
	}
}
