﻿using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;

namespace Auditai.UI.Controls;

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
		// 自绘文字统一走 TextRenderer（GDI 通道），保证微软雅黑 hinting 正常、笔画不偏细
		Rectangle drawArea = Rectangle.Round(GetDrawArea());
		TextRenderer.DrawText(e.Graphics, Text, Font, drawArea, TextColor, GetTextFormatFlags());
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

	private TextFormatFlags GetTextFormatFlags()
	{
		TextFormatFlags flags = TextFormatFlags.Default;
		switch (TextAlign)
		{
		case ContentAlignment.TopLeft:
		case ContentAlignment.MiddleLeft:
		case ContentAlignment.BottomLeft:
			break;
		case ContentAlignment.TopCenter:
		case ContentAlignment.MiddleCenter:
		case ContentAlignment.BottomCenter:
			flags |= TextFormatFlags.HorizontalCenter;
			break;
		case ContentAlignment.TopRight:
		case ContentAlignment.MiddleRight:
		case ContentAlignment.BottomRight:
			flags |= TextFormatFlags.Right;
			break;
		}
		switch (TextAlign)
		{
		case ContentAlignment.MiddleLeft:
		case ContentAlignment.MiddleCenter:
		case ContentAlignment.MiddleRight:
			flags |= TextFormatFlags.VerticalCenter;
			break;
		case ContentAlignment.BottomLeft:
		case ContentAlignment.BottomCenter:
		case ContentAlignment.BottomRight:
			flags |= TextFormatFlags.Bottom;
			break;
		}
		return flags;
	}
}
