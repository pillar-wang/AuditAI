using System;
using System.Collections.Generic;
using System.Data;

namespace Leqisoft.Model;

public class TableListOperand : Operand
{
	public DataTable DataTable { get; } = new DataTable();


	public List<CellTextAlign> Aligns { get; } = new List<CellTextAlign>();


	public override OperandType OperandType => OperandType.TableListOperand;

	public override ValueOperand Add(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override Operand And(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override Operand Concatenate(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override ValueOperand Divide(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override Operand Equal(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override object Evaluate()
	{
		return this;
	}

	public override Operand GreaterThan(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override Operand LessThan(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override ValueOperand Multiply(Operand other)
	{
		throw new NotImplementedException();
	}

	public override ValueOperand Negate()
	{
		throw new FormulaTypeMismatchException();
	}

	public override Operand NotEqual(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override Operand Or(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override ValueOperand Subtract(Operand other)
	{
		throw new FormulaTypeMismatchException();
	}

	public override BoolOperand ToBool()
	{
		throw new FormulaTypeMismatchException();
	}

	public override DateOperand ToDate()
	{
		throw new FormulaTypeMismatchException();
	}

	public override NumberOperand ToNumber()
	{
		throw new FormulaTypeMismatchException();
	}

	public override StringOperand ToStringOp()
	{
		throw new FormulaTypeMismatchException();
	}

	public override ValueSetOperand ToValueSet()
	{
		throw new FormulaTypeMismatchException();
	}

	public override TimeOperand ToTime()
	{
		throw new FormulaTypeMismatchException();
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		throw new FormulaTypeMismatchException();
	}
}
