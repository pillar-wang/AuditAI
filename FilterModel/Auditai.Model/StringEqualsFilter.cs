using System;

namespace Auditai.Model;

[Serializable]
public class StringEqualsFilter : StringFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => (candidate.Value == null) ? (base.Value.Value == null) : (base.Value.ToString().Equals(candidate.ToString()) || base.Value.DisplayValue == candidate.DisplayValue);

	public StringEqualsFilter(int col, FilterValue value)
		: base(col, value)
	{
	}

	public StringEqualsFilter()
	{
	}
}
