using System;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class IsLedgerCollectFillFormula : FormulaBaseListener
{
	public bool IsFill { get; private set; }

	public bool IsLqAsc { get; private set; }

	public bool IsLqDesc { get; private set; }

	public bool IsLqCollect { get; private set; }

	public override void EnterFormula([NotNull] FormulaParser.FormulaContext context)
	{
		if (context.expr() is FormulaParser.FuncContext func)
		{
			CheckFunctionIsContainsFillFunction(func);
		}
	}

	private bool CheckFunctionIsContainsFillFunction(FormulaParser.FuncContext func)
	{
		string text = func.FuncName().GetText();
		if (CheckIsFillFunction(text))
		{
			return true;
		}
		if (StringComparer.OrdinalIgnoreCase.Equals("Select", text))
		{
			if (CheckSelectFunctionIsContainsFillFunction(func))
			{
				return true;
			}
			return false;
		}
		if (StringComparer.OrdinalIgnoreCase.Equals("If", text))
		{
			if (CheckIfFunctionIsContainsFillFunction(func))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private bool CheckSelectFunctionIsContainsFillFunction(FormulaParser.FuncContext func)
	{
		int num = 0;
		for (int i = 2; i < func.ChildCount - 1; i++)
		{
			num++;
			switch (num)
			{
			case 4:
				num = 0;
				break;
			case 3:
				if (func.GetChild(i).Payload is FormulaParser.FuncContext func2 && CheckFunctionIsContainsFillFunction(func2))
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	private bool CheckIfFunctionIsContainsFillFunction(FormulaParser.FuncContext func)
	{
		if (func.ChildCount > 4 && func.GetChild(4).Payload is FormulaParser.FuncContext func2 && CheckFunctionIsContainsFillFunction(func2))
		{
			return true;
		}
		if (func.ChildCount > 6 && func.GetChild(6).Payload is FormulaParser.FuncContext func3 && CheckFunctionIsContainsFillFunction(func3))
		{
			return true;
		}
		return false;
	}

	private bool CheckIsFillFunction(string funcName)
	{
		bool result = false;
		if (IsFillFormula._funcNames.Contains(funcName))
		{
			result = true;
			IsFill = true;
			if (funcName.Equals("LqAsc", StringComparison.OrdinalIgnoreCase) || funcName.Equals("DistinctUp", StringComparison.OrdinalIgnoreCase))
			{
				IsLqAsc = true;
			}
			else if (funcName.Equals("LqDesc", StringComparison.OrdinalIgnoreCase) || funcName.Equals("DistinctDown", StringComparison.OrdinalIgnoreCase))
			{
				IsLqDesc = true;
			}
		}
		if (funcName.Equals("LqCollect", StringComparison.OrdinalIgnoreCase) || funcName.Equals("CollectF", StringComparison.OrdinalIgnoreCase) || funcName.Equals("Collect", StringComparison.OrdinalIgnoreCase))
		{
			IsLqCollect = true;
		}
		return result;
	}
}
