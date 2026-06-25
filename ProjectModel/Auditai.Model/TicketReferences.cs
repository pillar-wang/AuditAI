using System.Collections.Generic;

namespace Auditai.Model;

public class TicketReferences
{
	public HashSet<TicketCellReference> Cell { get; } = new HashSet<TicketCellReference>();


	public HashSet<TicketRangeReference> Range { get; } = new HashSet<TicketRangeReference>();


	public HashSet<TicketColumnReference> Column { get; } = new HashSet<TicketColumnReference>();


	public override string ToString()
	{
		return "Cell: " + string.Join(", ", Cell) + "\tRange: " + string.Join(", ", Range) + "\tColumn: " + string.Join(", ", Column);
	}
}
