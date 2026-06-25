using System.Drawing;
using System.IO;

namespace Auditai.UI.Platform;

public class IconGenerator
{
	public static Icon CreateFromImage(Bitmap img)
	{
		return Icon.FromHandle(img.GetHicon());
	}

	public static Icon CreateFromColor(Color color, int width, int height)
	{
		using Bitmap bitmap = new Bitmap(width, height);
		Graphics graphics = Graphics.FromImage(bitmap);
		using (SolidBrush brush = new SolidBrush(color))
		{
			graphics.FillRectangle(brush, 0, 0, width, height);
		}
		return CreateFromImage(bitmap);
	}

	public static Icon LoadFromFile(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
		{
			return CreateFromColor(Color.White, 32, 32);
		}
		return new Icon(filePath);
	}
}
