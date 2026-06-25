using System.Drawing;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Controls;

public class RibbonItemLargeAdapter : ImageControl
{
	private Image _orign;

	private RibbonItem _ribbonItem;

	public RibbonItemLargeAdapter(RibbonItem ribbonItem)
	{
		_ribbonItem = ribbonItem;
		_orign = _ribbonItem.LargeImage;
	}

	public Image GetImage()
	{
		return _ribbonItem.LargeImage;
	}

	public Image GetOrignImage()
	{
		return _orign;
	}

	public void SetImage(Image image)
	{
		_ribbonItem.LargeImage = image;
	}
}
