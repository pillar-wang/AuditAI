using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;

namespace Leqisoft.Model;

public class FunctionNameExistsListener : FormulaBaseListener
{
	private HashSet<string> _funcNames;

	public string FunctionName { get; private set; }

	public FunctionNameExistsListener(IEnumerable<string> funcNames)
	{
		_funcNames = new HashSet<string>(funcNames, StringComparer.OrdinalIgnoreCase);
	}

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		if (FunctionName == null)
		{
			string text = context.FuncName().GetText();
			if (_funcNames.Contains(text))
			{
				FunctionName = text;
			}
		}
	}
}
