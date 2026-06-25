using System.Collections.Generic;
using System.Drawing;

namespace Auditai.UI.Platform.Chat;

public class DrawCollection
{
	public Dictionary<int, Dictionary<int, Image>> Values { get; private set; }

	public Image this[int r, int c]
	{
		get
		{
			if (Values.ContainsKey(r) && Values[r].ContainsKey(c))
			{
				return Values[r][c];
			}
			return null;
		}
	}

	public DrawCollection()
	{
		Values = new Dictionary<int, Dictionary<int, Image>>();
	}

	public void Add(int r, int c, Image image)
	{
		if (!Values.ContainsKey(r))
		{
			Values.Add(r, new Dictionary<int, Image>());
		}
		Dictionary<int, Image> dictionary = Values[r];
		if (!dictionary.ContainsKey(c))
		{
			dictionary.Add(c, image);
		}
		dictionary[c] = image;
	}

	public void Remove(int r, int c)
	{
		if (Values.ContainsKey(r))
		{
			Dictionary<int, Image> dictionary = Values[r];
			if (dictionary.ContainsKey(c))
			{
				dictionary.Remove(c);
			}
		}
	}

	public void Clear()
	{
		Values = new Dictionary<int, Dictionary<int, Image>>();
	}
}
