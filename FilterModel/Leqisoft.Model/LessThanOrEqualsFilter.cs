using System;

namespace Leqisoft.Model;

public class LessThanOrEqualsFilter<T> : ComparisonFilter<T> where T : IComparable<T>
{
	protected override Predicate<FilterValue> filter => delegate(FilterValue candidate)
	{
		try
		{
			FilterValue other = FilterValue.FromObject(base.Value);
			int num = candidate.CompareTo(other);
			if (num == -2)
			{
				return false;
			}
			return num <= 0;
		}
		catch
		{
			return false;
		}
	};

	public LessThanOrEqualsFilter(int col, T value)
		: base(col, value)
	{
	}

	public LessThanOrEqualsFilter()
	{
	}
}
