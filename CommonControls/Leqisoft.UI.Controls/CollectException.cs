using System;

namespace Leqisoft.UI.Controls;

public class CollectException : Exception
{
	public CollectException(string message)
		: base(message)
	{
	}
}
