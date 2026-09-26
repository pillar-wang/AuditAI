using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.Util;

namespace Auditai.UI.LedgerView;

/// <summary>
/// 风险检查方案云端共享库对话框：浏览团队库/系统库方案、预览检查项、导入到当前账套、上传本地方案、删除云端方案。
/// 导入只把方案映射成未落库的本地方案返回给调用方（SchemeToImport），落库与重名冲突由调用方处理。
/// </summary>
public class frmRiskCheckSchemeLibrary : Form
{
	// 统一设计语言常量：按钮 110×36、间距 12、按钮区距右边缘 50px
	private const int BtnWidth = 110;

	private const int BtnHeight = 36;

	private const int BtnGap = 12;

	private const int RightMargin = 50;

	private const int FormWidth = 900;

	private const int FormHeight = 600;

	private const int HeaderHeight = 44;

	private const int ToolbarHeight = 44;

	private const int BtnAreaHeight = 56;

	private readonly string _ledgerName;

	private readonly IList<RiskCheckScheme> _localSchemes;

	private C1FlexGridEx _grdSchemes;

	private C1FlexGridEx _grdPreview;

	private ComboBox _cboLocalScheme;

	private RadioButton _rbTeam;

	private RadioButton _rbSystem;

	private Button _btnImport;

	private Button _btnUpload;

	private Button _btnDelete;

	private Button _btnRefresh;

	private Button _btnClose;

	private Label _lblStatus;

	private Label _lblPreviewTitle;

	private List<RiskCheckSchemeSummaryDto> _schemes = new List<RiskCheckSchemeSummaryDto>();

	private RiskCheckSchemeDetailDto _currentDetail;

	private RiskCheckScheme _currentLocalScheme;

	private bool _busy;

	private bool _filling;

	private bool _localOnly;

	/// <summary>用户点"导入到当前账套"后的本地方案（未落库，Id 未分配）；取消则为 null</summary>
	public RiskCheckScheme SchemeToImport { get; private set; }

	/// <summary>会话内是否发生过上传/删除</summary>
	public bool CloudChanged { get; private set; }

	public frmRiskCheckSchemeLibrary(string ledgerDbPath, IList<RiskCheckScheme> localSchemes)
	{
		_ledgerName = string.IsNullOrWhiteSpace(ledgerDbPath) ? "" : Path.GetFileNameWithoutExtension(ledgerDbPath);
		_localSchemes = localSchemes ?? new List<RiskCheckScheme>();
		InitializeComponent();
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
	}

	private void InitializeComponent()
	{
		Text = "风险检查方案云端共享库";
		ClientSize = new Size(FormWidth, FormHeight);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		Font = new Font("微软雅黑", 9f);
		BackColor = Color.White;

		// ---- 顶部标题栏（浅蓝背景） ----
		Panel pnlHeader = new Panel
		{
			Dock = DockStyle.Top,
			Height = HeaderHeight,
			BackColor = Color.FromArgb(239, 246, 255)
		};
		Label lblTitle = new Label
		{
			Text = "风险检查方案云端共享库",
			Location = new Point(20, 0),
			Size = new Size(FormWidth - 40, HeaderHeight),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 10.5f, FontStyle.Bold),
			ForeColor = Color.FromArgb(29, 78, 216)
		};
		pnlHeader.Controls.Add(lblTitle);

		// ---- 上传区（本地方案下拉 + 上传目标） ----
		Panel pnlToolbar = new Panel
		{
			Dock = DockStyle.Top,
			Height = ToolbarHeight,
			BackColor = Color.White
		};
		Label lblLocal = new Label
		{
			Text = "本地方案：",
			Location = new Point(20, 15),
			AutoSize = true
		};
		_cboLocalScheme = new ComboBox
		{
			Location = new Point(100, 12),
			Size = new Size(220, 23),
			DropDownStyle = ComboBoxStyle.DropDownList,
			DisplayMember = "Name"
		};
		foreach (RiskCheckScheme scheme in _localSchemes)
		{
			if (scheme != null)
			{
				_cboLocalScheme.Items.Add(scheme);
			}
		}
		if (_cboLocalScheme.Items.Count > 0)
		{
			_cboLocalScheme.SelectedIndex = 0;
		}
		Label lblTarget = new Label
		{
			Text = "上传目标：",
			Location = new Point(336, 15),
			AutoSize = true
		};
		_rbTeam = new RadioButton
		{
			Text = "上传到团队库",
			Location = new Point(410, 13),
			AutoSize = true,
			Checked = true
		};
		_rbSystem = new RadioButton
		{
			Text = "上传为系统方案",
			Location = new Point(540, 13),
			AutoSize = true,
			Enabled = RiskCheckCloudHelper.CanManageSystemLibrary
		};
		pnlToolbar.Controls.Add(lblLocal);
		pnlToolbar.Controls.Add(_cboLocalScheme);
		pnlToolbar.Controls.Add(lblTarget);
		pnlToolbar.Controls.Add(_rbTeam);
		pnlToolbar.Controls.Add(_rbSystem);

		// ---- 列表区 + 预览区 ----
		Panel pnlBody = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Color.White,
			Padding = new Padding(20, 8, 20, 8)
		};
		Panel pnlList = new Panel { Dock = DockStyle.Top, Height = 236 };
		Label lblListTitle = new Label
		{
			Text = "云端方案（团队库在前，系统库方案只读）",
			Dock = DockStyle.Top,
			Height = 24,
			TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = Color.FromArgb(71, 85, 105)
		};
		_grdSchemes = CreateGrid();
		AddColumn(_grdSchemes, "范围", "scope", 70, TextAlignEnum.CenterCenter, typeof(string));
		AddColumn(_grdSchemes, "名称", "name", 200, TextAlignEnum.LeftCenter, typeof(string));
		AddColumn(_grdSchemes, "检查项数", "count", 80, TextAlignEnum.CenterCenter, typeof(string));
		AddColumn(_grdSchemes, "发布者", "owner", 110, TextAlignEnum.LeftCenter, typeof(string));
		AddColumn(_grdSchemes, "更新时间", "updated", 150, TextAlignEnum.LeftCenter, typeof(string));
		AddColumn(_grdSchemes, "备注", "note", 180, TextAlignEnum.LeftCenter, typeof(string));
		BuildHeaderRow(_grdSchemes);
		_grdSchemes.AfterRowColChange += delegate
		{
			if (!_filling)
			{
				PreviewSelectedAsync();
			}
		};
		pnlList.Controls.Add(_grdSchemes);
		pnlList.Controls.Add(lblListTitle);

		Panel pnlPreview = new Panel { Dock = DockStyle.Fill };
		_lblPreviewTitle = new Label
		{
			Text = "检查项预览",
			Dock = DockStyle.Top,
			Height = 24,
			TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = Color.FromArgb(71, 85, 105)
		};
		_grdPreview = CreateGrid();
		AddColumn(_grdPreview, "类型", "type", 80, TextAlignEnum.CenterCenter, typeof(string));
		AddColumn(_grdPreview, "说明", "note", 240, TextAlignEnum.LeftCenter, typeof(string));
		AddColumn(_grdPreview, "摘要", "digest", 300, TextAlignEnum.LeftCenter, typeof(string));
		BuildHeaderRow(_grdPreview);
		pnlPreview.Controls.Add(_grdPreview);
		pnlPreview.Controls.Add(_lblPreviewTitle);

		pnlBody.Controls.Add(pnlPreview);
		pnlBody.Controls.Add(pnlList);

		// ---- 底部按钮区 ----
		Panel pnlButtons = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = BtnAreaHeight,
			BackColor = Color.FromArgb(250, 251, 252)
		};
		_lblStatus = new Label
		{
			Text = "",
			Location = new Point(20, 20),
			AutoSize = true,
			ForeColor = Color.FromArgb(100, 116, 139)
		};
		_btnClose = MakeButton("关 闭", FormWidth - RightMargin - BtnWidth, DialogResult.Cancel);
		_btnRefresh = MakeButton("刷 新", FormWidth - RightMargin - (BtnWidth + BtnGap) - BtnWidth);
		_btnDelete = MakeButton("删 除", FormWidth - RightMargin - (BtnWidth + BtnGap) * 2 - BtnWidth);
		_btnUpload = MakeButton("上传本地方案", FormWidth - RightMargin - (BtnWidth + BtnGap) * 3 - BtnWidth);
		_btnImport = MakeButton("导入到当前账套", FormWidth - RightMargin - (BtnWidth + BtnGap) * 4 - BtnWidth);
		_btnImport.Click += delegate
		{
			ImportSelected();
		};
		_btnUpload.Click += delegate
		{
			UploadLocalAsync();
		};
		_btnDelete.Click += delegate
		{
			DeleteSelectedAsync();
		};
		_btnRefresh.Click += delegate
		{
			RefreshAsync();
		};
		pnlButtons.Controls.Add(_lblStatus);
		pnlButtons.Controls.Add(_btnClose);
		pnlButtons.Controls.Add(_btnRefresh);
		pnlButtons.Controls.Add(_btnDelete);
		pnlButtons.Controls.Add(_btnUpload);
		pnlButtons.Controls.Add(_btnImport);

		// 停靠顺序：后添加的先停靠 —— Header 贴顶、上传区其次、列表/预览填充剩余、按钮贴底
		Controls.Add(pnlButtons);
		Controls.Add(pnlBody);
		Controls.Add(pnlToolbar);
		Controls.Add(pnlHeader);

		CancelButton = _btnClose;
		Load += OnFormLoad;
	}

	private static C1FlexGridEx CreateGrid()
	{
		C1FlexGridEx grid = new C1FlexGridEx
		{
			AllowEditing = false,
			Dock = DockStyle.Fill,
			Font = new Font("微软雅黑", 9f),
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle,
			VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom,
			ExtendLastCol = true,
			SelectionMode = SelectionModeEnum.Row,
			FocusRect = FocusRectEnum.None
		};
		grid.Rows.DefaultSize = 24;
		return grid;
	}

	private static void AddColumn(C1FlexGridEx grid, string caption, string name, int width, TextAlignEnum align, Type dataType)
	{
		C1.Win.C1FlexGrid.Column column = grid.Cols.Add();
		column.Caption = caption;
		column.Name = name;
		column.DataType = dataType;
		column.TextAlign = align;
		column.Width = width;
	}

	private static void BuildHeaderRow(C1FlexGridEx grid)
	{
		C1.Win.C1FlexGrid.Row header = grid.Rows.Add();
		C1.Win.C1FlexGrid.CellStyle headerStyle = grid.Styles.Add("cloudHeader");
		headerStyle.TextAlign = TextAlignEnum.CenterCenter;
		for (int i = 0; i < grid.Cols.Count; i++)
		{
			grid.SetCellStyle(0, i, headerStyle);
			header[grid.Cols[i].Name] = grid.Cols[i].Caption;
		}
		grid.Rows.Fixed = 1;
		grid.Cols.Fixed = 0;
	}

	private Button MakeButton(string text, int x, DialogResult result = DialogResult.None)
	{
		Button btn = new Button
		{
			Text = text,
			Location = new Point(x, 10),
			Size = new Size(BtnWidth, BtnHeight),
			Font = new Font("微软雅黑", 9f),
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.White,
			ForeColor = Color.FromArgb(30, 41, 59),
			UseVisualStyleBackColor = false,
			Cursor = Cursors.Hand,
			DialogResult = result
		};
		btn.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
		btn.FlatAppearance.BorderSize = 1;
		btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 247, 252);
		return btn;
	}

	private async void OnFormLoad(object sender, EventArgs e)
	{
		try
		{
			if (!RiskCheckCloudHelper.IsCloudAvailable)
			{
				_localOnly = true;
				UpdateButtons();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持云端方案库", MessageBoxButtons.OK, Text);
				return;
			}
			await LoadSchemesAsync();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "打开云端方案库失败：" + ErrorText(ex), scroll: true);
		}
	}

	private RiskCheckSchemeSummaryDto SelectedSummary
	{
		get
		{
			int row = _grdSchemes.Selection.TopRow;
			if (row < _grdSchemes.Rows.Fixed || row >= _grdSchemes.Rows.Count)
			{
				return null;
			}
			return _grdSchemes.Rows[row].UserData as RiskCheckSchemeSummaryDto;
		}
	}

	private async Task LoadSchemesAsync()
	{
		SetBusy(true, "加载中…");
		try
		{
			_schemes = await WebApiClient.GetRiskCheckSchemes() ?? new List<RiskCheckSchemeSummaryDto>();
		}
		catch (Exception ex)
		{
			_schemes = new List<RiskCheckSchemeSummaryDto>();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "加载云端方案列表失败：" + ErrorText(ex), scroll: true);
		}
		finally
		{
			SetBusy(false, "");
		}
		_currentDetail = null;
		_currentLocalScheme = null;
		ClearPreview();
		RefreshSchemeGrid();
	}

	private void RefreshSchemeGrid()
	{
		_filling = true;
		_grdSchemes.BeginUpdate();
		try
		{
			_grdSchemes.Rows.Count = _grdSchemes.Rows.Fixed;
			foreach (RiskCheckSchemeSummaryDto dto in _schemes)
			{
				C1.Win.C1FlexGrid.Row row = _grdSchemes.Rows.Add();
				row.UserData = dto;
				row["scope"] = RiskCheckCloudHelper.ScopeLabel(dto.Scope);
				row["name"] = dto.Name ?? "";
				row["count"] = dto.RuleCount.ToString();
				row["owner"] = dto.OwnerName ?? "";
				row["updated"] = dto.UpdatedAt ?? "";
				row["note"] = dto.Note ?? "";
			}
		}
		finally
		{
			_grdSchemes.EndUpdate();
			_filling = false;
		}
		if (_grdSchemes.Rows.Count > _grdSchemes.Rows.Fixed)
		{
			_grdSchemes.Select(_grdSchemes.Rows.Fixed, 0);
		}
		UpdateButtons();
	}

	private void ClearPreview()
	{
		_lblPreviewTitle.Text = "检查项预览";
		_grdPreview.BeginUpdate();
		try
		{
			_grdPreview.Rows.Count = _grdPreview.Rows.Fixed;
		}
		finally
		{
			_grdPreview.EndUpdate();
		}
	}

	private void FillPreview(RiskCheckScheme scheme)
	{
		_grdPreview.BeginUpdate();
		try
		{
			_grdPreview.Rows.Count = _grdPreview.Rows.Fixed;
			if (scheme == null)
			{
				return;
			}
			foreach (RiskCheckRule rule in scheme.Rules)
			{
				C1.Win.C1FlexGrid.Row row = _grdPreview.Rows.Add();
				row["type"] = (rule.RuleType == RiskCheckRule.RULE_TYPE_FORMULA) ? "公式检查" : "条件检查";
				row["note"] = rule.Note ?? "";
				row["digest"] = RiskCheckRuleText.BuildDigest(rule);
			}
		}
		finally
		{
			_grdPreview.EndUpdate();
		}
	}

	private async void PreviewSelectedAsync()
	{
		try
		{
			RiskCheckSchemeSummaryDto summary = SelectedSummary;
			_currentDetail = null;
			_currentLocalScheme = null;
			ClearPreview();
			UpdateButtons();
			if (summary == null)
			{
				return;
			}
			_lblPreviewTitle.Text = "检查项预览：" + (summary.Name ?? "");
			SetStatus("加载详情…");
			RiskCheckSchemeDetailDto detail;
			try
			{
				detail = await WebApiClient.GetRiskCheckSchemeDetail(summary.Id);
			}
			catch (Exception ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "加载方案详情失败：" + ErrorText(ex), scroll: true);
				return;
			}
			finally
			{
				SetStatus("");
			}
			// 加载期间用户可能已切换选择，过期结果丢弃
			if (SelectedSummary == null || SelectedSummary.Id != summary.Id)
			{
				return;
			}
			if (detail == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该方案详情为空，可能已被删除");
				return;
			}
			_currentDetail = detail;
			_currentLocalScheme = RiskCheckCloudHelper.MapToLocal(detail);
			FillPreview(_currentLocalScheme);
			UpdateButtons();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "加载方案详情失败：" + ErrorText(ex), scroll: true);
		}
	}

	private void ImportSelected()
	{
		RiskCheckSchemeSummaryDto summary = SelectedSummary;
		if (summary == null || _currentDetail == null || _currentLocalScheme == null || _currentDetail.Id != summary.Id)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一个云端方案并等待详情加载完成");
			return;
		}
		SchemeToImport = _currentLocalScheme;
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private async void UploadLocalAsync()
	{
		try
		{
			if (!RiskCheckCloudHelper.CanManageTeamLibrary)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "仅团队管理员可上传方案");
				return;
			}
			RiskCheckScheme scheme = _cboLocalScheme.SelectedItem as RiskCheckScheme;
			if (scheme == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要上传的本地方案");
				return;
			}
			if (scheme.Rules.Count == 0)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该本地方案没有检查项，无法上传");
				return;
			}
			bool systemLibrary = _rbSystem.Checked;
			if (systemLibrary && !RiskCheckCloudHelper.CanManageSystemLibrary)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "仅系统管理员可上传为系统方案");
				return;
			}
			using frmRiskCheckSchemePublish dialog = new frmRiskCheckSchemePublish(scheme.Name, scheme.Note, _ledgerName, systemLibrary);
			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}
			RiskCheckSchemeSaveRequestDto request = RiskCheckCloudHelper.BuildSaveRequest(scheme, dialog.SchemeName, dialog.SchemeNote, _ledgerName, dialog.Overwrite);
			SetBusy(true, "上传中…");
			try
			{
				if (systemLibrary)
				{
					await WebApiClient.PublishRiskCheckSchemeAsSystem(request);
				}
				else
				{
					await WebApiClient.SaveRiskCheckScheme(request);
				}
				CloudChanged = true;
			}
			catch (Exception ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "上传失败：" + ErrorText(ex), scroll: true);
				return;
			}
			finally
			{
				SetBusy(false, "");
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "上传成功");
			await LoadSchemesAsync();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "上传失败：" + ErrorText(ex), scroll: true);
		}
	}

	private async void DeleteSelectedAsync()
	{
		try
		{
			RiskCheckSchemeSummaryDto summary = SelectedSummary;
			if (summary == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要删除的云端方案");
				return;
			}
			if (summary.Scope == 0)
			{
				if (!RiskCheckCloudHelper.CanManageTeamLibrary)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "仅团队管理员可删除团队库方案");
					return;
				}
			}
			else if (!RiskCheckCloudHelper.CanManageSystemLibrary)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "仅系统管理员可删除系统方案");
				return;
			}
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question,
				"确定删除方案\"" + (summary.Name ?? "") + "\"吗？",
				MessageBoxButtons.YesNo, "删除方案") != DialogResult.Yes)
			{
				return;
			}
			SetBusy(true, "删除中…");
			try
			{
				await WebApiClient.DeleteRiskCheckScheme(summary.Id);
				CloudChanged = true;
			}
			catch (Exception ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "删除失败：" + ErrorText(ex), scroll: true);
				return;
			}
			finally
			{
				SetBusy(false, "");
			}
			await LoadSchemesAsync();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "删除失败：" + ErrorText(ex), scroll: true);
		}
	}

	private async void RefreshAsync()
	{
		try
		{
			if (_localOnly)
			{
				return;
			}
			await LoadSchemesAsync();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "加载云端方案列表失败：" + ErrorText(ex), scroll: true);
		}
	}

	private void SetBusy(bool busy, string status)
	{
		_busy = busy;
		SetStatus(status);
		UpdateButtons();
	}

	private void SetStatus(string status)
	{
		_lblStatus.Text = status ?? "";
	}

	private void UpdateButtons()
	{
		if (_localOnly)
		{
			_btnImport.Enabled = false;
			_btnUpload.Enabled = false;
			_btnDelete.Enabled = false;
			_btnRefresh.Enabled = false;
			_btnClose.Enabled = true;
			return;
		}
		RiskCheckSchemeSummaryDto summary = SelectedSummary;
		bool detailReady = summary != null && _currentDetail != null && _currentLocalScheme != null && _currentDetail.Id == summary.Id;
		_btnImport.Enabled = !_busy && detailReady;
		_btnUpload.Enabled = !_busy && RiskCheckCloudHelper.CanManageTeamLibrary;
		bool canDelete = summary != null && (summary.Scope == 0
			? RiskCheckCloudHelper.CanManageTeamLibrary
			: RiskCheckCloudHelper.CanManageSystemLibrary);
		_btnDelete.Enabled = !_busy && canDelete;
		_btnRefresh.Enabled = !_busy;
		_btnClose.Enabled = true;
	}

	private static string ErrorText(Exception ex)
	{
		if (ex is HttpRequestException && ex.InnerException != null)
		{
			return ex.InnerException.Message;
		}
		return ex.Message;
	}
}
