using System;

namespace Auditai.Model;

public abstract class Operand
{
	public abstract OperandType OperandType { get; }

	public static implicit operator Operand(int value)
	{
		return new NumberOperand(value);
	}

	public static implicit operator Operand(double value)
	{
		return new NumberOperand(value);
	}

	public static implicit operator Operand(DateTime value)
	{
		return new DateOperand(value);
	}

	public static implicit operator Operand(string value)
	{
		return new StringOperand(value);
	}

	public static implicit operator Operand(bool value)
	{
		return new BoolOperand(value);
	}

	public static implicit operator Operand(TimeSpan value)
	{
		return new TimeOperand(value);
	}

	public abstract object Evaluate();

	public abstract NumberOperand ToNumber();

	public abstract DateOperand ToDate();

	public abstract BoolOperand ToBool();

	public virtual StringOperand ToStringOp()
	{
		return new StringOperand(ToString());
	}

	public abstract TimeOperand ToTime();

	public abstract DateYearMonthOperand ToDateYearMonth();

	public abstract ValueOperand Add(Operand other);

	public abstract ValueOperand Subtract(Operand other);

	public abstract ValueOperand Multiply(Operand other);

	public abstract ValueOperand Divide(Operand other);

	public abstract Operand Concatenate(Operand other);

	public abstract ValueOperand Negate();

	public abstract Operand Equal(Operand other);

	public abstract Operand NotEqual(Operand other);

	public abstract Operand GreaterThan(Operand other);

	public abstract Operand GreaterThanOrEqual(Operand other);

	public abstract Operand LessThan(Operand other);

	public abstract Operand LessThanOrEqual(Operand other);

	public abstract Operand And(Operand other);

	public abstract Operand Or(Operand other);

	public abstract ValueSetOperand ToValueSet();

	public virtual ValueSetOperand ToValueSetOrderByRowIndex()
	{
		return ToValueSet();
	}
}
