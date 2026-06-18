using System;

namespace Leqisoft.UI.LedgerView;

public class PreviewNotSupport : Exception
{
	public PreviewNotSupport(string message)
		: base(message)
	{
	}
}
