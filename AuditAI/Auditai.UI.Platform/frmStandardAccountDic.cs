using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using Auditai.LocalDataStore;
using Auditai.UI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Platform;

/// <summary>
/// 标准科目字典配置窗口：树形展示标准科目，支持添加/修改/删除、导入导出 JSON、保存到本地 config\StandardAccountDic.json
/// 数据源：窗口加载时优先读本地 config 文件（同步 IO 直接读）；本地不存在时用 StorageRouter.GetStandardAccountDic() 拉取结果展示
/// </summary>
public class frmStandardAccountDic : Form
{
	// 统一设计语言常量：按钮 110×40、间距 12、按钮区距右边缘约 50px
	private const int BtnWidth = 110;
	private const int BtnHeight = 40;
	private const int BtnGap = 12;
	private const int RightMargin = 50;

	private static readonly Font _fontNormal = new Font("微软雅黑", 9f);

	private StandardAccountDictionary _dict = new StandardAccountDictionary();

	private Panel _pnlHeader;
	private Label _lblTitle;
	private Label _lblSummary;
	private Panel _pnlGrid;
	private C1FlexGridEx _grid;
	private Panel _pnlRowTools;

	private C1Button _btnAddRoot;
	private C1Button _btnAddChild;
	private C1Button _btnModify;
	private C1Button _btnDelete;
	private C1Button _btnImport;
	private C1Button _btnExport;
	private C1Button _btnSave;
	private C1Button _btnClose;

	public frmStandardAccountDic()
	{
		InitializeComponent();
		// 先应用主题（浅蓝小清新，作用于 C1 网格等），再重设主/次按钮配色（C1 皮肤会覆盖自定义配色）
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		RefreshButtonStyles();
		Load += FrmStandardAccountDic_Load;
	}

	/// <summary>本地字典文件路径（保存目标）</summary>
	private static string GetLocalConfigPath()
	{
		return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "StandardAccountDic.json");
	}

	private void InitializeComponent()
	{
		Text = "标准科目字典";
		ClientSize = new Size(960, 700);
		MinimumSize = new Size(820, 620);
		StartPosition = FormStartPosition.CenterScreen;
		MaximizeBox = true;
		MinimizeBox = false;
		ShowInTaskbar = false;
		Font = new Font("微软雅黑", 9.5f);
		BackColor = Color.White;

		// ---- 顶部标题栏（浅蓝背景，标题有内边距不紧贴边缘） ----
		_pnlHeader = new Panel
		{
			Dock = DockStyle.Top,
			Height = 64,
			BackColor = AuditTheme.BrandSubtle
		};
		_lblTitle = new Label
		{
			Text = "标准科目字典",
			Location = new Point(20, 8),
			Size = new Size(400, 28),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 12f, FontStyle.Bold),
			ForeColor = AuditTheme.BrandActive
		};
		_lblSummary = new Label
		{
			Text = "加载中…",
			Location = new Point(22, 38),
			Size = new Size(900, 18),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 9f),
			ForeColor = AuditTheme.TextMuted
		};
		_pnlHeader.Controls.Add(_lblTitle);
		_pnlHeader.Controls.Add(_lblSummary);
		Controls.Add(_pnlHeader);

		// ---- 中央树形列表 ----
		_pnlGrid = new Panel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12, 8, 12, 8),
			BackColor = Color.White
		};
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowEditing = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowResizing = AllowResizingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			ExtendLastCol = true,
			SelectionMode = SelectionModeEnum.Row,
			Font = new Font("微软雅黑", 9.5f),
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None
		};
		_grid.Cols.Count = 0;
		_grid.Cols.Fixed = 0;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Rows.DefaultSize = 32;
		// 树形列放在第 0 列（科目编码），参照 AccountTreeEditor 的树形用法
		_grid.Tree.Column = 0;
		C1.Win.C1FlexGrid.Column colCode = _grid.Cols.Add();
		colCode.Name = "Code";
		colCode.Caption = "科目编码";
		colCode.Width = 200;
		C1.Win.C1FlexGrid.Column colName = _grid.Cols.Add();
		colName.Name = "Name";
		colName.Caption = "科目名称";
		colName.Width = 380;
		C1.Win.C1FlexGrid.Column colDc = _grid.Cols.Add();
		colDc.Name = "Dc";
		colDc.Caption = "借贷方向";
		colDc.Width = 100;
		colDc.TextAlign = TextAlignEnum.CenterCenter;
		_grid.MouseDoubleClick += Grid_MouseDoubleClick;
		_pnlGrid.Controls.Add(_grid);
		Controls.Add(_pnlGrid);

		// ---- 底部操作栏（左提示 + 右对齐两行按钮组，单行 8 按钮共 964px 超窗宽会与提示重叠、被裁切） ----
		_pnlRowTools = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 98,
			BackColor = AuditTheme.SurfaceMuted
		};
		Label lblHint = new Label
		{
			Text = "双击科目可修改；选中科目后可添加下级",
			Location = new Point(15, 6),
			Size = new Size(255, 86),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 9f),
			ForeColor = AuditTheme.TextMuted
		};
		_pnlRowTools.Controls.Add(lblHint);

		const int row1Y = 6;
		const int row2Y = 52;
		// 数据操作行（下）：关闭、保存、导出、导入
		int right = RightMargin;
		_btnClose = MakeButtonRAnchor("关闭", row2Y, right, BtnClose_Click);
		right += BtnWidth + BtnGap;
		_btnSave = MakeButtonRAnchor("保存", row2Y, right, BtnSave_Click);
		right += BtnWidth + BtnGap;
		_btnExport = MakeButtonRAnchor("导出 JSON", row2Y, right, BtnExport_Click);
		right += BtnWidth + BtnGap;
		_btnImport = MakeButtonRAnchor("导入 JSON", row2Y, right, BtnImport_Click);
		// 编辑工具行（上）：删除、修改、添加子科目、添加一级科目
		right = RightMargin;
		_btnDelete = MakeButtonRAnchor("删除", row1Y, right, BtnDelete_Click);
		right += BtnWidth + BtnGap;
		_btnModify = MakeButtonRAnchor("修改", row1Y, right, BtnModify_Click);
		right += BtnWidth + BtnGap;
		_btnAddChild = MakeButtonRAnchor("添加子科目", row1Y, right, BtnAddChild_Click);
		right += BtnWidth + BtnGap;
		_btnAddRoot = MakeButtonRAnchor("添加一级科目", row1Y, right, BtnAddRoot_Click);

		_pnlRowTools.Controls.AddRange(new Control[] { _btnAddRoot, _btnAddChild, _btnModify, _btnDelete, _btnImport, _btnExport, _btnSave, _btnClose });
		// 底部面板首次布局时把按钮校准到正确的距右位置，后续 Anchor Top|Right 自动随缩放保持对齐
		AttachRightAnchorCalibrator(_pnlRowTools);
		Controls.Add(_pnlRowTools);

		CancelButton = _btnClose;
	}

	/// <summary>统一设计语言：创建按钮（110×40，先设 Anchor Top|Right + 初始占位 Location，后续由底部面板的 Layout 事件统一校准右对齐）</summary>
	private C1Button MakeButtonRAnchor(string text, int y, int distanceFromRight, EventHandler onClick)
	{
		C1Button btn = new C1Button
		{
			Text = text,
			Size = new Size(BtnWidth, BtnHeight),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f),
			// 初始占位 X=0 会在 Layout 时被校准
			Location = new Point(0, y)
		};
		// 把 distanceFromRight 暂存到 Tag 里，供面板 Layout 时读取
		btn.Tag = distanceFromRight;
		btn.Click += onClick;
		return btn;
	}

	/// <summary>给底部操作面板注册 Layout 事件，首次布局时把所有带 right-anchor Tag 的按钮校准到正确的距右位置</summary>
	private static void AttachRightAnchorCalibrator(Panel panel)
	{
		bool calibrated = false;
		panel.Layout += delegate
		{
			if (calibrated) return;
			calibrated = true;
			foreach (Control c in panel.Controls)
			{
				if (c.Tag is int dist && dist >= 0)
				{
					c.Location = new Point(panel.ClientSize.Width - dist - c.Width, c.Location.Y);
				}
			}
		};
	}

	/// <summary>统一设计语言：给按钮应用 8px 圆角区域（与 frmFindPwd 保持一致）</summary>
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

	/// <summary>统一设计语言：重新应用主/次按钮配色（主按钮品牌蓝底白字，次按钮白底灰字浅灰边框）</summary>
	private void RefreshButtonStyles()
	{
		foreach (C1Button btn in new[] { _btnAddRoot, _btnAddChild, _btnModify, _btnDelete, _btnImport, _btnExport, _btnClose })
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
		// 主按钮：品牌蓝底白字
		_btnSave.FlatStyle = FlatStyle.Flat;
		_btnSave.FlatAppearance.BorderSize = 0;
		_btnSave.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		_btnSave.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		_btnSave.ForeColor = Color.White;
		_btnSave.BackColor = AuditTheme.Brand;
		_btnSave.UseVisualStyleBackColor = false;
		_btnSave.Font = new Font("微软雅黑", 9.5f, FontStyle.Bold);
		ApplyRoundedButton(_btnSave, 8);
	}

	// ==================== 数据加载 ====================

	private void FrmStandardAccountDic_Load(object sender, EventArgs e)
	{
		try
		{
			string path = GetLocalConfigPath();
			if (File.Exists(path))
			{
				// 本地 config 文件读取是同步 IO，直接同步读
				_dict = StandardAccountDictionary.LoadFromFile(path) ?? new StandardAccountDictionary();
				PopulateTree();
			}
			else
			{
				// 本地不存在则从 StorageRouter 拉取结果展示（fire-and-forget，方法内部已 try-catch）
				_ = LoadFromRouterAsync();
			}
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "加载标准科目字典失败：" + ex.Message);
		}
	}

	/// <summary>从 StorageRouter 拉取标准科目字典（异步，结束后刷新树）</summary>
	private async System.Threading.Tasks.Task LoadFromRouterAsync()
	{
		try
		{
			JObject obj = await Auditai.LocalDataStore.StorageRouter.GetStandardAccountDic();
			if (IsDisposed)
			{
				return;
			}
			_dict = (obj != null) ? obj.ToObject<StandardAccountDictionary>() : null;
			if (_dict == null)
			{
				_dict = new StandardAccountDictionary();
			}
			PopulateTree();
		}
		catch (Exception ex)
		{
			if (!IsDisposed)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "从云端加载标准科目字典失败：" + ex.Message);
			}
		}
	}

	// ==================== 树形展示 ====================

	/// <summary>根据 _dict 重建树（每次数据变更后整树刷新，_dict.Accounts 为唯一数据源）</summary>
	private void PopulateTree()
	{
		_grid.BeginUpdate();
		try
		{
			_grid.Rows.Count = 1; // 仅保留表头
			foreach (StandardAccountNode root in _dict.GetTree())
			{
				AppendNode(root, null);
			}
		}
		finally
		{
			_grid.EndUpdate();
		}
		_grid.ExpandAll();
		UpdateSummary();
	}

	private void AppendNode(StandardAccountNode node, Node parentNode)
	{
		C1.Win.C1FlexGrid.Row row;
		if (parentNode != null)
		{
			row = parentNode.AddNode(NodeTypeEnum.LastChild, node.Code, node.Code, null).Row;
		}
		else
		{
			row = _grid.Rows.Add();
			row.IsNode = true;
		}
		row["Code"] = node.Code;
		row["Name"] = node.Name;
		row["Dc"] = DcToText(node.Dc);
		row.UserData = node;
		foreach (StandardAccountNode child in node.Children)
		{
			AppendNode(child, row.Node);
		}
	}

	private StandardAccountNode GetSelectedNode()
	{
		int row = _grid.Row;
		if (row < _grid.Rows.Fixed || row >= _grid.Rows.Count)
		{
			return null;
		}
		return _grid.Rows[row].UserData as StandardAccountNode;
	}

	private void UpdateSummary()
	{
		int count = (_dict.Accounts != null) ? _dict.Accounts.Count : 0;
		_lblSummary.Text = $"共 {count} 个科目 · 版本 v{_dict.Version} · 保存到本地 config\\StandardAccountDic.json";
	}

	/// <summary>借贷方向文本：1=借 -1=贷 null=空</summary>
	private static string DcToText(int? dc)
	{
		if (dc == 1) return "借";
		if (dc == -1) return "贷";
		return string.Empty;
	}

	private StandardAccount FindAccount(string code)
	{
		return (_dict.Accounts != null) ? _dict.Accounts.FirstOrDefault(a => a != null && a.Code == code) : null;
	}

	/// <summary>父级显示文本（编辑对话框用）</summary>
	private string GetParentDisplay(string parentCode)
	{
		if (string.IsNullOrEmpty(parentCode))
		{
			return "（一级科目）";
		}
		StandardAccount acc = FindAccount(parentCode);
		return (acc != null) ? $"{acc.Code} {acc.Name}" : parentCode;
	}

	// ==================== 添加/修改/删除 ====================

	private void BtnAddRoot_Click(object sender, EventArgs e)
	{
		ShowEditDialog(null, null);
	}

	private void BtnAddChild_Click(object sender, EventArgs e)
	{
		StandardAccountNode node = GetSelectedNode();
		if (node == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在列表中选中作为父级的科目");
			return;
		}
		ShowEditDialog(null, node.Code);
	}

	private void BtnModify_Click(object sender, EventArgs e)
	{
		StandardAccountNode node = GetSelectedNode();
		if (node == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在列表中选中要修改的科目");
			return;
		}
		ShowEditDialog(node.Code, null);
	}

	private void Grid_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (GetSelectedNode() != null)
		{
			BtnModify_Click(sender, e);
		}
	}

	/// <summary>
	/// 弹出科目编辑对话框。editCode 非空表示修改；否则为添加，parentCode 非空表示添加子科目
	/// </summary>
	private void ShowEditDialog(string editCode, string parentCode)
	{
		StandardAccount editing = (editCode != null) ? FindAccount(editCode) : null;
		using (frmStandardAccountEdit dlg = new frmStandardAccountEdit())
		{
			if (editing != null)
			{
				dlg.DialogTitle = "修改科目";
				dlg.InitValues(editing.Code, editing.Name, editing.Dc, GetParentDisplay(editing.ParentCode));
			}
			else
			{
				dlg.DialogTitle = (parentCode != null) ? "添加子科目" : "添加一级科目";
				dlg.InitValues(string.Empty, string.Empty, null, GetParentDisplay(parentCode));
			}
			// 编码唯一性、不能与父级相同（由本窗口校验，对话框内提示）
			dlg.CodeValidator = delegate(string code)
			{
				if (editing != null && code == editing.Code)
				{
					return null; // 修改时编码未变化视为合法
				}
				if (_dict.Accounts != null && _dict.Accounts.Any(a => a != null && a.Code == code))
				{
					return $"科目编码 {code} 已存在";
				}
				if (parentCode != null && code == parentCode)
				{
					return "科目编码不能与父级编码相同";
				}
				return null;
			};
			if (dlg.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}
			if (editing != null)
			{
				string oldCode = editing.Code;
				editing.Code = dlg.AccountCode;
				editing.Name = dlg.AccountName;
				editing.Dc = dlg.AccountDc;
				if (oldCode != editing.Code)
				{
					// 编码变化时同步下级科目的 parentCode
					foreach (StandardAccount child in _dict.Accounts.Where(a => a != null && a.ParentCode == oldCode))
					{
						child.ParentCode = editing.Code;
					}
				}
			}
			else
			{
				_dict.Accounts.Add(new StandardAccount
				{
					Code = dlg.AccountCode,
					Name = dlg.AccountName,
					Dc = dlg.AccountDc,
					ParentCode = parentCode
				});
			}
			PopulateTree();
		}
	}

	private void BtnDelete_Click(object sender, EventArgs e)
	{
		StandardAccountNode node = GetSelectedNode();
		if (node == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在列表中选中要删除的科目");
			return;
		}
		// 存在下级科目时阻止删除
		if (_dict.Accounts != null && _dict.Accounts.Any(a => a != null && a.ParentCode == node.Code))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先删除下级科目");
			return;
		}
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"确定要删除科目 {node.Code} {node.Name} 吗？", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		StandardAccount acc = FindAccount(node.Code);
		if (acc != null)
		{
			_dict.Accounts.Remove(acc);
		}
		PopulateTree();
	}

	// ==================== 导入/导出/保存 ====================

	private void BtnImport_Click(object sender, EventArgs e)
	{
		try
		{
			using (OpenFileDialog dlg = new OpenFileDialog())
			{
				dlg.Title = "导入标准科目字典";
				dlg.Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*";
				if (dlg.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				StandardAccountDictionary imported = StandardAccountDictionary.LoadFromFile(dlg.FileName);
				if (imported == null)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "无法读取所选文件");
					return;
				}
				string error;
				if (!imported.Validate(out error))
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导入的字典数据校验未通过：\n" + error);
					return;
				}
				_dict = imported; // 整库替换
				PopulateTree();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"导入成功，共 {_dict.Accounts.Count} 个科目。修改尚未保存到本地配置。");
			}
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导入失败：" + ex.Message);
		}
	}

	private void BtnExport_Click(object sender, EventArgs e)
	{
		try
		{
			string error;
			if (!_dict.Validate(out error))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前数据校验未通过，请先修正：\n" + error);
				return;
			}
			using (SaveFileDialog dlg = new SaveFileDialog())
			{
				dlg.Title = "导出标准科目字典";
				dlg.Filter = "JSON 文件 (*.json)|*.json";
				dlg.FileName = "StandardAccountDic.json";
				if (dlg.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				// 序列化为 { "Version": n, "Accounts": [ { code, name, dc, parentCode } ] }
				File.WriteAllText(dlg.FileName, JsonConvert.SerializeObject(_dict, Formatting.Indented));
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出成功");
			}
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出失败：" + ex.Message);
		}
	}

	private void BtnSave_Click(object sender, EventArgs e)
	{
		try
		{
			string error;
			if (!_dict.Validate(out error))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "保存失败，数据校验未通过：\n" + error);
				return;
			}
			_dict.Version++;
			string path = GetLocalConfigPath();
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, JsonConvert.SerializeObject(_dict, Formatting.Indented));
			UpdateSummary();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已保存");
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "保存失败：" + ex.Message);
		}
	}

	private void BtnClose_Click(object sender, EventArgs e)
	{
		Close();
	}
}

/// <summary>
/// 标准科目编辑小对话框：录入/修改科目编码、名称、借贷方向（编码、名称必填）
/// </summary>
public class frmStandardAccountEdit : Form
{
	private Label _lblDlgTitle;
	private Label _lblParentValue;
	private TextBox _txtCode;
	private TextBox _txtName;
	private ComboBox _cmbDc;
	private C1Button _btnOk;
	private C1Button _btnCancel;

	/// <summary>编码校验委托：返回错误信息，null 表示合法（由父窗口提供唯一性等校验）</summary>
	public Func<string, string> CodeValidator { get; set; }

	/// <summary>对话框标题（显示在顶部标题栏）</summary>
	public string DialogTitle
	{
		set { _lblDlgTitle.Text = value; }
	}

	public string AccountCode => _txtCode.Text.Trim();

	public string AccountName => _txtName.Text.Trim();

	/// <summary>借贷方向：1=借方 -1=贷方 null=未指定</summary>
	public int? AccountDc => (_cmbDc.SelectedIndex == 1) ? 1 : ((_cmbDc.SelectedIndex == 2) ? -1 : (int?)null);

	public frmStandardAccountEdit()
	{
		InitializeComponent();
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		RefreshButtonStyles();
	}

	public void InitValues(string code, string name, int? dc, string parentDisplay)
	{
		_txtCode.Text = code;
		_txtName.Text = name;
		_lblParentValue.Text = parentDisplay;
		_cmbDc.SelectedIndex = (dc == 1) ? 1 : ((dc == -1) ? 2 : 0);
	}

	private void InitializeComponent()
	{
		Text = "科目编辑";
		ClientSize = new Size(540, 360);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		Font = new Font("微软雅黑", 9.5f);

		// ---- 顶部标题栏（浅蓝背景，内边距 20 不紧贴边缘） ----
		Panel pnlHeader = new Panel
		{
			Dock = DockStyle.Top,
			Height = 52,
			BackColor = AuditTheme.BrandSubtle
		};
		_lblDlgTitle = new Label
		{
			Text = "添加科目",
			Location = new Point(20, 0),
			Size = new Size(440, 52),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 12f, FontStyle.Bold),
			ForeColor = AuditTheme.BrandActive
		};
		pnlHeader.Controls.Add(_lblDlgTitle);
		Controls.Add(pnlHeader);

		// ---- 表单区 ----
		Panel pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
		Label lblParent = MakeLabel("父级科目", 14);
		_lblParentValue = new Label
		{
			Location = new Point(128, 14),
			Size = new Size(380, 28),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 9.5f),
			ForeColor = AuditTheme.TextSecondary
		};
		Label lblCode = MakeLabel("科目编码 *", 60);
		_txtCode = new TextBox
		{
			Location = new Point(128, 54),
			Size = new Size(380, 30),
			Font = new Font("微软雅黑", 10f)
		};
		Label lblName = MakeLabel("科目名称 *", 108);
		_txtName = new TextBox
		{
			Location = new Point(128, 104),
			Size = new Size(380, 30),
			Font = new Font("微软雅黑", 10f)
		};
		Label lblDc = MakeLabel("借贷方向", 158);
		_cmbDc = new ComboBox
		{
			Location = new Point(128, 154),
			Size = new Size(380, 30),
			DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
			Font = new Font("微软雅黑", 10f)
		};
		_cmbDc.Items.AddRange(new object[] { "未指定", "借方", "贷方" });
		_cmbDc.SelectedIndex = 0;
		Label lblRequired = new Label
		{
			Text = "带 * 为必填项",
			Location = new Point(128, 202),
			Size = new Size(380, 20),
			Font = new Font("微软雅黑", 9f),
			ForeColor = AuditTheme.TextMuted
		};
		pnlBody.Controls.AddRange(new Control[] { lblParent, _lblParentValue, lblCode, _txtCode, lblName, _txtName, lblDc, _cmbDc, lblRequired });
		Controls.Add(pnlBody);

		// ---- 底部按钮区 ----
		Panel pnlButtons = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = 64,
			BackColor = AuditTheme.SurfaceMuted
		};
		_btnOk = new C1Button
		{
			Text = "确定",
			Size = new Size(110, 40),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f, FontStyle.Bold)
		};
		_btnOk.Click += BtnOk_Click;
		_btnCancel = new C1Button
		{
			Text = "取消",
			Size = new Size(110, 40),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Font = new Font("微软雅黑", 9.5f),
			DialogResult = DialogResult.Cancel
		};
		// 右对齐定位：btnCancel 在左、btnOk 在右，间距 12，右边距 20
		// 父容器 Width 在首次 Layout 后才正确，用委托一次性校准
		bool calibrated = false;
		pnlButtons.Layout += delegate
		{
			if (calibrated) return;
			calibrated = true;
			int pw = pnlButtons.ClientSize.Width;
			_btnOk.Location = new Point(pw - 20 - _btnOk.Width, 12);
			_btnCancel.Location = new Point(_btnOk.Location.X - 12 - _btnCancel.Width, 12);
		};
		pnlButtons.Controls.Add(_btnOk);
		pnlButtons.Controls.Add(_btnCancel);
		Controls.Add(pnlButtons);

		AcceptButton = _btnOk;
		CancelButton = _btnCancel;
	}

	private static Label MakeLabel(string text, int y)
	{
		return new Label
		{
			Text = text,
			Location = new Point(24, y),
			Size = new Size(104, 28),
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("微软雅黑", 9.5f),
			ForeColor = AuditTheme.TextSecondary
		};
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

	/// <summary>统一设计语言：重新应用主/次按钮配色</summary>
	private void RefreshButtonStyles()
	{
		// 主按钮：品牌蓝底白字
		_btnOk.FlatStyle = FlatStyle.Flat;
		_btnOk.FlatAppearance.BorderSize = 0;
		_btnOk.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		_btnOk.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		_btnOk.ForeColor = Color.White;
		_btnOk.BackColor = AuditTheme.Brand;
		_btnOk.UseVisualStyleBackColor = false;
		ApplyRoundedButton(_btnOk, 8);
		// 次按钮：白底灰字 + 浅灰边框
		_btnCancel.FlatStyle = FlatStyle.Flat;
		_btnCancel.FlatAppearance.BorderSize = 1;
		_btnCancel.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
		_btnCancel.FlatAppearance.MouseOverBackColor = AuditTheme.SurfaceSubtle;
		_btnCancel.ForeColor = Color.FromArgb(30, 41, 59);
		_btnCancel.BackColor = Color.White;
		_btnCancel.UseVisualStyleBackColor = false;
		ApplyRoundedButton(_btnCancel, 8);
	}

	private void BtnOk_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(_txtCode.Text))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目编码不能为空");
			_txtCode.Focus();
			return;
		}
		if (string.IsNullOrWhiteSpace(_txtName.Text))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目名称不能为空");
			_txtName.Focus();
			return;
		}
		if (CodeValidator != null)
		{
			string error = CodeValidator(_txtCode.Text.Trim());
			if (!string.IsNullOrEmpty(error))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, error);
				_txtCode.Focus();
				return;
			}
		}
		DialogResult = DialogResult.OK;
	}
}
