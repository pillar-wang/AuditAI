using System;

namespace Auditai.Model;

public class WithinRangeFilter<T> : ComparisonFilter<T> where T : IComparable<T>
{
	public T Value2 { get; set; }

	protected override Predicate<FilterValue> filter => delegate(FilterValue candidate)
	{
		try
		{
			FilterValue other = FilterValue.FromObject(base.Value);
			FilterValue other2 = FilterValue.FromObject(Value2);
			int num = candidate.CompareTo(other);
			int num2 = candidate.CompareTo(other2);
			if (num == -2 || num2 == -2)
			{
				return false;
			}
			return num >= 0 && num2 <= 0;
		}
		catch
		{
			return false;
		}
	};

	public WithinRangeFilter(int col, T begin, T end)
		: base(col, begin)
	{
		Value2 = end;
	}

	public WithinRangeFilter()
	{
	}
}
