using Newtonsoft.Json;

namespace Leqisoft.Model;

public class TableTitleColumn
{
	public float Width { get; set; }

	[JsonIgnore]
	public int WidthDisplay { get; set; }
}
