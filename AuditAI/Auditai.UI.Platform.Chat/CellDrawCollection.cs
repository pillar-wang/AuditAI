using System.Collections.Generic;
using System.Drawing;
using Auditai.Model;

namespace Auditai.UI.Platform.Chat;

public class CellDrawCollection
{
	public Dictionary<Cell, System.Drawing.Image> Values { get; private set; }

	public System.Drawing.Image this[Cell cell]
	{
		get
		{
			if (Values.TryGetValue(cell, out var value))
			{
				return value;
			}
			return null;
		}
	}

	public bool IsEmpty => Values.Count == 0;

	public CellDrawCollection()
	{
		Values = new Dictionary<Cell, System.Drawing.Image>();
	}

	public void Add(Cell cell, System.Drawing.Image image)
	{
		if (!Values.ContainsKey(cell))
		{
			Values.Add(cell, image);
		}
	}

	public void Clear()
	{
		Values.Clear();
	}
}
