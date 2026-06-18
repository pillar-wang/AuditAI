using Antlr4.Runtime.Misc;

namespace Leqisoft.Model;

public class GetTicketReferencesListener : FormulaBaseListener
{
	public TicketReferences Result { get; } = new TicketReferences();


	public override void EnterRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		int row = int.Parse(context.Int(0).GetText());
		int col = int.Parse(context.Int(1).GetText());
		Result.Cell.Add(new TicketCellReference
		{
			Row = row,
			Col = col
		});
	}

	public override void EnterRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int row = int.Parse(context.Int(0).GetText());
		int col = int.Parse(context.Int(1).GetText());
		int row2 = int.Parse(context.Int(2).GetText());
		int col2 = int.Parse(context.Int(3).GetText());
		Result.Range.Add(new TicketRangeReference
		{
			Row1 = row,
			Col1 = col,
			Row2 = row2,
			Col2 = col2
		});
	}

	public override void EnterRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context)
	{
		int col = int.Parse(context.Int().GetText());
		Result.Column.Add(new TicketColumnReference
		{
			Col = col
		});
	}
}
