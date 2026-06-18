using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Leqisoft.Model;

public class GetRefAtPosListener : FormulaDisplayParserBaseListener
{
	private bool _isInFunc;

	public List<Tuple<int, int>> RefIntervals { get; } = new List<Tuple<int, int>>();


	public override void ExitTableCell([NotNull] FormulaDisplayParser.TableCellContext context)
	{
		AddRef(context, context.RBracket());
	}

	public override void ExitColumnCell([NotNull] FormulaDisplayParser.ColumnCellContext context)
	{
		AddRef(context, context.RBracket());
	}

	public override void ExitColumn([NotNull] FormulaDisplayParser.ColumnContext context)
	{
		AddRef(context, context.RBracket());
	}

	public override void ExitColumnWildcard([NotNull] FormulaDisplayParser.ColumnWildcardContext context)
	{
		AddRef(context, context.RBracket());
	}

	public override void ExitTableColumn([NotNull] FormulaDisplayParser.TableColumnContext context)
	{
		AddRef(context, context.RBracket());
	}

	public override void ExitTableColumnWildcard([NotNull] FormulaDisplayParser.TableColumnWildcardContext context)
	{
		AddRef(context, context.RBracket());
	}

	public override void ExitTableRange([NotNull] FormulaDisplayParser.TableRangeContext context)
	{
		AddRef(context, context.RBracket());
	}

	public override void ExitTreeNode([NotNull] FormulaDisplayParser.TreeNodeContext context)
	{
		if (!_isInFunc)
		{
			AddRef(context, context.RBrace());
		}
	}

	public override void ExitRange([NotNull] FormulaDisplayParser.RangeContext context)
	{
		AddRef(context, context.RBracket());
	}

	public override void EnterFunc([NotNull] FormulaDisplayParser.FuncContext context)
	{
		string text = context.funcName().GetText();
		if (text.Equals("Title", StringComparison.OrdinalIgnoreCase) || text.Equals("Foot", StringComparison.OrdinalIgnoreCase))
		{
			_isInFunc = true;
		}
	}

	public override void ExitFunc([NotNull] FormulaDisplayParser.FuncContext context)
	{
		string text = context.funcName().GetText();
		if (text.Equals("Title", StringComparison.OrdinalIgnoreCase) || text.Equals("Foot", StringComparison.OrdinalIgnoreCase))
		{
			AddRef(context, context.RParen());
			_isInFunc = false;
		}
	}

	private void AddRef(ParserRuleContext context, ITerminalNode terminal)
	{
		if (terminal != null)
		{
			RefIntervals.Add(Tuple.Create(context.Start.StartIndex, terminal.Symbol.StopIndex));
		}
	}
}
