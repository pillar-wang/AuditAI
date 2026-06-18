using System.Drawing;
using C1.Win.C1Ribbon;

namespace Leqisoft.UI.CommonControls;

public class RibbonLargeFlickerProxy : AbstractFlickerProxy
{
	private RibbonButton _rb;

	public RibbonLargeFlickerProxy(RibbonButton rb)
	{
		_rb = rb;
		orignImage = GetImage();
		twinkleImage = GetImage();
		orignContent = GetContent();
		twinkleContent = GetContent();
	}

	protected override void SetView(Image image, string content)
	{
		_rb.LargeImage = image;
	}

	protected override string GetContent()
	{
		return _rb.Text;
	}

	protected override Image GetImage()
	{
		return _rb.LargeImage;
	}

	public override bool IsDisposed()
	{
		if (!(_rb == null))
		{
			return _rb.IsDisposed;
		}
		return true;
	}
}
