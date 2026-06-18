using System.Drawing;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class TicketInputCellVM
{
	public bool IsMixTicketDynamicDataRow;

	public bool IsMixTicketFixedDataRow;

	public bool IsMixTicketKeyCell;

	public bool IsMixTicketExistDesignInputValue;

	public string MixTicketExistDesignInputValueShouldDisplayValue;

	public object Value { get; set; }

	public Column Column { get; set; }

	public Row TableRow
	{
		get
		{
			if (TableCell != null)
			{
				return TableCell.Row;
			}
			return null;
		}
	}

	public bool Dirty { get; private set; }

	public string Formula { get; set; }

	public bool IsFormula { get; set; }

	public bool IsFormulaFromTicket { get; set; }

	public bool IsField { get; set; }

	public Cell TempCell { get; set; }

	public Cell TableCell { get; set; }

	public bool IsDynamicTicketDataRow { get; set; }

	public bool IsFixedMultiRowKey { get; set; }

	public bool IsFixedMultiRowValue { get; set; }

	public string FixedMultiRowKeyCellDisplayValue { get; set; }

	public bool IsFixedMultiFixedCell { get; set; }

	public bool IsControlFormulaLocked
	{
		get
		{
			if (TableCell != null)
			{
				return TableCell.Row.Table.ControlLockCells.Contains(TableCell);
			}
			return false;
		}
	}

	public bool IsDynamicRowDataCell { get; set; }

	public bool IsDynamicRowKeyCell { get; set; }

	public TicketColumn TicketColumn { get; set; }

	public TicketCell TicketCell { get; set; }

	public CellAttachments Attachments { get; set; }

	public bool IsAttachmentsDirty { get; set; }

	public bool IsShowVirtualValue { get; set; }

	public string VirtualValue { get; set; }

	public bool IsExistManualInputValue
	{
		get
		{
			if (TableCell == null)
			{
				return false;
			}
			return TableCell.IsExistManualInputValue;
		}
	}

	public bool IsTableExistCell => TableCell != null;

	public bool IsExistWarning
	{
		get
		{
			if (TableCell != null)
			{
				return TableCell.Row.Table.ControlWarningCells.Contains(TableCell);
			}
			return false;
		}
	}

	public bool IsExistRemind
	{
		get
		{
			if (TableCell != null)
			{
				return TableCell.Row.Table.ControlRemindCells.Contains(TableCell);
			}
			return false;
		}
	}

	public Color ForeColor
	{
		get
		{
			if (!IsDynamicTicketDataRow)
			{
				return TicketCell.ForeColor;
			}
			return TicketColumn.ForeColor;
		}
	}

	public Color BackColor
	{
		get
		{
			if (!IsDynamicTicketDataRow)
			{
				return TicketCell.BackColor;
			}
			return TicketColumn.BackColor;
		}
	}

	public Font Font
	{
		get
		{
			if (!IsDynamicTicketDataRow)
			{
				return TicketCell.GetFont();
			}
			return TicketColumn.GetFont();
		}
	}

	public CellTextAlign Align
	{
		get
		{
			if (!IsDynamicTicketDataRow)
			{
				return TicketCell.Align;
			}
			return TicketColumn.Align;
		}
	}

	public int Indent
	{
		get
		{
			if (!IsDynamicTicketDataRow)
			{
				return TicketCell.Indent;
			}
			return TicketColumn.Indent;
		}
	}

	public TicketBorder Right
	{
		get
		{
			if (!IsDynamicTicketDataRow)
			{
				return TicketCell.Right;
			}
			return TicketColumn.Right;
		}
	}

	public TicketBorder Left
	{
		get
		{
			if (!IsDynamicTicketDataRow)
			{
				return TicketCell.Left;
			}
			return TicketColumn.Left;
		}
	}

	public DataFormat? DataFormat
	{
		get
		{
			if (!IsDynamicTicketDataRow)
			{
				return TicketCell.DataFormat;
			}
			return TicketColumn.DataFormat;
		}
	}

	public bool IsAllowManualInputOnFormula
	{
		get
		{
			if (!IsFormula)
			{
				return false;
			}
			if (IsFormulaFromTicket)
			{
				return false;
			}
			return true;
		}
	}

	public TicketBorder GetTop(bool isFirstDataRow)
	{
		if (IsDynamicTicketDataRow)
		{
			if (isFirstDataRow)
			{
				return TicketColumn.Top;
			}
			return TicketColumn.Middle;
		}
		return TicketCell.Top;
	}

	public TicketBorder GetBottom(bool isLastDataRow)
	{
		if (IsDynamicTicketDataRow)
		{
			if (isLastDataRow)
			{
				return TicketColumn.Bottom;
			}
			return TicketColumn.Middle;
		}
		return TicketCell.Bottom;
	}

	public bool IsSetToNumberFormat()
	{
		if (Column == null)
		{
			return false;
		}
		if (!DataFormat.HasValue)
		{
			return false;
		}
		return Cell.IsSetToNumberFormat(DataFormat.Value.FormatType);
	}

	public string ConvertInputValueToDisplayValue(object inputValue)
	{
		if (inputValue == null)
		{
			return string.Empty;
		}
		if (Column == null)
		{
			if (!DataFormat.HasValue)
			{
				return inputValue.ToString();
			}
			return Cell.GetDisplayValueImpl(inputValue, DataFormat.Value);
		}
		return Cell.GetDisplayValueImpl(inputValue, Column.GetFormat());
	}

	public string GetDisplayValue()
	{
		object obj = Value;
		if (IsField && Column != null)
		{
			if (IsFixedMultiRowKey)
			{
				return FixedMultiRowKeyCellDisplayValue;
			}
			if (TableCell != null && (!IsFormula || !IsFormulaFromTicket))
			{
				obj = ((!IsMixTicketExistDesignInputValue) ? TableCell.Value : ((MixTicketExistDesignInputValueShouldDisplayValue == null) ? string.Empty : MixTicketExistDesignInputValueShouldDisplayValue));
			}
		}
		if (Column == null)
		{
			if (!DataFormat.HasValue)
			{
				return obj.ToString();
			}
			return Cell.GetDisplayValueImpl(obj, DataFormat.Value);
		}
		return Cell.GetDisplayValueImpl(obj, Column.GetFormat());
	}
}
