using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class GetReferredTablesListener : FormulaBaseListener
{
	public HashSet<Id64> ReferredTables { get; } = new HashSet<Id64>();


	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		ReferredTables.Add(Id64.Parse(context.Int(0).GetText()));
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		ReferredTables.Add(Id64.Parse(context.Int(0).GetText()));
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		ReferredTables.Add(Id64.Parse(context.Int(0).GetText()));
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		ReferredTables.Add(Id64.Parse(context.Int(0).GetText()));
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		ReferredTables.Add(Id64.Parse(context.Int(0).GetText()));
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		ReferredTables.Add(Id64.Parse(context.Int(0).GetText()));
	}

	public override void ExitRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context)
	{
		ReferredTables.Add(Id64.Parse(context.Int().GetText()));
	}
}
