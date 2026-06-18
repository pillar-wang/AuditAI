using System.Collections.Generic;

namespace Leqisoft.Model;

public class ColumnIdComparer : IEqualityComparer<Column>
{
	public static ColumnIdComparer Instance { get; } = new ColumnIdComparer();


	private ColumnIdComparer()
	{
	}

	public bool Equals(Column x, Column y)
	{
		return x.Id == y.Id;
	}

	public int GetHashCode(Column obj)
	{
		return obj.Id.GetHashCode();
	}
}
