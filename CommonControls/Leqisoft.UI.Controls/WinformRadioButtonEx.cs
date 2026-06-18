using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Leqisoft.UI.Controls;

public class WinformRadioButtonEx : RadioButton
{
	protected bool _isMouseOver;

	public Color RadioCircleBackgroundColor { get; set; } = Color.White;


	public Color RadioCircleMouseOverColor { get; set; } = Color.Black;


	public Color RadioCircleCheckedColor { get; set; } = Color.Black;


	public Color RadioCircleNormalColor { get; set; } = Color.Black;


	public int RadioCircleSize { get; set; } = 12;


	public int RadioCheckedCircleOffset { get; set; } = 2;


	public int SpaceBetweenRadiocCirleAndText { get; set; } = 3;


	public WinformRadioButtonEx()
	{
		base.MouseHover += WinformRadioButtonEx_MouseHover;
		base.MouseLeave += WinformRadioButtonEx_MouseLeave;
	}

	private void WinformRadioButtonEx_MouseLeave(object sender, EventArgs e)
	{
		_isMouseOver = false;
		Invalidate();
	}

	private void WinformRadioButtonEx_MouseHover(object sender, EventArgs e)
	{
		_isMouseOver = true;
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		Graphics graphics = pevent.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		int num = (base.Size.Height - RadioCircleSize) / 2 - 1;
		Rectangle rect = new Rectangle(0, num, RadioCircleSize, RadioCircleSize);
		graphics.Clear(RadioCircleBackgroundColor);
		if (base.Checked)
		{
			using (SolidBrush brush = new SolidBrush(_isMouseOver ? RadioCircleMouseOverColor : RadioCircleCheckedColor))
			{
				int num2 = -RadioCheckedCircleOffset;
				rect.Inflate(num2, num2);
				graphics.FillEllipse(brush, rect);
				rect.Inflate(-num2, -num2);
			}
			using Pen pen = new Pen(_isMouseOver ? RadioCircleMouseOverColor : RadioCircleCheckedColor);
			graphics.DrawEllipse(pen, rect);
		}
		else
		{
			using Pen pen2 = new Pen(_isMouseOver ? RadioCircleMouseOverColor : RadioCircleNormalColor);
			graphics.DrawEllipse(pen2, rect);
		}
		using SolidBrush brush2 = new SolidBrush(ForeColor);
		int num3 = base.Size.Width - RadioCircleSize;
		Rectangle rectangle = new Rectangle(RadioCircleSize + SpaceBetweenRadiocCirleAndText, 0, num3, base.Size.Height);
		StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Near;
		stringFormat.LineAlignment = StringAlignment.Center;
		graphics.DrawString(Text, Font, brush2, rectangle, stringFormat);
	}
}
