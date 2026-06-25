using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class PasteTicketFormulaRewriter : FormulaBaseListener
{
	private readonly TokenStreamRewriter _rewriter;

	private readonly int _rowOffset;

	private readonly int _colOffset;

	private readonly int _rowCount;

	private readonly int _colCount;

	public PasteTicketFormulaRewriter(TokenStreamRewriter rewriter, int rowOffset, int colOffset, int rowCount, int colCount)
	{
		_rewriter = rewriter;
		_rowOffset = rowOffset;
		_colOffset = colOffset;
		_rowCount = rowCount;
		_colCount = colCount;
	}

	public override void EnterRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		num += _rowOffset;
		if (0 <= num && num < _rowCount)
		{
			_rewriter.Replace(context.Int(0).Symbol, num.ToString());
		}
		int num2 = int.Parse(context.Int(1).GetText());
		num2 += _colOffset;
		if (0 <= num2 && num2 < _colCount)
		{
			_rewriter.Replace(context.Int(1).Symbol, num2.ToString());
		}
	}

	public override void EnterRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		num += _rowOffset;
		if (0 <= num && num < _rowCount)
		{
			_rewriter.Replace(context.Int(0).Symbol, num.ToString());
		}
		int num2 = int.Parse(context.Int(1).GetText());
		num2 += _colOffset;
		if (0 <= num2 && num2 < _colCount)
		{
			_rewriter.Replace(context.Int(1).Symbol, num2.ToString());
		}
		int num3 = int.Parse(context.Int(2).GetText());
		num3 += _rowOffset;
		if (0 <= num3 && num3 < _rowCount)
		{
			_rewriter.Replace(context.Int(2).Symbol, num3.ToString());
		}
		int num4 = int.Parse(context.Int(3).GetText());
		num4 += _colOffset;
		if (0 <= num4 && num4 < _colCount)
		{
			_rewriter.Replace(context.Int(3).Symbol, num4.ToString());
		}
	}

	public override void EnterRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context)
	{
		int num = int.Parse(context.Int().GetText());
		num += _colOffset;
		if (0 <= num && num < _colCount)
		{
			_rewriter.Replace(context.Int().Symbol, num.ToString());
		}
	}
}
