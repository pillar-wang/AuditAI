namespace Auditai.DTO;

public class Cell
{
	public Id64 Id { get; set; }

	public Id64 RowId { get; set; }

	public Id64 ColumnId { get; set; }

	public BinaryValue Value { get; set; }

	public int Dirty { get; set; }

	public int Status { get; set; }

	public string Formula { get; set; }

	public Id64? StyleId { get; set; }

	public string CollectSource { get; set; }

	public string HeaderFormula { get; set; }
}
