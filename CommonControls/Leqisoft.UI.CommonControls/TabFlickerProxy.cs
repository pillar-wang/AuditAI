using System.Drawing;
using C1.Win.C1Command;

namespace Leqisoft.UI.CommonControls;

public class TabFlickerProxy : AbstractFlickerProxy
{
	private C1DockingTabPage _tabPage;

	public TabFlickerProxy(C1DockingTabPage tabPage)
	{
		_tabPage = tabPage;
		orignImage = GetImage();
		twinkleImage = GetImage();
		orignContent = GetContent();
		twinkleContent = GetContent();
	}

	protected override void SetView(Image image, string content)
	{
		_tabPage.Image = image;
	}

	protected override string GetContent()
	{
		return _tabPage.Text;
	}

	protected override Image GetImage()
	{
		return _tabPage.Image;
	}

	public override bool IsDisposed()
	{
		if (_tabPage != null)
		{
			return _tabPage.IsDisposed;
		}
		return true;
	}
}
