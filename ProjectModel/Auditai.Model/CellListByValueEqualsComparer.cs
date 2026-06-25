using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class CellListByValueEqualsComparer : IEqualityComparer<List<Cell>>
{
	public static CellListByValueEqualsComparer Instance { get; } = new CellListByValueEqualsComparer();


	public bool Equals(List<Cell> x, List<Cell> y)
	{
		return x.SequenceEqual(y, CellValueEqualsComparer.Instance);
	}

	public int GetHashCode(List<Cell> obj)
	{
		int num = 19;
		foreach (Cell item in obj)
		{
			num = num * 31 + ValueOperand.FromObject(item.Value).GetHashCode();
		}
		return num;
	}
}
