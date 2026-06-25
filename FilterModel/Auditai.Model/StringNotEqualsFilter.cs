using System;

namespace Auditai.Model;

[Serializable]
public class StringNotEqualsFilter : StringFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => (candidate.Value == null) ? (base.Value.Value != null) : (!base.Value.Equals(candidate));

	public StringNotEqualsFilter(int col, FilterValue value)
		: base(col, value)
	{
	}

	public StringNotEqualsFilter()
	{
	}
}
