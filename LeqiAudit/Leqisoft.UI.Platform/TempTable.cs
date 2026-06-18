using System.Collections.Generic;

namespace Leqisoft.UI.Platform;

public class TempTable
{
	private readonly Dictionary<int, TempRow> _rows = new Dictionary<int, TempRow>();

	public long TableId { get; set; }

	public int SheetOffset { get; set; }

	public TempRow this[int row] => _rows[row];

	public void Add(TempRow sheetRow)
	{
		_rows.Add(sheetRow.Index, sheetRow);
	}

	public Dictionary<int, TempRow>.Enumerator GetEnumerator()
	{
		return _rows.GetEnumerator();
	}
}
