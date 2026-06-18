using System;

namespace Leqisoft.DTO;

public class NormalException : Exception
{
	public NormalException(string message)
		: base(message)
	{
	}
}
