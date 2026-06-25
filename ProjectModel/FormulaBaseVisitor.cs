using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public class FormulaBaseVisitor<Result> : AbstractParseTreeVisitor<Result>, IFormulaVisitor<Result>, IParseTreeVisitor<Result>
{
	public virtual Result VisitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitString([NotNull] FormulaParser.StringContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitLt([NotNull] FormulaParser.LtContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitAddsub([NotNull] FormulaParser.AddsubContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitFloat([NotNull] FormulaParser.FloatContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitNeg([NotNull] FormulaParser.NegContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitParen([NotNull] FormulaParser.ParenContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitAnd([NotNull] FormulaParser.AndContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitGte([NotNull] FormulaParser.GteContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitPower([NotNull] FormulaParser.PowerContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitLte([NotNull] FormulaParser.LteContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitOr([NotNull] FormulaParser.OrContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitConcat([NotNull] FormulaParser.ConcatContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitEq([NotNull] FormulaParser.EqContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitGt([NotNull] FormulaParser.GtContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitInt([NotNull] FormulaParser.IntContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitMuldiv([NotNull] FormulaParser.MuldivContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitFunc([NotNull] FormulaParser.FuncContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitNe([NotNull] FormulaParser.NeContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitFormula([NotNull] FormulaParser.FormulaContext context)
	{
		return VisitChildren(context);
	}

	public virtual Result VisitExprs([NotNull] FormulaParser.ExprsContext context)
	{
		return VisitChildren(context);
	}
}
