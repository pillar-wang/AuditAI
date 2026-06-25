using System;

namespace Auditai.UI.Controls;

public class CollectException : Exception
{
	public CollectException(string message)
		: base(message)
	{
	}
}
