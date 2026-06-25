using System;

namespace Auditai.DTO;

public class NormalException : Exception
{
	public NormalException(string message)
		: base(message)
	{
	}
}
