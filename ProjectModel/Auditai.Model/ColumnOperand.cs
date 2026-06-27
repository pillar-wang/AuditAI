﻿﻿﻿﻿using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class ColumnOperand : CellsOperand
{
	protected List<Cell> _allRowsCellList;

	internal static Dictionary<Column, Dictionary<object, List<Cell>>> _columnCellsEqualCache = new Dictionary<Column, Dictionary<object, List<Cell>>>();

	internal static Dictionary<Column, CellValueSearchIndex> _columnCellsSearchIndexCache = new Dictionary<Column, CellValueSearchIndex>();

	internal static Dictionary<Column, OperandValueDictionary> _columnCellsStringEqualCache = new Dictionary<Column, OperandValueDictionary>();

	internal static Dictionary<Column, OperandValueDictionary> _columnCellsNotEqualCache = new Dictionary<Column, OperandValueDictionary>();

	internal static Dictionary<Column, OperandValueDictionary> _columnCellsGreatThanCache = new Dictionary<Column, OperandValueDictionary>();

	internal static Dictionary<Column, OperandValueDictionary> _columnCellsGreatThanOrEqualCache = new Dictionary<Column, OperandValueDictionary>();

	internal static Dictionary<Column, OperandValueDictionary> _columnCellsLessThanCache = new Dictionary<Column, OperandValueDictionary>();

	internal static Dictionary<Column, OperandValueDictionary> _columnCellsLessThanOrEqualCache = new Dictionary<Column, OperandValueDictionary>();

	public Column Column { get; }

	public IEnumerable<ValueOperand> CellValues => base.Cells.Select((Cell cell) => ValueOperand.FromObject(cell.Value));

	public override OperandType OperandType => OperandType.ColumnOperand;

	internal ColumnOperand(Column column)
		: base(FormulaEvaluator.GetCells(column), column.Table)
	{
		Column = column;
		System.Diagnostics.Debug.WriteLine($"[ColumnOperand] ctor: column='{column.Caption}', Id={column.Id}, TableId={column.Table?.Id}, base.Cells.Count={base.Cells?.Count ?? 0}");
		if (base.Cells == null || base.Cells.Count == 0)
		{
			System.Diagnostics.Debug.WriteLine($"[ColumnOperand] ctor: WARNING - no cells! Table.Rows.Count={column.Table?.Rows?.Count}, _loaded={column.Table?._loaded}");
		}
	}

	public override Cell GetCellByRowIndex(int rowIndex)
	{
		if (_allRowsCellList != null)
		{
			return _allRowsCellList[rowIndex];
		}
		if (rowIndex >= 0 && rowIndex < base.Cells.Count)
		{
			Cell cell = base.Cells[rowIndex];
			if (cell.Row.Index == rowIndex)
			{
				return cell;
			}
		}
		_allRowsCellList = Column.GetCells().ToList();
		return _allRowsCellList[rowIndex];
	}

	private Dictionary<object, List<Cell>> GetCellEqualDic()
	{
		if (!_columnCellsEqualCache.ContainsKey(Column))
		{
			_columnCellsEqualCache.Add(Column, (from c in base.Cells
				group c by (c.Value is string text) ? text.Trim() : c.Value).ToDictionary((IGrouping<object, Cell> g) => g.Key, (IGrouping<object, Cell> g) => g.ToList()));
		}
		return _columnCellsEqualCache[Column];
	}

	private CellValueSearchIndex GetColumnValueSearchIndex()
	{
		if (!_columnCellsSearchIndexCache.TryGetValue(Column, out var value))
		{
			value = new CellValueSearchIndex(base.Cells);
			_columnCellsSearchIndexCache.Add(Column, value);
		}
		return value;
	}

	private bool GetOperandValueEqualResult(Operand value, out List<Cell> cellList)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		switch (operandType)
		{
		case OperandType.BoolOperand:
		case OperandType.DateOperand:
		case OperandType.DateYearMonthOperand:
		case OperandType.NumberOperand:
		case OperandType.TimeOperand:
			cellList = GetColumnValueSearchIndex().FindEqualValue(value);
			return true;
		default:
			cellList = null;
			return false;
		}
	}

	private List<Cell> GetStringOperandValueEqualResult(StringOperand value)
	{
		List<Cell> result = null;
		if (!_columnCellsStringEqualCache.TryGetValue(Column, out var value2))
		{
			value2 = new OperandValueDictionary();
			_columnCellsStringEqualCache.Add(Column, value2);
			result = GetColumnValueSearchIndex().FindEqualValue(value);
			value2.Add(value, result);
		}
		else if (!value2.Get(value, out result))
		{
			result = GetColumnValueSearchIndex().FindEqualValue(value);
			value2.Add(value, result);
		}
		return result;
	}

	private bool GetOperandValueNotEqualResult(Operand value, out List<Cell> cellList)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		switch (operandType)
		{
		case OperandType.BoolOperand:
		case OperandType.DateOperand:
		case OperandType.DateYearMonthOperand:
		case OperandType.NumberOperand:
		case OperandType.StringOperand:
		case OperandType.TimeOperand:
		{
			if (!_columnCellsNotEqualCache.TryGetValue(Column, out var value2))
			{
				value2 = new OperandValueDictionary();
				_columnCellsNotEqualCache.Add(Column, value2);
				cellList = GetColumnValueSearchIndex().FindNotEqualValue(value);
				value2.Add(value, cellList);
			}
			else if (!value2.Get(value, out cellList))
			{
				cellList = GetColumnValueSearchIndex().FindNotEqualValue(value);
				value2.Add(value, cellList);
			}
			return true;
		}
		default:
			cellList = null;
			return false;
		}
	}

	private bool GetOperandValueGreatThanResult(Operand value, out List<Cell> cellList)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		switch (operandType)
		{
		case OperandType.BoolOperand:
		case OperandType.DateOperand:
		case OperandType.DateYearMonthOperand:
		case OperandType.NumberOperand:
		case OperandType.StringOperand:
		case OperandType.TimeOperand:
		{
			if (!_columnCellsGreatThanCache.TryGetValue(Column, out var value2))
			{
				value2 = new OperandValueDictionary();
				_columnCellsGreatThanCache.Add(Column, value2);
				cellList = GetColumnValueSearchIndex().FindGreatThanValue(value);
				value2.Add(value, cellList);
			}
			else if (!value2.Get(value, out cellList))
			{
				cellList = GetColumnValueSearchIndex().FindGreatThanValue(value);
				value2.Add(value, cellList);
			}
			return true;
		}
		default:
			cellList = null;
			return false;
		}
	}

	private bool GetOperandValueGreatThanOrEqualResult(Operand value, out List<Cell> cellList)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		switch (operandType)
		{
		case OperandType.BoolOperand:
		case OperandType.DateOperand:
		case OperandType.DateYearMonthOperand:
		case OperandType.NumberOperand:
		case OperandType.StringOperand:
		case OperandType.TimeOperand:
		{
			if (!_columnCellsGreatThanOrEqualCache.TryGetValue(Column, out var value2))
			{
				value2 = new OperandValueDictionary();
				_columnCellsGreatThanOrEqualCache.Add(Column, value2);
				cellList = GetColumnValueSearchIndex().FindGreatThanOrEqualValue(value);
				value2.Add(value, cellList);
			}
			else if (!value2.Get(value, out cellList))
			{
				cellList = GetColumnValueSearchIndex().FindGreatThanOrEqualValue(value);
				value2.Add(value, cellList);
			}
			return true;
		}
		default:
			cellList = null;
			return false;
		}
	}

	private bool GetOperandValueLessThanResult(Operand value, out List<Cell> cellList)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		switch (operandType)
		{
		case OperandType.BoolOperand:
		case OperandType.DateOperand:
		case OperandType.DateYearMonthOperand:
		case OperandType.NumberOperand:
		case OperandType.StringOperand:
		case OperandType.TimeOperand:
		{
			if (!_columnCellsLessThanCache.TryGetValue(Column, out var value2))
			{
				value2 = new OperandValueDictionary();
				_columnCellsLessThanCache.Add(Column, value2);
				cellList = GetColumnValueSearchIndex().FindLessThanValue(value);
				value2.Add(value, cellList);
			}
			else if (!value2.Get(value, out cellList))
			{
				cellList = GetColumnValueSearchIndex().FindLessThanValue(value);
				value2.Add(value, cellList);
			}
			return true;
		}
		default:
			cellList = null;
			return false;
		}
	}

	private bool GetOperandValueLessThanOrEqualResult(Operand value, out List<Cell> cellList)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		switch (operandType)
		{
		case OperandType.BoolOperand:
		case OperandType.DateOperand:
		case OperandType.DateYearMonthOperand:
		case OperandType.NumberOperand:
		case OperandType.StringOperand:
		case OperandType.TimeOperand:
		{
			if (!_columnCellsLessThanOrEqualCache.TryGetValue(Column, out var value2))
			{
				value2 = new OperandValueDictionary();
				_columnCellsLessThanOrEqualCache.Add(Column, value2);
				cellList = GetColumnValueSearchIndex().FindLessThanOrEqualValue(value);
				value2.Add(value, cellList);
			}
			else if (!value2.Get(value, out cellList))
			{
				cellList = GetColumnValueSearchIndex().FindLessThanOrEqualValue(value);
				value2.Add(value, cellList);
			}
			return true;
		}
		default:
			cellList = null;
			return false;
		}
	}

	public override Operand Equal(Operand other)
	{
		if (GetOperandValueEqualResult(other, out var cellList))
		{
			return new CellsOperand(cellList, Table);
		}
		StringOperand stringOperand = null;
		if (other is CellOperand { Value: StringOperand value })
		{
			if (!ContainsWildcard(value))
			{
				Dictionary<object, List<Cell>> cellEqualDic = GetCellEqualDic();
				if (!cellEqualDic.TryGetValue(value.Value.Trim(), out var value2))
				{
					return MakeEmpty();
				}
				return new CellsOperand(value2, Table);
			}
			stringOperand = value;
		}
		else if (other is StringOperand stringOperand2)
		{
			if (!ContainsWildcard(stringOperand2))
			{
				Dictionary<object, List<Cell>> cellEqualDic2 = GetCellEqualDic();
				if (!cellEqualDic2.TryGetValue(stringOperand2.Value.Trim(), out var value3))
				{
					return MakeEmpty();
				}
				return new CellsOperand(value3, Table);
			}
			stringOperand = stringOperand2;
		}
		if (stringOperand != null)
		{
			return new CellsOperand(GetStringOperandValueEqualResult(stringOperand), Table);
		}
		return base.Equal(other);
		static bool ContainsWildcard(StringOperand so)
		{
			string value4 = so.Value;
			if (value4 == null)
			{
				return false;
			}
			for (int num = value4.Length - 1; num >= 0; num--)
			{
				char c = value4[num];
				if (c == '*' || c == '?')
				{
					return true;
				}
			}
			return false;
		}
	}

	public override Operand NotEqual(Operand other)
	{
		if (GetOperandValueNotEqualResult(other, out var cellList))
		{
			return new CellsOperand(cellList, Table);
		}
		return base.NotEqual(other);
	}

	public override Operand GreaterThan(Operand other)
	{
		if (GetOperandValueGreatThanResult(other, out var cellList))
		{
			return new CellsOperand(cellList, Table);
		}
		return base.GreaterThan(other);
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		if (GetOperandValueGreatThanOrEqualResult(other, out var cellList))
		{
			return new CellsOperand(cellList, Table);
		}
		return base.GreaterThanOrEqual(other);
	}

	public override Operand LessThan(Operand other)
	{
		if (GetOperandValueLessThanResult(other, out var cellList))
		{
			return new CellsOperand(cellList, Table);
		}
		return base.LessThan(other);
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		if (GetOperandValueLessThanOrEqualResult(other, out var cellList))
		{
			return new CellsOperand(cellList, Table);
		}
		return base.LessThanOrEqual(other);
	}

	public override int GetHashCode()
	{
		return Column.Id.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ColumnOperand columnOperand))
		{
			return false;
		}
		return columnOperand.Column == Column;
	}
}
