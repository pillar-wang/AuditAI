using System.Collections.Generic;
using Newtonsoft.Json;

namespace Leqisoft.DTO;

public class MachineCookie
{
	public const string MACHINE_SIGN = "cookie_machine_sign";

	[JsonProperty]
	private Dictionary<string, object> _cookieMap = new Dictionary<string, object>();

	public object Get(string key)
	{
		if (key == null)
		{
			return null;
		}
		if (!_cookieMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public void Set(string key, object value)
	{
		if (key != null)
		{
			if (_cookieMap.ContainsKey(key))
			{
				_cookieMap[key] = value;
			}
			else
			{
				_cookieMap.Add(key, value);
			}
		}
	}
}
