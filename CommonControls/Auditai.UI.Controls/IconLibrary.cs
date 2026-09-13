using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Auditai.UI.Controls;

/// <summary>
/// 统一字体图标管理器（单例）：
/// 从程序集内嵌的 Phosphor Icons Fill 实心 TTF 加载私有字体，提供字形绘制
/// （矢量，任意尺寸清晰）与位图生成（按 名称/尺寸/颜色/样式 四元组缓存）。
/// bold/duotone/fill 三种 style 值统一走单层 Fill 实心渲染（历史调用方 style 值兼容保留，渲染结果相同）。
/// 加载失败时回退到系统 Segoe MDL2 Assets（码点不保证一致，仅避免崩溃）。
/// 图标语义名→码点映射见 <see cref="IconNames"/>。
/// 图标字体：Phosphor Icons Fill 实心风格 (MIT License, https://phosphoricons.com)
/// </summary>
public static class IconLibrary
{
	/// <summary>fill 样式（Phosphor Fill 实心单色，当前唯一渲染风格）。</summary>
	public const string StyleFill = "fill";

	/// <summary>bold 样式（历史值，Phosphor 各字重码点一致，现统一渲染为 Fill 实心）。</summary>
	public const string StyleBold = "bold";

	/// <summary>duotone 样式（历史值，现统一渲染为 Fill 实心单层，不再双层绘制）。</summary>
	public const string StyleDuotone = "duotone";

	// ============================================================
	//  标准图标尺寸阶梯（4px 网格对齐）
	// ============================================================
	/// <summary>12px — 超小图标（状态指示、角标）</summary>
	public const int SizeTiny = 12;
	/// <summary>14px — 小图标（菜单项、侧边栏）</summary>
	public const int SizeSmall = 14;
	/// <summary>16px — 标准图标（按钮内、表格操作）</summary>
	public const int SizeDefault = 16;
	/// <summary>20px — 中等图标（功能区小图标）</summary>
	public const int SizeMedium = 20;
	/// <summary>24px — 大图标（功能区大图标、空状态）</summary>
	public const int SizeLarge = 24;
	/// <summary>32px — 超大图标（对话框、向导）</summary>
	public const int SizeExtraLarge = 32;

	private const string FillResourceSuffix = "Phosphor-Fill.ttf";

	private static readonly object _lock = new object();

	private static FontFamily _fillFamily;

	// GDI+ AddMemoryFont 要求内存在字体集合存活期间保持有效：
	// 字节缓冲永久固定（Pinned GCHandle），字体集合与字节缓冲均保存在静态字段中不释放，
	// 否则 FontFamily 可能失效。沿用原有防失效写法。
	private static byte[] _fillBytes;
	private static PrivateFontCollection _fillCollection;
	private static GCHandle _fillPin;

	private static bool _loadAttempted;

	private static readonly Dictionary<string, Font> _fontCache = new Dictionary<string, Font>();   // key: 样式|像素尺寸

	private static readonly Dictionary<string, Bitmap> _bitmapCache = new Dictionary<string, Bitmap>();

	/// <summary>图标默认颜色（与界面辅助文字色一致）。</summary>
	public static Color DefaultColor { get; set; } = Color.FromArgb(71, 85, 105);

	/// <summary>DPI 缩放系数（如 144dpi→1.5f）。位图按 逻辑尺寸 × 该系数 生成更高分辨率，
	/// 外层 DrawImage 以逻辑/物理尺寸绘制时次采样降样，避免高 DPI（PerMonitorV2）下图标被位图拉伸而模糊。
	/// 由宿主程序启动时依据当前屏幕 DPI 设置。</summary>
	public static float DpiScale { get; set; } = 1f;

	/// <summary>由 IconLibrary 生成的高清位图，返回其 96dpi 基准逻辑宽度。</summary>
	public static int LogicalWidth(Image image) => image == null ? 0 : (int)Math.Round(image.Width / DpiScale);

	/// <summary>由 IconLibrary 生成的高清位图，返回其 96dpi 基准逻辑高度。</summary>
	public static int LogicalHeight(Image image) => image == null ? 0 : (int)Math.Round(image.Height / DpiScale);

	/// <summary>由 IconLibrary 生成的高清位图，返回其 96dpi 基准逻辑尺寸。
	/// 布局代码应使用逻辑尺寸与 LogicalWidth/LogicalHeight（而非 image.Width/.Height），
	/// 绘制用带目标尺寸的 DrawImage(g, img, rect)，避免高 DPI 下位图放大后被原样铺开导致错位。</summary>
	public static Size LogicalSize(Image image) => new Size(LogicalWidth(image), LogicalHeight(image));

	/// <summary>Fill 字体是否成功加载为内嵌 Phosphor 字体（false 表示使用了系统回退字体）。</summary>
	public static bool LoadedFromEmbedded { get; private set; }

	private static FontFamily EnsureFont()
	{
		FontFamily family = _fillFamily;
		if (family != null)
		{
			return family;
		}
		lock (_lock)
		{
			family = _fillFamily;
			if (family != null)
			{
				return family;
			}
			if (!_loadAttempted)
			{
				_loadAttempted = true;
				_fillFamily = TryLoadFamily(FillResourceSuffix, out _fillBytes, out _fillCollection, out _fillPin);
				LoadedFromEmbedded = _fillFamily != null;
			}
			family = _fillFamily;
			if (family == null)
			{
				// 回退：Windows 10/11 内置符号字体（码点不保证一致，仅避免崩溃）
				family = new FontFamily("Segoe MDL2 Assets");
				_fillFamily = family;
			}
			return family;
		}
	}

	/// <summary>按资源名后缀加载内嵌 TTF 为私有字体；任何异常吞掉并返回 null（调用方回退系统字体），不外抛。</summary>
	private static FontFamily TryLoadFamily(string resourceSuffix, out byte[] fontBytes, out PrivateFontCollection collection, out GCHandle pin)
	{
		fontBytes = null;
		collection = null;
		pin = default;
		try
		{
			byte[] bytes = LoadEmbeddedTtf(resourceSuffix);
			if (bytes == null)
			{
				return null;
			}
			GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			pin = handle;
			IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
			PrivateFontCollection fonts = new PrivateFontCollection();
			collection = fonts;
			fonts.AddMemoryFont(ptr, bytes.Length);
			fontBytes = bytes;
			return fonts.Families[0];
		}
		catch
		{
			if (pin.IsAllocated)
			{
				pin.Free();
			}
			if (collection != null)
			{
				try { collection.Dispose(); } catch { }
			}
			fontBytes = null;
			collection = null;
			pin = default;
			return null;
		}
	}

	private static byte[] LoadEmbeddedTtf(string resourceSuffix)
	{
		string[] names = typeof(IconLibrary).Assembly.GetManifestResourceNames();
		foreach (string name in names)
		{
			if (!name.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			using Stream stream = typeof(IconLibrary).Assembly.GetManifestResourceStream(name);
			if (stream == null)
			{
				continue;
			}
			byte[] bytes = new byte[stream.Length];
			int read = 0;
			while (read < bytes.Length)
			{
				int n = stream.Read(bytes, read, bytes.Length - read);
				if (n <= 0)
				{
					break;
				}
				read += n;
			}
			return bytes;
		}
		return null;
	}

	/// <summary>语义名是否映射到了码点（Fill 实心）。</summary>
	public static bool HasGlyph(string semanticName)
	{
		return IconNames.TryResolve(semanticName, out _);
	}

	/// <summary>
	/// 在指定矩形内居中绘制字形（矢量渲染，DPI 高分屏下仍清晰），Fill 实心样式。
	/// 语义名未映射时静默跳过（渲染空白占位，不崩溃）。
	/// </summary>
	public static void DrawGlyph(Graphics g, string semanticName, Rectangle rect, Color color)
	{
		DrawGlyph(g, semanticName, rect, color, StyleFill);
	}

	/// <summary>
	/// 在指定矩形内居中绘制字形（矢量渲染，DPI 高分屏下仍清晰）。
	/// style 兼容 "fill"/"bold"/"duotone"（历史值），三者统一走单层 Fill 实心渲染。
	/// 语义名未映射时静默跳过（渲染空白占位，不崩溃）。
	/// </summary>
	public static void DrawGlyph(Graphics g, string semanticName, Rectangle rect, Color color, string style)
	{
		// "x"（关闭）图标：内嵌 Phosphor-Fill.ttf 中 0xe4f6 码点与映射表不匹配
		// （小字号渲染成实心方块、大字号渲染成竖条），因此关闭图标不依赖字体码点，
		// 直接 GDI+ 自绘叉线，保证任意尺寸/DPI 下都是清晰的 ×。
		if (string.Equals(semanticName, "x", StringComparison.OrdinalIgnoreCase))
		{
			DrawCloseGlyph(g, rect, color);
			return;
		}
		if (!IconNames.TryResolve(semanticName, out int codepoint) || codepoint <= 0)
		{
			return;
		}
		// 注意：字体按尺寸缓存共享，绝不能 using 释放，否则第二次绘制会用到已释放字体而崩溃
		Font font = GetOrCreateFont(style, rect.Height);
		DrawLayer(g, font, codepoint, rect, color);
	}

	/// <summary>自绘关闭叉线（不依赖字体码点）：两条对角圆头线段，内边距约 22%，线宽随尺寸缩放。</summary>
	private static void DrawCloseGlyph(Graphics g, Rectangle rect, Color color)
	{
		if (rect.Width <= 0 || rect.Height <= 0)
		{
			return;
		}
		float size = Math.Min(rect.Width, rect.Height);
		float pad = size * 0.22f;
		float left = rect.Left + pad;
		float top = rect.Top + pad;
		float right = rect.Right - pad;
		float bottom = rect.Bottom - pad;
		float width = Math.Max(1.8f, size * 0.13f);
		using (Pen pen = new Pen(color, width)
		{
			StartCap = LineCap.Round,
			EndCap = LineCap.Round,
			LineJoin = LineJoin.Round
		})
		{
			g.DrawLine(pen, left, top, right, bottom);
			g.DrawLine(pen, right, top, left, bottom);
		}
	}

	/// <summary>单层字形绘制（居中布局 + 抗锯齿）。</summary>
	private static void DrawLayer(Graphics g, Font font, int codepoint, Rectangle rect, Color color)
	{
		TextRenderingHint hint = g.TextRenderingHint;
		// 图标位图/控件多为透明背景：ClearType（子像素）在透明背景上会产生彩色毛边，
		// 小字号图标更会糊成块状。统一改用 AntiAlias 灰度抗锯齿，透明背景下边缘干净锐利。
		g.TextRenderingHint = TextRenderingHint.AntiAlias;
		try
		{
			using SolidBrush brush = new SolidBrush(color);
			using StringFormat format = new StringFormat
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center,
				FormatFlags = StringFormatFlags.NoWrap,
				Trimming = StringTrimming.None
			};
			g.DrawString(char.ConvertFromUtf32(codepoint), font, brush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), format);
		}
		finally
		{
			g.TextRenderingHint = hint;
		}
	}

	/// <summary>生成字形位图（32bppArgb，Fill 实心，按 名称|尺寸|颜色 缓存）。</summary>
	public static Bitmap CreateBitmap(string semanticName, int size, Color color)
	{
		return CreateBitmap(semanticName, size, color, StyleFill);
	}

	/// <summary>生成字形位图（32bppArgb，按 名称|尺寸|颜色|样式|DPI 五元组缓存）。style 兼容 "fill"/"bold"/"duotone"，统一 Fill 实心渲染。
	/// 位图按 DpiScale 放大为高分辨率，供高 DPI 下绘制不模糊。</summary>
	public static Bitmap CreateBitmap(string semanticName, int size, Color color, string style)
	{
		string normalizedStyle = string.IsNullOrEmpty(style) ? StyleFill : style;
		// 位图实际像素尺寸 = 逻辑尺寸 × DPI 缩放，四舍五入且至少 1px；cache key 用像素尺寸以隔离不同 DPI 的位图
		int pixel = Math.Max(1, (int)Math.Round((float)size * DpiScale));
		string key = semanticName + "|" + size + "|p" + pixel + "|" + color.ToArgb() + "|" + normalizedStyle;
		lock (_lock)
		{
			if (_bitmapCache.TryGetValue(key, out Bitmap cached) && !IsDisposed(cached))
			{
				return cached;
			}
			Bitmap bmp = new Bitmap(pixel, pixel, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				DrawGlyph(g, semanticName, new Rectangle(0, 0, pixel, pixel), color, normalizedStyle);
			}
			_bitmapCache[key] = bmp;
			return bmp;
		}
	}

	/// <summary>生成窗体标题图标（Form.Icon，透明背景字形位图转换），Fill 实心。位图留在缓存中，不可释放。</summary>
	public static Icon CreateIcon(string semanticName, int size, Color color)
	{
		return CreateIcon(semanticName, size, color, StyleFill);
	}

	/// <summary>生成窗体标题图标（Form.Icon，透明背景字形位图转换）。style 兼容 "fill"/"bold"/"duotone"。位图留在缓存中，不可释放。</summary>
	public static Icon CreateIcon(string semanticName, int size, Color color, string style)
	{
		Bitmap bmp = CreateBitmap(semanticName, size, color, style);
		return Icon.FromHandle(bmp.GetHicon());
	}

	/// <summary>
	/// 生成"圆角彩色底 + 白色 Phosphor 字形"位图（Office 365 / Material 风格）。
	/// 语义：圆角色块承载白色实心字形，视觉上有"块感"，比透明底单色字形更精致醒目。
	/// 位图按 DPI 缩放并缓存，key = "tiled|name|size|pixel|bg|fg"。
	/// </summary>
	public static Bitmap CreateTiledBitmap(string semanticName, int size, Color backColor)
	{
		return CreateTiledBitmap(semanticName, size, backColor, Color.White);
	}

	/// <summary>生成带指定前景色的圆角彩色底图标（默认前景白）。</summary>
	public static Bitmap CreateTiledBitmap(string semanticName, int size, Color backColor, Color glyphColor)
	{
		int pixel = Math.Max(1, (int)Math.Round((float)size * DpiScale));
		// 圆角半径占 14%：够圆润，但安全区大，字形能撑满
		int radius = Math.Max(2, (int)Math.Round(pixel * 0.14f));
		// 字形最大化到安全区上限（pixel - 2*radius - 2px padding），饱满且绝不裁切
		int glyphPixel = Math.Max(4, pixel - 2 * radius - 2);

		string key = "tiled|" + semanticName + "|" + size + "|p" + pixel + "|" + backColor.ToArgb() + "|" + glyphColor.ToArgb();
		lock (_lock)
		{
			if (_bitmapCache.TryGetValue(key, out Bitmap cached) && !IsDisposed(cached))
			{
				return cached;
			}
			Bitmap bmp = new Bitmap(pixel, pixel, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				// 1) 画圆角矩形底
				var rect = new Rectangle(0, 0, pixel - 1, pixel - 1);
				using (GraphicsPath path = new GraphicsPath())
				{
					path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
				AddRoundedRect(path, rect, radius);
					using (SolidBrush brush = new SolidBrush(backColor))
						g.FillPath(brush, path);
				}
				// 2) 白色 Phosphor 字形居中
				int offset = (pixel - glyphPixel) / 2;
				DrawGlyph(g, semanticName, new Rectangle(offset, offset, glyphPixel, glyphPixel), glyphColor, StyleFill);
			}
			_bitmapCache[key] = bmp;
			return bmp;
		}
	}

	/// <summary>向 GraphicsPath 添加一个圆角矩形（四角半径统一）。</summary>
	private static void AddRoundedRect(GraphicsPath path, Rectangle r, int radius)
	{
		int d = radius * 2;
		path.AddArc(r.X, r.Y, d, d, 180, 90);
		path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
		path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
		path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
		path.CloseFigure();
	}

	private static bool IsDisposed(Bitmap bmp)
	{
		try
		{
			return bmp.Width <= 0;
		}
		catch
		{
			return true;
		}
	}

	private static Font GetOrCreateFont(string style, int pixelSize)
	{
		int size = Math.Max(4, pixelSize);
		string key = style + "|" + size;
		lock (_lock)
		{
			if (_fontCache.TryGetValue(key, out Font font))
			{
				return font;
			}
			// 按 1.0 倍字号渲染以获得与原位图相当的视觉重量（Phosphor 字形自带少量内边距）
			font = new Font(EnsureFont(), size, GraphicsUnit.Pixel);
			_fontCache[key] = font;
			return font;
		}
	}

	/// <summary>
	/// 清空位图缓存（主题切换等场景下调用，后续生成按新颜色重建）。
	/// 注意：只解除缓存引用、不调用 Dispose——旧位图可能仍被窗体/命令按钮引用，
	/// 释放会导致其重绘时抛出参数异常；未再引用的位图交给 GC 回收。
	/// </summary>
	public static void ClearCache()
	{
		lock (_lock)
		{
			_bitmapCache.Clear();
		}
	}
}

/// <summary>语义图标名→Phosphor Icons Unicode 码点映射表（Phosphor 各字重码点一致：Bold 列同时即 Fill 实心码点）。</summary>
public static class IconNames
{
	// [0] = Phosphor 码点（Bold 与 Fill 实心同码点，fill-style.css 与 bold-style.css 同名比对 100% 一致，渲染统一用此列）；
	// [1] = Duotone 主层 :after 码点（历史数据保留；当前渲染已不再使用 duotone 双层方案）。
	// 表体由 gen-phosphor.ps1 从官方 style.css 生成，请勿手工修改。
	private static readonly Dictionary<string, int[]> Map = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase)
	{
		{ "acorn", new[] { 0xeb9a, 0xeb9b } },
		{ "activity", new[] { 0xe000, 0x0000 } },
		{ "address-book", new[] { 0xe6f8, 0xe6f9 } },
		{ "address-book-tabs", new[] { 0xee4e, 0xee4f } },
		{ "airplane", new[] { 0xe002, 0xe003 } },
		{ "airplane-in-flight", new[] { 0xe4fe, 0xe4ff } },
		{ "airplane-landing", new[] { 0xe502, 0xe503 } },
		{ "airplane-takeoff", new[] { 0xe504, 0xe505 } },
		{ "airplane-taxiing", new[] { 0xe500, 0xe501 } },
		{ "airplane-tilt", new[] { 0xe5d6, 0xe5d7 } },
		{ "airplay", new[] { 0xe004, 0xe005 } },
		{ "air-traffic-control", new[] { 0xecd8, 0xecd9 } },
		{ "alarm", new[] { 0xe006, 0xe007 } },
		{ "alien", new[] { 0xe8a6, 0xe8a7 } },
		{ "align-bottom", new[] { 0xe506, 0xe507 } },
		{ "align-bottom-simple", new[] { 0xeb0c, 0xeb0d } },
		{ "align-center-horizontal", new[] { 0xe50a, 0xe50b } },
		{ "align-center-horizontal-simple", new[] { 0xeb0e, 0xeb0f } },
		{ "align-center-vertical", new[] { 0xe50c, 0xe50d } },
		{ "align-center-vertical-simple", new[] { 0xeb10, 0xeb11 } },
		{ "align-left", new[] { 0xe50e, 0xe50f } },
		{ "align-left-simple", new[] { 0xeaee, 0xeaef } },
		{ "align-right", new[] { 0xe510, 0xe511 } },
		{ "align-right-simple", new[] { 0xeb12, 0xeb13 } },
		{ "align-top", new[] { 0xe512, 0xe513 } },
		{ "align-top-simple", new[] { 0xeb14, 0xeb15 } },
		{ "amazon-logo", new[] { 0xe96c, 0xe96d } },
		{ "ambulance", new[] { 0xe572, 0xe573 } },
		{ "anchor", new[] { 0xe514, 0xe515 } },
		{ "anchor-simple", new[] { 0xe5d8, 0xe5d9 } },
		{ "android-logo", new[] { 0xe008, 0xe009 } },
		{ "angle", new[] { 0xe7bc, 0xe7bd } },
		{ "angular-logo", new[] { 0xeb80, 0xeb81 } },
		{ "aperture", new[] { 0xe00a, 0xe00b } },
		{ "apple-logo", new[] { 0xe516, 0xe517 } },
		{ "apple-podcasts-logo", new[] { 0xeb96, 0xeb97 } },
		{ "approximate-equals", new[] { 0xedaa, 0xedab } },
		{ "app-store-logo", new[] { 0xe974, 0xe975 } },
		{ "app-window", new[] { 0xe5da, 0xe5db } },
		{ "archive", new[] { 0xe00c, 0xe00d } },
		{ "archive-box", new[] { 0xe00e, 0x0000 } },
		{ "archive-tray", new[] { 0xe010, 0x0000 } },
		{ "armchair", new[] { 0xe012, 0xe013 } },
		{ "arrow-arc-left", new[] { 0xe014, 0xe015 } },
		{ "arrow-arc-right", new[] { 0xe016, 0xe017 } },
		{ "arrow-bend-double-up-left", new[] { 0xe03a, 0xe03b } },
		{ "arrow-bend-double-up-right", new[] { 0xe03c, 0xe03d } },
		{ "arrow-bend-down-left", new[] { 0xe018, 0xe019 } },
		{ "arrow-bend-down-right", new[] { 0xe01a, 0xe01b } },
		{ "arrow-bend-left-down", new[] { 0xe01c, 0xe01d } },
		{ "arrow-bend-left-up", new[] { 0xe01e, 0xe01f } },
		{ "arrow-bend-right-down", new[] { 0xe020, 0xe021 } },
		{ "arrow-bend-right-up", new[] { 0xe022, 0xe023 } },
		{ "arrow-bend-up-left", new[] { 0xe024, 0xe025 } },
		{ "arrow-bend-up-right", new[] { 0xe026, 0xe027 } },
		{ "arrow-circle-down", new[] { 0xe028, 0xe029 } },
		{ "arrow-circle-down-left", new[] { 0xe02a, 0xe02b } },
		{ "arrow-circle-down-right", new[] { 0xe02c, 0xe02d } },
		{ "arrow-circle-left", new[] { 0xe05a, 0xe05b } },
		{ "arrow-circle-right", new[] { 0xe02e, 0xe02f } },
		{ "arrow-circle-up", new[] { 0xe030, 0xe031 } },
		{ "arrow-circle-up-left", new[] { 0xe032, 0xe033 } },
		{ "arrow-circle-up-right", new[] { 0xe034, 0xe035 } },
		{ "arrow-clockwise", new[] { 0xe036, 0xe037 } },
		{ "arrow-counter-clockwise", new[] { 0xe038, 0xe039 } },
		{ "arrow-down", new[] { 0xe03e, 0xe03f } },
		{ "arrow-down-left", new[] { 0xe040, 0xe041 } },
		{ "arrow-down-right", new[] { 0xe042, 0xe043 } },
		{ "arrow-elbow-down-left", new[] { 0xe044, 0xe045 } },
		{ "arrow-elbow-down-right", new[] { 0xe046, 0xe047 } },
		{ "arrow-elbow-left", new[] { 0xe048, 0xe049 } },
		{ "arrow-elbow-left-down", new[] { 0xe04a, 0xe04b } },
		{ "arrow-elbow-left-up", new[] { 0xe04c, 0xe04d } },
		{ "arrow-elbow-right", new[] { 0xe04e, 0xe04f } },
		{ "arrow-elbow-right-down", new[] { 0xe050, 0xe051 } },
		{ "arrow-elbow-right-up", new[] { 0xe052, 0xe053 } },
		{ "arrow-elbow-up-left", new[] { 0xe054, 0xe055 } },
		{ "arrow-elbow-up-right", new[] { 0xe056, 0xe057 } },
		{ "arrow-fat-down", new[] { 0xe518, 0xe519 } },
		{ "arrow-fat-left", new[] { 0xe51a, 0xe51b } },
		{ "arrow-fat-line-down", new[] { 0xe51c, 0xe51d } },
		{ "arrow-fat-line-left", new[] { 0xe51e, 0xe51f } },
		{ "arrow-fat-line-right", new[] { 0xe520, 0xe521 } },
		{ "arrow-fat-lines-down", new[] { 0xe524, 0xe525 } },
		{ "arrow-fat-lines-left", new[] { 0xe526, 0xe527 } },
		{ "arrow-fat-lines-right", new[] { 0xe528, 0xe529 } },
		{ "arrow-fat-lines-up", new[] { 0xe52a, 0xe52b } },
		{ "arrow-fat-line-up", new[] { 0xe522, 0xe523 } },
		{ "arrow-fat-right", new[] { 0xe52c, 0xe52d } },
		{ "arrow-fat-up", new[] { 0xe52e, 0xe52f } },
		{ "arrow-left", new[] { 0xe058, 0xe059 } },
		{ "arrow-line-down", new[] { 0xe05c, 0xe05d } },
		{ "arrow-line-down-left", new[] { 0xe05e, 0xe05f } },
		{ "arrow-line-down-right", new[] { 0xe060, 0xe061 } },
		{ "arrow-line-left", new[] { 0xe062, 0xe063 } },
		{ "arrow-line-right", new[] { 0xe064, 0xe065 } },
		{ "arrow-line-up", new[] { 0xe066, 0xe067 } },
		{ "arrow-line-up-left", new[] { 0xe068, 0xe069 } },
		{ "arrow-line-up-right", new[] { 0xe06a, 0xe06b } },
		{ "arrow-right", new[] { 0xe06c, 0xe06d } },
		{ "arrows-clockwise", new[] { 0xe094, 0xe095 } },
		{ "arrows-counter-clockwise", new[] { 0xe096, 0xe097 } },
		{ "arrows-down-up", new[] { 0xe098, 0xe099 } },
		{ "arrows-horizontal", new[] { 0xeb06, 0xeb07 } },
		{ "arrows-in", new[] { 0xe09a, 0xe09b } },
		{ "arrows-in-cardinal", new[] { 0xe09c, 0xe09d } },
		{ "arrows-in-line-horizontal", new[] { 0xe530, 0xe531 } },
		{ "arrows-in-line-vertical", new[] { 0xe532, 0xe533 } },
		{ "arrows-in-simple", new[] { 0xe09e, 0xe09f } },
		{ "arrows-left-right", new[] { 0xe0a0, 0xe0a1 } },
		{ "arrows-merge", new[] { 0xed3e, 0xed3f } },
		{ "arrows-out", new[] { 0xe0a2, 0xe0a3 } },
		{ "arrows-out-cardinal", new[] { 0xe0a4, 0xe0a5 } },
		{ "arrows-out-line-horizontal", new[] { 0xe534, 0xe535 } },
		{ "arrows-out-line-vertical", new[] { 0xe536, 0xe537 } },
		{ "arrows-out-simple", new[] { 0xe0a6, 0xe0a7 } },
		{ "arrow-square-down", new[] { 0xe06e, 0xe06f } },
		{ "arrow-square-down-left", new[] { 0xe070, 0xe071 } },
		{ "arrow-square-down-right", new[] { 0xe072, 0xe073 } },
		{ "arrow-square-in", new[] { 0xe5dc, 0xe5dd } },
		{ "arrow-square-left", new[] { 0xe074, 0xe075 } },
		{ "arrow-square-out", new[] { 0xe5de, 0xe5df } },
		{ "arrow-square-right", new[] { 0xe076, 0xe077 } },
		{ "arrow-square-up", new[] { 0xe078, 0xe079 } },
		{ "arrow-square-up-left", new[] { 0xe07a, 0xe07b } },
		{ "arrow-square-up-right", new[] { 0xe07c, 0xe07d } },
		{ "arrows-split", new[] { 0xed3c, 0xed3d } },
		{ "arrows-vertical", new[] { 0xeb04, 0xeb05 } },
		{ "arrow-u-down-left", new[] { 0xe07e, 0xe07f } },
		{ "arrow-u-down-right", new[] { 0xe080, 0xe081 } },
		{ "arrow-u-left-down", new[] { 0xe082, 0xe083 } },
		{ "arrow-u-left-up", new[] { 0xe084, 0xe085 } },
		{ "arrow-up", new[] { 0xe08e, 0xe08f } },
		{ "arrow-up-left", new[] { 0xe090, 0xe091 } },
		{ "arrow-up-right", new[] { 0xe092, 0xe093 } },
		{ "arrow-u-right-down", new[] { 0xe086, 0xe087 } },
		{ "arrow-u-right-up", new[] { 0xe088, 0xe089 } },
		{ "arrow-u-up-left", new[] { 0xe08a, 0xe08b } },
		{ "arrow-u-up-right", new[] { 0xe08c, 0xe08d } },
		{ "article", new[] { 0xe0a8, 0xe0a9 } },
		{ "article-medium", new[] { 0xe5e0, 0xe5e1 } },
		{ "article-ny-times", new[] { 0xe5e2, 0xe5e3 } },
		{ "asclepius", new[] { 0xee34, 0xee35 } },
		{ "asterisk", new[] { 0xe0aa, 0xe0ab } },
		{ "asterisk-simple", new[] { 0xe832, 0xe833 } },
		{ "at", new[] { 0xe0ac, 0xe0ad } },
		{ "atom", new[] { 0xe5e4, 0xe5e5 } },
		{ "avocado", new[] { 0xee04, 0xee05 } },
		{ "axe", new[] { 0xe9fc, 0xe9fd } },
		{ "baby", new[] { 0xe774, 0xe775 } },
		{ "baby-carriage", new[] { 0xe818, 0xe819 } },
		{ "backpack", new[] { 0xe922, 0xe923 } },
		{ "backspace", new[] { 0xe0ae, 0xe0af } },
		{ "bag", new[] { 0xe0b0, 0xe0b1 } },
		{ "bag-simple", new[] { 0xe5e6, 0xe5e7 } },
		{ "balloon", new[] { 0xe76c, 0xe76d } },
		{ "bandaids", new[] { 0xe0b2, 0xe0b3 } },
		{ "bank", new[] { 0xe0b4, 0xe0b5 } },
		{ "barbell", new[] { 0xe0b6, 0xe0b7 } },
		{ "barcode", new[] { 0xe0b8, 0xe0b9 } },
		{ "barn", new[] { 0xec72, 0xec73 } },
		{ "barricade", new[] { 0xe948, 0xe949 } },
		{ "baseball", new[] { 0xe71a, 0xe71b } },
		{ "baseball-cap", new[] { 0xea28, 0xea29 } },
		{ "baseball-helmet", new[] { 0xee4a, 0xee4b } },
		{ "basket", new[] { 0xe964, 0xe965 } },
		{ "basketball", new[] { 0xe724, 0xe725 } },
		{ "bathtub", new[] { 0xe81e, 0xe81f } },
		{ "battery-charging", new[] { 0xe0ba, 0xe0bb } },
		{ "battery-charging-vertical", new[] { 0xe0bc, 0xe0bd } },
		{ "battery-empty", new[] { 0xe0be, 0xe0bf } },
		{ "battery-full", new[] { 0xe0c0, 0xe0c1 } },
		{ "battery-high", new[] { 0xe0c2, 0xe0c3 } },
		{ "battery-low", new[] { 0xe0c4, 0xe0c5 } },
		{ "battery-medium", new[] { 0xe0c6, 0xe0c7 } },
		{ "battery-plus", new[] { 0xe808, 0xe809 } },
		{ "battery-plus-vertical", new[] { 0xec50, 0xec51 } },
		{ "battery-vertical-empty", new[] { 0xe7c6, 0xe7c7 } },
		{ "battery-vertical-full", new[] { 0xe7c4, 0xe7c5 } },
		{ "battery-vertical-high", new[] { 0xe7c2, 0xe7c3 } },
		{ "battery-vertical-low", new[] { 0xe7be, 0xe7bf } },
		{ "battery-vertical-medium", new[] { 0xe7c0, 0xe7c1 } },
		{ "battery-warning", new[] { 0xe0c8, 0xe0c9 } },
		{ "battery-warning-vertical", new[] { 0xe0ca, 0xe0cb } },
		{ "beach-ball", new[] { 0xed24, 0xed25 } },
		{ "beanie", new[] { 0xea2a, 0xea2b } },
		{ "bed", new[] { 0xe0cc, 0xe0cd } },
		{ "beer-bottle", new[] { 0xe7b0, 0xe7b1 } },
		{ "beer-stein", new[] { 0xeb62, 0xeb63 } },
		{ "behance-logo", new[] { 0xe7f4, 0xe7f5 } },
		{ "bell", new[] { 0xe0ce, 0xe0cf } },
		{ "bell-ringing", new[] { 0xe5e8, 0xe5e9 } },
		{ "bell-simple", new[] { 0xe0d0, 0xe0d1 } },
		{ "bell-simple-ringing", new[] { 0xe5ea, 0xe5eb } },
		{ "bell-simple-slash", new[] { 0xe0d2, 0xe0d3 } },
		{ "bell-simple-z", new[] { 0xe5ec, 0xe5ed } },
		{ "bell-slash", new[] { 0xe0d4, 0xe0d5 } },
		{ "bell-z", new[] { 0xe5ee, 0xe5ef } },
		{ "belt", new[] { 0xea2c, 0xea2d } },
		{ "bezier-curve", new[] { 0xeb00, 0xeb01 } },
		{ "bicycle", new[] { 0xe0d6, 0xe0d7 } },
		{ "binary", new[] { 0xee60, 0xee61 } },
		{ "binoculars", new[] { 0xea64, 0xea65 } },
		{ "biohazard", new[] { 0xe9e0, 0xe9e1 } },
		{ "bird", new[] { 0xe72c, 0xe72d } },
		{ "blueprint", new[] { 0xeda0, 0xeda1 } },
		{ "bluetooth", new[] { 0xe0da, 0xe0db } },
		{ "bluetooth-connected", new[] { 0xe0dc, 0xe0dd } },
		{ "bluetooth-slash", new[] { 0xe0de, 0xe0df } },
		{ "bluetooth-x", new[] { 0xe0e0, 0xe0e1 } },
		{ "boat", new[] { 0xe786, 0xe787 } },
		{ "bomb", new[] { 0xee0a, 0xee0b } },
		{ "bone", new[] { 0xe7f2, 0xe7f3 } },
		{ "book", new[] { 0xe0e2, 0xe0e3 } },
		{ "book-bookmark", new[] { 0xe0e4, 0xe0e5 } },
		{ "bookmark", new[] { 0xe0e8, 0xe0e9 } },
		{ "bookmarks", new[] { 0xe0ec, 0xe0ed } },
		{ "bookmark-simple", new[] { 0xe0ea, 0xe0eb } },
		{ "bookmarks-simple", new[] { 0xe5f0, 0xe5f1 } },
		{ "book-open", new[] { 0xe0e6, 0xe0e7 } },
		{ "book-open-text", new[] { 0xe8f2, 0xe8f3 } },
		{ "book-open-user", new[] { 0xede0, 0xede1 } },
		{ "books", new[] { 0xe758, 0xe759 } },
		{ "boot", new[] { 0xecca, 0xeccb } },
		{ "boules", new[] { 0xe722, 0xe723 } },
		{ "bounding-box", new[] { 0xe6ce, 0xe6cf } },
		{ "bowl-food", new[] { 0xeaa4, 0xeaa5 } },
		{ "bowling-ball", new[] { 0xea34, 0xea35 } },
		{ "bowl-steam", new[] { 0xe8e4, 0xe8e5 } },
		{ "box-arrow-down", new[] { 0xe00e, 0xe00f } },
		{ "box-arrow-up", new[] { 0xee54, 0xee55 } },
		{ "boxing-glove", new[] { 0xea36, 0xea37 } },
		{ "brackets-angle", new[] { 0xe862, 0xe863 } },
		{ "brackets-curly", new[] { 0xe860, 0xe861 } },
		{ "brackets-round", new[] { 0xe864, 0xe865 } },
		{ "brackets-square", new[] { 0xe85e, 0xe85f } },
		{ "brain", new[] { 0xe74e, 0xe74f } },
		{ "brandy", new[] { 0xe6b4, 0xe6b5 } },
		{ "bread", new[] { 0xe81c, 0xe81d } },
		{ "bridge", new[] { 0xea68, 0xea69 } },
		{ "briefcase", new[] { 0xe0ee, 0xe0ef } },
		{ "briefcase-metal", new[] { 0xe5f2, 0xe5f3 } },
		{ "broadcast", new[] { 0xe0f2, 0xe0f3 } },
		{ "broom", new[] { 0xec54, 0xec55 } },
		{ "browser", new[] { 0xe0f4, 0xe0f5 } },
		{ "browsers", new[] { 0xe0f6, 0xe0f7 } },
		{ "bug", new[] { 0xe5f4, 0xe5f5 } },
		{ "bug-beetle", new[] { 0xe5f6, 0xe5f7 } },
		{ "bug-droid", new[] { 0xe5f8, 0xe5f9 } },
		{ "building", new[] { 0xe100, 0xe101 } },
		{ "building-apartment", new[] { 0xe0fe, 0xe103 } },
		{ "building-office", new[] { 0xe0ff, 0xe104 } },
		{ "buildings", new[] { 0xe102, 0xe105 } },
		{ "bulldozer", new[] { 0xec6c, 0xec6d } },
		{ "bus", new[] { 0xe106, 0xe107 } },
		{ "butterfly", new[] { 0xea6e, 0xea6f } },
		{ "cable-car", new[] { 0xe49c, 0xe49d } },
		{ "cactus", new[] { 0xe918, 0xe919 } },
		{ "caduceus", new[] { 0xee34, 0x0000 } },
		{ "cake", new[] { 0xe780, 0xe781 } },
		{ "calculator", new[] { 0xe538, 0xe539 } },
		{ "calendar", new[] { 0xe108, 0xe109 } },
		{ "calendar-blank", new[] { 0xe10a, 0xe10b } },
		{ "calendar-check", new[] { 0xe712, 0xe713 } },
		{ "calendar-dot", new[] { 0xe7b2, 0xe7b3 } },
		{ "calendar-dots", new[] { 0xe7b4, 0xe7b5 } },
		{ "calendar-heart", new[] { 0xe8b0, 0xe8b1 } },
		{ "calendar-minus", new[] { 0xea14, 0xea15 } },
		{ "calendar-plus", new[] { 0xe714, 0xe715 } },
		{ "calendar-slash", new[] { 0xea12, 0xea13 } },
		{ "calendar-star", new[] { 0xe8b2, 0xe8b3 } },
		{ "calendar-x", new[] { 0xe10c, 0xe10d } },
		{ "call-bell", new[] { 0xe7de, 0xe7df } },
		{ "camera", new[] { 0xe10e, 0xe10f } },
		{ "camera-plus", new[] { 0xec58, 0xec59 } },
		{ "camera-rotate", new[] { 0xe7a4, 0xe7a5 } },
		{ "camera-slash", new[] { 0xe110, 0xe111 } },
		{ "campfire", new[] { 0xe9d8, 0xe9d9 } },
		{ "car", new[] { 0xe112, 0xe113 } },
		{ "car-battery", new[] { 0xee30, 0xee31 } },
		{ "cardholder", new[] { 0xe5fa, 0xe5fb } },
		{ "cards", new[] { 0xe0f8, 0xe0f9 } },
		{ "cards-three", new[] { 0xee50, 0xee51 } },
		{ "caret-circle-double-down", new[] { 0xe116, 0xe117 } },
		{ "caret-circle-double-left", new[] { 0xe118, 0xe119 } },
		{ "caret-circle-double-right", new[] { 0xe11a, 0xe11b } },
		{ "caret-circle-double-up", new[] { 0xe11c, 0xe11d } },
		{ "caret-circle-down", new[] { 0xe11e, 0xe11f } },
		{ "caret-circle-left", new[] { 0xe120, 0xe121 } },
		{ "caret-circle-right", new[] { 0xe122, 0xe123 } },
		{ "caret-circle-up", new[] { 0xe124, 0xe125 } },
		{ "caret-circle-up-down", new[] { 0xe13e, 0xe13f } },
		{ "caret-double-down", new[] { 0xe126, 0xe127 } },
		{ "caret-double-left", new[] { 0xe128, 0xe129 } },
		{ "caret-double-right", new[] { 0xe12a, 0xe12b } },
		{ "caret-double-up", new[] { 0xe12c, 0xe12d } },
		{ "caret-down", new[] { 0xe136, 0xe137 } },
		{ "caret-left", new[] { 0xe138, 0xe139 } },
		{ "caret-line-down", new[] { 0xe134, 0xe135 } },
		{ "caret-line-left", new[] { 0xe132, 0xe133 } },
		{ "caret-line-right", new[] { 0xe130, 0xe131 } },
		{ "caret-line-up", new[] { 0xe12e, 0xe12f } },
		{ "caret-right", new[] { 0xe13a, 0xe13b } },
		{ "caret-up", new[] { 0xe13c, 0xe13d } },
		{ "caret-up-down", new[] { 0xe140, 0xe141 } },
		{ "car-profile", new[] { 0xe8cc, 0xe8cd } },
		{ "carrot", new[] { 0xed38, 0xed39 } },
		{ "car-simple", new[] { 0xe114, 0xe115 } },
		{ "cash-register", new[] { 0xed80, 0xed81 } },
		{ "cassette-tape", new[] { 0xed2e, 0xed2f } },
		{ "castle-turret", new[] { 0xe9d0, 0xe9d1 } },
		{ "cat", new[] { 0xe748, 0xe749 } },
		{ "cell-signal-full", new[] { 0xe142, 0xe143 } },
		{ "cell-signal-high", new[] { 0xe144, 0xe145 } },
		{ "cell-signal-low", new[] { 0xe146, 0xe147 } },
		{ "cell-signal-medium", new[] { 0xe148, 0xe149 } },
		{ "cell-signal-none", new[] { 0xe14a, 0x0000 } },
		{ "cell-signal-slash", new[] { 0xe14c, 0xe14d } },
		{ "cell-signal-x", new[] { 0xe14e, 0xe14f } },
		{ "cell-tower", new[] { 0xebaa, 0xebab } },
		{ "certificate", new[] { 0xe766, 0xe767 } },
		{ "chair", new[] { 0xe950, 0xe951 } },
		{ "chalkboard", new[] { 0xe5fc, 0xe5fd } },
		{ "chalkboard-simple", new[] { 0xe5fe, 0xe5ff } },
		{ "chalkboard-teacher", new[] { 0xe600, 0xe601 } },
		{ "champagne", new[] { 0xeaca, 0xeacb } },
		{ "charging-station", new[] { 0xe8d0, 0xe8d1 } },
		{ "chart-bar", new[] { 0xe150, 0xe151 } },
		{ "chart-bar-horizontal", new[] { 0xe152, 0xe153 } },
		{ "chart-donut", new[] { 0xeaa6, 0xeaa7 } },
		{ "chart-line", new[] { 0xe154, 0xe155 } },
		{ "chart-line-down", new[] { 0xe8b6, 0xe8b7 } },
		{ "chart-line-up", new[] { 0xe156, 0xe157 } },
		{ "chart-pie", new[] { 0xe158, 0xe159 } },
		{ "chart-pie-slice", new[] { 0xe15a, 0xe15b } },
		{ "chart-polar", new[] { 0xeaa8, 0xeaa9 } },
		{ "chart-scatter", new[] { 0xeaac, 0xeaad } },
		{ "chat", new[] { 0xe15c, 0xe15d } },
		{ "chat-centered", new[] { 0xe160, 0xe161 } },
		{ "chat-centered-dots", new[] { 0xe164, 0xe165 } },
		{ "chat-centered-slash", new[] { 0xe162, 0xe163 } },
		{ "chat-centered-text", new[] { 0xe166, 0xe167 } },
		{ "chat-circle", new[] { 0xe168, 0xe169 } },
		{ "chat-circle-dots", new[] { 0xe16c, 0xe16d } },
		{ "chat-circle-slash", new[] { 0xe16a, 0xe16b } },
		{ "chat-circle-text", new[] { 0xe16e, 0xe16f } },
		{ "chat-dots", new[] { 0xe170, 0xe171 } },
		{ "chats", new[] { 0xe17c, 0xe17d } },
		{ "chats-circle", new[] { 0xe17e, 0xe17f } },
		{ "chat-slash", new[] { 0xe15e, 0xe15f } },
		{ "chats-teardrop", new[] { 0xe180, 0xe181 } },
		{ "chat-teardrop", new[] { 0xe172, 0xe173 } },
		{ "chat-teardrop-dots", new[] { 0xe176, 0xe177 } },
		{ "chat-teardrop-slash", new[] { 0xe174, 0xe175 } },
		{ "chat-teardrop-text", new[] { 0xe178, 0xe179 } },
		{ "chat-text", new[] { 0xe17a, 0xe17b } },
		{ "check", new[] { 0xe182, 0xe183 } },
		{ "check-circle", new[] { 0xe184, 0xe185 } },
		{ "checkerboard", new[] { 0xe8c4, 0xe8c5 } },
		{ "check-fat", new[] { 0xeba6, 0xeba7 } },
		{ "checks", new[] { 0xe53a, 0xe53b } },
		{ "check-square", new[] { 0xe186, 0xe187 } },
		{ "check-square-offset", new[] { 0xe188, 0xe189 } },
		{ "cheers", new[] { 0xea4a, 0xea4b } },
		{ "cheese", new[] { 0xe9fe, 0xe9ff } },
		{ "chef-hat", new[] { 0xed8e, 0xed8f } },
		{ "cherries", new[] { 0xe830, 0xe831 } },
		{ "church", new[] { 0xecea, 0xeceb } },
		{ "cigarette", new[] { 0xed90, 0xed91 } },
		{ "cigarette-slash", new[] { 0xed92, 0xed93 } },
		{ "circle", new[] { 0xe18a, 0xe18b } },
		{ "circle-dashed", new[] { 0xe602, 0xe603 } },
		{ "circle-half", new[] { 0xe18c, 0xe18d } },
		{ "circle-half-tilt", new[] { 0xe18e, 0xe18f } },
		{ "circle-notch", new[] { 0xeb44, 0xeb45 } },
		{ "circles-four", new[] { 0xe190, 0xe191 } },
		{ "circles-three", new[] { 0xe192, 0xe193 } },
		{ "circles-three-plus", new[] { 0xe194, 0xe195 } },
		{ "circle-wavy", new[] { 0xe604, 0x0000 } },
		{ "circle-wavy-check", new[] { 0xe606, 0x0000 } },
		{ "circle-wavy-question", new[] { 0xe608, 0x0000 } },
		{ "circle-wavy-warning", new[] { 0xe60c, 0x0000 } },
		{ "circuitry", new[] { 0xe9c2, 0xe9c3 } },
		{ "city", new[] { 0xea6a, 0xea6b } },
		{ "clipboard", new[] { 0xe196, 0xe197 } },
		{ "clipboard-text", new[] { 0xe198, 0xe199 } },
		{ "clock", new[] { 0xe19a, 0xe19b } },
		{ "clock-afternoon", new[] { 0xe19c, 0xe19d } },
		{ "clock-clockwise", new[] { 0xe19e, 0xe19f } },
		{ "clock-countdown", new[] { 0xed2c, 0xed2d } },
		{ "clock-counter-clockwise", new[] { 0xe1a0, 0xe1a1 } },
		{ "clock-user", new[] { 0xedec, 0xeded } },
		{ "closed-captioning", new[] { 0xe1a4, 0xe1a5 } },
		{ "cloud", new[] { 0xe1aa, 0xe1ab } },
		{ "cloud-arrow-down", new[] { 0xe1ac, 0xe1ad } },
		{ "cloud-arrow-up", new[] { 0xe1ae, 0xe1af } },
		{ "cloud-check", new[] { 0xe1b0, 0xe1b1 } },
		{ "cloud-fog", new[] { 0xe53c, 0xe53d } },
		{ "cloud-lightning", new[] { 0xe1b2, 0xe1b3 } },
		{ "cloud-moon", new[] { 0xe53e, 0xe53f } },
		{ "cloud-rain", new[] { 0xe1b4, 0xe1b5 } },
		{ "cloud-slash", new[] { 0xe1b6, 0xe1b7 } },
		{ "cloud-snow", new[] { 0xe1b8, 0xe1b9 } },
		{ "cloud-sun", new[] { 0xe540, 0xe541 } },
		{ "cloud-warning", new[] { 0xea98, 0xea99 } },
		{ "cloud-x", new[] { 0xea96, 0xea97 } },
		{ "clover", new[] { 0xedc8, 0xedc9 } },
		{ "club", new[] { 0xe1ba, 0xe1bb } },
		{ "coat-hanger", new[] { 0xe7fe, 0xe7ff } },
		{ "coda-logo", new[] { 0xe7ce, 0xe7cf } },
		{ "code", new[] { 0xe1bc, 0xe1bd } },
		{ "code-block", new[] { 0xeafe, 0xeaff } },
		{ "codepen-logo", new[] { 0xe978, 0xe979 } },
		{ "codesandbox-logo", new[] { 0xea06, 0xea07 } },
		{ "code-simple", new[] { 0xe1be, 0xe1bf } },
		{ "coffee", new[] { 0xe1c2, 0xe1c3 } },
		{ "coffee-bean", new[] { 0xe1c0, 0xe1c1 } },
		{ "coin", new[] { 0xe60e, 0xe60f } },
		{ "coins", new[] { 0xe78e, 0xe78f } },
		{ "coin-vertical", new[] { 0xeb48, 0xeb49 } },
		{ "columns", new[] { 0xe546, 0xe547 } },
		{ "columns-plus-left", new[] { 0xe544, 0xe545 } },
		{ "columns-plus-right", new[] { 0xe542, 0xe543 } },
		{ "command", new[] { 0xe1c4, 0xe1c5 } },
		{ "compass", new[] { 0xe1c8, 0xe1c9 } },
		{ "compass-rose", new[] { 0xe1c6, 0xe1c7 } },
		{ "compass-tool", new[] { 0xea0e, 0xea0f } },
		{ "computer-tower", new[] { 0xe548, 0xe549 } },
		{ "confetti", new[] { 0xe81a, 0xe81b } },
		{ "contactless-payment", new[] { 0xed42, 0xed43 } },
		{ "control", new[] { 0xeca6, 0xeca7 } },
		{ "cookie", new[] { 0xe6ca, 0xe6cb } },
		{ "cooking-pot", new[] { 0xe764, 0xe765 } },
		{ "copy", new[] { 0xe1ca, 0xe1cb } },
		{ "copyleft", new[] { 0xe86a, 0xe86b } },
		{ "copyright", new[] { 0xe54a, 0xe54b } },
		{ "copy-simple", new[] { 0xe1cc, 0xe1cd } },
		{ "corners-in", new[] { 0xe1ce, 0xe1cf } },
		{ "corners-out", new[] { 0xe1d0, 0xe1d1 } },
		{ "couch", new[] { 0xe7f6, 0xe7f7 } },
		{ "court-basketball", new[] { 0xee36, 0xee37 } },
		{ "cow", new[] { 0xeabe, 0xeabf } },
		{ "cowboy-hat", new[] { 0xed12, 0xed13 } },
		{ "cpu", new[] { 0xe610, 0xe611 } },
		{ "crane", new[] { 0xed48, 0xed4b } },
		{ "crane-tower", new[] { 0xed49, 0xed4d } },
		{ "credit-card", new[] { 0xe1d2, 0xe1d3 } },
		{ "cricket", new[] { 0xee12, 0xee13 } },
		{ "crop", new[] { 0xe1d4, 0xe1d5 } },
		{ "cross", new[] { 0xe8a0, 0xe8a1 } },
		{ "crosshair", new[] { 0xe1d6, 0xe1d7 } },
		{ "crosshair-simple", new[] { 0xe1d8, 0xe1d9 } },
		{ "crown", new[] { 0xe614, 0xe615 } },
		{ "crown-cross", new[] { 0xee5e, 0xee5f } },
		{ "crown-simple", new[] { 0xe616, 0xe617 } },
		{ "cube", new[] { 0xe1da, 0xe1db } },
		{ "cube-focus", new[] { 0xed0a, 0xed0b } },
		{ "cube-transparent", new[] { 0xec7c, 0xec7d } },
		{ "currency-btc", new[] { 0xe618, 0xe619 } },
		{ "currency-circle-dollar", new[] { 0xe54c, 0xe54d } },
		{ "currency-cny", new[] { 0xe54e, 0xe54f } },
		{ "currency-dollar", new[] { 0xe550, 0xe551 } },
		{ "currency-dollar-simple", new[] { 0xe552, 0xe553 } },
		{ "currency-eth", new[] { 0xeada, 0xeadb } },
		{ "currency-eur", new[] { 0xe554, 0xe555 } },
		{ "currency-gbp", new[] { 0xe556, 0xe557 } },
		{ "currency-inr", new[] { 0xe558, 0xe559 } },
		{ "currency-jpy", new[] { 0xe55a, 0xe55b } },
		{ "currency-krw", new[] { 0xe55c, 0xe55d } },
		{ "currency-kzt", new[] { 0xec4c, 0xec4d } },
		{ "currency-ngn", new[] { 0xeb52, 0xeb53 } },
		{ "currency-rub", new[] { 0xe55e, 0xe55f } },
		{ "cursor", new[] { 0xe1dc, 0xe1dd } },
		{ "cursor-click", new[] { 0xe7c8, 0xe7c9 } },
		{ "cursor-text", new[] { 0xe7d8, 0xe7d9 } },
		{ "cylinder", new[] { 0xe8fc, 0xe8fd } },
		{ "database", new[] { 0xe1de, 0xe1df } },
		{ "desk", new[] { 0xed16, 0xed17 } },
		{ "desktop", new[] { 0xe560, 0xe561 } },
		{ "desktop-tower", new[] { 0xe562, 0xe563 } },
		{ "detective", new[] { 0xe83e, 0xe83f } },
		{ "device-mobile", new[] { 0xe1e0, 0xe1e1 } },
		{ "device-mobile-camera", new[] { 0xe1e2, 0xe1e3 } },
		{ "device-mobile-slash", new[] { 0xee46, 0xee47 } },
		{ "device-mobile-speaker", new[] { 0xe1e4, 0xe1e5 } },
		{ "device-rotate", new[] { 0xedf2, 0xedf3 } },
		{ "devices", new[] { 0xeba4, 0xeba5 } },
		{ "device-tablet", new[] { 0xe1e6, 0xe1e7 } },
		{ "device-tablet-camera", new[] { 0xe1e8, 0xe1e9 } },
		{ "device-tablet-speaker", new[] { 0xe1ea, 0xe1eb } },
		{ "dev-to-logo", new[] { 0xed0e, 0xed0f } },
		{ "diamond", new[] { 0xe1ec, 0xe1ed } },
		{ "diamonds-four", new[] { 0xe8f4, 0xe8f5 } },
		{ "dice-five", new[] { 0xe1ee, 0xe1ef } },
		{ "dice-four", new[] { 0xe1f0, 0xe1f1 } },
		{ "dice-one", new[] { 0xe1f2, 0xe1f3 } },
		{ "dice-six", new[] { 0xe1f4, 0xe1f5 } },
		{ "dice-three", new[] { 0xe1f6, 0xe1f7 } },
		{ "dice-two", new[] { 0xe1f8, 0xe1f9 } },
		{ "disc", new[] { 0xe564, 0xe565 } },
		{ "disco-ball", new[] { 0xed98, 0xed99 } },
		{ "discord-logo", new[] { 0xe61a, 0xe61b } },
		{ "divide", new[] { 0xe1fa, 0xe1fb } },
		{ "dna", new[] { 0xe924, 0xe925 } },
		{ "dog", new[] { 0xe74a, 0xe74b } },
		{ "door", new[] { 0xe61c, 0xe61d } },
		{ "door-open", new[] { 0xe7e6, 0xe7e7 } },
		{ "dot", new[] { 0xecde, 0xecdf } },
		{ "dot-outline", new[] { 0xece0, 0xece1 } },
		{ "dots-nine", new[] { 0xe1fc, 0xe1fd } },
		{ "dots-six", new[] { 0xe794, 0xe795 } },
		{ "dots-six-vertical", new[] { 0xeae2, 0xeae3 } },
		{ "dots-three", new[] { 0xe1fe, 0xe1ff } },
		{ "dots-three-circle", new[] { 0xe200, 0xe201 } },
		{ "dots-three-circle-vertical", new[] { 0xe202, 0xe203 } },
		{ "dots-three-outline", new[] { 0xe204, 0xe205 } },
		{ "dots-three-outline-vertical", new[] { 0xe206, 0xe207 } },
		{ "dots-three-vertical", new[] { 0xe208, 0xe209 } },
		{ "download", new[] { 0xe20a, 0xe20b } },
		{ "download-simple", new[] { 0xe20c, 0xe20d } },
		{ "dress", new[] { 0xea7e, 0xea7f } },
		{ "dresser", new[] { 0xe94e, 0xe94f } },
		{ "dribbble-logo", new[] { 0xe20e, 0xe20f } },
		{ "drone", new[] { 0xed74, 0xed75 } },
		{ "drop", new[] { 0xe210, 0xe211 } },
		{ "dropbox-logo", new[] { 0xe7d0, 0xe7d1 } },
		{ "drop-half", new[] { 0xe566, 0xe567 } },
		{ "drop-half-bottom", new[] { 0xeb40, 0xeb41 } },
		{ "drop-simple", new[] { 0xee32, 0xee33 } },
		{ "drop-slash", new[] { 0xe954, 0xe955 } },
		{ "ear", new[] { 0xe70c, 0xe70d } },
		{ "ear-slash", new[] { 0xe70e, 0xe70f } },
		{ "egg", new[] { 0xe812, 0xe813 } },
		{ "egg-crack", new[] { 0xeb64, 0xeb65 } },
		{ "eject", new[] { 0xe212, 0xe213 } },
		{ "eject-simple", new[] { 0xe6ae, 0xe6af } },
		{ "elevator", new[] { 0xecc0, 0xecc1 } },
		{ "empty", new[] { 0xedbc, 0xedbd } },
		{ "engine", new[] { 0xea80, 0xea81 } },
		{ "envelope", new[] { 0xe214, 0xe215 } },
		{ "envelope-open", new[] { 0xe216, 0xe217 } },
		{ "envelope-simple", new[] { 0xe218, 0xe219 } },
		{ "envelope-simple-open", new[] { 0xe21a, 0xe21b } },
		{ "equalizer", new[] { 0xebbc, 0xebbd } },
		{ "equals", new[] { 0xe21c, 0xe21d } },
		{ "eraser", new[] { 0xe21e, 0xe21f } },
		{ "escalator-down", new[] { 0xecba, 0xecbb } },
		{ "escalator-up", new[] { 0xecbc, 0xecbd } },
		{ "exam", new[] { 0xe742, 0xe743 } },
		{ "exclamation-mark", new[] { 0xee44, 0xee45 } },
		{ "exclude", new[] { 0xe882, 0xe883 } },
		{ "exclude-square", new[] { 0xe880, 0xe881 } },
		{ "export", new[] { 0xeaf0, 0xeaf1 } },
		{ "eye", new[] { 0xe220, 0xe221 } },
		{ "eye-closed", new[] { 0xe222, 0xe223 } },
		{ "eyedropper", new[] { 0xe568, 0xe569 } },
		{ "eyedropper-sample", new[] { 0xeac4, 0xeac5 } },
		{ "eyeglasses", new[] { 0xe7ba, 0xe7bb } },
		{ "eyes", new[] { 0xee5c, 0xee5d } },
		{ "eye-slash", new[] { 0xe224, 0xe225 } },
		{ "facebook-logo", new[] { 0xe226, 0xe227 } },
		{ "face-mask", new[] { 0xe56a, 0xe56b } },
		{ "factory", new[] { 0xe760, 0xe761 } },
		{ "faders", new[] { 0xe228, 0xe229 } },
		{ "faders-horizontal", new[] { 0xe22a, 0xe22b } },
		{ "fallout-shelter", new[] { 0xe9de, 0xe9df } },
		{ "fan", new[] { 0xe9f2, 0xe9f3 } },
		{ "farm", new[] { 0xec70, 0xec71 } },
		{ "fast-forward", new[] { 0xe6a6, 0xe6a7 } },
		{ "fast-forward-circle", new[] { 0xe22c, 0xe22d } },
		{ "feather", new[] { 0xe9c0, 0xe9c1 } },
		{ "fediverse-logo", new[] { 0xed66, 0xed67 } },
		{ "figma-logo", new[] { 0xe22e, 0xe22f } },
		{ "file", new[] { 0xe230, 0xe231 } },
		{ "file-archive", new[] { 0xeb2a, 0xeb2b } },
		{ "file-arrow-down", new[] { 0xe232, 0xe233 } },
		{ "file-arrow-up", new[] { 0xe61e, 0xe61f } },
		{ "file-audio", new[] { 0xea20, 0xea21 } },
		{ "file-c", new[] { 0xeb32, 0xeb36 } },
		{ "file-cloud", new[] { 0xe95e, 0xe95f } },
		{ "file-code", new[] { 0xe914, 0xe915 } },
		{ "file-cpp", new[] { 0xeb2e, 0xeb2f } },
		{ "file-c-sharp", new[] { 0xeb30, 0xeb31 } },
		{ "file-css", new[] { 0xeb34, 0xeb37 } },
		{ "file-csv", new[] { 0xeb1c, 0xeb1d } },
		{ "file-dashed", new[] { 0xe704, 0xe705 } },
		{ "file-doc", new[] { 0xeb1e, 0xeb1f } },
		{ "file-dotted", new[] { 0xe704, 0x0000 } },
		{ "file-html", new[] { 0xeb38, 0xeb39 } },
		{ "file-image", new[] { 0xea24, 0xea25 } },
		{ "file-ini", new[] { 0xeb33, 0xeb3b } },
		{ "file-jpg", new[] { 0xeb1a, 0xeb1b } },
		{ "file-js", new[] { 0xeb24, 0xeb25 } },
		{ "file-jsx", new[] { 0xeb3a, 0xeb3d } },
		{ "file-lock", new[] { 0xe95c, 0xe95d } },
		{ "file-magnifying-glass", new[] { 0xe238, 0xe239 } },
		{ "file-md", new[] { 0xed50, 0xed51 } },
		{ "file-minus", new[] { 0xe234, 0xe235 } },
		{ "file-pdf", new[] { 0xe702, 0xe703 } },
		{ "file-plus", new[] { 0xe236, 0xe237 } },
		{ "file-png", new[] { 0xeb18, 0xeb19 } },
		{ "file-ppt", new[] { 0xeb20, 0xeb21 } },
		{ "file-py", new[] { 0xeb2c, 0xeb2d } },
		{ "file-rs", new[] { 0xeb28, 0xeb29 } },
		{ "files", new[] { 0xe710, 0xe711 } },
		{ "file-search", new[] { 0xe238, 0x0000 } },
		{ "file-sql", new[] { 0xed4e, 0xed4f } },
		{ "file-svg", new[] { 0xed08, 0xed09 } },
		{ "file-text", new[] { 0xe23a, 0xe23b } },
		{ "file-ts", new[] { 0xeb26, 0xeb27 } },
		{ "file-tsx", new[] { 0xeb3c, 0xeb3f } },
		{ "file-txt", new[] { 0xeb35, 0xeb43 } },
		{ "file-video", new[] { 0xea22, 0xea23 } },
		{ "file-vue", new[] { 0xeb3e, 0xeb47 } },
		{ "file-x", new[] { 0xe23c, 0xe23d } },
		{ "file-xls", new[] { 0xeb22, 0xeb23 } },
		{ "file-zip", new[] { 0xe958, 0xe959 } },
		{ "film-reel", new[] { 0xe8c0, 0xe8c1 } },
		{ "film-script", new[] { 0xeb50, 0xeb51 } },
		{ "film-slate", new[] { 0xe8c2, 0xe8c3 } },
		{ "film-strip", new[] { 0xe792, 0xe793 } },
		{ "fingerprint", new[] { 0xe23e, 0xe23f } },
		{ "fingerprint-simple", new[] { 0xe240, 0xe241 } },
		{ "finn-the-human", new[] { 0xe56c, 0xe56d } },
		{ "fire", new[] { 0xe242, 0xe243 } },
		{ "fire-extinguisher", new[] { 0xe9e8, 0xe9e9 } },
		{ "fire-simple", new[] { 0xe620, 0xe621 } },
		{ "fire-truck", new[] { 0xe574, 0xe575 } },
		{ "first-aid", new[] { 0xe56e, 0xe56f } },
		{ "first-aid-kit", new[] { 0xe570, 0xe571 } },
		{ "fish", new[] { 0xe728, 0xe729 } },
		{ "fish-simple", new[] { 0xe72a, 0xe72b } },
		{ "flag", new[] { 0xe244, 0xe245 } },
		{ "flag-banner", new[] { 0xe622, 0xe623 } },
		{ "flag-banner-fold", new[] { 0xecf2, 0xecf3 } },
		{ "flag-checkered", new[] { 0xea38, 0xea39 } },
		{ "flag-pennant", new[] { 0xecf0, 0xecf1 } },
		{ "flame", new[] { 0xe624, 0xe625 } },
		{ "flashlight", new[] { 0xe246, 0xe247 } },
		{ "flask", new[] { 0xe79e, 0xe79f } },
		{ "flip-horizontal", new[] { 0xed6a, 0xed6b } },
		{ "flip-vertical", new[] { 0xed6c, 0xed6d } },
		{ "floppy-disk", new[] { 0xe248, 0xe249 } },
		{ "floppy-disk-back", new[] { 0xeaf4, 0xeaf5 } },
		{ "flow-arrow", new[] { 0xe6ec, 0xe6ed } },
		{ "flower", new[] { 0xe75e, 0xe75f } },
		{ "flower-lotus", new[] { 0xe6cc, 0xe6cd } },
		{ "flower-tulip", new[] { 0xeacc, 0xeacd } },
		{ "flying-saucer", new[] { 0xeb4a, 0xeb4b } },
		{ "folder", new[] { 0xe24a, 0xe24b } },
		{ "folder-dashed", new[] { 0xe8f8, 0xe8f9 } },
		{ "folder-dotted", new[] { 0xe8f8, 0x0000 } },
		{ "folder-lock", new[] { 0xea3c, 0xea3d } },
		{ "folder-minus", new[] { 0xe254, 0xe255 } },
		{ "folder-notch", new[] { 0xe24a, 0x0000 } },
		{ "folder-notch-minus", new[] { 0xe254, 0x0000 } },
		{ "folder-notch-open", new[] { 0xe256, 0x0000 } },
		{ "folder-notch-plus", new[] { 0xe258, 0x0000 } },
		{ "folder-open", new[] { 0xe256, 0xe257 } },
		{ "folder-plus", new[] { 0xe258, 0xe259 } },
		{ "folders", new[] { 0xe260, 0xe261 } },
		{ "folder-simple", new[] { 0xe25a, 0xe25b } },
		{ "folder-simple-dashed", new[] { 0xec2a, 0xec2b } },
		{ "folder-simple-dotted", new[] { 0xec2a, 0x0000 } },
		{ "folder-simple-lock", new[] { 0xeb5e, 0xeb5f } },
		{ "folder-simple-minus", new[] { 0xe25c, 0xe25d } },
		{ "folder-simple-plus", new[] { 0xe25e, 0xe25f } },
		{ "folder-simple-star", new[] { 0xec2e, 0xec2f } },
		{ "folder-simple-user", new[] { 0xeb60, 0xeb61 } },
		{ "folder-star", new[] { 0xea86, 0xea87 } },
		{ "folder-user", new[] { 0xeb46, 0xeb4c } },
		{ "football", new[] { 0xe718, 0xe719 } },
		{ "football-helmet", new[] { 0xee4c, 0xee4d } },
		{ "footprints", new[] { 0xea88, 0xea89 } },
		{ "fork-knife", new[] { 0xe262, 0xe263 } },
		{ "four-k", new[] { 0xea5c, 0xea5d } },
		{ "frame-corners", new[] { 0xe626, 0xe627 } },
		{ "framer-logo", new[] { 0xe264, 0xe265 } },
		{ "function", new[] { 0xebe4, 0xebe5 } },
		{ "funnel", new[] { 0xe266, 0xe267 } },
		{ "funnel-simple", new[] { 0xe268, 0xe269 } },
		{ "funnel-simple-x", new[] { 0xe26a, 0xe26b } },
		{ "funnel-x", new[] { 0xe26c, 0xe26d } },
		{ "game-controller", new[] { 0xe26e, 0xe26f } },
		{ "garage", new[] { 0xecd6, 0xecd7 } },
		{ "gas-can", new[] { 0xe8ce, 0xe8cf } },
		{ "gas-pump", new[] { 0xe768, 0xe769 } },
		{ "gauge", new[] { 0xe628, 0xe629 } },
		{ "gavel", new[] { 0xea32, 0xea33 } },
		{ "gear", new[] { 0xe270, 0xe271 } },
		{ "gear-fine", new[] { 0xe87c, 0xe87d } },
		{ "gear-six", new[] { 0xe272, 0xe273 } },
		{ "gender-female", new[] { 0xe6e0, 0xe6e1 } },
		{ "gender-intersex", new[] { 0xe6e6, 0xe6e7 } },
		{ "gender-male", new[] { 0xe6e2, 0xe6e3 } },
		{ "gender-neuter", new[] { 0xe6ea, 0xe6eb } },
		{ "gender-nonbinary", new[] { 0xe6e4, 0xe6e5 } },
		{ "gender-transgender", new[] { 0xe6e8, 0xe6e9 } },
		{ "ghost", new[] { 0xe62a, 0xe62b } },
		{ "gif", new[] { 0xe274, 0xe275 } },
		{ "gift", new[] { 0xe276, 0xe277 } },
		{ "git-branch", new[] { 0xe278, 0xe279 } },
		{ "git-commit", new[] { 0xe27a, 0xe27b } },
		{ "git-diff", new[] { 0xe27c, 0xe27d } },
		{ "git-fork", new[] { 0xe27e, 0xe27f } },
		{ "github-logo", new[] { 0xe576, 0xe577 } },
		{ "gitlab-logo", new[] { 0xe694, 0xe695 } },
		{ "gitlab-logo-simple", new[] { 0xe696, 0xe697 } },
		{ "git-merge", new[] { 0xe280, 0xe281 } },
		{ "git-pull-request", new[] { 0xe282, 0xe283 } },
		{ "globe", new[] { 0xe288, 0xe289 } },
		{ "globe-hemisphere-east", new[] { 0xe28a, 0xe28b } },
		{ "globe-hemisphere-west", new[] { 0xe28c, 0xe28d } },
		{ "globe-simple", new[] { 0xe28e, 0xe28f } },
		{ "globe-simple-x", new[] { 0xe284, 0xe285 } },
		{ "globe-stand", new[] { 0xe290, 0xe291 } },
		{ "globe-x", new[] { 0xe286, 0xe287 } },
		{ "goggles", new[] { 0xecb4, 0xecb5 } },
		{ "golf", new[] { 0xea3e, 0xea3f } },
		{ "goodreads-logo", new[] { 0xed10, 0xed11 } },
		{ "google-cardboard-logo", new[] { 0xe7b6, 0xe7b7 } },
		{ "google-chrome-logo", new[] { 0xe976, 0xe977 } },
		{ "google-drive-logo", new[] { 0xe8f6, 0xe8f7 } },
		{ "google-logo", new[] { 0xe292, 0xe293 } },
		{ "google-photos-logo", new[] { 0xeb92, 0xeb93 } },
		{ "google-play-logo", new[] { 0xe294, 0xe295 } },
		{ "google-podcasts-logo", new[] { 0xeb94, 0xeb95 } },
		{ "gps", new[] { 0xedd8, 0xedd9 } },
		{ "gps-fix", new[] { 0xedd6, 0xedd7 } },
		{ "gps-slash", new[] { 0xedd4, 0xedd5 } },
		{ "gradient", new[] { 0xeb42, 0xeb4d } },
		{ "graduation-cap", new[] { 0xe62c, 0xe62d } },
		{ "grains", new[] { 0xec68, 0xec69 } },
		{ "grains-slash", new[] { 0xec6a, 0xec6b } },
		{ "graph", new[] { 0xeb58, 0xeb59 } },
		{ "graphics-card", new[] { 0xe612, 0xe613 } },
		{ "greater-than", new[] { 0xedc4, 0xedc5 } },
		{ "greater-than-or-equal", new[] { 0xeda2, 0xeda3 } },
		{ "grid-four", new[] { 0xe296, 0xe297 } },
		{ "grid-nine", new[] { 0xec8c, 0xec8d } },
		{ "guitar", new[] { 0xea8a, 0xea8b } },
		{ "hair-dryer", new[] { 0xea66, 0xea67 } },
		{ "hamburger", new[] { 0xe790, 0xe791 } },
		{ "hammer", new[] { 0xe80e, 0xe80f } },
		{ "hand", new[] { 0xe298, 0xe299 } },
		{ "hand-arrow-down", new[] { 0xea4e, 0xea4f } },
		{ "hand-arrow-up", new[] { 0xee5a, 0xee5b } },
		{ "handbag", new[] { 0xe29c, 0xe29d } },
		{ "handbag-simple", new[] { 0xe62e, 0xe62f } },
		{ "hand-coins", new[] { 0xea8c, 0xea8d } },
		{ "hand-deposit", new[] { 0xee82, 0xee83 } },
		{ "hand-eye", new[] { 0xea4c, 0xea4d } },
		{ "hand-fist", new[] { 0xe57a, 0xe57b } },
		{ "hand-grabbing", new[] { 0xe57c, 0xe57d } },
		{ "hand-heart", new[] { 0xe810, 0xe811 } },
		{ "hand-palm", new[] { 0xe57e, 0xe57f } },
		{ "hand-peace", new[] { 0xe7cc, 0xe7cd } },
		{ "hand-pointing", new[] { 0xe29a, 0xe29b } },
		{ "hands-clapping", new[] { 0xe6a0, 0xe6a1 } },
		{ "handshake", new[] { 0xe582, 0xe583 } },
		{ "hand-soap", new[] { 0xe630, 0xe631 } },
		{ "hands-praying", new[] { 0xecc8, 0xecc9 } },
		{ "hand-swipe-left", new[] { 0xec94, 0xec95 } },
		{ "hand-swipe-right", new[] { 0xec92, 0xec93 } },
		{ "hand-tap", new[] { 0xec90, 0xec91 } },
		{ "hand-waving", new[] { 0xe580, 0xe581 } },
		{ "hand-withdraw", new[] { 0xee80, 0xee81 } },
		{ "hard-drive", new[] { 0xe29e, 0xe29f } },
		{ "hard-drives", new[] { 0xe2a0, 0xe2a1 } },
		{ "hard-hat", new[] { 0xed46, 0xed47 } },
		{ "hash", new[] { 0xe2a2, 0xe2a3 } },
		{ "hash-straight", new[] { 0xe2a4, 0xe2a5 } },
		{ "head-circuit", new[] { 0xe7d4, 0xe7d5 } },
		{ "headlights", new[] { 0xe6fe, 0xe6ff } },
		{ "headphones", new[] { 0xe2a6, 0xe2a7 } },
		{ "headset", new[] { 0xe584, 0xe585 } },
		{ "heart", new[] { 0xe2a8, 0xe2a9 } },
		{ "heartbeat", new[] { 0xe2ac, 0xe2ad } },
		{ "heart-break", new[] { 0xebe8, 0xebe9 } },
		{ "heart-half", new[] { 0xec48, 0xec49 } },
		{ "heart-straight", new[] { 0xe2aa, 0xe2ab } },
		{ "heart-straight-break", new[] { 0xeb98, 0xeb99 } },
		{ "hexagon", new[] { 0xe2ae, 0xe2af } },
		{ "high-definition", new[] { 0xea8e, 0xea8f } },
		{ "high-heel", new[] { 0xe8e8, 0xe8e9 } },
		{ "highlighter", new[] { 0xec76, 0xec77 } },
		{ "highlighter-circle", new[] { 0xe632, 0xe633 } },
		{ "hockey", new[] { 0xec86, 0xec87 } },
		{ "hoodie", new[] { 0xecd0, 0xecd1 } },
		{ "horse", new[] { 0xe2b0, 0xe2b1 } },
		{ "hospital", new[] { 0xe844, 0xe845 } },
		{ "hourglass", new[] { 0xe2b2, 0xe2b3 } },
		{ "hourglass-high", new[] { 0xe2b4, 0xe2b5 } },
		{ "hourglass-low", new[] { 0xe2b6, 0xe2b7 } },
		{ "hourglass-medium", new[] { 0xe2b8, 0xe2b9 } },
		{ "hourglass-simple", new[] { 0xe2ba, 0xe2bb } },
		{ "hourglass-simple-high", new[] { 0xe2bc, 0xe2bd } },
		{ "hourglass-simple-low", new[] { 0xe2be, 0xe2bf } },
		{ "hourglass-simple-medium", new[] { 0xe2c0, 0xe2c1 } },
		{ "house", new[] { 0xe2c2, 0xe2c3 } },
		{ "house-line", new[] { 0xe2c4, 0xe2c5 } },
		{ "house-simple", new[] { 0xe2c6, 0xe2c7 } },
		{ "hurricane", new[] { 0xe88e, 0xe88f } },
		{ "ice-cream", new[] { 0xe804, 0xe805 } },
		{ "identification-badge", new[] { 0xe6f6, 0xe6f7 } },
		{ "identification-card", new[] { 0xe2c8, 0xe2c9 } },
		{ "image", new[] { 0xe2ca, 0xe2cb } },
		{ "image-broken", new[] { 0xe7a8, 0xe7a9 } },
		{ "images", new[] { 0xe836, 0xe837 } },
		{ "image-square", new[] { 0xe2cc, 0xe2cd } },
		{ "images-square", new[] { 0xe834, 0xe835 } },
		{ "infinity", new[] { 0xe634, 0xe635 } },
		{ "info", new[] { 0xe2ce, 0xe2cf } },
		{ "instagram-logo", new[] { 0xe2d0, 0xe2d1 } },
		{ "intersect", new[] { 0xe2d2, 0xe2d3 } },
		{ "intersection", new[] { 0xedba, 0xedbb } },
		{ "intersect-square", new[] { 0xe87a, 0xe87b } },
		{ "intersect-three", new[] { 0xecc4, 0xecc5 } },
		{ "invoice", new[] { 0xee42, 0xee43 } },
		{ "island", new[] { 0xee06, 0xee07 } },
		{ "jar", new[] { 0xe7e0, 0xe7e3 } },
		{ "jar-label", new[] { 0xe7e1, 0xe7e5 } },
		{ "jeep", new[] { 0xe2d4, 0xe2d5 } },
		{ "joystick", new[] { 0xea5e, 0xea5f } },
		{ "kanban", new[] { 0xeb54, 0xeb55 } },
		{ "key", new[] { 0xe2d6, 0xe2d7 } },
		{ "keyboard", new[] { 0xe2d8, 0xe2d9 } },
		{ "keyhole", new[] { 0xea78, 0xea79 } },
		{ "key-return", new[] { 0xe782, 0xe783 } },
		{ "knife", new[] { 0xe636, 0xe637 } },
		{ "ladder", new[] { 0xe9e4, 0xe9e5 } },
		{ "ladder-simple", new[] { 0xec26, 0xec27 } },
		{ "lamp", new[] { 0xe638, 0xe639 } },
		{ "lamp-pendant", new[] { 0xee2e, 0xee2f } },
		{ "laptop", new[] { 0xe586, 0xe587 } },
		{ "lasso", new[] { 0xedc6, 0xedc7 } },
		{ "lastfm-logo", new[] { 0xe842, 0xe843 } },
		{ "layout", new[] { 0xe6d6, 0xe6d7 } },
		{ "leaf", new[] { 0xe2da, 0xe2db } },
		{ "lectern", new[] { 0xe95a, 0xe95b } },
		{ "lego", new[] { 0xe8c6, 0xe8c8 } },
		{ "lego-smiley", new[] { 0xe8c7, 0xe8c9 } },
		{ "lemniscate", new[] { 0xe634, 0x0000 } },
		{ "less-than", new[] { 0xedac, 0xedad } },
		{ "less-than-or-equal", new[] { 0xeda4, 0xeda5 } },
		{ "letter-circle-h", new[] { 0xebf8, 0xebf9 } },
		{ "letter-circle-p", new[] { 0xec08, 0xec09 } },
		{ "letter-circle-v", new[] { 0xec14, 0xec15 } },
		{ "lifebuoy", new[] { 0xe63a, 0xe63b } },
		{ "lightbulb", new[] { 0xe2dc, 0xe2dd } },
		{ "lightbulb-filament", new[] { 0xe63c, 0xe63d } },
		{ "lighthouse", new[] { 0xe9f6, 0xe9f7 } },
		{ "lightning", new[] { 0xe2de, 0xe2df } },
		{ "lightning-a", new[] { 0xea84, 0xea85 } },
		{ "lightning-slash", new[] { 0xe2e0, 0xe2e1 } },
		{ "line-segment", new[] { 0xe6d2, 0xe6d3 } },
		{ "line-segments", new[] { 0xe6d4, 0xe6d5 } },
		{ "line-vertical", new[] { 0xed70, 0xed71 } },
		{ "link", new[] { 0xe2e2, 0xe2e3 } },
		{ "link-break", new[] { 0xe2e4, 0xe2e5 } },
		{ "linkedin-logo", new[] { 0xe2ee, 0xe2ef } },
		{ "link-simple", new[] { 0xe2e6, 0xe2e7 } },
		{ "link-simple-break", new[] { 0xe2e8, 0xe2e9 } },
		{ "link-simple-horizontal", new[] { 0xe2ea, 0xe2eb } },
		{ "link-simple-horizontal-break", new[] { 0xe2ec, 0xe2ed } },
		{ "linktree-logo", new[] { 0xedee, 0xedef } },
		{ "linux-logo", new[] { 0xeb02, 0xeb03 } },
		{ "list", new[] { 0xe2f0, 0xe2f1 } },
		{ "list-bullets", new[] { 0xe2f2, 0xe2f3 } },
		{ "list-checks", new[] { 0xeadc, 0xeadd } },
		{ "list-dashes", new[] { 0xe2f4, 0xe2f5 } },
		{ "list-heart", new[] { 0xebde, 0xebdf } },
		{ "list-magnifying-glass", new[] { 0xebe0, 0xebe1 } },
		{ "list-numbers", new[] { 0xe2f6, 0xe2f7 } },
		{ "list-plus", new[] { 0xe2f8, 0xe2f9 } },
		{ "list-star", new[] { 0xebdc, 0xebdd } },
		{ "lock", new[] { 0xe2fa, 0xe2fb } },
		{ "lockers", new[] { 0xecb8, 0xecb9 } },
		{ "lock-key", new[] { 0xe2fe, 0xe2ff } },
		{ "lock-key-open", new[] { 0xe300, 0xe301 } },
		{ "lock-laminated", new[] { 0xe302, 0xe303 } },
		{ "lock-laminated-open", new[] { 0xe304, 0xe305 } },
		{ "lock-open", new[] { 0xe306, 0xe307 } },
		{ "lock-simple", new[] { 0xe308, 0xe309 } },
		{ "lock-simple-open", new[] { 0xe30a, 0xe30b } },
		{ "log", new[] { 0xed82, 0xed83 } },
		{ "magic-wand", new[] { 0xe6b6, 0xe6b7 } },
		{ "magnet", new[] { 0xe680, 0xe681 } },
		{ "magnet-straight", new[] { 0xe682, 0xe683 } },
		{ "magnifying-glass", new[] { 0xe30c, 0xe30d } },
		{ "magnifying-glass-minus", new[] { 0xe30e, 0xe30f } },
		{ "magnifying-glass-plus", new[] { 0xe310, 0xe311 } },
		{ "mailbox", new[] { 0xec1e, 0xec1f } },
		{ "map-pin", new[] { 0xe316, 0xe317 } },
		{ "map-pin-area", new[] { 0xee3a, 0xee3b } },
		{ "map-pin-line", new[] { 0xe318, 0xe319 } },
		{ "map-pin-plus", new[] { 0xe314, 0xe315 } },
		{ "map-pin-simple", new[] { 0xee3e, 0xee3f } },
		{ "map-pin-simple-area", new[] { 0xee3c, 0xee3d } },
		{ "map-pin-simple-line", new[] { 0xee38, 0xee39 } },
		{ "map-trifold", new[] { 0xe31a, 0xe31b } },
		{ "markdown-logo", new[] { 0xe508, 0xe509 } },
		{ "marker-circle", new[] { 0xe640, 0xe641 } },
		{ "martini", new[] { 0xe31c, 0xe31d } },
		{ "mask-happy", new[] { 0xe9f4, 0xe9f5 } },
		{ "mask-sad", new[] { 0xeb9e, 0xeb9f } },
		{ "mastodon-logo", new[] { 0xed68, 0xed69 } },
		{ "math-operations", new[] { 0xe31e, 0xe31f } },
		{ "matrix-logo", new[] { 0xed64, 0xed65 } },
		{ "medal", new[] { 0xe320, 0xe321 } },
		{ "medal-military", new[] { 0xecfc, 0xecfd } },
		{ "medium-logo", new[] { 0xe322, 0xe323 } },
		{ "megaphone", new[] { 0xe324, 0xe325 } },
		{ "megaphone-simple", new[] { 0xe642, 0xe643 } },
		{ "member-of", new[] { 0xedc2, 0xedc3 } },
		{ "memory", new[] { 0xe9c4, 0xe9c5 } },
		{ "messenger-logo", new[] { 0xe6d8, 0xe6d9 } },
		{ "meta-logo", new[] { 0xed02, 0xed03 } },
		{ "meteor", new[] { 0xe9ba, 0xe9bb } },
		{ "metronome", new[] { 0xec8e, 0xec8f } },
		{ "microphone", new[] { 0xe326, 0xe327 } },
		{ "microphone-slash", new[] { 0xe328, 0xe329 } },
		{ "microphone-stage", new[] { 0xe75c, 0xe75d } },
		{ "microscope", new[] { 0xec7a, 0xec7b } },
		{ "microsoft-excel-logo", new[] { 0xeb6c, 0xeb6d } },
		{ "microsoft-outlook-logo", new[] { 0xeb70, 0xeb71 } },
		{ "microsoft-powerpoint-logo", new[] { 0xeace, 0xeacf } },
		{ "microsoft-teams-logo", new[] { 0xeb66, 0xeb67 } },
		{ "microsoft-word-logo", new[] { 0xeb6a, 0xeb6b } },
		{ "minus", new[] { 0xe32a, 0xe32b } },
		{ "minus-circle", new[] { 0xe32c, 0xe32d } },
		{ "minus-square", new[] { 0xed4c, 0xed53 } },
		{ "money", new[] { 0xe588, 0xe589 } },
		{ "money-wavy", new[] { 0xee68, 0xee69 } },
		{ "monitor", new[] { 0xe32e, 0xe32f } },
		{ "monitor-arrow-up", new[] { 0xe58a, 0xe58b } },
		{ "monitor-play", new[] { 0xe58c, 0xe58d } },
		{ "moon", new[] { 0xe330, 0xe331 } },
		{ "moon-stars", new[] { 0xe58e, 0xe58f } },
		{ "moped", new[] { 0xe824, 0xe825 } },
		{ "moped-front", new[] { 0xe822, 0xe823 } },
		{ "mosque", new[] { 0xecee, 0xecef } },
		{ "motorcycle", new[] { 0xe80a, 0xe80b } },
		{ "mountains", new[] { 0xe7ae, 0xe7af } },
		{ "mouse", new[] { 0xe33a, 0xe33b } },
		{ "mouse-left-click", new[] { 0xe334, 0xe335 } },
		{ "mouse-middle-click", new[] { 0xe338, 0xe339 } },
		{ "mouse-right-click", new[] { 0xe336, 0xe337 } },
		{ "mouse-scroll", new[] { 0xe332, 0xe333 } },
		{ "mouse-simple", new[] { 0xe644, 0xe645 } },
		{ "music-note", new[] { 0xe33c, 0xe33d } },
		{ "music-notes", new[] { 0xe340, 0xe341 } },
		{ "music-note-simple", new[] { 0xe33e, 0xe33f } },
		{ "music-notes-minus", new[] { 0xee0c, 0xee0d } },
		{ "music-notes-plus", new[] { 0xeb7c, 0xeb7d } },
		{ "music-notes-simple", new[] { 0xe342, 0xe343 } },
		{ "navigation-arrow", new[] { 0xeade, 0xeadf } },
		{ "needle", new[] { 0xe82e, 0xe82f } },
		{ "network", new[] { 0xedde, 0xeddf } },
		{ "network-slash", new[] { 0xeddc, 0xeddd } },
		{ "network-x", new[] { 0xedda, 0xeddb } },
		{ "newspaper", new[] { 0xe344, 0xe345 } },
		{ "newspaper-clipping", new[] { 0xe346, 0xe347 } },
		{ "notches", new[] { 0xed3a, 0xed3b } },
		{ "note", new[] { 0xe348, 0xe349 } },
		{ "note-blank", new[] { 0xe34a, 0xe34b } },
		{ "notebook", new[] { 0xe34e, 0xe34f } },
		{ "notepad", new[] { 0xe63e, 0xe63f } },
		{ "note-pencil", new[] { 0xe34c, 0xe34d } },
		{ "not-equals", new[] { 0xeda6, 0xeda7 } },
		{ "notification", new[] { 0xe6fa, 0xe6fb } },
		{ "notion-logo", new[] { 0xe9a0, 0xe9a1 } },
		{ "not-member-of", new[] { 0xedae, 0xedaf } },
		{ "not-subset-of", new[] { 0xedb0, 0xedb1 } },
		{ "not-superset-of", new[] { 0xedb2, 0xedb3 } },
		{ "nuclear-plant", new[] { 0xed7c, 0xed7d } },
		{ "number-circle-eight", new[] { 0xe352, 0xe353 } },
		{ "number-circle-five", new[] { 0xe358, 0xe359 } },
		{ "number-circle-four", new[] { 0xe35e, 0xe35f } },
		{ "number-circle-nine", new[] { 0xe364, 0xe365 } },
		{ "number-circle-one", new[] { 0xe36a, 0xe36b } },
		{ "number-circle-seven", new[] { 0xe370, 0xe371 } },
		{ "number-circle-six", new[] { 0xe376, 0xe377 } },
		{ "number-circle-three", new[] { 0xe37c, 0xe37d } },
		{ "number-circle-two", new[] { 0xe382, 0xe383 } },
		{ "number-circle-zero", new[] { 0xe388, 0xe389 } },
		{ "number-eight", new[] { 0xe350, 0xe351 } },
		{ "number-five", new[] { 0xe356, 0xe357 } },
		{ "number-four", new[] { 0xe35c, 0xe35d } },
		{ "number-nine", new[] { 0xe362, 0xe363 } },
		{ "number-one", new[] { 0xe368, 0xe369 } },
		{ "number-seven", new[] { 0xe36e, 0xe36f } },
		{ "number-six", new[] { 0xe374, 0xe375 } },
		{ "number-square-eight", new[] { 0xe354, 0xe355 } },
		{ "number-square-five", new[] { 0xe35a, 0xe35b } },
		{ "number-square-four", new[] { 0xe360, 0xe361 } },
		{ "number-square-nine", new[] { 0xe366, 0xe367 } },
		{ "number-square-one", new[] { 0xe36c, 0xe36d } },
		{ "number-square-seven", new[] { 0xe372, 0xe373 } },
		{ "number-square-six", new[] { 0xe378, 0xe379 } },
		{ "number-square-three", new[] { 0xe37e, 0xe37f } },
		{ "number-square-two", new[] { 0xe384, 0xe385 } },
		{ "number-square-zero", new[] { 0xe38a, 0xe38b } },
		{ "number-three", new[] { 0xe37a, 0xe37b } },
		{ "number-two", new[] { 0xe380, 0xe381 } },
		{ "number-zero", new[] { 0xe386, 0xe387 } },
		{ "numpad", new[] { 0xe3c8, 0xe3c9 } },
		{ "nut", new[] { 0xe38c, 0xe38d } },
		{ "ny-times-logo", new[] { 0xe646, 0xe647 } },
		{ "octagon", new[] { 0xe38e, 0xe38f } },
		{ "office-chair", new[] { 0xea46, 0xea47 } },
		{ "onigiri", new[] { 0xee2c, 0xee2d } },
		{ "open-ai-logo", new[] { 0xe7d2, 0xe7d3 } },
		{ "option", new[] { 0xe8a8, 0xe8a9 } },
		{ "orange", new[] { 0xee40, 0xee41 } },
		{ "orange-slice", new[] { 0xed36, 0xed37 } },
		{ "oven", new[] { 0xed8c, 0xed8d } },
		{ "package", new[] { 0xe390, 0xe391 } },
		{ "paint-brush", new[] { 0xe6f0, 0xe6f1 } },
		{ "paint-brush-broad", new[] { 0xe590, 0xe591 } },
		{ "paint-brush-household", new[] { 0xe6f2, 0xe6f3 } },
		{ "paint-bucket", new[] { 0xe392, 0xe393 } },
		{ "paint-roller", new[] { 0xe6f4, 0xe6f5 } },
		{ "palette", new[] { 0xe6c8, 0xe6c9 } },
		{ "panorama", new[] { 0xeaa2, 0xeaa3 } },
		{ "pants", new[] { 0xec88, 0xec89 } },
		{ "paperclip", new[] { 0xe39a, 0xe39b } },
		{ "paperclip-horizontal", new[] { 0xe592, 0xe593 } },
		{ "paper-plane", new[] { 0xe394, 0xe395 } },
		{ "paper-plane-right", new[] { 0xe396, 0xe397 } },
		{ "paper-plane-tilt", new[] { 0xe398, 0xe399 } },
		{ "parachute", new[] { 0xea7c, 0xea7d } },
		{ "paragraph", new[] { 0xe960, 0xe961 } },
		{ "parallelogram", new[] { 0xecc6, 0xecc7 } },
		{ "park", new[] { 0xecb2, 0xecb3 } },
		{ "password", new[] { 0xe752, 0xe753 } },
		{ "path", new[] { 0xe39c, 0xe39d } },
		{ "patreon-logo", new[] { 0xe98a, 0xe98b } },
		{ "pause", new[] { 0xe39e, 0xe39f } },
		{ "pause-circle", new[] { 0xe3a0, 0xe3a1 } },
		{ "paw-print", new[] { 0xe648, 0xe649 } },
		{ "paypal-logo", new[] { 0xe98c, 0xe98d } },
		{ "peace", new[] { 0xe3a2, 0xe3a3 } },
		{ "pen", new[] { 0xe3aa, 0xe3ab } },
		{ "pencil", new[] { 0xe3ae, 0xe3af } },
		{ "pencil-circle", new[] { 0xe3b0, 0xe3b1 } },
		{ "pencil-line", new[] { 0xe3b2, 0xe3b3 } },
		{ "pencil-ruler", new[] { 0xe906, 0xe907 } },
		{ "pencil-simple", new[] { 0xe3b4, 0xe3b5 } },
		{ "pencil-simple-line", new[] { 0xebc6, 0xebc7 } },
		{ "pencil-simple-slash", new[] { 0xecf6, 0xecf7 } },
		{ "pencil-slash", new[] { 0xecf8, 0xecf9 } },
		{ "pen-nib", new[] { 0xe3ac, 0xe3ad } },
		{ "pen-nib-straight", new[] { 0xe64a, 0xe64b } },
		{ "pentagon", new[] { 0xec7e, 0xec7f } },
		{ "pentagram", new[] { 0xec5c, 0xec5d } },
		{ "pepper", new[] { 0xe94a, 0xe94b } },
		{ "percent", new[] { 0xe3b6, 0xe3b7 } },
		{ "person", new[] { 0xe3a8, 0xe3a9 } },
		{ "person-arms-spread", new[] { 0xecfe, 0xecff } },
		{ "person-simple", new[] { 0xe72e, 0xe72f } },
		{ "person-simple-bike", new[] { 0xe734, 0xe735 } },
		{ "person-simple-circle", new[] { 0xee58, 0xee59 } },
		{ "person-simple-hike", new[] { 0xed54, 0xed55 } },
		{ "person-simple-run", new[] { 0xe730, 0xe731 } },
		{ "person-simple-ski", new[] { 0xe71c, 0xe71d } },
		{ "person-simple-snowboard", new[] { 0xe71e, 0xe71f } },
		{ "person-simple-swim", new[] { 0xe736, 0xe737 } },
		{ "person-simple-tai-chi", new[] { 0xed5c, 0xed5d } },
		{ "person-simple-throw", new[] { 0xe732, 0xe733 } },
		{ "person-simple-walk", new[] { 0xe73a, 0xe73b } },
		{ "perspective", new[] { 0xebe6, 0xebe7 } },
		{ "phone", new[] { 0xe3b8, 0xe3b9 } },
		{ "phone-call", new[] { 0xe3ba, 0xe3bb } },
		{ "phone-disconnect", new[] { 0xe3bc, 0xe3bd } },
		{ "phone-incoming", new[] { 0xe3be, 0xe3bf } },
		{ "phone-list", new[] { 0xe3cc, 0xe3cd } },
		{ "phone-outgoing", new[] { 0xe3c0, 0xe3c1 } },
		{ "phone-pause", new[] { 0xe3ca, 0xe3cb } },
		{ "phone-plus", new[] { 0xec56, 0xec57 } },
		{ "phone-slash", new[] { 0xe3c2, 0xe3c3 } },
		{ "phone-transfer", new[] { 0xe3c6, 0xe3c7 } },
		{ "phone-x", new[] { 0xe3c4, 0xe3c5 } },
		{ "phosphor-logo", new[] { 0xe3ce, 0xe3cf } },
		{ "pi", new[] { 0xec80, 0xec81 } },
		{ "piano-keys", new[] { 0xe9c8, 0xe9c9 } },
		{ "picnic-table", new[] { 0xee26, 0xee27 } },
		{ "picture-in-picture", new[] { 0xe64c, 0xe64d } },
		{ "piggy-bank", new[] { 0xea04, 0xea05 } },
		{ "pill", new[] { 0xe700, 0xe701 } },
		{ "ping-pong", new[] { 0xea42, 0xea43 } },
		{ "pinterest-logo", new[] { 0xe64e, 0xe64f } },
		{ "pint-glass", new[] { 0xedd0, 0xedd1 } },
		{ "pinwheel", new[] { 0xeb9c, 0xeb9d } },
		{ "pipe", new[] { 0xed86, 0xed87 } },
		{ "pipe-wrench", new[] { 0xed88, 0xed89 } },
		{ "pix-logo", new[] { 0xecc2, 0xecc3 } },
		{ "pizza", new[] { 0xe796, 0xe797 } },
		{ "placeholder", new[] { 0xe650, 0xe651 } },
		{ "planet", new[] { 0xe652, 0xe653 } },
		{ "plant", new[] { 0xebae, 0xebaf } },
		{ "play", new[] { 0xe3d0, 0xe3d1 } },
		{ "play-circle", new[] { 0xe3d2, 0xe3d3 } },
		{ "playlist", new[] { 0xe6aa, 0xe6ab } },
		{ "play-pause", new[] { 0xe8be, 0xe8bf } },
		{ "plug", new[] { 0xe946, 0xe947 } },
		{ "plug-charging", new[] { 0xeb5c, 0xeb5d } },
		{ "plugs", new[] { 0xeb56, 0xeb57 } },
		{ "plugs-connected", new[] { 0xeb5a, 0xeb5b } },
		{ "plus", new[] { 0xe3d4, 0xe3d5 } },
		{ "plus-circle", new[] { 0xe3d6, 0xe3d7 } },
		{ "plus-minus", new[] { 0xe3d8, 0xe3d9 } },
		{ "plus-square", new[] { 0xed4a, 0xed56 } },
		{ "poker-chip", new[] { 0xe594, 0xe595 } },
		{ "police-car", new[] { 0xec4a, 0xec4b } },
		{ "polygon", new[] { 0xe6d0, 0xe6d1 } },
		{ "popcorn", new[] { 0xeb4e, 0xeb4f } },
		{ "popsicle", new[] { 0xebbe, 0xebbf } },
		{ "potted-plant", new[] { 0xec22, 0xec23 } },
		{ "power", new[] { 0xe3da, 0xe3db } },
		{ "prescription", new[] { 0xe7a2, 0xe7a3 } },
		{ "presentation", new[] { 0xe654, 0xe655 } },
		{ "presentation-chart", new[] { 0xe656, 0xe657 } },
		{ "printer", new[] { 0xe3dc, 0xe3dd } },
		{ "prohibit", new[] { 0xe3de, 0xe3df } },
		{ "prohibit-inset", new[] { 0xe3e0, 0xe3e1 } },
		{ "projector-screen", new[] { 0xe658, 0xe659 } },
		{ "projector-screen-chart", new[] { 0xe65a, 0xe65b } },
		{ "pulse", new[] { 0xe000, 0xe001 } },
		{ "push-pin", new[] { 0xe3e2, 0xe3e3 } },
		{ "push-pin-simple", new[] { 0xe65c, 0xe65d } },
		{ "push-pin-simple-slash", new[] { 0xe65e, 0xe65f } },
		{ "push-pin-slash", new[] { 0xe3e4, 0xe3e5 } },
		{ "puzzle-piece", new[] { 0xe596, 0xe597 } },
		{ "qr-code", new[] { 0xe3e6, 0xe3e7 } },
		{ "question", new[] { 0xe3e8, 0xe3eb } },
		{ "question-mark", new[] { 0xe3e9, 0xe3ed } },
		{ "queue", new[] { 0xe6ac, 0xe6ad } },
		{ "quotes", new[] { 0xe660, 0xe661 } },
		{ "rabbit", new[] { 0xeac2, 0xeac3 } },
		{ "racquet", new[] { 0xee02, 0xee03 } },
		{ "radical", new[] { 0xe3ea, 0xe3ef } },
		{ "radio", new[] { 0xe77e, 0xe77f } },
		{ "radioactive", new[] { 0xe9dc, 0xe9dd } },
		{ "radio-button", new[] { 0xeb08, 0xeb09 } },
		{ "rainbow", new[] { 0xe598, 0xe599 } },
		{ "rainbow-cloud", new[] { 0xe59a, 0xe59b } },
		{ "ranking", new[] { 0xed62, 0xed63 } },
		{ "read-cv-logo", new[] { 0xed0c, 0xed0d } },
		{ "receipt", new[] { 0xe3ec, 0xe3f1 } },
		{ "receipt-x", new[] { 0xed40, 0xed41 } },
		{ "record", new[] { 0xe3ee, 0xe3f3 } },
		{ "rectangle", new[] { 0xe3f0, 0xe3f5 } },
		{ "rectangle-dashed", new[] { 0xe3f2, 0xe3f7 } },
		{ "recycle", new[] { 0xe75a, 0xe75b } },
		{ "reddit-logo", new[] { 0xe59c, 0xe59d } },
		{ "repeat", new[] { 0xe3f6, 0xe3f9 } },
		{ "repeat-once", new[] { 0xe3f8, 0xe3fb } },
		{ "replit-logo", new[] { 0xeb8a, 0xeb8b } },
		{ "resize", new[] { 0xed6e, 0xed6f } },
		{ "rewind", new[] { 0xe6a8, 0xe6a9 } },
		{ "rewind-circle", new[] { 0xe3fa, 0xe3fd } },
		{ "road-horizon", new[] { 0xe838, 0xe839 } },
		{ "robot", new[] { 0xe762, 0xe763 } },
		{ "rocket", new[] { 0xe3fc, 0xe3ff } },
		{ "rocket-launch", new[] { 0xe3fe, 0xe401 } },
		{ "rows", new[] { 0xe5a2, 0xe5a3 } },
		{ "rows-plus-bottom", new[] { 0xe59e, 0xe59f } },
		{ "rows-plus-top", new[] { 0xe5a0, 0xe5a1 } },
		{ "rss", new[] { 0xe400, 0xe403 } },
		{ "rss-simple", new[] { 0xe402, 0xe405 } },
		{ "rug", new[] { 0xea1a, 0xea1b } },
		{ "ruler", new[] { 0xe6b8, 0xe6b9 } },
		{ "sailboat", new[] { 0xe78a, 0xe78b } },
		{ "scales", new[] { 0xe750, 0xe751 } },
		{ "scan", new[] { 0xebb6, 0xebb7 } },
		{ "scan-smiley", new[] { 0xebb4, 0xebb5 } },
		{ "scissors", new[] { 0xeae0, 0xeae1 } },
		{ "scooter", new[] { 0xe820, 0xe821 } },
		{ "screencast", new[] { 0xe404, 0xe407 } },
		{ "screwdriver", new[] { 0xe86e, 0xe86f } },
		{ "scribble", new[] { 0xe806, 0xe807 } },
		{ "scribble-loop", new[] { 0xe662, 0xe663 } },
		{ "scroll", new[] { 0xeb7a, 0xeb7b } },
		{ "seal", new[] { 0xe604, 0xe605 } },
		{ "seal-check", new[] { 0xe606, 0xe607 } },
		{ "seal-percent", new[] { 0xe60a, 0xe60b } },
		{ "seal-question", new[] { 0xe608, 0xe609 } },
		{ "seal-warning", new[] { 0xe60c, 0xe60d } },
		{ "seat", new[] { 0xeb8e, 0xeb8f } },
		{ "seatbelt", new[] { 0xedfe, 0xedff } },
		{ "security-camera", new[] { 0xeca4, 0xeca5 } },
		{ "selection", new[] { 0xe69a, 0xe69b } },
		{ "selection-all", new[] { 0xe746, 0xe747 } },
		{ "selection-background", new[] { 0xeaf8, 0xeaf9 } },
		{ "selection-foreground", new[] { 0xeaf6, 0xeaf7 } },
		{ "selection-inverse", new[] { 0xe744, 0xe745 } },
		{ "selection-plus", new[] { 0xe69c, 0xe69d } },
		{ "selection-slash", new[] { 0xe69e, 0xe69f } },
		{ "shapes", new[] { 0xec5e, 0xec5f } },
		{ "share", new[] { 0xe406, 0xe409 } },
		{ "share-fat", new[] { 0xed52, 0xed57 } },
		{ "share-network", new[] { 0xe408, 0xe40b } },
		{ "shield", new[] { 0xe40a, 0xe40d } },
		{ "shield-check", new[] { 0xe40c, 0xe40f } },
		{ "shield-checkered", new[] { 0xe708, 0xe709 } },
		{ "shield-chevron", new[] { 0xe40e, 0xe411 } },
		{ "shield-plus", new[] { 0xe706, 0xe707 } },
		{ "shield-slash", new[] { 0xe410, 0xe413 } },
		{ "shield-star", new[] { 0xec34, 0xec35 } },
		{ "shield-warning", new[] { 0xe412, 0xe414 } },
		{ "shipping-container", new[] { 0xe78c, 0xe78d } },
		{ "shirt-folded", new[] { 0xea92, 0xea93 } },
		{ "shooting-star", new[] { 0xecfa, 0xecfb } },
		{ "shopping-bag", new[] { 0xe416, 0xe417 } },
		{ "shopping-bag-open", new[] { 0xe418, 0xe419 } },
		{ "shopping-cart", new[] { 0xe41e, 0xe41f } },
		{ "shopping-cart-simple", new[] { 0xe420, 0xe421 } },
		{ "shovel", new[] { 0xe9e6, 0xe9e7 } },
		{ "shower", new[] { 0xe776, 0xe777 } },
		{ "shrimp", new[] { 0xeab4, 0xeab5 } },
		{ "shuffle", new[] { 0xe422, 0xe423 } },
		{ "shuffle-angular", new[] { 0xe424, 0xe425 } },
		{ "shuffle-simple", new[] { 0xe426, 0xe427 } },
		{ "sidebar", new[] { 0xeab6, 0xeab7 } },
		{ "sidebar-simple", new[] { 0xec24, 0xec25 } },
		{ "sigma", new[] { 0xeab8, 0xeab9 } },
		{ "signature", new[] { 0xebac, 0xebad } },
		{ "sign-in", new[] { 0xe428, 0xe429 } },
		{ "sign-out", new[] { 0xe42a, 0xe42b } },
		{ "signpost", new[] { 0xe89c, 0xe89d } },
		{ "sim-card", new[] { 0xe664, 0xe665 } },
		{ "siren", new[] { 0xe9b8, 0xe9b9 } },
		{ "sketch-logo", new[] { 0xe42c, 0xe42d } },
		{ "skip-back", new[] { 0xe5a4, 0xe5a5 } },
		{ "skip-back-circle", new[] { 0xe42e, 0xe42f } },
		{ "skip-forward", new[] { 0xe5a6, 0xe5a7 } },
		{ "skip-forward-circle", new[] { 0xe430, 0xe431 } },
		{ "skull", new[] { 0xe916, 0xe917 } },
		{ "skype-logo", new[] { 0xe8dc, 0xe8dd } },
		{ "slack-logo", new[] { 0xe5a8, 0xe5a9 } },
		{ "sliders", new[] { 0xe432, 0xe433 } },
		{ "sliders-horizontal", new[] { 0xe434, 0xe435 } },
		{ "slideshow", new[] { 0xed32, 0xed33 } },
		{ "smiley", new[] { 0xe436, 0xe437 } },
		{ "smiley-angry", new[] { 0xec62, 0xec63 } },
		{ "smiley-blank", new[] { 0xe438, 0xe439 } },
		{ "smiley-meh", new[] { 0xe43a, 0xe43b } },
		{ "smiley-melting", new[] { 0xee56, 0xee57 } },
		{ "smiley-nervous", new[] { 0xe43c, 0xe43d } },
		{ "smiley-sad", new[] { 0xe43e, 0xe43f } },
		{ "smiley-sticker", new[] { 0xe440, 0xe441 } },
		{ "smiley-wink", new[] { 0xe666, 0xe667 } },
		{ "smiley-x-eyes", new[] { 0xe442, 0xe443 } },
		{ "snapchat-logo", new[] { 0xe668, 0xe669 } },
		{ "sneaker", new[] { 0xe80c, 0xe80d } },
		{ "sneaker-move", new[] { 0xed60, 0xed61 } },
		{ "snowflake", new[] { 0xe5aa, 0xe5ab } },
		{ "soccer-ball", new[] { 0xe716, 0xe717 } },
		{ "sock", new[] { 0xecce, 0xeccf } },
		{ "solar-panel", new[] { 0xed7a, 0xed7e } },
		{ "solar-roof", new[] { 0xed7b, 0xed7f } },
		{ "sort-ascending", new[] { 0xe444, 0xe445 } },
		{ "sort-descending", new[] { 0xe446, 0xe447 } },
		{ "soundcloud-logo", new[] { 0xe8de, 0xe8df } },
		{ "spade", new[] { 0xe448, 0xe449 } },
		{ "sparkle", new[] { 0xe6a2, 0xe6a3 } },
		{ "speaker-hifi", new[] { 0xea08, 0xea09 } },
		{ "speaker-high", new[] { 0xe44a, 0xe44b } },
		{ "speaker-low", new[] { 0xe44c, 0xe44d } },
		{ "speaker-none", new[] { 0xe44e, 0xe44f } },
		{ "speaker-simple-high", new[] { 0xe450, 0xe451 } },
		{ "speaker-simple-low", new[] { 0xe452, 0xe453 } },
		{ "speaker-simple-none", new[] { 0xe454, 0xe455 } },
		{ "speaker-simple-slash", new[] { 0xe456, 0xe457 } },
		{ "speaker-simple-x", new[] { 0xe458, 0xe459 } },
		{ "speaker-slash", new[] { 0xe45a, 0xe45b } },
		{ "speaker-x", new[] { 0xe45c, 0xe45d } },
		{ "speedometer", new[] { 0xee74, 0xee75 } },
		{ "sphere", new[] { 0xee66, 0xee67 } },
		{ "spinner", new[] { 0xe66a, 0xe66b } },
		{ "spinner-ball", new[] { 0xee28, 0xee29 } },
		{ "spinner-gap", new[] { 0xe66c, 0xe66d } },
		{ "spiral", new[] { 0xe9fa, 0xe9fb } },
		{ "split-horizontal", new[] { 0xe872, 0xe873 } },
		{ "split-vertical", new[] { 0xe876, 0xe877 } },
		{ "spotify-logo", new[] { 0xe66e, 0xe66f } },
		{ "spray-bottle", new[] { 0xe7e4, 0xe7e8 } },
		{ "square", new[] { 0xe45e, 0xe45f } },
		{ "square-half", new[] { 0xe462, 0xe463 } },
		{ "square-half-bottom", new[] { 0xeb16, 0xeb17 } },
		{ "square-logo", new[] { 0xe690, 0xe691 } },
		{ "squares-four", new[] { 0xe464, 0xe465 } },
		{ "square-split-horizontal", new[] { 0xe870, 0xe871 } },
		{ "square-split-vertical", new[] { 0xe874, 0xe875 } },
		{ "stack", new[] { 0xe466, 0xe467 } },
		{ "stack-minus", new[] { 0xedf4, 0xedf5 } },
		{ "stack-overflow-logo", new[] { 0xeb78, 0xeb79 } },
		{ "stack-plus", new[] { 0xedf6, 0xedf7 } },
		{ "stack-simple", new[] { 0xe468, 0xe469 } },
		{ "stairs", new[] { 0xe8ec, 0xe8ed } },
		{ "stamp", new[] { 0xea48, 0xea49 } },
		{ "standard-definition", new[] { 0xea90, 0xea91 } },
		{ "star", new[] { 0xe46a, 0xe46b } },
		{ "star-and-crescent", new[] { 0xecf4, 0xecf5 } },
		{ "star-four", new[] { 0xe6a4, 0xe6a5 } },
		{ "star-half", new[] { 0xe70a, 0xe70b } },
		{ "star-of-david", new[] { 0xe89e, 0xe89f } },
		{ "steam-logo", new[] { 0xead4, 0xead5 } },
		{ "steering-wheel", new[] { 0xe9ac, 0xe9ad } },
		{ "steps", new[] { 0xecbe, 0xecbf } },
		{ "stethoscope", new[] { 0xe7ea, 0xe7eb } },
		{ "sticker", new[] { 0xe5ac, 0xe5ad } },
		{ "stool", new[] { 0xea44, 0xea45 } },
		{ "stop", new[] { 0xe46c, 0xe46d } },
		{ "stop-circle", new[] { 0xe46e, 0xe46f } },
		{ "storefront", new[] { 0xe470, 0xe471 } },
		{ "strategy", new[] { 0xea3a, 0xea3b } },
		{ "stripe-logo", new[] { 0xe698, 0xe699 } },
		{ "student", new[] { 0xe73e, 0xe73f } },
		{ "subset-of", new[] { 0xedc0, 0xedc1 } },
		{ "subset-proper-of", new[] { 0xedb6, 0xedb7 } },
		{ "subtitles", new[] { 0xe1a8, 0xe1a9 } },
		{ "subtitles-slash", new[] { 0xe1a6, 0xe1a7 } },
		{ "subtract", new[] { 0xebd6, 0xebd7 } },
		{ "subtract-square", new[] { 0xebd4, 0xebd5 } },
		{ "subway", new[] { 0xe498, 0xe499 } },
		{ "suitcase", new[] { 0xe5ae, 0xe5af } },
		{ "suitcase-rolling", new[] { 0xe9b0, 0xe9b1 } },
		{ "suitcase-simple", new[] { 0xe5b0, 0xe5b1 } },
		{ "sun", new[] { 0xe472, 0xe473 } },
		{ "sun-dim", new[] { 0xe474, 0xe475 } },
		{ "sunglasses", new[] { 0xe816, 0xe817 } },
		{ "sun-horizon", new[] { 0xe5b6, 0xe5b7 } },
		{ "superset-of", new[] { 0xedb8, 0xedb9 } },
		{ "superset-proper-of", new[] { 0xedb4, 0xedb5 } },
		{ "swap", new[] { 0xe83c, 0xe83d } },
		{ "swatches", new[] { 0xe5b8, 0xe5b9 } },
		{ "swimming-pool", new[] { 0xecb6, 0xecb7 } },
		{ "sword", new[] { 0xe5ba, 0xe5bb } },
		{ "synagogue", new[] { 0xecec, 0xeced } },
		{ "syringe", new[] { 0xe968, 0xe969 } },
		{ "table", new[] { 0xe476, 0xe477 } },
		{ "tabs", new[] { 0xe778, 0xe779 } },
		{ "tag", new[] { 0xe478, 0xe479 } },
		{ "tag-chevron", new[] { 0xe672, 0xe673 } },
		{ "tag-simple", new[] { 0xe47a, 0xe47b } },
		{ "target", new[] { 0xe47c, 0xe47d } },
		{ "taxi", new[] { 0xe902, 0xe903 } },
		{ "tea-bag", new[] { 0xe8e6, 0xe8e7 } },
		{ "telegram-logo", new[] { 0xe5bc, 0xe5bd } },
		{ "television", new[] { 0xe754, 0xe755 } },
		{ "television-simple", new[] { 0xeae6, 0xeae7 } },
		{ "tennis-ball", new[] { 0xe720, 0xe721 } },
		{ "tent", new[] { 0xe8ba, 0xe8bb } },
		{ "terminal", new[] { 0xe47e, 0xe47f } },
		{ "terminal-window", new[] { 0xeae8, 0xeae9 } },
		{ "test-tube", new[] { 0xe7a0, 0xe7a1 } },
		{ "text-aa", new[] { 0xe6ee, 0xe6ef } },
		{ "text-align-center", new[] { 0xe480, 0xe481 } },
		{ "text-align-justify", new[] { 0xe482, 0xe483 } },
		{ "text-align-left", new[] { 0xe484, 0xe485 } },
		{ "text-align-right", new[] { 0xe486, 0xe487 } },
		{ "text-a-underline", new[] { 0xed34, 0xed35 } },
		{ "text-b", new[] { 0xe5be, 0xe5bf } },
		{ "text-bolder", new[] { 0xe5be, 0x0000 } },
		{ "textbox", new[] { 0xeb0a, 0xeb0b } },
		{ "text-columns", new[] { 0xec96, 0xec97 } },
		{ "text-h", new[] { 0xe6ba, 0xe6bb } },
		{ "text-h-five", new[] { 0xe6c4, 0xe6c5 } },
		{ "text-h-four", new[] { 0xe6c2, 0xe6c3 } },
		{ "text-h-one", new[] { 0xe6bc, 0xe6bd } },
		{ "text-h-six", new[] { 0xe6c6, 0xe6c7 } },
		{ "text-h-three", new[] { 0xe6c0, 0xe6c1 } },
		{ "text-h-two", new[] { 0xe6be, 0xe6bf } },
		{ "text-indent", new[] { 0xea1e, 0xea1f } },
		{ "text-italic", new[] { 0xe5c0, 0xe5c1 } },
		{ "text-outdent", new[] { 0xea1c, 0xea1d } },
		{ "text-strikethrough", new[] { 0xe5c2, 0xe5c3 } },
		{ "text-subscript", new[] { 0xec98, 0xec99 } },
		{ "text-superscript", new[] { 0xec9a, 0xec9b } },
		{ "text-t", new[] { 0xe48a, 0xe48b } },
		{ "text-t-slash", new[] { 0xe488, 0xe489 } },
		{ "text-underline", new[] { 0xe5c4, 0xe5c5 } },
		{ "thermometer", new[] { 0xe5c6, 0xe5c7 } },
		{ "thermometer-cold", new[] { 0xe5c8, 0xe5c9 } },
		{ "thermometer-hot", new[] { 0xe5ca, 0xe5cb } },
		{ "thermometer-simple", new[] { 0xe5cc, 0xe5cd } },
		{ "threads-logo", new[] { 0xed9e, 0xed9f } },
		{ "three-d", new[] { 0xea5a, 0xea5b } },
		{ "thumbs-down", new[] { 0xe48c, 0xe48d } },
		{ "thumbs-up", new[] { 0xe48e, 0xe48f } },
		{ "ticket", new[] { 0xe490, 0xe491 } },
		{ "tidal-logo", new[] { 0xed1c, 0xed1d } },
		{ "tiktok-logo", new[] { 0xeaf2, 0xeaf3 } },
		{ "tilde", new[] { 0xeda8, 0xeda9 } },
		{ "timer", new[] { 0xe492, 0xe493 } },
		{ "tipi", new[] { 0xed30, 0xed31 } },
		{ "tip-jar", new[] { 0xe7e2, 0xe7e9 } },
		{ "tire", new[] { 0xedd2, 0xedd3 } },
		{ "toggle-left", new[] { 0xe674, 0xe675 } },
		{ "toggle-right", new[] { 0xe676, 0xe677 } },
		{ "toilet", new[] { 0xe79a, 0xe79b } },
		{ "toilet-paper", new[] { 0xe79c, 0xe79d } },
		{ "toolbox", new[] { 0xeca0, 0xeca1 } },
		{ "tooth", new[] { 0xe9cc, 0xe9cd } },
		{ "tornado", new[] { 0xe88c, 0xe88d } },
		{ "tote", new[] { 0xe494, 0xe495 } },
		{ "tote-simple", new[] { 0xe678, 0xe679 } },
		{ "towel", new[] { 0xede6, 0xede7 } },
		{ "tractor", new[] { 0xec6e, 0xec6f } },
		{ "trademark", new[] { 0xe9f0, 0xe9f1 } },
		{ "trademark-registered", new[] { 0xe3f4, 0xe415 } },
		{ "traffic-cone", new[] { 0xe9a8, 0xe9a9 } },
		{ "traffic-sign", new[] { 0xe67a, 0xe67b } },
		{ "traffic-signal", new[] { 0xe9aa, 0xe9ab } },
		{ "train", new[] { 0xe496, 0xe497 } },
		{ "train-regional", new[] { 0xe49e, 0xe49f } },
		{ "train-simple", new[] { 0xe4a0, 0xe4a1 } },
		{ "tram", new[] { 0xe9ec, 0xe9ed } },
		{ "translate", new[] { 0xe4a2, 0xe4a3 } },
		{ "trash", new[] { 0xe4a6, 0xe4a7 } },
		{ "trash-simple", new[] { 0xe4a8, 0xe4a9 } },
		{ "tray", new[] { 0xe4aa, 0xe4ab } },
		{ "tray-arrow-down", new[] { 0xe010, 0xe011 } },
		{ "tray-arrow-up", new[] { 0xee52, 0xee53 } },
		{ "treasure-chest", new[] { 0xede2, 0xede3 } },
		{ "tree", new[] { 0xe6da, 0xe6db } },
		{ "tree-evergreen", new[] { 0xe6dc, 0xe6dd } },
		{ "tree-palm", new[] { 0xe91a, 0xe91b } },
		{ "tree-structure", new[] { 0xe67c, 0xe67d } },
		{ "tree-view", new[] { 0xee48, 0xee49 } },
		{ "trend-down", new[] { 0xe4ac, 0xe4ad } },
		{ "trend-up", new[] { 0xe4ae, 0xe4af } },
		{ "triangle", new[] { 0xe4b0, 0xe4b1 } },
		{ "triangle-dashed", new[] { 0xe4b2, 0xe4b3 } },
		{ "trolley", new[] { 0xe5b2, 0xe5b3 } },
		{ "trolley-suitcase", new[] { 0xe5b4, 0xe5b5 } },
		{ "trophy", new[] { 0xe67e, 0xe67f } },
		{ "truck", new[] { 0xe4b4, 0xe4b5 } },
		{ "truck-trailer", new[] { 0xe4b6, 0xe4b7 } },
		{ "t-shirt", new[] { 0xe670, 0xe671 } },
		{ "tumblr-logo", new[] { 0xe8d4, 0xe8d5 } },
		{ "twitch-logo", new[] { 0xe5ce, 0xe5cf } },
		{ "twitter-logo", new[] { 0xe4ba, 0xe4bb } },
		{ "umbrella", new[] { 0xe684, 0xe685 } },
		{ "umbrella-simple", new[] { 0xe686, 0xe687 } },
		{ "union", new[] { 0xedbe, 0xedbf } },
		{ "unite", new[] { 0xe87e, 0xe87f } },
		{ "unite-square", new[] { 0xe878, 0xe879 } },
		{ "upload", new[] { 0xe4be, 0xe4bf } },
		{ "upload-simple", new[] { 0xe4c0, 0xe4c1 } },
		{ "usb", new[] { 0xe956, 0xe957 } },
		{ "user", new[] { 0xe4c2, 0xe4c3 } },
		{ "user-check", new[] { 0xeafa, 0xeafb } },
		{ "user-circle", new[] { 0xe4c4, 0xe4c5 } },
		{ "user-circle-check", new[] { 0xec38, 0xec39 } },
		{ "user-circle-dashed", new[] { 0xec36, 0xec37 } },
		{ "user-circle-gear", new[] { 0xe4c6, 0xe4c7 } },
		{ "user-circle-minus", new[] { 0xe4c8, 0xe4c9 } },
		{ "user-circle-plus", new[] { 0xe4ca, 0xe4cb } },
		{ "user-focus", new[] { 0xe6fc, 0xe6fd } },
		{ "user-gear", new[] { 0xe4cc, 0xe4cd } },
		{ "user-list", new[] { 0xe73c, 0xe73d } },
		{ "user-minus", new[] { 0xe4ce, 0xe4cf } },
		{ "user-plus", new[] { 0xe4d0, 0xe4d1 } },
		{ "user-rectangle", new[] { 0xe4d2, 0xe4d3 } },
		{ "users", new[] { 0xe4d6, 0xe4d7 } },
		{ "users-four", new[] { 0xe68c, 0xe68d } },
		{ "user-sound", new[] { 0xeca8, 0xeca9 } },
		{ "user-square", new[] { 0xe4d4, 0xe4d5 } },
		{ "users-three", new[] { 0xe68e, 0xe68f } },
		{ "user-switch", new[] { 0xe756, 0xe757 } },
		{ "van", new[] { 0xe826, 0xe827 } },
		{ "vault", new[] { 0xe76e, 0xe76f } },
		{ "vector-three", new[] { 0xee62, 0xee63 } },
		{ "vector-two", new[] { 0xee64, 0xee65 } },
		{ "vibrate", new[] { 0xe4d8, 0xe4d9 } },
		{ "video", new[] { 0xe740, 0xe741 } },
		{ "video-camera", new[] { 0xe4da, 0xe4db } },
		{ "video-camera-slash", new[] { 0xe4dc, 0xe4dd } },
		{ "video-conference", new[] { 0xedce, 0xedcf } },
		{ "vignette", new[] { 0xeba2, 0xeba3 } },
		{ "vinyl-record", new[] { 0xecac, 0xecad } },
		{ "virtual-reality", new[] { 0xe7b8, 0xe7b9 } },
		{ "virus", new[] { 0xe7d6, 0xe7d7 } },
		{ "visor", new[] { 0xee2a, 0xee2b } },
		{ "voicemail", new[] { 0xe4de, 0xe4df } },
		{ "volleyball", new[] { 0xe726, 0xe727 } },
		{ "wall", new[] { 0xe688, 0xe689 } },
		{ "wallet", new[] { 0xe68a, 0xe68b } },
		{ "warehouse", new[] { 0xecd4, 0xecd5 } },
		{ "warning", new[] { 0xe4e0, 0xe4e1 } },
		{ "warning-circle", new[] { 0xe4e2, 0xe4e3 } },
		{ "warning-diamond", new[] { 0xe7fc, 0xe7fd } },
		{ "warning-octagon", new[] { 0xe4e4, 0xe4e5 } },
		{ "washing-machine", new[] { 0xede8, 0xede9 } },
		{ "watch", new[] { 0xe4e6, 0xe4e7 } },
		{ "waveform", new[] { 0xe802, 0xe803 } },
		{ "waveform-slash", new[] { 0xe800, 0xe801 } },
		{ "waves", new[] { 0xe6de, 0xe6df } },
		{ "wave-sawtooth", new[] { 0xea9c, 0xea9d } },
		{ "wave-sine", new[] { 0xea9a, 0xea9b } },
		{ "wave-square", new[] { 0xea9e, 0xea9f } },
		{ "wave-triangle", new[] { 0xeaa0, 0xeaa1 } },
		{ "webcam", new[] { 0xe9b2, 0xe9b3 } },
		{ "webcam-slash", new[] { 0xecdc, 0xecdd } },
		{ "webhooks-logo", new[] { 0xecae, 0xecaf } },
		{ "wechat-logo", new[] { 0xe8d2, 0xe8d3 } },
		{ "whatsapp-logo", new[] { 0xe5d0, 0xe5d1 } },
		{ "wheelchair", new[] { 0xe4e8, 0xe4e9 } },
		{ "wheelchair-motion", new[] { 0xe89a, 0xe89b } },
		{ "wifi-high", new[] { 0xe4ea, 0xe4eb } },
		{ "wifi-low", new[] { 0xe4ec, 0xe4ed } },
		{ "wifi-medium", new[] { 0xe4ee, 0xe4ef } },
		{ "wifi-none", new[] { 0xe4f0, 0x0000 } },
		{ "wifi-slash", new[] { 0xe4f2, 0xe4f3 } },
		{ "wifi-x", new[] { 0xe4f4, 0xe4f5 } },
		{ "wind", new[] { 0xe5d2, 0xe5d3 } },
		{ "windmill", new[] { 0xe9f8, 0xe9f9 } },
		{ "windows-logo", new[] { 0xe692, 0xe693 } },
		{ "wine", new[] { 0xe6b2, 0xe6b3 } },
		{ "wrench", new[] { 0xe5d4, 0xe5d5 } },
		{ "x", new[] { 0xe4f6, 0xe4f7 } },
		{ "x-circle", new[] { 0xe4f8, 0xe4f9 } },
		{ "x-logo", new[] { 0xe4bc, 0xe4bd } },
		{ "x-square", new[] { 0xe4fa, 0xe4fb } },
		{ "yarn", new[] { 0xed9a, 0xed9b } },
		{ "yin-yang", new[] { 0xe92a, 0xe92b } },
		{ "youtube-logo", new[] { 0xe4fc, 0xe4fd } },
	};

	/// <summary>解析渲染码点（Phosphor 各字重码点一致，Fill 实心渲染即用此码点）。</summary>
	public static bool TryResolve(string name, out int codepoint)
	{
		codepoint = 0;
		if (name == null || !Map.TryGetValue(name, out int[] pair) || pair[0] <= 0)
		{
			return false;
		}
		codepoint = pair[0];
		return true;
	}

	/// <summary>解析 Duotone 双层码点对（[0]=副层 :before，[1]=主层 :after）。历史接口保留；当前渲染已统一 Fill 单层，不再调用。</summary>
	public static bool TryResolveDuotone(string name, out int subCodepoint, out int mainCodepoint)
	{
		subCodepoint = 0;
		mainCodepoint = 0;
		if (name == null || !Map.TryGetValue(name, out int[] pair) || pair[0] <= 0 || pair[1] <= 0)
		{
			return false;
		}
		subCodepoint = pair[0];
		mainCodepoint = pair[1];
		return true;
	}
}
