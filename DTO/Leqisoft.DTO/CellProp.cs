namespace Leqisoft.DTO;

public class CellProp
{
	public Id64 TableId { get; set; }

	public Id64 CellId { get; set; }

	public int Dirty { get; set; }

	public int Status { get; set; }

	public byte[] Attachments { get; set; }
}
