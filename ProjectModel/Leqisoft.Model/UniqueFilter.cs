using System;
using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public sealed class UniqueFilter : FilterBase
{
	public override HashSet<int> Execute(List<FilterValue> data)
	{
		return new HashSet<int>(from tup in (from tup in data.Select((FilterValue fv, int i) => Tuple.Create(fv, i))
				group tup by tup.Item1 into g
				where g.Count() == 1
				select g).SelectMany((IGrouping<FilterValue, Tuple<FilterValue, int>> g) => g)
			select tup.Item2);
	}

	public override string ToFormula(string colText)
	{
		throw new NotSupportedException();
	}
}
