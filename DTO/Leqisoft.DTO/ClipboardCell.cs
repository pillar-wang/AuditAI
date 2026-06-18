using System;

namespace Leqisoft.DTO;

[Serializable]
public class ClipboardCell
{
	public const string ClipboardFormat = "LeqiClipboardCell";

	public long TableId;

	public long CellId;
}
