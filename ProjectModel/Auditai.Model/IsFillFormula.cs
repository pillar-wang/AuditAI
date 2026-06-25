using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class IsFillFormula : FormulaBaseListener
{
	internal static HashSet<string> _funcNames = new HashSet<string>(new string[14]
	{
		"LqDistinct", "LqFilter", "LqMax", "LqMin", "LqAsc", "LqDesc", "LqCrossTable", "Distinct", "DistinctF", "MaxF",
		"MinF", "DistinctUp", "DistinctDown", "CrossTable"
	}, StringComparer.OrdinalIgnoreCase);

	public bool IsFill { get; private set; }

	public bool IsLqAsc { get; private set; }

	public bool IsLqDesc { get; private set; }

	public bool IsLqCollect { get; private set; }

	public override void EnterFormula([NotNull] FormulaParser.FormulaContext context)
	{
		if (!(context.expr() is FormulaParser.FuncContext funcContext))
		{
			return;
		}
		string text = funcContext.FuncName().GetText();
		if (_funcNames.Contains(text))
		{
			IsFill = true;
			if (text.Equals("LqAsc", StringComparison.OrdinalIgnoreCase) || text.Equals("DistinctUp", StringComparison.OrdinalIgnoreCase))
			{
				IsLqAsc = true;
			}
			else if (text.Equals("LqDesc", StringComparison.OrdinalIgnoreCase) || text.Equals("DistinctDown", StringComparison.OrdinalIgnoreCase))
			{
				IsLqDesc = true;
			}
		}
		if (text.Equals("LqCollect", StringComparison.OrdinalIgnoreCase) || text.Equals("CollectF", StringComparison.OrdinalIgnoreCase) || text.Equals("Collect", StringComparison.OrdinalIgnoreCase))
		{
			IsLqCollect = true;
		}
	}
}
