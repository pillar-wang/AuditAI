using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using C1.Win.C1Themes;

namespace Leqisoft.UI.Controls;

public class LeqiTheme
{
	private C1Theme _c1theme;

	public string Id { get; set; }

	public string Name { get; set; }

	public string FriendName { get; set; }

	public ThemeEnum ThemeFlags { get; set; }

	public ThemeContext ThemeContext { get; set; }

	public byte[] ThemeBytes { get; set; }

	public C1Theme GetC1Theme()
	{
		if (_c1theme != null)
		{
			return _c1theme;
		}
		if (!C1ThemeController.IsThemeRegistered(Name))
		{
			using MemoryStream stream = new MemoryStream(ThemeBytes);
			C1ThemeController.RegisterTheme(stream, C1ThemeFormat.Xml);
		}
		_c1theme = C1ThemeController.GetThemeByName(Name, throwException: false);
		return _c1theme;
	}

	public LeqiTheme()
	{
		ThemeContext = new ThemeContext();
	}

	public Bitmap GetThemedBitmap(Bitmap bitmap)
	{
		if (ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			bitmap = (Bitmap)new WhiteImageStrategy().ProcessImage(bitmap);
		}
		return bitmap;
	}

	public Icon GetThemedIcon(params Bitmap[] bitmaps)
	{
		List<MemoryStream> list = bitmaps.Select(delegate(Bitmap b)
		{
			MemoryStream memoryStream2 = new MemoryStream();
			GetThemedBitmap(b).Save(memoryStream2, ImageFormat.Png);
			memoryStream2.Seek(0L, SeekOrigin.Begin);
			return memoryStream2;
		}).ToList();
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((short)0);
		binaryWriter.Write((short)1);
		binaryWriter.Write((short)bitmaps.Length);
		int num = 6 + 16 * bitmaps.Length;
		for (int i = 0; i < bitmaps.Length; i++)
		{
			binaryWriter.Write((byte)bitmaps[i].Width);
			binaryWriter.Write((byte)bitmaps[i].Height);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((short)1);
			binaryWriter.Write((short)32);
			binaryWriter.Write((int)list[i].Length);
			binaryWriter.Write(num);
			num += (int)list[i].Length;
		}
		foreach (MemoryStream item in list)
		{
			item.CopyTo(memoryStream);
		}
		binaryWriter.Flush();
		binaryWriter.Seek(0, SeekOrigin.Begin);
		return new Icon(memoryStream);
	}

	public ThemeBackground GetBackground(string path)
	{
		return (ThemeBackground)GetC1Theme().GetEnum(path);
	}

	public Color GetBackgroundSolidColor(string path)
	{
		return GetBackground(path).GetSolidColor().GetValueOrDefault();
	}

	public string GetCssBackground(string path, int alpha = 255)
	{
		ThemeBackground background = GetBackground(path);
		switch (background.BackgroundType)
		{
		case ThemeBackgroundType.Solid:
		{
			ThemeSolidBackground themeSolidBackground = (ThemeSolidBackground)background;
			return ColorToCss(Color.FromArgb(alpha, themeSolidBackground.Color.Value));
		}
		case ThemeBackgroundType.TwoColorLinear:
		{
			ThemeGradientBackground themeGradientBackground = (ThemeGradientBackground)background;
			LinearGradient linearGradient = (LinearGradient)themeGradientBackground.Mode;
			TwoColorGradient twoColorGradient = (TwoColorGradient)themeGradientBackground.Colors;
			return "linear-gradient(" + LinearGradientModeToCss(linearGradient.Mode.Value) + ", " + ColorToCss(twoColorGradient.Color1.Value) + ", " + ColorToCss(twoColorGradient.Color2.Value) + ")";
		}
		default:
			return ColorToCss(background.GetSolidColor().Value);
		}
	}

	public string GetCssColor(string path)
	{
		return ColorToCss(GetC1Theme().GetColor(path));
	}

	public static string ColorToCss(Color color)
	{
		return $"rgba({color.R}, {color.G}, {color.B}, {(double)(int)color.A / 255.0})";
	}

	public static string LinearGradientModeToCss(LinearGradientMode lgm)
	{
		if (lgm == LinearGradientMode.Vertical)
		{
			return "to bottom";
		}
		throw new ArgumentOutOfRangeException();
	}
}
