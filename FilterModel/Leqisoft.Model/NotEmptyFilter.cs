using System;

namespace Leqisoft.Model;

[Serializable]
public class NotEmptyFilter : ByIndividualValueFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => candidate.Value != null && candidate.ToString() != string.Empty;

	public NotEmptyFilter(int col)
		: base(col)
	{
	}

	public NotEmptyFilter()
	{
	}
}
