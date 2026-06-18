using System.Drawing;
using TXTextControl;

namespace Leqisoft.UI.Platform;

public class FormatPainterContext
{
	public int BaseLine { get; set; }

	public bool Bold { get; set; }

	public string FontName { get; set; }

	public int FontSize { get; set; }

	public Color ForeColor { get; set; }

	public bool Italic { get; set; }

	public bool Strikeout { get; set; }

	public Color TextBackColor { get; set; }

	public FontUnderlineStyle Underline { get; set; }

	public int AbsoluteLineSpacing { get; set; }

	public HorizontalAlignment Alignment { get; set; }

	public int BottomDistance { get; set; }

	public int HangingIndent { get; set; }

	public int LeftIndent { get; set; }

	public int LineSpacing { get; set; }

	public int TopDistance { get; set; }

	public int RightIndent { get; set; }

	public static FormatPainterContext FromSelection(Selection sel)
	{
		dynamic paragraphFormat = sel.ParagraphFormat;
		return new FormatPainterContext
		{
			BaseLine = sel.Baseline,
			Bold = sel.Bold,
			FontName = sel.FontName,
			FontSize = sel.FontSize,
			ForeColor = sel.ForeColor,
			Italic = sel.Italic,
			Strikeout = sel.Strikeout,
			TextBackColor = sel.TextBackColor,
			Underline = sel.Underline,
			AbsoluteLineSpacing = paragraphFormat.AbsoluteLineSpacing,
			Alignment = paragraphFormat.Alignment,
			BottomDistance = paragraphFormat.BottomDistance,
			HangingIndent = paragraphFormat.HangingIndent,
			LeftIndent = paragraphFormat.LeftIndent,
			LineSpacing = paragraphFormat.LineSpacing,
			TopDistance = paragraphFormat.TopDistance,
			RightIndent = paragraphFormat.RightIndent
		};
	}

	public void Apply(Selection sel)
	{
		sel.Baseline = BaseLine;
		sel.Bold = Bold;
		sel.FontName = FontName;
		sel.FontSize = FontSize;
		sel.ForeColor = ForeColor;
		sel.Italic = Italic;
		sel.Strikeout = Strikeout;
		sel.TextBackColor = TextBackColor;
		sel.Underline = Underline;
		dynamic paragraphFormat = sel.ParagraphFormat;
		if (AbsoluteLineSpacing > 0)
		{
			paragraphFormat.AbsoluteLineSpacing = AbsoluteLineSpacing;
		}
		else
		{
			paragraphFormat.LineSpacing = LineSpacing;
		}
		paragraphFormat.Alignment = Alignment;
		paragraphFormat.BottomDistance = BottomDistance;
		paragraphFormat.LeftIndent = LeftIndent;
		paragraphFormat.HangingIndent = HangingIndent;
		paragraphFormat.TopDistance = TopDistance;
		paragraphFormat.RightIndent = RightIndent;
	}
}
