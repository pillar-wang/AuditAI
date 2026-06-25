using System.Collections.Generic;

namespace Auditai.UI.LedgerView;

public class LedgerSelectionChangedEventArgs : LedgerEventArgs
{
	public List<double> Numbers { get; set; }
}
