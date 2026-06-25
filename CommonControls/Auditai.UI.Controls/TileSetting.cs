using System.Drawing;

namespace Auditai.UI.Controls;

public class TileSetting
{
	public int Width { get; set; } = 96;


	public int Height { get; set; } = 54;


	public int FontSize { get; set; } = 9;


	public Color FontColor { get; set; } = Color.Black;


	public Color HotBorderColor { get; set; } = Color.LightGray;


	public ContentAlignment FontAlignment { get; set; } = ContentAlignment.MiddleCenter;

}
