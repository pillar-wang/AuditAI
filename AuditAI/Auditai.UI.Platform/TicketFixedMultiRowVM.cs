using System.Collections.Generic;
using System.Linq;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class TicketFixedMultiRowVM
{
	public List<TicketInputCellVM> KeyCells { get; } = new List<TicketInputCellVM>();


	public List<TicketInputCellVM> ValueCells { get; } = new List<TicketInputCellVM>();


	public Row Row { get; set; }

	public bool IsEmpty()
	{
		return ValueCells.All((TicketInputCellVM c) => "".Equals(c.Value) || 0.0.Equals(c.Value));
	}
}
