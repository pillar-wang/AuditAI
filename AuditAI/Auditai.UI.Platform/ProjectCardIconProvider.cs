using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Auditai.UI.Platform;

/// <summary>
/// 项目卡片左上角类型图标提供器：GDI+ 自绘统一风格的扁平位图图标
/// （圆角彩色底 + 白色符号，64px 高清基板，20px/24px 绘制时高清不糊）。
/// 替代旧的 Resources.tile 像素风位图与 IconLibrary 字体字形，随 DPI 生成高分辨率。
/// </summary>
internal static class ProjectCardIconProvider
{
	private static readonly Dictionary<string, Image> _cache = new Dictionary<string, Image>();

	public static Image Project => Get("project", Color.FromArgb(59, 130, 246));

	public static Image SystemTemplate => Get("template", Color.FromArgb(245, 158, 11));

	public static Image VipTemplate => Get("vip", Color.FromArgb(168, 85, 247));

	public static Image CustomTemplate => Get("custom", Color.FromArgb(249, 115, 22));

	private static Image Get(string key, Color bg)
	{
		if (_cache.TryGetValue(key, out Image cached))
		{
			return cached;
		}
		int px = 64;
		Bitmap bmp = new Bitmap(px, px, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		using (Graphics g = Graphics.FromImage(bmp))
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			// 圆角底色块（留 2px 空白边）
			using (GraphicsPath back = Rounded(new Rectangle(2, 2, 60, 60), 16))
			using (SolidBrush backBrush = new SolidBrush(bg))
			{
				g.FillPath(backBrush, back);
			}
			using (SolidBrush white = new SolidBrush(Color.White))
			{
				switch (key)
				{
				case "project":
					DrawFolder(g, white);
					break;
				case "template":
					DrawDocument(g, white, false);
					break;
				case "vip":
					DrawCrown(g, white);
					break;
				default:
					DrawDocument(g, white, true);
					break;
				}
			}
		}
		_cache[key] = bmp;
		return bmp;
	}

	private static void DrawFolder(Graphics g, Brush white)
	{
		// 文件夹剪影：页签 + 主体（白色填充）
		using (GraphicsPath path = new GraphicsPath())
		{
			path.AddArc(12, 16, 8, 8, 180f, 90f);
			path.AddLine(20, 16, 30, 16);
			path.AddLine(30, 16, 34, 22);
			path.AddLine(34, 22, 52, 22);
			path.AddArc(52, 22, 8, 8, 270f, 90f);
			path.AddLine(60, 30, 60, 52);
			path.AddArc(60, 52, 8, 8, 0f, 90f);
			path.AddLine(52, 60, 12, 60);
			path.AddArc(12, 60, 8, 8, 90f, 90f);
			path.AddLine(4, 52, 4, 22);
			path.AddArc(4, 16, 8, 8, 90f, 90f);
			path.CloseFigure();
			g.FillPath(white, path);
		}
	}

	private static void DrawDocument(Graphics g, Brush white, bool pencil)
	{
		// 文档剪影（圆角矩形 + 右上折角）
		using (GraphicsPath path = new GraphicsPath())
		{
			path.AddArc(10, 10, 10, 10, 180f, 90f);
			path.AddLine(20, 10, 40, 10);
			path.AddLine(40, 10, 54, 24);
			path.AddLine(54, 24, 54, 54);
			path.AddArc(54, 54, 10, 10, 0f, 90f);
			path.AddLine(44, 64, 20, 64);
			path.AddArc(10, 54, 10, 10, 90f, 90f);
			path.CloseFigure();
			g.FillPath(white, path);
		}
		if (pencil)
		{
			// 自定义模板：文档右下叠加一支铅笔（与底色同色表示"可编辑"，以白色描虚线替代）
			using (Pen pen = new Pen(Color.FromArgb(255, 249, 115, 22), 3f))
			{
				pen.StartCap = LineCap.Round;
				pen.EndCap = LineCap.Round;
				g.DrawLine(pen, 20, 54, 46, 28);
			}
		}
	}

	private static void DrawCrown(Graphics g, Brush white)
	{
		// 皇冠剪影：锯齿顶 + 宽底
		using (GraphicsPath path = new GraphicsPath())
		{
			path.AddLine(12, 52, 6, 18);
			path.AddLine(6, 18, 22, 32);
			path.AddLine(22, 32, 32, 14);
			path.AddLine(32, 14, 42, 32);
			path.AddLine(42, 32, 58, 18);
			path.AddLine(58, 18, 52, 52);
			path.CloseFigure();
			g.FillPath(white, path);
		}
		// 底缘 3 个圆珠
		using (SolidBrush white2 = new SolidBrush(Color.White))
		{
			g.FillEllipse(white2, 16, 48, 7, 7);
			g.FillEllipse(white2, 28, 44, 8, 8);
			g.FillEllipse(white2, 42, 48, 7, 7);
		}
	}

	private static GraphicsPath Rounded(Rectangle rect, int radius)
	{
		GraphicsPath path = new GraphicsPath();
		int d = radius * 2;
		path.AddArc(rect.X, rect.Y, d, d, 180f, 90f);
		path.AddArc(rect.Right - d, rect.Y, d, d, 270f, 90f);
		path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0f, 90f);
		path.AddArc(rect.X, rect.Bottom - d, d, d, 90f, 90f);
		path.CloseFigure();
		return path;
	}
}