using System;
using System.Linq;

namespace Auditai.Model;

public class CellOperand : Operand
{
	public Cell Cell { get; }

	public ValueOperand Value => ValueOperand.FromCellValue(Cell);

	public override OperandType OperandType => OperandType.CellOperand;

	public CellOperand(Cell cell)
	{
		Cell = cell;
	}

	public override object Evaluate()
	{
		return Value.Evaluate();
	}

	public override string ToString()
	{
		return Cell.GetDisplayValue();
	}

	public override NumberOperand ToNumber()
	{
		return Value.ToNumber();
	}

	public override DateOperand ToDate()
	{
		return Value.ToDate();
	}

	public override BoolOperand ToBool()
	{
		return Value.ToBool();
	}

	public override TimeOperand ToTime()
	{
		return Value.ToTime();
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		return Value.ToDateYearMonth();
	}

	public override ValueOperand Add(Operand other)
	{
		return Value.Add(other);
	}

	public override ValueOperand Subtract(Operand other)
	{
		return Value.Subtract(other);
	}

	public override ValueOperand Multiply(Operand other)
	{
		return Value.Multiply(other);
	}

	public override ValueOperand Divide(Operand other)
	{
		return Value.Divide(other);
	}

	public override Operand Concatenate(Operand other)
	{
		return ToString() + other.ToString();
	}

	public override ValueOperand Negate()
	{
		return Value.Negate();
	}

	public override Operand Equal(Operand other)
	{
		return Value.Equal(other);
	}

	public override Operand NotEqual(Operand other)
	{
		return Value.NotEqual(other);
	}

	public override Operand GreaterThan(Operand other)
	{
		return Value.GreaterThan(other);
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		return Value.GreaterThanOrEqual(other);
	}

	public override Operand LessThan(Operand other)
	{
		return Value.LessThan(other);
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		return Value.LessThanOrEqual(other);
	}

	public override Operand And(Operand other)
	{
		return Value.And(other);
	}

	public override Operand Or(Operand other)
	{
		return Value.Or(other);
	}

	public override int GetHashCode()
	{
		return Cell.Id.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CellOperand cellOperand))
		{
			return false;
		}
		return cellOperand.Cell == Cell;
	}

	public override ValueSetOperand ToValueSet()
	{
		if (Cell.Value is string text)
		{
			return new ValueSetOperand(from s1 in text.Split(StringOperand._splitter, StringSplitOptions.RemoveEmptyEntries)
				select Tuple.Create(Cell.Row, ValueOperand.FromObject(s1)));
		}
		return new ValueSetOperand(new Tuple<Row, ValueOperand>[1] { Tuple.Create(Cell.Row, ValueOperand.FromCellValue(Cell)) });
	}
}
