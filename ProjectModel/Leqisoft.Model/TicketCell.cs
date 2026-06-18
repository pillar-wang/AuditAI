using System.Drawing;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

[JsonObject]
public class TicketCell
{
	public string Text { get; set; }

	public Id64 Field { get; set; }

	public string FontFamily { get; set; }

	public float FontSize { get; set; }

	public CellTextAlign Align { get; set; }

	public int Indent { get; set; }

	public TicketBorder Top { get; set; }

	public TicketBorder Right { get; set; }

	public TicketBorder Bottom { get; set; }

	public TicketBorder Left { get; set; }

	public bool Bold { get; set; }

	public Color ForeColor { get; set; }

	public bool Italic { get; set; }

	public Color BackColor { get; set; }

	public string Formula { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public DataFormat? DataFormat { get; set; }

	public string InputValue { get; set; }

	public bool IsInMixRangeFixedDataRow { get; set; }

	public bool IsInMixRangeDynamicDataRow { get; set; }

	public bool IsMixRangeTicketKey { get; set; }

	public bool HasField()
	{
		return !Field.IsZero();
	}

	public bool HasText()
	{
		return !string.IsNullOrEmpty(Text);
	}

	public bool HasFormula()
	{
		return !string.IsNullOrEmpty(Formula);
	}

	public string GetInputValue()
	{
		if (!string.IsNullOrEmpty(InputValue))
		{
			return InputValue;
		}
		return Text;
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
