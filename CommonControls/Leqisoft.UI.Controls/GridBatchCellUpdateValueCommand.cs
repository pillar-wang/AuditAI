using System.Collections.Generic;
using System.Linq;
using C1.Win.C1FlexGrid;

namespace Leqisoft.UI.Controls;

public class GridBatchCellUpdateValueCommand : GridCommandBase
{
	private List<GridCellInfo> _cellInfos;

	public GridBatchCellUpdateValueCommand(IEnumerable<GridCellInfo> cellInfos)
	{
		_cellInfos = cellInfos.ToList();
	}

	public override void Execute()
	{
		C1FlexGridBase c1FlexGridBase = null;
		try
		{
			foreach (GridCellInfo cellInfo in _cellInfos)
			{
				if (cellInfo.IsCellExisting)
				{
					if (c1FlexGridBase == null)
					{
						c1FlexGridBase = cellInfo.Row.Grid;
						c1FlexGridBase.BeginUpdate();
					}
					cellInfo.UpdateValue(cellInfo.NewValue);
				}
			}
		}
		finally
		{
			c1FlexGridBase?.EndUpdate();
		}
	}

	public override void Undo()
	{
		foreach (GridCellInfo cellInfo in _cellInfos)
		{
			if (cellInfo.IsCellExisting)
			{
				cellInfo.UpdateValue(cellInfo.OldValue);
			}
		}
	}
}
