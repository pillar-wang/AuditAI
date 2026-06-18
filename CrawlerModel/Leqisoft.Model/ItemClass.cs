using System.Collections.Generic;

namespace Leqisoft.Model;

public class ItemClass
{
	public string Code { get; set; }

	public string Name { get; set; }

	public List<Item> Items { get; set; } = new List<Item>();


	public override string ToString()
	{
		return Code + " " + Name;
	}
}
