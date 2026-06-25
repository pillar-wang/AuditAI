using System;

namespace Auditai.Model;

[Serializable]
public class StringNotEndwith : StringFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => candidate.Value == null || base.Value.Value == null || !candidate.ToString().EndsWith(base.Value.ToString());

	public StringNotEndwith(int col, FilterValue value)
		: base(col, value)
	{
	}

	public StringNotEndwith()
	{
	}
}
