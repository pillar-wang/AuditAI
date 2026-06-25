using System;

namespace Auditai.DTO;

public class PushEntityMeta
{
	public int Version { get; set; }

	public long UserId { get; set; }

	public DateTime Time { get; set; }

	public int Length { get; set; }
}
