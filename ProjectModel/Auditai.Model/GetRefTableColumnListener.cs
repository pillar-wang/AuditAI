using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Auditai.DTO;

namespace Auditai.Model;

public class GetRefTableColumnListener : FormulaBaseListener
{
	private readonly Table _table;

	public List<Column> ReferredColumns { get; private set; }

	public GetRefTableColumnListener(Table table)
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
				ReferredColumns = new List<Column>();
			}
			if (!ReferredColumns.Contains(byId))
			{
				ReferredColumns.Add(byId);
			}
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
				ReferredColumns = new List<Column>();
			}
			if (!ReferredColumns.Contains(byId))
			{
				ReferredColumns.Add(byId);
			}
		}
	}
}
