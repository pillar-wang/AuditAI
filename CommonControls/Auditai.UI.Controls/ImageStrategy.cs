using System.Drawing;

namespace Auditai.UI.Controls;

public interface ImageStrategy
{
	Image ProcessImage(Image image);
}
