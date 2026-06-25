using System;

namespace Auditai.UI.Controls;

public class ResizeEventArgs : EventArgs
{
	public int RowCol { get; set; }

	public int HeightWidth { get; set; }
}
