using System;

namespace Leqisoft.Model;

public class NotEqualsFilter<T> : ComparisonFilter<T> where T : IComparable<T>
{
	protected override Predicate<FilterValue> filter => delegate(FilterValue candidate)
	{
		FilterValue obj = FilterValue.FromObject(base.Value);
		return !candidate.Equals(obj);
	};

	public NotEqualsFilter(int col, T value)
		: base(col, value)
	{
	}

	public NotEqualsFilter()
	{
	}
}
