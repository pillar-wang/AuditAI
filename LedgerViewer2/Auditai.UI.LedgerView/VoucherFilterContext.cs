using System;
using System.Collections.Generic;
using C1.Win.C1FlexGrid;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

public class VoucherFilterContext : FilterContext
{
	private readonly VoucherListEditor _vle;

	public VoucherFilterContext(VoucherListEditor vle)
	{
		_vle = vle;
	}

	public override string GetColumnCaption(string columnId)
	{
		return _vle._grid.Cols[columnId].Caption;
	}

	public override List<FilterValue> GetColumnData(string columnId)
	{
		List<FilterValue> list = new List<FilterValue>();
		int columnIndex = GetColumnIndex(columnId);
		for (int i = 0; i < _vle._grid.BodyRowsCount; i++)
		{
			list.Add(GetData(i, columnIndex));
		}
		return list;
	}

	public override Type GetColumnDataType(string columnId)
	{
		return _vle._grid.Cols[columnId].DataType;
	}

	public override string GetColumnDataTypeFormatString(string columnId)
	{
		return string.Empty;
	}

	public override string GetColumnId(C1.Win.C1FlexGrid.Column col)
	{
		return col.Name;
	}

	public override int GetColumnIndex(string columnId)
	{
		return _vle._grid.Cols[columnId].Index - _vle._grid.Cols.Fixed;
	}

	public override FilterValue GetData(int row, int col)
	{
		return FilterValue.FromObject(_vle.GetValue(row, col), _vle.GetTextDisplay(row, col));
	}

	public override Tuple<bool, string, string> IsCheckBox(int row, int col)
	{
		return Tuple.Create(item1: false, "", "");
	}
}
