using System;

namespace Auditai.UI.LedgerView;

public class LedgerMergeException : Exception
{
	public LedgerMergeException(string mess)
		: base(mess)
	{
	}
}
