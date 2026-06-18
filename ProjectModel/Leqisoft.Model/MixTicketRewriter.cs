using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Leqisoft.Model;

public class MixTicketRewriter : FormulaBaseListener
{
	private readonly TokenStreamRewriter _rewriter;

	private readonly Func<int, bool> _checkIsDynamicDataRowCallback;

	private readonly Func<int, int, long> _getTicketColumnIdCallback;

	private readonly Func<int, int> _getFixedRowIndexCallback;

	public MixTicketRewriter(TokenStreamRewriter rewriter, Func<int, bool> checkIsDynamicDataRowCallback, Func<int, int, long> getTicketColumnIdCallback, Func<int, int> getFixedRowIndexCallback)
	{
		_rewriter = rewriter;
		_checkIsDynamicDataRowCallback = checkIsDynamicDataRowCallback;
		_getTicketColumnIdCallback = getTicketColumnIdCallback;
		_getFixedRowIndexCallback = getFixedRowIndexCallback;
	}

	public override void EnterRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		if (_checkIsDynamicDataRowCallback(num))
		{
			int arg = int.Parse(context.Int(1).GetText());
			long num2 = _getTicketColumnIdCallback(num, arg);
			_rewriter.Replace(context.Start, context.Stop, $"[10:{num2}]");
		}
		else
		{
			int num3 = _getFixedRowIndexCallback(num);
			_rewriter.Replace(context.Int(0).Symbol, $"{num3}");
		}
	}

	public override void EnterRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		int num2 = int.Parse(context.Int(2).GetText());
		for (int i = num; i <= num2; i++)
		{
			int num3 = i;
			if (_checkIsDynamicDataRowCallback(num3))
			{
				int arg = int.Parse(context.Int(1).GetText());
				long num4 = _getTicketColumnIdCallback(num3, arg);
				_rewriter.Replace(context.Start, context.Stop, $"[10:{num4}]");
				return;
			}
		}
		int num5 = _getFixedRowIndexCallback(num);
		int num6 = _getFixedRowIndexCallback(num2);
		_rewriter.Replace(context.Int(0).Symbol, $"{num5}");
		_rewriter.Replace(context.Int(2).Symbol, $"{num6}");
	}
}
