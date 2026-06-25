using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public sealed class EquidistanceFilter : SampleCountFilter
{
	public override HashSet<int> Execute(List<FilterValue> data)
	{
		if (base.Count == 0)
		{
			return new HashSet<int>(Enumerable.Range(0, data.Count));
		}
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < data.Count; i += base.Count)
		{
			hashSet.Add(i);
		}
		return hashSet;
	}
}
