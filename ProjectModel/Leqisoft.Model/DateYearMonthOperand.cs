using System;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public sealed class DateYearMonthOperand : ValueOperand
{
	public DateYearMonth Value { get; }

	public static DateYearMonthOperand Zero { get; } = new DateYearMonth(DateTime.MinValue);


	public Cell Cell { get; set; }

	public override OperandType OperandType => OperandType.DateYearMonthOperand;

	public DateYearMonthOperand(DateYearMonth value)
		: base(value)
	{
		Value = value;
	}

	public static implicit operator DateYearMonthOperand(DateYearMonth dt)
	{
		return new DateYearMonthOperand(dt);
	}

	public static explicit operator DateYearMonth(DateYearMonthOperand op)
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
		return Value.Date.ToString("yyyy年MM月");
	}

	public override NumberOperand ToNumber()
	{
		return (Value.Date.Year - DateTime.MinValue.Year) * 12 + (Value.Date.Month - DateTime.MinValue.Month);
	}

	public override DateOperand ToDate()
	{
		return new DateOperand(Value.Date);
	}

	public override BoolOperand ToBool()
	{
		return ToNumber().ToBool();
	}

	public override TimeOperand ToTime()
	{
		throw new FormulaNotApplicableException("年月型无法转换为时间型");
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		return this;
	}

	public override ValueOperand Add(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is DateYearMonthOperand dateYearMonthOperand))
			{
				if (!(other is DateOperand dateOperand))
				{
					if (other is ErrorOperand result)
					{
						return result;
					}
					return Add(other.ToNumber());
				}
				try
				{
					return new DateYearMonthOperand(Value.AddMonths(dateOperand.Value.Date.Year * 12 + dateOperand.Value.Date.Month));
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
				return new DateYearMonthOperand(Value.AddMonths(dateYearMonthOperand.Value.Date.Year * 12 + dateYearMonthOperand.Value.Date.Month));
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
		try
		{
			return new DateYearMonthOperand(Value.AddMonths((int)numberOperand.Value));
		}
		catch (ArgumentOutOfRangeException ex5)
		{
			throw new FormulaBadValueException(ex5.Message);
		}
		catch (OverflowException ex6)
		{
			throw new FormulaBadValueException(ex6.Message);
		}
	}

	public override ValueOperand Subtract(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is DateYearMonthOperand dateYearMonthOperand))
			{
				if (!(other is DateOperand dateOperand))
				{
					if (other is ErrorOperand result)
					{
						return result;
					}
					return Subtract(other.ToNumber());
				}
				try
				{
					return new DateYearMonthOperand(Value.AddMonths(-(dateOperand.Value.Date.Year * 12 + dateOperand.Value.Date.Month)));
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
				return new DateYearMonthOperand(Value.AddMonths(-(dateYearMonthOperand.Value.Date.Year * 12 + dateYearMonthOperand.Value.Date.Month)));
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
		try
		{
			return new DateYearMonthOperand(Value.AddMonths(-(int)numberOperand.Value));
		}
		catch (ArgumentOutOfRangeException ex5)
		{
			throw new FormulaBadValueException(ex5.Message);
		}
		catch (OverflowException ex6)
		{
			throw new FormulaBadValueException(ex6.Message);
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
		if (!(other is DateYearMonthOperand dateYearMonthOperand))
		{
			if (!(other is DateOperand dateOperand))
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
			return Value.IsYearMonthEqual(dateOperand.Value);
		}
		return Value.IsYearMonthEqual(dateYearMonthOperand.Value);
	}

	public override Operand NotEqual(Operand other)
	{
		if (!(other is DateYearMonthOperand dateYearMonthOperand))
		{
			if (!(other is DateOperand dateOperand))
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
			return !Value.IsYearMonthEqual(dateOperand.Value);
		}
		return !Value.IsYearMonthEqual(dateYearMonthOperand.Value);
	}

	public override Operand GreaterThan(Operand other)
	{
		if (!(other is DateYearMonthOperand dateYearMonthOperand))
		{
			if (!(other is DateOperand dateOperand))
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
			return Value.CompareTo(dateOperand.Value) > 0;
		}
		return Value.CompareTo(dateYearMonthOperand.Value) > 0;
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		if (!(other is DateYearMonthOperand dateYearMonthOperand))
		{
			if (!(other is DateOperand dateOperand))
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
			return Value.CompareTo(dateOperand.Value) >= 0;
		}
		return Value.CompareTo(dateYearMonthOperand.Value) >= 0;
	}

	public override Operand LessThan(Operand other)
	{
		if (!(other is DateYearMonthOperand dateYearMonthOperand))
		{
			if (!(other is DateOperand dateOperand))
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
			return Value.CompareTo(dateOperand.Value) < 0;
		}
		return Value.CompareTo(dateYearMonthOperand.Value) < 0;
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		if (!(other is DateYearMonthOperand dateYearMonthOperand))
		{
			if (!(other is DateOperand dateOperand))
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
			return Value.CompareTo(dateOperand.Value) <= 0;
		}
		return Value.CompareTo(dateYearMonthOperand.Value) <= 0;
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
		return Value.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is DateYearMonthOperand { Value: var value } dateYearMonthOperand && value.Date.Year == Value.Date.Year)
		{
			return dateYearMonthOperand.Value.Date.Month == Value.Date.Month;
		}
		return false;
	}

	public override ValueSetOperand ToValueSet()
	{
		return new ValueSetOperand(new Tuple<Row, ValueOperand>[1] { Tuple.Create<Row, ValueOperand>(null, this) });
	}
}
