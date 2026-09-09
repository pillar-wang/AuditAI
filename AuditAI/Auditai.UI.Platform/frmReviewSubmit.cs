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
/// 上报审核对话框：自动带出项目信息，选择审批流程模板或手工配置 1~3 级审批人，
/// 填写报告类型/意见类型/报告文号/上报说明后调用 Review/Submit 上报。
/// 模板由团队管理员在"流程模板"中预置；套用后级数锁定，非"发起人自选"级别由服务端按模板解析。
/// 审批人候选：WebApiClient.GetTeamUsers()（团队成员，与同事管理同源数据），默认选中团队管理员；加载失败时回退当前登录用户。
/// 相邻两级审批人为同一人时做软提示（行业三级复核规范：报告撰写人与一级复核人原则上不得为同一人），由用户确认后可继续。
/// </summary>
public class frmReviewSubmit : Form
{
	// 统一设计语言常量：按钮 110×40、间距 12、按钮区距右边缘 50px
	private const int BtnWidth = 110;
	private const int BtnHeight = 40;
	private const int BtnGap = 12;
	private const int RightMargin = 50;

	private const int FormWidth = 480;
	private const int HeaderHeight = 44;
	private const int InfoHeight = 34;
	private const int BtnAreaHeight = 56;

	private const int LabelX = 24;
	private const int LabelWidth = 94;
	private const int ControlX = 122;
	private const int ControlWidth = FormWidth - ControlX - 24;

	// 表单行纵坐标（两列 label+control 布局；首行为审批流程模板选择）
	private const int RowFlow = 14;
	private const int RowReportType = 56;
	private const int RowOpinionType = 98;
	private const int RowReportNo = 140;
	private const int RowNote = 182;
	private const int NoteHeight = 64;
	private const int RowLevel = 254;
	private const int ReviewersTop = 296;
	private const int ReviewerPitch = 42;

	// .NET Framework 的 TextBox 无 PlaceholderText，用 EM_SETCUEBANNER 实现输入提示
	private const int EM_SETCUEBANNER = 0x1501;

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

	private static readonly string[] ReportTypeNames = { "年报审计", "中期审计", "专项审计", "其他" };
	private static readonly string[] OpinionTypeNames = { "无保留意见", "保留意见", "否定意见", "无法表示意见", "其他" };
	private static readonly string[] LevelNames = { "一级", "二级", "三级" };

	private readonly Auditai.DTO.Project _project;
	private List<Auditai.DTO.User> _users = new List<Auditai.DTO.User>();
	private List<ReviewFlowTemplateDto> _templates = new List<ReviewFlowTemplateDto>();
	/// <summary>当前套用的模板（null=手工配置）</summary>
	private ReviewFlowTemplateDto _selectedTemplate;
	/// <summary>各行是否锁定（模板指定/团队管理员级别，服务端自行解析）</summary>
	private readonly bool[] _rowLocked = new bool[3];

	private ComboBox _cmbFlow;
	private ComboBox _cmbReportType;
	private ComboBox _cmbOpinionType;
	private TextBox _txtReportNo;
	private TextBox _txtNote;
	private ComboBox _cmbLevel;
	private readonly ComboBox[] _cmbReviewers = new ComboBox[3];
	private readonly Label[] _lblReviewers = new Label[3];
	private Label _lblReviewerHint;
	private C1Button _btnSubmit;
	private C1Button _btnCancel;

	/// <summary>弹出上报对话框（自动带出项目信息）。返回 OK 表示已成功上报。</summary>
	public static DialogResult ShowSubmit(IWin32Window owner, Auditai.DTO.Project project)
	{
		using (frmReviewSubmit dlg = new frmReviewSubmit(project))
		{
			return dlg.ShowDialog(owner);
		}
	}

	public frmReviewSubmit(Auditai.DTO.Project project)
	{
		_project = project;
		InitializeComponent();
		// 先应用主题（浅蓝小清新，作用于 C1 控件等），再重设主/次按钮配色（C1 皮肤会覆盖自定义配色）
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		RefreshButtonStyles();
		UpdateReviewerRows();
		Load += FrmReviewSubmit_Load;
	}

	private void InitializeComponent()
	{
		Text = "上报审核";
		ClientSize = new Size(FormWidth, 506);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		Font = new Font("微软雅黑", 9f);
		BackColor = Color.White;

		// ---- 顶部标题栏（浅蓝背景，标题内边距 20 不紧贴边缘） ----
		Panel pnlHeader = new Panel
		{
			Dock = DockStyle.Top,
			Height = HeaderHeight,
			BackColor = AuditTheme.BrandSubtle
		};
		Label lblTitle = new Label
		{
			Text = "上报审核",
			Location = new Point(20, 0),
			Size = new Size(FormWidth - 40, HeaderHeight),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 10.5f, FontStyle.Bold),
			ForeColor = AuditTheme.BrandActive
		};
		pnlHeader.Controls.Add(lblTitle);

		// ---- 项目信息区（只读展示，浅蓝底色块） ----
		Panel pnlInfo = new Panel
		{
			Dock = DockStyle.Top,
			Height = InfoHeight,
			BackColor = AuditTheme.SurfaceMuted
		};
		Label lblProject = new Label
		{
			Text = "项目：" + ProjectDisplay,
			Location = new Point(20, 0),
			Size = new Size(FormWidth - 40, InfoHeight),
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			ForeColor = AuditTheme.TextSecondary
		};
		pnlInfo.Controls.Add(lblProject);

		// ---- 表单区（两列 label+control） ----
		Panel pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

		Label lblFlow = MakeFormLabel("审批流程", RowFlow);
		_cmbFlow = MakeCombo(RowFlow);
		_cmbFlow.Items.Add("手工配置");
		_cmbFlow.SelectedIndex = 0;
		_cmbFlow.SelectedIndexChanged += ApplyFlowSelection;

		Label lblReportType = MakeFormLabel("报告类型", RowReportType);
		_cmbReportType = MakeCombo(RowReportType);
		_cmbReportType.Items.AddRange(ReportTypeNames);
		_cmbReportType.SelectedIndex = 0;

		Label lblOpinionType = MakeFormLabel("意见类型", RowOpinionType);
		_cmbOpinionType = MakeCombo(RowOpinionType);
		_cmbOpinionType.Items.AddRange(OpinionTypeNames);
		_cmbOpinionType.SelectedIndex = 0;

		Label lblReportNo = MakeFormLabel("报告文号", RowReportNo);
		_txtReportNo = new TextBox
		{
			Location = new Point(ControlX, RowReportNo),
			Size = new Size(ControlWidth, 30)
		};

		Label lblNote = MakeFormLabel("上报说明", RowNote);
		_txtNote = new TextBox
		{
			Location = new Point(ControlX, RowNote),
			Size = new Size(ControlWidth, NoteHeight),
			Multiline = true,
			ScrollBars = ScrollBars.Vertical
		};

		Label lblLevel = MakeFormLabel("审批级数", RowLevel);
		_cmbLevel = new ComboBox
		{
			Location = new Point(ControlX, RowLevel),
			Size = new Size(120, 30),
			DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		};
		_cmbLevel.Items.AddRange(new object[] { "1级", "2级", "3级" });
		_cmbLevel.SelectedIndex = 0;
		_cmbLevel.SelectedIndexChanged += delegate
		{
			UpdateReviewerRows();
		};

		for (int i = 0; i < 3; i++)
		{
			_lblReviewers[i] = MakeFormLabel(LevelNames[i] + "审批人 *", ReviewersTop + i * ReviewerPitch);
			_cmbReviewers[i] = MakeCombo(ReviewersTop + i * ReviewerPitch);
		}
		_lblReviewerHint = new Label
		{
			Text = "提示：相邻两级审批人不宜为同一人",
			Location = new Point(ControlX, ReviewersTop + 3 * ReviewerPitch + 2),
			Size = new Size(ControlWidth, 18),
			Font = new Font("微软雅黑", 8.5f),
			ForeColor = AuditTheme.TextMuted
		};

		List<Control> bodyControls = new List<Control>
		{
			lblFlow, _cmbFlow, lblReportType, _cmbReportType, lblOpinionType, _cmbOpinionType,
			lblReportNo, _txtReportNo, lblNote, _txtNote, lblLevel, _cmbLevel,
			_lblReviewerHint
		};
		bodyControls.AddRange(_lblReviewers);
		bodyControls.AddRange(_cmbReviewers);
		pnlBody.Controls.AddRange(bodyControls.ToArray());

		// ---- 底部按钮区（提 交：110px 宽、距右边缘 50px、间距 12px） ----
		Panel pnlButtons = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = BtnAreaHeight,
			BackColor = AuditTheme.SurfaceMuted
		};
		_btnSubmit = new C1Button
		{
			Text = "提 交",
			Location = new Point(FormWidth - RightMargin - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f, FontStyle.Bold)
		};
		_btnSubmit.Click += BtnSubmit_Click;
		_btnCancel = new C1Button
		{
			Text = "取 消",
			Location = new Point(FormWidth - RightMargin - BtnWidth - BtnGap - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f),
			DialogResult = DialogResult.Cancel
		};
		pnlButtons.Controls.Add(_btnSubmit);
		pnlButtons.Controls.Add(_btnCancel);

		// 停靠顺序：后添加的先停靠 —— Header 贴顶、Info 其次、按钮贴底、表单区填充剩余
		Controls.Add(pnlButtons);
		Controls.Add(pnlBody);
		Controls.Add(pnlInfo);
		Controls.Add(pnlHeader);

		AcceptButton = _btnSubmit;
		CancelButton = _btnCancel;
	}

	/// <summary>项目信息展示文本（项目名 + 编号）</summary>
	private string ProjectDisplay
	{
		get
		{
			string name = string.IsNullOrWhiteSpace(_project?.Name) ? "（未命名项目）" : _project.Name;
			string no = string.IsNullOrWhiteSpace(_project?.Number) ? "—" : _project.Number;
			return name + "（编号：" + no + "）";
		}
	}

	private static Label MakeFormLabel(string text, int y)
	{
		return new Label
		{
			Text = text,
			Location = new Point(LabelX, y + 4),
			Size = new Size(LabelWidth, 22),
			TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = AuditTheme.TextSecondary
		};
	}

	private static ComboBox MakeCombo(int y)
	{
		return new ComboBox
		{
			Location = new Point(ControlX, y),
			Size = new Size(ControlWidth, 30),
			DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		};
	}

	/// <summary>按审批级数显隐各审批人行，并同步调整窗体高度（固定尺寸弹窗，仅高度随级数变化）</summary>
	private void UpdateReviewerRows()
	{
		int level = (_cmbLevel.SelectedIndex >= 0) ? _cmbLevel.SelectedIndex + 1 : 1;
		for (int i = 0; i < 3; i++)
		{
			bool visible = i < level;
			_lblReviewers[i].Visible = visible;
			_cmbReviewers[i].Visible = visible;
		}
		_lblReviewerHint.Location = new Point(ControlX, ReviewersTop + level * ReviewerPitch + 2);
		ClientSize = new Size(FormWidth, HeaderHeight + InfoHeight + BtnAreaHeight + ReviewersTop + level * ReviewerPitch + 34);
	}

	// ==================== 数据加载 ====================

	private async void FrmReviewSubmit_Load(object sender, EventArgs e)
	{
		// 输入提示须在句柄创建后设置
		SendMessage(_txtReportNo.Handle, EM_SETCUEBANNER, (IntPtr)1, "如：沪华审字[2026]第018号");
		SendMessage(_txtNote.Handle, EM_SETCUEBANNER, (IntPtr)1, "可填写审计范围、重点事项等说明");
		await LoadReviewersAsync();
		await LoadTemplatesAsync();
	}

	/// <summary>加载团队审批流程模板（仅启用项）；加载失败保持仅"手工配置"可选</summary>
	private async Task LoadTemplatesAsync()
	{
		List<ReviewFlowTemplateDto> templates;
		try
		{
			templates = await Auditai.Util.WebApiClient.GetReviewFlowTemplates();
		}
		catch
		{
			return; // 网络等异常时保持手工配置模式，不阻断上报
		}
		if (templates == null || templates.Count == 0)
		{
			return;
		}
		_templates = templates;
		_cmbFlow.SelectedIndexChanged -= ApplyFlowSelection;
		try
		{
			int prev = Math.Max(_cmbFlow.SelectedIndex, 0);
			foreach (ReviewFlowTemplateDto t in _templates)
			{
				_cmbFlow.Items.Add(t.Name + (t.Enabled == 1 ? "" : "（已停用）"));
			}
			_cmbFlow.SelectedIndex = prev; // 恢复"手工配置"，不自动套用
		}
		finally
		{
			_cmbFlow.SelectedIndexChanged += ApplyFlowSelection;
		}
	}

	/// <summary>审批流程下拉切换：手工配置恢复可编辑；套用模板则按节点锁定/开放各行</summary>
	private void ApplyFlowSelection(object sender, EventArgs e)
	{
		int idx = _cmbFlow.SelectedIndex;
		if (idx <= 0)
		{
			_selectedTemplate = null;
			_cmbLevel.Enabled = true;
			for (int i = 0; i < 3; i++)
			{
				_rowLocked[i] = false;
				_cmbReviewers[i].Enabled = true;
				// 移除模板占位项，恢复与 _users 的索引对应
				ComboBox cmb = _cmbReviewers[i];
				while (cmb.Items.Count > _users.Count)
				{
					cmb.Items.RemoveAt(cmb.Items.Count - 1);
				}
				if (cmb.SelectedIndex >= _users.Count)
				{
					FillReviewerCombo(cmb);
				}
			}
			return;
		}

		ReviewFlowTemplateDto template = _templates[idx - 1];
		if (template.Enabled != 1)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该审批流程模板已停用，请选择其他流程或手工配置");
			_cmbFlow.SelectedIndexChanged -= ApplyFlowSelection;
			try { _cmbFlow.SelectedIndex = 0; } finally { _cmbFlow.SelectedIndexChanged += ApplyFlowSelection; }
			ApplyFlowSelection(this, EventArgs.Empty);
			return;
		}
		_selectedTemplate = template;

		// 级数与节点按模板锁定
		_cmbLevel.Enabled = false;
		if (_cmbLevel.SelectedIndex != template.TotalLevel - 1)
		{
			_cmbLevel.SelectedIndex = template.TotalLevel - 1; // 触发 UpdateReviewerRows
		}
		else
		{
			UpdateReviewerRows();
		}

		List<ReviewFlowNodeDto> nodes = template.Nodes.OrderBy((ReviewFlowNodeDto n) => n.Level).ToList();
		// 防御 nodes.Count < TotalLevel 的并发不一致（管理员同时编辑模板）：缺失行按自选开放，提交时服务端按模板解析
		for (int i = 0; i < template.TotalLevel && i < nodes.Count && i < 3; i++)
		{
			ReviewFlowNodeDto node = nodes[i];
			if (node.AssigneeType == 2)
			{
				// 发起人自选：开放选择
				_rowLocked[i] = false;
				_cmbReviewers[i].Enabled = true;
				continue;
			}
			_rowLocked[i] = true;
			_cmbReviewers[i].Enabled = false;
			SelectOrMarkReviewer(_cmbReviewers[i], node);
		}
		for (int i = nodes.Count; i < template.TotalLevel && i < 3; i++)
		{
			_rowLocked[i] = false;
			_cmbReviewers[i].Enabled = true;
		}
	}

	/// <summary>锁定行：优先按 ReviewerId 选中成员；找不到则追加占位项（服务端按模板自行解析审批人）</summary>
	private void SelectOrMarkReviewer(ComboBox cmb, ReviewFlowNodeDto node)
	{
		int idx = node.AssigneeType == 0
			? _users.FindIndex((Auditai.DTO.User u) => u.Id == node.ReviewerId)
			: _users.FindIndex((Auditai.DTO.User u) => u.IsTeamAdmin);
		if (idx >= 0)
		{
			cmb.SelectedIndex = idx;
			return;
		}
		// 占位项：index >= _users.Count，提交时跳过该行，由服务端解析
		string mark = node.AssigneeType == 0
			? (string.IsNullOrWhiteSpace(node.ReviewerName) ? "模板指定审批人" : node.ReviewerName + "（模板指定）")
			: "团队管理员（自动解析）";
		cmb.BeginUpdate();
		try
		{
			// 移除旧的占位项，保留团队成员项在前（与 _users 一一对应）
			while (cmb.Items.Count > _users.Count)
			{
				cmb.Items.RemoveAt(cmb.Items.Count - 1);
			}
			cmb.Items.Add(mark);
			cmb.SelectedIndex = cmb.Items.Count - 1;
		}
		finally
		{
			cmb.EndUpdate();
		}
	}

	/// <summary>加载团队成员作为审批人候选（默认选中团队管理员）；加载失败或为空时回退当前登录用户</summary>
	private async Task LoadReviewersAsync()
	{
		List<Auditai.DTO.User> users = new List<Auditai.DTO.User>();
		try
		{
			IEnumerable<Auditai.DTO.User> teamUsers = await Auditai.Util.WebApiClient.GetTeamUsers();
			if (teamUsers != null)
			{
				users = teamUsers.Where((Auditai.DTO.User u) => u != null).OrderByDescending((Auditai.DTO.User u) => u.IsTeamAdmin).ToList();
			}
		}
		catch
		{
			// 网络等异常时回退当前用户，保证对话框可用
		}
		if (users.Count == 0)
		{
			Auditai.Model.User cur = Auditai.Model.User.Current;
			if (cur != null)
			{
				users.Add(new Auditai.DTO.User
				{
					Id = cur.Id,
					UserName = cur.UserName,
					Name = cur.Name,
					IsTeamAdmin = cur.IsTeamAdmin
				});
			}
		}
		_users = users;
		foreach (ComboBox cmb in _cmbReviewers)
		{
			FillReviewerCombo(cmb);
		}
	}

	private void FillReviewerCombo(ComboBox cmb)
	{
		cmb.BeginUpdate();
		try
		{
			cmb.Items.Clear();
			foreach (Auditai.DTO.User u in _users)
			{
				string display = string.IsNullOrWhiteSpace(u.Name) ? u.UserName : u.Name;
				if (u.IsTeamAdmin)
				{
					display += "（团队管理员）";
				}
				cmb.Items.Add(display);
			}
		}
		finally
		{
			cmb.EndUpdate();
		}
		int idx = _users.FindIndex((Auditai.DTO.User u) => u.IsTeamAdmin);
		if (idx < 0)
		{
			idx = _users.FindIndex((Auditai.DTO.User u) => u.Id == (Auditai.Model.User.Current?.Id ?? 0));
		}
		cmb.SelectedIndex = (cmb.Items.Count > 0) ? Math.Max(idx, 0) : -1;
	}

	// ==================== 提交 ====================

	private async void BtnSubmit_Click(object sender, EventArgs e)
	{
		bool byTemplate = _selectedTemplate != null;
		int level = byTemplate ? _selectedTemplate.TotalLevel : (_cmbLevel.SelectedIndex >= 0 ? _cmbLevel.SelectedIndex + 1 : 1);
		// 必填校验：各级审批人已选（报告文号/上报说明可空；模板锁定行可由服务端解析，跳过）
		for (int i = 0; i < level; i++)
		{
			bool locked = byTemplate && _rowLocked[i];
			if (!locked && _cmbReviewers[i].SelectedIndex < 0)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择" + LevelNames[i] + "审批人");
				_cmbReviewers[i].Focus();
				return;
			}
		}
		// 相邻两级审批人为同一人：软提示，确认后可继续（模板模式流转由预置流程决定，不再提示）
		if (!byTemplate)
		{
			for (int i = 1; i < level; i++)
			{
				long prevId = _users[_cmbReviewers[i - 1].SelectedIndex].Id;
				long curId = _users[_cmbReviewers[i].SelectedIndex].Id;
				if (prevId == curId && Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question,
					LevelNames[i - 1] + "级与" + LevelNames[i] + "级审批人为同一人，建议逐级由不同人员复核，确定继续提交吗？",
					MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					return;
				}
			}
		}
		List<ReviewReviewerItem> reviewers = new List<ReviewReviewerItem>();
		for (int i = 0; i < level; i++)
		{
			// 模板锁定行可能选中占位项（index >= _users.Count）：跳过，由服务端按模板解析
			if (byTemplate && _rowLocked[i])
			{
				continue;
			}
			if (_cmbReviewers[i].SelectedIndex < 0 || _cmbReviewers[i].SelectedIndex >= _users.Count)
			{
				continue;
			}
			reviewers.Add(new ReviewReviewerItem
			{
				Level = i + 1,
				UserId = _users[_cmbReviewers[i].SelectedIndex].Id
			});
		}
		ReviewSubmitRequest req = new ReviewSubmitRequest
		{
			ProjectId = _project.Id.ToString(),
			ReportType = _cmbReportType.SelectedIndex,
			ReportNo = _txtReportNo.Text.Trim(),
			OpinionType = _cmbOpinionType.SelectedIndex,
			Note = _txtNote.Text.Trim(),
			TemplateId = byTemplate ? _selectedTemplate.Id : null,
			Reviewers = reviewers
		};
		_btnSubmit.Enabled = false;
		try
		{
			await Auditai.Util.WebApiClient.SubmitReview(req);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "上报成功，等待审批");
			DialogResult = DialogResult.OK;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "上报失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
		catch (ServerException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "上报失败：" + ex.Message);
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "上报失败：" + ex.Message);
		}
		finally
		{
			// 成功路径下窗体已关闭，避免操作已释放控件
			if (!IsDisposed)
			{
				_btnSubmit.Enabled = true;
			}
		}
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

	/// <summary>统一设计语言：主按钮品牌蓝底白字，次按钮白底灰字浅灰边框</summary>
	private void RefreshButtonStyles()
	{
		_btnSubmit.FlatStyle = FlatStyle.Flat;
		_btnSubmit.FlatAppearance.BorderSize = 0;
		_btnSubmit.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		_btnSubmit.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		_btnSubmit.ForeColor = Color.White;
		_btnSubmit.BackColor = AuditTheme.Brand;
		_btnSubmit.UseVisualStyleBackColor = false;
		ApplyRoundedButton(_btnSubmit, 8);
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
