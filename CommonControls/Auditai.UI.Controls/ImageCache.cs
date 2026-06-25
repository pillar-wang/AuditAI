using System.Collections.Generic;
using System.Drawing;

namespace Auditai.UI.Controls;

public class ImageCache
{
	private static ImageCache _instance;

	private Dictionary<Image, Image> WhiteToColor;

	private Dictionary<Image, Image> ColorToWhite;

	public static ImageCache GetInstance()
	{
		if (_instance == null)
		{
			_instance = new ImageCache();
		}
		return _instance;
	}

	private ImageCache()
	{
		WhiteToColor = new Dictionary<Image, Image>();
		ColorToWhite = new Dictionary<Image, Image>();
	}

	public void Add(Image color, Image white)
	{
		if (color != null && white != null)
		{
			if (!WhiteToColor.ContainsKey(white))
			{
				WhiteToColor.Add(white, color);
			}
			if (!ColorToWhite.ContainsKey(color))
			{
				ColorToWhite.Add(color, white);
			}
		}
	}

	public Image GetColor(Image image)
	{
		if (image == null)
		{
			return null;
		}
		if (WhiteToColor.ContainsKey(image))
		{
			return WhiteToColor[image];
		}
		if (ColorToWhite.ContainsKey(image))
		{
			return image;
		}
		return null;
	}

	public Image GetWhite(Image image)
	{
		if (image == null)
		{
			return null;
		}
		if (ColorToWhite.ContainsKey(image))
		{
			return ColorToWhite[image];
		}
		if (WhiteToColor.ContainsKey(image))
		{
			return image;
		}
		return null;
	}
}
