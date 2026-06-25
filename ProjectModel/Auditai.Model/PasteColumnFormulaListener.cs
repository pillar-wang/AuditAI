using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Auditai.DTO;

namespace Auditai.Model;

public class PasteColumnFormulaListener : FormulaBaseListener
{
	private class InnerListener : FormulaBaseListener
	{
		private PasteColumnFormulaListener _owner;

		private FormulaParser.ExprContext _expr;

		public InnerListener(PasteColumnFormulaListener owner, FormulaParser.ExprContext expr)
		{
			_owner = owner;
			_expr = expr;
		}

		public override void EnterRefColumn([NotNull] FormulaParser.RefColumnContext context)
		{
			Id64 id = Id64.Parse(context.Int(0).GetText());
			Id64 id2 = Id64.Parse(context.Int(1).GetText());
			Table tableById = Project.Current.GetTableById(id);
			if (tableById != null)
			{
				string uniqueFormulaName = _owner._dest.GetUniqueFormulaName();
				Column byCaption = tableById.Columns.GetByCaption(uniqueFormulaName);
				if (byCaption != null)
				{
					_owner._rewriter.Replace(context.Int(1).Symbol, byCaption.Id.ToString());
				}
			}
		}
	}

	private TokenStreamRewriter _rewriter;

	private FormulaReferenceResolver _resolver;

	private Column _source;

	private Column _dest;

	private int dontRewrite;

	private int _offset;

	private bool CrossTable { get; }

	public PasteColumnFormulaListener(TokenStreamRewriter rewriter, FormulaReferenceResolver resolver, Column source, Column dest)
	{
		_rewriter = rewriter;
		_resolver = resolver;
		_source = source;
		_dest = dest;
		CrossTable = source.Table != dest.Table;
		if (!CrossTable)
		{
			_offset = dest.Index - source.Index;
		}
	}

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		string text = context.FuncName().GetText();
		if (string.Equals(text, "LqSumIf", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqVLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqDistinct", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqFilter", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqAsc", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqDesc", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqCollect", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "SumIf", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "VLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "Distinct", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "DistinctF", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "DistinctUp", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "DistinctDown", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "CollectF", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "Collect", StringComparison.OrdinalIgnoreCase))
		{
			dontRewrite++;
		}
	}

	public override void ExitFunc([NotNull] FormulaParser.FuncContext context)
	{
		string text = context.FuncName().GetText();
		if (string.Equals(text, "LqSumIf", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqCollect", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "LqVLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "SumIf", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "CollectF", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "VLookUp", StringComparison.OrdinalIgnoreCase))
		{
			dontRewrite--;
			for (int i = 0; i < context.expr().Length / 2; i++)
			{
				InnerListener listener = new InnerListener(this, context.expr(i * 2 + 1));
				ParseTreeWalker.Default.Walk(listener, context.expr(i * 2 + 1));
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
		else if (string.Equals(text, "Collect", StringComparison.OrdinalIgnoreCase))
		{
			dontRewrite--;
			for (int j = 0; j < context.expr().Length; j++)
			{
				InnerListener listener2 = new InnerListener(this, context.expr(j));
				ParseTreeWalker.Default.Walk(listener2, context.expr(j));
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
		if (column == null || column.Table != _source.Table)
		{
			return;
		}
		if (CrossTable)
		{
			int num = column.Index - _source.Index;
			try
			{
				Column column2 = _dest.Table.Columns[_dest.Index + num];
				_rewriter.Replace(context.Int(0).Symbol, _dest.Table.Id.ToString());
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

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		if (dontRewrite != 0)
		{
			return;
		}
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Column column = _resolver.ResolveTableColumn(arg, arg2);
		if (column == null || column.Table != _source.Table)
		{
			return;
		}
		if (CrossTable)
		{
			int num = column.Index - _source.Index;
			try
			{
				Column column2 = _dest.Table.Columns[_dest.Index + num];
				_rewriter.Replace(context.Int(0).Symbol, _dest.Table.Id.ToString());
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
}
