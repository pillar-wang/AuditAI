using System;

namespace Leqisoft.UI.Platform;

public class SheetOutOfRangeException : Exception
{
	public SheetOutOfRangeException(string message)
		: base(message)
	{
	}
}
