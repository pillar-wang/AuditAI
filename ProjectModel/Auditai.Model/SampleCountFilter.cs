using System;
using Newtonsoft.Json;

namespace Auditai.Model;

public abstract class SampleCountFilter : FilterBase
{
	[JsonProperty]
	public int Count { get; set; }

	public override string ToFormula(string colText)
	{
		throw new NotSupportedException();
	}
}
