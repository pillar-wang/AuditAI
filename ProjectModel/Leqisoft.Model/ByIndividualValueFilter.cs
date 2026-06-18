using System;
using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public abstract class ByIndividualValueFilter : FilterBase
{
	public sealed override HashSet<int> Execute(List<FilterValue> data)
	{
		return new HashSet<int>(from tup in data.Select((FilterValue f, int i) => Tuple.Create(f, i))
			where Execute(tup.Item1)
			select tup.Item2);
	}

	public abstract bool Execute(FilterValue value);
}
