using System;
using System.Collections.Generic;
using System.Linq;
using C1.Win.C1FlexGrid;
using Leqisoft.Model;

namespace Leqisoft.UI.Controls;

public class GridFilterContext : FilterContext
{
	private readonly C1FlexGridEx _grid;

	public GridFilterContext(C1FlexGridEx grid)
	{
		_grid = grid;
		_grid.AfterDragColumn += _grid_AfterDragColumn;
	}

	private void _grid_AfterDragColumn(object sender, DragRowColEventArgs e)
	{
		_grid.FilterManager.Populate();
	}

	public override List<FilterValue> GetColumnData(string columnId)
	{
		int col = GetColumnIndex(columnId);
		return (from i in Enumerable.Range(0, _grid.BodyRowsCount)
			select GetData(i, col)).ToList();
	}

	public override string GetColumnId(C1.Win.C1FlexGrid.Column col)
	{
		return col.Name;
	}

	public override int GetColumnIndex(string columnId)
	{
		return _grid.Cols[columnId].Index - _grid.Cols.Fixed;
	}

	public override FilterValue GetData(int row, int col)
	{
		return FilterValue.FromObject(_grid.BodyGetData(row, col), _grid.GetDataDisplay(row + _grid.Rows.Fixed, col + _grid.Cols.Fixed));
	}

	public override Type GetColumnDataType(string columnId)
	{
		return _grid.Cols[columnId].DataType;
	}

	public override string GetColumnDataTypeFormatString(string columnId)
	{
		return _grid.Cols[columnId].Format;
	}

	public override Tuple<bool, string, string> IsCheckBox(int row, int col)
	{
		if (!(_grid.BodyGetCell(row, col).Style?.DataType == typeof(bool)))
		{
			return Tuple.Create(item1: false, "", "");
		}
		return Tuple.Create(item1: true, "选中", "未选中");
	}

	public override string GetColumnCaption(string columnId)
	{
		return _grid.Cols[columnId].Caption;
	}
}
