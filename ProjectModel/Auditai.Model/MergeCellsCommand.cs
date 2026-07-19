using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class MergeCellsCommand : CommandBase
{
	private Table _table;
	private int _topRow;
	private int _leftCol;
	private int _bottomRow;
	private int _rightCol;
	private bool _changeRowRole;
	private bool _hasExecuted;

	// 保存被移除的冲突合并及其原始 Status
	private List<Tuple<CellMerge, SyncStatus>> _removedMerges;
	// 保存被 RemoveMerge 添加到 RemovedMerges 的 ID（用于 Undo 清理）
	private List<Id64> _addedToRemovedMerges;
	// 保存行角色变更
	private RowRole? _oldRowRole;
	// 保存所有单元格的值、数据类型和样式（不仅仅是左上角）
	private List<Tuple<Cell, object, CellStyle>> _savedCellStates;
	// 保存创建的合并对象（Redo 时复用，保持 ID 一致）
	private CellMerge _createdMerge;

	public MergeCellsCommand(Table table, int topRow, int leftCol, int bottomRow, int rightCol, bool changeRowRole = false)
	{
		_table = table;
		_topRow = topRow;
		_leftCol = leftCol;
		_bottomRow = bottomRow;
		_rightCol = rightCol;
		_changeRowRole = changeRowRole;
		_hasExecuted = false;
		_removedMerges = new List<Tuple<CellMerge, SyncStatus>>();
		_addedToRemovedMerges = new List<Id64>();
		_savedCellStates = new List<Tuple<Cell, object, CellStyle>>();
	}

	public override void Execute()
	{
		if (!_hasExecuted)
		{
			_removedMerges.Clear();
			_addedToRemovedMerges.Clear();
			_savedCellStates.Clear();
			_oldRowRole = null;

			// 1. 保存所有单元格的值和样式（合并会清空非左上角单元格的值）
			foreach (var cell in _table.EnumerateCellRange(_topRow, _leftCol, _bottomRow, _rightCol))
			{
				_savedCellStates.Add(Tuple.Create(cell, cell.Value, cell.Style));
			}

			// 2. 解除冲突的合并（保存对象和 Status，记录添加到 RemovedMerges 的 ID）
			var beforeRemovedMerges = new HashSet<Id64>(_table.RemovedMerges);
			foreach (CellMerge item in _table.MergedCells.Where(m =>
				_table.AreMergesConflict(_topRow, _leftCol, _bottomRow, _rightCol,
					m.TopLeft.Row.Index, m.TopLeft.Column.Index,
					m.BottomRight.Row.Index, m.BottomRight.Column.Index)).ToList())
			{
				_removedMerges.Add(Tuple.Create(item, item.Status));
				_table.UnmergeCells(item.TopLeft.Row.Index, item.TopLeft.Column.Index);
			}
			// 记录 UnmergeCells 添加到 RemovedMerges 的 ID
			foreach (var id in _table.RemovedMerges)
				if (!beforeRemovedMerges.Contains(id))
					_addedToRemovedMerges.Add(id);

			// 3. 修改行角色
			if (_changeRowRole)
			{
				RowRole role = _table.Rows[_topRow].Role;
				if (role == RowRole.Normal || role == RowRole.Among || role == RowRole.Minus)
				{
					_oldRowRole = role;
					_table.Rows[_topRow].UpdateRole(RowRole.Fixed);
				}
			}

			// 4. 修改左上角单元格数据类型和样式
			Cell topLeftCell = _table[_topRow, _leftCol];
			topLeftCell.ChangeDataType(typeof(string));
			topLeftCell.UpdateStyle(_table.CellStyles.MutateAndGet(topLeftCell.Style, s =>
			{
				s.DataType = typeof(string);
			}));

			// 5. 合并单元格（会创建新的 CellMerge）
			_table.MergeCells(_topRow, _leftCol, _bottomRow, _rightCol);
			_createdMerge = _table.MergedCells.LastOrDefault();
			_hasExecuted = true;
		}
		else
		{
			// Redo：复用保存的合并对象，保持 ID 一致
			// 重新解除冲突合并
			foreach (var item in _removedMerges)
			{
				if (_table.MergedCells.Contains(item.Item1))
				{
					_table.MergedCells.Remove(item.Item1);
					_table.RemovedMerges.Add(item.Item1.Id);
				}
			}
			// 重新修改行角色
			if (_oldRowRole.HasValue)
			{
				_table.Rows[_topRow].UpdateRole(RowRole.Fixed);
			}
			// 重新修改单元格
			Cell topLeftCell = _table[_topRow, _leftCol];
			topLeftCell.ChangeDataType(typeof(string));
			topLeftCell.UpdateStyle(_table.CellStyles.MutateAndGet(topLeftCell.Style, s =>
			{
				s.DataType = typeof(string);
			}));
			// 重新合并（直接添加保存的合并对象，不创建新的）
			if (_createdMerge != null)
			{
				// 恢复单元格值（与首次执行一致）
				topLeftCell.UpdateValue(_savedCellStates[0].Item2);
				_table.MergedCells.Add(_createdMerge);
			}
		}
		_table.NeedSave = true;
	}

	public override void Undo()
	{
		// 1. 直接从 MergedCells 移除创建的合并（不使用 RemoveMerge，避免污染 RemovedMerges）
		if (_createdMerge != null && _table.MergedCells.Contains(_createdMerge))
		{
			_table.MergedCells.Remove(_createdMerge);
		}
		// 如果创建的合并是 New 状态，清理可能被添加到 RemovedMerges 的 ID
		if (_createdMerge != null)
		{
			_table.RemovedMerges.Remove(_createdMerge.Id);
			_table.MergesToDelete.Remove(_createdMerge.Id);
		}

		// 2. 恢复所有单元格的值和样式
		foreach (var item in _savedCellStates)
		{
			if (item.Item1.IsExisting)
			{
				item.Item1.UpdateValue(item.Item2);
				item.Item1.UpdateStyle(item.Item3);
			}
		}

		// 3. 恢复行角色
		if (_oldRowRole.HasValue)
		{
			_table.Rows[_topRow].UpdateRole(_oldRowRole.Value);
		}

		// 4. 恢复被移除的冲突合并（直接添加，恢复原始 Status）
		foreach (var item in _removedMerges)
		{
			item.Item1.Status = item.Item2;
			_table.MergedCells.Add(item.Item1);
		}

		// 5. 清理 RemovedMerges（移除 Execute 时 UnmergeCells 添加的 ID）
		foreach (var id in _addedToRemovedMerges)
		{
			_table.RemovedMerges.Remove(id);
		}

		_table.NeedSave = true;
	}
}
