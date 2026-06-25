using System;

namespace Auditai.UI.Platform;

public class ProgressArgs : EventArgs
{
	public ProgressEnum Type { get; set; }

	public string Message { get; set; }

	public int Progress { get; set; }
}
