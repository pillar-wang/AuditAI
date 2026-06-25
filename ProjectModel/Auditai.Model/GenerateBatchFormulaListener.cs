using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class GenerateBatchFormulaListener : FormulaBaseListener
{
	private string _funcName;

	private int _nestedLevel;

	public HashSet<string> ExistingTables { get; } = new HashSet<string>();


	public FormulaParser.ExprContext FirstExpr { get; private set; }

	public FormulaParser.ExprContext SecondExpr { get; private set; }

	public int InsertPosition { get; private set; }

	public GenerateBatchFormulaListener(string funcName)
	{
		_funcName = funcName;
	}

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		base.EnterFunc(context);
		if (!string.Equals(context.FuncName().GetText(), _funcName, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		_nestedLevel++;
		if (FirstExpr == null)
		{
			if (string.Equals(_funcName, "LqDistinct", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "LqAsc", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "LqDesc", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "Distinct", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "DistinctUp", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "DistinctDown", StringComparison.OrdinalIgnoreCase))
			{
				FirstExpr = context.expr(0);
				InsertPosition = context.Stop.StartIndex;
			}
			else if (string.Equals(_funcName, "LqFilter", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "LqVLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "LqSumIf", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "DistinctF", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "VLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(_funcName, "SumIf", StringComparison.OrdinalIgnoreCase))
			{
				FirstExpr = context.expr(0);
				SecondExpr = context.expr(1);
				InsertPosition = context.Stop.StartIndex;
			}
		}
	}

	public override void ExitFunc([NotNull] FormulaParser.FuncContext context)
	{
		if (string.Equals(context.FuncName().GetText(), _funcName, StringComparison.OrdinalIgnoreCase))
		{
			_nestedLevel--;
		}
		base.ExitFunc(context);
	}

	public override void EnterRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		base.EnterRefColumn(context);
		if (_nestedLevel > 0)
		{
			ExistingTables.Add(context.Int(0).GetText());
		}
	}
}
