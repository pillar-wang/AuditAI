using System.Collections.Generic;

namespace Auditai.Model;

public class InsertRowsCommand : CommandBase
{
	private Table _table;
	private int _index;
	private int _count;

	// 保存创建的 Row 和 Cell 对象，使 Redo 时复用同一批对象（保持 ID 一致）
	private List<Row> _createdRows;
	private List<Cell> _createdCells;
	private bool _hasExecuted;

	public InsertRowsCommand(Table table, int index, int count)
	{
		_table = table;
		_index = index;
		_count = count;
		_createdRows = new List<Row>();
		_createdCells = new List<Cell>();
		_hasExecuted = false;
	}

	public override void Execute()
	{
		if (!_hasExecuted)
		{
			// 首次执行：通过 Insert 创建新行，然后保存创建的对象引用
			int rowsBefore = _table.Rows.Count;
			_table.Rows.Insert(_index, _count);
			// 保存新创建的 Row 对象
			for (int i = _index; i < _index + _count && i < _table.Rows.Count; i++)
			{
				_createdRows.Add(_table.Rows[i]);
			}
			// 保存新创建的 Cell 对象
			int colCount = _table.Columns.Count;
			for (int i = _index; i < _index + _count && i < _table.Rows.Count; i++)
			{
				for (int j = 0; j < colCount; j++)
				{
					_createdCells.Add(_table[i, j]);
				}
			}
			_hasExecuted = true;
		}
		else
		{
			// Redo：直接重新插入之前创建的对象（保持 ID 一致）
			int colCount = _table.Columns.Count;
			_table.Rows._list.InsertRange(_index, _createdRows);
			_table.Rows.ResetIndex();
			int cellInsertPos = _index * colCount;
			_table.Cells._list.InsertRange(cellInsertPos, _createdCells);
			// 恢复 Status（Remove 会改为 LocalDeleted）
			foreach (var row in _createdRows)
			{
				row.Status = SyncStatus.New;
				row.NeedSave = true;
			}
			foreach (var cell in _createdCells)
			{
				cell.Status = SyncStatus.New;
				cell.NeedSave = true;
			}
			// 清理 Remove 添加到同步列表的 ID
			foreach (var row in _createdRows)
			{
				_table.RowsToDelete.Remove(row.Id);
				_table.RemovedRows.Remove(row.Id);
			}
			foreach (var cell in _createdCells)
			{
				_table.CellsToDelete.Remove(cell.Id);
				_table.RemovedCells.Remove(cell.Id);
			}
			_table.Project.FormulaMapDirty = true;
			_table.Ticket.IsCacheExpired = true;
			FormulaEvaluator.ClearCache();
			_table.NeedSave = true;
		}
	}

	public override void Undo()
	{
		// 直接从内部列表移除（而不是调用 Rows.Remove，避免修改同步列表）
		int colCount = _table.Columns.Count;
		// 移除 Cell
		int cellRemovePos = _index * colCount;
		int cellRemoveCount = _count * colCount;
		_table.Cells._list.RemoveRange(cellRemovePos, cellRemoveCount);
		// 移除 Row
		_table.Rows._list.RemoveRange(_index, _count);
		_table.Rows.ResetIndex();
		// 标记为已删除（用于服务端同步）
		foreach (var row in _createdRows)
		{
			if (row.Status == SyncStatus.New)
			{
				_table.RowsToDelete.Add(row.Id);
			}
			else if (row.Status == SyncStatus.Synced)
			{
				_table.RemovedRows.Add(row.Id);
			}
			row.Status = SyncStatus.LocalDeleted;
			row.NeedSave = true;
		}
		foreach (var cell in _createdCells)
		{
			if (cell.HasFormula)
			{
				_table.Project.FormulaManager.RemoveHostObject(_table.Id, cell.Id);
			}
			if (cell.Status == SyncStatus.New)
			{
				_table.CellsToDelete.Add(cell.Id);
			}
			else if (cell.Status == SyncStatus.Synced)
			{
				_table.RemovedCells.Add(cell.Id);
			}
			cell.Status = SyncStatus.LocalDeleted;
			cell.NeedSave = true;
		}
		_table.Project.FormulaMapDirty = true;
		_table.Ticket.IsCacheExpired = true;
		FormulaEvaluator.ClearCache();
		_table.NeedSave = true;
	}
}
