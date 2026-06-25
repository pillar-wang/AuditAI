using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

internal class FormulaTrigger
{
	public Table Table { get; set; }

	public Cell SrcCell { get; set; }

	public RangeOperand SrcRange { get; set; }

	public Column SrcColumn { get; set; }

	public Cell SrcHeaderCell { get; set; }

	public Cell DstCell { get; set; }

	public Column DstColumn { get; set; }

	public Cell DstHeaderCell { get; set; }

	public FormulaTriggerKind Kind { get; set; }

	public void Execute(IEnumerable<Cell> valueChanged)
	{
		int dstCellRowEnd;
		int lastRow;
		switch (Kind)
		{
		case FormulaTriggerKind.Cell_Cell:
			if (!valueChanged.Contains(SrcCell))
			{
				break;
			}
			ThrowIfCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstCell))
				{
					DstCell.TryApplyFormula();
					Table._formulaExecuted.Add(DstCell);
				}
			}
			else
			{
				DstCell.TryApplyFormula();
			}
			break;
		case FormulaTriggerKind.Cell_Column:
			if (!valueChanged.Contains(SrcCell))
			{
				break;
			}
			ThrowIfColumnNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstColumn))
				{
					DstColumn.TryApplyFormula(rethrow: false, evalLqDistinct: false);
					Table._formulaExecuted.Add(DstColumn);
				}
			}
			else
			{
				DstColumn.TryApplyFormula(rethrow: false, evalLqDistinct: false);
			}
			break;
		case FormulaTriggerKind.Range_Cell:
			if (!valueChanged.Any((Cell c) => SrcRange.TopLeft.Row.Index <= c.Row.Index && SrcRange.TopLeft.Column.Index <= c.Column.Index && c.Row.Index <= SrcRange.BottomRight.Row.Index && c.Column.Index <= SrcRange.BottomRight.Column.Index))
			{
				break;
			}
			ThrowIfCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstCell))
				{
					DstCell.TryApplyFormula();
					Table._formulaExecuted.Add(DstCell);
				}
			}
			else
			{
				DstCell.TryApplyFormula();
			}
			break;
		case FormulaTriggerKind.Range_Column:
			if (!valueChanged.Any((Cell c) => SrcRange.TopLeft.Row.Index <= c.Row.Index && SrcRange.TopLeft.Column.Index <= c.Column.Index && c.Row.Index <= SrcRange.BottomRight.Row.Index && c.Column.Index <= SrcRange.BottomRight.Column.Index))
			{
				break;
			}
			ThrowIfColumnNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstColumn))
				{
					DstColumn.TryApplyFormula(rethrow: false, evalLqDistinct: false);
					Table._formulaExecuted.Add(DstColumn);
				}
			}
			else
			{
				DstColumn.TryApplyFormula(rethrow: false, evalLqDistinct: false);
			}
			break;
		case FormulaTriggerKind.ColumnWildcard_Cell:
			if (!valueChanged.Any((Cell c) => c.Column == SrcColumn && c.Row == DstCell.Row))
			{
				break;
			}
			ThrowIfCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstCell))
				{
					DstCell.TryApplyFormula();
					Table._formulaExecuted.Add(DstCell);
				}
			}
			else
			{
				DstCell.TryApplyFormula();
			}
			break;
		case FormulaTriggerKind.ColumnWildcard_Column:
		{
			List<int> rows = (from c in valueChanged
				where c.Column == SrcColumn
				select c.Row.Index).ToList();
			ThrowIfColumnNotExists();
			DstColumn.TryApplyFormulaToRows(rows);
			break;
		}
		case FormulaTriggerKind.Column_Cell:
			if (!valueChanged.Any((Cell c) => c.Column == SrcColumn))
			{
				break;
			}
			ThrowIfCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstCell))
				{
					DstCell.TryApplyFormula();
					Table._formulaExecuted.Add(DstCell);
				}
			}
			else
			{
				DstCell.TryApplyFormula();
			}
			break;
		case FormulaTriggerKind.Column_Column:
			if (!valueChanged.Any((Cell c) => c.Column == SrcColumn))
			{
				break;
			}
			ThrowIfColumnNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstColumn))
				{
					DstColumn.TryApplyFormula(rethrow: false, evalLqDistinct: false);
					Table._formulaExecuted.Add(DstColumn);
				}
			}
			else
			{
				DstColumn.TryApplyFormula(rethrow: false, evalLqDistinct: false);
			}
			break;
		case FormulaTriggerKind.Cell_HeaderCell:
			if (!valueChanged.Contains(SrcCell))
			{
				break;
			}
			ThrowIfHeaderCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstHeaderCell))
				{
					DstHeaderCell.TryApplyHeaderFormula(evalLqDistinct: false);
					Table._formulaExecuted.Add(DstHeaderCell);
				}
			}
			else
			{
				DstHeaderCell.TryApplyHeaderFormula(evalLqDistinct: false);
			}
			break;
		case FormulaTriggerKind.Range_HeaderCell:
			if (!valueChanged.Any((Cell c) => SrcRange.TopLeft.Row.Index <= c.Row.Index && SrcRange.TopLeft.Column.Index <= c.Column.Index && c.Row.Index <= SrcRange.BottomRight.Row.Index && c.Column.Index <= SrcRange.BottomRight.Column.Index))
			{
				break;
			}
			ThrowIfHeaderCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstHeaderCell))
				{
					DstHeaderCell.TryApplyHeaderFormula(evalLqDistinct: false);
					Table._formulaExecuted.Add(DstHeaderCell);
				}
			}
			else
			{
				DstHeaderCell.TryApplyHeaderFormula(evalLqDistinct: false);
			}
			break;
		case FormulaTriggerKind.Column_HeaderCell:
			if (!valueChanged.Any((Cell c) => c.Column == SrcColumn))
			{
				break;
			}
			ThrowIfHeaderCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstHeaderCell))
				{
					DstHeaderCell.TryApplyHeaderFormula(evalLqDistinct: false);
					Table._formulaExecuted.Add(DstHeaderCell);
				}
			}
			else
			{
				DstHeaderCell.TryApplyHeaderFormula(evalLqDistinct: false);
			}
			break;
		case FormulaTriggerKind.ColumnWildcard_HeaderCell:
		{
			dstCellRowEnd = DstHeaderCell.GetHeaderLastRow();
			List<int> rows = (from c in valueChanged
				where c.Column == SrcColumn
				select c.Row.Index into i
				where i > DstHeaderCell.Row.Index && i <= dstCellRowEnd
				select i).ToList();
			ThrowIfHeaderCellNotExists();
			DstHeaderCell.TryApplyHeaderFormulaToRows(rows);
			break;
		}
		case FormulaTriggerKind.HeaderCell_Cell:
			lastRow = SrcHeaderCell.GetHeaderLastRow();
			if (!valueChanged.Any((Cell c) => c.Column == SrcHeaderCell.Column && c.Row.Index > SrcHeaderCell.Row.Index && c.Row.Index <= lastRow))
			{
				break;
			}
			ThrowIfCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstCell))
				{
					DstCell.TryApplyFormula();
					Table._formulaExecuted.Add(DstCell);
				}
			}
			else
			{
				DstCell.TryApplyFormula();
			}
			break;
		case FormulaTriggerKind.HeaderCell_Column:
			lastRow = SrcHeaderCell.GetHeaderLastRow();
			if (!valueChanged.Any((Cell c) => c.Column == SrcHeaderCell.Column && c.Row.Index > SrcHeaderCell.Row.Index && c.Row.Index <= lastRow))
			{
				break;
			}
			ThrowIfColumnNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstColumn))
				{
					DstColumn.TryApplyFormula(rethrow: false, evalLqDistinct: false);
					Table._formulaExecuted.Add(DstColumn);
				}
			}
			else
			{
				DstColumn.TryApplyFormula(rethrow: false, evalLqDistinct: false);
			}
			break;
		case FormulaTriggerKind.HeaderCell_HeaderCell:
			lastRow = SrcHeaderCell.GetHeaderLastRow();
			if (!valueChanged.Any((Cell c) => c.Column == SrcHeaderCell.Column && c.Row.Index > SrcHeaderCell.Row.Index && c.Row.Index <= lastRow))
			{
				break;
			}
			ThrowIfHeaderCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstHeaderCell))
				{
					DstHeaderCell.TryApplyHeaderFormula(evalLqDistinct: false);
					Table._formulaExecuted.Add(DstHeaderCell);
				}
			}
			else
			{
				DstHeaderCell.TryApplyHeaderFormula(evalLqDistinct: false);
			}
			break;
		case FormulaTriggerKind.HeaderCellWildcard_Cell:
			if (!valueChanged.Any((Cell c) => c.Column == SrcHeaderCell.Column && c.Row == DstCell.Row))
			{
				break;
			}
			ThrowIfCellNotExists();
			if (Table._isBatchUpdating)
			{
				if (!Table._formulaExecuted.Contains(DstCell))
				{
					DstCell.TryApplyFormula();
					Table._formulaExecuted.Add(DstCell);
				}
			}
			else
			{
				DstCell.TryApplyFormula();
			}
			break;
		case FormulaTriggerKind.HeaderCellWildcard_Column:
		{
			List<int> rows = (from c in valueChanged
				where c.Column == SrcHeaderCell.Column
				select c.Row.Index).ToList();
			ThrowIfColumnNotExists();
			DstColumn.TryApplyFormulaToRows(rows);
			break;
		}
		case FormulaTriggerKind.HeaderCellWildcard_HeaderCell:
		{
			dstCellRowEnd = DstHeaderCell.GetHeaderLastRow();
			List<int> rows = (from c in valueChanged
				where c.Column == SrcHeaderCell.Column
				select c.Row.Index into i
				where i > DstHeaderCell.Row.Index && i <= dstCellRowEnd
				select i).ToList();
			ThrowIfHeaderCellNotExists();
			DstHeaderCell.TryApplyHeaderFormulaToRows(rows);
			break;
		}
		}
		void ThrowIfCellNotExists()
		{
			if (!DstCell._Table.Cells._list.Contains(DstCell))
			{
				throw new FormulaBadReferenceException();
			}
		}
		void ThrowIfColumnNotExists()
		{
			if (!DstColumn.Table.Columns._list.Contains(DstColumn))
			{
				throw new FormulaBadReferenceException();
			}
		}
		void ThrowIfHeaderCellNotExists()
		{
			if (!DstHeaderCell._Table.Cells._list.Contains(DstHeaderCell))
			{
				throw new FormulaBadReferenceException();
			}
		}
	}
}
