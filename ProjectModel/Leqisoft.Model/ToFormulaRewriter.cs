using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Leqisoft.Model;

public class ToFormulaRewriter : FormulaDisplayParserBaseListener
{
	public const long PSEUDOCOL_ROW = long.MaxValue;

	protected readonly TokenStreamRewriter _rewriter;

	protected readonly FormulaContext _context;

	public ToFormulaRewriter(TokenStreamRewriter rewriter, FormulaContext context)
	{
		_rewriter = rewriter;
		_context = context;
	}

	public override void ExitTableCell([NotNull] FormulaDisplayParser.TableCellContext context)
	{
		string text = context.TableName().GetText();
		string text2 = context.columnName().GetText();
		int row = int.Parse(context.Int().GetText());
		Table table = _context.Project.GetTableByCanonicalName(text).LoadAndReturn();
		Cell cell = table.ResolveCell(text2, row);
		_rewriter.Replace(context.Start, context.Stop, $"[1:{table.Id}:{cell.Id}]");
	}

	public override void ExitColumnCell([NotNull] FormulaDisplayParser.ColumnCellContext context)
	{
		string text = context.columnName().GetText();
		int row = int.Parse(context.Int().GetText());
		Cell cell = _context.Table.ResolveCell(text, row);
		_rewriter.Replace(context.Start, context.Stop, $"[1:{_context.Table.Id}:{cell.Id}]");
	}

	public override void ExitTableColumn([NotNull] FormulaDisplayParser.TableColumnContext context)
	{
		string text = context.TableName().GetText();
		string text2 = context.columnName().GetText();
		if (_context.Kind == FormulaContextKind.LedgerCollectFormulaEdit)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals(_context.LegderVirtualTableSetting.GetBalanceVirtualTableName(), text))
			{
				long balanceVirtualTableColumnId = _context.LegderVirtualTableSetting.GetBalanceVirtualTableColumnId(text2);
				if (balanceVirtualTableColumnId == -1)
				{
					throw new FormulaBadReferenceException();
				}
				_rewriter.Replace(context.Start, context.Stop, $"[2:{_context.LegderVirtualTableSetting.GetBalanceVirtualTableId()}:{balanceVirtualTableColumnId}]");
				return;
			}
			if (StringComparer.OrdinalIgnoreCase.Equals(_context.LegderVirtualTableSetting.GetVoucherVirtualTableName(), text))
			{
				long voucherVirtualTableColumnId = _context.LegderVirtualTableSetting.GetVoucherVirtualTableColumnId(text2);
				if (voucherVirtualTableColumnId == -1)
				{
					throw new FormulaBadReferenceException();
				}
				_rewriter.Replace(context.Start, context.Stop, $"[2:{_context.LegderVirtualTableSetting.GetVoucherVirtualTableId()}:{voucherVirtualTableColumnId}]");
				return;
			}
		}
		Table table = _context.Project.GetTableByCanonicalName(text).LoadAndReturn();
		if (text2.Equals("Row", StringComparison.OrdinalIgnoreCase))
		{
			_rewriter.Replace(context.Start, context.Stop, $"[2:{table.Id}:{long.MaxValue}]");
			return;
		}
		Column byCaption = table.Columns.GetByCaption(text2);
		if (byCaption != null)
		{
			_rewriter.Replace(context.Start, context.Stop, $"[2:{table.Id}:{byCaption.Id}]");
			return;
		}
		Cell byCaption2 = table.Cells.GetByCaption(text2);
		_rewriter.Replace(context.Start, context.Stop, $"[6:{table.Id}:{byCaption2.Id}]");
	}

	public override void ExitColumn([NotNull] FormulaDisplayParser.ColumnContext context)
	{
		string text = context.columnName().GetText();
		if (text.Equals("Row", StringComparison.OrdinalIgnoreCase))
		{
			_rewriter.Replace(context.Start, context.Stop, $"[2:{_context.Table.Id}:{long.MaxValue}]");
			return;
		}
		Column byCaption = _context.Table.Columns.GetByCaption(text);
		if (byCaption != null)
		{
			_rewriter.Replace(context.Start, context.Stop, $"[2:{_context.Table.Id}:{byCaption.Id}]");
			return;
		}
		Cell byCaption2 = _context.Table.Cells.GetByCaption(text);
		_rewriter.Replace(context.Start, context.Stop, $"[6:{_context.Table.Id}:{byCaption2.Id}]");
	}

	public override void ExitTableColumnWildcard([NotNull] FormulaDisplayParser.TableColumnWildcardContext context)
	{
		string text = context.TableName().GetText();
		string text2 = context.columnName().GetText();
		Table table = _context.Project.GetTableByCanonicalName(text).LoadAndReturn();
		Column byCaption = table.Columns.GetByCaption(text2);
		if (byCaption != null)
		{
			_rewriter.Replace(context.Start, context.Stop, $"[4:{table.Id}:{byCaption.Id}]");
			return;
		}
		Cell byCaption2 = _context.Table.Cells.GetByCaption(text2);
		_rewriter.Replace(context.Start, context.Stop, $"[7:{table.Id}:{byCaption2.Id}]");
	}

	public override void ExitColumnWildcard([NotNull] FormulaDisplayParser.ColumnWildcardContext context)
	{
		string text = context.columnName().GetText();
		Column byCaption = _context.Table.Columns.GetByCaption(text);
		if (byCaption != null)
		{
			_rewriter.Replace(context.Start, context.Stop, $"[4:{_context.Table.Id}:{byCaption.Id}]");
			return;
		}
		Cell byCaption2 = _context.Table.Cells.GetByCaption(text);
		_rewriter.Replace(context.Start, context.Stop, $"[7:{_context.Table.Id}:{byCaption2.Id}]");
	}

	public override void ExitTableRange([NotNull] FormulaDisplayParser.TableRangeContext context)
	{
		string text = context.TableName().GetText();
		Table table = _context.Project.GetTableByCanonicalName(text).LoadAndReturn();
		string text2 = context.columnName(0).GetText();
		int row = int.Parse(context.Int(0).GetText());
		Cell cell = table.ResolveCell(text2, row);
		string text3 = context.columnName(1).GetText();
		int row2 = int.Parse(context.Int(1).GetText());
		Cell cell2 = table.ResolveCell(text3, row2);
		_rewriter.Replace(context.Start, context.Stop, $"[3:{table.Id}:{cell.Id}:{cell2.Id}]");
	}

	public override void ExitRange([NotNull] FormulaDisplayParser.RangeContext context)
	{
		string text = context.columnName(0).GetText();
		int row = int.Parse(context.Int(0).GetText());
		Cell cell = _context.Table.ResolveCell(text, row);
		string text2 = context.columnName(1).GetText();
		int row2 = int.Parse(context.Int(1).GetText());
		Cell cell2 = _context.Table.ResolveCell(text2, row2);
		_rewriter.Replace(context.Start, context.Stop, $"[3:{_context.Table.Id}:{cell.Id}:{cell2.Id}]");
	}

	public override void ExitTreeNode([NotNull] FormulaDisplayParser.TreeNodeContext context)
	{
		TreeNodeBase treeNodeByCanonicalName = _context.Project.GetTreeNodeByCanonicalName(context.TableName().GetText());
		_rewriter.Replace(context.Start, context.Stop, $"[5:{treeNodeByCanonicalName.Id}]");
	}

	public override void ExitFunc([NotNull] FormulaDisplayParser.FuncContext context)
	{
		string text = context.funcName().GetText();
		if ((text.Equals("TicketTitle", StringComparison.OrdinalIgnoreCase) || text.Equals("TicketFoot", StringComparison.OrdinalIgnoreCase)) && _context.Kind != FormulaContextKind.TicketDesign && _context.Kind != FormulaContextKind.TicketInput)
		{
			throw new FormulaFunctionNotExistException();
		}
		if (context.funcName().GetText().Equals("Title", StringComparison.OrdinalIgnoreCase) && context.expr().Length == 2)
		{
			_rewriter.InsertAfter(context.LParen().Symbol, $"[5:{_context.Table.Id}],");
		}
	}
}
