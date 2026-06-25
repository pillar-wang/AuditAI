using System;
using System.Linq;
using System.Reflection;

namespace Auditai.UI.Controls;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public struct FontSize
{
	private static float[] FontSizesByOrder;

	public string Text { get; set; }

	public float Value { get; set; }

	public static FontSize[] FontSizes { get; }

	public FontSize(string text, float value)
	{
		Text = text;
		Value = value;
	}

	static FontSize()
	{
		FontSizes = new FontSize[29]
		{
			new FontSize("初号", 42f),
			new FontSize("小初", 36f),
			new FontSize("一号", 26f),
			new FontSize("小一", 24f),
			new FontSize("二号", 22f),
			new FontSize("小二", 18f),
			new FontSize("三号", 16f),
			new FontSize("小三", 15f),
			new FontSize("四号", 14f),
			new FontSize("小四", 12f),
			new FontSize("五号", 10.5f),
			new FontSize("小五", 9f),
			new FontSize("6", 6f),
			new FontSize("8", 8f),
			new FontSize("9", 9f),
			new FontSize("10", 10f),
			new FontSize("11", 11f),
			new FontSize("12", 12f),
			new FontSize("14", 14f),
			new FontSize("16", 16f),
			new FontSize("18", 18f),
			new FontSize("20", 20f),
			new FontSize("22", 22f),
			new FontSize("24", 24f),
			new FontSize("26", 26f),
			new FontSize("28", 28f),
			new FontSize("36", 36f),
			new FontSize("48", 48f),
			new FontSize("72", 72f)
		};
		FontSizesByOrder = (from fs in FontSizes.Select((FontSize fs) => fs.Value).Distinct()
			orderby fs
			select fs).ToArray();
	}

	public static float Grow(float fontSize)
	{
		if (fontSize >= 72f)
		{
			return (float)Math.Ceiling((fontSize + 1f) / 10f) * 10f;
		}
		return FontSizesByOrder.FirstOrDefault((float fs) => fs > fontSize);
	}

	public static float Shrink(float fontSize)
	{
		if (fontSize <= 1f)
		{
			return 1f;
		}
		if (fontSize <= 6f)
		{
			return fontSize - 1f;
		}
		return FontSizesByOrder.TakeWhile((float fs) => fs < fontSize).Max();
	}
}
