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
/// 审批流程模板管理对话框（仅团队管理员入口）：模板列表 + 新建/编辑/删除/启用停用。
/// 数据源 ReviewFlow/GetTemplates?includeDisabled=1；删除/保存均即时生效，已生成的审核单不受影响。
/// </summary>
public class frmReviewFlowTemplates : Form
{
	// 统一设计语言常量：按钮 110×40、间距 12、按钮区距右边缘 50px
	private const int BtnWidth = 110;
	private const int BtnHeight = 40;
	private const int BtnGap = 12;
	private const int RightMargin = 50;

	// 窗体宽度须容纳底部 6 按钮：6×110 + 5×12 + 右边距 50 + 左侧余量 ≥ 790
	private const int FormWidth = 800;
	private const int HeaderHeight = 44;
	private const int BtnAreaHeight = 56;

	private ListView _list;
	private C1Button _btnNew;
	private C1Button _btnEdit;
	private C1Button _btnDelete;
	private C1Button _btnToggle;
	private C1Button _btnRefresh;
	private C1Button _btnClose;
	private List<ReviewFlowTemplateDto> _templates = new List<ReviewFlowTemplateDto>();

	public frmReviewFlowTemplates()
	{
		InitializeComponent();
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		RefreshButtonStyles();
	}

	private void InitializeComponent()
	{
		Text = "审批流程模板";
		ClientSize = new Size(FormWidth, 480);
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
			Text = "审批流程模板",
			Location = new Point(20, 0),
			Size = new Size(FormWidth - 40, HeaderHeight),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 10.5f, FontStyle.Bold),
			ForeColor = AuditTheme.BrandActive
		};
		pnlHeader.Controls.Add(lblTitle);

		// ---- 列表区 ----
		Panel pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20, 12, 20, 12) };
		_list = new ListView
		{
			Dock = DockStyle.Fill,
			View = View.Details,
			FullRowSelect = true,
			HideSelection = false,
			MultiSelect = false,
			BorderStyle = BorderStyle.FixedSingle
		};
		_list.Columns.Add("模板名称", 220);
		_list.Columns.Add("审批级数", 90);
		_list.Columns.Add("状态", 90);
		_list.Columns.Add("更新时间", 160);
		_list.DoubleClick += delegate
		{
			EditSelected();
		};
		_list.SelectedIndexChanged += delegate
		{
			UpdateSelectionButtons();
		};
		pnlBody.Controls.Add(_list);

		// ---- 底部按钮区 ----
		Panel pnlButtons = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = BtnAreaHeight,
			BackColor = AuditTheme.SurfaceMuted
		};
		_btnClose = MakeButton("关 闭", FormWidth - RightMargin - BtnWidth, DialogResult.Cancel);
		_btnRefresh = MakeButton("刷 新", FormWidth - RightMargin - BtnWidth - BtnGap - BtnWidth);
		_btnToggle = MakeButton("启/停用", FormWidth - RightMargin - (BtnWidth + BtnGap) * 2 - BtnWidth);
		_btnDelete = MakeButton("删 除", FormWidth - RightMargin - (BtnWidth + BtnGap) * 3 - BtnWidth);
		_btnEdit = MakeButton("编 辑", FormWidth - RightMargin - (BtnWidth + BtnGap) * 4 - BtnWidth);
		_btnNew = MakeButton("新 建", FormWidth - RightMargin - (BtnWidth + BtnGap) * 5 - BtnWidth);
		_btnNew.Click += async delegate
		{
			await NewTemplateAsync();
		};
		_btnEdit.Click += delegate
		{
			EditSelected();
		};
		_btnDelete.Click += async delegate
		{
			await DeleteSelectedAsync();
		};
		_btnToggle.Click += async delegate
		{
			await ToggleSelectedAsync();
		};
		_btnRefresh.Click += async delegate
		{
			await LoadTemplatesAsync();
		};
		pnlButtons.Controls.Add(_btnClose);
		pnlButtons.Controls.Add(_btnRefresh);
		pnlButtons.Controls.Add(_btnToggle);
		pnlButtons.Controls.Add(_btnDelete);
		pnlButtons.Controls.Add(_btnEdit);
		pnlButtons.Controls.Add(_btnNew);

		// 停靠顺序：后添加的先停靠 —— Header 贴顶、按钮贴底、列表区填充剩余
		Controls.Add(pnlButtons);
		Controls.Add(pnlBody);
		Controls.Add(pnlHeader);

		CancelButton = _btnClose;
		Load += async delegate
		{
			if (Auditai.Util.WebApiClient.IsLocalMode)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持审批流程模板，请登录云端后使用");
				Close();
				return;
			}
			await LoadTemplatesAsync();
		};
	}

	private C1Button MakeButton(string text, int x, DialogResult result = DialogResult.None)
	{
		C1Button btn = new C1Button
		{
			Text = text,
			Location = new Point(x, 8),
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9f),
			DialogResult = result
		};
		return btn;
	}

	/// <summary>加载模板列表（含停用项）</summary>
	private async Task LoadTemplatesAsync()
	{
		try
		{
			_templates = await Auditai.Util.WebApiClient.GetReviewFlowTemplates(includeDisabled: true);
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "加载模板列表失败：" + ex.Message);
			_templates = new List<ReviewFlowTemplateDto>();
		}
		_list.BeginUpdate();
		try
		{
			_list.Items.Clear();
			foreach (ReviewFlowTemplateDto t in _templates)
			{
				ListViewItem item = new ListViewItem(t.Name ?? "");
				item.SubItems.Add(t.TotalLevel + " 级");
				item.SubItems.Add(t.Enabled == 1 ? "启用" : "停用");
				item.SubItems.Add(string.IsNullOrWhiteSpace(t.UpdatedAt) ? (t.CreatedAt ?? "—") : t.UpdatedAt);
				item.Tag = t;
				if (t.Enabled != 1)
				{
					item.ForeColor = Color.Gray;
				}
				_list.Items.Add(item);
			}
		}
		finally
		{
			_list.EndUpdate();
		}
		UpdateSelectionButtons();
	}

	private ReviewFlowTemplateDto SelectedTemplate
	{
		get { return _list.SelectedItems.Count > 0 ? _list.SelectedItems[0].Tag as ReviewFlowTemplateDto : null; }
	}

	private void UpdateSelectionButtons()
	{
		bool has = SelectedTemplate != null;
		_btnEdit.Enabled = has;
		_btnDelete.Enabled = has;
		_btnToggle.Enabled = has;
	}

	private async Task NewTemplateAsync()
	{
		ReviewFlowTemplateDto saved = await frmReviewFlowTemplateEdit.ShowEditAsync(this, null);
		if (saved != null)
		{
			await LoadTemplatesAsync();
		}
	}

	private async void EditSelected()
	{
		ReviewFlowTemplateDto template = SelectedTemplate;
		if (template == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要编辑的模板");
			return;
		}
		ReviewFlowTemplateDto saved = await frmReviewFlowTemplateEdit.ShowEditAsync(this, template);
		if (saved != null)
		{
			await LoadTemplatesAsync();
		}
	}

	private async Task DeleteSelectedAsync()
	{
		ReviewFlowTemplateDto template = SelectedTemplate;
		if (template == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要删除的模板");
			return;
		}
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question,
			"确定删除模板\"" + template.Name + "\"吗？\n已按该模板生成的审核单不受影响。",
			MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		try
		{
			await Auditai.Util.WebApiClient.DeleteReviewFlowTemplate(template.Id);
			await LoadTemplatesAsync();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "删除失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "删除失败：" + ex.Message);
		}
	}

	private async Task ToggleSelectedAsync()
	{
		ReviewFlowTemplateDto template = SelectedTemplate;
		if (template == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要启用/停用的模板");
			return;
		}
		int target = template.Enabled == 1 ? 0 : 1;
		try
		{
			// 轻量启停：仅翻转 Enabled 不做节点校验（指定审批人离职时仍可停用坏模板）
			await Auditai.Util.WebApiClient.SetReviewFlowTemplateEnabled(template.Id, target);
			await LoadTemplatesAsync();
		}
		catch (HttpRequestException ex)
		{
			await LoadTemplatesAsync(); // 恢复列表真实状态
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "操作失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
		catch (Exception ex)
		{
			await LoadTemplatesAsync();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "操作失败：" + ex.Message);
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

	/// <summary>统一设计语言：主按钮品牌蓝底白字（新建），其余白底灰字浅灰边框；关闭主次与新建一致</summary>
	private void RefreshButtonStyles()
	{
		_btnNew.FlatStyle = FlatStyle.Flat;
		_btnNew.FlatAppearance.BorderSize = 0;
		_btnNew.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		_btnNew.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		_btnNew.ForeColor = Color.White;
		_btnNew.BackColor = AuditTheme.Brand;
		_btnNew.UseVisualStyleBackColor = false;
		ApplyRoundedButton(_btnNew, 8);
		foreach (C1Button btn in new[] { _btnEdit, _btnDelete, _btnToggle, _btnRefresh, _btnClose })
		{
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 1;
			btn.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
			btn.FlatAppearance.MouseOverBackColor = AuditTheme.SurfaceSubtle;
			btn.ForeColor = Color.FromArgb(30, 41, 59);
			btn.BackColor = Color.White;
			btn.UseVisualStyleBackColor = false;
			ApplyRoundedButton(btn, 8);
		}
	}
}
