namespace Auditai.UI.Controls;

public class GridCellUpdateValueCommand : GridCommandBase
{
	private GridCellInfo _cellInfo;

	public GridCellUpdateValueCommand(GridCellInfo cellInfo)
	{
		_cellInfo = cellInfo;
	}

	public override void Execute()
	{
		if (_cellInfo.IsCellExisting)
		{
			_cellInfo.UpdateValue(_cellInfo.NewValue);
		}
	}

	public override void Undo()
	{
		if (_cellInfo.IsCellExisting)
		{
			_cellInfo.UpdateValue(_cellInfo.OldValue);
		}
	}
}
