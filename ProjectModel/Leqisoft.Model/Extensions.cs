using System.Drawing;

namespace Leqisoft.Model;

public static class Extensions
{
	public static int? ToNullableInt(this Color? color)
	{
		if (color.HasValue)
		{
			return color.Value.ToArgb();
		}
		return null;
	}

	public static Color? ToNullableColor(this int? int32)
	{
		if (int32.HasValue)
		{
			return Color.FromArgb(int32.Value);
		}
		return null;
	}
}
