using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class ValidationGetReferencesListener : FormulaBaseListener
{
	private FormulaEvaluationEnvironment _env;

	private bool _isInLqSumIfOrVLookUp;

	private FormulaReferenceResolver _resolver => _env.Resolver;

	public FormulaReferences References { get; } = new FormulaReferences();


	public ValidationGetReferencesListener(FormulaEvaluationEnvironment env)
	{
		_env = env;
	}

	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		if (!_isInLqSumIfOrVLookUp)
		{
			Id64 arg = Id64.Parse(context.Int(0).GetText());
			Id64 arg2 = Id64.Parse(context.Int(1).GetText());
			References.CellReferences.Add(_resolver.ResolveTableCell(arg, arg2));
		}
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		if (!_isInLqSumIfOrVLookUp)
		{
			Id64 arg = Id64.Parse(context.Int(0).GetText());
			Id64 arg2 = Id64.Parse(context.Int(1).GetText());
			References.ColumnReferences.Add(_resolver.ResolveTableColumn(arg, arg2));
		}
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		if (!_isInLqSumIfOrVLookUp)
		{
			Id64 arg = Id64.Parse(context.Int(0).GetText());
			Id64 arg2 = Id64.Parse(context.Int(1).GetText());
			References.ColumnWildcardReferences.Add(_resolver.ResolveTableColumn(arg, arg2));
		}
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		if (!_isInLqSumIfOrVLookUp)
		{
			Id64 arg = Id64.Parse(context.Int(0).GetText());
			Id64 arg2 = Id64.Parse(context.Int(1).GetText());
			Id64 arg3 = Id64.Parse(context.Int(2).GetText());
			References.RangeReferences.Add(new RangeOperand(_resolver.ResolveTableCell(arg, arg2), _resolver.ResolveTableCell(arg, arg3)));
		}
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		if (!_isInLqSumIfOrVLookUp)
		{
			Id64 arg = Id64.Parse(context.Int(0).GetText());
			Id64 arg2 = Id64.Parse(context.Int(1).GetText());
			References.HeaderCellReferences.Add(_resolver.ResolveTableCell(arg, arg2));
		}
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		if (!_isInLqSumIfOrVLookUp)
		{
			Id64 arg = Id64.Parse(context.Int(0).GetText());
			Id64 arg2 = Id64.Parse(context.Int(1).GetText());
			References.HeaderCellWildcardReferences.Add(_resolver.ResolveTableCell(arg, arg2));
		}
	}

	public override void ExitFunc([NotNull] FormulaParser.FuncContext context)
	{
		try
		{
			string text = context.FuncName().GetText();
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
			else if (text.Equals("Foot", StringComparison.OrdinalIgnoreCase))
			{
				if (context.expr(0) is FormulaParser.RefTreeNodeContext refTreeNodeContext2)
				{
					Id64 arg2 = Id64.Parse(refTreeNodeContext2.Int().GetText());
					int item3 = int.Parse(context.expr(1).GetText());
					int item4 = int.Parse(context.expr(2).GetText());
					TreeTableNode treeTableNode2 = (TreeTableNode)_resolver.ResolveTreeNode(arg2);
					References.FootReferences.Add(Tuple.Create(treeTableNode2.Table, item3, item4));
				}
			}
			else if (text.Equals("LqSumIf", StringComparison.OrdinalIgnoreCase) || text.Equals("LqVLookUp", StringComparison.OrdinalIgnoreCase) || text.Equals("SumIf", StringComparison.OrdinalIgnoreCase) || text.Equals("VLookUp", StringComparison.OrdinalIgnoreCase))
			{
				_isInLqSumIfOrVLookUp = false;
			}
		}
		catch
		{
		}
	}

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		string text = context.FuncName().GetText();
		if (!text.Equals("LqSumIf", StringComparison.OrdinalIgnoreCase) && !text.Equals("LqVLookUp", StringComparison.OrdinalIgnoreCase) && !text.Equals("SumIf", StringComparison.OrdinalIgnoreCase) && !text.Equals("VLookUp", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		GetReferencesListener getReferencesListener = new GetReferencesListener(_resolver);
		if (context.expr().Length > 1)
		{
			ParseTreeWalker.Default.Walk(getReferencesListener, context.expr(0));
			if (context.expr(1) is FormulaParser.RefColumnContext refColumnContext)
			{
				Id64 arg = Id64.Parse(refColumnContext.Int(0).GetText());
				Id64 arg2 = Id64.Parse(refColumnContext.Int(1).GetText());
				if (getReferencesListener.References.ColumnWildcardReferences.Count > 0)
				{
					References.ColumnWildcardReferences.UnionWith(getReferencesListener.References.ColumnWildcardReferences);
				}
				FormulaEvaluationVisitor formulaEvaluationVisitor = new FormulaEvaluationVisitor(_env);
				CellsOperand cellsOperand = formulaEvaluationVisitor.Visit(context.expr(0)) as CellsOperand;
				Column column = _resolver.ResolveTableColumn(arg, arg2);
				List<Cell> list = column.GetCells().ToList();
				foreach (int row in cellsOperand.Rows)
				{
					if (row < column.Table.Rows.Count)
					{
						RowRole role = column.Table.Rows[row].Role;
						if (role == RowRole.Normal || role == RowRole.Among || role == RowRole.Minus)
						{
							References.CellReferences.Add(list[row]);
						}
					}
				}
			}
		}
		_isInLqSumIfOrVLookUp = true;
	}
}
