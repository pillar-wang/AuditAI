using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class SequenceEqualsComparer<TList, TEle> : IEqualityComparer<TList> where TList : IEnumerable<TEle>
{
	public static SequenceEqualsComparer<TList, TEle> Instance { get; } = new SequenceEqualsComparer<TList, TEle>();


	public bool Equals(TList x, TList y)
	{
		return x.SequenceEqual(y);
	}

	public int GetHashCode(TList obj)
	{
		int num = 19;
		foreach (TEle item in obj)
		{
			num = num * 31 + item.GetHashCode();
		}
		return num;
	}
}
