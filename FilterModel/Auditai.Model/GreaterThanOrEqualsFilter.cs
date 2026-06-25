using System;

namespace Auditai.Model;

public class GreaterThanOrEqualsFilter<T> : ComparisonFilter<T> where T : IComparable<T>
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
			return num >= 0;
		}
		catch
		{
			return false;
		}
	};

	public GreaterThanOrEqualsFilter(int col, T value)
		: base(col, value)
	{
	}

	public GreaterThanOrEqualsFilter()
	{
	}
}
