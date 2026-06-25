using System;
using System.Drawing;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class WinformProgressBarEx2 : ProgressBar
{
	private SolidBrush _chunkPaintBrush;

	protected override void OnForeColorChanged(EventArgs e)
	{
		if (_chunkPaintBrush != null)
		{
			_chunkPaintBrush.Color = ForeColor;
		}
	}

	public WinformProgressBarEx2()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (_chunkPaintBrush != null)
		{
			_chunkPaintBrush.Dispose();
			_chunkPaintBrush = null;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (_chunkPaintBrush == null)
		{
			_chunkPaintBrush = new SolidBrush(ForeColor);
		}
		int num = (int)((float)base.Value * 1f / (float)base.Maximum * (float)base.Width);
		Rectangle rect = new Rectangle(0, 0, num, base.Height);
		e.Graphics.Clear(BackColor);
		e.Graphics.FillRectangle(_chunkPaintBrush, rect);
	}
}
