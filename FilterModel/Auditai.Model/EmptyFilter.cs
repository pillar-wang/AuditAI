using System;

namespace Auditai.Model;

[Serializable]
public class EmptyFilter : ByIndividualValueFilter
{
	protected override Predicate<FilterValue> filter => (FilterValue candidate) => candidate.Equals(null) || candidate.Equals(string.Empty) || candidate.Equals(false);

	public EmptyFilter(int col)
		: base(col)
	{
	}

	public EmptyFilter()
	{
	}
}
