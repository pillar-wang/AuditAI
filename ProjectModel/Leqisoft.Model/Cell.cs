using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class Cell
{
	[CompilerGenerated]
	private sealed class _003CGetCells_003Ed__133 : IEnumerable<Cell>, IEnumerable, IEnumerator<Cell>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Cell _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public Cell _003C_003E4__this;

		private int _003ClastRow_003E5__2;

		private int _003Ci_003E5__3;

		Cell IEnumerator<Cell>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetCells_003Ed__133(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			Cell cell = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003ClastRow_003E5__2 = cell.GetHeaderLastRow();
				_003Ci_003E5__3 = cell.Row.Index + 1;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				break;
			}
			if (_003Ci_003E5__3 <= _003ClastRow_003E5__2)
			{
				_003C_003E2__current = cell._Table[_003Ci_003E5__3, cell.Column.Index];
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Cell> IEnumerable<Cell>.GetEnumerator()
		{
			_003CGetCells_003Ed__133 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetCells_003Ed__133(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Cell>)this).GetEnumerator();
		}
	}

	private const int MAX_CELL_VALUE_LENGTH = 10000;

	internal static HashSet<Id64> _circularRefDetector = new HashSet<Id64>();

	private static HashSet<FormulaTrigger> _invalids = new HashSet<FormulaTrigger>();

	private static int _updateValueDepth = 0;

	private string _collectSource;

	private CellPrivateData _cellPrivateData;

	public CellDirtyMask Dirty;

	public static bool UpdateValueSuccessFlag { get; set; }

	public Id64 Id { get; set; }

	public SyncStatus Status { get; set; }

	public bool NeedSave { get; set; }

	internal Table _Table => Column?.Table;

	public object Value { get; set; }

	public bool IsExisting
	{
		get
		{
			if (Status != 0)
			{
				return Status == SyncStatus.Synced;
			}
			return true;
		}
	}

	public Column Column { get; set; }

	public Row Row { get; set; }

	public string Formula { get; set; }

	public bool HasFormula => !string.IsNullOrEmpty(Formula);

	public string HeaderFormula { get; set; }

	public bool HasHeaderFormula => !string.IsNullOrEmpty(HeaderFormula);

	public bool IsFormulaRefSelfTableColumnOrCell { get; private set; }

	public CellStyle Style { get; set; }

	public string CollectSource
	{
		get
		{
			return _collectSource;
		}
		set
		{
			_collectSource = value ?? string.Empty;
		}
	}

	public string DisplayFontFamily
	{
		get
		{
			string text = Style?.FontFamily;
			if (text == null && ShouldApplyColumnFormula())
			{
				text = Column.Style?.FontFamily;
			}
			if (text == null)
			{
				text = _Table.DefaultStyle.FontFamily;
			}
			return text;
		}
	}

	public Color DisplayForeColor
	{
		get
		{
			Color? color = Style?.ForeColor;
			if (!color.HasValue && ShouldApplyColumnFormula())
			{
				color = Column.Style?.ForeColor;
			}
			if (!color.HasValue)
			{
				color = _Table.DefaultStyle.ForeColor;
			}
			return color.Value;
		}
	}

	public Color DisplayBackColor
	{
		get
		{
			Color? color = Style?.BackColor;
			if (!color.HasValue && ShouldApplyColumnFormula())
			{
				color = Column.Style?.BackColor;
			}
			if (!color.HasValue)
			{
				color = _Table.DefaultStyle.BackColor;
			}
			return color.Value;
		}
	}

	public float DisplayFontSize
	{
		get
		{
			float? num = Style?.FontSize;
			if (!num.HasValue && ShouldApplyColumnFormula())
			{
				num = Column.Style?.FontSize;
			}
			if (!num.HasValue)
			{
				num = _Table.DefaultStyle.FontSize;
			}
			return num.Value;
		}
	}

	public CellTextAlign DisplayAlign
	{
		get
		{
			CellTextAlign? cellTextAlign = Style?.Align;
			if (!cellTextAlign.HasValue && ShouldApplyColumnFormula())
			{
				cellTextAlign = Column.Style?.Align;
			}
			if (!cellTextAlign.HasValue)
			{
				cellTextAlign = _Table.DefaultStyle.Align;
			}
			return cellTextAlign.Value;
		}
	}

	public int DisplayMargin
	{
		get
		{
			int? num = Style?.Margin;
			if (!num.HasValue && ShouldApplyColumnFormula())
			{
				num = Column.Style?.Margin;
			}
			if (!num.HasValue)
			{
				num = _Table.DefaultStyle.Margin;
			}
			return num.Value;
		}
	}

	public bool DisplayBold
	{
		get
		{
			bool? flag = Style?.Bold;
			if (!flag.HasValue && ShouldApplyColumnFormula())
			{
				flag = Column.Style?.Bold;
			}
			if (!flag.HasValue)
			{
				flag = _Table.DefaultStyle.Bold;
			}
			return flag.Value;
		}
	}

	public bool DisplayItalic
	{
		get
		{
			bool? flag = Style?.Italic;
			if (!flag.HasValue && ShouldApplyColumnFormula())
			{
				flag = Column.Style?.Italic;
			}
			if (!flag.HasValue)
			{
				flag = _Table.DefaultStyle.Italic;
			}
			return flag.Value;
		}
	}

	public bool DisplayUnderline
	{
		get
		{
			bool? flag = Style?.Underline;
			if (!flag.HasValue && ShouldApplyColumnFormula())
			{
				flag = Column.Style?.Underline;
			}
			if (!flag.HasValue)
			{
				flag = _Table.DefaultStyle.Underline;
			}
			return flag.Value;
		}
	}

	public Type DisplayDataType
	{
		get
		{
			Type type = Style?.DataType;
			if (type == null && ShouldApplyColumnFormula())
			{
				type = Column.Style?.DataType;
			}
			if (type == null)
			{
				type = _Table.DefaultStyle.DataType;
			}
			return type;
		}
	}

	public DataFormat DisplayFormat
	{
		get
		{
			DataFormat? dataFormat = Style?.Format;
			if (!dataFormat.HasValue && ShouldApplyColumnFormula())
			{
				dataFormat = Column.Style?.Format;
			}
			if (!dataFormat.HasValue)
			{
				dataFormat = _Table.DefaultStyle.Format;
			}
			return dataFormat.Value;
		}
	}

	public long DisplayLocked
	{
		get
		{
			long? num = Style?.Locker;
			if (!num.HasValue && ShouldApplyColumnFormula())
			{
				num = Column.Style?.Locker;
			}
			if (!num.HasValue)
			{
				num = _Table.DefaultStyle.Locker;
			}
			return num.Value;
		}
	}

	public string DisplayDefaultValue
	{
		get
		{
			string text = Style?.DefaultValue;
			if (text == null && ShouldApplyColumnFormula())
			{
				text = Column.Style?.DefaultValue;
			}
			if (text == null)
			{
				text = _Table.DefaultStyle.DefaultValue;
			}
			return text ?? string.Empty;
		}
	}

	public string DisplayComment
	{
		get
		{
			string text = Style?.Comment;
			if (text == null && ShouldApplyColumnFormula())
			{
				text = Column.Style?.Comment;
			}
			if (text == null)
			{
				text = _Table.DefaultStyle.Comment;
			}
			return text ?? string.Empty;
		}
	}

	public bool IsValueEmpty
	{
		get
		{
			object value = Value;
			if (!(value is string value2))
			{
				if (!(value is double num))
				{
					if (value is DateYearMonth dateYearMonth)
					{
						return dateYearMonth.IsZero;
					}
					return false;
				}
				return num == 0.0;
			}
			return string.IsNullOrWhiteSpace(value2);
		}
	}

	public bool IsExistManualInputValue
	{
		get
		{
			if (_cellPrivateData == null)
			{
				return false;
			}
			return _cellPrivateData.IsExistManualInputValue;
		}
		set
		{
			if ((value || _cellPrivateData != null) && (_cellPrivateData == null || _cellPrivateData.IsExistManualInputValue != value))
			{
				if (_cellPrivateData == null)
				{
					_cellPrivateData = new CellPrivateData();
				}
				_cellPrivateData.IsExistManualInputValue = value;
				SetCellPrivateDataToDirty();
			}
		}
	}

	public bool IsAllowManualInputOnFormula
	{
		get
		{
			if (HasFormula)
			{
				return false;
			}
			if (!HasColumnFormula())
			{
				return false;
			}
			return true;
		}
	}

	public bool IsAllowUseColumnFormulaResultUpdateCellValue
	{
		get
		{
			if (HasFormula)
			{
				return false;
			}
			if (!ShouldApplyColumnFormula())
			{
				return false;
			}
			if (IsExistManualInputValue)
			{
				return false;
			}
			return true;
		}
	}

	public bool HasCellFormulaOrColumnFormula
	{
		get
		{
			if (!HasColumnFormula())
			{
				return HasFormula;
			}
			return true;
		}
	}

	public bool IsEmpty
	{
		get
		{
			if (Value == null || Value.Equals(string.Empty))
			{
				return true;
			}
			switch (DisplayFormat.FormatType)
			{
			case DataFormatType.General:
			case DataFormatType.Number:
			case DataFormatType.Percentage:
			case DataFormatType.NumDollar:
			case DataFormatType.NumRmb:
			case DataFormatType.Comma:
				if (Value is double num)
				{
					return num == 0.0;
				}
				return false;
			case DataFormatType.DateSlash:
			case DataFormatType.DateDash:
			case DataFormatType.DateChinese:
			case DataFormatType.DateDot:
				if (Value is DateTime dateTime)
				{
					return dateTime == default(DateTime);
				}
				return false;
			case DataFormatType.DateYearMonthChinese:
			case DataFormatType.DateYearMonthDash:
			case DataFormatType.DateYearMonthSlash:
			case DataFormatType.DateYearMonthDot:
				if (Value is DateYearMonth dateYearMonth)
				{
					return dateYearMonth.IsZero;
				}
				return false;
			case DataFormatType.BoolYesNo:
			case DataFormatType.BoolRightWrong:
			case DataFormatType.BoolTickCross:
			case DataFormatType.BoolCheckBox:
			case DataFormatType.BoolOnOff:
				if (Value is bool flag)
				{
					return !flag;
				}
				return false;
			case DataFormatType.ComboList:
				if (Value != null)
				{
					return Value.Equals(string.Empty);
				}
				return true;
			default:
				return false;
			}
		}
	}

	public bool IsEditable
	{
		get
		{
			if (DisplayLocked == 0L && (!HasCellFormulaOrColumnFormula || IsAllowManualInputOnFormula) && !TryGetHeaderCellFormulaCell(out var _) && !Row.IsLocked)
			{
				return !_Table.ControlLockCells.Contains(this);
			}
			return false;
		}
	}

	private void SetCellPrivateDataToDirty()
	{
		NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsValueDirty = true;
		}
		_Table.NeedSave = true;
		_Table.Ticket.IsCacheExpired = true;
	}

	public void DeserializeCellPrivateData(byte[] data)
	{
		_cellPrivateData = CellPrivateData.FromBytes(data);
	}

	public Font GetFont()
	{
		return new Font(DisplayFontFamily, DisplayFontSize, GetFontStyle());
	}

	private FontStyle GetFontStyle()
	{
		FontStyle fontStyle = FontStyle.Regular;
		if (DisplayBold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (DisplayItalic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (DisplayUnderline)
		{
			fontStyle |= FontStyle.Underline;
		}
		return fontStyle;
	}

	public string GetDisplayValue(bool applyZeroFormat = true)
	{
		DataFormat displayFormat = DisplayFormat;
		return GetDisplayValueImpl(Value, displayFormat, applyZeroFormat);
	}

	public static string GetDisplayValueImpl(object value, DataFormat df, bool applyZeroFormat = true)
	{
		switch (df.FormatType)
		{
		case DataFormatType.General:
		case DataFormatType.Number:
		case DataFormatType.Percentage:
		case DataFormatType.NumDollar:
		case DataFormatType.NumRmb:
		case DataFormatType.Comma:
		{
			string formatString = df.GetFormatString();
			if (value is double d)
			{
				if (NumberOperand.Equals(d, 0.0))
				{
					if (applyZeroFormat)
					{
						switch (df.ZeroFormat)
						{
						case ZeroFormat.Zero:
							return string.Format("{0:" + formatString + "}", value);
						case ZeroFormat.Empty:
							return string.Empty;
						case ZeroFormat.Dash:
							return "-";
						}
						break;
					}
					return string.Format("{0:" + formatString + "}", value);
				}
				return string.Format("{0:" + formatString + "}", value);
			}
			return value.ToString();
		}
		case DataFormatType.DateSlash:
		case DataFormatType.DateDash:
		case DataFormatType.DateChinese:
		case DataFormatType.DateDot:
		{
			string formatString = df.GetFormatString();
			if (value is DateTime dateTime2)
			{
				if (dateTime2 == DateOperand.Zero.Value)
				{
					return string.Empty;
				}
				return string.Format("{0:" + formatString + "}", value);
			}
			if (value is DateYearMonth dateYearMonth2)
			{
				if (dateYearMonth2.IsZero)
				{
					return string.Empty;
				}
				return string.Format("{0:" + formatString + "}", dateYearMonth2.Date);
			}
			return value.ToString();
		}
		case DataFormatType.DateYearMonthChinese:
		case DataFormatType.DateYearMonthDash:
		case DataFormatType.DateYearMonthSlash:
		case DataFormatType.DateYearMonthDot:
		{
			string formatString = df.GetFormatString();
			if (value is DateYearMonth dateYearMonth)
			{
				if (dateYearMonth.IsZero)
				{
					return string.Empty;
				}
				return string.Format("{0:" + formatString + "}", dateYearMonth.Date);
			}
			if (value is DateTime dateTime)
			{
				if (dateTime == DateOperand.Zero.Value)
				{
					return string.Empty;
				}
				return string.Format("{0:" + formatString + "}", value);
			}
			return value.ToString();
		}
		case DataFormatType.BoolYesNo:
		case DataFormatType.BoolRightWrong:
		case DataFormatType.BoolTickCross:
			if (!(value is bool key))
			{
				return value.ToString();
			}
			return df.GetFormatDictForBool()[key];
		case DataFormatType.BoolCheckBox:
		case DataFormatType.BoolOnOff:
			if (!(value is bool))
			{
				return value.ToString();
			}
			if (!(bool)value)
			{
				return string.Empty;
			}
			return "√";
		case DataFormatType.ComboList:
			return value.ToString();
		case DataFormatType.TimeLong:
			if (!(value is TimeSpan timeSpan4))
			{
				return value.ToString();
			}
			return $"{timeSpan4.Hours}:{timeSpan4.Minutes:d2}:{timeSpan4.Seconds:d2}";
		case DataFormatType.TimeLongChinese:
			if (!(value is TimeSpan timeSpan3))
			{
				return value.ToString();
			}
			return $"{timeSpan3.Hours}时{timeSpan3.Minutes:d2}分{timeSpan3.Seconds:d2}秒";
		case DataFormatType.TimeShort:
			if (!(value is TimeSpan timeSpan2))
			{
				return value.ToString();
			}
			return $"{timeSpan2.Hours}:{timeSpan2.Minutes:d2}";
		case DataFormatType.TimeShortChinese:
			if (!(value is TimeSpan timeSpan))
			{
				return value.ToString();
			}
			return $"{timeSpan.Hours}时{timeSpan.Minutes:d2}分";
		}
		throw new ArgumentOutOfRangeException();
	}

	public static bool IsSetToNumberFormat(DataFormatType cellFormat)
	{
		if ((uint)(cellFormat - 1) <= 3u || cellFormat == DataFormatType.Comma)
		{
			return true;
		}
		return false;
	}

	public bool IsSetToNumberFormat()
	{
		DataFormatType formatType = DisplayFormat.FormatType;
		if ((uint)(formatType - 1) <= 3u || formatType == DataFormatType.Comma)
		{
			return true;
		}
		return false;
	}

	public static object ConvertInputValueToCellValue(object value)
	{
		if (value is string { Length: >10000 } text)
		{
			return text.Substring(0, 10000);
		}
		if (value == null)
		{
			return string.Empty;
		}
		if (value is decimal num)
		{
			return (double)num;
		}
		if (value is DBNull)
		{
			return string.Empty;
		}
		return value;
	}

	public bool UpdateValue(object value)
	{
		if (value is string { Length: >10000 } text)
		{
			value = text.Substring(0, 10000);
		}
		if (value == null)
		{
			value = string.Empty;
		}
		if (value is decimal num)
		{
			value = (double)num;
		}
		if (Value.Equals(value))
		{
			return false;
		}
		if (_Table.EnableFormulaTrigger)
		{
			if (_circularRefDetector.Contains(Id))
			{
				return false;
			}
			EnterStack();
		}
		NeedSave = true;
		Value = value;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsValueDirty = true;
		}
		if (_Table.EnableFormulaTrigger)
		{
			if (_Table._isBatchUpdating)
			{
				_Table._batchUpdatingCells.Add(this);
			}
			else
			{
				foreach (FormulaTrigger formulaTrigger in _Table._formulaTriggers)
				{
					try
					{
						formulaTrigger.Execute(new Cell[1] { this });
					}
					catch (FormulaBadReferenceException)
					{
						_invalids.Add(formulaTrigger);
					}
				}
				_Table.EvalControlFormula();
			}
		}
		_Table.NeedSave = true;
		_Table.Ticket.IsCacheExpired = true;
		if (_Table.EnableFormulaTrigger)
		{
			LeaveStack();
		}
		UpdateValueSuccessFlag = true;
		return true;
	}

	private void EnterStack()
	{
		_updateValueDepth++;
		_circularRefDetector.Add(Id);
	}

	private void LeaveStack()
	{
		_updateValueDepth--;
		_circularRefDetector.Remove(Id);
		if (_updateValueDepth != 0)
		{
			return;
		}
		foreach (FormulaTrigger invalid in _invalids)
		{
			_Table._formulaTriggers.Remove(invalid);
		}
		_invalids.Clear();
	}

	public void UpdateStyle(CellStyle style)
	{
		if (Style != style)
		{
			NeedSave = true;
			Style = style;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsStyleDirty = true;
			}
			_Table.NeedSave = true;
		}
	}

	internal void UpdateDependencies()
	{
		IsFormulaRefSelfTableColumnOrCell = false;
		Table table = Row.Table;
		table._formulaTriggers.RemoveWhere((FormulaTrigger ft) => ft.DstCell == this);
		if (!HasFormula)
		{
			return;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(table.Project);
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Formula);
			FormulaReferences references = formulaEvaluator.GetReferences(resolver);
			foreach (Cell cellReference in references.CellReferences)
			{
				_Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.Cell_Cell,
					SrcCell = cellReference,
					DstCell = this
				});
				if (cellReference._Table == _Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (RangeOperand rangeReference in references.RangeReferences)
			{
				table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.Range_Cell,
					SrcRange = rangeReference,
					DstCell = this
				});
				if (rangeReference.Table == _Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (Column columnWildcardReference in references.ColumnWildcardReferences)
			{
				table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.ColumnWildcard_Cell,
					SrcColumn = columnWildcardReference,
					DstCell = this
				});
				if (columnWildcardReference.Table == _Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (Column columnReference in references.ColumnReferences)
			{
				table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.Column_Cell,
					SrcColumn = columnReference,
					DstCell = this
				});
				if (columnReference.Table == _Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (Cell headerCellReference in references.HeaderCellReferences)
			{
				_Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.HeaderCell_Cell,
					SrcHeaderCell = headerCellReference,
					DstCell = this
				});
				if (headerCellReference._Table == _Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (Cell headerCellWildcardReference in references.HeaderCellWildcardReferences)
			{
				table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.HeaderCellWildcard_Cell,
					SrcHeaderCell = headerCellWildcardReference,
					DstCell = this
				});
				if (headerCellWildcardReference._Table == _Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
		}
		catch (FormulaException)
		{
		}
	}

	internal void UpdateHeaderCellDependencies()
	{
		Table table = Row.Table;
		table._formulaTriggers.RemoveWhere((FormulaTrigger ft) => ft.DstHeaderCell == this);
		if (!HasHeaderFormula)
		{
			return;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(table.Project);
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(HeaderFormula);
			FormulaReferences references = formulaEvaluator.GetReferences(resolver);
			foreach (Cell cellReference in references.CellReferences)
			{
				_Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.Cell_HeaderCell,
					SrcCell = cellReference,
					DstHeaderCell = this
				});
			}
			foreach (RangeOperand rangeReference in references.RangeReferences)
			{
				table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.Range_HeaderCell,
					SrcRange = rangeReference,
					DstHeaderCell = this
				});
			}
			foreach (Column columnWildcardReference in references.ColumnWildcardReferences)
			{
				table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.ColumnWildcard_HeaderCell,
					SrcColumn = columnWildcardReference,
					DstHeaderCell = this
				});
			}
			foreach (Column columnReference in references.ColumnReferences)
			{
				table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.Column_HeaderCell,
					SrcColumn = columnReference,
					DstHeaderCell = this
				});
			}
			foreach (Cell headerCellReference in references.HeaderCellReferences)
			{
				_Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.HeaderCell_HeaderCell,
					SrcHeaderCell = headerCellReference,
					DstHeaderCell = this
				});
			}
			foreach (Cell headerCellWildcardReference in references.HeaderCellWildcardReferences)
			{
				_Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = _Table,
					Kind = FormulaTriggerKind.HeaderCellWildcard_HeaderCell,
					SrcHeaderCell = headerCellWildcardReference,
					DstHeaderCell = this
				});
			}
		}
		catch (FormulaException)
		{
		}
	}

	public void UpdateFormula(string text)
	{
		NeedSave = true;
		Formula = text;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsFormulaDirty = true;
		}
		ApplyFormula();
		UpdateDependencies();
		_Table.Project.FormulaManager.ReplaceHostCell(new FormulaRecord
		{
			TableId = _Table.Id,
			ObjectId = Id,
			Formula = Formula
		});
		_Table.NeedSave = true;
		_Table.Project.FormulaMapDirty = true;
	}

	public void UpdateHeaderFormula(string text)
	{
		NeedSave = true;
		HeaderFormula = text;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsHeaderFormulaDirty = true;
		}
		ApplyHeaderFormula(evalLqDistinct: true);
		UpdateHeaderCellDependencies();
		_Table.Project.FormulaManager.ReplaceHostHeaderCell(new FormulaRecord
		{
			TableId = _Table.Id,
			ObjectId = Id,
			Formula = Formula
		});
		_Table.NeedSave = true;
		_Table.Project.FormulaMapDirty = true;
	}

	public void UpdateCollectSource(string cs)
	{
		NeedSave = true;
		CollectSource = cs;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsCollectSourceDirty = true;
		}
		_Table.NeedSave = true;
	}

	public void ChangeDataType(Type t)
	{
		UpdateValue(ChangeDataTypeImpl(Value, t));
	}

	public static object ChangeDataTypeImpl(object Value, Type t)
	{
		try
		{
			ValueOperand valueOperand = ValueOperand.FromObject(Value);
			object result = string.Empty;
			if (t == typeof(string))
			{
				result = valueOperand.ToString();
			}
			else if (t == typeof(DateTime) && !Value.Equals(""))
			{
				result = valueOperand.ToDate().Value;
			}
			else if (t == typeof(double))
			{
				result = valueOperand.ToNumber().Value;
			}
			else if (t == typeof(bool))
			{
				result = valueOperand.ToBool().Value;
			}
			else if (t == typeof(TimeSpan) && !Value.Equals(""))
			{
				result = valueOperand.ToTime().Value;
			}
			else if (t == typeof(DateYearMonth) && !Value.Equals(""))
			{
				result = valueOperand.ToDateYearMonth().Value;
			}
			return result;
		}
		catch
		{
			return string.Empty;
		}
	}

	public static ValueOperand ChangeToValueOperand(object Value, Type t)
	{
		try
		{
			ValueOperand valueOperand = ValueOperand.FromObject(Value);
			if (t == typeof(string))
			{
				return valueOperand.ToString();
			}
			if (t == typeof(DateTime) && !Value.Equals(""))
			{
				return valueOperand.ToDate();
			}
			if (t == typeof(double))
			{
				return valueOperand.ToNumber();
			}
			if (t == typeof(bool))
			{
				return valueOperand.ToBool();
			}
			if (t == typeof(TimeSpan) && !Value.Equals(""))
			{
				return valueOperand.ToTime();
			}
			if (t == typeof(DateYearMonth) && !Value.Equals(""))
			{
				return valueOperand.ToDateYearMonth();
			}
			return string.Empty;
		}
		catch
		{
			return string.Empty;
		}
	}

	public BinaryValue GetBinaryValue()
	{
		BinaryValue result = BinaryValue.FromObject(Value);
		result.SetAdditionalData(CellPrivateData.GetBytes(_cellPrivateData, this));
		return result;
	}

	public Leqisoft.DTO.Cell ToDto()
	{
		BinaryValue value = BinaryValue.FromObject(Value);
		value.SetAdditionalData(CellPrivateData.GetBytes(_cellPrivateData, this));
		return new Leqisoft.DTO.Cell
		{
			Id = Id,
			Dirty = Dirty.ToInt(),
			Status = (int)Status,
			ColumnId = Column.Id,
			RowId = Row.Id,
			Value = value,
			Formula = Formula,
			StyleId = Style?.Id,
			CollectSource = CollectSource,
			HeaderFormula = HeaderFormula
		};
	}

	public override string ToString()
	{
		return $"({Row.Index},{Column.Index}) {Value}";
	}

	public void TryApplyFormula()
	{
		try
		{
			ApplyFormula();
		}
		catch (FormulaException)
		{
		}
		catch (Exception)
		{
		}
	}

	public void ApplyFormula(bool isIgnoreColSheetBadRef = true)
	{
		if (HasFormula)
		{
			Table table = Row.Table;
			FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(table.Project);
			FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
			{
				Resolver = resolver,
				RowIndex = Row.Index,
				HostTable = table,
				RefManager = table.Project.DataReferenceManager,
				IsIgnoreColSheetFunBadRefrence = isIgnoreColSheetBadRef,
				RefEvalContext = new DataReferenceEvaluationContext
				{
					Project = table.Project,
					CurrentTreeNode = table.TreeNode
				}
			};
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Formula)
			{
				Env = env
			};
			Operand operand = formulaEvaluator.EvaluateToOperand();
			UpdateValue(operand.Evaluate());
		}
	}

	public int GetHeaderLastRow()
	{
		IEnumerable<int> source = from r in _Table.HeaderRowCache
			select r.Index into i
			where i > Row.Index
			select i;
		if (source.Any())
		{
			return source.Min() - 1;
		}
		return _Table.Rows.Count - 1;
	}

	[IteratorStateMachine(typeof(_003CGetCells_003Ed__133))]
	public IEnumerable<Cell> GetCells()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetCells_003Ed__133(-2)
		{
			_003C_003E4__this = this
		};
	}

	public bool TryGetHeaderCellFormulaCell(out Cell headerCell)
	{
		headerCell = null;
		if (Row.Role == RowRole.Header || Row.Role == RowRole.Fixed)
		{
			return false;
		}
		List<int> source = (from r in _Table.HeaderRowCache
			select r.Index into i
			where i < Row.Index
			select i).ToList();
		if (source.Any())
		{
			int num = source.Max();
			if (_Table.Rows[num].Role == RowRole.Header)
			{
				headerCell = _Table[num, Column.Index];
				return headerCell.HasHeaderFormula;
			}
			return false;
		}
		return false;
	}

	public void TryApplyHeaderFormula(bool evalLqDistinct = true)
	{
		try
		{
			ApplyHeaderFormula(evalLqDistinct);
		}
		catch (FormulaException)
		{
		}
	}

	public void ApplyHeaderFormula(bool evalLqDistinct)
	{
		if (!HasHeaderFormula)
		{
			return;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(_Table.Project);
		FormulaEvaluationEnvironment formulaEvaluationEnvironment = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RefManager = _Table.Project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = _Table.Project,
				CurrentTreeNode = _Table.TreeNode
			}
		};
		FormulaEvaluator formulaEvaluator = new FormulaEvaluator(HeaderFormula)
		{
			Env = formulaEvaluationEnvironment
		};
		IsFillFormula isFillFormula = formulaEvaluator.IsFill();
		if (isFillFormula.IsFill)
		{
			if (!evalLqDistinct)
			{
				return;
			}
			ThrowIfSameRowHaveFillingFormula();
			Operand operand = formulaEvaluator.EvaluateToOperand();
			if (operand is ValueSetOperand { Set: var set })
			{
				if (set == null)
				{
					return;
				}
				HashSet<object> hashSet = new HashSet<object>();
				for (int num = GetHeaderLastRow(); num >= Row.Index + 1; num--)
				{
					Row row = _Table.Rows[num];
					if (row.Role != RowRole.Subtotal && row.Role != RowRole.Total && row.Role != RowRole.Fixed)
					{
						object value = _Table[num, Column.Index].Value;
						if (hashSet.Contains(value))
						{
							row.Remove();
						}
						else
						{
							hashSet.Add(value);
						}
					}
				}
				List<Tuple<Row, ValueOperand>> list = (from i in Enumerable.Range(Row.Index + 1, GetHeaderLastRow() - Row.Index).Where(delegate(int i)
					{
						Row row3 = _Table.Rows[i];
						return row3.Role == RowRole.Normal || row3.Role == RowRole.Among || row3.Role == RowRole.Minus;
					})
					select Tuple.Create(_Table.Rows[i], ValueOperand.FromObject(_Table[i, Column.Index].Value))).ToList();
				HashSet<Tuple<Row, ValueOperand>> hashSet2 = new HashSet<Tuple<Row, ValueOperand>>(list, Tuple2Item2Comparer.Instance);
				HashSet<object> nowValues = new HashSet<object>(set.Select((Tuple<Row, ValueOperand> tup) => tup.Item2));
				IEnumerable<Tuple<Row, ValueOperand>> source = list.Where((Tuple<Row, ValueOperand> tup) => !nowValues.Contains(tup.Item2));
				List<Row> list2 = new List<Row>();
				if (set.Count == 0)
				{
					set.Add(Tuple.Create<Row, ValueOperand>(null, ValueOperand.FromObject(string.Empty)));
				}
				else
				{
					set.ExceptWith(list);
				}
				Row row2 = _Table.Rows.Skip(Row.Index + 1).Take(GetHeaderLastRow() - Row.Index).LastOrDefault((Row r) => r.Role == RowRole.Total);
				foreach (Tuple<Row, ValueOperand> item in set)
				{
					int num2 = row2?.Index ?? (GetHeaderLastRow() + 1);
					_Table.Rows.Insert(num2, 1);
					_Table[num2, Column.Index].UpdateValue(item.Item2.Evaluate());
				}
				{
					foreach (Tuple<Row, ValueOperand> item2 in source.Reverse())
					{
						item2.Item1.Remove();
					}
					return;
				}
			}
			CrossTableOperand crossTableOperand = operand as CrossTableOperand;
			if (crossTableOperand != null)
			{
				return;
			}
			throw new FormulaTypeMismatchException();
		}
		Operand operand2 = formulaEvaluator.EvaluateToOperand();
		if (operand2 is CellsOperand { IsCollectFill: not false } cellsOperand)
		{
			if (!evalLqDistinct)
			{
				return;
			}
			if (cellsOperand.Cells.Count == 0)
			{
				cellsOperand.Cells.Add(new Cell
				{
					Value = string.Empty
				});
			}
			int count = cellsOperand.Cells.Count;
			int headerLastRow = GetHeaderLastRow();
			List<Row> list3 = (from r in _Table.Rows.Skip(Row.Index + 1).Take(headerLastRow - Row.Index)
				where r.Role == RowRole.Header || r.Role == RowRole.Subtotal || r.Role == RowRole.Total || r.Role == RowRole.Fixed
				select r).ToList();
			int num3 = headerLastRow + 1;
			foreach (Row item3 in Enumerable.Reverse(list3))
			{
				_Table.Rows.Move(item3.Index, 1, num3);
				num3--;
			}
			int num4 = headerLastRow - Row.Index - list3.Count;
			if (count > num4)
			{
				_Table.Rows.Insert(num3, count - num4);
			}
			else if (count < num4)
			{
				_Table.Rows.Remove(Row.Index + 1 + count, num4 - count);
			}
			for (int j = 0; j < count; j++)
			{
				Cell cell = _Table[j + Row.Index + 1, Column.Index];
				cell.UpdateValue(cellsOperand.Cells[j].Value);
			}
		}
		else
		{
			int index = Column.Index;
			int headerLastRow2 = GetHeaderLastRow();
			for (int k = Row.Index + 1; k <= headerLastRow2; k++)
			{
				Cell cell2 = _Table[k, index];
				formulaEvaluationEnvironment.RowIndex = k;
				formulaEvaluationEnvironment.HostTable = _Table;
				cell2.UpdateValue(formulaEvaluator.Evaluate());
			}
		}
	}

	public void TryApplyHeaderFormulaToRows(List<int> rows)
	{
		if (!HasHeaderFormula)
		{
			return;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(_Table.Project);
		FormulaEvaluationEnvironment formulaEvaluationEnvironment = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RefManager = _Table.Project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = _Table.Project,
				CurrentTreeNode = _Table.TreeNode
			}
		};
		try
		{
			int index = Column.Index;
			int count = rows.Count;
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(HeaderFormula)
			{
				Env = formulaEvaluationEnvironment
			};
			for (int i = 0; i < count; i++)
			{
				Cell cell = _Table[rows[i], index];
				formulaEvaluationEnvironment.RowIndex = rows[i];
				formulaEvaluationEnvironment.HostTable = _Table;
				cell.UpdateValue(formulaEvaluator.Evaluate());
			}
		}
		catch (FormulaException)
		{
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private void ThrowIfSameRowHaveFillingFormula()
	{
		foreach (Column column in _Table.Columns)
		{
			if (column == Column)
			{
				continue;
			}
			Cell cell = _Table[Row.Index, column.Index];
			if (HasHeaderFormula)
			{
				bool isFill;
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.HeaderFormula);
					isFill = formulaEvaluator.IsFill().IsFill;
				}
				catch (FormulaException)
				{
					break;
				}
				if (isFill)
				{
					throw new FormulaNotApplicableException("同一浮动行内最多只能有一列填充型公式");
				}
			}
		}
	}

	public void SetSynced()
	{
		if (Status == SyncStatus.New)
		{
			Status = SyncStatus.Synced;
			NeedSave = true;
		}
		if (Dirty.AnySet())
		{
			Dirty = default(CellDirtyMask);
			NeedSave = true;
		}
	}

	public void ForceTagValueAndFormulaDirty()
	{
		Dirty.IsValueDirty = true;
		Dirty.IsFormulaDirty = true;
	}

	public bool ShouldApplyColumnFormula()
	{
		if (Row.Role != RowRole.Header)
		{
			return Row.Role != RowRole.Fixed;
		}
		return false;
	}

	public bool HasColumnFormula()
	{
		if (ShouldApplyColumnFormula())
		{
			return Column.HasFormula;
		}
		return false;
	}

	public Cell GetMergeTopLeftCell()
	{
		return _Table.MergedCells.FirstOrDefault((CellMerge m) => m.Contains(this))?.TopLeft;
	}

	internal Cell Duplicate()
	{
		return new Cell
		{
			Formula = string.Empty,
			Id = Project.Current.GetNextId(),
			NeedSave = true,
			Status = SyncStatus.New,
			Value = Value,
			HeaderFormula = string.Empty,
			CollectSource = string.Empty
		};
	}

	public string GetUniqueFormulaName()
	{
		int num = 1;
		int num2 = 0;
		string formulaCanonicalCaption = Column.GetFormulaCanonicalCaption(GetDisplayValue());
		foreach (Column column in _Table.Columns)
		{
			string formulaCanonicalCaption2 = Column.GetFormulaCanonicalCaption(column.CaptionDisplay);
			if (formulaCanonicalCaption2 == formulaCanonicalCaption)
			{
				num2++;
			}
		}
		foreach (Row item in from r in _Table.HeaderRowCache
			where r.Role == RowRole.Header
			orderby r.Index
			select r)
		{
			for (int i = 0; i < _Table.Columns.Count; i++)
			{
				string formulaCanonicalCaption3 = Column.GetFormulaCanonicalCaption(_Table[item.Index, i].GetDisplayValue());
				if (formulaCanonicalCaption3 == formulaCanonicalCaption)
				{
					num2++;
				}
				if (_Table[item.Index, i] == this)
				{
					num = num2;
				}
			}
		}
		if (num2 != 1)
		{
			return $"{formulaCanonicalCaption}_{num}";
		}
		return formulaCanonicalCaption;
	}

	public void SortAscending()
	{
		Row row2 = _Table.Rows.Skip(Row.Index + 1).FirstOrDefault((Row r) => r.Role == RowRole.Normal || r.Role == RowRole.Minus || r.Role == RowRole.Among);
		if (row2 != null)
		{
			int index = row2.Index;
			int num = _Table.Rows.Skip(index + 1).FirstOrDefault((Row r) => r.Role == RowRole.Fixed || r.Role == RowRole.Header || r.Role == RowRole.Subtotal || r.Role == RowRole.Total)?.Index ?? _Table.Rows.Count;
			List<int> pickupOrder = (from r in _Table.Rows.Skip(index).Take(num - index).OrderByCellValue((Row row) => _Table[row.Index, Column.Index].Value)
				select r.Index).ToList();
			_Table.Rows.Reorder(index, pickupOrder);
		}
	}

	public void SortDescending()
	{
		Row row2 = _Table.Rows.Skip(Row.Index + 1).FirstOrDefault((Row r) => r.Role == RowRole.Normal || r.Role == RowRole.Minus || r.Role == RowRole.Among);
		if (row2 != null)
		{
			int index = row2.Index;
			int num = _Table.Rows.Skip(index + 1).FirstOrDefault((Row r) => r.Role == RowRole.Fixed || r.Role == RowRole.Header || r.Role == RowRole.Subtotal || r.Role == RowRole.Total)?.Index ?? _Table.Rows.Count;
			List<int> pickupOrder = (from r in _Table.Rows.Skip(index).Take(num - index).OrderByCellValueDescending((Row row) => _Table[row.Index, Column.Index].Value)
				select r.Index).ToList();
			_Table.Rows.Reorder(index, pickupOrder);
		}
	}

	public static double ToDoubleOr0(object o)
	{
		if (o is double)
		{
			return (double)o;
		}
		return 0.0;
	}

	public double ValueToDoubleOr0()
	{
		return ToDoubleOr0(Value);
	}

	public Cell Clone()
	{
		return (Cell)MemberwiseClone();
	}
}
