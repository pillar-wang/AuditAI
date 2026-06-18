using System.Drawing;
using Newtonsoft.Json;

namespace Leqisoft.DTO;

public class FontSetting
{
	private float _fontSize;

	[JsonProperty(PropertyName = "Bold")]
	public bool Bold { get; set; }

	[JsonProperty(PropertyName = "FontFamily")]
	public string FontFamily { get; set; }

	[JsonProperty(PropertyName = "FontColor")]
	public Color FontColor { get; set; }

	[JsonProperty(PropertyName = "FontSize")]
	public float FontSize
	{
		get
		{
			if (_fontSize < 5f)
			{
				return 5f;
			}
			if (_fontSize > 75f)
			{
				return 75f;
			}
			return _fontSize;
		}
		set
		{
			_fontSize = value;
		}
	}

	public FontSetting()
	{
		FontFamily = "微软雅黑";
		FontColor = Color.Black;
		FontSize = 9f;
	}
}
