using System;
using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class CellsOperand : Operand
{
	internal static CellListHashSetDictionary _leftOperatorCellListHashSetDictionary = new CellListHashSetDictionary();

	internal static CellListHashSetDictionary _rightOperatorCellListHashSetDictionary = new CellListHashSetDictionary();

	public virtual Table Table { get; }

	public List<Cell> Cells { get; }

	public HashSet<int> Rows => new HashSet<int>(Cells.Select((Cell cell) => cell.Row.Index));

	public bool IsCollectFill { get; set; }

	public override OperandType OperandType => OperandType.CellsOperand;

	public virtual Cell GetCellByRowIndex(int rowIndex)
	{
		throw new FormulaTypeMismatchException();
	}

	public CellsOperand MakeEmpty()
	{
		return new CellsOperand(Enumerable.Empty<Cell>().ToList(), Table);
	}

	public CellsOperand(List<Cell> cells, Table table)
	{
		Cells = cells;
		Table = table;
	}

	public override ValueOperand Add(Operand other)
	{
		throw new FormulaNotApplicableException("单元格集合的 + 运算无效。");
	}

	public override Operand And(Operand other)
	{
		if (other is CellsOperand cellsOperand)
		{
			if (cellsOperand.Cells.Count == 0 || Cells.Count == 0)
			{
				return new CellsOperand(new List<Cell>(), Table);
			}
			List<Cell> list = null;
			HashSet<int> hashSet = null;
			if (Cells.Count <= cellsOperand.Cells.Count)
			{
				list = Cells;
				if (!_rightOperatorCellListHashSetDictionary.Get(cellsOperand.Cells, out var result))
				{
					result = new HashSet<int>(cellsOperand.Cells.Select((Cell c) => c.Row.Index));
					_rightOperatorCellListHashSetDictionary.Add(cellsOperand.Cells, result);
				}
				hashSet = result;
			}
			else
			{
				list = cellsOperand.Cells;
				if (!_leftOperatorCellListHashSetDictionary.Get(Cells, out var result2))
				{
					result2 = new HashSet<int>(Cells.Select((Cell c) => c.Row.Index));
					_leftOperatorCellListHashSetDictionary.Add(Cells, result2);
				}
				hashSet = result2;
			}
			List<Cell> list2 = new List<Cell>(list.Count);
			foreach (Cell item in list)
			{
				if (hashSet.Contains(item.Row.Index))
				{
					list2.Add(item);
				}
			}
			return new CellsOperand(list2, Table);
		}
		throw new FormulaNotApplicableException("单元格集合的 AND 运算无效。");
	}

	public override ValueOperand Divide(Operand other)
	{
		throw new FormulaNotApplicableException("单元格集合的 / 运算无效。");
	}

	public override Operand Equal(Operand other)
	{
		return new CellsOperand(Cells.Where((Cell c) => ((BoolOperand)ValueOperand.FromObject(c.Value).Equal(other)).Value).ToList(), Table);
	}

	public override object Evaluate()
	{
		return ToValueSet().Evaluate();
	}

	public override Operand GreaterThan(Operand other)
	{
		return new CellsOperand(Cells.Where((Cell c) => ((BoolOperand)ValueOperand.FromObject(c.Value).GreaterThan(other)).Value).ToList(), Table);
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		return new CellsOperand(Cells.Where((Cell c) => ((BoolOperand)ValueOperand.FromObject(c.Value).GreaterThanOrEqual(other)).Value).ToList(), Table);
	}

	public override Operand LessThan(Operand other)
	{
		return new CellsOperand(Cells.Where((Cell c) => ((BoolOperand)ValueOperand.FromObject(c.Value).LessThan(other)).Value).ToList(), Table);
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		return new CellsOperand(Cells.Where((Cell c) => ((BoolOperand)ValueOperand.FromObject(c.Value).LessThanOrEqual(other)).Value).ToList(), Table);
	}

	public override ValueOperand Multiply(Operand other)
	{
		throw new FormulaNotApplicableException("单元格集合的 * 运算无效。");
	}

	public override ValueOperand Negate()
	{
		throw new FormulaNotApplicableException("单元格集合的求相反数运算无效。");
	}

	public override Operand Concatenate(Operand other)
	{
		if (other is CellsOperand cellsOperand)
		{
			return new CellsOperand(Cells.Concat(cellsOperand.Cells).ToList(), Table);
		}
		throw new FormulaTypeMismatchException();
	}

	public override Operand NotEqual(Operand other)
	{
		return new CellsOperand(Cells.Where((Cell c) => ((BoolOperand)ValueOperand.FromObject(c.Value).NotEqual(other)).Value).ToList(), Table);
	}

	public override Operand Or(Operand other)
	{
		if (other is CellsOperand cellsOperand)
		{
			if (cellsOperand.Cells.Count == 0)
			{
				return this;
			}
			if (Cells.Count == 0)
			{
				return cellsOperand;
			}
			List<Cell> list = null;
			List<Cell> list2 = null;
			HashSet<int> checkSet = null;
			if (Cells.Count > cellsOperand.Cells.Count)
			{
				list2 = cellsOperand.Cells;
				list = Cells;
				if (!_leftOperatorCellListHashSetDictionary.Get(Cells, out var result))
				{
					result = new HashSet<int>(Cells.Select((Cell c) => c.Row.Index));
					_leftOperatorCellListHashSetDictionary.Add(Cells, result);
				}
				checkSet = result;
			}
			else
			{
				list2 = Cells;
				list = cellsOperand.Cells;
				if (!_rightOperatorCellListHashSetDictionary.Get(cellsOperand.Cells, out var result2))
				{
					result2 = new HashSet<int>(cellsOperand.Cells.Select((Cell c) => c.Row.Index));
					_rightOperatorCellListHashSetDictionary.Add(cellsOperand.Cells, result2);
				}
				checkSet = result2;
			}
			List<Cell> cells = list.Concat(list2.Where((Cell c) => !checkSet.Contains(c.Row.Index))).ToList();
			return new CellsOperand(cells, Table);
		}
		throw new FormulaNotApplicableException("单元格集合的 OR 运算右侧必须为另一个单元格集合。");
	}

	public override ValueOperand Subtract(Operand other)
	{
		throw new FormulaNotApplicableException("单元格集合的 - 运算无效。");
	}

	public override BoolOperand ToBool()
	{
		throw new FormulaNotApplicableException("单元格集合无法转换为是否型。");
	}

	public override DateOperand ToDate()
	{
		throw new FormulaNotApplicableException("单元格集合无法转换为日期型。");
	}

	public override NumberOperand ToNumber()
	{
		throw new FormulaNotApplicableException("单元格集合无法转换为数值型。");
	}

	public override TimeOperand ToTime()
	{
		throw new FormulaNotApplicableException("单元格集合无法转换为时间型。");
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		throw new FormulaNotApplicableException("单元格集合无法转换为年月型。");
	}

	public override ValueSetOperand ToValueSet()
	{
		return new ValueSetOperand(from c in Cells
			where c.Row.Role == RowRole.Normal || c.Row.Role == RowRole.Among || c.Row.Role == RowRole.Minus
			select Tuple.Create(c.Row, ValueOperand.FromCellValue(c)));
	}

	public override ValueSetOperand ToValueSetOrderByRowIndex()
	{
		List<Cell> list = Cells.Where((Cell c) => c.Row.Role == RowRole.Normal || c.Row.Role == RowRole.Among || c.Row.Role == RowRole.Minus).ToList();
		list.Sort((Cell left, Cell right) => left.Row.Index.CompareTo(right.Row.Index));
		return new ValueSetOperand(list.Select((Cell c) => Tuple.Create(c.Row, ValueOperand.FromCellValue(c))));
	}

	public override string ToString()
	{
		return ToValueSet().ToString();
	}
}
