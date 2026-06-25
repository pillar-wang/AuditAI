using System;

namespace Auditai.Model;

public class StringNotContainsFilter : StringFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => candidate.Value == null || base.Value.Value == null || !candidate.ToString().Contains(base.Value.ToString());

	public StringNotContainsFilter(int col, FilterValue value)
		: base(col, value)
	{
	}

	public StringNotContainsFilter()
	{
	}
}
