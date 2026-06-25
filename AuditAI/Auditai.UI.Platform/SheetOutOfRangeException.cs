using System;

namespace Auditai.UI.Platform;

public class SheetOutOfRangeException : Exception
{
	public SheetOutOfRangeException(string message)
		: base(message)
	{
	}
}
