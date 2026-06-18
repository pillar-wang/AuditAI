using System.Drawing;
using C1.Win.C1Tile;

namespace Leqisoft.UI.CommonControls;

public class TileFlickerProxy : AbstractFlickerProxy
{
	private Tile _tile;

	public string Text1 { get; set; } = string.Empty;


	public TileFlickerProxy(Tile tile)
	{
		_tile = tile;
		orignContent = GetContent();
		twinkleContent = GetContent();
		orignImage = GetImage();
		twinkleImage = GetImage();
		emptyImage = null;
	}

	protected override string GetContent()
	{
		return _tile.Text;
	}

	protected override Image GetImage()
	{
		return _tile.Image1;
	}

	protected override void SetView(Image image, string content)
	{
		if (content == twinkleContent)
		{
			_tile.Text1 = Text1;
		}
		else if (content == GetEmptyContent())
		{
			_tile.Text1 = GetEmptyContent();
		}
		_tile.Image1 = image;
		_tile.Text = content;
	}

	public override void Stop()
	{
		base.Stop();
		_tile.Text1 = string.Empty;
	}

	public override bool IsDisposed()
	{
		if (_tile != null)
		{
			return _tile.IsDisposed;
		}
		return true;
	}
}
