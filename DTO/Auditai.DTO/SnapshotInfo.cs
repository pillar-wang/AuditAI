using System;

namespace Auditai.DTO;

public class SnapshotInfo
{
	public int Id { get; set; }

	public Id64 TreeNodeId { get; set; }

	public string Name { get; set; }

	public DateTime DateTime { get; set; }

	public int Size { get; set; }

	public int Kind { get; set; }

	public bool Deleted { get; set; }
}
