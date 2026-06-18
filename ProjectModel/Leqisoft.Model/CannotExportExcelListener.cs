using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class CannotExportExcelListener : FormulaBaseListener
{
	private static HashSet<string> _funcNames = new HashSet<string>(new string[56]
	{
		"Select", "Index", "Split", "Replace", "LqCountIf", "LqDistinct", "LqFilter", "LqMax", "LqMin", "LqSumIf",
		"LqVLookUp", "Distinct", "DistinctF", "DistinctUp", "DistinctDown", "Collect", "CollectF", "VLookUp", "SumIf", "CountIf",
		"MaxF", "MinF", "SumFind", "CrossTable", "ExtractText", "ExtractNumber", "YearMonth", "Years", "Months", "Days",
		"WideChar", "Concat", "DaysInMonth", "Union", "Intersect", "IsDate", "IsBool", "Var", "Title", "Foot",
		"TicketTitle", "TicketFoot", "Col", "Sheet", "Sort", "RowCreator", "LqCrossTable", "SizeOf", "CountSub", "Now",
		"Today", "ArraySum", "SortUp", "SortDown", "Repeat", "Join"
	}, StringComparer.InvariantCultureIgnoreCase);

	private CannotExportExcelContext _context;

	private int _sumDepth;

	public bool ForbidExport { get; private set; }

	public CannotExportExcelListener(CannotExportExcelContext context)
	{
		_context = context;
	}

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		if (ForbidExport)
		{
			return;
		}
		string text = context.FuncName().GetText();
		if ("Sum".Equals(text, StringComparison.OrdinalIgnoreCase))
		{
			if (_context.IsExistUnNormalRow)
			{
				ForbidExport = true;
			}
			else
			{
				_sumDepth++;
			}
		}
		else
		{
			ForbidExport = _funcNames.Contains(text);
		}
	}

	public override void ExitFunc([NotNull] FormulaParser.FuncContext context)
	{
		if (!ForbidExport)
		{
			string text = context.FuncName().GetText();
			if ("Sum".Equals(text, StringComparison.OrdinalIgnoreCase))
			{
				_sumDepth--;
			}
		}
	}

	public override void EnterRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		if (!ForbidExport && _sumDepth > 0 && Id64.Parse(context.Int(0).GetText()).Value != _context.CurrentTableId)
		{
			ForbidExport = true;
		}
	}

	public override void EnterRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		if (!ForbidExport && _sumDepth > 0 && Id64.Parse(context.Int(0).GetText()).Value != _context.CurrentTableId)
		{
			ForbidExport = true;
		}
	}
}
