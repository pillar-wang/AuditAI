namespace Leqisoft.Model;

public class FormulaSyntaxException : FormulaException
{
	public int CharPosition { get; }

	public FormulaSyntaxException(string message, int charPosition)
		: base(message)
	{
		CharPosition = charPosition;
	}
}
