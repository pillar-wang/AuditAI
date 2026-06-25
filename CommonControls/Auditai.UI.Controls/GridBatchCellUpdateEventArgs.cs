using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.UI.Controls;

public class GridBatchCellUpdateEventArgs : EventArgs
{
	public List<GridCellUpdateEventArgs> CellUpdateEventArgs { get; private set; }

	public GridBatchCellUpdateEventArgs(IEnumerable<GridCellUpdateEventArgs> cellUpdateEventArgs)
	{
		CellUpdateEventArgs = cellUpdateEventArgs.ToList();
	}
}
