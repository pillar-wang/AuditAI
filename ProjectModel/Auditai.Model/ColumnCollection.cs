using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class ColumnCollection : IEnumerable<Column>, IEnumerable
{
	internal readonly List<Column> _list = new List<Column>();

	private readonly Table _table;

	public Column this[int index] => _list[index];

	public int Count => _list.Count;

	public IEnumerable<Column> WhereVisible => _list.Where((Column c) => c.Visible);

	public int VisibleCount => WhereVisible.Count();

	public int DefaultWidth { get; set; } = 100;


	public Column GetVisibleColumnAt(int index)
	{
		return WhereVisible.ElementAt(index);
	}

	public void Append(int count)
	{
		Insert(_list.Count, count);
	}

	public void Insert(int index, int count)
	{
		int count2 = _list.Count;
		List<Column> list = new List<Column>(count);
		for (int i = 0; i < count; i++)
		{
			Column column = new Column
			{
				Id = Project.Current.GetNextId(),
				Table = _table,
				Index = index + i,
				Visible = true,
				Width = DefaultWidth,
				CaptionFormula = string.Empty,
				Formula = string.Empty
			};
			if (_list.Count == 0)
			{
				column.CaptionStyle.Deserialize(_table.DefaultStyle.Serialize());
			}
			else if (_list.Count == index)
			{
				column.CaptionStyle.Deserialize(_list[index - 1].CaptionStyle.Serialize());
			}
			else
			{
				column.CaptionStyle.Deserialize(_list[index].CaptionStyle.Serialize());
			}
			column.CaptionStyle.Align = CellTextAlign.MiddleCenter;
			list.Add(column);
		}
		_list.InsertRange(index, list);
		for (int j = 0; j < count; j++)
		{
			list[j].Caption = GetNewColumnCaption();
		}
		ResetIndex();
		List<Cell> list2 = new List<Cell>(_table.Rows.Count * count);
		for (int k = 0; k < _table.Rows.Count; k++)
		{
			for (int l = 0; l < count; l++)
			{
				Cell cell = _table.MakeNewCell();
				cell.Row = _table.Rows[k];
				cell.Column = list[l];
				list2.Add(cell);
			}
		}
		List<Cell> list3 = _table.Cells._list;
		list3.AddRange(Enumerable.Repeat<Cell>(null, list2.Count));
		for (int num = _table.Rows.Count - 1; num >= 0; num--)
		{
			for (int num2 = _list.Count - 1; num2 >= 0; num2--)
			{
				if (num2 >= index + count)
				{
					list3[num * _list.Count + num2] = list3[num * count2 + num2 - count];
				}
				else if (num2 >= index)
				{
					list3[num * _list.Count + num2] = list2[num * count + num2 - index];
				}
				else
				{
					list3[num * _list.Count + num2] = list3[num * count2 + num2];
				}
			}
		}
		_table.NeedSave = true;
	}

	public void Move(int index, int count, int newIndex)
	{
		if (newIndex >= index && newIndex <= index + count)
		{
			return;
		}
		List<Column> range = _list.GetRange(index, count);
		if (newIndex < index)
		{
			for (int num = index - 1; num >= newIndex; num--)
			{
				_list[num + count] = _list[num];
			}
			for (int i = 0; i < range.Count; i++)
			{
				_list[newIndex + i] = range[i];
			}
		}
		else
		{
			for (int j = index + count; j < newIndex; j++)
			{
				_list[j - count] = _list[j];
			}
			for (int k = 0; k < range.Count; k++)
			{
				_list[newIndex - count + k] = range[k];
			}
		}
		ResetIndex();
		_table.Cells.MoveColumns(index, count, newIndex);
		_table.NeedSave = true;
		_table.RemoveInvalidMerges();
	}

	public void Remove(int index, int count)
	{
		for (int i = index; i < index + count; i++)
		{
			for (int j = 0; j < _table.Rows.Count; j++)
			{
				Cell cell = _table[j, i];
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
				if (cell.HasFormula)
				{
					_table.Project.FormulaManager.RemoveHostObject(_table.Id, cell.Id);
				}
				CellMerge cellMerge = _table.MergedCells.FirstOrDefault((CellMerge m) => m.TopLeft == cell || m.BottomRight == cell);
				if (cellMerge != null)
				{
					_table.RemoveMerge(cellMerge);
				}
			}
			Column column = _list[i];
			if (column.Status == SyncStatus.New)
			{
				_table.ColumnsToDelete.Add(column.Id);
			}
			else if (column.Status == SyncStatus.Synced)
			{
				_table.RemovedColumns.Add(column.Id);
			}
			column.Status = SyncStatus.LocalDeleted;
			_table.Project.FormulaManager.RemoveHostObject(_table.Id, column.Id);
		}
		_table.Cells._list.RemoveAll((Cell c) => index <= c.Column.Index && c.Column.Index < index + count);
		_list.RemoveRange(index, count);
		ResetIndex();
		_table.Project.FormulaMapDirty = true;
		_table.NeedSave = true;
	}

	public Column GetByCaption(string caption)
	{
		return _list.FirstOrDefault((Column c) => c.GetUniqueFormulaName() == caption);
	}

	public Column GetById(Id64 id)
	{
		return _list.FirstOrDefault((Column c) => c.Id == id);
	}

	public IEnumerator<Column> GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	public void Resize(int width)
	{
		List<Column> list = _list.Where((Column c) => c.Visible).ToList();
		if (list.Count == 0)
		{
			return;
		}
		int num = list.Sum((Column c) => c.Width);
		double num2 = (double)width / (double)num;
		int num3 = width;
		foreach (Column item in list)
		{
			item.UpdateWidth((int)Math.Round((double)item.Width * num2));
			num3 -= item.Width;
		}
		int num4 = 0;
		while (num3 != 0)
		{
			if (num3 < 0)
			{
				list[num4 % list.Count].Width--;
				num3++;
			}
			else
			{
				list[num4 % list.Count].Width++;
				num3--;
			}
			num4++;
		}
	}

	internal ColumnCollection(Table table)
	{
		_table = table;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal void ResetIndex()
	{
		for (int i = 0; i < _list.Count; i++)
		{
			_list[i].Index = i;
		}
	}

	internal void Clear()
	{
		_list.Clear();
	}

	private string GetNewColumnCaption()
	{
		int i;
		for (i = 1; this.Any((Column c) => c.Caption == $"新列 {i}"); i++)
		{
		}
		return $"新列 {i}";
	}
}
