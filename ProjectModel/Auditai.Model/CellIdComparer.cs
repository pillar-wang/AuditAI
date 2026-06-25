using System.Collections.Generic;

namespace Auditai.Model;

public class CellIdComparer : IEqualityComparer<Cell>
{
	public static CellIdComparer Instance { get; } = new CellIdComparer();


	private CellIdComparer()
	{
	}

	public bool Equals(Cell x, Cell y)
	{
		return x.Id == y.Id;
	}

	public int GetHashCode(Cell obj)
	{
		return obj.Id.GetHashCode();
	}
}
