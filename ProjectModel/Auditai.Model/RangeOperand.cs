using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class RangeOperand : CellsOperand
{
	public Cell TopLeft { get; set; }

	public Cell BottomRight { get; set; }

	internal ValueOperand SingleValue
	{
		get
		{
			if (TopLeft != BottomRight)
			{
				throw new FormulaNotApplicableException("区域包含多于一个单元格，运算无效。");
			}
			return ValueOperand.FromObject(TopLeft.Value);
		}
	}

	public IEnumerable<ValueOperand> CellValues => base.Cells.Select((Cell r) => ValueOperand.FromObject(r.Value));

	public override OperandType OperandType => OperandType.RangeOperand;

	public RangeOperand(Cell topLeft, Cell bottomRight)
		: base(GetCells(topLeft, bottomRight), topLeft._Table)
	{
		TopLeft = topLeft;
		BottomRight = bottomRight;
	}

	public override Cell GetCellByRowIndex(int rowIndex)
	{
		throw new FormulaTypeMismatchException();
	}

	private static List<Cell> GetCells(Cell c1, Cell c2)
	{
		Table table = c1._Table;
		List<Cell> list = new List<Cell>();
		for (int i = c1.Row.Index; i <= c2.Row.Index; i++)
		{
			for (int j = c1.Column.Index; j <= c2.Column.Index; j++)
			{
				list.Add(table[i, j]);
			}
		}
		return list;
	}

	public override object Evaluate()
	{
		return SingleValue.Evaluate();
	}

	public override NumberOperand ToNumber()
	{
		return SingleValue.ToNumber();
	}

	public override DateOperand ToDate()
	{
		return SingleValue.ToDate();
	}

	public override BoolOperand ToBool()
	{
		return SingleValue.ToBool();
	}

	public override TimeOperand ToTime()
	{
		return SingleValue.ToTime();
	}

	public override ValueOperand Add(Operand other)
	{
		return SingleValue.Add(other);
	}

	public override ValueOperand Subtract(Operand other)
	{
		return SingleValue.Subtract(other);
	}

	public override ValueOperand Multiply(Operand other)
	{
		return SingleValue.Multiply(other);
	}

	public override ValueOperand Divide(Operand other)
	{
		return SingleValue.Divide(other);
	}

	public override ValueOperand Negate()
	{
		return SingleValue.Negate();
	}

	public override int GetHashCode()
	{
		return TopLeft.Id.GetHashCode() + BottomRight.Id.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RangeOperand rangeOperand))
		{
			return false;
		}
		if (rangeOperand.TopLeft == TopLeft)
		{
			return rangeOperand.BottomRight == BottomRight;
		}
		return false;
	}
}
