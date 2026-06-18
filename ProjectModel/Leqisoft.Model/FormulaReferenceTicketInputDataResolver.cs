namespace Leqisoft.Model;

public abstract class FormulaReferenceTicketInputDataResolver
{
	public abstract Cell GetTicketTitleCell(int row, int col);

	public abstract Cell GetTicketFooterCell(int row, int col);
}
