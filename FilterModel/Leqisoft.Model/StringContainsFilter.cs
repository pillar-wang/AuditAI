using System;

namespace Leqisoft.Model;

[Serializable]
public class StringContainsFilter : StringFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => candidate.Value != null && base.Value.Value != null && candidate.ToString().Contains(base.Value.ToString());

	public StringContainsFilter(int col, FilterValue value)
		: base(col, value)
	{
	}

	public StringContainsFilter()
	{
	}
}
