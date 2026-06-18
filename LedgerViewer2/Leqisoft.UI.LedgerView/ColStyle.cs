using Newtonsoft.Json;

namespace Leqisoft.UI.LedgerView;

[JsonObject("ColStyle")]
public class ColStyle
{
	[JsonProperty("Name")]
	public string Name { get; set; }

	[JsonProperty("Width")]
	public int Width { get; set; }

	[JsonProperty("Visible")]
	public bool Visible { get; set; }

	public ColStyle()
	{
		Name = string.Empty;
		Width = 0;
		Visible = true;
	}

	public ColStyle(string name, bool visible)
		: this()
	{
		Width = 0;
		Name = name;
		Visible = visible;
	}
}
