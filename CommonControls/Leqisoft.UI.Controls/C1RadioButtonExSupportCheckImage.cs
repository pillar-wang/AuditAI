using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Leqisoft.UI.Controls;

public class C1RadioButtonExSupportCheckImage : RadioButton
{
	public delegate void PaintEventHandle(object sender, PaintEventArgs e);

	private bool _isMouseOver;

	private bool _isPressed;

	public Image CheckedImage { get; set; }

	public Image UnCheckedImage { get; set; }

	public Color CheckedBackgroundColor { get; set; } = Color.Transparent;


	public Color CheckedForeColor { get; set; } = Color.Black;


	public Color CheckedBorderColor { get; set; } = Color.Transparent;


	public int CheckedBorderWidth { get; set; }

	public Color HotBackgroundColor { get; set; } = Color.Transparent;


	public Color HotForeColor { get; set; } = Color.Black;


	public Color HotBorderColor { get; set; } = Color.Transparent;


	public int HotBorderWidth { get; set; }

	public Color PressedBackgroundColor { get; set; } = Color.Transparent;


	public Color PressedBorderColor { get; set; } = Color.Transparent;


	public Color PressedForeColor { get; set; } = Color.Black;


	public int PressedBorderWidth { get; set; }

	public Size CheckImageSize { get; set; } = new Size(100, 100);


	public int TextImageDistance { get; set; }

	public bool TextOnImageLeft { get; set; } = true;


	public int CheckedImageYOffset { get; set; }

	public int UnCheckedImageYOffset { get; set; }

	public int CornerRadius { get; set; }

	public PaintEventHandle BackgroundPaintHandle { get; set; }

	public C1RadioButtonExSupportCheckImage()
	{
		DoubleBuffered = true;
		base.MouseEnter += C1RadioButtonExSupportCheckImage_MouseEnter;
		base.MouseLeave += C1RadioButtonExSupportCheckImage_MouseLeave;
		base.MouseDown += C1RadioButtonExSupportCheckImage_MouseDown;
		base.MouseUp += C1RadioButtonExSupportCheckImage_MouseUp;
	}

	private void C1RadioButtonExSupportCheckImage_MouseUp(object sender, MouseEventArgs e)
	{
		_isPressed = false;
		Invalidate();
	}

	private void C1RadioButtonExSupportCheckImage_MouseDown(object sender, MouseEventArgs e)
	{
		_isPressed = true;
		Invalidate();
	}

	private void C1RadioButtonExSupportCheckImage_MouseLeave(object sender, EventArgs e)
	{
		_isPressed = false;
		_isMouseOver = false;
		Invalidate();
	}

	private void C1RadioButtonExSupportCheckImage_MouseEnter(object sender, EventArgs e)
	{
		_isMouseOver = true;
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		Graphics graphics = pevent.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(Color.White);
		PaintBackground(pevent);
		PaintTextAndImage(pevent);
		PaintBorder(pevent);
	}

	private void PaintBackground(PaintEventArgs pevent)
	{
		if (BackgroundPaintHandle == null)
		{
			OnPaintBackground(pevent);
		}
		else
		{
			BackgroundPaintHandle(this, pevent);
		}
		if (_isPressed && PressedBackgroundColor.A > 0)
		{
			PaintPressedBackground(pevent);
		}
		else if (_isMouseOver && HotBackgroundColor.A > 0)
		{
			PaintHotBackground(pevent);
		}
		else if (base.Checked && CheckedBackgroundColor.A > 0)
		{
			PaintCheckedBackground(pevent);
		}
	}

	private Color GetTextColor()
	{
		if (_isPressed)
		{
			return PressedForeColor;
		}
		if (_isMouseOver)
		{
			return HotForeColor;
		}
		if (base.Checked)
		{
			return CheckedForeColor;
		}
		return ForeColor;
	}

	private void PaintTextAndImage(PaintEventArgs pevent)
	{
		if (TextOnImageLeft)
		{
			int left = base.Margin.Left;
			if (Text != null && Text != "")
			{
				using SolidBrush brush = new SolidBrush(GetTextColor());
				Rectangle rectangle = new Rectangle(left, 0, TextImageDistance, base.Size.Height);
				StringFormat stringFormat = new StringFormat();
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Center;
				pevent.Graphics.DrawString(Text, Font, brush, rectangle, stringFormat);
			}
			left += TextImageDistance;
			int num = 0;
			Image image = null;
			if (base.Checked)
			{
				image = CheckedImage;
				num = CheckedImageYOffset;
			}
			else
			{
				image = UnCheckedImage;
				num = UnCheckedImageYOffset;
			}
			if (image != null)
			{
				int num2 = (base.Size.Height - CheckImageSize.Height) / 2 + num;
				Rectangle rect = new Rectangle(left, num2, CheckImageSize.Width, CheckImageSize.Height);
				pevent.Graphics.DrawImage(image, rect);
			}
			return;
		}
		int left2 = base.Margin.Left;
		int num3 = 0;
		Image image2 = null;
		if (base.Checked)
		{
			image2 = CheckedImage;
			num3 = CheckedImageYOffset;
		}
		else
		{
			image2 = UnCheckedImage;
			num3 = UnCheckedImageYOffset;
		}
		if (image2 != null)
		{
			int num4 = (base.Size.Height - CheckImageSize.Height) / 2 + num3;
			Rectangle rect2 = new Rectangle(left2, num4, CheckImageSize.Width, CheckImageSize.Height);
			pevent.Graphics.DrawImage(image2, rect2);
		}
		left2 += TextImageDistance;
		if (Text == null || !(Text != ""))
		{
			return;
		}
		using SolidBrush brush2 = new SolidBrush(GetTextColor());
		Rectangle rectangle2 = new Rectangle(left2, 0, base.Size.Width - TextImageDistance, base.Size.Height);
		StringFormat stringFormat2 = new StringFormat();
		stringFormat2.Alignment = StringAlignment.Near;
		stringFormat2.LineAlignment = StringAlignment.Center;
		pevent.Graphics.DrawString(Text, Font, brush2, rectangle2, stringFormat2);
	}

	private void PaintBorder(PaintEventArgs pevent)
	{
		if (_isPressed && PressedBorderWidth > 0)
		{
			PaintPressedBorder(pevent);
		}
		else if (_isMouseOver && HotBorderWidth > 0)
		{
			PaintHotBorder(pevent);
		}
		else if (base.Checked && CheckedBorderWidth > 0)
		{
			PaintCheckedBorder(pevent);
		}
	}

	private void PaintPressedBackground(PaintEventArgs pevent)
	{
		using SolidBrush brush = new SolidBrush(PressedBackgroundColor);
		Rectangle clipRectangle = pevent.ClipRectangle;
		clipRectangle.X--;
		clipRectangle.Y--;
		clipRectangle.Width++;
		FillRectangle(brush, clipRectangle, pevent);
	}

	private void PaintHotBackground(PaintEventArgs pevent)
	{
		using SolidBrush brush = new SolidBrush(HotBackgroundColor);
		Rectangle clipRectangle = pevent.ClipRectangle;
		clipRectangle.X--;
		clipRectangle.Y--;
		clipRectangle.Width++;
		FillRectangle(brush, clipRectangle, pevent);
	}

	private void PaintCheckedBackground(PaintEventArgs pevent)
	{
		using SolidBrush brush = new SolidBrush(CheckedBackgroundColor);
		Rectangle clipRectangle = pevent.ClipRectangle;
		clipRectangle.X--;
		clipRectangle.Y--;
		clipRectangle.Width++;
		FillRectangle(brush, clipRectangle, pevent);
	}

	private void PaintCheckedBorder(PaintEventArgs pevent)
	{
		using Pen pen = new Pen(CheckedBorderColor, CheckedBorderWidth);
		DrawRectangle(pen, base.ClientRectangle, pevent);
	}

	private void PaintHotBorder(PaintEventArgs pevent)
	{
		using Pen pen = new Pen(HotBorderColor, HotBorderWidth);
		DrawRectangle(pen, base.ClientRectangle, pevent);
	}

	private void PaintPressedBorder(PaintEventArgs pevent)
	{
		using Pen pen = new Pen(PressedBorderColor, PressedBorderWidth);
		DrawRectangle(pen, base.ClientRectangle, pevent);
	}

	private void DrawRectangle(Pen pen, Rectangle rect, PaintEventArgs pevent)
	{
		if (CornerRadius > 0)
		{
			GraphicsPath roundedRectangle = GetRoundedRectangle(rect, CornerRadius);
			pevent.Graphics.DrawPath(pen, roundedRectangle);
		}
		else
		{
			pevent.Graphics.DrawRectangle(pen, rect);
		}
	}

	private void FillRectangle(Brush brush, Rectangle rect, PaintEventArgs pevent)
	{
		if (CornerRadius > 0)
		{
			GraphicsPath roundedRectangle = GetRoundedRectangle(rect, CornerRadius);
			pevent.Graphics.FillPath(brush, roundedRectangle);
		}
		else
		{
			pevent.Graphics.FillRectangle(brush, rect);
		}
	}

	public static GraphicsPath GetRoundedRectangle(Rectangle rectangle, int cornerRadius)
	{
		int num = cornerRadius * 2;
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddArc(rectangle.X, rectangle.Y, num, num, 180f, 90f);
		graphicsPath.AddLine(rectangle.X + cornerRadius, rectangle.Y, rectangle.Right - num, rectangle.Y);
		graphicsPath.AddArc(rectangle.X + rectangle.Width - num, rectangle.Y, num, num, 270f, 90f);
		graphicsPath.AddLine(rectangle.Right, rectangle.Y + num, rectangle.Right, rectangle.Y + rectangle.Height - num);
		graphicsPath.AddArc(rectangle.X + rectangle.Width - num, rectangle.Y + rectangle.Height - num, num, num, 0f, 90f);
		graphicsPath.AddLine(rectangle.Right - num, rectangle.Bottom, rectangle.X + num, rectangle.Bottom);
		graphicsPath.AddArc(rectangle.X, rectangle.Bottom - num, num, num, 90f, 90f);
		graphicsPath.AddLine(rectangle.X, rectangle.Bottom - num, rectangle.X, rectangle.Y + num);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}
}
