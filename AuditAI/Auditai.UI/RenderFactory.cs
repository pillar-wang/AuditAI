using System;
using C1.C1Preview;

namespace Auditai.UI;

public class RenderFactory
{
	public static RenderRichText CreateRichText(string render)
	{
		RenderRichText renderRichText = new RenderRichText();
		switch (render)
		{
		case "left":
			renderRichText.Style.TextAlignHorz = AlignHorzEnum.Left;
			break;
		case "center":
			renderRichText.Style.TextAlignHorz = AlignHorzEnum.Center;
			break;
		case "right":
			renderRichText.Style.TextAlignHorz = AlignHorzEnum.Right;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case "default":
			break;
		}
		return renderRichText;
	}
}
