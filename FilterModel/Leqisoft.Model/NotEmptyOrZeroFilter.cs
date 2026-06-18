using System;

namespace Leqisoft.Model;

[Serializable]
public class NotEmptyOrZeroFilter : ByIndividualValueFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => !candidate.Equals(string.Empty) && !candidate.Equals(0m) && !candidate.Equals(0);

	public NotEmptyOrZeroFilter(int col)
		: base(col)
	{
	}

	public NotEmptyOrZeroFilter()
	{
	}
}
