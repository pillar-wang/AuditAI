using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public sealed class MaxFilter : FilterBase
{
	public int Count { get; set; }

	public override HashSet<int> Execute(List<FilterValue> data)
	{
		return new HashSet<int>(from tup in (from tup in data.Select((FilterValue fv, int i) => Tuple.Create(fv, i))
				where tup.Item1.DataType == FilterDataType.Number
				orderby tup.Item1.Number descending
				select tup).Take(Count)
			select tup.Item2);
	}

	public override string ToFormula(string colText)
	{
		throw new NotSupportedException();
	}
}
