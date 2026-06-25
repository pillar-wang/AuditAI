using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Auditai.DTO;

namespace Auditai.Model;

public class GetRefEntriesListener : FormulaBaseListener
{
	public List<FormulaRefInfo> Result { get; } = new List<FormulaRefInfo>();


	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 tableId = Id64.Parse(context.Int(0).GetText());
		Id64 id = Id64.Parse(context.Int(1).GetText());
		Result.Add(new FormulaRefInfo
		{
			Kind = FormulaHostKind.Cell,
			TableId = tableId,
			Id1 = id
		});
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		Id64 tableId = Id64.Parse(context.Int(0).GetText());
		Id64 id = Id64.Parse(context.Int(1).GetText());
		Result.Add(new FormulaRefInfo
		{
			Kind = FormulaHostKind.HeaderCell,
			TableId = tableId,
			Id1 = id
		});
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		Id64 tableId = Id64.Parse(context.Int(0).GetText());
		Id64 id = Id64.Parse(context.Int(1).GetText());
		Result.Add(new FormulaRefInfo
		{
			Kind = FormulaHostKind.HeaderCellWildcard,
			TableId = tableId,
			Id1 = id
		});
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 tableId = Id64.Parse(context.Int(0).GetText());
		Id64 id = Id64.Parse(context.Int(1).GetText());
		Result.Add(new FormulaRefInfo
		{
			Kind = FormulaHostKind.Column,
			TableId = tableId,
			Id1 = id
		});
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 tableId = Id64.Parse(context.Int(0).GetText());
		Id64 id = Id64.Parse(context.Int(1).GetText());
		Result.Add(new FormulaRefInfo
		{
			Kind = FormulaHostKind.ColumnWildcard,
			TableId = tableId,
			Id1 = id
		});
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 tableId = Id64.Parse(context.Int(0).GetText());
		Id64 id = Id64.Parse(context.Int(1).GetText());
		Id64 id2 = Id64.Parse(context.Int(2).GetText());
		Result.Add(new FormulaRefInfo
		{
			Kind = FormulaHostKind.Range,
			TableId = tableId,
			Id1 = id,
			Id2 = id2
		});
	}
}
