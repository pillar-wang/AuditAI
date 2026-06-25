using Auditai.DTO;

namespace Auditai.Model;

public class FormulaRefInfo
{
	public FormulaHostKind Kind { get; set; }

	public Id64 TableId { get; set; }

	public Id64 Id1 { get; set; }

	public Id64 Id2 { get; set; }

	public int Int1 { get; set; }

	public int Int2 { get; set; }
}
