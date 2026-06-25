using System;

namespace Auditai.DTO;

public class TreeGroup
{
	public Id64 Id { get; set; }

	public Guid ProjectId { get; set; }

	public string Name { get; set; }

	public int Index { get; set; }

	public int ServerIndex { get; set; }

	public int Status { get; set; }

	public int Dirty { get; set; }
}
