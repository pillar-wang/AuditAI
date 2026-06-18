using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public interface IFormulaDisplayParserListener : IParseTreeListener
{
	void EnterFuncName([NotNull] FormulaDisplayParser.FuncNameContext context);

	void ExitFuncName([NotNull] FormulaDisplayParser.FuncNameContext context);

	void EnterColumnName([NotNull] FormulaDisplayParser.ColumnNameContext context);

	void ExitColumnName([NotNull] FormulaDisplayParser.ColumnNameContext context);

	void EnterRelOp([NotNull] FormulaDisplayParser.RelOpContext context);

	void ExitRelOp([NotNull] FormulaDisplayParser.RelOpContext context);

	void EnterAdd([NotNull] FormulaDisplayParser.AddContext context);

	void ExitAdd([NotNull] FormulaDisplayParser.AddContext context);

	void EnterTableCell([NotNull] FormulaDisplayParser.TableCellContext context);

	void ExitTableCell([NotNull] FormulaDisplayParser.TableCellContext context);

	void EnterColumnCell([NotNull] FormulaDisplayParser.ColumnCellContext context);

	void ExitColumnCell([NotNull] FormulaDisplayParser.ColumnCellContext context);

	void EnterSub([NotNull] FormulaDisplayParser.SubContext context);

	void ExitSub([NotNull] FormulaDisplayParser.SubContext context);

	void EnterString([NotNull] FormulaDisplayParser.StringContext context);

	void ExitString([NotNull] FormulaDisplayParser.StringContext context);

	void EnterMul([NotNull] FormulaDisplayParser.MulContext context);

	void ExitMul([NotNull] FormulaDisplayParser.MulContext context);

	void EnterColumn([NotNull] FormulaDisplayParser.ColumnContext context);

	void ExitColumn([NotNull] FormulaDisplayParser.ColumnContext context);

	void EnterRange([NotNull] FormulaDisplayParser.RangeContext context);

	void ExitRange([NotNull] FormulaDisplayParser.RangeContext context);

	void EnterConcat([NotNull] FormulaDisplayParser.ConcatContext context);

	void ExitConcat([NotNull] FormulaDisplayParser.ConcatContext context);

	void EnterDiv([NotNull] FormulaDisplayParser.DivContext context);

	void ExitDiv([NotNull] FormulaDisplayParser.DivContext context);

	void EnterNumber([NotNull] FormulaDisplayParser.NumberContext context);

	void ExitNumber([NotNull] FormulaDisplayParser.NumberContext context);

	void EnterNeg([NotNull] FormulaDisplayParser.NegContext context);

	void ExitNeg([NotNull] FormulaDisplayParser.NegContext context);

	void EnterParen([NotNull] FormulaDisplayParser.ParenContext context);

	void ExitParen([NotNull] FormulaDisplayParser.ParenContext context);

	void EnterFunc([NotNull] FormulaDisplayParser.FuncContext context);

	void ExitFunc([NotNull] FormulaDisplayParser.FuncContext context);

	void EnterTableColumnWildcard([NotNull] FormulaDisplayParser.TableColumnWildcardContext context);

	void ExitTableColumnWildcard([NotNull] FormulaDisplayParser.TableColumnWildcardContext context);

	void EnterColumnWildcard([NotNull] FormulaDisplayParser.ColumnWildcardContext context);

	void ExitColumnWildcard([NotNull] FormulaDisplayParser.ColumnWildcardContext context);

	void EnterRel([NotNull] FormulaDisplayParser.RelContext context);

	void ExitRel([NotNull] FormulaDisplayParser.RelContext context);

	void EnterTableColumn([NotNull] FormulaDisplayParser.TableColumnContext context);

	void ExitTableColumn([NotNull] FormulaDisplayParser.TableColumnContext context);

	void EnterTableRange([NotNull] FormulaDisplayParser.TableRangeContext context);

	void ExitTableRange([NotNull] FormulaDisplayParser.TableRangeContext context);

	void EnterPower([NotNull] FormulaDisplayParser.PowerContext context);

	void ExitPower([NotNull] FormulaDisplayParser.PowerContext context);

	void EnterTreeNode([NotNull] FormulaDisplayParser.TreeNodeContext context);

	void ExitTreeNode([NotNull] FormulaDisplayParser.TreeNodeContext context);

	void EnterFormula([NotNull] FormulaDisplayParser.FormulaContext context);

	void ExitFormula([NotNull] FormulaDisplayParser.FormulaContext context);

	void EnterExprs([NotNull] FormulaDisplayParser.ExprsContext context);

	void ExitExprs([NotNull] FormulaDisplayParser.ExprsContext context);
}
