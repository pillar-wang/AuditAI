using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class TicketDisplayRewriter : DisplayRewriter
{
	private readonly int _dataRowStart;

	private readonly int _dataRowCount;

	private readonly TicketTable _ticket;

	public TicketDisplayRewriter(TokenStreamRewriter rewriter, FormulaReferenceResolver resolver, Table contextTable, int dataRowStart, int dataRowCount, TicketTable ticket)
		: base(rewriter, resolver, contextTable)
	{
		_dataRowStart = dataRowStart;
		_dataRowCount = dataRowCount;
		_ticket = ticket;
	}

	public override void ExitRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		string excelColumnName = Column.GetExcelColumnName(int.Parse(context.Int(1).GetText()));
		_rewriter.Replace(context.Start, context.Stop, $"[{excelColumnName},{num + 1}]");
	}

	public override void ExitRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int num = int.Parse(context.Int(0).GetText());
		string excelColumnName = Column.GetExcelColumnName(int.Parse(context.Int(1).GetText()));
		int num2 = int.Parse(context.Int(2).GetText());
		string excelColumnName2 = Column.GetExcelColumnName(int.Parse(context.Int(3).GetText()));
		_rewriter.Replace(context.Start, context.Stop, $"[{excelColumnName},{num + 1}:{excelColumnName2},{num2 + 1}]");
	}

	public override void ExitRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context)
	{
		if (_ticket != null && _ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			long item = long.Parse(context.Int().GetText());
			foreach (TicketTableMixRangeTemplateRow dynamicDataRowTemplateRow in _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows)
			{
				int num = dynamicDataRowTemplateRow.TicketColumnIdList.IndexOf(item);
				if (num != -1)
				{
					string excelColumnName = Column.GetExcelColumnName(num);
					_rewriter.Replace(context.Start, context.Stop, $"[{excelColumnName},{dynamicDataRowTemplateRow.RefTicketTableRowIndex + 1}:{excelColumnName},{dynamicDataRowTemplateRow.BottomBorderRefTicketTableRowIndex + 1}]");
					return;
				}
			}
		}
		string excelColumnName2 = Column.GetExcelColumnName(int.Parse(context.Int().GetText()));
		_rewriter.Replace(context.Start, context.Stop, $"[{excelColumnName2},{_dataRowStart + 1}:{excelColumnName2},{_dataRowStart + _dataRowCount}]");
	}
}
