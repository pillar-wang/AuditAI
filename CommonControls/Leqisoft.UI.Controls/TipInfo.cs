using System;

namespace Leqisoft.UI.Controls;

public class TipInfo
{
	public string Key { get; set; }

	public string Title { get; set; }

	public string Body { get; set; }

	public static TipInfo Parse(string str)
	{
		if (str == null)
		{
			return null;
		}
		string[] array = str.Split(new string[1] { "[|]" }, StringSplitOptions.None);
		if (array.Length < 2)
		{
			return null;
		}
		return new TipInfo
		{
			Title = array[0],
			Body = array[1]
		};
	}
}
