using System;

namespace Auditai.UI.LedgerView;

public class PreviewNotSupport : Exception
{
	public PreviewNotSupport(string message)
		: base(message)
	{
	}
}
