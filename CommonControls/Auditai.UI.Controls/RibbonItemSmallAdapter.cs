using System.Drawing;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Controls;

public class RibbonItemSmallAdapter : ImageControl
{
	private Image _orign;

	private RibbonItem _ribbonItem;

	public RibbonItemSmallAdapter(RibbonItem ribbonItem)
	{
		_ribbonItem = ribbonItem;
		_orign = _ribbonItem.SmallImage;
	}

	public Image GetImage()
	{
		return _ribbonItem.SmallImage;
	}

	public Image GetOrignImage()
	{
		return _orign;
	}

	public void SetImage(Image image)
	{
		_ribbonItem.SmallImage = image;
	}
}
