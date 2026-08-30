using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1SplitContainer;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Platform;

/// <summary>
/// 主题应用工具 — Google Blue 风格精修层
/// 
/// 说明：
/// - C1 控件（FlexGrid、Ribbon、SplitContainer 等）的整体样式由 C1ThemeController 负责
/// - 本类负责 C1 主题覆盖不到的地方：标准 WinForms 控件、自定义绘制、细节微调
/// - 每个方法都是独立的，可以按窗体按需调用
/// </summary>
public static class ThemeApplier
{
	#region Form / 基础窗体

	/// <summary>
	/// 应用到普通 Form 背景和字体
	/// </summary>
	public static void ApplyForm(Form form)
	{
		form.BackColor = AuditTheme.Surface;
		form.ForeColor = AuditTheme.Text;
		form.Font = AuditTheme.FontDefault;
	}

	#endregion

	#region C1FlexGrid 表格（微调，在 C1Theme 基础上叠加）

	/// <summary>
	/// 在 C1 主题基础上，对表格做 Google Blue 风格精修
	/// 仅修改那些 C1Theme 覆盖不到或效果不理想的样式
	/// </summary>
	public static void ApplyFlexGrid(C1FlexGrid grid)
	{
		grid.BeginUpdate();
		try
		{
			// 字体
			grid.Font = AuditTheme.FontDefault;

			// 行高调整 — 更紧凑的表格行
			if (grid.Rows.Count > 0)
			{
				grid.Rows[0].Height = 30; // 列头行
			}

			// 列头样式微调
			var fixedStyle = grid.Styles.Fixed;
			fixedStyle.Font = AuditTheme.FontSmallBold;
			fixedStyle.ForeColor = AuditTheme.TextMuted;
			fixedStyle.TextAlign = TextAlignEnum.LeftCenter;

			// 数字列右对齐（通过样式设置，调用方可指定具体列）
			grid.Styles.Normal.Font = AuditTheme.FontDefault;

			// 选中行高亮色 — Google Blue 淡蓝
			grid.Styles.Highlight.BackColor = AuditTheme.BrandSubtle;
			grid.Styles.Highlight.ForeColor = AuditTheme.Text;

			// 焦点单元格
			grid.Styles.Focus.BackColor = AuditTheme.BrandSubtle;
			grid.Styles.Focus.ForeColor = AuditTheme.Text;
		}
		finally
		{
			grid.EndUpdate();
		}
	}

	/// <summary>
	/// 设置表格某列为数字格式：右对齐 + 等宽字体 + 千分位
	/// </summary>
	public static void SetNumericColumn(C1FlexGrid grid, int col)
	{
		var style = grid.Cols[col].StyleDisplay;
		style.Font = AuditTheme.FontMono;
		style.TextAlign = TextAlignEnum.RightCenter;
		grid.Cols[col].Format = "#,##0.00";
	}

	#endregion

	#region C1SplitContainer 分栏（微调）

	/// <summary>
	/// 微调分栏控件的颜色，在 C1Theme 基础上叠加
	/// </summary>
	public static void ApplySplitContainer(C1SplitContainer splitter)
	{
		splitter.SplitterWidth = 1;
		splitter.SplitterColor = AuditTheme.Border;

		foreach (C1SplitterPanel panel in splitter.Panels)
		{
			panel.BackColor = AuditTheme.Surface;
			panel.Font = AuditTheme.FontDefault;
		}
	}

	#endregion

	#region Button 按钮

	/// <summary>
	/// 应用主要按钮样式（填充蓝底白字）
	/// </summary>
	public static void ApplyPrimaryButton(Button btn)
	{
		btn.FlatStyle = FlatStyle.Flat;
		btn.FlatAppearance.BorderSize = 0;
		btn.BackColor = AuditTheme.Brand;
		btn.ForeColor = AuditTheme.BrandForeground;
		btn.Font = AuditTheme.FontDefaultBold;
		btn.Cursor = Cursors.Hand;
		btn.Padding = new Padding(16, 6, 16, 6);

		btn.MouseEnter += (s, e) => btn.BackColor = AuditTheme.BrandHover;
		btn.MouseLeave += (s, e) => btn.BackColor = AuditTheme.Brand;
		btn.MouseDown += (s, e) => btn.BackColor = AuditTheme.BrandActive;
		btn.MouseUp += (s, e) => btn.BackColor = AuditTheme.BrandHover;
	}

	/// <summary>
	/// 应用次要按钮样式（边框 + 透明底）
	/// </summary>
	public static void ApplySecondaryButton(Button btn)
	{
		btn.FlatStyle = FlatStyle.Flat;
		btn.FlatAppearance.BorderSize = 1;
		btn.FlatAppearance.BorderColor = AuditTheme.BorderStrong;
		btn.BackColor = AuditTheme.Surface;
		btn.ForeColor = AuditTheme.Text;
		btn.Font = AuditTheme.FontDefault;
		btn.Cursor = Cursors.Hand;
		btn.Padding = new Padding(16, 6, 16, 6);

		btn.MouseEnter += (s, e) =>
		{
			btn.BackColor = AuditTheme.SurfaceMuted;
			btn.FlatAppearance.BorderColor = AuditTheme.Brand;
		};
		btn.MouseLeave += (s, e) =>
		{
			btn.BackColor = AuditTheme.Surface;
			btn.FlatAppearance.BorderColor = AuditTheme.BorderStrong;
		};
	}

	/// <summary>
	/// 应用文字按钮样式（无边框，hover 淡蓝底）
	/// </summary>
	public static void ApplyTextButton(Button btn)
	{
		btn.FlatStyle = FlatStyle.Flat;
		btn.FlatAppearance.BorderSize = 0;
		btn.BackColor = Color.Transparent;
		btn.ForeColor = AuditTheme.Brand;
		btn.Font = AuditTheme.FontDefault;
		btn.Cursor = Cursors.Hand;
		btn.TextAlign = ContentAlignment.MiddleCenter;

		btn.MouseEnter += (s, e) => btn.BackColor = AuditTheme.BrandSubtle;
		btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
	}

	#endregion

	#region TextBox 输入框

	/// <summary>
	/// 应用输入框样式
	/// </summary>
	public static void ApplyTextBox(TextBox txt)
	{
		txt.BackColor = AuditTheme.Surface;
		txt.ForeColor = AuditTheme.Text;
		txt.Font = AuditTheme.FontDefault;
		txt.BorderStyle = BorderStyle.FixedSingle;
	}

	#endregion

	#region Panel 面板

	/// <summary>
	/// 应用卡片面板样式
	/// </summary>
	public static void ApplyCardPanel(Panel pnl)
	{
		pnl.BackColor = AuditTheme.Surface;
		pnl.ForeColor = AuditTheme.Text;
		pnl.Font = AuditTheme.FontDefault;
		pnl.Padding = new Padding(12);
	}

	/// <summary>
	/// 应用工具栏面板样式（淡灰底 + 底部边框）
	/// </summary>
	public static void ApplyToolbarPanel(Panel pnl)
	{
		pnl.BackColor = AuditTheme.SurfaceMuted;
		pnl.ForeColor = AuditTheme.Text;
		pnl.Font = AuditTheme.FontDefault;
		pnl.Padding = new Padding(8, 6, 8, 6);
	}

	#endregion

	#region StatusBar 状态栏

	/// <summary>
	/// 应用到 C1StatusBar
	/// </summary>
	public static void ApplyStatusBar(C1StatusBar statusBar)
	{
		statusBar.Font = AuditTheme.FontSmall;
	}

	#endregion

	#region TreeView 树视图

	/// <summary>
	/// 应用到 TreeView 导航树 — 自绘节点，Google Blue 选中态
	/// </summary>
	public static void ApplyTreeView(TreeView tree)
	{
		tree.BackColor = AuditTheme.SidebarBg;
		tree.ForeColor = AuditTheme.Text;
		tree.Font = AuditTheme.FontDefault;
		tree.BorderStyle = BorderStyle.None;
		tree.FullRowSelect = true;
		tree.HotTracking = true;
		tree.HideSelection = false;
		tree.Indent = 16;
		tree.ItemHeight = 26;

		tree.DrawMode = TreeViewDrawMode.OwnerDrawText;
		tree.DrawNode -= Tree_DrawNode;
		tree.DrawNode += Tree_DrawNode;
	}

	private static void Tree_DrawNode(object sender, DrawTreeNodeEventArgs e)
	{
		var tree = (TreeView)sender;
		Graphics g = e.Graphics;
		TreeNode node = e.Node;
		Rectangle bounds = node.Bounds;

		if (bounds.Width <= 0 || bounds.Height <= 0)
		{
			e.DrawDefault = true;
			return;
		}

		int fullWidth = tree.ClientSize.Width;
		Rectangle fullRect = new Rectangle(0, bounds.Top, fullWidth, bounds.Height);

		// 选中节点：蓝色背景 + 左边框
		if ((e.State & TreeNodeStates.Selected) != 0)
		{
			using (var brush = new SolidBrush(AuditTheme.BrandSubtle))
			{
				g.FillRectangle(brush, fullRect);
			}
			using (var pen = new Pen(AuditTheme.Brand, 2f))
			{
				g.DrawLine(pen, 0, bounds.Top, 0, bounds.Bottom);
			}
		}
		else if ((e.State & TreeNodeStates.Hot) != 0)
		{
			using (var brush = new SolidBrush(AuditTheme.SurfaceSubtle))
			{
				g.FillRectangle(brush, fullRect);
			}
		}

		// 文字
		TextRenderer.DrawText(g, node.Text, tree.Font,
			new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height),
			AuditTheme.Text,
			TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
	}

	#endregion

	#region TabControl 标签页

	/// <summary>
	/// 应用到 TabControl — 自绘标签，Google 风格下划线
	/// </summary>
	public static void ApplyTabControl(TabControl tab)
	{
		tab.Appearance = TabAppearance.Normal;
		tab.BackColor = AuditTheme.Surface;
		tab.ForeColor = AuditTheme.Text;
		tab.Font = AuditTheme.FontDefault;
		tab.SizeMode = TabSizeMode.Fixed;
		tab.ItemSize = new Size(120, 32);

		tab.DrawMode = TabDrawMode.OwnerDrawFixed;
		tab.DrawItem -= Tab_DrawItem;
		tab.DrawItem += Tab_DrawItem;
	}

	private static void Tab_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
	{
		var tab = (TabControl)sender;
		Graphics g = e.Graphics;
		TabPage page = tab.TabPages[e.Index];
		Rectangle rect = tab.GetTabRect(e.Index);

		bool isSelected = (e.State & DrawItemState.Selected) != 0;

		// 背景
		if (isSelected)
		{
			using (var brush = new SolidBrush(AuditTheme.Surface))
			{
				g.FillRectangle(brush, rect);
			}
			// 选中项底部蓝色线条
			using (var pen = new Pen(AuditTheme.Brand, 2f))
			{
				g.DrawLine(pen, rect.Left + 8, rect.Bottom - 1, rect.Right - 8, rect.Bottom - 1);
			}
		}
		else
		{
			using (var brush = new SolidBrush(AuditTheme.SurfaceMuted))
			{
				g.FillRectangle(brush, rect);
			}
		}

		// 文字
		Color textColor = isSelected ? AuditTheme.Brand : AuditTheme.TextMuted;
		Font textFont = isSelected ? AuditTheme.FontDefaultBold : AuditTheme.FontDefault;
		TextRenderer.DrawText(g, page.Text, textFont, rect, textColor,
			TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
	}

	#endregion

	#region Label 标签

	/// <summary>
	/// 应用标题标签样式
	/// </summary>
	public static void ApplyHeaderLabel(Label lbl)
	{
		lbl.Font = AuditTheme.FontHeader;
		lbl.ForeColor = AuditTheme.Text;
		lbl.BackColor = Color.Transparent;
	}

	/// <summary>
	/// 应用次要标签样式（说明文字）
	/// </summary>
	public static void ApplyMutedLabel(Label lbl)
	{
		lbl.Font = AuditTheme.FontSmall;
		lbl.ForeColor = AuditTheme.TextMuted;
		lbl.BackColor = Color.Transparent;
	}

	#endregion

	#region Dialog / 通用对话框精修

	/// <summary>
	/// 通用对话框 Google 风格精修
	/// 递归遍历控件树，统一按钮、输入框、标签的样式
	/// 
	/// 适用：所有通过 Theme.SetCurrentTree 应用主题的对话框窗体
	/// 效果：
	/// - 按钮高度 36px（对话框内按钮略小于主按钮 40px）
	/// - 主按钮（Google 蓝填充）+ 次按钮（白底灰边）自动识别
	/// - 输入框高度统一
	/// - 标签文字色对齐
	/// </summary>
	public static void ApplyDialogStyle(Control root)
	{
		// 只在 Google Blue 主题下生效
		var theme = Controls.Theme.SelectedAuditaiTheme;
		if (theme == null || theme.Name != "auditai_GoogleBlue")
		{
			return;
		}

		RecursiveRefine(root);
	}

	private static void RecursiveRefine(Control control)
	{
		if (control == null) return;

		// C1SplitContainer 的 Panels 需要单独遍历
		if (control is C1.Win.C1SplitContainer.C1SplitContainer splitContainer)
		{
			foreach (C1.Win.C1SplitContainer.C1SplitterPanel panel in splitContainer.Panels)
			{
				RecursiveRefine(panel);
			}
		}

		// 精修当前控件
		RefineControl(control);

		// 递归子控件
		foreach (Control child in control.Controls)
		{
			RecursiveRefine(child);
		}
	}

	private static void RefineControl(Control c)
	{
		// C1Button：统一高度 + 主/次按钮样式
		if (c is C1.Win.C1Input.C1Button btn)
		{
			RefineButton(btn);
		}
		// C1TextBoxEx：统一高度和字体
		else if (c is Auditai.UI.Controls.C1TextBoxEx txt)
		{
			txt.Font = AuditTheme.FontBody;
			txt.ForeColor = AuditTheme.Text;
			if (txt.Height < AuditTheme.InputHeight - 4)
			{
				txt.Height = AuditTheme.InputHeight;
			}
		}
		// C1Label：统一字体和颜色
		else if (c is C1.Win.C1Input.C1Label lbl)
		{
			// 跳过明显是标题的大字号标签
			if (lbl.Font.Size > 11) return;
			lbl.Font = AuditTheme.FontBody;
			// 只调整非高亮文字色
			if (lbl.ForeColor == Color.Black || lbl.ForeColor.ToArgb() == Color.FromArgb(30, 41, 59).ToArgb())
			{
				lbl.ForeColor = AuditTheme.TextSecondary;
			}
		}
		// 标准 Label：统一字体和颜色
		else if (c is Label label)
		{
			if (label.Font.Size > 11) return;
			if (label.Font.Bold) return; // 粗体标签一般是标题，跳过
			label.Font = AuditTheme.FontBody;
			if (label.ForeColor == Color.Black)
			{
				label.ForeColor = AuditTheme.TextSecondary;
			}
		}
		// NumericUpDown：统一高度
		else if (c is NumericUpDown num)
		{
			num.Font = AuditTheme.FontBody;
		}
		// CheckBox：统一字体
		else if (c is CheckBox chk)
		{
			chk.Font = AuditTheme.FontBody;
			chk.ForeColor = AuditTheme.Text;
		}
		// ComboBox：统一字体
		else if (c is ComboBox cbo)
		{
			cbo.Font = AuditTheme.FontBody;
		}
	}

	private static void RefineButton(C1.Win.C1Input.C1Button btn)
	{
		if (btn == null) return;

		// 过小的按钮（图标按钮、ToolBar 按钮）跳过
		if (btn.Width < 50 || btn.Height < 28) return;

		// 统一按钮高度为 36px（对话框内次一级尺寸）
		if (btn.Height > 28 && btn.Height < AuditTheme.ButtonHeightSm)
		{
			btn.Height = AuditTheme.ButtonHeightSm;
		}

		// 统一字体
		btn.Font = AuditTheme.FontBodyBold;

		// 判断是否为主按钮：
		// 1. 背景色接近品牌蓝
		// 2. DialogResult == OK（对话框的"确定"按钮）
		bool isPrimary = IsPrimaryColor(btn.BackColor)
			|| btn.DialogResult == DialogResult.OK;

		if (isPrimary)
		{
			// 主按钮：确保是 Google 蓝
			btn.BackColor = AuditTheme.Brand;
			btn.ForeColor = Color.White;
			btn.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
			btn.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		}
		else if (btn.BackColor == Color.White || btn.BackColor.ToArgb() == Color.FromArgb(255, 255, 255).ToArgb())
		{
			// 白底按钮：次按钮样式（统一边框色）
			btn.ForeColor = AuditTheme.Text;
			btn.FlatAppearance.BorderColor = AuditTheme.BorderStrong;
			btn.FlatAppearance.MouseOverBackColor = AuditTheme.SurfaceMuted;
			btn.FlatAppearance.MouseDownBackColor = AuditTheme.SurfaceHover;
		}
	}

	/// <summary>
	/// 判断颜色是否接近品牌蓝（主按钮色）
	/// </summary>
	private static bool IsPrimaryColor(Color c)
	{
		// 品牌蓝及常见变体
		int argb = c.ToArgb();
		return argb == AuditTheme.Brand.ToArgb()
			|| argb == Color.FromArgb(0, 120, 215).ToArgb()
			|| argb == Color.FromArgb(0, 195, 245).ToArgb()
			|| argb == Color.FromArgb(74, 144, 217).ToArgb()
			|| argb == Color.FromArgb(0, 100, 200).ToArgb()
			|| (c.R < 100 && c.G >= 100 && c.G < 200 && c.B > 180); // 偏蓝的颜色
	}

	#endregion
}
