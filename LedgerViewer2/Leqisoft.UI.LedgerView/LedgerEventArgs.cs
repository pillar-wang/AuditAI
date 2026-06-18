using System;

namespace Leqisoft.UI.LedgerView;

public class LedgerEventArgs : EventArgs
{
	public LedgerViewer Viewer { get; set; }
}
