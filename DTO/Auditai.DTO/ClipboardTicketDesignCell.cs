using System;

namespace Auditai.DTO;

[Serializable]
public class ClipboardTicketDesignCell
{
	public const string ClipboardFormat = "AuditaiClipboardTicketDesignCell";

	public int Row;

	public int Col;

	public string Text;

	public string Formula;
}
