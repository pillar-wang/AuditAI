using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class DeleteColumnsCommand : CommandBase
{
	private Table _table;
	private int _index;
	private int _count;
	private List<CellMerge> _originalMerges;

	// 保存被删除的 Column 对象（保留原始 ID）
	private List<Column> _savedColumns;
	// 保存被删除的 Cell 对象（按行排列，匹配 Cells._list 的顺序）
	private List<Cell> _savedCells;
	// 保存原始 Status
	private List<Tuple<Column, SyncStatus>> _savedColumnStatus;
	private List<Tuple<Cell, SyncStatus>> _savedCellStatus;
	// 保存有公式的单元格
	private List<Cell> _formulaCells;
	// 保存有公式的列
	private List<Column> _formulaColumns;
	// 保存被 Remove 添加到同步列表的 ID
	private HashSet<Id64> _addedToRemovedColumns;
	private HashSet<Id64> _addedToColumnsToDelete;
	private HashSet<Id64> _addedToRemovedCells;
	private HashSet<Id64> _addedToCellsToDelete;
	// 保存原始列数（Undo 时需要知道插入前的列数）
	private int _savedColCount;

	public DeleteColumnsCommand(Table table, int index, int count, List<CellMerge> originalMerges = null)
	{
		_table = table;
		_index = index;
		_count = count;
		_originalMerges = originalMerges ?? new List<CellMerge>();
		_savedColumns = new List<Column>();
		_savedCells = new List<Cell>();
		_savedColumnStatus = new List<Tuple<Column, SyncStatus>>();
		_savedCellStatus = new List<Tuple<Cell, SyncStatus>>();
		_formulaCells = new List<Cell>();
		_formulaColumns = new List<Column>();
		_addedToRemovedColumns = new HashSet<Id64>();
		_addedToColumnsToDelete = new HashSet<Id64>();
		_addedToRemovedCells = new HashSet<Id64>();
		_addedToCellsToDelete = new HashSet<Id64>();
	}

	public override void Execute()
	{
		_savedColumns.Clear();
		_savedCells.Clear();
		_savedColumnStatus.Clear();
		_savedCellStatus.Clear();
		_formulaCells.Clear();
		_formulaColumns.Clear();
		_addedToRemovedColumns.Clear();
		_addedToColumnsToDelete.Clear();
		_addedToRemovedCells.Clear();
		_addedToCellsToDelete.Clear();

		_savedColCount = _table.Columns.Count;

		// 1. 保存同步列表的当前状态
		var beforeRemovedColumns = new HashSet<Id64>(_table.RemovedColumns);
		var beforeColumnsToDelete = new HashSet<Id64>(_table.ColumnsToDelete);
		var beforeRemovedCells = new HashSet<Id64>(_table.RemovedCells);
		var beforeCellsToDelete = new HashSet<Id64>(_table.CellsToDelete);

		// 2. 保存 Column 对象和原始 Status
		for (int i = _index; i < _index + _count && i < _table.Columns.Count; i++)
		{
			var col = _table.Columns[i];
			_savedColumns.Add(col);
			_savedColumnStatus.Add(Tuple.Create(col, col.Status));
			if (col.HasFormula)
			{
				_formulaColumns.Add(col);
			}
		}

		// 3. 保存 Cell 对象（按行排列，匹配 Cells._list 的顺序）
		for (int i = 0; i < _table.Rows.Count; i++)
		{
			for (int j = _index; j < _index + _count && j < _table.Columns.Count; j++)
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

		// 4. 执行删除
		_table.Columns.Remove(_index, _count);

		// 5. 记录 Remove 添加到同步列表的 ID
		foreach (var id in _table.RemovedColumns)
			if (!beforeRemovedColumns.Contains(id))
				_addedToRemovedColumns.Add(id);
		foreach (var id in _table.ColumnsToDelete)
			if (!beforeColumnsToDelete.Contains(id))
				_addedToColumnsToDelete.Add(id);
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
		int rowCount = _table.Rows.Count;
		int currentColCount = _table.Columns.Count;

		// 1. 将保存的 Column 对象直接重新插入内部列表（保留原始 ID）
		_table.Columns._list.InsertRange(_index, _savedColumns);
		_table.Columns.ResetIndex();

		// 2. 将保存的 Cell 对象重新插入 Cells._list
		// 删除后每行的列数为 currentColCount，需要在每个行的 _index 位置插入 _count 个单元格
		// 从最后一行开始插入，避免索引偏移
		for (int i = rowCount - 1; i >= 0; i--)
		{
			int insertPos = i * currentColCount + _index;
			int cellStart = i * _count;
			_table.Cells._list.InsertRange(insertPos, _savedCells.GetRange(cellStart, _count));
		}

		// 3. 恢复 Column 的 Status
		foreach (var item in _savedColumnStatus)
		{
			item.Item1.Status = item.Item2;
		}

		// 4. 恢复 Cell 的 Status
		foreach (var item in _savedCellStatus)
		{
			item.Item1.Status = item.Item2;
			item.Item1.NeedSave = true;
		}

		// 5. 清理同步列表
		foreach (var id in _addedToRemovedColumns)
			_table.RemovedColumns.Remove(id);
		foreach (var id in _addedToColumnsToDelete)
			_table.ColumnsToDelete.Remove(id);
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
		foreach (var col in _formulaColumns)
		{
			_table.Project.FormulaManager.ReplaceHostColumn(new FormulaRecord
			{
				TableId = _table.Id,
				ObjectId = col.Id,
				Formula = col.Formula
			});
		}

		// 7. 移除合并调整产生的合并，恢复原始合并
		var currentMerges = _table.MergedCells.Where(m =>
			(m.TopLeft.Column.Index >= _index && m.TopLeft.Column.Index < _index + _count) ||
			(m.BottomRight.Column.Index >= _index && m.BottomRight.Column.Index < _index + _count)).ToList();
		foreach (var merge in currentMerges)
		{
			_table.RemoveMerge(merge);
		}
		foreach (var merge in _originalMerges)
		{
			_table.MergedCells.Add(merge);
		}

		// 8. 刷新公式和缓存
		_table.Project.FormulaMapDirty = true;
		_table.Ticket.IsCacheExpired = true;
		FormulaEvaluator.ClearCache();
		_table.NeedSave = true;
	}
}
