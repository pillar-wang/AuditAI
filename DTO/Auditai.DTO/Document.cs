namespace Auditai.DTO;

public class Document
{
	public Id64 Id { get; set; }

	public int Version { get; set; }

	public long Locker { get; set; }

	public string SectPr { get; set; }

	public Id64 MergeTable { get; set; }

	public int Dirty { get; set; }
}
