using System.Collections.Generic;
using Newtonsoft.Json;

namespace Leqisoft.UI.Controls.CollectDic;

[JsonObject("row")]
public class ERow
{
	[JsonProperty("cd")]
	public Dictionary<string, ECell> _cellsDic;

	public ECell this[string key]
	{
		get
		{
			return _cellsDic[key];
		}
		set
		{
			if (_cellsDic.ContainsKey(key))
			{
				_cellsDic[key] = value;
			}
			else
			{
				_cellsDic.Add(key, value);
			}
		}
	}

	public ERow()
	{
		_cellsDic = new Dictionary<string, ECell>();
	}
}
