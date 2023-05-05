﻿using System;

namespace PGMS.CQSLight.Extensions
{
	public static class EpochExtensions
	{

		public static DateTime FromEpochAbsolute(this long unixTime)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddSeconds(unixTime);
		}

		public static DateTime FromEpoch(this long unixTime)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddSeconds(unixTime).ToLocalTime();
		}

		public static DateTime FromEpochInMilliseconds(this double unixTime)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddMilliseconds(unixTime).ToLocalTime();
		}

		public static DateTime? FromEpoch(this long? unixTime)
		{
			return unixTime?.FromEpoch();
		}

		public static DateTime? FromEpochInMilliseconds(this long? unixTime)
		{
			return unixTime?.FromEpoch();
		}

		public static long ToEpochAbsolute(this DateTime date)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64((date - epoch).TotalSeconds);
		}

		public static long ToEpoch(this DateTime date)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds);
		}

		public static double ToEpochInMilliseconds(this DateTime date)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return (date.ToUniversalTime() - epoch).TotalMilliseconds;
		}

		public static long? ToEpoch(this DateTime? date)
		{
			return date?.ToEpoch();
		}
	}
}