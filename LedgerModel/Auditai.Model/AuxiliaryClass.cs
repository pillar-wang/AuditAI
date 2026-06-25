using System.Collections.Generic;

namespace Auditai.Model;

public class AuxiliaryClass
{
	public string Code { get; set; }

	public string Name { get; set; }

	public List<AuxiliaryItem> Items { get; }

	public AuxiliaryClass()
	{
		Items = new List<AuxiliaryItem>();
	}

	public override string ToString()
	{
		return Code + " " + Name;
	}
}
