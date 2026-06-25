using System;

namespace Auditai.UI.LedgerView;

public class LedgerEventArgs : EventArgs
{
	public LedgerViewer Viewer { get; set; }
}
