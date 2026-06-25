using System.Drawing;
using C1.Win.C1Command;

namespace Auditai.UI.Controls;

public class C1CommandAdapter : ImageControl
{
	private Image _orign;

	private C1Command _c1Command;

	public C1CommandAdapter(C1Command command)
	{
		_c1Command = command;
		_orign = _c1Command.Image;
	}

	public C1CommandAdapter(C1Command command, Image orign)
	{
		_c1Command = command;
		_orign = orign;
	}

	public Image GetImage()
	{
		return _c1Command.Image;
	}

	public Image GetOrignImage()
	{
		return _orign;
	}

	public void SetImage(Image image)
	{
		_c1Command.Image = image;
	}
}
