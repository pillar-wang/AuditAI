using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public interface IFormulaListener : IParseTreeListener
{
	void EnterRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context);

	void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context);

	void EnterString([NotNull] FormulaParser.StringContext context);

	void ExitString([NotNull] FormulaParser.StringContext context);

	void EnterRefCell([NotNull] FormulaParser.RefCellContext context);

	void ExitRefCell([NotNull] FormulaParser.RefCellContext context);

	void EnterLt([NotNull] FormulaParser.LtContext context);

	void ExitLt([NotNull] FormulaParser.LtContext context);

	void EnterAddsub([NotNull] FormulaParser.AddsubContext context);

	void ExitAddsub([NotNull] FormulaParser.AddsubContext context);

	void EnterRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context);

	void ExitRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context);

	void EnterFloat([NotNull] FormulaParser.FloatContext context);

	void ExitFloat([NotNull] FormulaParser.FloatContext context);

	void EnterRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context);

	void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context);

	void EnterNeg([NotNull] FormulaParser.NegContext context);

	void ExitNeg([NotNull] FormulaParser.NegContext context);

	void EnterParen([NotNull] FormulaParser.ParenContext context);

	void ExitParen([NotNull] FormulaParser.ParenContext context);

	void EnterAnd([NotNull] FormulaParser.AndContext context);

	void ExitAnd([NotNull] FormulaParser.AndContext context);

	void EnterGte([NotNull] FormulaParser.GteContext context);

	void ExitGte([NotNull] FormulaParser.GteContext context);

	void EnterPower([NotNull] FormulaParser.PowerContext context);

	void ExitPower([NotNull] FormulaParser.PowerContext context);

	void EnterLte([NotNull] FormulaParser.LteContext context);

	void ExitLte([NotNull] FormulaParser.LteContext context);

	void EnterOr([NotNull] FormulaParser.OrContext context);

	void ExitOr([NotNull] FormulaParser.OrContext context);

	void EnterRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context);

	void ExitRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context);

	void EnterRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context);

	void ExitRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context);

	void EnterRefColumn([NotNull] FormulaParser.RefColumnContext context);

	void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context);

	void EnterConcat([NotNull] FormulaParser.ConcatContext context);

	void ExitConcat([NotNull] FormulaParser.ConcatContext context);

	void EnterEq([NotNull] FormulaParser.EqContext context);

	void ExitEq([NotNull] FormulaParser.EqContext context);

	void EnterGt([NotNull] FormulaParser.GtContext context);

	void ExitGt([NotNull] FormulaParser.GtContext context);

	void EnterInt([NotNull] FormulaParser.IntContext context);

	void ExitInt([NotNull] FormulaParser.IntContext context);

	void EnterMuldiv([NotNull] FormulaParser.MuldivContext context);

	void ExitMuldiv([NotNull] FormulaParser.MuldivContext context);

	void EnterRefRange([NotNull] FormulaParser.RefRangeContext context);

	void ExitRefRange([NotNull] FormulaParser.RefRangeContext context);

	void EnterFunc([NotNull] FormulaParser.FuncContext context);

	void ExitFunc([NotNull] FormulaParser.FuncContext context);

	void EnterRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context);

	void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context);

	void EnterNe([NotNull] FormulaParser.NeContext context);

	void ExitNe([NotNull] FormulaParser.NeContext context);

	void EnterRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context);

	void ExitRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context);

	void EnterFormula([NotNull] FormulaParser.FormulaContext context);

	void ExitFormula([NotNull] FormulaParser.FormulaContext context);

	void EnterExprs([NotNull] FormulaParser.ExprsContext context);

	void ExitExprs([NotNull] FormulaParser.ExprsContext context);
}
