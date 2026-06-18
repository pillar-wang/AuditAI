using System;
using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

[Serializable]
public abstract class ByIndividualValueFilter : FilterBase
{
	protected abstract Predicate<FilterValue> filter { get; }

	public ByIndividualValueFilter(int col)
		: base(col)
	{
	}

	public ByIndividualValueFilter()
	{
	}

	protected internal sealed override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		return (from kv in values
			where filter(kv.Value)
			select kv.Key).ToList();
	}
}
