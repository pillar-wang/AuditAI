using System.Collections.Generic;

namespace Leqisoft.Model;

public class ValueOperandEqualityComparer : IEqualityComparer<ValueOperand>
{
	public static ValueOperandEqualityComparer Instance { get; } = new ValueOperandEqualityComparer();


	private ValueOperandEqualityComparer()
	{
	}

	public bool Equals(ValueOperand x, ValueOperand y)
	{
		return (x.Equal(y) as BoolOperand).Value;
	}

	public int GetHashCode(ValueOperand obj)
	{
		return obj.GetHashCode();
	}
}
