using System;
using System.Drawing;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class ProgressBarEx : ProgressBar
{
	private readonly SolidBrush _brush;

	public ProgressBarEx()
	{
		_brush = new SolidBrush(Color.Green);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.DoubleBuffer, value: true);
	}

	protected override void OnForeColorChanged(EventArgs e)
	{
		if (_brush != null)
		{
			_brush.Color = ForeColor;
		}
		base.OnForeColorChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle clipRectangle = e.ClipRectangle;
		clipRectangle.Width = (int)((double)clipRectangle.Width * ((double)base.Value / (double)base.Maximum)) - 4;
		if (ProgressBarRenderer.IsSupported)
		{
			ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
		}
		clipRectangle.Height -= 4;
		e.Graphics.FillRectangle(_brush, 2, 2, clipRectangle.Width, clipRectangle.Height);
	}
}
