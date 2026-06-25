namespace Auditai.Model;

public class EvalTicketControlFormulaVirtualTable : VirtualTable
{
	public EvalTicketControlFormulaVirtualTable(Table target)
		: base(target.Columns.Count, 1)
	{
	}
}
