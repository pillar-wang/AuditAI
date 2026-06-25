using System;

namespace Auditai.Model;

public class TableModelException : ApplicationException
{
	public TableModelException(string message)
		: base(message)
	{
	}
}
