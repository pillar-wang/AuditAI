using System;
using System.Collections.Generic;

namespace Auditai.Model;

public class Tuple2Item2Comparer : EqualityComparer<Tuple<Row, ValueOperand>>
{
	public static Tuple2Item2Comparer Instance { get; } = new Tuple2Item2Comparer();


	public override bool Equals(Tuple<Row, ValueOperand> x, Tuple<Row, ValueOperand> y)
	{
		return (x.Item2.Equal(y.Item2) as BoolOperand).Value;
	}

	public override int GetHashCode(Tuple<Row, ValueOperand> obj)
	{
		return obj.Item2.GetHashCode();
	}
}
