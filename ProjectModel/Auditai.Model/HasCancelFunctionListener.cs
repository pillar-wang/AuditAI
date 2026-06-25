using System;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class HasCancelFunctionListener : FormulaBaseListener
{
	public bool Result { get; private set; }

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		string text = context.FuncName().GetText();
		if (text.Equals("Cancel", StringComparison.OrdinalIgnoreCase))
		{
			Result = true;
		}
	}
}
