using System;
using System.Collections.Generic;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class FormulaReferenceModelResolverForRecycleFile : FormulaReferenceModelResolver
{
	protected Id64 _recycledTableId;

	protected Id64 _newTableId;

	protected TreeTableNode _newTableNode;

	protected Dictionary<Id64, Cell> _oldCellDic;

	protected Dictionary<Id64, Column> _oldColumnDic;

	protected Dictionary<Id64, Cell> _newCellDic;

	protected Dictionary<Id64, Column> _newColumnDic;

	protected internal override Func<Id64, Id64, Cell> ResolveTableCell => delegate(Id64 tableId, Id64 cellId)
	{
		if (tableId == _recycledTableId)
		{
			return _oldCellDic[cellId] ?? throw new FormulaBadReferenceException();
		}
		return (tableId == _newTableId) ? (_newCellDic[cellId] ?? throw new FormulaBadReferenceException()) : base.ResolveTableCell(tableId, cellId);
	};

	protected internal override Func<Id64, Id64, Column> ResolveTableColumn => delegate(Id64 tableId, Id64 columnId)
	{
		if (tableId == _recycledTableId)
		{
			return _oldColumnDic[columnId] ?? throw new FormulaBadReferenceException();
		}
		return (tableId == _newTableId) ? (_newColumnDic[columnId] ?? throw new FormulaBadReferenceException()) : base.ResolveTableColumn(tableId, columnId);
	};

	protected internal override Func<Id64, Id64, Id64, RangeOperand> ResolveTableRange => delegate(Id64 tableId, Id64 topLeftId, Id64 bottomRightId)
	{
		if (tableId == _recycledTableId)
		{
			Cell topLeft = _oldCellDic[topLeftId] ?? throw new FormulaBadReferenceException();
			Cell bottomRight = _oldCellDic[bottomRightId] ?? throw new FormulaBadReferenceException();
			return new RangeOperand(topLeft, bottomRight) ?? throw new FormulaBadReferenceException();
		}
		if (tableId == _newTableId)
		{
			Cell topLeft2 = _newCellDic[topLeftId] ?? throw new FormulaBadReferenceException();
			Cell bottomRight2 = _newCellDic[bottomRightId] ?? throw new FormulaBadReferenceException();
			return new RangeOperand(topLeft2, bottomRight2) ?? throw new FormulaBadReferenceException();
		}
		return base.ResolveTableRange(tableId, topLeftId, bottomRightId);
	};

	protected internal override Func<Id64, TreeNodeBase> ResolveTreeNode => delegate(Id64 nodeId)
	{
		if (nodeId == _recycledTableId)
		{
			return _newTableNode;
		}
		return (nodeId == _newTableId) ? _newTableNode : base.ResolveTreeNode(nodeId);
	};

	public FormulaReferenceModelResolverForRecycleFile(Project project, Id64 recycledTableId, TreeTableNode newTableNode, Dictionary<Id64, Cell> oldCellDic, Dictionary<Id64, Column> oldColumnDic, Dictionary<Id64, Cell> newCellDic, Dictionary<Id64, Column> newColumnDic)
		: base(project)
	{
		_recycledTableId = recycledTableId;
		_newTableId = newTableNode.Id;
		_newTableNode = newTableNode;
		_oldCellDic = oldCellDic;
		_oldColumnDic = oldColumnDic;
		_newCellDic = newCellDic;
		_newColumnDic = newColumnDic;
	}
}
