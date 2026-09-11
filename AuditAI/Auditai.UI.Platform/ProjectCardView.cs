using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Auditai.DTO;
using Auditai.UI.Controls;
// DTO 与 System.Drawing 均有 Image 类型，本文件位图语义统一指 System.Drawing.Image
using Image = System.Drawing.Image;

namespace Auditai.UI.Platform;

/// <summary>
/// 项目卡片流视图库：项目卡片数据模型、单张卡片、分组卡片流容器、已收藏横向卡片行、KPI 统计条。
/// 全部自绘（TextRenderer + IconLibrary），主题色即时取自
/// Theme.SelectedAuditaiTheme.ThemeContext（DarkColor 动态取色）与 AuditTheme 静态 Token。
/// 供 FormProjectManage 等宿主接线使用。
/// </summary>

/// <summary>项目卡片流共享字体（进程级缓存，与 AuditTheme 字体同策略：常驻不随控件释放）。</summary>
internal static class CardFlowFonts
{
	/// <summary>项目名（8.5pt Bold：保证每行容纳更多字符，长名称完整显示）。</summary>
	public static readonly Font Title = new Font("微软雅黑", 8.5f, FontStyle.Bold);

	/// <summary>次要文字（编号/被审计单位/分组计数/收藏标题/KPI 标签，8.5pt）。</summary>
	public static readonly Font Secondary = new Font("微软雅黑", 8.5f);

	/// <summary>小号文字（类别药丸/成员头像/相对时间，8pt）。</summary>
	public static readonly Font Tiny = new Font("微软雅黑", 8f);

	/// <summary>分组头标题（9.5pt Bold）。</summary>
	public static readonly Font GroupTitle = new Font("微软雅黑", 9.5f, FontStyle.Bold);

	/// <summary>新建卡片文字（9.5pt）。</summary>
	public static readonly Font CreateTile = new Font("微软雅黑", 9.5f);

	/// <summary>KPI 数值（13pt Bold）。</summary>
	public static readonly Font KpiValue = new Font("微软雅黑", 13f, FontStyle.Bold);
}

/// <summary>项目卡片数据模型。</summary>
public class ProjectCardItem
{
	/// <summary>关联项目（"新建项目"特殊卡片为 null）。</summary>
	public Project Project { get; set; }

	/// <summary>是否已收藏（仅当容器 IsFavoriteEnabled 时绘制/响应）。</summary>
	public bool IsFavorite { get; set; }

	/// <summary>是否选中（由容器选择模型维护）。</summary>
	public bool IsSelected { get; set; }

	/// <summary>是否为"新建项目"特殊卡片（虚线边框 + 加号图标）。</summary>
	public bool IsCreateTile { get; set; }

	/// <summary>最近打开时间（null 则卡片底部显示"创建于 yyyy-MM-dd"）。</summary>
	public DateTime? OpenTime { get; set; }
}

/// <summary>单张自绘项目卡片（正方形 220×220，圆角白底 + 1px 边框 + 细微阴影，双缓冲）。</summary>
public class ProjectCard : Control
{
	/// <summary>卡片边长（正方形）。</summary>
	public const int CardWidth = 220;

	/// <summary>卡片边长（正方形，与宽度一致）。</summary>
	public const int CardHeight = 220;

	private const int Radius = 8;

	/// <summary>GDI 文本渲染格式（与 SideCommandBar 一致）。</summary>
	private const TextFormatFlags TextFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

	private const TextFormatFlags TextFlagsPlain = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

	private const TextFormatFlags TextFlagsCenter = TextFlags | TextFormatFlags.HorizontalCenter;

	private const TextFormatFlags TextFlagsRight = TextFlags | TextFormatFlags.Right;

	/// <summary>卡片标题多行格式：自动换行、末尾省略、顶部对齐（绘制用）。</summary>
	private const TextFormatFlags TextFlagsTitleMulti = TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

	/// <summary>卡片标题多行测量格式：同 WordBreak/NoPadding 但无 EndEllipsis，保证 MeasureText 返回完整换行后的实际高度（不被截断测量）。</summary>
	private const TextFormatFlags TextFlagsTitleMeasure = TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

	/// <summary>标题最多容纳的行数上限（3 行，8.5pt Bold 每行约 17px → 3×17=51px + 19px 余量 = 70px，底部两行有充足呼吸空间）。</summary>
	private const int TitleMaxHeight = 70;

	/// <summary>成员头像固定柔和色板（按名字字符和取色，白字可读）。</summary>
	private static readonly Color[] AvatarPalette = new Color[]
	{
		Color.FromArgb(127, 166, 217),
		Color.FromArgb(143, 199, 166),
		Color.FromArgb(232, 176, 136),
		Color.FromArgb(199, 155, 192),
		Color.FromArgb(181, 168, 221),
		Color.FromArgb(159, 208, 214),
		Color.FromArgb(217, 169, 162),
		Color.FromArgb(176, 183, 198)
	};

	private readonly ProjectCardItem _item;

	/// <summary>共享 ToolTip：仅当名称超长被省略时绑定，悬停显示完整名称兜底（进程级复用，不随卡片释放）。</summary>
	private static readonly ToolTip SharedTip = new ToolTip();

	/// <summary>是否已在 ToolTip 上注册过名称（避免每次 Paint 重复设置）。</summary>
	private bool _toolTipResolved;

	private bool _favoriteEnabled;

	private bool _hovered;

	private Rectangle _starHit;

	private Rectangle _moreRect;

	/// <summary>卡片数据。</summary>
	public ProjectCardItem Item
	{
		get
		{
			return _item;
		}
	}

	/// <summary>是否绘制/响应收藏星标（由容器注入）。</summary>
	public bool IsFavoriteEnabled
	{
		get
		{
			return _favoriteEnabled;
		}
		set
		{
			_favoriteEnabled = value;
			Invalidate();
		}
	}

	/// <summary>项目/模板类型图标解析委托（容器注入；返回的外部位图由宿主管理）。</summary>
	public Func<Project, Image> IconResolver { get; set; }

	/// <summary>类型强调色解析委托（用于卡片底色与边框；返回 Color.Empty 表示普通白卡）。</summary>
	public Func<Project, Color> TintResolver { get; set; }

	/// <summary>付费角标解析委托（返回角标位图或 null；外部位图由宿主管理）。</summary>
	public Func<Project, Image> PayBadgeResolver { get; set; }

	/// <summary>左键单击（非星标/更多命中区）时触发，供容器更新选择模型；"新建项目"卡片同样触发。</summary>
	public event Action Clicked;

	/// <summary>双击打开（"新建项目"卡片为单击触发）。</summary>
	public event Action Activated;

	/// <summary>点击收藏星标。</summary>
	public event Action StarClicked;

	/// <summary>点击悬停"更多"按钮（参数为屏幕坐标）。</summary>
	public event Action<Point> MoreClicked;

	/// <summary>右键单击（参数为屏幕坐标）。</summary>
	public event Action<Point> RightClicked;

	/// <summary>创建卡片。</summary>
	public ProjectCard(ProjectCardItem item)
	{
		_item = item ?? throw new ArgumentNullException("item");
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		Size = new Size(CardWidth, CardHeight);
		BackColor = AuditTheme.SurfaceMuted;
		Cursor = Cursors.Hand;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && _toolTipResolved)
		{
			// 解除共享 ToolTip 对卡片的引用，避免卡片随 SetGroups 反复重建时控件句柄/引用泄漏
			SharedTip.SetToolTip(this, "");
		}
		base.Dispose(disposing);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		_hovered = true;
		Invalidate();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		_hovered = false;
		Invalidate();
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (e.Button == MouseButtons.Right)
		{
			RightClicked?.Invoke(PointToScreen(e.Location));
			return;
		}
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		if (_favoriteEnabled && _starHit.Contains(e.Location))
		{
			StarClicked?.Invoke();
			return;
		}
		if (_hovered && _moreRect.Contains(e.Location))
		{
			MoreClicked?.Invoke(PointToScreen(e.Location));
			return;
		}
		Clicked?.Invoke();
		if (_item.IsCreateTile)
		{
			Activated?.Invoke();
		}
	}

	protected override void OnDoubleClick(EventArgs e)
	{
		base.OnDoubleClick(e);
		if (!_item.IsCreateTile)
		{
			Activated?.Invoke();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Color dark = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
		// 悬停整体上浮 2px（绘制 y-2，不用动画定时器）
		Rectangle body = new Rectangle(1, _hovered ? 1 : 3, Width - 2, Height - 6);
		_starHit = Rectangle.Empty;
		_moreRect = Rectangle.Empty;
		if (_item.IsCreateTile)
		{
			DrawCreateTile(g, body, dark);
			return;
		}
		Project p = _item.Project;
		if (p == null)
		{
			return;
		}
		DrawShadow(g, body, _hovered);
		DrawBody(g, body, dark, ResolveCardTint());
		// 右上角：状态徽标（26×26，规则同 CreateTileBadge）→ 收藏星标（20×20 命中区，视觉 16×16）→ 悬停"更多"（16×16）
		int rightX = body.Right - 8;
		if (TryGetBadge(p, out string badgeName, out Color badgeColor))
		{
			Rectangle badge = new Rectangle(rightX - 26, body.Y + 2, 26, 26);
			DrawBadge(g, badgeName, badgeColor, badge);
			rightX = badge.X - 2;
		}
		if (_favoriteEnabled)
		{
			Rectangle star = new Rectangle(rightX - 20, body.Y + 4, 20, 20);
			DrawStar(g, star);
			_starHit = star;
			rightX = star.X - 2;
		}
		Rectangle more = new Rectangle(rightX - 18, body.Y + 6, 16, 16);
		_moreRect = more;
		if (_hovered)
		{
			DrawMoreDots(g, more);
		}
		int padLeft = body.X + 11;
		// 第一行：类型图标 20×20 + 项目名（9.75f Bold，超长 EndEllipsis，占剩余宽度减星标/徽标/更多区）
		bool hasIcon = false;
		Image icon = ((IconResolver != null) ? IconResolver(p) : null);
		if (icon != null)
		{
			hasIcon = true;
			InterpolationMode mode = g.InterpolationMode;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(icon, new Rectangle(padLeft, body.Y + 4, 28, 28));
			g.InterpolationMode = mode;
		}
		int nameX = hasIcon ? padLeft + 34 : padLeft;
		int nameW = Math.Max(12, rightX - 6 - nameX);
		// 标题自适应：先按实际宽度测量完整名称所需高度（无 EndEllipsis，返回真实换行高度），
		// 再钳制到最多 3 行（TitleMaxHeight）。这样短名称紧凑、长名称完整显示、超长才省略。
		Size nameExt = TextRenderer.MeasureText(p.Name, CardFlowFonts.Title, new Size(nameW, int.MaxValue), TextFlagsTitleMeasure);
		int nameH = Math.Min(nameExt.Height, TitleMaxHeight);
		int nameTop = body.Y + 4;
		TextRenderer.DrawText(g, p.Name, CardFlowFonts.Title, new Rectangle(nameX, nameTop, nameW, nameH), AuditTheme.Text, TextFlagsTitleMulti);
		// 名称超长（完整高度超过分配高度，被省略）时，悬停 ToolTip 显示完整名称兜底
		if (!_toolTipResolved)
		{
			_toolTipResolved = true;
			if (nameExt.Height > nameH)
			{
				SharedTip.SetToolTip(this, p.Name);
			}
		}
		// 第二行：编号（8.5f Slate 系次要色）+ 类别药丸（距名称底部 6px，随名称行数浮动）
		int row2Y = nameTop + nameH + 6;
		int x = padLeft;
		if (!string.IsNullOrEmpty(p.Number))
		{
			TextRenderer.DrawText(g, p.Number, CardFlowFonts.Secondary, new Rectangle(x, row2Y, body.Right - 10 - x, 16), AuditTheme.TextMuted, TextFlags);
			x += TextRenderer.MeasureText(p.Number, CardFlowFonts.Secondary, new Size(int.MaxValue, int.MaxValue), TextFlagsPlain).Width;
		}
		DrawCategoryPill(g, p, x + 8, row2Y, body.Right - 10, dark);
		// 第三行：被审计单位（可选，row2 底部后 14px 起，高 46px 可容纳两行半）
		bool hasAuditee = !string.IsNullOrWhiteSpace(p.Auditee);
		int auditeeTop = row2Y + 30;
		if (hasAuditee)
		{
			TextRenderer.DrawText(g, "被审计单位：" + p.Auditee, CardFlowFonts.Secondary, new Rectangle(padLeft, auditeeTop, body.Right - 10 - padLeft, 46), AuditTheme.Slate, TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
		}
		// 第四行：成员头像组 + 右下角相对时间（固定在 body 底部上方 28px，给上面区块留足空间）
		int row4Y = body.Y + 186;
		DrawMembers(g, p, new Rectangle(padLeft, row4Y, 0, 18), body.Right - 10);
		TextRenderer.DrawText(g, ResolveTimeText(), CardFlowFonts.Tiny, new Rectangle(padLeft, row4Y + 1, body.Right - 10 - padLeft, 16), AuditTheme.TextMuted, TextFlagsRight);
		// 付费角标（左上角）
		Image pay = ((PayBadgeResolver != null) ? PayBadgeResolver(p) : null);
		if (pay != null)
		{
			g.DrawImage(pay, new Rectangle(4, 4, 26, 26));
		}
	}

	/// <summary>状态徽标规则（照 CreateProjectTile/CreateTileBadge：已归档优先，其次 ReviewStatus 1/2/3，0 不显示）。</summary>
	private static bool TryGetBadge(Project p, out string iconName, out Color color)
	{
		if (p.IsArchived)
		{
			iconName = "archive";
			color = AuditTheme.Slate;
			return true;
		}
		switch (p.ReviewStatus)
		{
		case 1:
			iconName = "clock-counter-clockwise";
			color = AuditTheme.Brand;
			return true;
		case 2:
			iconName = "check-circle";
			color = AuditTheme.SuccessText;
			return true;
		case 3:
			iconName = "arrow-u-up-left";
			color = AuditTheme.ErrorText;
			return true;
		}
		iconName = null;
		color = default(Color);
		return false;
	}

	/// <summary>绘制右上角状态徽标（沿用 CreateTileBadge 手法：20px 字形 + 3px 外扩；位图用完即释放，不持有）。</summary>
	private static void DrawBadge(Graphics g, string iconName, Color color, Rectangle rect26)
	{
		using Bitmap glyph = IconLibrary.CreateBitmap(iconName, 20, color, IconLibrary.StyleFill);
		g.DrawImage(glyph, rect26.X + 3, rect26.Y + 3, 20, 20);
	}

	/// <summary>绘制收藏星标（收藏实星 WarningText；未收藏空心灰星矢量描边）。</summary>
	private void DrawStar(Graphics g, Rectangle hitRect)
	{
		Rectangle visual = new Rectangle(hitRect.X + 2, hitRect.Y + 2, 16, 16);
		if (_item.IsFavorite)
		{
			using Bitmap bmp = IconLibrary.CreateBitmap("star", 16, AuditTheme.WarningText, IconLibrary.StyleFill);
			g.DrawImage(bmp, visual);
			return;
		}
		DrawStarOutline(g, visual, AuditTheme.BorderStrong);
	}

	/// <summary>空心五角星（IconLibrary 为 Fill 实心字体，空心星用 GraphicsPath 多边形自绘）。</summary>
	private static void DrawStarOutline(Graphics g, Rectangle r, Color color)
	{
		double cx = r.X + r.Width / 2.0;
		double cy = r.Y + r.Height / 2.0;
		double ro = r.Width / 2.0;
		double ri = ro * 0.45;
		PointF[] pts = new PointF[10];
		for (int i = 0; i < 10; i++)
		{
			double ang = -Math.PI / 2.0 + i * Math.PI / 5.0;
			double rad = (i % 2 == 0) ? ro : ri;
			pts[i] = new PointF((float)(cx + rad * Math.Cos(ang)), (float)(cy + rad * Math.Sin(ang)));
		}
		using Pen pen = new Pen(color);
		g.DrawPolygon(pen, pts);
	}

	/// <summary>悬停"更多"按钮（16px 垂直三点，自绘圆点）。</summary>
	private static void DrawMoreDots(Graphics g, Rectangle r)
	{
		using SolidBrush brush = new SolidBrush(AuditTheme.Slate);
		int cx = r.X + r.Width / 2 - 1;
		for (int i = 0; i < 3; i++)
		{
			g.FillEllipse(brush, cx, r.Y + 3 + i * 5, 2, 2);
		}
	}

	/// <summary>类别药丸（圆角 8 高 16，浅色底 DarkColor alpha 20、文字 8f DarkColor；无类别省略，多类别取 '|' 首段）。</summary>
	private static void DrawCategoryPill(Graphics g, Project p, int x, int y, int limitRight, Color dark)
	{
		string category = ((p.Category ?? "").Split('|')[0] ?? "").Trim();
		if (category.Length == 0)
		{
			return;
		}
		int tw = TextRenderer.MeasureText(category, CardFlowFonts.Tiny, new Size(int.MaxValue, int.MaxValue), TextFlagsPlain).Width;
		Rectangle pill = new Rectangle(x, y, tw + 14, 16);
		if (tw <= 0 || pill.Right > limitRight)
		{
			return;
		}
		using GraphicsPath path = CreateRounded(pill, 8);
		using SolidBrush back = new SolidBrush(Color.FromArgb(20, dark));
		g.FillPath(back, path);
		TextRenderer.DrawText(g, category, CardFlowFonts.Tiny, pill, dark, TextFlagsCenter);
	}

	/// <summary>成员头像组：前 3 个 User.Name 首字圆徽（18px，白字 8f），超出显示"+N"灰圆。</summary>
	private void DrawMembers(Graphics g, Project p, Rectangle row, int limitRight)
	{
		IEnumerable<User> source = p.Users ?? Enumerable.Empty<User>();
		List<User> all = source.Where((User u) => u != null && !string.IsNullOrEmpty(u.Name)).ToList();
		int shown = Math.Min(3, all.Count);
		int x = row.X;
		for (int i = 0; i < shown; i++)
		{
			DrawAvatar(g, new Rectangle(x, row.Y, 18, 18), all[i].Name);
			x += 22;
		}
		if (all.Count > 3 && x + 18 <= limitRight)
		{
			Rectangle r = new Rectangle(x, row.Y, 18, 18);
			using SolidBrush back = new SolidBrush(AuditTheme.Border);
			g.FillEllipse(back, r);
			TextRenderer.DrawText(g, "+" + (all.Count - 3), CardFlowFonts.Tiny, r, AuditTheme.Slate, TextFlagsCenter);
		}
	}

	/// <summary>单个首字圆徽（圆底色按名字字符和从固定柔和色板取）。</summary>
	private static void DrawAvatar(Graphics g, Rectangle r, string name)
	{
		string text = (name ?? "").Trim();
		string letter = (text.Length > 0) ? text.Substring(0, 1) : "?";
		int sum = 0;
		foreach (char c in text)
		{
			sum += c;
		}
		Color bg = AvatarPalette[Math.Abs(sum) % AvatarPalette.Length];
		using SolidBrush brush = new SolidBrush(bg);
		g.FillEllipse(brush, r);
		TextRenderer.DrawText(g, letter, CardFlowFonts.Tiny, r, Color.White, TextFlagsCenter);
	}

	/// <summary>右下角时间文案：有 OpenTime 显示相对时间（X分钟/小时/天前打开、昨天 HH:mm、yyyy-MM-dd），否则"创建于 yyyy-MM-dd"。</summary>
	private string ResolveTimeText()
	{
		if (_item.OpenTime.HasValue)
		{
			DateTime open = _item.OpenTime.Value;
			TimeSpan ts = DateTime.Now - open;
			if (ts.TotalMinutes < 60)
			{
				return string.Format("{0}分钟前打开", Math.Max(1, (int)ts.TotalMinutes));
			}
			if (ts.TotalHours < 24)
			{
				return string.Format("{0}小时前打开", (int)ts.TotalHours);
			}
			if (open.Date == DateTime.Today.AddDays(-1.0))
			{
				return string.Format("昨天 {0:HH:mm}", open);
			}
			if (ts.TotalDays < 7)
			{
				return string.Format("{0}天前打开", (int)ts.TotalDays);
			}
			return string.Format("{0:yyyy-MM-dd}", open);
		}
		Project p = _item.Project;
		return string.Format("创建于 {0:yyyy-MM-dd}", (p != null) ? p.CreateTime : DateTime.Now);
	}

	/// <summary>解析当前卡片类型强调色（TintResolver 未设置或返回 Color.Empty 时为白卡）。</summary>
	private Color ResolveCardTint()
	{
		if (TintResolver == null || _item.Project == null)
		{
			return Color.Empty;
		}
		return TintResolver(_item.Project);
	}

	/// <summary>卡片体（白底或类型淡彩底 + 选中 tint DarkColor/Tint alpha 14 + 边框：选中 2px / 悬停 / 普通 Border 或 Tint alpha 110）。</summary>
	private void DrawBody(Graphics g, Rectangle body, Color dark, Color tint)
	{
		bool hasTint = !tint.IsEmpty;
		Color fill = Color.White;
		if (hasTint)
		{
			// 类型淡彩底：tint 向白色混合 90%
			fill = Color.FromArgb(255,
				tint.R + (255 - tint.R) * 9 / 10,
				tint.G + (255 - tint.G) * 9 / 10,
				tint.B + (255 - tint.B) * 9 / 10);
		}
		using GraphicsPath path = CreateRounded(body, Radius);
		using (SolidBrush white = new SolidBrush(fill))
		{
			g.FillPath(white, path);
		}
		if (_item.IsSelected)
		{
			Color sel = hasTint ? tint : dark;
			using SolidBrush seltint = new SolidBrush(Color.FromArgb(14, sel));
			g.FillPath(seltint, path);
			using Pen pen = new Pen(sel, 2f);
			g.DrawPath(pen, path);
		}
		else if (_hovered)
		{
			using Pen pen = new Pen(hasTint ? Color.FromArgb(170, tint) : dark);
			g.DrawPath(pen, path);
		}
		else
		{
			using Pen pen = new Pen(hasTint ? Color.FromArgb(110, tint) : AuditTheme.Border);
			g.DrawPath(pen, path);
		}
	}

	/// <summary>细微阴影（悬停加深）。</summary>
	private static void DrawShadow(Graphics g, Rectangle body, bool hovered)
	{
		int alpha = hovered ? 34 : 16;
		using (SolidBrush s1 = new SolidBrush(Color.FromArgb(alpha, 15, 23, 42)))
		{
			g.FillRectangle(s1, body.X + 4, body.Bottom, body.Width - 8, 2);
		}
		using (SolidBrush s2 = new SolidBrush(Color.FromArgb(alpha / 2, 15, 23, 42)))
		{
			g.FillRectangle(s2, body.X + 8, body.Bottom + 2, body.Width - 16, 1);
		}
	}

	/// <summary>"新建项目"特殊卡片：虚线边框（DarkColor alpha 100）+ 中心 24px plus 图标 + 居中文字 9.5f DarkColor。</summary>
	private void DrawCreateTile(Graphics g, Rectangle body, Color dark)
	{
		using GraphicsPath path = CreateRounded(body, Radius);
		using (SolidBrush white = new SolidBrush(Color.White))
		{
			g.FillPath(white, path);
		}
		using (Pen pen = _item.IsSelected
			? new Pen(dark, 2f)
			: new Pen(Color.FromArgb(_hovered ? 190 : 100, dark)) { DashStyle = DashStyle.Dash })
		{
			g.DrawPath(pen, path);
		}
		using Bitmap plus = IconLibrary.CreateBitmap("plus", 24, dark, IconLibrary.StyleFill);
		// 图标+文字整体在卡内垂直居中（图标 24 → 文字 22，间距 8）
		int blockH = 24 + 8 + 22;
		int centY = body.Y + (body.Height - blockH) / 2;
		g.DrawImage(plus, body.X + (body.Width - 24) / 2, centY, 24, 24);
		Rectangle textRect = new Rectangle(body.X, centY + 32, body.Width, 22);
		TextRenderer.DrawText(g, "新建项目", CardFlowFonts.CreateTile, textRect, dark, TextFlagsCenter);
	}

	/// <summary>圆角矩形路径（与 SideCommandBar 同手法）。</summary>
	private static GraphicsPath CreateRounded(Rectangle rect, int radius)
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

/// <summary>分组卡片流容器（AutoScroll，分组头由容器自绘并做命中测试，卡片为子控件手动换行排列）。</summary>
public class ProjectCardFlow : Panel
{
	/// <summary>分组头高度。</summary>
	public const int GroupHeadHeight = 34;

	/// <summary>卡片间距（水平/垂直）。</summary>
	public const int CardGap = 12;

	/// <summary>组间空行。</summary>
	public const int GroupGap = 8;

	private const TextFormatFlags TextFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

	private const TextFormatFlags TextFlagsPlain = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

	private sealed class GroupState
	{
		public readonly string Title;

		public readonly List<ProjectCardItem> Items;

		public readonly List<ProjectCard> Cards = new List<ProjectCard>();

		public bool Collapsed;

		public GroupState(string title, List<ProjectCardItem> items)
		{
			Title = title;
			Items = items;
		}
	}

	private sealed class GroupHead
	{
		public readonly GroupState Group;

		public readonly Rectangle Rect;

		public GroupHead(GroupState group, Rectangle rect)
		{
			Group = group;
			Rect = rect;
		}
	}

	private readonly List<GroupState> _groups = new List<GroupState>();

	private readonly List<GroupHead> _headRects = new List<GroupHead>();

	private readonly HashSet<string> _collapsed = new HashSet<string>();

	private readonly List<ProjectCardItem> _selected = new List<ProjectCardItem>();

	private int _hoverHeadIndex = -1;

	private bool _layouting;

	/// <summary>分组数据（分组头非子控件，由容器自绘）。</summary>
	public class CardGroupData
	{
		/// <summary>组名（空则不绘制分组头、不可折叠）。</summary>
		public string Title { get; set; }

		/// <summary>组内卡片数据。</summary>
		public IList<ProjectCardItem> Items { get; set; }
	}

	/// <summary>项目/模板类型图标解析委托（转发给每张卡片）。</summary>
	public Func<Project, Image> IconResolver { get; set; }

	/// <summary>类型强调色解析委托（转发给每张卡片；返回 Color.Empty 表示普通白卡）。</summary>
	public Func<Project, Color> TintResolver { get; set; }

	/// <summary>付费角标解析委托（转发给每张卡片）。</summary>
	public Func<Project, Image> PayBadgeResolver { get; set; }

	/// <summary>是否绘制/响应收藏星标。</summary>
	public bool IsFavoriteEnabled { get; set; }

	/// <summary>是否允许多选（回收站用多选，其他单选；单选时单击独占选中）。</summary>
	public bool AllowMultiSelect { get; set; }

	/// <summary>双击/单击打开（转发自卡片 Activated）。</summary>
	public event Action<ProjectCardItem> ItemActivated;

	/// <summary>卡片右键（参数为屏幕坐标）。</summary>
	public event Action<ProjectCardItem, Point> ItemRightClicked;

	/// <summary>卡片悬停"更多"点击（参数为屏幕坐标）。</summary>
	public event Action<ProjectCardItem, Point> ItemMoreClicked;

	/// <summary>卡片收藏星标点击。</summary>
	public event Action<ProjectCardItem> ItemStarClicked;

	/// <summary>选中集合变化。</summary>
	public event Action SelectionChanged;

	/// <summary>创建分组卡片流容器。</summary>
	public ProjectCardFlow()
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		DoubleBuffered = true;
		AutoScroll = true;
		Padding = new Padding(16);
		BackColor = AuditTheme.SurfaceMuted;
	}

	/// <summary>单选视角下的当前选中项目（无选中为 null）。</summary>
	public Project SelectedProject
	{
		get
		{
			ProjectCardItem item = _selected.FirstOrDefault((ProjectCardItem i) => i.Project != null);
			return (item != null) ? item.Project : null;
		}
	}

	/// <summary>当前选中项目列表（多选时含多项）。</summary>
	public List<Project> SelectedProjects
	{
		get
		{
			return _selected.Where((ProjectCardItem i) => i.Project != null).Select((ProjectCardItem i) => i.Project).ToList();
		}
	}

	/// <summary>清空选择（有选中时触发 SelectionChanged）。</summary>
	public void ClearSelection()
	{
		if (_selected.Count == 0)
		{
			return;
		}
		foreach (ProjectCardItem item in _selected)
		{
			item.IsSelected = false;
		}
		_selected.Clear();
		RefreshSelectionVisual();
		SelectionChanged?.Invoke();
	}

	/// <summary>重建卡片子控件（折叠状态按组名保留；卡片内位图用 using 绘制不持有，无需特殊释放）。</summary>
	public void SetGroups(IEnumerable<CardGroupData> groups)
	{
		SuspendLayout();
		try
		{
			foreach (ProjectCard old in Controls.OfType<ProjectCard>().ToList())
			{
				Controls.Remove(old);
				old.Dispose();
			}
			_groups.Clear();
			_headRects.Clear();
			_selected.Clear();
			foreach (CardGroupData data in groups ?? Enumerable.Empty<CardGroupData>())
			{
				if (data == null)
				{
					continue;
				}
				List<ProjectCardItem> items = ((data.Items != null) ? data.Items.Where((ProjectCardItem i) => i != null).ToList() : new List<ProjectCardItem>());
				GroupState group = new GroupState(data.Title, items);
				foreach (ProjectCardItem item in items)
				{
					ProjectCard card = CreateCard(item);
					group.Cards.Add(card);
					Controls.Add(card);
				}
				if (!string.IsNullOrEmpty(group.Title) && _collapsed.Contains(group.Title))
				{
					group.Collapsed = true;
				}
				_groups.Add(group);
			}
			Relayout();
			Invalidate();
		}
		finally
		{
			ResumeLayout(false);
		}
	}

	/// <summary>刷新所有卡片主题色（卡片即时取 Theme.SelectedAuditaiTheme，触发全部重绘即可）。</summary>
	public void ApplyTheme()
	{
		foreach (GroupState group in _groups)
		{
			foreach (ProjectCard card in group.Cards)
			{
				card.BackColor = BackColor;
				card.Invalidate();
			}
		}
		Invalidate();
	}

	private ProjectCard CreateCard(ProjectCardItem item)
	{
		ProjectCard card = new ProjectCard(item)
		{
			IconResolver = IconResolver,
			TintResolver = TintResolver,
			PayBadgeResolver = PayBadgeResolver,
			IsFavoriteEnabled = IsFavoriteEnabled,
			BackColor = BackColor
		};
		card.Clicked += delegate
		{
			if (!item.IsCreateTile)
			{
				HandleSelect(item);
			}
		};
		card.Activated += delegate
		{
			ItemActivated?.Invoke(item);
		};
		card.StarClicked += delegate
		{
			ItemStarClicked?.Invoke(item);
		};
		card.RightClicked += delegate (Point pt)
		{
			ItemRightClicked?.Invoke(item, pt);
		};
		card.MoreClicked += delegate (Point pt)
		{
			ItemMoreClicked?.Invoke(item, pt);
		};
		return card;
	}

	private void HandleSelect(ProjectCardItem item)
	{
		if (AllowMultiSelect)
		{
			if (_selected.Contains(item))
			{
				_selected.Remove(item);
				item.IsSelected = false;
			}
			else
			{
				_selected.Add(item);
				item.IsSelected = true;
			}
		}
		else
		{
			foreach (ProjectCardItem old in _selected)
			{
				old.IsSelected = false;
			}
			_selected.Clear();
			_selected.Add(item);
			item.IsSelected = true;
		}
		RefreshSelectionVisual();
		SelectionChanged?.Invoke();
	}

	private void RefreshSelectionVisual()
	{
		foreach (GroupState group in _groups)
		{
			foreach (ProjectCard card in group.Cards)
			{
				card.Invalidate();
			}
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		Relayout();
	}

	protected override void OnPaddingChanged(EventArgs e)
	{
		base.OnPaddingChanged(e);
		Relayout();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		// 分组头命中测试（内容坐标）：点击切换折叠
		Point off = AutoScrollPosition;
		Point content = new Point(e.X - off.X, e.Y - off.Y);
		foreach (GroupHead head in _headRects)
		{
			if (head.Rect.Contains(content))
			{
				head.Group.Collapsed = !head.Group.Collapsed;
				if (head.Group.Collapsed)
				{
					_collapsed.Add(head.Group.Title);
				}
				else
				{
					_collapsed.Remove(head.Group.Title);
				}
				Relayout();
				Invalidate();
				return;
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		Point off = AutoScrollPosition;
		Point content = new Point(e.X - off.X, e.Y - off.Y);
		int index = -1;
		for (int i = 0; i < _headRects.Count; i++)
		{
			if (_headRects[i].Rect.Contains(content))
			{
				index = i;
				break;
			}
		}
		if (index != _hoverHeadIndex)
		{
			_hoverHeadIndex = index;
			Cursor = ((index >= 0) ? Cursors.Hand : Cursors.Default);
			Invalidate();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (_hoverHeadIndex >= 0)
		{
			_hoverHeadIndex = -1;
			Cursor = Cursors.Default;
			Invalidate();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (_headRects.Count == 0)
		{
			return;
		}
		Graphics g = e.Graphics;
		Color dark = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
		Point off = AutoScrollPosition;
		for (int i = 0; i < _headRects.Count; i++)
		{
			GroupHead head = _headRects[i];
			Rectangle r = new Rectangle(head.Rect.X + off.X, head.Rect.Y + off.Y, head.Rect.Width, head.Rect.Height);
			if (r.Bottom < 0 || r.Top > Height)
			{
				continue;
			}
			int count = head.Group.Items.Count;
			TextRenderer.DrawText(g, head.Group.Title, CardFlowFonts.GroupTitle, r, dark, TextFlags);
			int titleW = TextRenderer.MeasureText(head.Group.Title, CardFlowFonts.GroupTitle, new Size(int.MaxValue, int.MaxValue), TextFlagsPlain).Width;
			TextRenderer.DrawText(g, "(" + count + ")", CardFlowFonts.Secondary, new Rectangle(r.X + titleW + 4, r.Y, 80, r.Height), AuditTheme.Slate, TextFlagsPlain);
			// 自绘 12px chevron：展开朝下、折叠朝右
			DrawChevron(g, new Rectangle(r.Right - 18, r.Y + (GroupHeadHeight - 12) / 2, 12, 12), head.Group.Collapsed, AuditTheme.Slate);
		}
	}

	/// <summary>自绘折叠 chevron（线条矢量）。</summary>
	private static void DrawChevron(Graphics g, Rectangle r, bool collapsed, Color color)
	{
		using Pen pen = new Pen(color, 1.6f)
		{
			StartCap = LineCap.Round,
			EndCap = LineCap.Round
		};
		int cx = r.X + r.Width / 2;
		int cy = r.Y + r.Height / 2;
		if (collapsed)
		{
			g.DrawLine(pen, cx - 2, cy - 4, cx + 3, cy);
			g.DrawLine(pen, cx + 3, cy, cx - 2, cy + 4);
		}
		else
		{
			g.DrawLine(pen, cx - 4, cy - 2, cx, cy + 3);
			g.DrawLine(pen, cx, cy + 3, cx + 4, cy - 2);
		}
	}

	/// <summary>手动排列：分组头（34px）→ 卡片按宽度换行 → 组间空行 8px；总内容高度写入 AutoScrollMinSize。</summary>
	private void Relayout()
	{
		if (_layouting)
		{
			return;
		}
		_layouting = true;
		try
		{
			int avail = Width - Padding.Horizontal;
			int contentH = LayoutPass(avail, false);
			// 垂直滚动条出现会压缩可用宽度，预估重排一次
			if (contentH > ClientSize.Height)
			{
				avail -= SystemInformation.VerticalScrollBarWidth;
				contentH = LayoutPass(avail, false);
			}
			contentH = LayoutPass(avail, true);
			AutoScrollMinSize = new Size(0, contentH + Padding.Bottom);
		}
		finally
		{
			_layouting = false;
		}
	}

	/// <summary>单趟布局（apply=false 仅试算内容高度，不落子控件位置）。</summary>
	private int LayoutPass(int avail, bool apply)
	{
		int cardW = ProjectCard.CardWidth;
		int cardH = ProjectCard.CardHeight;
		int cols = Math.Max(1, (avail + CardGap) / (cardW + CardGap));
		int originX = Padding.Left;
		int y = Padding.Top;
		if (apply)
		{
			_headRects.Clear();
		}
		foreach (GroupState group in _groups)
		{
			bool hasHead = !string.IsNullOrEmpty(group.Title);
			foreach (ProjectCard card in group.Cards)
			{
				card.Visible = !group.Collapsed;
			}
			if (hasHead)
			{
				Rectangle head = new Rectangle(Padding.Left, y, Math.Max(cardW, avail), GroupHeadHeight);
				if (apply)
				{
					_headRects.Add(new GroupHead(group, head));
				}
				y += GroupHeadHeight;
			}
			if (!group.Collapsed)
			{
				int col = 0;
				foreach (ProjectCard card in group.Cards)
				{
					if (apply)
					{
						card.Location = new Point(originX + col * (cardW + CardGap), y);
					}
					col++;
					if (col >= cols)
					{
						col = 0;
						y += cardH + CardGap;
					}
				}
				if (col > 0)
				{
					y += cardH + CardGap;
				}
			}
			y += GroupGap;
		}
		return y;
	}
}

/// <summary>"已收藏"横向卡片行（Dock.Top 用，AutoScroll 横向；高度随卡片高度自适应）。</summary>
public class ProjectFavoritesBar : Panel
{
	/// <summary>固定总高度（含标题；随卡片高度自适应，避免卡片被压缩显示不全）。</summary>
	public const int BarHeight = ProjectCard.CardHeight + 20 + 12;

	/// <summary>标题区高度。</summary>
	public const int TitleHeight = 20;

	/// <summary>卡片间距。</summary>
	public const int CardGap = 12;

	/// <summary>左右内边距。</summary>
	public const int SidePadding = 16;

	private const TextFormatFlags TextFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

	private readonly List<ProjectCard> _cards = new List<ProjectCard>();

	private ProjectCardItem _selected;

	/// <summary>项目/模板类型图标解析委托（转发给每张卡片）。</summary>
	public Func<Project, Image> IconResolver { get; set; }

	/// <summary>类型强调色解析委托（转发给每张卡片；返回 Color.Empty 表示普通白卡）。</summary>
	public Func<Project, Color> TintResolver { get; set; }

	/// <summary>付费角标解析委托（转发给每张卡片）。</summary>
	public Func<Project, Image> PayBadgeResolver { get; set; }

	/// <summary>是否绘制/响应收藏星标。</summary>
	public bool IsFavoriteEnabled { get; set; }

	/// <summary>双击/单击打开（转发自卡片 Activated）。</summary>
	public event Action<ProjectCardItem> ItemActivated;

	/// <summary>卡片右键（参数为屏幕坐标）。</summary>
	public event Action<ProjectCardItem, Point> ItemRightClicked;

	/// <summary>卡片悬停"更多"点击（参数为屏幕坐标）。</summary>
	public event Action<ProjectCardItem, Point> ItemMoreClicked;

	/// <summary>卡片收藏星标点击。</summary>
	public event Action<ProjectCardItem> ItemStarClicked;

	/// <summary>选中变化。</summary>
	public event Action SelectionChanged;

	/// <summary>创建已收藏卡片行。</summary>
	public ProjectFavoritesBar()
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		DoubleBuffered = true;
		AutoScroll = true;
		Height = BarHeight;
		BackColor = AuditTheme.SurfaceMuted;
	}

	/// <summary>单选视角下的当前选中项目（无选中为 null）。</summary>
	public Project SelectedProject
	{
		get
		{
			return (_selected != null) ? _selected.Project : null;
		}
	}

	/// <summary>清空选择（有选中时触发 SelectionChanged）。</summary>
	public void ClearSelection()
	{
		if (_selected == null)
		{
			return;
		}
		_selected.IsSelected = false;
		_selected = null;
		RefreshVisual();
		SelectionChanged?.Invoke();
	}

	/// <summary>重建收藏卡片行；空集合时自身 Visible=false（由宿主再统一控制）。</summary>
	public void SetFavorites(IEnumerable<ProjectCardItem> items)
	{
		List<ProjectCardItem> list = ((items != null) ? items.Where((ProjectCardItem i) => i != null && !i.IsCreateTile).ToList() : new List<ProjectCardItem>());
		SuspendLayout();
		try
		{
			foreach (ProjectCard old in Controls.OfType<ProjectCard>().ToList())
			{
				Controls.Remove(old);
				old.Dispose();
			}
			_cards.Clear();
			_selected = null;
			int x = SidePadding;
			foreach (ProjectCardItem item in list)
			{
				ProjectCard card = CreateCard(item);
				card.Location = new Point(x, TitleHeight);
				x += ProjectCard.CardWidth + CardGap;
				_cards.Add(card);
				Controls.Add(card);
			}
			AutoScrollMinSize = new Size((list.Count > 0) ? (x - CardGap + SidePadding) : 0, 0);
			Visible = list.Count > 0;
			Invalidate();
		}
		finally
		{
			ResumeLayout(false);
		}
	}

	/// <summary>刷新所有卡片主题色。</summary>
	public void ApplyTheme()
	{
		foreach (ProjectCard card in _cards)
		{
			card.BackColor = BackColor;
			card.Invalidate();
		}
		Invalidate();
	}

	private ProjectCard CreateCard(ProjectCardItem item)
	{
		ProjectCard card = new ProjectCard(item)
		{
			IconResolver = IconResolver,
			TintResolver = TintResolver,
			PayBadgeResolver = PayBadgeResolver,
			IsFavoriteEnabled = IsFavoriteEnabled,
			BackColor = BackColor
		};
		card.Clicked += delegate
		{
			HandleSelect(item);
		};
		card.Activated += delegate
		{
			ItemActivated?.Invoke(item);
		};
		card.StarClicked += delegate
		{
			ItemStarClicked?.Invoke(item);
		};
		card.RightClicked += delegate (Point pt)
		{
			ItemRightClicked?.Invoke(item, pt);
		};
		card.MoreClicked += delegate (Point pt)
		{
			ItemMoreClicked?.Invoke(item, pt);
		};
		return card;
	}

	private void HandleSelect(ProjectCardItem item)
	{
		if (_selected == item)
		{
			return;
		}
		if (_selected != null)
		{
			_selected.IsSelected = false;
		}
		_selected = item;
		item.IsSelected = true;
		RefreshVisual();
		SelectionChanged?.Invoke();
	}

	private void RefreshVisual()
	{
		foreach (ProjectCard card in _cards)
		{
			card.Invalidate();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		// 左上"已收藏"小标题（8.5f Slate，占高 20px）
		TextRenderer.DrawText(e.Graphics, "已收藏", CardFlowFonts.Secondary, new Rectangle(SidePadding, 0, 200, TitleHeight), AuditTheme.Slate, TextFlags);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		// 仅横向内容：把滚轮转为横向滚动
		int max = Math.Max(0, AutoScrollMinSize.Width - ClientSize.Width);
		if (max <= 0)
		{
			return;
		}
		int x = -AutoScrollPosition.X - e.Delta;
		x = Math.Max(0, Math.Min(x, max));
		AutoScrollPosition = new Point(x, 0);
	}
}

/// <summary>KPI 概览统计条（高 46，Dock.Top；白底圆角面板，项间竖分隔线，选中项高亮）。</summary>
public class KpiStrip : Control
{
	/// <summary>固定高度。</summary>
	public const int StripHeight = 46;

	/// <summary>左右内边距。</summary>
	public const int SidePadding = 16;

	private const TextFormatFlags TextFlagsPlain = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

	private List<KpiItem> _items = new List<KpiItem>();

	private readonly List<Rectangle> _itemRects = new List<Rectangle>();

	private int _hoverIndex = -1;

	private string _selectedTag;

	/// <summary>KPI 项数据。</summary>
	public class KpiItem
	{
		/// <summary>标签文字（如"全部项目"）。</summary>
		public string Label { get; set; }

		/// <summary>数值。</summary>
		public int Value { get; set; }

		/// <summary>选中标识（Tag 相同的项高亮）。</summary>
		public string Tag { get; set; }

		/// <summary>是否显示（默认 true）。</summary>
		public bool Visible { get; set; } = true;

		/// <summary>统计项选中填充色（null 时用主题色；分类项不设置，保持统一主题色）。</summary>
		public Color? AccentColor { get; set; }

		/// <summary>分类筛选项：点击切换 <see cref="Checked"/>（复选），不取消其他分类选中。</summary>
		public bool IsCategory { get; set; }

		/// <summary>分类项选中状态（仅 <see cref="IsCategory"/> 为 true 时生效）。</summary>
		public bool Checked { get; set; }
	}

	/// <summary>点击 KPI 项（再次点击已选中项同样触发，由容器方负责取消选中）。</summary>
	public event Action<KpiItem> ItemClicked;

	/// <summary>当前选中标识（Tag 相同者高亮；置 null 清除）。</summary>
	public string SelectedTag
	{
		get
		{
			return _selectedTag;
		}
		set
		{
			_selectedTag = value;
			Invalidate();
		}
	}

	private List<KpiItem> _allItems = new List<KpiItem>();

	private bool _showCategories = true;

	/// <summary>是否显示分类筛选项（搜索模式下隐藏，仅保留统计项）。</summary>
	public bool ShowCategories
	{
		get
		{
			return _showCategories;
		}
		set
		{
			if (_showCategories == value)
			{
				return;
			}
			_showCategories = value;
			RebuildItems();
		}
	}

	/// <summary>创建 KPI 统计条。</summary>
	public KpiStrip()
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		Height = StripHeight;
		BackColor = AuditTheme.SurfaceMuted;
		Cursor = Cursors.Hand;
	}

	/// <summary>设置 KPI 项（Visible=false 的项被过滤）。</summary>
	public void SetItems(IEnumerable<KpiItem> items)
	{
		_allItems = ((items != null) ? items.Where((KpiItem i) => i != null && i.Visible).ToList() : new List<KpiItem>());
		RebuildItems();
	}

	/// <summary>按 ShowCategories 过滤展示项并重排。</summary>
	private void RebuildItems()
	{
		_items = _allItems.Where((KpiItem i) => i.IsCategory ? _showCategories : true).ToList();
		_hoverIndex = -1;
		RecalcLayout();
		Invalidate();
	}

	/// <summary>刷新主题色。</summary>
	public void ApplyTheme()
	{
		Invalidate();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		RecalcLayout();
	}

	private void RecalcLayout()
	{
		_itemRects.Clear();
		// 现代筛选 chips：等高 30 圆角药丸，项间距 8
		int pillH = 30;
		int padH = 14;
		int y = (Height - pillH) / 2;
		int x = SidePadding;
		foreach (KpiItem item in _items)
		{
			int vw = (item.Value >= 0) ? TextRenderer.MeasureText(item.Value.ToString(), CardFlowFonts.KpiValue, new Size(int.MaxValue, int.MaxValue), TextFlagsPlain).Width : 0;
			int lw = (string.IsNullOrEmpty(item.Label) ? 0 : TextRenderer.MeasureText(item.Label, CardFlowFonts.Secondary, new Size(int.MaxValue, int.MaxValue), TextFlagsPlain).Width);
			int w = padH + vw + ((lw > 0) ? (6 + lw) : 0) + padH;
			_itemRects.Add(new Rectangle(x, y, w, pillH));
			x += w + 8;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		int index = HitIndex(e.Location);
		if (index != _hoverIndex)
		{
			_hoverIndex = index;
			Invalidate();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (_hoverIndex >= 0)
		{
			_hoverIndex = -1;
			Invalidate();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		int index = HitIndex(e.Location);
		if (index < 0)
		{
			return;
		}
		KpiItem item = _items[index];
		if (item.IsCategory)
		{
			// 分类筛选项：复选切换，不取消其他分类
			item.Checked = !item.Checked;
			Invalidate();
		}
		else
		{
			// 统计项：单选
			_selectedTag = item.Tag;
			Invalidate();
		}
		ItemClicked?.Invoke(item);
	}

	private int HitIndex(Point p)
	{
		for (int i = 0; i < _itemRects.Count; i++)
		{
			if (_itemRects[i].Contains(p))
			{
				return i;
			}
		}
		return -1;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (_itemRects.Count != _items.Count)
		{
			RecalcLayout();
		}
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Color dark = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
		// 白底圆角 8 面板感
		Rectangle panel = new Rectangle(0, 0, Width - 1, Height - 1);
		using (GraphicsPath panelPath = CreateRounded(panel, AuditTheme.Radius))
		{
			using (SolidBrush white = new SolidBrush(Color.White))
			{
				g.FillPath(white, panelPath);
			}
			using Pen border = new Pen(AuditTheme.Border);
			g.DrawPath(border, panelPath);
		}
		for (int i = 0; i < _items.Count && i < _itemRects.Count; i++)
		{
			KpiItem item = _items[i];
			Rectangle r = _itemRects[i];
			bool selected = item.IsCategory ? item.Checked : string.Equals(item.Tag, _selectedTag, StringComparison.Ordinal);
			bool isStat = !item.IsCategory;
			Color accent = item.AccentColor ?? dark;
			// 统计项常态彩色：未选中=浅彩底+彩色边框（悬停加深），选中=实心彩底白字；
			// 分类项：未选中=白底细框，选中=主题色实心白字
			using (GraphicsPath itemPath = CreateRounded(r, r.Height / 2))
			{
				if (selected)
				{
					using (SolidBrush back = new SolidBrush(accent))
					{
						g.FillPath(back, itemPath);
					}
				}
				else if (isStat)
				{
					using (SolidBrush back = new SolidBrush(i == _hoverIndex ? Color.FromArgb(72, accent) : Color.FromArgb(30, accent)))
					{
						g.FillPath(back, itemPath);
					}
					using (Pen pen = new Pen(i == _hoverIndex ? Color.FromArgb(190, accent) : Color.FromArgb(110, accent)))
					{
						g.DrawPath(pen, itemPath);
					}
				}
				else
				{
					using (SolidBrush back = new SolidBrush(i == _hoverIndex ? Color.FromArgb(18, dark) : Color.White))
					{
						g.FillPath(back, itemPath);
					}
					using (Pen pen = new Pen(i == _hoverIndex ? AuditTheme.BorderStrong : AuditTheme.Border))
					{
						g.DrawPath(pen, itemPath);
					}
				}
			}
			// 数值：选中白；统计项未选中=彩色；分类项未选中=主题深色。标签：选中白弱化；统计项=灰蓝；分类项=Slate
			int x = r.X + 14;
			string valueText = item.Value.ToString();
			int vw = TextRenderer.MeasureText(valueText, CardFlowFonts.KpiValue, new Size(int.MaxValue, int.MaxValue), TextFlagsPlain).Width;
			Color valueColor = selected ? Color.White : (isStat ? accent : dark);
			TextRenderer.DrawText(g, valueText, CardFlowFonts.KpiValue, new Rectangle(x, r.Y, vw, r.Height), valueColor, TextFlagsPlain);
			x += vw + 6;
			if (!string.IsNullOrEmpty(item.Label))
			{
				Color labelColor = selected ? Color.FromArgb(235, 255, 255, 255) : AuditTheme.Slate;
				TextRenderer.DrawText(g, item.Label, CardFlowFonts.Secondary, new Rectangle(x, r.Y, r.Right - 14 - x, r.Height), labelColor, TextFlagsPlain);
			}
		}
	}

	/// <summary>圆角矩形路径（与 SideCommandBar 同手法）。</summary>
	private static GraphicsPath CreateRounded(Rectangle rect, int radius)
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
