using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;

namespace Leqisoft.UI.Controls;

public class C1LabelEx : C1Label
{
	public delegate bool BackgroundPaintHandle(object sender, PaintEventArgs e);

	public Color TextColor { get; set; } = Color.Black;


	public BackgroundPaintHandle BackgroundRenderCallback { get; set; }

	public Action<object, PaintEventArgs> PaintCallback { get; set; }

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		if (BackgroundRenderCallback == null)
		{
			base.OnPaintBackground(e);
		}
		else if (!BackgroundRenderCallback(this, e))
		{
			base.OnPaintBackground(e);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		using (SolidBrush brush = new SolidBrush(TextColor))
		{
			e.Graphics.DrawString(Text, Font, brush, GetDrawArea(), GetStringFormat());
		}
		if (PaintCallback != null)
		{
			PaintCallback(this, e);
		}
	}

	private RectangleF GetDrawArea()
	{
		Rectangle clientRectangle = base.ClientRectangle;
		return new RectangleF(clientRectangle.X, clientRectangle.Y, clientRectangle.Width, clientRectangle.Height);
	}

	private StringFormat GetStringFormat()
	{
		StringFormat stringFormat = new StringFormat();
		StringAlignment alignment = StringAlignment.Near;
		StringAlignment lineAlignment = StringAlignment.Near;
		switch (TextAlign)
		{
		case ContentAlignment.TopLeft:
			alignment = StringAlignment.Near;
			lineAlignment = StringAlignment.Near;
			break;
		case ContentAlignment.TopCenter:
			alignment = StringAlignment.Center;
			lineAlignment = StringAlignment.Near;
			break;
		case ContentAlignment.TopRight:
			alignment = StringAlignment.Far;
			lineAlignment = StringAlignment.Near;
			break;
		case ContentAlignment.MiddleLeft:
			alignment = StringAlignment.Near;
			lineAlignment = StringAlignment.Center;
			break;
		case ContentAlignment.MiddleCenter:
			alignment = StringAlignment.Center;
			lineAlignment = StringAlignment.Center;
			break;
		case ContentAlignment.MiddleRight:
			alignment = StringAlignment.Far;
			lineAlignment = StringAlignment.Center;
			break;
		case ContentAlignment.BottomLeft:
			alignment = StringAlignment.Near;
			lineAlignment = StringAlignment.Far;
			break;
		case ContentAlignment.BottomCenter:
			alignment = StringAlignment.Center;
			lineAlignment = StringAlignment.Far;
			break;
		case ContentAlignment.BottomRight:
			alignment = StringAlignment.Far;
			lineAlignment = StringAlignment.Far;
			break;
		}
		stringFormat.Alignment = alignment;
		stringFormat.LineAlignment = lineAlignment;
		return stringFormat;
	}
}
