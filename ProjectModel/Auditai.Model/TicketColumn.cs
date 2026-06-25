using System;
using System.Drawing;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Model;

[JsonObject]
public class TicketColumn
{
	private int _width;

	public int Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = Math.Max(1, value);
		}
	}

	public Id64 Field { get; set; }

	public string FontFamily { get; set; }

	public float FontSize { get; set; }

	public CellTextAlign Align { get; set; }

	public int Indent { get; set; }

	public TicketBorder Top { get; set; }

	public TicketBorder Right { get; set; }

	public TicketBorder Bottom { get; set; }

	public TicketBorder Left { get; set; }

	public TicketBorder Middle { get; set; }

	public Color ForeColor { get; set; }

	public bool Bold { get; set; }

	public bool Italic { get; set; }

	public Color BackColor { get; set; }

	public string Formula { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public DataFormat? DataFormat { get; set; }

	public bool IsHiddenColumn { get; set; }

	public bool HasField()
	{
		return !Field.IsZero();
	}

	public bool HasFormula()
	{
		return !string.IsNullOrEmpty(Formula);
	}

	public Font GetFont()
	{
		FontStyle fontStyle = FontStyle.Regular;
		if (Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		return new Font(FontFamily, FontSize, fontStyle);
	}
}
