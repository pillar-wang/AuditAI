using System;
using System.Collections.Generic;
using C1.Win.C1FlexGrid;
using Auditai.Model;

namespace Auditai.UI.Controls;

public abstract class FilterContext
{
	public abstract FilterValue GetData(int row, int col);

	public virtual FilterValue GetDataFilterOnColumnHeader(int col)
	{
		return GetData(0, col);
	}

	public abstract string GetColumnId(C1.Win.C1FlexGrid.Column col);

	public abstract List<FilterValue> GetColumnData(string columnId);

	public abstract int GetColumnIndex(string columnId);

	public abstract Type GetColumnDataType(string columnId);

	public abstract string GetColumnDataTypeFormatString(string columnId);

	public abstract Tuple<bool, string, string> IsCheckBox(int row, int col);

	public virtual Tuple<bool, string, string> IsCheckBoxFilterOnColumnHeader(int col)
	{
		return IsCheckBox(0, col);
	}

	public abstract string GetColumnCaption(string columnId);
}
