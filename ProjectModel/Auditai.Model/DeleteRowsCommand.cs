using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class DeleteRowsCommand : CommandBase
{
	private Table _table;
	private int _index;
	private int _count;
	private List<CellMerge> _originalMerges;

	// 保存被删除的 Row 对象（保留原始 ID）
	private List<Row> _savedRows;
	// 保存被删除的 Cell 对象（保留原始 ID、公式等）
	private List<Cell> _savedCells;
	// 保存原始 Status（Remove 会改为 LocalDeleted）
	private List<Tuple<Row, SyncStatus>> _savedRowStatus;
	private List<Tuple<Cell, SyncStatus>> _savedCellStatus;
	// 保存有公式的单元格（需要在 Undo 时重新注册）
	private List<Cell> _formulaCells;
	// 保存 HeaderRowCache 中被移除的行
	private List<Row> _savedHeaderRows;
	// 保存被 Remove 添加到同步列表的 ID
	private HashSet<Id64> _addedToRemovedRows;
	private HashSet<Id64> _addedToRowsToDelete;
	private HashSet<Id64> _addedToRemovedCells;
	private HashSet<Id64> _addedToCellsToDelete;

	public DeleteRowsCommand(Table table, int index, int count, List<CellMerge> originalMerges = null)
	{
		_table = table;
		_index = index;
		_count = count;
		_originalMerges = originalMerges ?? new List<CellMerge>();
		_savedRows = new List<Row>();
		_savedCells = new List<Cell>();
		_savedRowStatus = new List<Tuple<Row, SyncStatus>>();
		_savedCellStatus = new List<Tuple<Cell, SyncStatus>>();
		_formulaCells = new List<Cell>();
		_savedHeaderRows = new List<Row>();
		_addedToRemovedRows = new HashSet<Id64>();
		_addedToRowsToDelete = new HashSet<Id64>();
		_addedToRemovedCells = new HashSet<Id64>();
		_addedToCellsToDelete = new HashSet<Id64>();
	}

	public override void Execute()
	{
		_savedRows.Clear();
		_savedCells.Clear();
		_savedRowStatus.Clear();
		_savedCellStatus.Clear();
		_formulaCells.Clear();
		_savedHeaderRows.Clear();
		_addedToRemovedRows.Clear();
		_addedToRowsToDelete.Clear();
		_addedToRemovedCells.Clear();
		_addedToCellsToDelete.Clear();

		// 1. 保存同步列表的当前状态（用于后续计算 Remove 添加了什么）
		var beforeRemovedRows = new HashSet<Id64>(_table.RemovedRows);
		var beforeRowsToDelete = new HashSet<Id64>(_table.RowsToDelete);
		var beforeRemovedCells = new HashSet<Id64>(_table.RemovedCells);
		var beforeCellsToDelete = new HashSet<Id64>(_table.CellsToDelete);

		// 2. 保存 Row 对象和原始 Status
		for (int i = _index; i < _index + _count && i < _table.Rows.Count; i++)
		{
			var row = _table.Rows[i];
			_savedRows.Add(row);
			_savedRowStatus.Add(Tuple.Create(row, row.Status));
			if (_table.HeaderRowCache.Contains(row))
			{
				_savedHeaderRows.Add(row);
			}
		}

		// 3. 保存 Cell 对象和原始 Status
		int colCount = _table.Columns.Count;
		for (int i = _index; i < _index + _count && i < _table.Rows.Count; i++)
		{
			for (int j = 0; j < colCount; j++)
			{
				var cell = _table[i, j];
				_savedCells.Add(cell);
				_savedCellStatus.Add(Tuple.Create(cell, cell.Status));
				if (cell.HasFormula)
				{
					_formulaCells.Add(cell);
				}
			}
		}

		// 4. 执行删除（会修改 Status、添加到同步列表、移除公式主机对象、从内部列表移除）
		_table.Rows.Remove(_index, _count);

		// 5. 记录 Remove 添加到同步列表的 ID（用于 Undo 时清理）
		foreach (var id in _table.RemovedRows)
			if (!beforeRemovedRows.Contains(id))
				_addedToRemovedRows.Add(id);
		foreach (var id in _table.RowsToDelete)
			if (!beforeRowsToDelete.Contains(id))
				_addedToRowsToDelete.Add(id);
		foreach (var id in _table.RemovedCells)
			if (!beforeRemovedCells.Contains(id))
				_addedToRemovedCells.Add(id);
		foreach (var id in _table.CellsToDelete)
			if (!beforeCellsToDelete.Contains(id))
				_addedToCellsToDelete.Add(id);

		_table.NeedSave = true;
	}

	public override void Undo()
	{
		int colCount = _table.Columns.Count;

		// 1. 将保存的 Row 对象直接重新插入内部列表（保留原始 ID）
		_table.Rows._list.InsertRange(_index, _savedRows);
		_table.Rows.ResetIndex();

		// 2. 将保存的 Cell 对象直接重新插入内部列表
		int cellInsertPos = _index * colCount;
		_table.Cells._list.InsertRange(cellInsertPos, _savedCells);

		// 3. 恢复 Row 的 Status
		foreach (var item in _savedRowStatus)
		{
			item.Item1.Status = item.Item2;
			item.Item1.NeedSave = true;
		}

		// 4. 恢复 Cell 的 Status
		foreach (var item in _savedCellStatus)
		{
			item.Item1.Status = item.Item2;
			item.Item1.NeedSave = true;
		}

		// 5. 清理同步列表（移除 Remove 时添加的 ID）
		foreach (var id in _addedToRemovedRows)
			_table.RemovedRows.Remove(id);
		foreach (var id in _addedToRowsToDelete)
			_table.RowsToDelete.Remove(id);
		foreach (var id in _addedToRemovedCells)
			_table.RemovedCells.Remove(id);
		foreach (var id in _addedToCellsToDelete)
			_table.CellsToDelete.Remove(id);

		// 6. 重新注册公式主机对象
		foreach (var cell in _formulaCells)
		{
			_table.Project.FormulaManager.ReplaceHostCell(new FormulaRecord
			{
				TableId = _table.Id,
				ObjectId = cell.Id,
				Formula = cell.Formula
			});
		}

		// 7. 恢复 HeaderRowCache
		foreach (var row in _savedHeaderRows)
		{
			_table.HeaderRowCache.Add(row);
		}

		// 8. 移除合并调整产生的合并，恢复原始合并
		var currentMerges = _table.MergedCells.Where(m =>
			(m.TopLeft.Row.Index >= _index && m.TopLeft.Row.Index < _index + _count) ||
			(m.BottomRight.Row.Index >= _index && m.BottomRight.Row.Index < _index + _count)).ToList();
		foreach (var merge in currentMerges)
		{
			_table.RemoveMerge(merge);
		}
		foreach (var merge in _originalMerges)
		{
			_table.MergedCells.Add(merge);
		}

		// 9. 刷新公式和缓存
		_table.Project.FormulaMapDirty = true;
		_table.Ticket.IsCacheExpired = true;
		FormulaEvaluator.ClearCache();
		_table.NeedSave = true;
	}
}
