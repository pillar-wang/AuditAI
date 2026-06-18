namespace Leqisoft.Model;

public class CellUpdateValueCommand : CommandBase
{
	private object _OldValue;

	private object _newValue;

	private Cell _cell;

	public bool IsExistManualInputValue { get; set; }

	private bool _oldIsExistManualInputValue { get; set; }

	public CellUpdateValueCommand(Cell cell, object newValue)
	{
		_OldValue = cell.Value;
		_cell = cell;
		_newValue = newValue;
		_oldIsExistManualInputValue = cell.IsExistManualInputValue;
	}

	public override void Execute()
	{
		if (_cell.IsExisting)
		{
			_cell.UpdateValue(_newValue);
			_cell.IsExistManualInputValue = IsExistManualInputValue;
		}
	}

	public override void Undo()
	{
		if (_cell.IsExisting)
		{
			_cell.UpdateValue(_OldValue);
			_cell.IsExistManualInputValue = _oldIsExistManualInputValue;
		}
	}
}
