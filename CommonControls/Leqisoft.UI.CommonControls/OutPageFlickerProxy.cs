using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Command;

namespace Leqisoft.UI.CommonControls;

public class OutPageFlickerProxy : AbstractFlickerProxy
{
	private C1OutPage _outPage;

	public OutPageFlickerProxy(C1OutPage outPage)
	{
		_outPage = outPage;
		orignImage = GetImage();
		twinkleImage = GetImage();
		orignContent = GetContent();
		twinkleContent = GetContent();
	}

	public OutPageFlickerProxy(C1OutPage outPage, Timer timer, Image trans)
		: this(outPage)
	{
		SetTimer(timer);
		UpdateEmptyImage(trans);
	}

	protected override void SetView(Image image, string content)
	{
		_outPage.Image = image;
	}

	protected override string GetContent()
	{
		return _outPage.Text;
	}

	protected override Image GetImage()
	{
		return _outPage.Image;
	}

	public override bool IsDisposed()
	{
		if (_outPage != null)
		{
			return _outPage.IsDisposed;
		}
		return true;
	}
}
