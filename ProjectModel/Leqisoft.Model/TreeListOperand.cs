using System;
using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class TreeListOperand : Operand
{
	public List<TreeListNode> Roots { get; } = new List<TreeListNode>();


	public override OperandType OperandType => OperandType.TreeListOperand;

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
		if (other is TreeListOperand rhs)
		{
			return Merge(rhs);
		}
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

	public override DateYearMonthOperand ToDateYearMonth()
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

	public TreeListNode AddOrGet(string s)
	{
		TreeListNode treeListNode = Roots.FirstOrDefault((TreeListNode n) => n.Text == s);
		if (treeListNode == null)
		{
			treeListNode = new TreeListNode
			{
				Text = s
			};
			Roots.Add(treeListNode);
		}
		return treeListNode;
	}

	private TreeListOperand Merge(TreeListOperand rhs)
	{
		foreach (TreeListNode root in rhs.Roots)
		{
			TreeListNode l2 = AddOrGet(root.Text);
			Recurse(l2, root);
		}
		return this;
		static void Recurse(TreeListNode l, TreeListNode r)
		{
			foreach (TreeListNode child in r.Children)
			{
				TreeListNode l3 = l.AddOrGet(child.Text);
				Recurse(l3, child);
			}
		}
	}
}
