using System;
using System.Security.Cryptography;
using System.Text;

namespace GameWer.Helper
{
	public class Crypto
	{
		public static string GetMD5FromLine(string input)
		{
			string result;
			using (MD5 md = MD5.Create())
			{
				byte[] bytes = Encoding.ASCII.GetBytes(input);
				byte[] array = md.ComputeHash(bytes);
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < array.Length; i++)
				{
					stringBuilder.Append(array[i].ToString("X2"));
				}
				result = stringBuilder.ToString().ToLower();
			}
			return result;
		}
	}
}
