using System;

namespace Leqisoft.Model;

public class TableModelException : ApplicationException
{
	public TableModelException(string message)
		: base(message)
	{
	}
}
