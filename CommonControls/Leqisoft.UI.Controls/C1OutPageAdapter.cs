using System.Drawing;
using C1.Win.C1Command;

namespace Leqisoft.UI.Controls;

public class C1OutPageAdapter : ImageControl
{
	private C1OutPage _owner;

	private Image _original;

	public C1OutPageAdapter(C1OutPage owner)
	{
		_owner = owner;
		_original = owner.Image;
	}

	public Image GetImage()
	{
		return _owner.Image;
	}

	public Image GetOrignImage()
	{
		return _original;
	}

	public void SetImage(Image image)
	{
		_owner.Image = image;
	}
}
