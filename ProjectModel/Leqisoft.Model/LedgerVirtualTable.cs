using Leqisoft.DTO;

namespace Leqisoft.Model;

public class LedgerVirtualTable : VirtualTable
{
	private Id64 _tableId;

	public override Id64 Id => _tableId;

	public LedgerVirtualTable(long tableId, int rowsCount, int colsCount)
		: base(rowsCount, colsCount)
	{
		_tableId = new Id64(tableId);
		base.Rows.ResetIndex();
		base.Columns.ResetIndex();
	}
}
