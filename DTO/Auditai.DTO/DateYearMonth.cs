using System;

namespace Auditai.DTO;

public struct DateYearMonth
{
	public DateTime Date;

	public string ToStringFormat;

	public bool IsZero
	{
		get
		{
			if (Date.Year == 1)
			{
				return Date.Month == 1;
			}
			return false;
		}
	}

	public DateYearMonth()
	{
		ToStringFormat = null;
		Date = DateTime.MinValue;
	}

	public DateYearMonth(DateTime date)
	{
		ToStringFormat = null;
		Date = date;
	}

	public DateYearMonth AddMonths(int months)
	{
		return new DateYearMonth(Date.AddMonths(months));
	}

	public DateYearMonth AddYears(int years)
	{
		return new DateYearMonth(Date.AddYears(years));
	}

	public override string ToString()
	{
		if (string.IsNullOrWhiteSpace(ToStringFormat))
		{
			return Date.ToString("yyyy年M月");
		}
		return Date.ToString(ToStringFormat);
	}

	public override bool Equals(object obj)
	{
		if (obj is DateYearMonth dateYearMonth)
		{
			if (Date.Year == dateYearMonth.Date.Year)
			{
				return Date.Month == dateYearMonth.Date.Month;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Date.Year.GetHashCode() & Date.Month.GetHashCode();
	}

	public bool IsYearMonthEqual(DateYearMonth ym)
	{
		if (Date.Year == ym.Date.Year)
		{
			return Date.Month == ym.Date.Month;
		}
		return false;
	}

	public bool IsYearMonthEqual(DateTime date)
	{
		if (Date.Year == date.Year)
		{
			return Date.Month == date.Month;
		}
		return false;
	}

	public int CompareTo(DateYearMonth other)
	{
		if (Date.Year > other.Date.Year)
		{
			return 1;
		}
		if (Date.Year < other.Date.Year)
		{
			return -1;
		}
		if (Date.Month > other.Date.Month)
		{
			return 1;
		}
		if (Date.Month < other.Date.Month)
		{
			return -1;
		}
		return 0;
	}

	public int CompareTo(DateTime other)
	{
		if (Date.Year > other.Year)
		{
			return 1;
		}
		if (Date.Year < other.Year)
		{
			return -1;
		}
		if (Date.Month > other.Month)
		{
			return 1;
		}
		if (Date.Month < other.Month)
		{
			return -1;
		}
		return 0;
	}
}
