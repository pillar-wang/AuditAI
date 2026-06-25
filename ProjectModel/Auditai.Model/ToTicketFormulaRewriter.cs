using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class ToTicketFormulaRewriter : ToFormulaRewriter
{
	private static readonly Regex _rxTicketColumn = new Regex("^[A-Za-z]{1,2}$");

	public ToTicketFormulaRewriter(TokenStreamRewriter rewriter, FormulaContext context)
		: base(rewriter, context)
	{
	}

	public override void ExitColumnCell([NotNull] FormulaDisplayParser.ColumnCellContext context)
	{
		string text = context.columnName().GetText();
		if (_rxTicketColumn.IsMatch(text))
		{
			int num = int.Parse(context.Int().GetText()) - 1;
			_rewriter.Replace(context.Start, context.Stop, $"[8:{num}:{Column.ExcelColumnNameToNumber(text)}]");
		}
		else
		{
			base.ExitColumnCell(context);
		}
	}

	public override void ExitRange([NotNull] FormulaDisplayParser.RangeContext context)
	{
		string text = context.columnName(0).GetText();
		string text2 = context.columnName(1).GetText();
		if (_rxTicketColumn.IsMatch(text) && _rxTicketColumn.IsMatch(text2))
		{
			int num = int.Parse(context.Int(0).GetText()) - 1;
			int num2 = int.Parse(context.Int(1).GetText()) - 1;
			_rewriter.Replace(context.Start, context.Stop, $"[9:{num}:{Column.ExcelColumnNameToNumber(text)}:{num2}:{Column.ExcelColumnNameToNumber(text2)}]");
		}
		else
		{
			base.ExitRange(context);
		}
	}
}
