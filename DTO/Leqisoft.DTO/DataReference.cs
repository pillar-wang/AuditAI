namespace Leqisoft.DTO;

public class DataReference
{
	public Id64 Id { get; set; }

	public string Key { get; set; }

	public string Value { get; set; }

	public int Status { get; set; }

	public int Dirty { get; set; }

	public int Kind { get; set; }
}
