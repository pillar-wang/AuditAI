using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class MixTicketRewriterToDesignFormula : FormulaBaseListener
{
	private readonly TokenStreamRewriter _rewriter;

	private readonly Func<int, int> _convertFixedRowIndexToVmRowIndexCallback;

	public MixTicketRewriterToDesignFormula(TokenStreamRewriter rewriter, Func<int, int> convertFixedRowIndexToVmRowIndexCallback)
	{
		_rewriter = rewriter;
		_convertFixedRowIndexToVmRowIndexCallback = convertFixedRowIndexToVmRowIndexCallback;
	}

	public override void EnterRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		int arg = int.Parse(context.Int(0).GetText());
		_rewriter.Replace(context.Int(0).Symbol, $"{_convertFixedRowIndexToVmRowIndexCallback(arg)}");
	}

	public override void EnterRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int arg = int.Parse(context.Int(0).GetText());
		int arg2 = int.Parse(context.Int(2).GetText());
		int num = _convertFixedRowIndexToVmRowIndexCallback(arg);
		int num2 = _convertFixedRowIndexToVmRowIndexCallback(arg2);
		_rewriter.Replace(context.Int(0).Symbol, $"{num}");
		_rewriter.Replace(context.Int(2).Symbol, $"{num2}");
	}
}
