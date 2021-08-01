using System;

namespace GameWer.Helper
{
	public class Date
	{
		internal static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			result = result.AddSeconds(unixTimeStamp).ToLocalTime();
			return result;
		}
	}
}
