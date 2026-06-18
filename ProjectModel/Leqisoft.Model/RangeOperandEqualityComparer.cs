using System.Collections.Generic;

namespace Leqisoft.Model;

public class RangeOperandEqualityComparer : IEqualityComparer<RangeOperand>
{
	public static RangeOperandEqualityComparer Default { get; } = new RangeOperandEqualityComparer();


	public bool Equals(RangeOperand x, RangeOperand y)
	{
		if (x.TopLeft == y.TopLeft)
		{
			return x.BottomRight == y.BottomRight;
		}
		return false;
	}

	public int GetHashCode(RangeOperand obj)
	{
		return obj.TopLeft.GetHashCode() ^ obj.BottomRight.GetHashCode();
	}
}
