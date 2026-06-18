using System;

namespace Leqisoft.DTO;

[Serializable]
public class ClipboardTicketDesignCell
{
	public const string ClipboardFormat = "LeqiClipboardTicketDesignCell";

	public int Row;

	public int Col;

	public string Text;

	public string Formula;
}
