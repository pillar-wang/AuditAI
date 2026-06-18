using Antlr4.Runtime;

namespace Leqisoft.Model;

public class FormulaDisplayErrorStrategy : DefaultErrorStrategy
{
	public static FormulaDisplayErrorStrategy Instance { get; } = new FormulaDisplayErrorStrategy();


	private FormulaDisplayErrorStrategy()
	{
	}

	protected override void ReportMissingToken(Parser recognizer)
	{
		throw new FormulaSyntaxException($"在位置 {recognizer.CurrentToken.StartIndex} 处缺少符号“{recognizer.GetExpectedTokens().ToString(recognizer.Vocabulary)}”", recognizer.CurrentToken.StartIndex);
	}

	protected override void ReportUnwantedToken(Parser recognizer)
	{
		throw new FormulaSyntaxException($"在位置 {recognizer.CurrentToken.StartIndex} 处存在多余符号“{recognizer.CurrentToken.Text}”。", recognizer.CurrentToken.StartIndex);
	}

	protected override void ReportInputMismatch(Parser recognizer, InputMismatchException e)
	{
		if (e.OffendingToken.Type == -1)
		{
			throw new FormulaSyntaxException("公式不完整。", e.OffendingToken.StartIndex);
		}
		throw new FormulaSyntaxException($"在位置 {e.OffendingToken.StartIndex} 处存在错误符号“{e.OffendingToken.Text}”", e.OffendingToken.StartIndex);
	}

	protected override void ReportNoViableAlternative(Parser recognizer, NoViableAltException e)
	{
		throw new FormulaSyntaxException($"在位置 {e.OffendingToken.StartIndex} 处无法识别符号“{e.OffendingToken.Text}”", e.OffendingToken.StartIndex);
	}
}
