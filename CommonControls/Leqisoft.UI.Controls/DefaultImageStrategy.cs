using System.Drawing;

namespace Leqisoft.UI.Controls;

public class DefaultImageStrategy : ImageStrategy
{
	public Image ProcessImage(Image image)
	{
		Image color = ImageCache.GetInstance().GetColor(image);
		if (color != null)
		{
			return color;
		}
		return image;
	}
}
