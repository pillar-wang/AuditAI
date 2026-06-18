using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class DisplayRewriter : FormulaBaseListener
{
	protected readonly TokenStreamRewriter _rewriter;

	protected readonly FormulaReferenceResolver _resolver;

	protected readonly Table _contextTable;

	public DisplayRewriter(TokenStreamRewriter rewriter, FormulaReferenceResolver resolver, Table contextTable)
	{
		_rewriter = rewriter;
		_contextTable = contextTable;
		_resolver = resolver;
	}

	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		string text;
		try
		{
			Cell cell = _resolver.ResolveTableCell(arg, arg2);
			text = $"{GetTableNamePart(cell._Table)}[{cell.Column.GetUniqueFormulaName()},{cell.Row.Index + 1}]";
		}
		catch (FormulaBadReferenceException)
		{
			text = "[无效单元格引用]";
		}
		_rewriter.Replace(context.Start, context.Stop, text);
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		Id64 arg = Id64.Parse(context.Int(1).GetText());
		string text;
		if (arg.Value == long.MaxValue)
		{
			Table t = ((TreeTableNode)_resolver.ResolveTreeNode(id)).Table.LoadAndReturn();
			text = GetTableNamePart(t) + "[Row]";
		}
		else
		{
			try
			{
				Column column = _resolver.ResolveTableColumn(id, arg);
				text = GetTableNamePart(column.Table) + "[" + column.GetUniqueFormulaName() + "]";
			}
			catch (FormulaBadReferenceException)
			{
				text = "[无效列引用]";
			}
		}
		_rewriter.Replace(context.Start, context.Stop, text);
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		string text;
		try
		{
			Column column = _resolver.ResolveTableColumn(arg, arg2);
			text = GetTableNamePart(column.Table) + "[" + column.GetUniqueFormulaName() + ",*]";
		}
		catch (FormulaBadReferenceException)
		{
			text = "[无效列引用,*]";
		}
		_rewriter.Replace(context.Start, context.Stop, text);
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		string text;
		try
		{
			Cell cell = _resolver.ResolveTableCell(arg, arg2);
			text = GetTableNamePart(cell._Table) + "[" + cell.GetUniqueFormulaName() + ",*]";
		}
		catch (FormulaBadReferenceException)
		{
			text = "[无效列引用,*]";
		}
		_rewriter.Replace(context.Start, context.Stop, text);
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Id64 arg3 = Id64.Parse(context.Int(2).GetText());
		string text;
		try
		{
			Cell cell = _resolver.ResolveTableCell(arg, arg2);
			Cell cell2 = _resolver.ResolveTableCell(arg, arg3);
			text = $"{GetTableNamePart(cell._Table)}[{cell.Column.GetUniqueFormulaName()},{cell.Row.Index + 1}:{cell2.Column.GetUniqueFormulaName()},{cell2.Row.Index + 1}]";
		}
		catch (FormulaBadReferenceException)
		{
			text = "[无效区域引用]";
		}
		_rewriter.Replace(context.Start, context.Stop, text);
	}

	public override void ExitRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context)
	{
		Id64 arg = Id64.Parse(context.Int().GetText());
		TreeNodeBase treeNodeBase = _resolver.ResolveTreeNode(arg);
		_rewriter.Replace(context.Start, context.Stop, "{" + treeNodeBase.FormulaUniqueName + "}");
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		string text;
		try
		{
			Cell cell = _resolver.ResolveTableCell(arg, arg2);
			text = GetTableNamePart(cell._Table) + "[" + cell.GetUniqueFormulaName() + "]";
		}
		catch (FormulaBadReferenceException)
		{
			text = "[无效单元格引用]";
		}
		_rewriter.Replace(context.Start, context.Stop, text);
	}

	public override void ExitFunc([NotNull] FormulaParser.FuncContext context)
	{
	}

	private string GetTableNamePart(Table t)
	{
		if (_contextTable == null)
		{
			return "{" + t.GetCanonicalName() + "}";
		}
		if (t != _contextTable)
		{
			return "{" + t.GetCanonicalName() + "}";
		}
		return string.Empty;
	}
}
