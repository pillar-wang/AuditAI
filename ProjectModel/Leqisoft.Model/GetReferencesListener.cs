using System;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class GetReferencesListener : FormulaBaseListener
{
	private FormulaReferenceResolver _resolver;

	public FormulaReferences References { get; } = new FormulaReferences();


	public GetReferencesListener(FormulaReferenceResolver resolver)
	{
		_resolver = resolver;
	}

	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		References.CellReferences.Add(_resolver.ResolveTableCell(arg, arg2));
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		try
		{
			References.ColumnReferences.Add(_resolver.ResolveTableColumn(arg, arg2));
		}
		catch (FormulaBadReferenceException)
		{
		}
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		References.ColumnWildcardReferences.Add(_resolver.ResolveTableColumn(arg, arg2));
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Id64 arg3 = Id64.Parse(context.Int(2).GetText());
		References.RangeReferences.Add(new RangeOperand(_resolver.ResolveTableCell(arg, arg2), _resolver.ResolveTableCell(arg, arg3)));
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		References.HeaderCellReferences.Add(_resolver.ResolveTableCell(arg, arg2));
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		References.HeaderCellWildcardReferences.Add(_resolver.ResolveTableCell(arg, arg2));
	}

	public override void ExitFunc([NotNull] FormulaParser.FuncContext context)
	{
		try
		{
			ITerminalNode terminalNode = context.FuncName();
			if (terminalNode == null)
			{
				return;
			}
			string text = terminalNode.GetText();
			if (text.Equals("Title", StringComparison.OrdinalIgnoreCase))
			{
				if (context.expr(0) is FormulaParser.RefTreeNodeContext refTreeNodeContext)
				{
					Id64 arg = Id64.Parse(refTreeNodeContext.Int().GetText());
					int item = int.Parse(context.expr(1).GetText());
					int item2 = int.Parse(context.expr(2).GetText());
					TreeTableNode treeTableNode = (TreeTableNode)_resolver.ResolveTreeNode(arg);
					References.TitleReferences.Add(Tuple.Create(treeTableNode.Table, item, item2));
				}
			}
			else if (text.Equals("Foot", StringComparison.OrdinalIgnoreCase) && context.expr(0) is FormulaParser.RefTreeNodeContext refTreeNodeContext2)
			{
				Id64 arg2 = Id64.Parse(refTreeNodeContext2.Int().GetText());
				int item3 = int.Parse(context.expr(1).GetText());
				int item4 = int.Parse(context.expr(2).GetText());
				TreeTableNode treeTableNode2 = (TreeTableNode)_resolver.ResolveTreeNode(arg2);
				References.FootReferences.Add(Tuple.Create(treeTableNode2.Table, item3, item4));
			}
		}
		catch
		{
			throw new FormulaSyntaxException("", 0);
		}
	}
}
