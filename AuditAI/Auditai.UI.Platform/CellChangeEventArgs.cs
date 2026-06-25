using System;

namespace Auditai.UI.Platform;

public class CellChangeEventArgs : EventArgs
{
	public CellValue Cell { get; set; }

	public bool? GrantAll { get; set; }

	public long? Add { get; set; }

	public long? Remove { get; set; }
}
