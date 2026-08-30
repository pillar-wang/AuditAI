using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class ProgressBarEx : ProgressBar
{
	private readonly SolidBrush _brush;

	/// <summary>轨道底色（浅灰蓝，与 MessageShowBox 提示框边框色系一致）</summary>
	private Color _trackColor = Color.FromArgb(222, 231, 240);

	public Color TrackColor
	{
		get { return _trackColor; }
		set { _trackColor = value; }
	}

	public ProgressBarEx()
	{
		_brush = new SolidBrush(Color.Green);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.ResizeRedraw, value: true);
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
		var g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;

		int h = Height;
		int radius = h / 2;

		// 1) 圆角胶囊轨道
		using (var trackPath = CreateRoundedPath(0, 0, Width, h, radius))
		using (var trackBrush = new SolidBrush(_trackColor))
		{
			g.FillPath(trackBrush, trackPath);
		}

		// 2) 圆角进度填充（低百分比时保证至少一个圆头可见）
		int fillW = (int)((double)Width * ((double)Value / (double)Maximum));
		if (Value > 0 && fillW < h)
		{
			fillW = h;
		}
		if (fillW > Width)
		{
			fillW = Width;
		}
		if (fillW > 0)
		{
			int fillRadius = Math.Min(radius, fillW / 2);
			using (var fillPath = CreateRoundedPath(0, 0, fillW, h, fillRadius))
			using (var fillBrush = new SolidBrush(ForeColor))
			{
				g.FillPath(fillBrush, fillPath);
			}
		}
	}

	private static GraphicsPath CreateRoundedPath(int x, int y, int w, int h, int r)
	{
		var path = new GraphicsPath();
		if (r < 1)
		{
			path.AddRectangle(new Rectangle(x, y, w, h));
			return path;
		}
		path.AddArc(x, y, r * 2, r * 2, 180f, 90f);
		path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270f, 90f);
		path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0f, 90f);
		path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90f, 90f);
		path.CloseFigure();
		return path;
	}
}
