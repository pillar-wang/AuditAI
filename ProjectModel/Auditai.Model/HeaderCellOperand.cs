using System.Linq;

namespace Auditai.Model;

public class HeaderCellOperand : CellsOperand
{
	private int _headerCellRowIndex;

	public Cell HeaderCell { get; }

	public override OperandType OperandType => OperandType.HeaderCellOperand;

	public HeaderCellOperand(Cell headerCell)
		: base(headerCell.GetCells().ToList(), headerCell._Table)
	{
		HeaderCell = headerCell;
		_headerCellRowIndex = headerCell.Row.Index;
	}

	public override Cell GetCellByRowIndex(int rowIndex)
	{
		return base.Cells[rowIndex - _headerCellRowIndex - 1];
	}
}
