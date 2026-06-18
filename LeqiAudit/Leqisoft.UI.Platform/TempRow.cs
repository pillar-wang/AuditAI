using System.Collections.Generic;

namespace Leqisoft.UI.Platform;

public class TempRow
{
	private readonly Dictionary<int, TempCell> _cells = new Dictionary<int, TempCell>();

	public int Index { get; set; }

	public TempCell this[int col] => _cells[col];

	public void Add(TempCell cell)
	{
		if (_cells.ContainsKey(cell.Col))
		{
			_cells[cell.Col] = cell;
		}
		else
		{
			_cells.Add(cell.Col, cell);
		}
	}

	public TempRow(int index)
	{
		Index = index;
	}

	public Dictionary<int, TempCell>.Enumerator GetEnumerator()
	{
		return _cells.GetEnumerator();
	}
}
