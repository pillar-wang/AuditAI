using System.Collections.Generic;

namespace Auditai.Model;

internal class BoolOperandSearchIndexList : CellValueSearchIndexListBase
{
	protected List<Cell> _trueValueCellList;

	protected List<Cell> _falseValueCellList;

	public override void BuildIndexData(List<Cell> cellList)
	{
		_trueValueCellList = new List<Cell>(cellList.Count);
		_falseValueCellList = new List<Cell>(cellList.Count);
		foreach (Cell cell in cellList)
		{
			if (ValueOperand.FromObject(cell.Value).ToBool().Value)
			{
				_trueValueCellList.Add(cell);
			}
			else
			{
				_falseValueCellList.Add(cell);
			}
		}
		_trueValueCellList.TrimExcess();
		_falseValueCellList.TrimExcess();
		_sourceCellList = cellList;
	}

	public override List<Cell> FindEqualValue(Operand value)
	{
		if (value.ToBool().Value)
		{
			return _trueValueCellList;
		}
		return _falseValueCellList;
	}

	public override List<Cell> FindNotEqualValue(Operand value)
	{
		if (!value.ToBool().Value)
		{
			return _trueValueCellList;
		}
		return _falseValueCellList;
	}

	public override List<Cell> FindGreatThanValue(Operand value)
	{
		if (value.ToBool().Value)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		return _trueValueCellList;
	}

	public override List<Cell> FindGreatThanOrEqualValue(Operand value)
	{
		if (value.ToBool().Value)
		{
			return _trueValueCellList;
		}
		return _sourceCellList;
	}

	public override List<Cell> FindLessThanValue(Operand value)
	{
		if (value.ToBool().Value)
		{
			return _falseValueCellList;
		}
		return CellValueSearchIndex.EmptyCellList;
	}

	public override List<Cell> FindLessThanOrEqualValue(Operand value)
	{
		if (value.ToBool().Value)
		{
			return _sourceCellList;
		}
		return _falseValueCellList;
	}
}
