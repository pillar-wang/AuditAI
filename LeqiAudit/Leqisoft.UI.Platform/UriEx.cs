using System;
using System.Text.RegularExpressions;

namespace Leqisoft.UI.Platform;

public static class UriEx
{
	public static bool TryGetValue(this Uri url, string key, out string value)
	{
		Regex regex = new Regex("[?&]" + key + "=([^&]+)");
		Match match = regex.Match(url.Query);
		System.Text.RegularExpressions.Group group = match.Groups[1];
		value = group.Value;
		return group.Success;
	}
}
