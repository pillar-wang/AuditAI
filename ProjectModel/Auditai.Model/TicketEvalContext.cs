using System;
using Auditai.DTO;

namespace Auditai.Model;

public class TicketEvalContext
{
	public Func<Id64, int, int, Cell> ResolveColumnWildcard { get; set; }

	public Func<Id64, CellsOperand> ResolveColumn { get; set; }

	public Func<int, int, Cell> ResolveTicketCell { get; set; }

	public Func<int, int, int, int, CellsOperand> ResolveTicketRange { get; set; }

	public Func<int, CellsOperand> ResolveTicketColumn { get; set; }
}
