using System;

namespace Leqisoft.Model;

[Serializable]
public class StringNotBeginwith : StringFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => candidate.Value == null || base.Value.Value == null || !candidate.ToString().StartsWith(base.Value.ToString());

	public StringNotBeginwith(int col, FilterValue value)
		: base(col, value)
	{
	}

	public StringNotBeginwith()
	{
	}
}
