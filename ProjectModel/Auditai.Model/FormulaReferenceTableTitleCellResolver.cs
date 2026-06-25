namespace Auditai.Model;

public abstract class FormulaReferenceTableTitleCellResolver
{
	public abstract TableTitleCell GetTableTitleCell(Table table, int row, int col);
}
