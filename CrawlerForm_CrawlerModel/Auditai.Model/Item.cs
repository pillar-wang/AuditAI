namespace Auditai.Model;

public class Item
{
	public string Code { get; set; }

	public string Name { get; set; }

	public ItemClass ItemClass { get; set; }

	public override string ToString()
	{
		return Code + " " + Name;
	}
}
