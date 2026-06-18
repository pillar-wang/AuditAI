using System;
using System.Collections.Generic;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class CellValueSortComparer : IComparer<object>
{
	public static CellValueSortComparer Instance { get; } = new CellValueSortComparer();


	public int Compare(object x, object y)
	{
		if (x == null || y == null)
		{
			throw new ArgumentNullException();
		}
		if (x is DateTime dateTime && y is DateTime value)
		{
			return dateTime.CompareTo(value);
		}
		if (x is DateYearMonth dateYearMonth && y is DateYearMonth other)
		{
			return dateYearMonth.CompareTo(other);
		}
		if (x is string s && double.TryParse(s, out var result))
		{
			x = result;
		}
		if (y is string s2 && double.TryParse(s2, out var result2))
		{
			y = result2;
		}
		if (x is double num && y is double value2)
		{
			return num.CompareTo(value2);
		}
		if (x is double && !(y is double))
		{
			return -1;
		}
		if (!(x is double) && y is double)
		{
			return 1;
		}
		return x.ToString().CompareTo(y.ToString());
	}
}
