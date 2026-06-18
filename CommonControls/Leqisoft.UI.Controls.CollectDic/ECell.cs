using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Leqisoft.UI.Controls.CollectDic;

[JsonObject("cell")]
public class ECell
{
	[JsonProperty("s")]
	public bool IsSingle { get; set; }

	[JsonProperty("v")]
	public string Value { get; set; }

	[JsonProperty("vs")]
	public List<string> Values { get; set; }

	public ECell()
	{
		Values = new List<string>();
	}

	public bool AnyMatch(string value)
	{
		if (value == null)
		{
			return false;
		}
		if (IsSingle)
		{
			return Regex.IsMatch(value.Trim(), Value);
		}
		return Values.Any((string v) => Regex.IsMatch(value.Trim(), v));
	}

	public bool AnyEquals(string value)
	{
		if (IsSingle)
		{
			return value.Equals(Value?.Trim());
		}
		return Values.Any((string v) => value.Equals(v?.Trim()));
	}
}
