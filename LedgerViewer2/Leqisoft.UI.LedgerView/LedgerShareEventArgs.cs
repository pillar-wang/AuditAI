using C1.Win.C1Command;

namespace Leqisoft.UI.LedgerView;

public class LedgerShareEventArgs : LedgerEventArgs
{
	public C1CommandLink Link { get; set; }

	public string File { get; set; }
}
