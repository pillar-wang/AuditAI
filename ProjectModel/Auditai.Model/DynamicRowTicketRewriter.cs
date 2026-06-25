using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class DynamicRowTicketRewriter : FormulaBaseListener
{
	private readonly TokenStreamRewriter _rewriter;

	private readonly int _dataRowStart;

	private readonly int _dataRowCount;

	public DynamicRowTicketRewriter(TokenStreamRewriter rewriter, int dataRowStart, int dataRowCount)
	{
		_rewriter = rewriter;
		_dataRowStart = dataRowStart;
		_dataRowCount = dataRowCount;
	}

	public override void EnterRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		if (num >= _dataRowStart && num < _dataRowStart + _dataRowCount)
		{
			_rewriter.Replace(context.Start, context.Stop, "[10:" + context.Int(1).GetText() + "]");
		}
	}

	public override void EnterRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		int num2 = int.Parse(context.Int(2).GetText());
		if (num < _dataRowStart + _dataRowCount && num2 >= _dataRowStart)
		{
			_rewriter.Replace(context.Start, context.Stop, "[10:" + context.Int(1).GetText() + "]");
		}
	}
}
