using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public sealed class DateMaxFilter : FilterBase
{
	public override HashSet<int> Execute(List<FilterValue> data)
	{
		DateTime max = (from fv in data
			where fv.DataType == FilterDataType.Date
			select fv.Date).Max();
		return new HashSet<int>(from tup in data.Select((FilterValue fv, int i) => Tuple.Create(fv, i))
			where tup.Item1.Date == max
			select tup.Item2);
	}

	public override string ToFormula(string colText)
	{
		throw new NotSupportedException();
	}
}
