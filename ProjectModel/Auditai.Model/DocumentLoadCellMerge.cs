using Auditai.DTO;

namespace Auditai.Model;

public class DocumentLoadCellMerge
{
	public Id64 Id { get; set; }

	public int Row1 { get; set; }

	public int Col1 { get; set; }

	public int Row2 { get; set; }

	public int Col2 { get; set; }

	public bool IsRightBorderSet { get; set; }

	public override string ToString()
	{
		return $"{Id} ({Row1},{Col1})-({Row2},{Col2}) {IsRightBorderSet}";
	}
}
