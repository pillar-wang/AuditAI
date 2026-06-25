using System;

namespace Auditai.DTO;

public class Image
{
	public Id64 Id { get; set; }

	public Guid FileId { get; set; }

	public int Version { get; set; }

	public int Dirty { get; set; }

	public float CenterX { get; set; }

	public float CenterY { get; set; }

	public float ZoomFactor { get; set; }

	public string PageSetup { get; set; }

	public int RotateFlip { get; set; }
}
