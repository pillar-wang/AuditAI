using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class PasteHeaderCellFormulaListener : FormulaBaseListener
{
	private class InnerListener : FormulaBaseListener
	{
		private PasteHeaderCellFormulaListener _owner;

		private FormulaParser.ExprContext _expr;

		public InnerListener(PasteHeaderCellFormulaListener owner, FormulaParser.ExprContext expr)
		{
			_owner = owner;
			_expr = expr;
		}

		public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
		{
			Id64 id = Id64.Parse(context.Int(0).GetText());
			Id64 id2 = Id64.Parse(context.Int(1).GetText());
			Table tableById = Project.Current.GetTableById(id);
			if (tableById != null)
			{
				string uniqueFormulaName = _owner._dest.GetUniqueFormulaName();
				Cell byCaption = tableById.Cells.GetByCaption(uniqueFormulaName);
				if (byCaption != null)
				{
					_owner._rewriter.Replace(context.Int(1).Symbol, byCaption.Id.ToString());
				}
			}
		}
	}

	private TokenStreamRewriter _rewriter;

	private FormulaReferenceResolver _resolver;

	private Cell _source;

	private Cell _dest;

	private int dontRewrite;

	private int _offset;

	private bool CrossTable { get; }

	public PasteHeaderCellFormulaListener(TokenStreamRewriter rewriter, FormulaReferenceResolver resolver, Cell source, Cell dest)
	{
		_rewriter = rewriter;
		_resolver = resolver;
		_source = source;
		_dest = dest;
		CrossTable = source._Table != dest._Table;
		if (!CrossTable)
		{
			_offset = dest.Column.Index - source.Column.Index;
		}
	}

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		string text = context.FuncName().GetText();
		if (string.Equals(text, "LqSumIf", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqVLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqDistinct", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqFilter", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqAsc", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqDesc", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "SumIf", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "VLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "Distinct", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "DistinctF", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "DistinctUp", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "DistinctDown", StringComparison.OrdinalIgnoreCase))
		{
			dontRewrite++;
		}
	}

	public override void ExitFunc([NotNull] FormulaParser.FuncContext context)
	{
		string text = context.FuncName().GetText();
		if (string.Equals(text, "LqSumIf", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "SumIf", StringComparison.OrdinalIgnoreCase))
		{
			dontRewrite--;
			InnerListener listener = new InnerListener(this, context.expr(1));
			ParseTreeWalker.Default.Walk(listener, context.expr(1));
		}
		else if (string.Equals(text, "LqVLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "VLookUp", StringComparison.OrdinalIgnoreCase))
		{
			dontRewrite--;
			for (int i = 0; i < context.expr().Length / 2; i++)
			{
				InnerListener listener2 = new InnerListener(this, context.expr(i * 2 + 1));
				ParseTreeWalker.Default.Walk(listener2, context.expr(i * 2 + 1));
			}
		}
		else if (string.Equals(text, "LqDistinct", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqFilter", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqAsc", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqDesc", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "Distinct", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "DistinctF", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "DistinctUp", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "DistinctDown", StringComparison.OrdinalIgnoreCase))
		{
			dontRewrite--;
			if (!CrossTable)
			{
				throw new FormulaException("同一表格内最多允许一列填充公式");
			}
		}
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		if (dontRewrite != 0)
		{
			return;
		}
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Column column = _resolver.ResolveTableColumn(arg, arg2);
		if (column == null || column.Table != _source._Table)
		{
			return;
		}
		if (CrossTable)
		{
			int num = column.Index - _source.Column.Index;
			try
			{
				Column column2 = _dest._Table.Columns[_dest.Column.Index + num];
				_rewriter.Replace(context.Int(0).Symbol, _dest._Table.Id.ToString());
				_rewriter.Replace(context.Int(1).Symbol, column2.Id.ToString());
				return;
			}
			catch (IndexOutOfRangeException)
			{
				return;
			}
		}
		try
		{
			Column column3 = column.Table.Columns[column.Index + _offset];
			_rewriter.Replace(context.Int(1).Symbol, column3.Id.ToString());
		}
		catch (IndexOutOfRangeException)
		{
		}
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		if (dontRewrite != 0)
		{
			return;
		}
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Column column = _resolver.ResolveTableColumn(arg, arg2);
		if (column == null || column.Table != _source._Table)
		{
			return;
		}
		if (CrossTable)
		{
			int num = column.Index - _source.Column.Index;
			try
			{
				Column column2 = _dest._Table.Columns[_dest.Column.Index + num];
				_rewriter.Replace(context.Int(0).Symbol, _dest._Table.Id.ToString());
				_rewriter.Replace(context.Int(1).Symbol, column2.Id.ToString());
				return;
			}
			catch (ArgumentOutOfRangeException)
			{
				return;
			}
		}
		try
		{
			Column column3 = column.Table.Columns[column.Index + _offset];
			_rewriter.Replace(context.Int(1).Symbol, column3.Id.ToString());
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		if (dontRewrite != 0)
		{
			return;
		}
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Cell cell = _resolver.ResolveTableCell(arg, arg2);
		if (cell == null || cell._Table != _source._Table)
		{
			return;
		}
		if (CrossTable)
		{
			int num = cell.Column.Index - _source.Column.Index;
			try
			{
				Cell cell2 = _dest._Table[_source.Row.Index, _dest.Column.Index + num];
				_rewriter.Replace(context.Int(0).Symbol, _dest._Table.Id.ToString());
				_rewriter.Replace(context.Int(1).Symbol, cell2.Id.ToString());
				return;
			}
			catch (IndexOutOfRangeException)
			{
				return;
			}
		}
		try
		{
			Cell cell3 = cell._Table[_source.Row.Index, cell.Column.Index + _offset];
			_rewriter.Replace(context.Int(1).Symbol, cell3.Id.ToString());
		}
		catch (IndexOutOfRangeException)
		{
		}
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		base.ExitRefHeaderCellWildcard(context);
	}
}
