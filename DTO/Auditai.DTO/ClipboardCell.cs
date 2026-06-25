using System;

namespace Auditai.DTO;

[Serializable]
public class ClipboardCell
{
	public const string ClipboardFormat = "AuditaiClipboardCell";

	public long TableId;

	public long CellId;
}
