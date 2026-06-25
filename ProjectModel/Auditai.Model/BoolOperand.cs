using System;

namespace Auditai.Model;

public sealed class BoolOperand : ValueOperand
{
	public bool Value { get; }

	public override OperandType OperandType => OperandType.BoolOperand;

	public static implicit operator BoolOperand(bool value)
	{
		return new BoolOperand(value);
	}

	public static explicit operator bool(BoolOperand op)
	{
		return op.Value;
	}

	public BoolOperand(bool value)
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
		return Convert.ToDouble(Value);
	}

	public override DateOperand ToDate()
	{
		return ToNumber().ToDate();
	}

	public override BoolOperand ToBool()
	{
		return this;
	}

	public override TimeOperand ToTime()
	{
		throw new FormulaNotApplicableException("是否型无法转换为时间型");
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		throw new FormulaNotApplicableException("是否型无法转换为年月型");
	}

	public override ValueOperand Add(Operand other)
	{
		return ToNumber().Add(other);
	}

	public override ValueOperand Subtract(Operand other)
	{
		return ToNumber().Subtract(other);
	}

	public override ValueOperand Multiply(Operand other)
	{
		return ToNumber().Multiply(other);
	}

	public override ValueOperand Divide(Operand other)
	{
		return ToNumber().Divide(other);
	}

	public override Operand Concatenate(Operand other)
	{
		return ToString() + other.ToString();
	}

	public override ValueOperand Negate()
	{
		return ToNumber().Negate();
	}

	public override Operand Equal(Operand other)
	{
		if (!(other is BoolOperand boolOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (other is StringOperand stringOperand)
					{
						return (Value && stringOperand.Value == "√") || (!Value && stringOperand.Value == "");
					}
					return false;
				}
				return Equal(rangeOperand.ToBool());
			}
			return Equal(cellOperand.Value);
		}
		return Value == boolOperand.Value;
	}

	public override Operand NotEqual(Operand other)
	{
		if (!(other is BoolOperand boolOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (other is StringOperand stringOperand)
					{
						return (!Value || !(stringOperand.Value == "√")) && (Value || !(stringOperand.Value == ""));
					}
					return true;
				}
				return NotEqual(rangeOperand.ToBool());
			}
			return NotEqual(cellOperand.Value);
		}
		return Value != boolOperand.Value;
	}

	public override Operand GreaterThan(Operand other)
	{
		return ToNumber().GreaterThan(other);
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		return ToNumber().GreaterThanOrEqual(other);
	}

	public override Operand LessThan(Operand other)
	{
		return ToNumber().LessThan(other);
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		return ToNumber().LessThanOrEqual(other);
	}

	public override Operand And(Operand other)
	{
		return Value && other.ToBool().Value;
	}

	public override Operand Or(Operand other)
	{
		return Value || other.ToBool().Value;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BoolOperand boolOperand))
		{
			return false;
		}
		return boolOperand.Value == Value;
	}

	public override ValueSetOperand ToValueSet()
	{
		return new ValueSetOperand(new Tuple<Row, ValueOperand>[1] { Tuple.Create<Row, ValueOperand>(null, this) });
	}
}
