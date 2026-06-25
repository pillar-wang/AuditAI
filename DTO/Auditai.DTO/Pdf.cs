using System;

namespace Auditai.DTO;

public class Pdf
{
	public Id64 Id { get; set; }

	public Guid FileId { get; set; }

	public int Version { get; set; }
}
