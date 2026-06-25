using System;
using System.Collections.Generic;
using System.Linq;
using C1.Win.C1FlexGrid;
using Auditai.DTO;
using Auditai.Model;

namespace Auditai.UI.Controls;

public class TableFilterContext : FilterContext
{
	public Auditai.Model.Table Table { get; set; }

	public override FilterValue GetData(int row, int col)
	{
		Auditai.Model.Cell cell = Table[row, col];
		return FilterValue.FromObject(cell.Value, cell.GetDisplayValue());
	}

	public override FilterValue GetDataFilterOnColumnHeader(int col)
	{
		Auditai.Model.Column column = Table.Columns[col];
		Type dataType = column.GetDataType();
		object value = string.Empty;
		if (dataType == typeof(DateTime))
		{
			value = DateTime.Now;
		}
		else if (dataType == typeof(TimeSpan))
		{
			value = TimeSpan.FromDays(1.0);
		}
		else if (dataType == typeof(DateYearMonth))
		{
			value = new DateYearMonth(DateTime.Now);
		}
		object value2 = Auditai.Model.Cell.ChangeDataTypeImpl(value, dataType);
		return FilterValue.FromObject(value2, Auditai.Model.Cell.GetDisplayValueImpl(value2, column.GetFormat()));
	}

	public override List<FilterValue> GetColumnData(string columnId)
	{
		Auditai.Model.Column modelColumn = GetModelColumn(columnId);
		if (modelColumn == null)
		{
			return new List<FilterValue>();
		}
		return (from c in modelColumn.GetCells()
			select FilterValue.FromObject(c.Value, c.GetDisplayValue())).ToList();
	}

	public override string GetColumnId(C1.Win.C1FlexGrid.Column col)
	{
		return Table.Columns[col.Index - col.Grid.Cols.Fixed].Id.Value.ToString();
	}

	public override int GetColumnIndex(string columnId)
	{
		return GetModelColumn(columnId)?.Index ?? (-1);
	}

	private Auditai.Model.Column GetModelColumn(string columnId)
	{
		return Table.Columns.GetById(Id64.Parse(columnId));
	}

	public override Type GetColumnDataType(string columnId)
	{
		return GetModelColumn(columnId).Style?.DataType ?? typeof(string);
	}

	public override string GetColumnDataTypeFormatString(string columnId)
	{
		Auditai.Model.Column modelColumn = GetModelColumn(columnId);
		if (modelColumn == null)
		{
			return string.Empty;
		}
		return modelColumn.GetFormat().GetFormatString();
	}

	public override Tuple<bool, string, string> IsCheckBox(int row, int col)
	{
		return Table[row, col].DisplayFormat.FormatType switch
		{
			DataFormatType.BoolCheckBox => Tuple.Create(item1: true, "选中", "未选中"), 
			DataFormatType.BoolOnOff => Tuple.Create(item1: true, "打开", "关闭"), 
			_ => Tuple.Create(item1: false, "", ""), 
		};
	}

	public override Tuple<bool, string, string> IsCheckBoxFilterOnColumnHeader(int col)
	{
		return Table.Columns[col].GetFormat().FormatType switch
		{
			DataFormatType.BoolCheckBox => Tuple.Create(item1: true, "选中", "未选中"), 
			DataFormatType.BoolOnOff => Tuple.Create(item1: true, "打开", "关闭"), 
			_ => Tuple.Create(item1: false, "", ""), 
		};
	}

	public override string GetColumnCaption(string columnId)
	{
		return GetModelColumn(columnId).Caption;
	}
}
