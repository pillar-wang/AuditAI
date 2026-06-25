using C1.Win.C1FlexGrid;

namespace Auditai.UI.Controls;

public class GridCellInfo
{
	public C1FlexGrid FlexGrid { get; private set; }

	public Row Row { get; private set; }

	public Column Column { get; private set; }

	public object OldValue { get; private set; }

	public object NewValue { get; private set; }

	public bool IsCellExisting
	{
		get
		{
			if (FlexGrid.Rows.Contains(Row))
			{
				return FlexGrid.Cols.Contains(Column);
			}
			return false;
		}
	}

	public GridCellInfo(C1FlexGrid grid, int row, int col, object oldValue, object newValue)
	{
		OldValue = oldValue;
		NewValue = newValue;
		FlexGrid = grid;
		Row = grid.Rows[row];
		Column = grid.Cols[col];
	}

	public void UpdateValue(object newValue)
	{
		Row.Grid[Row.Index, Column.Index] = newValue;
	}
}
