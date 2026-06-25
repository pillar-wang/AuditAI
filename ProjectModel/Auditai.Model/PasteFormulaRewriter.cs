using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Auditai.DTO;

namespace Auditai.Model;

public class PasteFormulaRewriter : FormulaBaseListener
{
	private TokenStreamRewriter _rewriter;

	private FormulaReferenceResolver _resolver;

	private Cell _source;

	private Cell _dest;

	private int _offsetX;

	private int _offsetY;

	private bool CrossTable { get; }

	public PasteFormulaRewriter(TokenStreamRewriter rewriter, FormulaReferenceResolver resolver, Cell source, Cell dest)
	{
		_rewriter = rewriter;
		_resolver = resolver;
		_source = source;
		_dest = dest;
		CrossTable = source._Table != dest._Table;
		_offsetX = dest.Column.Index - source.Column.Index;
		_offsetY = dest.Row.Index - source.Row.Index;
	}

	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Cell cell = _resolver.ResolveTableCell(arg, arg2);
		if (cell != null && cell._Table == _source._Table && !CrossTable)
		{
			try
			{
				Cell cell2 = cell._Table[cell.Row.Index + _offsetY, cell.Column.Index + _offsetX];
				_rewriter.Replace(context.Int(1).Symbol, cell2.Id.ToString());
			}
			catch (IndexOutOfRangeException)
			{
			}
		}
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Cell cell = _resolver.ResolveTableCell(arg, arg2);
		Id64 arg3 = Id64.Parse(context.Int(2).GetText());
		Cell cell2 = _resolver.ResolveTableCell(arg, arg3);
		if (cell != null && cell2 != null && cell._Table == _source._Table && !CrossTable)
		{
			try
			{
				Cell cell3 = cell._Table[cell.Row.Index + _offsetY, cell.Column.Index + _offsetX];
				_rewriter.Replace(context.Int(1).Symbol, cell3.Id.ToString());
				Cell cell4 = cell2._Table[cell2.Row.Index + _offsetY, cell2.Column.Index + _offsetX];
				_rewriter.Replace(context.Int(2).Symbol, cell4.Id.ToString());
			}
			catch (IndexOutOfRangeException)
			{
			}
		}
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Column column = _resolver.ResolveTableColumn(arg, arg2);
		if (column != null && column.Table == _source._Table && !CrossTable && column == _source.Column)
		{
			try
			{
				Column column2 = column.Table.Columns[column.Index + _offsetX];
				_rewriter.Replace(context.Int(1).Symbol, column2.Id.ToString());
			}
			catch (IndexOutOfRangeException)
			{
			}
		}
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Column column = _resolver.ResolveTableColumn(arg, arg2);
		if (column != null && column.Table == _source._Table && !CrossTable)
		{
			try
			{
				Column column2 = column.Table.Columns[column.Index + _offsetX];
				_rewriter.Replace(context.Int(1).Symbol, column2.Id.ToString());
			}
			catch (IndexOutOfRangeException)
			{
			}
		}
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Cell cell = _resolver.ResolveTableCell(arg, arg2);
		if (cell != null && cell._Table == _source._Table && !CrossTable)
		{
			try
			{
				Cell cell2 = cell._Table[cell.Row.Index + _offsetY, cell.Column.Index + _offsetX];
				_rewriter.Replace(context.Int(1).Symbol, cell2.Id.ToString());
			}
			catch (IndexOutOfRangeException)
			{
			}
		}
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Cell cell = _resolver.ResolveTableCell(arg, arg2);
		if (cell != null && cell._Table == _source._Table && !CrossTable)
		{
			try
			{
				Cell cell2 = cell._Table[cell.Row.Index + _offsetY, cell.Column.Index + _offsetX];
				_rewriter.Replace(context.Int(1).Symbol, cell2.Id.ToString());
			}
			catch (IndexOutOfRangeException)
			{
			}
		}
	}
}
