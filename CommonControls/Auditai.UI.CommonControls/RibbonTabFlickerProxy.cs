using System.Drawing;
using C1.Win.C1Ribbon;

namespace Auditai.UI.CommonControls;

public class RibbonTabFlickerProxy : AbstractFlickerProxy
{
	private RibbonTab _rt;

	public RibbonTabFlickerProxy(RibbonTab rt)
	{
		_rt = rt;
		orignImage = GetImage();
		twinkleImage = GetImage();
		orignContent = GetContent();
		twinkleContent = GetContent();
	}

	protected override void SetView(Image image, string content)
	{
		_rt.Text = content;
		_rt.Image = image;
	}

	protected override string GetContent()
	{
		return _rt.Text;
	}

	protected override string GetEmptyContent()
	{
		return new string('\u3000', orignContent.Length);
	}

	protected override Image GetImage()
	{
		return _rt.Image;
	}

	public override bool IsDisposed()
	{
		if (_rt != null)
		{
			return _rt.IsDisposed;
		}
		return true;
	}
}
