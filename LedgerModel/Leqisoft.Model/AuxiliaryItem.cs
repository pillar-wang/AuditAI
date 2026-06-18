namespace Leqisoft.Model;

public class AuxiliaryItem
{
	public string Code { get; set; }

	public string Name { get; set; }

	public AuxiliaryClass Class { get; set; }

	public override string ToString()
	{
		return Code + " " + Name;
	}
}
