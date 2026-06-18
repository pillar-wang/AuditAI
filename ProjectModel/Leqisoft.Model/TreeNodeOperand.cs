namespace Leqisoft.Model;

public class TreeNodeOperand : Operand
{
	public TreeNodeBase TreeNode { get; }

	public override OperandType OperandType => OperandType.TreeNodeOperand;

	internal TreeNodeOperand(TreeNodeBase treeNode)
	{
		TreeNode = treeNode;
	}

	public override ValueOperand Add(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 + 运算");
	}

	public override Operand And(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 And 运算");
	}

	public override ValueOperand Divide(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 / 运算");
	}

	public override Operand Equal(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 = 运算");
	}

	public override object Evaluate()
	{
		if (TreeNode is TreeNodeNull)
		{
			return string.Empty;
		}
		return TreeNode.FormulaUniqueName;
	}

	public override Operand GreaterThan(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 > 运算");
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 >= 运算");
	}

	public override Operand LessThan(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 < 运算");
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 <= 运算");
	}

	public override ValueOperand Multiply(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 * 运算");
	}

	public override ValueOperand Negate()
	{
		throw new FormulaBadValueException("节点类型无法作求反运算");
	}

	public override Operand Concatenate(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 & 运算");
	}

	public override Operand NotEqual(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 <> 运算");
	}

	public override Operand Or(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 Or 运算");
	}

	public override ValueOperand Subtract(Operand other)
	{
		throw new FormulaBadValueException("节点类型无法作 - 运算");
	}

	public override BoolOperand ToBool()
	{
		throw new FormulaBadValueException("节点类型无法转换为是否型");
	}

	public override DateOperand ToDate()
	{
		throw new FormulaBadValueException("节点类型无法转换为日期型");
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		throw new FormulaNotApplicableException("节点类型无法转换为年月型");
	}

	public override NumberOperand ToNumber()
	{
		throw new FormulaBadValueException("节点类型无法转换为数值型");
	}

	public override ValueSetOperand ToValueSet()
	{
		throw new FormulaBadValueException("节点类型无法转换为数组型");
	}

	public override TimeOperand ToTime()
	{
		throw new FormulaBadValueException("节点类型无法转换为时间型");
	}

	public override string ToString()
	{
		if (TreeNode is TreeNodeNull)
		{
			return string.Empty;
		}
		return TreeNode.Name;
	}
}
