using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

/// <summary>
/// 内置绘制的经典文件类型图标（纸张 + 右上折角 + 类型色标），随软件打包。
/// 不依赖系统文件关联（SHGetFileInfo），各机器外观一致；
/// 提取式方案在无关联（如未装 PDF 阅读器）时会退化为通用图标，故弃用。
/// 自绘文字走 TextRenderer（GDI 通道）保证雅黑渲染一致。
/// 统一在 32px 母版上绘制，小尺寸（如树节点 16px）走高质量缩放并缓存。
/// </summary>
public static class FileIcons
{
	private const int Size = 32;
	private const int Fold = 8;

	// 纸张描边/折角灰、badge 字体按尺寸固定，进程内复用
	private static readonly Color PaperBorder = Color.FromArgb(166, 178, 192);
	private static readonly Color PaperFold = Color.FromArgb(226, 233, 240);
	private static readonly Font LetterFont = new Font("微软雅黑", 7f, FontStyle.Bold);

	// kind -> 32px 母版；缓存键 kind:size -> 缩放结果
	private static readonly ConcurrentDictionary<string, Bitmap> Cache = new();

	/// <summary>表格节点图标（Excel 经典绿）。</summary>
	public static Bitmap GetTable(int size = Size) => Resolve("table", size, () => CreateLabeled("X", Color.FromArgb(33, 115, 70)));

	/// <summary>Word 文档节点图标（Word 经典蓝）。</summary>
	public static Bitmap GetWord(int size = Size) => Resolve("word", size, () => CreateLabeled("W", Color.FromArgb(43, 87, 154)));

	/// <summary>Excel 节点图标（Excel 经典绿，与 Word 同款纸张）。</summary>
	public static Bitmap GetExcel(int size = Size) => Resolve("excel", size, () => CreateLabeled("X", Color.FromArgb(33, 115, 70)));

	/// <summary>PDF 节点图标（PDF 经典红）。</summary>
	public static Bitmap GetPdf(int size = Size) => Resolve("pdf", size, () => CreateLabeled("P", Color.FromArgb(192, 57, 43)));

	/// <summary>图片节点图标（照片经典样式：青山 + 太阳）。</summary>
	public static Bitmap GetImage(int size = Size) => Resolve("image", size, CreatePicture);

	/// <summary>文件夹节点图标（经典文件夹样式，蓝白主题：天蓝后片 + 亮蓝前片）。</summary>
	public static Bitmap GetFolder(int size = Size) => Resolve("folder", size, CreateFolder);

	private static Bitmap Resolve(string kind, int size, Func<Bitmap> masterFactory)
	{
		if (size <= 0) size = Size;
		var master = Cache.GetOrAdd(kind + ":" + Size, _ => masterFactory());
		if (size == Size) return master;
		return Cache.GetOrAdd(kind + ":" + size, _ => ScaleDown(master, size));
	}

	/// <summary>32px 母版高质量缩小（小尺寸走双三次插值，边缘清晰不糊）。</summary>
	private static Bitmap ScaleDown(Bitmap master, int size)
	{
		var bmp = new Bitmap(size, size);
		using (var g = Graphics.FromImage(bmp))
		{
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.DrawImage(master, new Rectangle(0, 0, size, size),
				new Rectangle(0, 0, Size, Size), GraphicsUnit.Pixel);
		}
		return bmp;
	}

	private static Bitmap CreateFolder()
	{
		var bmp = new Bitmap(Size, Size);
		using (var g = Graphics.FromImage(bmp))
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			// 后片（含左上标签页）
			var back = new[]
			{
				new Point(3, 7),
				new Point(12, 7),
				new Point(14, 10),
				new Point(28, 10),
				new Point(28, 26),
				new Point(3, 26),
			};
			using (var b = new SolidBrush(Color.FromArgb(2, 132, 199)))      // sky-600
				g.FillPolygon(b, back);
			// 前片（略亮，经典双层观感）
			using (var path = RoundedRect(new Rectangle(3, 12, 25, 14), 2))
			using (var b = new SolidBrush(Color.FromArgb(56, 189, 248)))     // sky-400
				g.FillPath(b, path);
			// 顶部高光线，增加立体感
			using (var p = new Pen(Color.FromArgb(125, 211, 252)))           // sky-300
				g.DrawLine(p, 4, 13, 27, 13);
		}
		return bmp;
	}

	private static Bitmap CreateLabeled(string letter, Color accent)
	{
		var bmp = new Bitmap(Size, Size);
		using (var g = Graphics.FromImage(bmp))
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			DrawPaper(g);
			// 左下角类型色标 + 白色首字母
			var badge = new Rectangle(6, 16, 11, 11);
			using (var b = new SolidBrush(accent))
			using (var path = RoundedRect(badge, 2))
			{
				g.FillPath(b, path);
			}
			TextRenderer.DrawText(g, letter, LetterFont, badge, Color.White,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		return bmp;
	}

	private static Bitmap CreatePicture()
	{
		var bmp = new Bitmap(Size, Size);
		using (var g = Graphics.FromImage(bmp))
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			DrawPaper(g);
			// 取景框：浅蓝底
			var frame = new Rectangle(6, 6, 19, 16);
			using (var b = new SolidBrush(Color.FromArgb(214, 235, 248)))
				g.FillRectangle(b, frame);
			using (var p = new Pen(PaperBorder))
				g.DrawRectangle(p, frame);
			// 太阳
			using (var b = new SolidBrush(Color.FromArgb(247, 181, 56)))
				g.FillEllipse(b, frame.Right - 9, frame.Top + 3, 5, 5);
			// 两座青山
			using (var b = new SolidBrush(Color.FromArgb(15, 118, 110)))
			{
				g.FillPolygon(b, new[] { new Point(7, 21), new Point(13, 11), new Point(19, 21) });
				g.FillPolygon(b, new[] { new Point(15, 21), new Point(20, 14), new Point(25, 21) });
			}
		}
		return bmp;
	}

	/// <summary>白色纸张 + 右上折角 + 灰描边。</summary>
	private static void DrawPaper(Graphics g)
	{
		var body = new[]
		{
			new Point(3, 2),
			new Point(Size - 4 - Fold, 2),
			new Point(Size - 4, 2 + Fold),
			new Point(Size - 4, Size - 3),
			new Point(3, Size - 3),
		};
		using (var path = new GraphicsPath())
		{
			path.AddPolygon(body);
			using (var b = new SolidBrush(Color.White))
				g.FillPath(b, path);
			using (var p = new Pen(PaperBorder))
				g.DrawPath(p, path);
		}
		var fold = new[]
		{
			new Point(Size - 4 - Fold, 2),
			new Point(Size - 4, 2 + Fold),
			new Point(Size - 4 - Fold, 2 + Fold),
		};
		using (var b = new SolidBrush(PaperFold))
			g.FillPolygon(b, fold);
		using (var p = new Pen(PaperBorder))
			g.DrawPolygon(p, fold);
	}

	private static GraphicsPath RoundedRect(Rectangle r, int radius)
	{
		var path = new GraphicsPath();
		int d = radius * 2;
		path.AddArc(r.X, r.Y, d, d, 180, 90);
		path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
		path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
		path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
		path.CloseFigure();
		return path;
	}
}
