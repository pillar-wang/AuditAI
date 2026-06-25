using System.Drawing;
using C1.Win.C1Ribbon;

namespace Auditai.UI.CommonControls;

public class RibbonFlickerProxy : AbstractFlickerProxy
{
	private RibbonButton _rb;

	public RibbonFlickerProxy(RibbonButton rb)
	{
		_rb = rb;
		orignImage = GetImage();
		twinkleImage = GetImage();
		orignContent = GetContent();
		twinkleContent = GetContent();
	}

	protected override void SetView(Image image, string content)
	{
		_rb.SmallImage = image;
	}

	protected override string GetContent()
	{
		return _rb.Text;
	}

	protected override Image GetImage()
	{
		return _rb.SmallImage;
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
