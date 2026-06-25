using System;
using Auditai.DTO;

namespace Auditai.Model;

public class LedgerVirtualTableEvalContext
{
	public Func<Id64, CellsOperand> BalanceTable_ResolveColumn { get; set; }

	public Func<Id64, CellsOperand> VoucherTable_ResolveColumn { get; set; }
}
