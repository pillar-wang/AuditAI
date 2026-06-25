using System.Drawing;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class ButtonAdapter : ImageControl
{
	private Image _orign;

	private Button _button;

	public ButtonAdapter(Button button)
	{
		_button = button;
		_orign = _button.Image;
	}

	public Image GetImage()
	{
		return _button.Image;
	}

	public Image GetOrignImage()
	{
		return _orign;
	}

	public void SetImage(Image image)
	{
		_button.Image = image;
	}
}
