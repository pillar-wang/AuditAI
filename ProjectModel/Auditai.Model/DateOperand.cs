using System;
using Auditai.DTO;

namespace Auditai.Model;

public sealed class DateOperand : ValueOperand
{
	public DateTime Value { get; }

	public static DateOperand Zero { get; } = new NumberOperand(0.0).ToDate();


	public Cell Cell { get; set; }

	public override OperandType OperandType => OperandType.DateOperand;

	public DateOperand(DateTime value)
		: base(value)
	{
		Value = value;
	}

	public static implicit operator DateOperand(DateTime dt)
	{
		return new DateOperand(dt);
	}

	public static explicit operator DateTime(DateOperand op)
	{
		return op.Value;
	}

	public override object Evaluate()
	{
		return Value;
	}

	public override string ToString()
	{
		if (Cell != null)
		{
			return Cell.GetDisplayValue();
		}
		return Value.ToString("yyyy年MM月dd日");
	}

	public override NumberOperand ToNumber()
	{
		return (Value - DateTime.MinValue).TotalDays;
	}

	public override DateOperand ToDate()
	{
		return this;
	}

	public override BoolOperand ToBool()
	{
		return ToNumber().ToBool();
	}

	public override TimeOperand ToTime()
	{
		throw new FormulaNotApplicableException("日期型无法转换为时间型");
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		return new DateYearMonth(Value);
	}

	public override ValueOperand Add(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is DateYearMonthOperand dateYearMonthOperand))
			{
				if (other is ErrorOperand result)
				{
					return result;
				}
				return Add(other.ToNumber());
			}
			try
			{
				return new DateOperand(Value.AddMonths(dateYearMonthOperand.Value.Date.Year * 12 + dateYearMonthOperand.Value.Date.Month));
			}
			catch (ArgumentOutOfRangeException ex)
			{
				throw new FormulaBadValueException(ex.Message);
			}
			catch (OverflowException ex2)
			{
				throw new FormulaBadValueException(ex2.Message);
			}
		}
		try
		{
			return Value + TimeSpan.FromDays(numberOperand.Value);
		}
		catch (ArgumentOutOfRangeException ex3)
		{
			throw new FormulaBadValueException(ex3.Message);
		}
		catch (OverflowException ex4)
		{
			throw new FormulaBadValueException(ex4.Message);
		}
	}

	public override ValueOperand Subtract(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is DateYearMonthOperand dateYearMonthOperand))
			{
				if (other is ErrorOperand result)
				{
					return result;
				}
				return Subtract(other.ToNumber());
			}
			try
			{
				return new DateOperand(Value.AddMonths(-(dateYearMonthOperand.Value.Date.Year * 12 + dateYearMonthOperand.Value.Date.Month)));
			}
			catch (ArgumentOutOfRangeException ex)
			{
				throw new FormulaBadValueException(ex.Message);
			}
			catch (OverflowException ex2)
			{
				throw new FormulaBadValueException(ex2.Message);
			}
		}
		try
		{
			return Value - TimeSpan.FromDays(numberOperand.Value);
		}
		catch (ArgumentOutOfRangeException ex3)
		{
			throw new FormulaBadValueException(ex3.Message);
		}
		catch (OverflowException ex4)
		{
			throw new FormulaBadValueException(ex4.Message);
		}
	}

	public override ValueOperand Multiply(Operand other)
	{
		return ToNumber().Multiply(other);
	}

	public override ValueOperand Divide(Operand other)
	{
		return ToNumber().Divide(other);
	}

	public override ValueOperand Negate()
	{
		return ToNumber().Negate();
	}

	public override Operand Concatenate(Operand other)
	{
		return ToString() + other.ToString();
	}

	public override Operand Equal(Operand other)
	{
		if (!(other is DateOperand dateOperand))
		{
			if (!(other is DateYearMonthOperand { Value: var value }))
			{
				if (!(other is CellOperand cellOperand))
				{
					if (other is RangeOperand rangeOperand)
					{
						return Equal(rangeOperand.ToDate());
					}
					return false;
				}
				return Equal(cellOperand.Value);
			}
			return value.IsYearMonthEqual(Value);
		}
		return Value.Date == dateOperand.Value.Date;
	}

	public override Operand NotEqual(Operand other)
	{
		if (!(other is DateOperand dateOperand))
		{
			if (!(other is DateYearMonthOperand { Value: var value }))
			{
				if (!(other is CellOperand cellOperand))
				{
					if (other is RangeOperand rangeOperand)
					{
						return NotEqual(rangeOperand.ToDate());
					}
					return true;
				}
				return NotEqual(cellOperand.Value);
			}
			return !value.IsYearMonthEqual(Value);
		}
		return Value.Date != dateOperand.Value.Date;
	}

	public override Operand GreaterThan(Operand other)
	{
		if (!(other is DateOperand dateOperand))
		{
			if (!(other is DateYearMonthOperand { Value: var value }))
			{
				if (!(other is CellOperand cellOperand))
				{
					if (other is RangeOperand rangeOperand)
					{
						return GreaterThan(rangeOperand.ToDate());
					}
					return false;
				}
				return GreaterThan(cellOperand.Value);
			}
			return value.CompareTo(Value) < 0;
		}
		return Value.Date > dateOperand.Value.Date;
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		if (!(other is DateOperand dateOperand))
		{
			if (!(other is DateYearMonthOperand { Value: var value }))
			{
				if (!(other is CellOperand cellOperand))
				{
					if (other is RangeOperand rangeOperand)
					{
						return GreaterThanOrEqual(rangeOperand.ToDate());
					}
					return false;
				}
				return GreaterThanOrEqual(cellOperand.Value);
			}
			return value.CompareTo(Value) <= 0;
		}
		return Value.Date >= dateOperand.Value.Date;
	}

	public override Operand LessThan(Operand other)
	{
		if (!(other is DateOperand dateOperand))
		{
			if (!(other is DateYearMonthOperand { Value: var value }))
			{
				if (!(other is CellOperand cellOperand))
				{
					if (other is RangeOperand rangeOperand)
					{
						return LessThan(rangeOperand.ToDate());
					}
					return false;
				}
				return LessThan(cellOperand.Value);
			}
			return value.CompareTo(Value) > 0;
		}
		return Value.Date < dateOperand.Value.Date;
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		if (!(other is DateOperand dateOperand))
		{
			if (!(other is DateYearMonthOperand { Value: var value }))
			{
				if (!(other is CellOperand cellOperand))
				{
					if (other is RangeOperand rangeOperand)
					{
						return LessThanOrEqual(rangeOperand.ToDate());
					}
					return false;
				}
				return LessThanOrEqual(cellOperand.Value);
			}
			return value.CompareTo(Value) >= 0;
		}
		return Value.Date <= dateOperand.Value.Date;
	}

	public override Operand And(Operand other)
	{
		return ToBool().And(other);
	}

	public override Operand Or(Operand other)
	{
		return ToBool().Or(other);
	}

	public override int GetHashCode()
	{
		return Value.Date.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is DateOperand { Value: var value })
		{
			return value.Date == Value.Date;
		}
		return false;
	}

	public override ValueSetOperand ToValueSet()
	{
		return new ValueSetOperand(new Tuple<Row, ValueOperand>[1] { Tuple.Create<Row, ValueOperand>(null, this) });
	}
}
