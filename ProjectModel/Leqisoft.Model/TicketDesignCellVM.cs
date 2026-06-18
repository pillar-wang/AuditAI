using System.Drawing;

namespace Leqisoft.Model;

public class TicketDesignCellVM
{
	private int _indent;

	public string Text { get; set; }

	public string FontFamily { get; set; }

	public float FontSize { get; set; }

	public Color ForeColor { get; set; }

	public bool Bold { get; set; }

	public bool Italic { get; set; }

	public Color BackColor { get; set; }

	public CellTextAlign Align { get; set; }

	public TicketBorder Top { get; set; }

	public TicketBorder Right { get; set; }

	public TicketBorder Bottom { get; set; }

	public TicketBorder Left { get; set; }

	public string Formula { get; set; }

	public DataFormat? DataFormat { get; set; }

	public long MixRangeTicketColumnId { get; set; } = -1L;


	public int CellId { get; set; }

	public int Indent
	{
		get
		{
			return _indent;
		}
		set
		{
			_indent = value;
			if (_indent < 0)
			{
				_indent = 0;
			}
		}
	}

	public bool HasFormula()
	{
		return !string.IsNullOrEmpty(Formula);
	}

	public void UpdateFormula(string s)
	{
		Formula = s;
	}

	public TicketCell ToModel()
	{
		return new TicketCell
		{
			Text = Text,
			FontFamily = FontFamily,
			FontSize = FontSize,
			ForeColor = ForeColor,
			Bold = Bold,
			Italic = Italic,
			BackColor = BackColor,
			Align = Align,
			Top = Top.Clone(),
			Right = Right.Clone(),
			Bottom = Bottom.Clone(),
			Left = Left.Clone(),
			Formula = Formula,
			Indent = Indent,
			DataFormat = DataFormat
		};
	}
}
