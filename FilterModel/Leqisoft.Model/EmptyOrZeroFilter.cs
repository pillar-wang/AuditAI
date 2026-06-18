using System;

namespace Leqisoft.Model;

[Serializable]
public class EmptyOrZeroFilter : ByIndividualValueFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => candidate.Equals(null) || candidate.Equals(string.Empty) || candidate.Equals(0m) || candidate.Equals(0);

	public EmptyOrZeroFilter(int col)
		: base(col)
	{
	}

	public EmptyOrZeroFilter()
	{
	}
}
