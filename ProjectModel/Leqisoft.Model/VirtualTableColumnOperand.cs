using System.Collections.Generic;

namespace Leqisoft.Model;

public class VirtualTableColumnOperand : CellsOperand
{
	public VirtualTableColumnOperand(List<Cell> cells, Table table)
		: base(cells, table)
	{
	}

	public override Cell GetCellByRowIndex(int rowIndex)
	{
		return base.Cells[rowIndex];
	}
}
