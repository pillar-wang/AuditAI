using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Auditai.UI.Platform;

public class SequenceInfo
{
	private static readonly Regex re = new Regex("\\d+", RegexOptions.RightToLeft);

	public string Prefix { get; set; }

	public string Suffix { get; set; }

	public int Start { get; set; }

	public int Step { get; set; }

	public int NumberLength { get; set; }

	public static SequenceInfo GetSequenceInfo(string s1, string s2)
	{
		if (string.IsNullOrWhiteSpace(s2))
		{
			Match match = re.Match(s1);
			if (match.Success)
			{
				string prefix = s1.Substring(0, match.Index);
				string suffix = s1.Substring(match.Index + match.Length);
				try
				{
					return new SequenceInfo
					{
						Prefix = prefix,
						Suffix = suffix,
						Start = int.Parse(match.Value),
						Step = 1,
						NumberLength = match.Length
					};
				}
				catch (OverflowException)
				{
					return null;
				}
			}
			return null;
		}
		MatchCollection matchCollection = re.Matches(s1);
		MatchCollection matchCollection2 = re.Matches(s2);
		if (matchCollection.Count == 0 || matchCollection2.Count == 0 || matchCollection.Count != matchCollection2.Count)
		{
			return null;
		}
		for (int i = 0; i < matchCollection.Count; i++)
		{
			Match match2 = matchCollection[i];
			Match match3 = matchCollection2[i];
			if (match2.Value == match3.Value)
			{
				continue;
			}
			string text = s1.Substring(0, match2.Index);
			string text2 = s1.Substring(match2.Index + match2.Length);
			string text3 = s2.Substring(0, match3.Index);
			string text4 = s2.Substring(match3.Index + match3.Length);
			if (text == text3 && text2 == text4)
			{
				try
				{
					return new SequenceInfo
					{
						Prefix = text,
						Suffix = text2,
						Start = int.Parse(match2.Value),
						Step = int.Parse(match3.Value) - int.Parse(match2.Value),
						NumberLength = match2.Length
					};
				}
				catch (OverflowException)
				{
					return null;
				}
			}
			return null;
		}
		return null;
	}

	public string GetNth(int n)
	{
		StringBuilder stringBuilder = new StringBuilder(Prefix);
		stringBuilder.AppendFormat($"{{0:D{NumberLength}}}", Start + n * Step);
		stringBuilder.Append(Suffix);
		return stringBuilder.ToString();
	}
}
