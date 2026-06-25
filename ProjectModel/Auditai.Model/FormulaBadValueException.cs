namespace Auditai.Model;

public class FormulaBadValueException : FormulaException
{
	public FormulaBadValueException()
	{
	}

	public FormulaBadValueException(string message)
		: base(message)
	{
	}
}
