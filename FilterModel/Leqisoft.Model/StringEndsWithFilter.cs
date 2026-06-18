using System;

namespace Leqisoft.Model;

[Serializable]
public class StringEndsWithFilter : StringFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => candidate.Value != null && base.Value.Value != null && candidate.ToString().EndsWith(base.Value.ToString());

	public StringEndsWithFilter(int col, FilterValue value)
		: base(col, value)
	{
	}

	public StringEndsWithFilter()
	{
	}
}
