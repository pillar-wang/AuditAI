using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Input;
using Auditai.DTO;

namespace Auditai.UI.Platform;

/// <summary>
/// 审批对话框：
/// - ShowApprove 完整模式：上部审核单信息（只读）+ 中部审批历史（ListView 每级一行）+ 意见输入，可"通过"（ApproveReview）或"退回"（RejectReview，意见必填）；
/// - ShowApprove isReject=true 快速退回模式：仅项目名 + 退回意见输入 + 退回按钮；
/// - ShowHistory 只读历史：GetReviewHistory 按轮次分组展示项目全部审批记录（TreeView，可滚动）。
/// </summary>
public class frmReviewApprove : Form
{
	// 统一设计语言常量：按钮 110×40、间距 12、按钮区距右边缘 50px
	private const int BtnWidth = 110;
	private const int BtnHeight = 40;
	private const int BtnGap = 12;
	private const int RightMargin = 50;

	// .NET Framework 的 TextBox 无 PlaceholderText，用 EM_SETCUEBANNER 实现输入提示
	private const int EM_SETCUEBANNER = 0x1501;

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

	private static readonly string[] ReportTypeNames = { "年报审计", "中期审计", "专项审计", "其他" };
	private static readonly string[] OpinionTypeNames = { "无保留意见", "保留意见", "否定意见", "无法表示意见", "其他" };
	private static readonly string[] SubmissionStatusNames = { "审批中", "已通过", "已退回", "已撤回" };
	private static readonly string[] NodeStatusNames = { "待审", "通过", "退回" };

	private readonly ReviewSubmissionDto _submission;
	private readonly bool _isReject;

	private TextBox _txtComment;
	private ListView _lvNodes;
	private C1Button _btnApprove;
	private C1Button _btnReject;
	private C1Button _btnCancel;

	/// <summary>弹出审批对话框。返回 OK 表示已完成通过/退回操作。</summary>
	public static DialogResult ShowApprove(IWin32Window owner, ReviewSubmissionDto submission, bool isReject)
	{
		using (frmReviewApprove dlg = new frmReviewApprove(submission, isReject))
		{
			return dlg.ShowDialog(owner);
		}
	}

	/// <summary>只读查看项目的全部轮次审批记录。</summary>
	public static DialogResult ShowHistory(IWin32Window owner, string projectId, string projectName)
	{
		using (frmReviewHistory dlg = new frmReviewHistory(projectId, projectName))
		{
			return dlg.ShowDialog(owner);
		}
	}

	private frmReviewApprove(ReviewSubmissionDto submission, bool isReject)
	{
		_submission = submission;
		_isReject = isReject;
		if (isReject)
		{
			InitializeQuickRejectUi();
		}
		else
		{
			InitializeFullUi();
		}
		// 先应用主题（浅蓝小清新，作用于 C1 控件等），再重设按钮配色（C1 皮肤会覆盖自定义配色）
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		RefreshButtonStyles();
		Load += FrmReviewApprove_Load;
	}

	// ==================== 完整模式布局（约 520×460） ====================

	private void InitializeFullUi()
	{
		Text = "审核审批";
		ClientSize = new Size(520, 460);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		Font = new Font("微软雅黑", 9f);
		BackColor = Color.White;

		// ---- 顶部标题栏（浅蓝背景，内边距 20 不紧贴边缘） ----
		Panel pnlHeader = new Panel
		{
			Dock = DockStyle.Top,
			Height = 44,
			BackColor = AuditTheme.BrandSubtle
		};
		Label lblTitle = new Label
		{
			Text = "审核审批（第 " + _submission.Round + " 轮）",
			Location = new Point(20, 0),
			Size = new Size(480, 44),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 10.5f, FontStyle.Bold),
			ForeColor = AuditTheme.BrandActive
		};
		pnlHeader.Controls.Add(lblTitle);

		// ---- 上部信息区（只读，两列布局） ----
		Panel pnlInfo = new Panel
		{
			Dock = DockStyle.Top,
			Height = 146,
			BackColor = AuditTheme.SurfaceMuted
		};
		pnlInfo.Controls.Add(MakeInfoCaption("项目名称", 16, 10));
		pnlInfo.Controls.Add(MakeInfoValue(ProjectDisplay(_submission.ProjectName), 82, 10, 168));
		pnlInfo.Controls.Add(MakeInfoCaption("轮次", 258, 10));
		pnlInfo.Controls.Add(MakeInfoValue("第 " + _submission.Round + " 轮", 324, 10, 180));
		pnlInfo.Controls.Add(MakeInfoCaption("报告类型", 16, 34));
		pnlInfo.Controls.Add(MakeInfoValue(IndexName(ReportTypeNames, _submission.ReportType), 82, 34, 168));
		pnlInfo.Controls.Add(MakeInfoCaption("报告文号", 258, 34));
		pnlInfo.Controls.Add(MakeInfoValue(DisplayOrDash(_submission.ReportNo), 324, 34, 180));
		pnlInfo.Controls.Add(MakeInfoCaption("意见类型", 16, 58));
		pnlInfo.Controls.Add(MakeInfoValue(IndexName(OpinionTypeNames, _submission.OpinionType), 82, 58, 168));
		pnlInfo.Controls.Add(MakeInfoCaption("上报人", 258, 58));
		pnlInfo.Controls.Add(MakeInfoValue(DisplayOrDash(_submission.SubmitterName), 324, 58, 180));
		pnlInfo.Controls.Add(MakeInfoCaption("上报时间", 16, 82));
		pnlInfo.Controls.Add(MakeInfoValue(DisplayOrDash(_submission.SubmitTime), 82, 82, 168));
		pnlInfo.Controls.Add(MakeInfoCaption("审批级数", 258, 82));
		pnlInfo.Controls.Add(MakeInfoValue(_submission.TotalLevel + " 级", 324, 82, 180));
		pnlInfo.Controls.Add(MakeInfoCaption("上报说明", 16, 106));
		Label lblNote = new Label
		{
			Text = DisplayOrDash(_submission.Note),
			Location = new Point(82, 106),
			Size = new Size(422, 32),
			TextAlign = ContentAlignment.TopLeft,
			AutoEllipsis = true,
			ForeColor = AuditTheme.Text
		};
		pnlInfo.Controls.Add(lblNote);

		// ---- 中部审批历史区（每级一行，带滚动） ----
		Panel pnlHistory = new Panel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12, 6, 12, 6),
			BackColor = Color.White
		};
		_lvNodes = new ListView
		{
			Dock = DockStyle.Fill,
			View = View.Details,
			FullRowSelect = true,
			MultiSelect = false,
			HideSelection = true,
			BorderStyle = BorderStyle.None,
			UseCompatibleStateImageBehavior = false,
			Font = new Font("微软雅黑", 9f)
		};
		_lvNodes.Columns.Add("级别", 44);
		_lvNodes.Columns.Add("审批人", 88);
		_lvNodes.Columns.Add("状态", 58);
		_lvNodes.Columns.Add("审批时间", 128);
		_lvNodes.Columns.Add("审批意见", 168);
		foreach (ReviewNodeDto node in (_submission.Nodes ?? new List<ReviewNodeDto>()).OrderBy((ReviewNodeDto n) => n.Level))
		{
			ListViewItem item = new ListViewItem(node.Level.ToString());
			item.SubItems.Add(DisplayOrDash(node.ReviewerName));
			item.SubItems.Add(IndexName(NodeStatusNames, node.Status));
			item.SubItems.Add(DisplayOrDash(node.ReviewTime));
			item.SubItems.Add(string.IsNullOrWhiteSpace(node.Comment) ? "—" : node.Comment);
			item.UseItemStyleForSubItems = false;
			item.SubItems[2].ForeColor = NodeStatusColor(node.Status);
			_lvNodes.Items.Add(item);
		}
		pnlHistory.Controls.Add(_lvNodes);

		// ---- 意见输入区（多行，约 2 行高） ----
		Panel pnlComment = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 64,
			BackColor = Color.White
		};
		Label lblComment = new Label
		{
			Text = "审批意见",
			Location = new Point(16, 10),
			Size = new Size(62, 44),
			TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = AuditTheme.TextSecondary
		};
		_txtComment = new TextBox
		{
			Location = new Point(82, 6),
			Size = new Size(422, 52),
			Multiline = true,
			ScrollBars = ScrollBars.Vertical
		};
		pnlComment.Controls.Add(lblComment);
		pnlComment.Controls.Add(_txtComment);

		// ---- 底部按钮区：通 过（绿色基调）/ 退 回（红色基调）/ 取 消 ----
		Panel pnlButtons = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 56,
			BackColor = AuditTheme.SurfaceMuted
		};
		_btnApprove = new C1Button
		{
			Text = "通 过",
			Location = new Point(520 - RightMargin - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f, FontStyle.Bold)
		};
		_btnApprove.Click += BtnApprove_Click;
		_btnReject = new C1Button
		{
			Text = "退 回",
			Location = new Point(520 - RightMargin - BtnWidth - BtnGap - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f)
		};
		_btnReject.Click += BtnReject_Click;
		_btnCancel = new C1Button
		{
			Text = "取 消",
			Location = new Point(520 - RightMargin - BtnWidth - BtnGap - BtnWidth - BtnGap - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f),
			DialogResult = DialogResult.Cancel
		};
		pnlButtons.Controls.Add(_btnApprove);
		pnlButtons.Controls.Add(_btnReject);
		pnlButtons.Controls.Add(_btnCancel);

		// 停靠顺序：后添加的先停靠 —— Header 贴顶、Info 其次、按钮/意见区贴底、历史区填充剩余
		Controls.Add(pnlButtons);
		Controls.Add(pnlComment);
		Controls.Add(pnlHistory);
		Controls.Add(pnlInfo);
		Controls.Add(pnlHeader);

		CancelButton = _btnCancel;
	}

	// ==================== 快速退回模式布局（约 460×232） ====================

	private void InitializeQuickRejectUi()
	{
		Text = "退回审核";
		ClientSize = new Size(460, 232);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		Font = new Font("微软雅黑", 9f);
		BackColor = Color.White;

		// ---- 顶部标题栏 ----
		Panel pnlHeader = new Panel
		{
			Dock = DockStyle.Top,
			Height = 44,
			BackColor = AuditTheme.BrandSubtle
		};
		Label lblTitle = new Label
		{
			Text = "退回审核",
			Location = new Point(20, 0),
			Size = new Size(420, 44),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 10.5f, FontStyle.Bold),
			ForeColor = AuditTheme.BrandActive
		};
		pnlHeader.Controls.Add(lblTitle);

		// ---- 项目信息行（只读） ----
		Panel pnlInfo = new Panel
		{
			Dock = DockStyle.Top,
			Height = 40,
			BackColor = AuditTheme.SurfaceMuted
		};
		Label lblProject = new Label
		{
			Text = "项目：" + ProjectDisplay(_submission.ProjectName) + "（第 " + _submission.Round + " 轮）",
			Location = new Point(20, 0),
			Size = new Size(420, 40),
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			ForeColor = AuditTheme.TextSecondary
		};
		pnlInfo.Controls.Add(lblProject);

		// ---- 退回意见输入区 ----
		Panel pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
		Label lblComment = new Label
		{
			Text = "退回意见 *",
			Location = new Point(24, 14),
			Size = new Size(90, 22),
			TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = AuditTheme.TextSecondary
		};
		_txtComment = new TextBox
		{
			Location = new Point(118, 12),
			Size = new Size(460 - 118 - 24, 92 - 24),
			Multiline = true,
			ScrollBars = ScrollBars.Vertical
		};
		pnlBody.Controls.Add(lblComment);
		pnlBody.Controls.Add(_txtComment);

		// ---- 底部按钮区：退 回（红色基调）/ 取 消 ----
		Panel pnlButtons = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 56,
			BackColor = AuditTheme.SurfaceMuted
		};
		_btnReject = new C1Button
		{
			Text = "退 回",
			Location = new Point(460 - RightMargin - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f, FontStyle.Bold)
		};
		_btnReject.Click += BtnReject_Click;
		_btnCancel = new C1Button
		{
			Text = "取 消",
			Location = new Point(460 - RightMargin - BtnWidth - BtnGap - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f),
			DialogResult = DialogResult.Cancel
		};
		pnlButtons.Controls.Add(_btnReject);
		pnlButtons.Controls.Add(_btnCancel);

		// 停靠顺序：后添加的先停靠
		Controls.Add(pnlButtons);
		Controls.Add(pnlBody);
		Controls.Add(pnlInfo);
		Controls.Add(pnlHeader);

		CancelButton = _btnCancel;
	}

	private void FrmReviewApprove_Load(object sender, EventArgs e)
	{
		// 输入提示须在句柄创建后设置
		SendMessage(_txtComment.Handle, EM_SETCUEBANNER, (IntPtr)1, _isReject ? "请填写退回原因（必填）" : "审批意见（退回时必填）");
	}

	// ==================== 通过 / 退回 ====================

	private async void BtnApprove_Click(object sender, EventArgs e)
	{
		SetBusy(true);
		try
		{
			await Auditai.Util.WebApiClient.ApproveReview(_submission.Id, _txtComment.Text.Trim());
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已通过该审核单");
			DialogResult = DialogResult.OK;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "审批失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
		catch (ServerException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "审批失败：" + ex.Message);
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "审批失败：" + ex.Message);
		}
		finally
		{
			// 成功路径下窗体已关闭，避免操作已释放控件
			if (!IsDisposed)
			{
				SetBusy(false);
			}
		}
	}

	private async void BtnReject_Click(object sender, EventArgs e)
	{
		string comment = _txtComment.Text.Trim();
		// 退回必填意见
		if (string.IsNullOrWhiteSpace(comment))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "退回时必须填写审批意见");
			_txtComment.Focus();
			return;
		}
		SetBusy(true);
		try
		{
			await Auditai.Util.WebApiClient.RejectReview(_submission.Id, comment);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已退回给提交人，可修改后重新上报");
			DialogResult = DialogResult.OK;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "退回失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
		catch (ServerException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "退回失败：" + ex.Message);
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "退回失败：" + ex.Message);
		}
		finally
		{
			// 成功路径下窗体已关闭，避免操作已释放控件
			if (!IsDisposed)
			{
				SetBusy(false);
			}
		}
	}

	private void SetBusy(bool busy)
	{
		if (_btnApprove != null)
		{
			_btnApprove.Enabled = !busy;
		}
		if (_btnReject != null)
		{
			_btnReject.Enabled = !busy;
		}
	}

	// ==================== 展示辅助 ====================

	private static string ProjectDisplay(string projectName)
	{
		return string.IsNullOrWhiteSpace(projectName) ? "（未知项目）" : projectName;
	}

	private static string DisplayOrDash(string value)
	{
		return string.IsNullOrWhiteSpace(value) ? "—" : value;
	}

	private static string IndexName(string[] names, int index)
	{
		return (index >= 0 && index < names.Length) ? names[index] : "其他";
	}

	private static Color NodeStatusColor(int status)
	{
		switch (status)
		{
			case 1:
				return AuditTheme.SuccessText;
			case 2:
				return AuditTheme.ErrorText;
			default:
				return AuditTheme.TextMuted;
		}
	}

	private static Label MakeInfoCaption(string text, int x, int y)
	{
		return new Label
		{
			Text = text,
			Location = new Point(x, y),
			Size = new Size(64, 18),
			TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = AuditTheme.TextSecondary
		};
	}

	private static Label MakeInfoValue(string text, int x, int y, int width)
	{
		return new Label
		{
			Text = text,
			Location = new Point(x, y),
			Size = new Size(width, 18),
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			ForeColor = AuditTheme.Text
		};
	}

	/// <summary>统一设计语言：给按钮应用 8px 圆角区域（与 frmStandardAccountDic 保持一致）</summary>
	private static void ApplyRoundedButton(Control btn, int radius)
	{
		using (var path = new System.Drawing.Drawing2D.GraphicsPath())
		{
			path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
			path.AddArc(btn.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
			path.AddArc(btn.Width - radius * 2, btn.Height - radius * 2, radius * 2, radius * 2, 0, 90);
			path.AddArc(0, btn.Height - radius * 2, radius * 2, radius * 2, 90, 90);
			path.CloseFigure();
			btn.Region = new System.Drawing.Region(path);
		}
	}

	/// <summary>统一设计语言：通过=浅绿底绿字、退回=浅红底红字、取消=白底灰字浅灰边框</summary>
	private void RefreshButtonStyles()
	{
		if (_btnApprove != null)
		{
			_btnApprove.FlatStyle = FlatStyle.Flat;
			_btnApprove.FlatAppearance.BorderSize = 1;
			_btnApprove.FlatAppearance.BorderColor = AuditTheme.SuccessBorder;
			_btnApprove.FlatAppearance.MouseDownBackColor = AuditTheme.SuccessBorder;
			_btnApprove.FlatAppearance.MouseOverBackColor = AuditTheme.SuccessSubtle;
			_btnApprove.ForeColor = AuditTheme.SuccessText;
			_btnApprove.BackColor = AuditTheme.SuccessSubtle;
			_btnApprove.UseVisualStyleBackColor = false;
			ApplyRoundedButton(_btnApprove, 8);
		}
		if (_btnReject != null)
		{
			_btnReject.FlatStyle = FlatStyle.Flat;
			_btnReject.FlatAppearance.BorderSize = 1;
			_btnReject.FlatAppearance.BorderColor = AuditTheme.ErrorBorder;
			_btnReject.FlatAppearance.MouseDownBackColor = AuditTheme.ErrorBorder;
			_btnReject.FlatAppearance.MouseOverBackColor = AuditTheme.ErrorSubtle;
			_btnReject.ForeColor = AuditTheme.ErrorText;
			_btnReject.BackColor = AuditTheme.ErrorSubtle;
			_btnReject.UseVisualStyleBackColor = false;
			ApplyRoundedButton(_btnReject, 8);
		}
		if (_btnCancel != null)
		{
			_btnCancel.FlatStyle = FlatStyle.Flat;
			_btnCancel.FlatAppearance.BorderSize = 1;
			_btnCancel.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
			_btnCancel.FlatAppearance.MouseOverBackColor = AuditTheme.SurfaceSubtle;
			_btnCancel.ForeColor = Color.FromArgb(30, 41, 59);
			_btnCancel.BackColor = Color.White;
			_btnCancel.UseVisualStyleBackColor = false;
			ApplyRoundedButton(_btnCancel, 8);
		}
	}
}

/// <summary>
/// 审批记录只读窗体：GetReviewHistory 拉取项目全部轮次审核单，按轮次分组（每轮：状态、提交时间、各级节点审批明细），TreeView 展示可滚动。
/// </summary>
public class frmReviewHistory : Form
{
	// 统一设计语言常量：按钮 110×40、按钮区距右边缘 50px
	private const int BtnWidth = 110;
	private const int BtnHeight = 40;
	private const int RightMargin = 50;

	private static readonly string[] ReportTypeNames = { "年报审计", "中期审计", "专项审计", "其他" };
	private static readonly string[] OpinionTypeNames = { "无保留意见", "保留意见", "否定意见", "无法表示意见", "其他" };
	private static readonly string[] SubmissionStatusNames = { "审批中", "已通过", "已退回", "已撤回" };
	private static readonly string[] NodeStatusNames = { "待审", "通过", "退回" };

	private readonly string _projectId;
	private readonly string _projectName;

	private Label _lblSummary;
	private TreeView _tvHistory;

	public frmReviewHistory(string projectId, string projectName)
	{
		_projectId = projectId;
		_projectName = projectName;
		InitializeComponent();
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		Load += FrmReviewHistory_Load;
	}

	private void InitializeComponent()
	{
		Text = "审批记录";
		ClientSize = new Size(560, 420);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		Font = new Font("微软雅黑", 9f);
		BackColor = Color.White;

		// ---- 顶部标题栏（标题 + 项目副标题，内边距 20 不紧贴边缘） ----
		Panel pnlHeader = new Panel
		{
			Dock = DockStyle.Top,
			Height = 52,
			BackColor = AuditTheme.BrandSubtle
		};
		Label lblTitle = new Label
		{
			Text = "审批记录",
			Location = new Point(20, 6),
			Size = new Size(300, 26),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 10.5f, FontStyle.Bold),
			ForeColor = AuditTheme.BrandActive
		};
		_lblSummary = new Label
		{
			Text = "项目：" + ProjectDisplay + "　加载中…",
			Location = new Point(22, 33),
			Size = new Size(518, 16),
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			Font = new Font("微软雅黑", 8.5f),
			ForeColor = AuditTheme.TextMuted
		};
		pnlHeader.Controls.Add(lblTitle);
		pnlHeader.Controls.Add(_lblSummary);

		// ---- 中央历史树（长内容可滚动） ----
		Panel pnlBody = new Panel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12, 8, 12, 8),
			BackColor = Color.White
		};
		_tvHistory = new TreeView
		{
			Dock = DockStyle.Fill,
			BorderStyle = BorderStyle.None,
			BackColor = Color.White,
			ForeColor = AuditTheme.Text,
			ItemHeight = 24,
			ShowLines = true,
			HideSelection = true
		};
		pnlBody.Controls.Add(_tvHistory);

		// ---- 底部按钮区（仅关 闭） ----
		Panel pnlButtons = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 56,
			BackColor = AuditTheme.SurfaceMuted
		};
		C1Button btnClose = new C1Button
		{
			Text = "关 闭",
			Location = new Point(560 - RightMargin - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f),
			DialogResult = DialogResult.Cancel
		};
		pnlButtons.Controls.Add(btnClose);

		// 停靠顺序：后添加的先停靠 —— Header 贴顶、按钮贴底、历史树填充剩余
		Controls.Add(pnlButtons);
		Controls.Add(pnlBody);
		Controls.Add(pnlHeader);

		CancelButton = btnClose;
		ApplyRoundedButton(btnClose, 8);
	}

	private string ProjectDisplay
	{
		get { return string.IsNullOrWhiteSpace(_projectName) ? "（未知项目）" : _projectName; }
	}

	private async void FrmReviewHistory_Load(object sender, EventArgs e)
	{
		try
		{
			List<ReviewSubmissionDto> submissions = await Auditai.Util.WebApiClient.GetReviewHistory(_projectId);
			if (IsDisposed)
			{
				return;
			}
			Populate(submissions ?? new List<ReviewSubmissionDto>());
		}
		catch (HttpRequestException ex)
		{
			if (!IsDisposed)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "加载审批记录失败：" + (ex.InnerException?.Message ?? ex.Message));
			}
		}
		catch (Exception ex)
		{
			if (!IsDisposed)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "加载审批记录失败：" + ex.Message);
			}
		}
	}

	/// <summary>按轮次分组重建历史树（每轮：状态/提交时间；子节点：报告信息、上报说明、各级节点审批明细与意见）</summary>
	private void Populate(List<ReviewSubmissionDto> submissions)
	{
		_tvHistory.BeginUpdate();
		try
		{
			_tvHistory.Nodes.Clear();
			foreach (ReviewSubmissionDto sub in submissions.OrderBy((ReviewSubmissionDto s) => s.Round))
			{
				System.Windows.Forms.TreeNode root = new System.Windows.Forms.TreeNode("第 " + sub.Round + " 轮 · " + IndexName(SubmissionStatusNames, sub.Status)
					+ " · 提交 " + DisplayOrDash(sub.SubmitTime) + " ")
				{
					NodeFont = new Font("微软雅黑", 9f, FontStyle.Bold),
					ForeColor = StatusColor(sub.Status)
				};
				root.Nodes.Add("报告类型：" + IndexName(ReportTypeNames, sub.ReportType)
					+ "　报告文号：" + DisplayOrDash(sub.ReportNo)
					+ "　意见类型：" + IndexName(OpinionTypeNames, sub.OpinionType)
					+ "　审批级数：" + sub.TotalLevel + " 级");
				root.Nodes.Add("上报人：" + DisplayOrDash(sub.SubmitterName)
					+ (string.IsNullOrWhiteSpace(sub.Note) ? "" : "　上报说明：" + sub.Note));
				foreach (ReviewNodeDto node in (sub.Nodes ?? new List<ReviewNodeDto>()).OrderBy((ReviewNodeDto n) => n.Level))
				{
					System.Windows.Forms.TreeNode n = root.Nodes.Add("第 " + node.Level + " 级 · " + DisplayOrDash(node.ReviewerName)
						+ " · " + IndexName(NodeStatusNames, node.Status)
						+ " · " + DisplayOrDash(node.ReviewTime));
					n.ForeColor = NodeStatusColor(node.Status);
					if (!string.IsNullOrWhiteSpace(node.Comment))
					{
						n.Nodes.Add("意见：" + node.Comment);
					}
				}
				_tvHistory.Nodes.Add(root);
			}
			if (_tvHistory.Nodes.Count == 0)
			{
				System.Windows.Forms.TreeNode empty = _tvHistory.Nodes.Add("暂无审批记录");
				empty.ForeColor = AuditTheme.TextMuted;
			}
			_tvHistory.ExpandAll();
		}
		finally
		{
			_tvHistory.EndUpdate();
		}
		_lblSummary.Text = "项目：" + ProjectDisplay + "（共 " + submissions.Count + " 轮审核单）";
	}

	private static string DisplayOrDash(string value)
	{
		return string.IsNullOrWhiteSpace(value) ? "—" : value;
	}

	private static string IndexName(string[] names, int index)
	{
		return (index >= 0 && index < names.Length) ? names[index] : "—";
	}

	private static Color StatusColor(int status)
	{
		switch (status)
		{
			case 1:
				return AuditTheme.SuccessText;
			case 2:
				return AuditTheme.ErrorText;
			case 3:
				return AuditTheme.TextMuted;
			default:
				return AuditTheme.Brand;
		}
	}

	private static Color NodeStatusColor(int status)
	{
		switch (status)
		{
			case 1:
				return AuditTheme.SuccessText;
			case 2:
				return AuditTheme.ErrorText;
			default:
				return AuditTheme.TextMuted;
		}
	}

	/// <summary>统一设计语言：给按钮应用 8px 圆角区域</summary>
	private static void ApplyRoundedButton(Control btn, int radius)
	{
		using (var path = new System.Drawing.Drawing2D.GraphicsPath())
		{
			path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
			path.AddArc(btn.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
			path.AddArc(btn.Width - radius * 2, btn.Height - radius * 2, radius * 2, radius * 2, 0, 90);
			path.AddArc(0, btn.Height - radius * 2, radius * 2, radius * 2, 90, 90);
			path.CloseFigure();
			btn.Region = new System.Drawing.Region(path);
		}
	}
}
