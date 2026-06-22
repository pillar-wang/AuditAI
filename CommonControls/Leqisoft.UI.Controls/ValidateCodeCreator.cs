﻿﻿﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

#pragma warning disable SCS0005 // 验证码图片绘制，视觉随机非安全场景
namespace Leqisoft.UI.Controls;

public class ValidateCodeCreator
{
	protected Color _bgColor = Color.Black;

	public int Length { get; set; } = 4;


	public void SetBackgroundColor(Color color)
	{
		_bgColor = color;
	}

	public Bitmap Create(out string text, int width, int height, int fontSize = 15)
	{
		text = GetRandomStr();
		Pen pen = new Pen(Color.Black);
		Font fontStyle = new Font("Buxton Sketch", fontSize, FontStyle.Bold);
		Bitmap bitmap = new Bitmap(width, height);
		Graphics graphic = Graphics.FromImage(bitmap);
		try
		{
			SizeF sizeF = graphic.MeasureString(text[0].ToString().ToUpper(), fontStyle);
			float num = text.Sum((char t) => graphic.MeasureString(t.ToString().ToUpper(), fontStyle).Width);
			PointF point = new PointF(((float)width - num) / 2f, ((float)height - sizeF.Height) / 2f);
			Random random = new Random();
			graphic.Clear(Color.White);
			random = new Random();
			for (int i = 0; i < 25; i++)
			{
				int x = random.Next(width);
				int x2 = random.Next(width);
				int y = random.Next(height);
				int y2 = random.Next(height);
				graphic.DrawLine(new Pen(GetRandomLightColor(), 2f), x, y, x2, y2);
			}
			graphic.DrawLine(new Pen(Color.Silver, 3f), 0f, point.Y + sizeF.Height / 2f, width, point.Y + sizeF.Height / 2f);
			for (int j = 0; j < 500; j++)
			{
				bitmap.SetPixel(random.Next(0, width), random.Next(0, height), GetRandomColor());
			}
			for (int k = 0; k < 150; k++)
			{
				graphic.DrawRectangle(new Pen(Color.AliceBlue), random.Next(width), random.Next(height), 1, 1);
			}
			string text2 = text;
			for (int l = 0; l < text2.Length; l++)
			{
				char c = text2[l];
				random = new Random(Guid.NewGuid().GetHashCode());
				int num2 = random.Next(-5, 5);
				graphic.RotateTransform(num2);
				Color color = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
				Color color2 = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
				Brush brush = new LinearGradientBrush(new Point(0, 0), new Point(1, 1), GetRandomDeepColor(), GetRandomDeepColor());
				graphic.DrawString(c.ToString().ToUpper(), fontStyle, brush, point);
				SizeF sizeF2 = graphic.MeasureString(c.ToString().ToUpper(), fontStyle);
				point.X += sizeF2.Width;
				graphic.RotateTransform(-num2);
			}
			return bitmap;
		}
		finally
		{
			if (graphic != null)
			{
				((IDisposable)graphic).Dispose();
			}
		}
		static Color GetRandomColor()
		{
			Random random3 = new Random(Guid.NewGuid().GetHashCode());
			return Color.FromArgb(random3.Next(0, 255), random3.Next(0, 255), random3.Next(0, 255));
		}
		static Color GetRandomDeepColor()
		{
			Random random2 = new Random(Guid.NewGuid().GetHashCode());
			int maxValue = 160;
			int maxValue2 = 100;
			int maxValue3 = 160;
			int red = random2.Next(maxValue);
			int green = random2.Next(maxValue2);
			int blue = random2.Next(maxValue3);
			return Color.FromArgb(red, green, blue);
		}
		static Color GetRandomLightColor()
		{
			Random random4 = new Random(Guid.NewGuid().GetHashCode());
			int num3 = 180;
			int num4 = 255;
			int red2 = random4.Next(num4) % (num4 - num3) + num3;
			int green2 = random4.Next(num4) % (num4 - num3) + num3;
			int blue2 = random4.Next(num4) % (num4 - num3) + num3;
			return Color.FromArgb(red2, green2, blue2);
		}
		string GetRandomStr()
		{
			Random random5 = new Random();
			string text3 = string.Empty;
			for (int m = 0; m < Length; m++)
			{
				text3 += (char)random5.Next(97, 122);
			}
			return text3;
		}
	}
}
