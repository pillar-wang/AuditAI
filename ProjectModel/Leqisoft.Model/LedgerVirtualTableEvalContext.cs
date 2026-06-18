using System;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class LedgerVirtualTableEvalContext
{
	public Func<Id64, CellsOperand> BalanceTable_ResolveColumn { get; set; }

	public Func<Id64, CellsOperand> VoucherTable_ResolveColumn { get; set; }
}
