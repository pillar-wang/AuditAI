using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace Auditai.UI.Platform;

/// <summary>
/// AuditAI 主题颜色 Token — 轻盈清新风
/// 与设计稿 auditai-redesign/index.html 的 CSS 变量一一对应
/// </summary>
public static class AuditTheme
{
	// ============================================================
	//  Surface 表面色
	// ============================================================
	public static readonly Color Surface = Color.FromArgb(255, 255, 255);
	public static readonly Color SurfaceMuted = Color.FromArgb(250, 251, 252);   // #fafbfc
	public static readonly Color SurfaceSubtle = Color.FromArgb(245, 247, 250);  // #f5f7fa
	public static readonly Color SurfaceHover = Color.FromArgb(237, 240, 245);   // #edf0f5 (悬停态)

	// ============================================================
	//  Text 文字色
	// ============================================================
	public static readonly Color Text = Color.FromArgb(30, 41, 59);            // #1e293b
	public static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);   // #475569
	public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);    // #64748b

	// ============================================================
	//  Border 边框色
	// ============================================================
	public static readonly Color Border = Color.FromArgb(229, 231, 235);       // #e5e7eb
	public static readonly Color BorderStrong = Color.FromArgb(209, 213, 219); // #d1d5db

	// ============================================================
	//  Brand — 清新蓝
	// ============================================================
	public static readonly Color Brand = Color.FromArgb(59, 130, 246);         // #3b82f6
	public static readonly Color BrandHover = Color.FromArgb(37, 99, 235);    // #2563eb
	public static readonly Color BrandActive = Color.FromArgb(29, 78, 216);    // #1d4ed8
	public static readonly Color BrandSubtle = Color.FromArgb(239, 246, 255);  // #eff6ff
	public static readonly Color BrandForeground = Color.White;

	// ============================================================
	//  State 状态色
	// ============================================================
	public static readonly Color SuccessText = Color.FromArgb(34, 197, 94);    // #22c55e
	public static readonly Color SuccessSubtle = Color.FromArgb(240, 253, 244); // #f0fdf4
	public static readonly Color SuccessBorder = Color.FromArgb(187, 247, 208); // #bbf7d0

	public static readonly Color ErrorText = Color.FromArgb(239, 68, 68);      // #ef4444
	public static readonly Color ErrorSubtle = Color.FromArgb(254, 242, 242);  // #fef2f2
	public static readonly Color ErrorBorder = Color.FromArgb(254, 202, 202);  // #fecaca

	public static readonly Color WarningText = Color.FromArgb(245, 158, 11);    // #f59e0b
	public static readonly Color WarningSubtle = Color.FromArgb(255, 251, 235); // #fffbeb
	public static readonly Color WarningBorder = Color.FromArgb(253, 230, 138); // #fde68a

	// ============================================================
	//  Extended 扩展语义色（商务色，图标体系 13 语义色扩展）
	// ============================================================
	public static readonly Color Navy = AuditTheme.TextSecondary;            // #334155 藏青
	public static readonly Color Indigo = Color.FromArgb(99, 102, 241);         // #6366f1 靛蓝
	public static readonly Color Teal = Color.FromArgb(20, 184, 166);          // #14b8a6 青碧
	public static readonly Color Amber = Color.FromArgb(245, 158, 11);           // #f59e0b 琥珀
	public static readonly Color Purple = Color.FromArgb(139, 92, 246);        // #8b5cf6 紫
	public static readonly Color Rose = Color.FromArgb(244, 63, 94);           // #f43f5e 绛红
	public static readonly Color Slate = Color.FromArgb(100, 116, 139);          // #64748b 石墨

	// ============================================================
	//  Chart 图表色
	// ============================================================
	public static readonly Color Chart1 = Color.FromArgb(59, 130, 246);        // #3b82f6 蓝
	public static readonly Color Chart2 = Color.FromArgb(34, 197, 94);         // #22c55e 绿
	public static readonly Color Chart3 = Color.FromArgb(250, 204, 21);         // #facc15 黄
	public static readonly Color Chart4 = Color.FromArgb(239, 68, 68);         // #ef4444 红
	public static readonly Color Chart5 = Color.FromArgb(139, 92, 246);        // #8b5cf6 紫

	// ============================================================
	//  Sidebar 侧边栏
	// ============================================================
	public static readonly Color SidebarBg = Color.FromArgb(250, 251, 252);    // #fafbfc
	public static readonly Color SidebarPrimary = Brand;
	public static readonly Color SidebarAccent = BrandSubtle;
	public static readonly Color SidebarBorder = Border;

	// ============================================================
	//  Gradient & Shadow 渐变与阴影
	// ============================================================
	public static readonly Color GradientStart = Color.FromArgb(249, 250, 251); // #f9fafb
	public static readonly Color GradientEnd = Color.FromArgb(255, 255, 255);   // #ffffff

	/// <summary>极淡阴影（Alpha=12）</summary>
	public static readonly Color ShadowLight = Color.FromArgb(12, 15, 23, 42);
	/// <summary>中等阴影（Alpha=20）</summary>
	public static readonly Color ShadowMedium = Color.FromArgb(20, 15, 23, 42);

	// ============================================================
	//  Typography 字体
	// ============================================================

	/// <summary>
	/// 优先使用 Microsoft YaHei UI（Win10+ 优化过 UI 内边距，更适合界面），
	/// 不可用时回退到微软雅黑。
	/// </summary>
	public static string FontFamilySans => _fontFamilySans ??= DetectSansFont();
	public static string FontFamilyMono => "Consolas";

	public static Font FontDefault => _fontDefault ??= new Font(FontFamilySans, 9.5f);
	public static Font FontDefaultBold => _fontDefaultBold ??= new Font(FontFamilySans, 9.5f, FontStyle.Bold);
	public static Font FontSmall => _fontSmall ??= new Font(FontFamilySans, 9f);
	public static Font FontSmallBold => _fontSmallBold ??= new Font(FontFamilySans, 9f, FontStyle.Bold);
	public static Font FontMono => _fontMono ??= new Font(FontFamilyMono, 9.5f);
	public static Font FontHeader => _fontHeader ??= new Font(FontFamilySans, 13f, FontStyle.Bold);

	private static string _fontFamilySans;
	private static Font _fontDefault;
	private static Font _fontDefaultBold;
	private static Font _fontSmall;
	private static Font _fontSmallBold;
	private static Font _fontMono;
	private static Font _fontHeader;

	private static string DetectSansFont()
	{
		try
		{
			using (InstalledFontCollection fonts = new InstalledFontCollection())
			{
				bool hasYaHeiUI = fonts.Families.Any(f => f.Name == "Microsoft YaHei UI");
				if (hasYaHeiUI) return "Microsoft YaHei UI";
			}
		}
		catch { /* 忽略任何检测异常，回退到默认字体 */ }
		return "微软雅黑";
	}

	// ============================================================
	//  Radius 圆角（WinForms 里用像素近似）
	// ============================================================
	public const int Radius = 8;
	public const int RadiusSm = 6;
	public const int RadiusLg = 12;

	// ============================================================
	//  Component Sizing 组件尺寸（清新风格规范）
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
	public const int Space10 = 40;

	// ============================================================
	//  Typography Scale 字体层级
	//  统一所有窗体的字号/字重，确保视觉层级一致
	//  7 级层级：Display → Title1 → Title2 → Body → BodyBold → Caption → Tiny
	// ============================================================

	/// <summary>页面大标题（16pt Bold，用于登录/注册等首屏标题）</summary>
	public static Font FontDisplay => _fontDisplay ??= new Font(FontFamilySans, 16f, FontStyle.Bold);

	/// <summary>一级标题（13pt Bold，用于弹窗/卡片主标题）</summary>
	public static Font FontTitle1 => _fontTitle1 ??= new Font(FontFamilySans, 13f, FontStyle.Bold);

	/// <summary>二级标题（11.5pt Bold，用于分组标题/次级标题）</summary>
	public static Font FontTitle => _fontTitle ??= new Font(FontFamilySans, 11.5f, FontStyle.Bold);

	/// <summary>正文/输入框文字（10pt Regular，最常用）</summary>
	public static Font FontBody => _fontBody ??= new Font(FontFamilySans, 10f);

	/// <summary>正文加粗（10pt Bold，按钮文字等）</summary>
	public static Font FontBodyBold => _fontBodyBold ??= new Font(FontFamilySans, 10f, FontStyle.Bold);

	/// <summary>辅助文字（9pt Regular，版本号、说明文字）</summary>
	public static Font FontCaption => _fontCaption ??= new Font(FontFamilySans, 9f);

	/// <summary>极小文字（8pt Regular，角标/状态文字/表格表头）</summary>
	public static Font FontTiny => _fontTiny ??= new Font(FontFamilySans, 8f);

	private static Font _fontDisplay;
	private static Font _fontTitle1;
	private static Font _fontTitle;
	private static Font _fontBody;
	private static Font _fontBodyBold;
	private static Font _fontCaption;
	private static Font _fontTiny;
}
