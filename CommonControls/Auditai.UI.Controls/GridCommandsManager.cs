using System;
using System.Collections.Generic;
using System.Linq;
using C1.Win.C1FlexGrid;

namespace Auditai.UI.Controls;

public class GridCommandsManager
{
	private C1FlexGrid _grid;

	private Stack<GridCommandBase> _undo = new Stack<GridCommandBase>();

	private Stack<GridCommandBase> _redo = new Stack<GridCommandBase>();

	private List<GridCellUpdateEventArgs> _batchUpdateCache;

	private bool _batching;

	private bool _pendding;

	private int _currentRow;

	private int _currentCol;

	private bool _sameCycle;

	private object _oldValueCache;

	public bool CanUndo => _undo.Count > 0;

	public bool CanRedo => _redo.Count > 0;

	public event EventHandler<GridCellUpdateEventArgs> AfterCellUpdate;

	public event EventHandler<GridBatchCellUpdateEventArgs> AfterBatchCellUpdate;

	public GridCommandsManager(C1FlexGrid grid)
	{
		_grid = grid;
		Initialize();
		AfterCellUpdate += TableCommandsManager_AfterCellUpdate;
		AfterBatchCellUpdate += TableCommandsManager_AfterBatchCellUpdate;
	}

	private void TableCommandsManager_AfterCellUpdate(object sender, GridCellUpdateEventArgs e)
	{
		if (!_pendding)
		{
			if (_batching)
			{
				_batchUpdateCache.Add(e);
			}
			else
			{
				NewCommand(new GridCellUpdateValueCommand(new GridCellInfo(_grid, e.Row, e.Col, e.OldValue, e.NewValue)));
			}
		}
	}

	private void TableCommandsManager_AfterBatchCellUpdate(object sender, GridBatchCellUpdateEventArgs e)
	{
		if (!_pendding)
		{
			NewCommand(new GridBatchCellUpdateValueCommand(_batchUpdateCache.Select((GridCellUpdateEventArgs c) => new GridCellInfo(_grid, c.Row, c.Col, c.OldValue, c.NewValue))));
		}
	}

	public void Undo()
	{
		if (CanUndo)
		{
			GridCommandBase gridCommandBase = _undo.Pop();
			_redo.Push(gridCommandBase);
			gridCommandBase.Undo();
		}
	}

	public void Redo()
	{
		if (CanRedo)
		{
			GridCommandBase gridCommandBase = _redo.Pop();
			_undo.Push(gridCommandBase);
			gridCommandBase.Execute();
		}
	}

	public void StartBatchUpdate()
	{
		_batchUpdateCache = new List<GridCellUpdateEventArgs>();
		_batching = true;
	}

	public void EndBatchUpdate()
	{
		_batching = false;
		this.AfterBatchCellUpdate?.Invoke(this, new GridBatchCellUpdateEventArgs(_batchUpdateCache));
	}

	public void StartPendding()
	{
		_pendding = true;
	}

	public void EndPendding()
	{
		_pendding = false;
	}

	public void NewCommand(GridCommandBase command)
	{
		_redo.Clear();
		_undo.Push(command);
	}

	private void Initialize()
	{
		_grid.CellChanged += _grid_CellChanged;
		_grid.ValidateEdit += _grid_ValidateEdit;
	}

	private void _grid_ValidateEdit(object sender, ValidateEditEventArgs e)
	{
		_currentRow = e.Row;
		_currentCol = e.Col;
		_sameCycle = true;
		_oldValueCache = _grid[e.Row, e.Col];
	}

	private void _grid_CellChanged(object sender, RowColEventArgs e)
	{
		if (_sameCycle && e.Row == _currentRow && e.Col == _currentCol)
		{
			this.AfterCellUpdate?.Invoke(sender, new GridCellUpdateEventArgs(e.Row, e.Col, _oldValueCache, _grid[e.Row, e.Col]));
		}
		_sameCycle = false;
	}
}
