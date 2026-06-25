using System.Drawing;

namespace Auditai.UI.Controls;

public interface ImageControl
{
	Image GetImage();

	Image GetOrignImage();

	void SetImage(Image image);
}
