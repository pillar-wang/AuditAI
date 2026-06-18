using System.Collections.Generic;

namespace Leqisoft.UI.LedgerView;

public class LedgerSelectionChangedEventArgs : LedgerEventArgs
{
	public List<double> Numbers { get; set; }
}
