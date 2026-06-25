using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class ValueSetOperand : Operand
{
	public HashSet<Tuple<Row, ValueOperand>> Set { get; }

	public static ValueSetOperand Error { get; } = new ValueSetOperand(Enumerable.Empty<Tuple<Row, ValueOperand>>());


	public static ValueSetOperand Empty { get; } = new ValueSetOperand(Enumerable.Empty<Tuple<Row, ValueOperand>>());


	public override OperandType OperandType => OperandType.ValueSetOperand;

	public ValueSetOperand(IEnumerable<Tuple<Row, ValueOperand>> values)
	{
		Set = new HashSet<Tuple<Row, ValueOperand>>(values, Tuple2Item2Comparer.Instance);
	}

	public override ValueOperand Add(Operand other)
	{
		throw new FormulaNotApplicableException("数组的 + 运算无效。");
	}

	public override Operand And(Operand other)
	{
		throw new FormulaNotApplicableException("数组的 AND 运算无效。");
	}

	public override ValueOperand Divide(Operand other)
	{
		throw new FormulaNotApplicableException("数组的 / 运算无效。");
	}

	public override Operand Equal(Operand other)
	{
		return Set.SetEquals(other.ToValueSet().Set);
	}

	public override object Evaluate()
	{
		return ToString();
	}

	public override Operand GreaterThan(Operand other)
	{
		return Set.IsSupersetOf(other.ToValueSet().Set);
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		throw new FormulaNotApplicableException("数组的 >= 运算无效。");
	}

	public override Operand LessThan(Operand other)
	{
		return Set.IsSubsetOf(other.ToValueSet().Set);
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		throw new FormulaNotApplicableException("数组的 <= 运算无效。");
	}

	public override ValueOperand Multiply(Operand other)
	{
		throw new FormulaNotApplicableException("数组的 * 运算无效。");
	}

	public override ValueOperand Negate()
	{
		throw new FormulaNotApplicableException("数组无法求相反数。");
	}

	public override Operand NotEqual(Operand other)
	{
		return !Set.SetEquals(other.ToValueSet().Set);
	}

	public override Operand Or(Operand other)
	{
		throw new FormulaNotApplicableException("数组的 OR 运算无效。");
	}

	public override ValueOperand Subtract(Operand other)
	{
		throw new FormulaNotApplicableException("数组的 - 运算无效。");
	}

	public override Operand Concatenate(Operand other)
	{
		return ToString() + other.ToString();
	}

	public override BoolOperand ToBool()
	{
		throw new FormulaNotApplicableException("数组无法转换为是否型。");
	}

	public override DateOperand ToDate()
	{
		throw new FormulaNotApplicableException("数组无法转换为日期型。");
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		throw new FormulaNotApplicableException("数组无法转换为年月型");
	}

	public override NumberOperand ToNumber()
	{
		throw new FormulaNotApplicableException("数组无法转换为数值型。");
	}

	public override TimeOperand ToTime()
	{
		throw new FormulaNotApplicableException("数组无法转换为时间型。");
	}

	public override ValueSetOperand ToValueSet()
	{
		return this;
	}

	public static ValueSetOperand UnionAll(IEnumerable<ValueSetOperand> sets)
	{
		if (!sets.Any())
		{
			return Empty;
		}
		return sets.Aggregate(delegate(ValueSetOperand s1, ValueSetOperand s2)
		{
			s1.Set.UnionWith(s2.Set);
			return s1;
		});
	}

	public static ValueSetOperand IntersectAll(IEnumerable<ValueSetOperand> sets)
	{
		if (!sets.Any())
		{
			return Empty;
		}
		return sets.Aggregate(delegate(ValueSetOperand s1, ValueSetOperand s2)
		{
			s1.Set.IntersectWith(s2.Set);
			return s1;
		});
	}

	public static ValueSetOperand ExceptAll(IEnumerable<ValueSetOperand> sets)
	{
		if (!sets.Any())
		{
			return Empty;
		}
		return sets.Aggregate(delegate(ValueSetOperand s1, ValueSetOperand s2)
		{
			s1.Set.ExceptWith(s2.Set);
			return s1;
		});
	}

	public override string ToString()
	{
		return string.Join("|", Set.Select((Tuple<Row, ValueOperand> t) => t.Item2));
	}
}
