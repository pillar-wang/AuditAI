using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class GetRefTableColumnAndCellListener : FormulaBaseListener
{
	private readonly Table _table;

	public HashSet<Cell> ReferredCells { get; private set; }

	public HashSet<Column> ReferredColumns { get; private set; }

	public GetRefTableColumnAndCellListener(Table table)
	{
		_table = table;
	}

	public override void EnterRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		Id64 id2 = Id64.Parse(context.Int(1).GetText());
		if (!(id == _table.Id))
		{
			return;
		}
		Column byId = _table.Columns.GetById(id2);
		if (byId != null)
		{
			if (ReferredColumns == null)
			{
				ReferredColumns = new HashSet<Column>();
			}
			ReferredColumns.Add(byId);
		}
	}

	public override void EnterRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		Id64 id2 = Id64.Parse(context.Int(1).GetText());
		if (!(id == _table.Id))
		{
			return;
		}
		Column byId = _table.Columns.GetById(id2);
		if (byId != null)
		{
			if (ReferredColumns == null)
			{
				ReferredColumns = new HashSet<Column>();
			}
			ReferredColumns.Add(byId);
		}
	}

	public override void EnterRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		Id64 id2 = Id64.Parse(context.Int(1).GetText());
		if (!(id == _table.Id))
		{
			return;
		}
		Cell byId = _table.Cells.GetById(id2);
		if (byId != null)
		{
			if (ReferredCells == null)
			{
				ReferredCells = new HashSet<Cell>();
			}
			ReferredCells.Add(byId);
		}
	}
}
