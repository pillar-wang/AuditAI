namespace Leqisoft.DTO;

public class TreeNode
{
	public Id64 Id { get; set; }

	public Id64 GroupId { get; set; }

	public Id64? ParentId { get; set; }

	public string Name { get; set; }

	public int Status { get; set; }

	public int Dirty { get; set; }

	public int Index { get; set; }

	public int ServerIndex { get; set; }

	public int Type { get; set; }

	public int Level { get; set; }

	public int Version { get; set; }

	public string Number { get; set; }

	public string Permissions { get; set; }

	public bool Visible { get; set; }

	public bool RowWrite { get; set; }

	public bool RowRead { get; set; }
}
