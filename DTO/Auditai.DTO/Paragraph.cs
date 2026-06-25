namespace Auditai.DTO;

public class Paragraph
{
	public Id64 Id { get; set; }

	public Id64 DocumentId { get; set; }

	public int Index { get; set; }

	public byte[] Stream { get; set; }

	public int Dirty { get; set; }

	public int Status { get; set; }

	public int ServerIndex { get; set; }

	public byte[] Section { get; set; }

	public string Comment { get; set; }
}
