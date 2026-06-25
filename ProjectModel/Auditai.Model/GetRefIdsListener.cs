using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Auditai.DTO;

namespace Auditai.Model;

public class GetRefIdsListener : FormulaBaseListener
{
	public List<Tuple<Id64, Id64, FormulaDependencyObjectKind>> Result { get; } = new List<Tuple<Id64, Id64, FormulaDependencyObjectKind>>();


	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 item = Id64.Parse(context.Int(0).GetText());
		Id64 item2 = Id64.Parse(context.Int(1).GetText());
		Result.Add(Tuple.Create(item, item2, FormulaDependencyObjectKind.Cell));
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		Id64 item = Id64.Parse(context.Int(0).GetText());
		Id64 item2 = Id64.Parse(context.Int(1).GetText());
		Result.Add(Tuple.Create(item, item2, FormulaDependencyObjectKind.HeaderCell));
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		Id64 item = Id64.Parse(context.Int(0).GetText());
		Id64 item2 = Id64.Parse(context.Int(1).GetText());
		Result.Add(Tuple.Create(item, item2, FormulaDependencyObjectKind.HeaderCell));
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 item = Id64.Parse(context.Int(0).GetText());
		Id64 item2 = Id64.Parse(context.Int(1).GetText());
		Result.Add(Tuple.Create(item, item2, FormulaDependencyObjectKind.Column));
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 item = Id64.Parse(context.Int(0).GetText());
		Id64 item2 = Id64.Parse(context.Int(1).GetText());
		Result.Add(Tuple.Create(item, item2, FormulaDependencyObjectKind.Column));
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 item = Id64.Parse(context.Int(0).GetText());
		Id64 item2 = Id64.Parse(context.Int(1).GetText());
		Id64 item3 = Id64.Parse(context.Int(2).GetText());
		Result.Add(Tuple.Create(item, item2, FormulaDependencyObjectKind.Cell));
		Result.Add(Tuple.Create(item, item3, FormulaDependencyObjectKind.Cell));
	}
}
