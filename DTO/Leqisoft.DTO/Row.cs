namespace Leqisoft.DTO;

public class Row
{
	public Id64 Id { get; set; }

	public Id64 TableId { get; set; }

	public int Index { get; set; }

	public int ServerIndex { get; set; }

	public int Height { get; set; }

	public bool Visible { get; set; }

	public int Dirty { get; set; }

	public int Status { get; set; }

	public long Locked { get; set; }

	public int Role { get; set; }

	public string Permissions { get; set; }

	public long Creator { get; set; }
}
