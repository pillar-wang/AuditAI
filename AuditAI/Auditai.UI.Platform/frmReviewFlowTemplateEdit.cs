using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Input;
using Auditai.DTO;

namespace Auditai.UI.Platform;

/// <summary>
/// 审批流程模板编辑对话框（仅团队管理员）：配置模板名称、1~3 级审批节点。
/// 每级指定方式：指定人（选择团队成员）/ 团队管理员（上报时自动解析）/ 发起人自选（提交人选择）。
/// 保存调用 ReviewFlow/SaveTemplate，服务端做名称唯一、级数连续等校验。
/// </summary>
public class frmReviewFlowTemplateEdit : Form
{
	// 统一设计语言常量：按钮 110×40、间距 12、按钮区距右边缘 50px
	private const int BtnWidth = 110;
	private const int BtnHeight = 40;
	private const int BtnGap = 12;
	private const int RightMargin = 50;

	private const int FormWidth = 480;
	private const int HeaderHeight = 44;
	private const int BtnAreaHeight = 56;

	private const int LabelX = 24;
	private const int LabelWidth = 94;
	private const int ControlX = 122;
	private const int ControlWidth = FormWidth - ControlX - 24;

	private const int RowName = 14;
	private const int RowLevelCount = 56;
	private const int LevelsTop = 98;
	private const int LevelPitch = 42;

	private static readonly string[] LevelNames = { "一级", "二级", "三级" };
	private static readonly string[] AssigneeTypeNames = { "指定人", "团队管理员", "发起人自选" };

	private readonly ReviewFlowTemplateDto _existing;
	private List<Auditai.DTO.User> _users = new List<Auditai.DTO.User>();

	private TextBox _txtName;
	private ComboBox _cmbLevelCount;
	private readonly ComboBox[] _cmbAssigneeType = new ComboBox[3];
	private readonly ComboBox[] _cmbReviewers = new ComboBox[3];
	private Label _lblHint;
	private C1Button _btnSave;
	private C1Button _btnCancel;
	private bool _loading;
	/// <summary>编辑模式待选中的指定人（LoadUsersAsync 完成后按此选中）</summary>
	private readonly long[] _pendingReviewer = new long[3];

	/// <summary>弹出编辑对话框；existing 为 null 表示新建。返回保存成功后的模板，取消返回 null。</summary>
	public static async Task<ReviewFlowTemplateDto> ShowEditAsync(IWin32Window owner, ReviewFlowTemplateDto existing)
	{
		using (frmReviewFlowTemplateEdit dlg = new frmReviewFlowTemplateEdit(existing))
		{
			return await dlg.ShowDialogAsync(owner);
		}
	}

	/// <summary>模态显示并异步等待结果（避免 async void 事件阻塞关闭）。</summary>
	private async Task<ReviewFlowTemplateDto> ShowDialogAsync(IWin32Window owner)
	{
		var tcs = new TaskCompletionSource<ReviewFlowTemplateDto>();
		Load += async (s, e) =>
		{
			try { await LoadUsersAsync(); }
			catch { /* 候选人加载失败不阻断编辑，保存时由服务端校验 */ }
		};
		FormClosed += (s, e) =>
		{
			if (!tcs.TrySetResult(_saved)) tcs.TrySetResult(null);
		};
		ShowDialog(owner);
		return await tcs.Task;
	}

	private ReviewFlowTemplateDto _saved;

	public frmReviewFlowTemplateEdit(ReviewFlowTemplateDto existing)
	{
		_existing = existing;
		InitializeComponent();
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		RefreshButtonStyles();
		UpdateLevelRows();
	}

	private void InitializeComponent()
	{
		Text = _existing == null ? "新建审批流程模板" : "编辑审批流程模板";
		ClientSize = new Size(FormWidth, 380);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		Font = new Font("微软雅黑", 9f);
		BackColor = Color.White;

		// ---- 顶部标题栏（浅蓝背景） ----
		Panel pnlHeader = new Panel
		{
			Dock = DockStyle.Top,
			Height = HeaderHeight,
			BackColor = AuditTheme.BrandSubtle
		};
		Label lblTitle = new Label
		{
			Text = _existing == null ? "新建审批流程模板" : "编辑审批流程模板",
			Location = new Point(20, 0),
			Size = new Size(FormWidth - 40, HeaderHeight),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 10.5f, FontStyle.Bold),
			ForeColor = AuditTheme.BrandActive
		};
		pnlHeader.Controls.Add(lblTitle);

		// ---- 表单区 ----
		Panel pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

		Label lblName = MakeFormLabel("模板名称 *", RowName);
		_txtName = new TextBox
		{
			Location = new Point(ControlX, RowName),
			Size = new Size(ControlWidth, 30),
			MaxLength = 50
		};

		Label lblLevelCount = MakeFormLabel("审批级数", RowLevelCount);
		_cmbLevelCount = new ComboBox
		{
			Location = new Point(ControlX, RowLevelCount),
			Size = new Size(120, 30),
			DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		};
		_cmbLevelCount.Items.AddRange(new object[] { "1级", "2级", "3级" });
		_cmbLevelCount.SelectedIndex = 0;
		_cmbLevelCount.SelectedIndexChanged += delegate
		{
			UpdateLevelRows();
		};

		for (int i = 0; i < 3; i++)
		{
			Label lblLevel = MakeFormLabel(LevelNames[i] + "审批人 *", LevelsTop + i * LevelPitch);
			_cmbAssigneeType[i] = new ComboBox
			{
				Location = new Point(ControlX, LevelsTop + i * LevelPitch),
				Size = new Size(140, 30),
				DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
			};
			_cmbAssigneeType[i].Items.AddRange(AssigneeTypeNames);
			_cmbAssigneeType[i].SelectedIndex = 0;
			_cmbAssigneeType[i].Tag = lblLevel;
			_cmbAssigneeType[i].SelectedIndexChanged += delegate
			{
				UpdateAssigneeRowEnabled();
			};
			_cmbReviewers[i] = new ComboBox
			{
				Location = new Point(ControlX + 150, LevelsTop + i * LevelPitch),
				Size = new Size(ControlWidth - 150, 30),
				DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
			};
			pnlBody.Controls.Add(lblLevel);
			pnlBody.Controls.Add(_cmbAssigneeType[i]);
			pnlBody.Controls.Add(_cmbReviewers[i]);
		}

		_lblHint = new Label
		{
			Text = "提示：报告撰写人与一级复核人原则上不宜为同一人",
			Location = new Point(ControlX, LevelsTop + 3 * LevelPitch + 2),
			Size = new Size(ControlWidth, 18),
			Font = new Font("微软雅黑", 8.5f),
			ForeColor = AuditTheme.TextMuted
		};
		pnlBody.Controls.Add(_lblHint);
		pnlBody.Controls.Add(lblName);
		pnlBody.Controls.Add(_txtName);
		pnlBody.Controls.Add(lblLevelCount);
		pnlBody.Controls.Add(_cmbLevelCount);

		// ---- 底部按钮区 ----
		Panel pnlButtons = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = BtnAreaHeight,
			BackColor = AuditTheme.SurfaceMuted
		};
		_btnSave = new C1Button
		{
			Text = "保 存",
			Location = new Point(FormWidth - RightMargin - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f, FontStyle.Bold)
		};
		_btnSave.Click += BtnSave_Click;
		_btnCancel = new C1Button
		{
			Text = "取 消",
			Location = new Point(FormWidth - RightMargin - BtnWidth - BtnGap - BtnWidth, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f),
			DialogResult = DialogResult.Cancel
		};
		pnlButtons.Controls.Add(_btnSave);
		pnlButtons.Controls.Add(_btnCancel);

		// 停靠顺序：后添加的先停靠 —— Header 贴顶、按钮贴底、表单区填充剩余
		Controls.Add(pnlButtons);
		Controls.Add(pnlBody);
		Controls.Add(pnlHeader);

		CancelButton = _btnCancel;

		if (_existing != null)
		{
			_txtName.Text = _existing.Name ?? "";
			_cmbLevelCount.SelectedIndex = Math.Max(0, Math.Min(2, _existing.TotalLevel - 1));
			List<ReviewFlowNodeDto> nodes = _existing.Nodes.OrderBy((ReviewFlowNodeDto n) => n.Level).ToList();
			for (int i = 0; i < nodes.Count && i < 3; i++)
			{
				_cmbAssigneeType[i].SelectedIndex = Math.Max(0, Math.Min(2, nodes[i].AssigneeType));
				if (nodes[i].AssigneeType == 0 && nodes[i].ReviewerId > 0)
				{
					_pendingReviewer[i] = nodes[i].ReviewerId; // LoadUsersAsync 完成后选中
				}
			}
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

	/// <summary>按审批级数显隐各行、跟随定位提示标签并调整窗体高度（固定尺寸弹窗，仅高度随级数变化）</summary>
	private void UpdateLevelRows()
	{
		_loading = true;
		try
		{
			int level = (_cmbLevelCount.SelectedIndex >= 0) ? _cmbLevelCount.SelectedIndex + 1 : 1;
			for (int i = 0; i < 3; i++)
			{
				bool visible = i < level;
				((Control)_cmbAssigneeType[i].Tag).Visible = visible;
				_cmbAssigneeType[i].Visible = visible;
				_cmbReviewers[i].Visible = visible;
			}
			// 提示标签跟随最后一级可见行，避免固定在三级行位置时被按钮区裁切
			_lblHint.Location = new Point(ControlX, LevelsTop + level * LevelPitch + 2);
			ClientSize = new Size(FormWidth, HeaderHeight + BtnAreaHeight + LevelsTop + level * LevelPitch + 56);
		}
		finally
		{
			_loading = false;
		}
		UpdateAssigneeRowEnabled();
	}

	/// <summary>仅"指定人"级别启用审批人下拉</summary>
	private void UpdateAssigneeRowEnabled()
	{
		if (_loading) return;
		for (int i = 0; i < 3; i++)
		{
			_cmbReviewers[i].Enabled = _cmbAssigneeType[i].SelectedIndex == 0;
		}
	}

	/// <summary>加载团队成员作为指定人候选（与上报对话框同源）</summary>
	private async Task LoadUsersAsync()
	{
		try
		{
			IEnumerable<Auditai.DTO.User> teamUsers = await Auditai.Util.WebApiClient.GetTeamUsers();
			if (teamUsers != null)
			{
				_users = teamUsers.Where((Auditai.DTO.User u) => u != null).ToList();
			}
		}
		catch
		{
			// 保持空候选，指定人级别在保存时由服务端校验
		}
		foreach (ComboBox cmb in _cmbReviewers)
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
		}
		// 编辑模式：选中模板指定的审批人
		for (int i = 0; i < 3; i++)
		{
			if (_pendingReviewer[i] > 0)
			{
				int idx = _users.FindIndex((Auditai.DTO.User u) => u.Id == _pendingReviewer[i]);
				if (idx >= 0)
				{
					_cmbReviewers[i].SelectedIndex = idx;
				}
			}
		}
	}

	private async void BtnSave_Click(object sender, EventArgs e)
	{
		if (Auditai.Util.WebApiClient.IsLocalMode)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持审批流程模板，请登录云端后使用");
			return;
		}
		string name = _txtName.Text.Trim();
		if (string.IsNullOrEmpty(name))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请填写模板名称");
			_txtName.Focus();
			return;
		}
		int level = (_cmbLevelCount.SelectedIndex >= 0) ? _cmbLevelCount.SelectedIndex + 1 : 1;
		ReviewFlowTemplateDto template = new ReviewFlowTemplateDto
		{
			Id = _existing?.Id,
			Name = name,
			TotalLevel = level,
			// 编辑保留原启用状态（启用/停用由列表页"启/停用"显式操作，避免编辑静默重新启用）
			Enabled = _existing?.Enabled ?? 1
		};
		for (int i = 0; i < level; i++)
		{
			int assigneeType = _cmbAssigneeType[i].SelectedIndex;
			long reviewerId = 0;
			if (assigneeType == 0)
			{
				if (_cmbReviewers[i].SelectedIndex < 0)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择" + LevelNames[i] + "审批人");
					_cmbReviewers[i].Focus();
					return;
				}
				reviewerId = _users[_cmbReviewers[i].SelectedIndex].Id;
			}
			template.Nodes.Add(new ReviewFlowNodeDto
			{
				Level = i + 1,
				AssigneeType = assigneeType,
				ReviewerId = reviewerId
			});
		}
		_btnSave.Enabled = false;
		try
		{
			_saved = await Auditai.Util.WebApiClient.SaveReviewFlowTemplate(template);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "模板已保存");
			Close();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "保存失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "保存失败：" + ex.Message);
		}
		finally
		{
			if (!IsDisposed)
			{
				_btnSave.Enabled = true;
			}
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

	/// <summary>统一设计语言：主按钮品牌蓝底白字，次按钮白底灰字浅灰边框</summary>
	private void RefreshButtonStyles()
	{
		_btnSave.FlatStyle = FlatStyle.Flat;
		_btnSave.FlatAppearance.BorderSize = 0;
		_btnSave.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		_btnSave.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		_btnSave.ForeColor = Color.White;
		_btnSave.BackColor = AuditTheme.Brand;
		_btnSave.UseVisualStyleBackColor = false;
		ApplyRoundedButton(_btnSave, 8);
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
