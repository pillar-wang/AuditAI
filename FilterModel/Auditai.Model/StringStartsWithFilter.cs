using System;

namespace Auditai.Model;

[Serializable]
public class StringStartsWithFilter : StringFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => candidate.Value != null && base.Value.Value != null && candidate.ToString().StartsWith(base.Value.ToString());

	public StringStartsWithFilter(int col, FilterValue value)
		: base(col, value)
	{
	}

	public StringStartsWithFilter()
	{
	}
}
