using System;

namespace Auditai.Model;

public class FormulaException : Exception
{
	public FormulaException()
	{
	}

	public FormulaException(string message)
		: base(message)
	{
	}
}
