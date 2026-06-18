using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Leqisoft.Model;

public sealed class SelectFilter : FilterBase
{
	[JsonProperty]
	public HashSet<string> Values { get; set; }

	public override HashSet<int> Execute(List<FilterValue> data)
	{
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < data.Count; i++)
		{
			if (Values.Contains(data[i].Display))
			{
				hashSet.Add(i);
			}
		}
		return hashSet;
	}

	public override string ToFormula(string colText)
	{
		return string.Join(" Or ", Values.Select((string v) => colText + "=\"" + v + "\""));
	}
}
