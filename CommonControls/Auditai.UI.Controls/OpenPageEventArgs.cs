using System;

namespace Auditai.UI.Controls;

public class OpenPageEventArgs : EventArgs
{
	public string url { get; set; }

	public string title { get; set; }
}
