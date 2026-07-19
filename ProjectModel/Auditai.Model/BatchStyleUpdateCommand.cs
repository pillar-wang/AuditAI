using System;
using System.Collections.Generic;

namespace Auditai.Model;

/// <summary>
/// 批量样式更新命令，支持单元格、列和表默认样式的撤销/恢复。
/// </summary>
public class BatchStyleUpdateCommand : CommandBase
{
	private Table _table;

	// 单元格样式变更
	private List<Tuple<Cell, CellStyle, CellStyle>> _cellUpdates;

	// 列样式变更
	private List<Tuple<Column, CellStyle, CellStyle>> _columnUpdates;

	// 表默认样式变更
	private CellStyle _oldDefaultStyle;
	private CellStyle _newDefaultStyle;

	public BatchStyleUpdateCommand(Table table)
	{
		_table = table;
		_cellUpdates = new List<Tuple<Cell, CellStyle, CellStyle>>();
		_columnUpdates = new List<Tuple<Column, CellStyle, CellStyle>>();
	}

	public void AddCellUpdate(Cell cell, CellStyle oldStyle, CellStyle newStyle)
	{
		_cellUpdates.Add(Tuple.Create(cell, oldStyle, newStyle));
	}

	public void AddColumnUpdate(Column column, CellStyle oldStyle, CellStyle newStyle)
	{
		_columnUpdates.Add(Tuple.Create(column, oldStyle, newStyle));
	}

	public void SetDefaultStyleUpdate(CellStyle oldStyle, CellStyle newStyle)
	{
		_oldDefaultStyle = oldStyle;
		_newDefaultStyle = newStyle;
	}

	public override void Execute()
	{
		// 应用新样式
		foreach (var item in _cellUpdates)
		{
			if (item.Item1.IsExisting)
			{
				item.Item1.UpdateStyle(item.Item3);
			}
		}
		foreach (var item in _columnUpdates)
		{
			item.Item1.UpdateStyle(item.Item3);
		}
		if (_newDefaultStyle != null)
		{
			_table.UpdateDefaultStyle(_newDefaultStyle);
		}
		_table.NeedSave = true;
	}

	public override void Undo()
	{
		// 恢复旧样式
		foreach (var item in _cellUpdates)
		{
			if (item.Item1.IsExisting)
			{
				item.Item1.UpdateStyle(item.Item2);
			}
		}
		foreach (var item in _columnUpdates)
		{
			item.Item1.UpdateStyle(item.Item2);
		}
		if (_oldDefaultStyle != null)
		{
			_table.UpdateDefaultStyle(_oldDefaultStyle);
		}
		_table.NeedSave = true;
	}
}
