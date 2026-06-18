using System;
using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public sealed class DateMinFilter : FilterBase
{
	public override HashSet<int> Execute(List<FilterValue> data)
	{
		DateTime min = (from fv in data
			where fv.DataType == FilterDataType.Date
			select fv.Date).Min();
		return new HashSet<int>(from tup in data.Select((FilterValue fv, int i) => Tuple.Create(fv, i))
			where tup.Item1.Date == min
			select tup.Item2);
	}

	public override string ToFormula(string colText)
	{
		throw new NotSupportedException();
	}
}
