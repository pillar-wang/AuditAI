using System;

namespace Leqisoft.UI.LedgerView;

public class LedgerMergeException : Exception
{
	public LedgerMergeException(string mess)
		: base(mess)
	{
	}
}
