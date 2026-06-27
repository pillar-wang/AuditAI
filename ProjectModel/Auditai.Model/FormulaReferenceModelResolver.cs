﻿﻿﻿﻿using System;
using Auditai.DTO;

namespace Auditai.Model;

public class FormulaReferenceModelResolver : FormulaReferenceResolver
{
	private Project _project;

	protected internal override Func<Id64, Id64, Cell> ResolveTableCell => (Id64 tableId, Id64 cellId) => _project.GetTableById(tableId)?.LoadAndReturn()?.GetCellById(cellId) ?? throw new FormulaBadReferenceException();

	protected internal override Func<Id64, Id64, Column> ResolveTableColumn => (Id64 tableId, Id64 columnId) =>
	{
		System.Diagnostics.Debug.WriteLine($"[FormulaReferenceModelResolver] ResolveTableColumn: tableId={tableId}, columnId={columnId}");
		var tbl = _project.GetTableById(tableId);
		System.Diagnostics.Debug.WriteLine($"[FormulaReferenceModelResolver] ResolveTableColumn: GetTableById returned {(tbl == null ? "NULL" : $"Id={tbl.Id}, _loaded={tbl._loaded}")}");
		if (tbl != null)
		{
			tbl.LoadAndReturn();
			System.Diagnostics.Debug.WriteLine($"[FormulaReferenceModelResolver] ResolveTableColumn: after LoadAndReturn, Rows.Count={tbl.Rows?.Count}, Columns.Count={tbl.Columns?.Count}, Cells.Count={tbl.Cells?.Count}");
			var col = tbl.Columns?.GetById(columnId);
			System.Diagnostics.Debug.WriteLine($"[FormulaReferenceModelResolver] ResolveTableColumn: GetById({columnId}) returned {(col == null ? "NULL" : $"Caption='{col.Caption}', StyleId={col.Style?.Id}")}");
			if (col != null) return col;
		}
		throw new FormulaBadReferenceException();
	};

	protected internal override Func<Id64, Id64, Id64, RangeOperand> ResolveTableRange => delegate(Id64 tableId, Id64 topLeftId, Id64 bottomRightId)
	{
		Table table = _project.GetTableById(tableId).LoadAndReturn();
		if (table == null)
		{
			throw new FormulaBadReferenceException();
		}
		Cell topLeft = table.GetCellById(topLeftId) ?? throw new FormulaBadReferenceException();
		Cell bottomRight = table.GetCellById(bottomRightId) ?? throw new FormulaBadReferenceException();
		return new RangeOperand(topLeft, bottomRight) ?? throw new FormulaBadReferenceException();
	};

	protected internal override Func<Id64, TreeNodeBase> ResolveTreeNode => (Id64 nodeId) => _project.GetNodeById(nodeId) ?? throw new FormulaBadReferenceException();

	protected internal override Func<string, TreeNodeBase> ResolveTreeNodeString => delegate(string name)
	{
		TreeNodeBase treeNodeByCanonicalName = _project.GetTreeNodeByCanonicalName(name);
		if (treeNodeByCanonicalName == null)
		{
			return (TreeNodeBase)null;
		}
		if (treeNodeByCanonicalName is TreeTableNode treeTableNode)
		{
			treeTableNode.Table.LoadAndReturn();
		}
		return treeNodeByCanonicalName;
	};

	public FormulaReferenceModelResolver(Project project)
	{
		_project = project;
	}
}
