using System.Drawing;
using System.IO;

namespace Auditai.UI.Platform;

public static class ImageEx
{
	public static Bitmap ToGray(this Bitmap src)
	{
		Bitmap bitmap = new Bitmap(src.Width, src.Height);
		for (int i = 0; i < src.Width; i++)
		{
			for (int j = 0; j < src.Height; j++)
			{
				Color pixel = src.GetPixel(i, j);
				int num = (pixel.R + pixel.G + pixel.B) / 3;
				bitmap.SetPixel(i, j, Color.FromArgb(pixel.A, num, num, num));
			}
		}
		return bitmap;
	}

	public static Bitmap ToSize(this Image image, int width, int height)
	{
		Bitmap bitmap = new Bitmap(width, height);
		using Graphics graphics = Graphics.FromImage(bitmap);
		int num = ((image.Width < image.Height) ? image.Width : image.Height);
		RectangleF destRect = new RectangleF(0f, 0f, bitmap.Width, bitmap.Height);
		RectangleF srcRect = new RectangleF(0f, 0f, num, num);
		graphics.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
		return bitmap;
	}

	public static byte[] ToBytes(this Image image)
	{
		try
		{
			string tempFileName = Path.GetTempFileName();
			image.Save(tempFileName);
			using FileStream fileStream = new FileStream(tempFileName, FileMode.Open, FileAccess.Read);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			return array;
		}
		catch
		{
			return null;
		}
	}
}
