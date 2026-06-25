using System;
using Auditai.DTO;

namespace Auditai.Model;

public class VirtualTableEvalContext
{
	public Func<Id64, int, Cell> ResolveColumnWildcard { get; set; }

	public Func<Id64, CellsOperand> ResolveColumn { get; set; }

	public Func<Id64, Cell> ResolveTableCell { get; set; }

	public Func<Table, int, int> ResolveTableTitleCell { get; set; }
}
