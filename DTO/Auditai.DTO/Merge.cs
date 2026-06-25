namespace Auditai.DTO;

public class Merge
{
	public long Id { get; set; }

	public long TableId { get; set; }

	public long TopLeft { get; set; }

	public long BottomRight { get; set; }

	public int Status { get; set; }
}
