using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public interface IFormulaVisitor<Result> : IParseTreeVisitor<Result>
{
	Result VisitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context);

	Result VisitString([NotNull] FormulaParser.StringContext context);

	Result VisitRefCell([NotNull] FormulaParser.RefCellContext context);

	Result VisitLt([NotNull] FormulaParser.LtContext context);

	Result VisitAddsub([NotNull] FormulaParser.AddsubContext context);

	Result VisitRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context);

	Result VisitFloat([NotNull] FormulaParser.FloatContext context);

	Result VisitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context);

	Result VisitNeg([NotNull] FormulaParser.NegContext context);

	Result VisitParen([NotNull] FormulaParser.ParenContext context);

	Result VisitAnd([NotNull] FormulaParser.AndContext context);

	Result VisitGte([NotNull] FormulaParser.GteContext context);

	Result VisitPower([NotNull] FormulaParser.PowerContext context);

	Result VisitLte([NotNull] FormulaParser.LteContext context);

	Result VisitOr([NotNull] FormulaParser.OrContext context);

	Result VisitRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context);

	Result VisitRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context);

	Result VisitRefColumn([NotNull] FormulaParser.RefColumnContext context);

	Result VisitConcat([NotNull] FormulaParser.ConcatContext context);

	Result VisitEq([NotNull] FormulaParser.EqContext context);

	Result VisitGt([NotNull] FormulaParser.GtContext context);

	Result VisitInt([NotNull] FormulaParser.IntContext context);

	Result VisitMuldiv([NotNull] FormulaParser.MuldivContext context);

	Result VisitRefRange([NotNull] FormulaParser.RefRangeContext context);

	Result VisitFunc([NotNull] FormulaParser.FuncContext context);

	Result VisitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context);

	Result VisitNe([NotNull] FormulaParser.NeContext context);

	Result VisitRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context);

	Result VisitFormula([NotNull] FormulaParser.FormulaContext context);

	Result VisitExprs([NotNull] FormulaParser.ExprsContext context);
}
