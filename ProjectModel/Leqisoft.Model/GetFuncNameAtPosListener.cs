using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Leqisoft.Model;

public class GetFuncNameAtPosListener : FormulaDisplayParserBaseListener
{
	private int _pos;

	public string FuncName { get; private set; }

	public int NthParameter { get; private set; }

	public GetFuncNameAtPosListener(int pos)
	{
		_pos = pos;
	}

	public override void ExitFunc([NotNull] FormulaDisplayParser.FuncContext context)
	{
		if (FuncName != null)
		{
			return;
		}
		if (context.Stop.Type == 19)
		{
			if (_pos > context.Start.StartIndex && _pos <= context.Stop.StopIndex)
			{
				FuncName = context.funcName().GetText();
				NthParameter = GetNthParameter();
			}
		}
		else if (_pos > context.Start.StartIndex)
		{
			FuncName = context.funcName().GetText();
			NthParameter = GetNthParameter();
		}
		int GetNthParameter()
		{
			ITerminalNode[] array = context.Comma();
			for (int i = 0; i < array.Length; i++)
			{
				if (_pos <= array[i].SourceInterval.a)
				{
					return i;
				}
			}
			return array.Length;
		}
	}
}
