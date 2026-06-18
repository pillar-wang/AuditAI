using System.Drawing;

namespace Leqisoft.UI.Controls;

public class WhiteImageStrategy : ImageStrategy
{
	public Image ProcessImage(Image image)
	{
		Image white = ImageCache.GetInstance().GetWhite(image);
		if (white != null)
		{
			return white;
		}
		if (image is Bitmap { Width: var width, Height: var height } bitmap)
		{
			Bitmap bitmap2 = new Bitmap(width, height);
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					Color pixel = bitmap.GetPixel(i, j);
					byte r = pixel.R;
					byte g = pixel.G;
					byte b = pixel.B;
					byte a = pixel.A;
					if (a == 0)
					{
						bitmap2.SetPixel(i, j, Color.FromArgb(a, r, g, b));
					}
					else
					{
						bitmap2.SetPixel(i, j, Color.FromArgb(a, 255, 255, 255));
					}
				}
			}
			ImageCache.GetInstance().Add(bitmap, bitmap2);
			return bitmap2;
		}
		return null;
	}
}
