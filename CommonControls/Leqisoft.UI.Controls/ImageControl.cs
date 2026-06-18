using System.Drawing;

namespace Leqisoft.UI.Controls;

public interface ImageControl
{
	Image GetImage();

	Image GetOrignImage();

	void SetImage(Image image);
}
