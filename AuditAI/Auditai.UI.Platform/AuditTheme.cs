using System.Drawing;

namespace Auditai.UI.Platform;

/// <summary>
/// AuditAI 主题颜色 Token — Google Blue 风格
/// 与设计稿 auditai-redesign/index.html 的 CSS 变量一一对应
/// </summary>
public static class AuditTheme
{
	// ============================================================
	//  Surface 表面色
	// ============================================================
	public static readonly Color Surface = Color.FromArgb(255, 255, 255);
	public static readonly Color SurfaceMuted = Color.FromArgb(248, 250, 252);   // #f8fafc
	public static readonly Color SurfaceSubtle = Color.FromArgb(241, 245, 249);  // #f1f5f9
	public static readonly Color SurfaceHover = Color.FromArgb(226, 232, 240);   // #e2e8f0 (悬停态)

	// ============================================================
	//  Text 文字色
	// ============================================================
	public static readonly Color Text = Color.FromArgb(15, 23, 42);            // #0f172a
	public static readonly Color TextSecondary = Color.FromArgb(51, 65, 85);   // #334155
	public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);    // #64748b

	// ============================================================
	//  Border 边框色
	// ============================================================
	public static readonly Color Border = Color.FromArgb(226, 232, 240);       // #e2e8f0
	public static readonly Color BorderStrong = Color.FromArgb(203, 213, 225); // #cbd5e1

	// ============================================================
	//  Brand — Google Blue
	// ============================================================
	public static readonly Color Brand = Color.FromArgb(26, 115, 232);         // #1a73e8
	public static readonly Color BrandHover = Color.FromArgb(23, 101, 204);    // #1765cc
	public static readonly Color BrandActive = Color.FromArgb(21, 87, 176);    // #1557b0
	public static readonly Color BrandSubtle = Color.FromArgb(232, 240, 254);  // #e8f0fe
	public static readonly Color BrandForeground = Color.White;

	// ============================================================
	//  State 状态色
	// ============================================================
	public static readonly Color SuccessText = Color.FromArgb(22, 163, 74);    // #16a34a
	public static readonly Color SuccessSubtle = Color.FromArgb(240, 253, 244); // #f0fdf4
	public static readonly Color SuccessBorder = Color.FromArgb(187, 247, 208); // #bbf7d0

	public static readonly Color ErrorText = Color.FromArgb(220, 38, 38);      // #dc2626
	public static readonly Color ErrorSubtle = Color.FromArgb(254, 242, 242);  // #fef2f2
	public static readonly Color ErrorBorder = Color.FromArgb(254, 202, 202);  // #fecaca

	public static readonly Color WarningText = Color.FromArgb(217, 119, 6);    // #d97706
	public static readonly Color WarningSubtle = Color.FromArgb(255, 251, 235); // #fffbeb
	public static readonly Color WarningBorder = Color.FromArgb(253, 230, 138); // #fde68a

	// ============================================================
	//  Chart 图表色（Google 四色）
	// ============================================================
	public static readonly Color Chart1 = Color.FromArgb(26, 115, 232);        // #1a73e8 蓝
	public static readonly Color Chart2 = Color.FromArgb(52, 168, 83);         // #34a853 绿
	public static readonly Color Chart3 = Color.FromArgb(251, 188, 4);         // #fbbc04 黄
	public static readonly Color Chart4 = Color.FromArgb(234, 67, 53);         // #ea4335 红
	public static readonly Color Chart5 = Color.FromArgb(147, 52, 230);        // #9334e6 紫

	// ============================================================
	//  Sidebar 侧边栏
	// ============================================================
	public static readonly Color SidebarBg = Color.FromArgb(248, 250, 252);    // #f8fafc
	public static readonly Color SidebarPrimary = Brand;
	public static readonly Color SidebarAccent = BrandSubtle;
	public static readonly Color SidebarBorder = Border;

	// ============================================================
	//  Typography 字体
	// ============================================================
	public static string FontFamilySans => "Microsoft YaHei UI";
	public static string FontFamilyMono => "Consolas";

	public static Font FontDefault => _fontDefault ??= new Font(FontFamilySans, 9f);
	public static Font FontDefaultBold => _fontDefaultBold ??= new Font(FontFamilySans, 9f, FontStyle.Bold);
	public static Font FontSmall => _fontSmall ??= new Font(FontFamilySans, 8.5f);
	public static Font FontSmallBold => _fontSmallBold ??= new Font(FontFamilySans, 8.5f, FontStyle.Bold);
	public static Font FontMono => _fontMono ??= new Font(FontFamilyMono, 9f);
	public static Font FontHeader => _fontHeader ??= new Font(FontFamilySans, 10f, FontStyle.Bold);

	private static Font _fontDefault;
	private static Font _fontDefaultBold;
	private static Font _fontSmall;
	private static Font _fontSmallBold;
	private static Font _fontMono;
	private static Font _fontHeader;

	// ============================================================
	//  Radius 圆角（WinForms 里用像素近似）
	// ============================================================
	public const int Radius = 8;
	public const int RadiusSm = 6;
	public const int RadiusLg = 12;

	// ============================================================
	//  Component Sizing 组件尺寸（Google 风格规范）
	//  所有窗体统一引用，确保视觉一致
	// ============================================================

	/// <summary>主按钮高度（40px，符合触控目标 ≥40px）</summary>
	public const int ButtonHeight = 40;

	/// <summary>次按钮高度</summary>
	public const int ButtonHeightSm = 36;

	/// <summary>按钮圆角半径（8px，高度的 20%）</summary>
	public const int ButtonRadius = 8;

	/// <summary>输入框高度（40px，与按钮同高）</summary>
	public const int InputHeight = 40;

	/// <summary>输入框圆角半径（6px，略小于按钮）</summary>
	public const int InputRadius = 6;

	/// <summary>卡片/弹窗圆角半径（12px）</summary>
	public const int CardRadius = 12;

	// ============================================================
	//  Spacing 间距（4px 网格系统）
	// ============================================================
	public const int Space1 = 4;
	public const int Space2 = 8;
	public const int Space3 = 12;
	public const int Space4 = 16;
	public const int Space5 = 20;
	public const int Space6 = 24;
	public const int Space8 = 32;

	// ============================================================
	//  Typography Scale 字体层级
	//  统一所有窗体的字号/字重，确保视觉层级一致
	// ============================================================

	/// <summary>页面大标题（14pt Bold，用于登录/注册等首屏标题）</summary>
	public static Font FontDisplay => _fontDisplay ??= new Font(FontFamilySans, 14f, FontStyle.Bold);

	/// <summary>卡片标题（12pt Bold，用于弹窗标题/分组标题）</summary>
	public static Font FontTitle => _fontTitle ??= new Font(FontFamilySans, 12f, FontStyle.Bold);

	/// <summary>正文/输入框文字（9.5pt Regular，最常用）</summary>
	public static Font FontBody => _fontBody ??= new Font(FontFamilySans, 9.5f);

	/// <summary>正文加粗（9.5pt Bold，按钮文字等）</summary>
	public static Font FontBodyBold => _fontBodyBold ??= new Font(FontFamilySans, 9.5f, FontStyle.Bold);

	/// <summary>辅助文字（8.5pt Regular，版本号、说明文字）</summary>
	public static Font FontCaption => _fontCaption ??= new Font(FontFamilySans, 8.5f);

	private static Font _fontDisplay;
	private static Font _fontTitle;
	private static Font _fontBody;
	private static Font _fontBodyBold;
	private static Font _fontCaption;
}
