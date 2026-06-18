namespace Leqisoft.DTO;

public class Column
{
	public Id64 Id { get; set; }

	public Id64? TableId { get; set; }

	public string ConsolidateAttribs { get; set; }

	public int SubtotalAttribs { get; set; }

	public int Index { get; set; }

	public int ServerIndex { get; set; }

	public string Caption { get; set; }

	public string CaptionStyle { get; set; }

	public int Width { get; set; }

	public bool Visible { get; set; }

	public int Dirty { get; set; }

	public int Status { get; set; }

	public string Formula { get; set; }

	public Id64? StyleId { get; set; }

	public string Permissions { get; set; }

	public string CaptionFormula { get; set; }

	public byte[] CrossAttributes { get; set; }
}
