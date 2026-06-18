using System;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class NumberOperand : ValueOperand
{
	public const double EQUALITY_EPSILON = 0.0001;

	public double Value { get; }

	public override OperandType OperandType => OperandType.NumberOperand;

	public static implicit operator NumberOperand(double value)
	{
		return new NumberOperand(value);
	}

	public static explicit operator int(NumberOperand op)
	{
		return (int)op.Value;
	}

	public static explicit operator double(NumberOperand op)
	{
		return op.Value;
	}

	public NumberOperand(double value)
		: base(value)
	{
		Value = value;
	}

	public override object Evaluate()
	{
		return Value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public override NumberOperand ToNumber()
	{
		return this;
	}

	public override DateOperand ToDate()
	{
		try
		{
			return DateTime.MinValue + TimeSpan.FromDays(Value);
		}
		catch (OverflowException)
		{
			throw new FormulaBadValueException("数值溢出，无法转换为日期");
		}
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		try
		{
			return new DateYearMonth(DateTime.MinValue.AddMonths((int)Value));
		}
		catch (OverflowException)
		{
			throw new FormulaBadValueException("数值溢出，无法转换为年月");
		}
	}

	public override BoolOperand ToBool()
	{
		return Convert.ToBoolean(Value);
	}

	public override ValueOperand Add(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (other is ErrorOperand result)
			{
				return result;
			}
			return Add(other.ToNumber());
		}
		return Value + numberOperand.Value;
	}

	public override ValueOperand Subtract(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (other is ErrorOperand result)
			{
				return result;
			}
			return Subtract(other.ToNumber());
		}
		return Value - numberOperand.Value;
	}

	public override ValueOperand Multiply(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (other is ErrorOperand result)
			{
				return result;
			}
			return Multiply(other.ToNumber());
		}
		return Value * numberOperand.Value;
	}

	public override ValueOperand Divide(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (other is ErrorOperand result)
			{
				return result;
			}
			return Divide(other.ToNumber());
		}
		return Value / numberOperand.Value;
	}

	public override Operand Concatenate(Operand other)
	{
		return ToString() + other.ToString();
	}

	public override ValueOperand Negate()
	{
		return 0.0 - Value;
	}

	public override Operand Equal(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (other is StringOperand stringOperand)
					{
						return Value == 0.0 && stringOperand.Value == string.Empty;
					}
					return false;
				}
				return Equal(rangeOperand.ToNumber());
			}
			return Equal(cellOperand.Value);
		}
		return Equals(Value, numberOperand.Value);
	}

	public override Operand NotEqual(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (other is StringOperand stringOperand)
					{
						return Value != 0.0 || stringOperand.Value != string.Empty;
					}
					return true;
				}
				return NotEqual(rangeOperand.ToNumber());
			}
			return NotEqual(cellOperand.Value);
		}
		return !Equals(Value, numberOperand.Value);
	}

	public override Operand GreaterThan(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (!(other is DateOperand dateOperand))
					{
						if (other is StringOperand stringOperand)
						{
							if (string.IsNullOrEmpty(stringOperand.Value))
							{
								return Value > 0.0;
							}
							double result;
							return double.TryParse(stringOperand.Value, out result) && Value > result;
						}
						return false;
					}
					return GreaterThan(dateOperand.ToNumber());
				}
				return GreaterThan(rangeOperand.ToNumber());
			}
			return GreaterThan(cellOperand.Value);
		}
		return Value > numberOperand.Value;
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (!(other is DateOperand dateOperand))
					{
						if (other is StringOperand stringOperand)
						{
							if (string.IsNullOrEmpty(stringOperand.Value))
							{
								return Value >= 0.0;
							}
							double result;
							return double.TryParse(stringOperand.Value, out result) && Value >= result;
						}
						return false;
					}
					return GreaterThanOrEqual(dateOperand.ToNumber());
				}
				return GreaterThanOrEqual(rangeOperand.ToNumber());
			}
			return GreaterThanOrEqual(cellOperand.Value);
		}
		return Value >= numberOperand.Value;
	}

	public override Operand LessThan(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (!(other is DateOperand dateOperand))
					{
						if (other is StringOperand stringOperand)
						{
							if (string.IsNullOrEmpty(stringOperand.Value))
							{
								return Value < 0.0;
							}
							double result;
							return double.TryParse(stringOperand.Value, out result) && Value < result;
						}
						return false;
					}
					return LessThan(dateOperand.ToNumber());
				}
				return LessThan(rangeOperand.ToNumber());
			}
			return LessThan(cellOperand.Value);
		}
		return Value < numberOperand.Value;
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		if (!(other is NumberOperand numberOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (!(other is DateOperand dateOperand))
					{
						if (other is StringOperand stringOperand)
						{
							if (string.IsNullOrEmpty(stringOperand.Value))
							{
								return Value <= 0.0;
							}
							double result;
							return double.TryParse(stringOperand.Value, out result) && Value <= result;
						}
						return false;
					}
					return LessThanOrEqual(dateOperand.ToNumber());
				}
				return LessThanOrEqual(rangeOperand.ToNumber());
			}
			return LessThanOrEqual(cellOperand.Value);
		}
		return Value <= numberOperand.Value;
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
		if (!(obj is NumberOperand numberOperand))
		{
			return false;
		}
		return numberOperand.Value == Value;
	}

	public override ValueSetOperand ToValueSet()
	{
		return new ValueSetOperand(new Tuple<Row, ValueOperand>[1] { Tuple.Create<Row, ValueOperand>(null, Value) });
	}

	public override TimeOperand ToTime()
	{
		return new TimeOperand(new TimeSpan((long)Value));
	}

	public static bool Equals(double d1, double d2)
	{
		return Math.Abs(d1 - d2) < 0.0001;
	}
}
