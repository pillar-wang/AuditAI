using System;

namespace Leqisoft.UI.Controls;

public static class DateTimeEx
{
	public static DateTime CopyToSpecificYear(this DateTime dateTime, int year)
	{
		int num = DateTime.DaysInMonth(year, dateTime.Month);
		int day = ((dateTime.Day > num) ? num : dateTime.Day);
		return new DateTime(year, dateTime.Month, day);
	}
}
