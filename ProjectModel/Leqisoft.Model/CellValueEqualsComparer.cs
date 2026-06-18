using System.Collections.Generic;

namespace Leqisoft.Model;

public class CellValueEqualsComparer : IEqualityComparer<Cell>
{
	public static CellValueEqualsComparer Instance { get; } = new CellValueEqualsComparer();


	public bool Equals(Cell x, Cell y)
	{
		return (bool)ValueOperand.FromObject(x.Value).Equal(ValueOperand.FromObject(y.Value)).ToBool();
	}

	public int GetHashCode(Cell obj)
	{
		return obj.Value.GetHashCode();
	}
}
