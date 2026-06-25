using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class FontComboBox : ComboBox
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private class LOGFONT
	{
		public int lfHeight;

		public int lfWidth;

		public int lfEscapement;

		public int lfOrientation;

		public int lfWeight;

		public byte lfItalic;

		public byte lfUnderline;

		public byte lfStrikeOut;

		public FontCharSet lfCharSet;

		public byte lfOutPrecision;

		public byte lfClipPrecision;

		public byte lfQuality;

		public byte lfPitchAndFamily;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string lfFaceName = string.Empty;
	}

	private enum FontCharSet : byte
	{
		ANSI_CHARSET = 0,
		DEFAULT_CHARSET = 1,
		SYMBOL_CHARSET = 2,
		SHIFTJIS_CHARSET = 128,
		HANGEUL_CHARSET = 129,
		HANGUL_CHARSET = 129,
		GB2312_CHARSET = 134,
		CHINESEBIG5_CHARSET = 136,
		OEM_CHARSET = byte.MaxValue,
		JOHAB_CHARSET = 130,
		HEBREW_CHARSET = 177,
		ARABIC_CHARSET = 178,
		GREEK_CHARSET = 161,
		TURKISH_CHARSET = 162,
		VIETNAMESE_CHARSET = 163,
		THAI_CHARSET = 222,
		EASTEUROPE_CHARSET = 238,
		RUSSIAN_CHARSET = 204,
		MAC_CHARSET = 77,
		BALTIC_CHARSET = 186
	}

	private static readonly Regex cjkCharRegex;

	private static readonly List<string> _families;

	private static readonly Dictionary<string, FontCharSet> _dicFamilyCharset;

	private static readonly StringFormat _sf;

	public string SelectedFontFamily => base.SelectedValue as string;

	static FontComboBox()
	{
		cjkCharRegex = new Regex("\\p{IsCJKUnifiedIdeographs}");
		_dicFamilyCharset = new Dictionary<string, FontCharSet>();
		_sf = new StringFormat(StringFormatFlags.NoWrap);
		using (InstalledFontCollection installedFontCollection = new InstalledFontCollection())
		{
			FontFamily[] families = installedFontCollection.Families;
			foreach (FontFamily fontFamily in families)
			{
				if (string.IsNullOrWhiteSpace(fontFamily.Name))
				{
					continue;
				}
				try
				{
					using Font font = new Font(fontFamily, 10f);
					LOGFONT lOGFONT = new LOGFONT();
					font.ToLogFont(lOGFONT);
					_dicFamilyCharset.Add(fontFamily.Name, lOGFONT.lfCharSet);
					font.FontFamily.Dispose();
					fontFamily.Dispose();
				}
				catch
				{
				}
			}
		}
		_families = _dicFamilyCharset.Keys.OrderBy((string f) => !cjkCharRegex.IsMatch(f)).ToList();
	}

	public FontComboBox()
	{
		base.DrawMode = DrawMode.OwnerDrawFixed;
		base.DrawItem += FontComboBox_DrawItem;
		base.DataSource = _families;
	}

	private void FontComboBox_DrawItem(object sender, DrawItemEventArgs e)
	{
		string text = (string)base.Items[e.Index];
		e.DrawBackground();
		e.DrawFocusRectangle();
		try
		{
			if (_dicFamilyCharset[text] == FontCharSet.SYMBOL_CHARSET)
			{
				TextRenderer.DrawText(e.Graphics, text, Control.DefaultFont, e.Bounds, e.ForeColor, TextFormatFlags.Default);
				return;
			}
			using FontFamily family = new FontFamily(text);
			using Font font = new Font(family, Font.Size);
			TextRenderer.DrawText(e.Graphics, text, font, e.Bounds, e.ForeColor, TextFormatFlags.Default);
			font.FontFamily.Dispose();
		}
		catch
		{
			TextRenderer.DrawText(e.Graphics, text, Control.DefaultFont, e.Bounds, e.ForeColor, TextFormatFlags.Default);
		}
	}
}
