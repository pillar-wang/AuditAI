using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Auditai.DTO;

namespace Auditai.Model;

public class ExcelRewriter : FormulaBaseListener
{
	private TokenStreamRewriter _rewriter;

	private readonly ExcelExporterContext _context;

	private int _sumFunDepth;

	public ExcelRewriter(TokenStreamRewriter rewriter, ExcelExporterContext context)
	{
		_rewriter = rewriter;
		_context = context;
	}

	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Cell cell = _context.Resolver.ResolveTableCell(arg, arg2);
		Tuple<int, int> tuple = _context.CellOffset(cell);
		int num = cell.Row.Index + tuple.Item1;
		int num2 = cell.Column.Index + tuple.Item2;
		string text = $"{ColumnIndexToColumnLetter(num2 + 1)}{num + 1}";
		if (cell._Table != _context.CurrentTable)
		{
			string text2 = _context.TablePathMapper(cell._Table);
			text = "'" + text2 + "'!" + text;
		}
		else if (_sumFunDepth > 0)
		{
			Tuple<int, int> tuple2 = _context.CellOffset(_context.CurrentCell);
			int num3 = _context.CurrentCell.Row.Index + tuple2.Item1;
			int num4 = _context.CurrentCell.Column.Index + tuple2.Item2;
			if (num3 == num && num4 == num2)
			{
				throw new FormulaNotApplicableException("");
			}
		}
		_rewriter.Replace(context.Start, context.Stop, text);
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Column column = _context.Resolver.ResolveTableColumn(arg, arg2);
		Cell cell = column.Table[_context.CurrentRowIndex, column.Index];
		Tuple<int, int> tuple = _context.CellOffset(cell);
		int num = cell.Row.Index + tuple.Item1;
		int num2 = cell.Column.Index + tuple.Item2;
		string text = $"{ColumnIndexToColumnLetter(num2 + 1)}{num + 1}";
		if (cell._Table != _context.CurrentTable)
		{
			string text2 = _context.TablePathMapper(cell._Table);
			text = "'" + text2 + "'!" + text;
		}
		else if (_sumFunDepth > 0)
		{
			Tuple<int, int> tuple2 = _context.CellOffset(_context.CurrentCell);
			int num3 = _context.CurrentCell.Row.Index + tuple2.Item1;
			int num4 = _context.CurrentCell.Column.Index + tuple2.Item2;
			if (num3 == num && num4 == num2)
			{
				throw new FormulaNotApplicableException("");
			}
		}
		_rewriter.Replace(context.Start, context.Stop, text);
	}

	private bool IsBetweenValue(int start, int end, int checkValue)
	{
		if (start < end)
		{
			if (checkValue >= start)
			{
				return checkValue <= end;
			}
			return false;
		}
		if (checkValue >= end)
		{
			return checkValue <= start;
		}
		return false;
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Id64 arg3 = Id64.Parse(context.Int(2).GetText());
		Cell cell = _context.Resolver.ResolveTableCell(arg, arg2);
		Cell cell2 = _context.Resolver.ResolveTableCell(arg, arg3);
		Tuple<int, int> tuple = _context.CellOffset(cell);
		int num = cell.Row.Index + tuple.Item1;
		int num2 = cell.Column.Index + tuple.Item2;
		Tuple<int, int> tuple2 = _context.CellOffset(cell2);
		int num3 = cell2.Row.Index + tuple2.Item1;
		int num4 = cell2.Column.Index + tuple2.Item2;
		string text = $"{ColumnIndexToColumnLetter(num2 + 1)}{num + 1}:{ColumnIndexToColumnLetter(num4 + 1)}{num3 + 1}";
		if (cell._Table != _context.CurrentTable)
		{
			string text2 = _context.TablePathMapper(cell._Table);
			text = "'" + text2 + "'!" + text;
		}
		else if (_sumFunDepth > 0)
		{
			Tuple<int, int> tuple3 = _context.CellOffset(_context.CurrentCell);
			int checkValue = _context.CurrentCell.Row.Index + tuple3.Item1;
			int checkValue2 = _context.CurrentCell.Column.Index + tuple3.Item2;
			if (IsBetweenValue(num, num3, checkValue) && IsBetweenValue(num2, num4, checkValue2))
			{
				throw new FormulaNotApplicableException("");
			}
		}
		_rewriter.Replace(context.Start, context.Stop, text);
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Column column = _context.Resolver.ResolveTableColumn(arg, arg2);
		string text = ColumnIndexToColumnLetter(column.Index + 1);
		string text2 = text + ":" + text;
		if (column.Table != _context.CurrentTable)
		{
			string text3 = _context.TablePathMapper(column.Table);
			text2 = "'" + text3 + "'!" + text2;
		}
		else if (_sumFunDepth > 0)
		{
			if (column.Index == _context.CurrentCell.Column.Index)
			{
				throw new FormulaNotApplicableException("");
			}
			if (_context.DataRowsCount == 0)
			{
				throw new FormulaNotApplicableException("");
			}
			text2 = $"{text}{_context.DataRowStartIndex + 1}:{text}{_context.DataRowStartIndex + _context.DataRowsCount}";
		}
		_rewriter.Replace(context.Start, context.Stop, text2);
	}

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		if ("Sum".Equals(context.FuncName().GetText(), StringComparison.OrdinalIgnoreCase))
		{
			_sumFunDepth++;
			if (_sumFunDepth > 1)
			{
				throw new FormulaNotApplicableException("");
			}
			return;
		}
		throw new FormulaNotApplicableException("");
	}

	public override void ExitFunc([NotNull] FormulaParser.FuncContext context)
	{
		if ("Sum".Equals(context.FuncName().GetText(), StringComparison.OrdinalIgnoreCase))
		{
			_sumFunDepth--;
		}
	}

	private static string ColumnIndexToColumnLetter(int colIndex)
	{
		int num = colIndex;
		string text = string.Empty;
		while (num > 0)
		{
			int num2 = (num - 1) % 26;
			text = (char)(65 + num2) + text;
			num = (num - num2) / 26;
		}
		return text;
	}
}
