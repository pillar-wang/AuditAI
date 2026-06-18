using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Leqisoft.Model;

public class OffsetTicketRewriter : FormulaBaseListener
{
	private readonly TokenStreamRewriter _rewriter;

	private readonly int _startRow;

	private readonly int _rowOffset;

	private readonly int _startCol;

	private readonly int _colOffset;

	public OffsetTicketRewriter(TokenStreamRewriter rewriter, int startRow, int rowOffset, int startCol, int colOffset)
	{
		_rewriter = rewriter;
		_startRow = startRow;
		_rowOffset = rowOffset;
		_startCol = startCol;
		_colOffset = colOffset;
	}

	public override void EnterRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		if (num >= _startRow)
		{
			num += _rowOffset;
			_rewriter.Replace(context.Int(0).Symbol, num);
		}
		int num2 = int.Parse(context.Int(1).GetText());
		if (num2 >= _startCol)
		{
			num2 += _colOffset;
			_rewriter.Replace(context.Int(1).Symbol, num2);
		}
	}

	public override void EnterRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		if (num >= _startRow)
		{
			num += _rowOffset;
			_rewriter.Replace(context.Int(0).Symbol, num);
		}
		int num2 = int.Parse(context.Int(1).GetText());
		if (num2 >= _startCol)
		{
			num2 += _colOffset;
			_rewriter.Replace(context.Int(1).Symbol, num2);
		}
		int num3 = int.Parse(context.Int(2).GetText());
		if (num3 >= _startRow)
		{
			num3 += _rowOffset;
			_rewriter.Replace(context.Int(2).Symbol, num3);
		}
		int num4 = int.Parse(context.Int(3).GetText());
		if (num4 >= _startCol)
		{
			num4 += _colOffset;
			_rewriter.Replace(context.Int(3).Symbol, num4);
		}
	}

	public override void EnterRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context)
	{
		int num = int.Parse(context.Int().GetText());
		if (num >= _startCol)
		{
			num += _colOffset;
			_rewriter.Replace(context.Int().Symbol, num);
		}
	}
}
