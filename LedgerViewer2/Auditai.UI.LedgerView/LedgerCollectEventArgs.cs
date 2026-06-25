using System;
using System.Collections.Generic;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

public class LedgerCollectEventArgs : LedgerEventArgs
{
	public CollectObjectEnum CollectObject { get; set; }

	public DateTime StartTime { get; set; }

	public DateTime EndTime { get; set; }

	public Account Account { get; set; }

	public object Auxiliary { get; set; }

	public List<object> Source { get; set; }

	public bool IsSourceComeFromMyMark { get; set; }

	public bool IsShowSubsidiaryAllAccountTypeTable { get; set; }

	public bool IsShowBalanceAllAccountTypeTable { get; set; }
}
