using System;

namespace Auditai.Model;

public class TimeOperand : ValueOperand
{
	public TimeSpan Value { get; }

	public override OperandType OperandType => OperandType.TimeOperand;

	public TimeOperand(TimeSpan value)
		: base(value)
	{
		Value = value;
	}

	public override ValueOperand Add(Operand other)
	{
		if (other is TimeOperand timeOperand)
		{
			return Value + timeOperand.Value;
		}
		return Add(other.ToTime());
	}

	public override Operand And(Operand other)
	{
		throw new FormulaNotApplicableException("时间型无法参与此运算");
	}

	public override Operand Concatenate(Operand other)
	{
		return ToStringOp().Concatenate(other);
	}

	public override ValueOperand Divide(Operand other)
	{
		throw new FormulaNotApplicableException("时间型无法参与此运算");
	}

	public override Operand Equal(Operand other)
	{
		if (other is TimeOperand timeOperand)
		{
			return Value == timeOperand.Value;
		}
		return Equal(other.ToTime());
	}

	public override object Evaluate()
	{
		return Value;
	}

	public override Operand GreaterThan(Operand other)
	{
		if (other is TimeOperand timeOperand)
		{
			return Value > timeOperand.Value;
		}
		return GreaterThan(other.ToTime());
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		if (other is TimeOperand timeOperand)
		{
			return Value >= timeOperand.Value;
		}
		return GreaterThanOrEqual(other.ToTime());
	}

	public override Operand LessThan(Operand other)
	{
		if (other is TimeOperand timeOperand)
		{
			return Value < timeOperand.Value;
		}
		return LessThan(other.ToTime());
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		if (other is TimeOperand timeOperand)
		{
			return Value <= timeOperand.Value;
		}
		return LessThanOrEqual(other.ToTime());
	}

	public override ValueOperand Multiply(Operand other)
	{
		throw new FormulaNotApplicableException("时间型无法参与此运算");
	}

	public override ValueOperand Negate()
	{
		return new TimeOperand(-Value);
	}

	public override Operand NotEqual(Operand other)
	{
		if (other is TimeOperand timeOperand)
		{
			return Value != timeOperand.Value;
		}
		return NotEqual(other.ToTime());
	}

	public override Operand Or(Operand other)
	{
		throw new FormulaNotApplicableException("时间型无法参与此运算");
	}

	public override ValueOperand Subtract(Operand other)
	{
		if (other is TimeOperand timeOperand)
		{
			try
			{
				return Value - timeOperand.Value;
			}
			catch (OverflowException)
			{
				throw new FormulaBadValueException("时间超出范围");
			}
		}
		return Subtract(other.ToTime());
	}

	public override BoolOperand ToBool()
	{
		return true;
	}

	public override DateOperand ToDate()
	{
		throw new FormulaNotApplicableException("时间型无法转换为日期型");
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		throw new FormulaNotApplicableException("时间型无法转换为年月型");
	}

	public override NumberOperand ToNumber()
	{
		return Value.Ticks;
	}

	public override ValueSetOperand ToValueSet()
	{
		return new ValueSetOperand(new Tuple<Row, ValueOperand>[1] { Tuple.Create<Row, ValueOperand>(null, this) });
	}

	public override TimeOperand ToTime()
	{
		return this;
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public static implicit operator TimeOperand(TimeSpan ts)
	{
		return new TimeOperand(ts);
	}
}
