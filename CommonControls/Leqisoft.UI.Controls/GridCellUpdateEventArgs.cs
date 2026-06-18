using System;

namespace Leqisoft.UI.Controls;

public class GridCellUpdateEventArgs : EventArgs
{
	public int Row { get; private set; }

	public int Col { get; private set; }

	public object OldValue { get; private set; }

	public object NewValue { get; private set; }

	public GridCellUpdateEventArgs(int row, int col, object oldValue, object newValue)
	{
		Row = row;
		Col = col;
		OldValue = oldValue;
		NewValue = newValue;
	}
}
