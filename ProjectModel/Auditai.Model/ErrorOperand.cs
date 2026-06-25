using System;

namespace Auditai.Model;

public class ErrorOperand : ValueOperand
{
	private readonly int index;

	private readonly string[] errorMessages = new string[4] { "#VALUE!", "#REF!", "#N/A", "" };

	private readonly Exception[] exceptions = new Exception[3]
	{
		new FormulaBadValueException(),
		new FormulaBadReferenceException(),
		new FormulaNotApplicableException("")
	};

	public static ErrorOperand BadValue { get; } = new ErrorOperand(0);


	public static ErrorOperand BadReference { get; } = new ErrorOperand(1);


	public static ErrorOperand NotApplicable { get; } = new ErrorOperand(2);


	public static ErrorOperand Empty { get; } = new ErrorOperand(3);


	public override OperandType OperandType => OperandType.ErrorOperand;

	private ErrorOperand(int index)
		: base(index)
	{
		this.index = index;
	}

	public override object Evaluate()
	{
		return ToString();
	}

	public override DateOperand ToDate()
	{
		throw new FormulaBadValueException("错误值无法转换为日期");
	}

	public override NumberOperand ToNumber()
	{
		throw new FormulaBadValueException("错误值无法转换为数值");
	}

	public override TimeOperand ToTime()
	{
		throw new FormulaBadValueException("错误值无法转换为时间");
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		throw new FormulaNotApplicableException("错误值无法转换为年月");
	}

	public override string ToString()
	{
		return errorMessages[index];
	}

	public override ValueOperand Add(Operand other)
	{
		return this;
	}

	public override ValueOperand Subtract(Operand other)
	{
		return this;
	}

	public override ValueOperand Multiply(Operand other)
	{
		return this;
	}

	public override ValueOperand Divide(Operand other)
	{
		return this;
	}

	public override ValueOperand Negate()
	{
		return this;
	}

	public override Operand Concatenate(Operand other)
	{
		return this;
	}

	public override Operand Equal(Operand other)
	{
		return this;
	}

	public override Operand NotEqual(Operand other)
	{
		return this;
	}

	public override Operand GreaterThan(Operand other)
	{
		return this;
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		return this;
	}

	public override Operand LessThan(Operand other)
	{
		return this;
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		return this;
	}

	public override BoolOperand ToBool()
	{
		throw new FormulaBadValueException("错误值无法转换为是否型");
	}

	public override Operand And(Operand other)
	{
		return this;
	}

	public override Operand Or(Operand other)
	{
		return this;
	}

	public override int GetHashCode()
	{
		return index.GetHashCode();
	}

	public override ValueSetOperand ToValueSet()
	{
		return ValueSetOperand.Error;
	}

	public void ThrowException()
	{
		throw exceptions[index];
	}
}
