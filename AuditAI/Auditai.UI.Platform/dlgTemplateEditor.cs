﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1InputPanel;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class dlgTemplateEditor : C1RibbonForm
{
	private enum Mode
	{
		Create,
		Modify,
		Duplicate,
		FromProject
	}

	private Mode _mode;

	private readonly TemplateUsersListSelector projectUsersListSelector;

	private readonly TemplateUsersTileSelector projectUsersTileSelector;

	private static readonly Regex _rxSplit = new Regex("[,，]");

	private TooltipManager tooltipManager = new TooltipManager();

	private IContainer components;

	private C1InputPanel inputPanel;

	private InputLabel lblNumber;

	private InputLabel lblCategory;

	private InputLabel lblName;

	private InputLabel lblNote;

	private InputTextBox txtNumber;

	private InputComboBox cboCategory;

	private InputTextBox txtName;

	private InputTextBox txtNote;

	private C1SplitContainer ctnMain;

	private C1SplitterPanel pnlInfoInput;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancel;

	private C1Button btnOk;

	private InputLabel lblAllMembers;

	private InputCheckBox chkAllMembers;

	private C1SplitterPanel pnlUserHead;

	private C1SplitterPanel pnlUserSelect;

	private InputGroupHeader 基本信息;

	private C1InputPanel c1InputPanel2;

	private InputGroupHeader inputGroupHeader1;

	private C1Button btnSwitchMode;

	private C1SplitterPanel pnlEmpty;

	private C1SplitterPanel c1SplitterPanel1;

	private C1InputPanel c1InputPanel3;

	private C1InputPanel c1InputPanel1;

	private C1CheckBox ckbSearch;

	private C1TextBox txbSearch;

	public ProjectUsersSelectorContext Context { get; } = new ProjectUsersSelectorContext();


	public Auditai.DTO.Project Template { get; set; }

	private ListTileViewMode _ListOrTile
	{
		get
		{
			return (Auditai.UI.Platform.ListTileViewMode)UserSet.Config.ProjectMembersViewMode;
		}
		set
		{
			UserSet.Config.ProjectMembersViewMode = value;
		}
	}

	public dlgTemplateEditor()
	{
		base.ShowInTaskbar = false;
		InitializeComponent();
		// 为搜索框设置独立编辑菜单：避免访问进程内可能已被释放的共享 ContextMenuStrip（点击搜索按钮显示搜索框时曾抛 ObjectDisposedException）
		txbSearch.ContextMenuStrip = BuildSearchBoxContextMenu();
		base.Load += DlgTemplateEditor_Load;
		base.Shown += DlgTemplateEditor_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		ckbSearch.CheckedChanged += CkbSearch_CheckedChanged;
		txbSearch.TextChanged += TxbSearch_TextChanged;
		projectUsersListSelector = new TemplateUsersListSelector();
		pnlUserSelect.Controls.Add(projectUsersListSelector.GetControl());
		projectUsersTileSelector = new TemplateUsersTileSelector();
		pnlUserSelect.Controls.Add(projectUsersTileSelector.GetControl());
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		AttachTooltip();
	}

	private void DlgTemplateEditor_Load(object sender, EventArgs e)
	{
		ToListOrTile(_ListOrTile);
	}

	/// <summary>构建搜索框独立编辑菜单（剪切/复制/粘贴/全选），避免依赖进程内共享 ContextMenuStrip。</summary>
	private static ContextMenuStrip BuildSearchBoxContextMenu()
	{
		ContextMenuStrip menu = new ContextMenuStrip { RenderMode = ToolStripRenderMode.System, ImageScalingSize = new Size(24, 24), AutoSize = true };
		ToolStripMenuItem cut = new ToolStripMenuItem("剪切", null, (s, e) => { if (s is ToolStripMenuItem item && item.Owner is ContextMenuStrip owner && owner.SourceControl is C1.Win.C1Input.C1TextBox box) box.Cut(); });
		ToolStripMenuItem copy = new ToolStripMenuItem("复制", null, (s, e) => { if (s is ToolStripMenuItem item && item.Owner is ContextMenuStrip owner && owner.SourceControl is C1.Win.C1Input.C1TextBox box) box.Copy(); });
		ToolStripMenuItem paste = new ToolStripMenuItem("粘贴", null, (s, e) => { if (s is ToolStripMenuItem item && item.Owner is ContextMenuStrip owner && owner.SourceControl is C1.Win.C1Input.C1TextBox box) box.Paste(); });
		ToolStripMenuItem selectAll = new ToolStripMenuItem("全选", null, (s, e) => { if (s is ToolStripMenuItem item && item.Owner is ContextMenuStrip owner && owner.SourceControl is C1.Win.C1Input.C1TextBox box) box.SelectAll(); });
		menu.Items.Add(cut);
		menu.Items.Add(copy);
		menu.Items.Add(paste);
		menu.Items.Add(new ToolStripSeparator());
		menu.Items.Add(selectAll);
		return menu;
	}

	private void TxbSearch_TextChanged(object sender, EventArgs e)
	{
		string[] array = _rxSplit.Split(txbSearch.Text);
		if (array.Length > 1)
		{
			array = array.Where((string s) => !string.IsNullOrEmpty(s)).ToArray();
		}
		for (int i = 0; i < Context.UserViewStates.Count; i++)
		{
			Tuple<Auditai.DTO.User, bool> uvs = Context.UserViewStates[i];
			int num = array.Sum((string s) => FuzzySearch.Filter(uvs.Item1.Name, s));
			Context.UserViewStates[i] = Tuple.Create(uvs.Item1, num > 0);
		}
		projectUsersListSelector.Search();
		projectUsersTileSelector.Search();
	}

	private void CkbSearch_CheckedChanged(object sender, EventArgs e)
	{
		txbSearch.Visible = ckbSearch.Checked;
	}

	private async void DlgTemplateEditor_Shown(object sender, EventArgs e)
	{
		ctnMain.Enabled = false;
		try
		{
			await PopulateCategoryCombo();
			switch (_mode)
			{
			case Mode.Create:
				base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Auditai.UI.Platform.IconRes.CreateProject16);
				await PopulateCreate();
				break;
			case Mode.Modify:
				base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.ctxMofify);
				await PopulateModify();
				break;
			case Mode.Duplicate:
				base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Auditai.UI.Platform.IconRes.DuplicateProject16);
				await PopulateDuplicate();
				break;
			case Mode.FromProject:
				base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Auditai.UI.Platform.IconRes.SaveAsTemplate16);
				await PopulateFromProject();
				break;
			}
			PopulateTemplate();
		}
		finally
		{
			ctnMain.Enabled = true;
		}
	}

	internal bool ShowCreate()
	{
		_mode = Mode.Create;
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		return ShowDialog() == DialogResult.OK;
	}

	internal bool ShowModify()
	{
		_mode = Mode.Modify;
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		return ShowDialog() == DialogResult.OK;
	}

	internal bool ShowDuplicate()
	{
		_mode = Mode.Duplicate;
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		return ShowDialog() == DialogResult.OK;
	}

	internal bool ShowFromProject()
	{
		_mode = Mode.FromProject;
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		return ShowDialog() == DialogResult.OK;
	}

	private void SetTheme()
	{
		btnSwitchMode.FlatStyle = FlatStyle.Flat;
		btnSwitchMode.FlatAppearance.BorderSize = 0;
		ctnMain.SplitterWidth = 0;
		pnlUserSelect.BorderWidth = 1;
		pnlUserSelect.BorderColor = Color.DarkGray;
		projectUsersListSelector.SetTheme();
		projectUsersTileSelector.SetTheme();
		ckbSearch.FlatStyle = FlatStyle.Flat;
		ckbSearch.FlatAppearance.BorderSize = 0;
		ckbSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
		ckbSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
		ckbSearch.FlatAppearance.CheckedBackColor = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeContext.TileColor;
	}

	internal void PopulateTemplate()
	{
		cboCategory.Text = Template.Category;
		txtName.Text = Template.Name;
		txtNote.Text = Template.Note;
		txtNumber.Text = Template.Number;
		chkAllMembers.Checked = Template.TeamVisible;
	}

	private async Task PopulateCreate()
	{
		Text = "新建模板";
		await PopulateTeamUsers();
	}

	private async Task PopulateModify()
	{
		Text = "修改模板";
		await PopulateTeamUsers();
	}

	private async Task PopulateDuplicate()
	{
		Text = "复制模板";
		await PopulateTeamUsers();
	}

	private async Task PopulateFromProject()
	{
		Text = "另存模板";
		await PopulateTeamUsers();
	}

	private async Task PopulateTeamUsers()
	{
		try
		{
			if (StorageRouter.IsLocalMode)
			{
				var users = (await StorageRouter.GetTeamUsersWithPic()).ToList();
				Context.RootUsers = users;
				Context.UserGroups = new List<UserGroup>();
				Context.UserViewStates = users.Select(u => Tuple.Create(u, true)).ToList();
				projectUsersListSelector.Context = Context;
				projectUsersTileSelector.Context = Context;
				projectUsersListSelector.PopulateUsers();
				projectUsersTileSelector.PopulateUsers();
				return;
			}
			Auditai.Model.User current = Auditai.Model.User.Current;
			if (current.TeamId == Guid.Empty)
			{
				await WebApiClient.GetUserById(current.Id);
				projectUsersListSelector.PopulateUsers();
				projectUsersTileSelector.PopulateUsers();
				return;
			}
			Context.Project = Template;
			Context.ManagerId = UserTeam.Current.ManagerId;
			Tuple<List<Auditai.DTO.User>, List<UserGroup>> tuple = await WebApiClient.GetTeamUserGroups();
			Context.RootUsers = tuple.Item1;
			Context.UserGroups = tuple.Item2;
			Context.UserViewStates = (from u in Context.RootUsers.Concat(Context.UserGroups.SelectMany((UserGroup g) => g.DescendantsUsers()))
				select Tuple.Create(u, item2: true)).ToList();
			projectUsersListSelector.Context = Context;
			projectUsersTileSelector.Context = Context;
			projectUsersListSelector.PopulateUsers();
			projectUsersTileSelector.PopulateUsers();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
			Cancel();
		}
	}

	private bool ValidateTemplate()
	{
		if (string.IsNullOrWhiteSpace(txtNumber.Text))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "模板编号为必填项，请填写！");
			return false;
		}
		if (string.IsNullOrWhiteSpace(txtName.Text))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "模板名称为必填项，请填写！");
			return false;
		}
		IEnumerable<Auditai.DTO.User> enumerable = ((_ListOrTile == ListTileViewMode.Tile) ? projectUsersTileSelector.ValidateAndGetUsers() : projectUsersListSelector.ValidateAndGetUsers());
		if (enumerable == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "模板成员为必填项，请填写！");
			return false;
		}
		if (enumerable.Count() == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "模板成员为必填项，请填写！");
			return false;
		}
		var userList = enumerable.ToList();
		// 校验1: 成员归属团队校验 —— 选中的成员必须来自当前团队的用户列表
		var validTeamUserIds = new HashSet<long>(
			(Context?.RootUsers ?? new List<Auditai.DTO.User>()).Select(u => u.Id)
				.Concat((Context?.UserGroups ?? new List<UserGroup>())
					.SelectMany(g => g.DescendantsUsers()).Select(u => u.Id))
		);
		var invalidUser = userList.FirstOrDefault(u => !validTeamUserIds.Contains(u.Id));
			if (invalidUser != null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"成员\"" + invalidUser.Name + "\"不属于当前团队，请重新选择成员！");
				return false;
			}
			// 校验2: 创建者必须在成员列表中且角色为可编辑的用户（系统管理员/支持人员可绕过）
			var currentUser = Auditai.Model.User.Current;
			if (currentUser != null && !currentUser.IsSystemAdmin && !currentUser.IsSystemSupporter)
			{
				var creatorInList = userList.FirstOrDefault(u => u.Id == currentUser.Id);
				if (creatorInList == null)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
						"创建者必须在成员列表中，请勾选自己为成员！");
					return false;
				}
				if (creatorInList.Role != UserRole.Editor)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
						"创建者的角色必须为\"可编辑的用户\"，请调整自己的角色！");
					return false;
				}
			}
			// 校验3: 未勾选"所有同事可用"时，至少有 1 名"可使用的用户"
			if (!chkAllMembers.Checked && !userList.Any(u => u.Role == UserRole.User))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"未勾选\"所有同事可用\"时，成员中至少要包含一名\"可使用的用户\"，" +
					"否则其他同事将无法使用此" + StringConstBase.Current.Template + "。" +
					"如需全体同事可用，请勾选\"所有同事可用\"。");
				return false;
			}
		Template.Category = cboCategory.Text;
		Template.Name = txtName.Text;
		Template.Note = txtNote.Text;
		Template.Number = txtNumber.Text;
		Template.TeamVisible = chkAllMembers.Checked;
		Template.Users = enumerable.ToList();
		return true;
	}

	private async Task PopulateCategoryCombo()
	{
		if (!StorageRouter.IsLocalMode)
		{
			try
			{
				HashSet<string> hashSet = new HashSet<string>((await WebApiClient.GetTemplates()).Select((Auditai.DTO.Project p) => p.Category.Split('|')).SelectMany((string[] cats) => cats));
				hashSet.Remove("");
				cboCategory.Items.AddText(hashSet.ToArray());
			}
			catch (Exception ex) when (ex is HttpRequestException || ex is System.IO.IOException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
			}
		}
	}

	private void btnOk_Click(object sender, EventArgs e)
	{
		if (ValidateTemplate())
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void Cancel()
	{
		base.DialogResult = DialogResult.Cancel;
		Hide();
	}

	public void AttachTooltip()
	{
		chkAllMembers.MouseEnter += delegate
		{
			if (tooltipManager.ShouldDisplay)
			{
				Rectangle bounds2 = chkAllMembers.Bounds;
				int left2 = bounds2.Left;
				int top2 = bounds2.Top;
				TipInfo tipInfo2 = TipInfo.Parse(TipResource.新建模板窗体_所有成员可见);
				tooltipManager.Show(tipInfo2, this, left2, top2);
			}
		};
		chkAllMembers.MouseLeave += delegate
		{
			tooltipManager.Hide();
		};
		var projectUsersControl = projectUsersListSelector.GetControl();
		if (projectUsersControl != null)
		{
			projectUsersControl.MouseEnter += delegate
			{
				if (tooltipManager.ShouldDisplay)
				{
					Rectangle bounds = projectUsersListSelector.GetControl().Bounds;
					int left = bounds.Left;
					int top = bounds.Top;
					TipInfo tipInfo = TipInfo.Parse(TipResource.模板成员);
					if (tipInfo != null)
					{
						tooltipManager.Show(tipInfo, this, left, top);
					}
				}
			};
			projectUsersControl.MouseLeave += delegate
			{
				tooltipManager.Hide();
			};
		}
	}

	private void btnSwitchMode_Click(object sender, EventArgs e)
	{
		if (_ListOrTile == ListTileViewMode.List)
		{
			ToListOrTile(ListTileViewMode.Tile);
			_ListOrTile = ListTileViewMode.Tile;
		}
		else
		{
			ToListOrTile(ListTileViewMode.List);
			_ListOrTile = ListTileViewMode.List;
		}
	}

	private void ToListOrTile(ListTileViewMode listOrTile)
	{
		switch (listOrTile)
		{
		case ListTileViewMode.List:
			projectUsersListSelector.GetControl().BringToFront();
			btnSwitchMode.Image = Auditai.UI.Platform.IconRes.toolTileMode;
			break;
		case ListTileViewMode.Tile:
			projectUsersTileSelector.GetControl().BringToFront();
			btnSwitchMode.Image = Auditai.UI.Platform.IconRes.toolListMode;
			break;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.inputPanel = new C1.Win.C1InputPanel.C1InputPanel();
		this.基本信息 = new C1.Win.C1InputPanel.InputGroupHeader();
		this.lblNumber = new C1.Win.C1InputPanel.InputLabel();
		this.txtNumber = new C1.Win.C1InputPanel.InputTextBox();
		this.lblName = new C1.Win.C1InputPanel.InputLabel();
		this.txtName = new C1.Win.C1InputPanel.InputTextBox();
		this.lblCategory = new C1.Win.C1InputPanel.InputLabel();
		this.cboCategory = new C1.Win.C1InputPanel.InputComboBox();
		this.lblAllMembers = new C1.Win.C1InputPanel.InputLabel();
		this.chkAllMembers = new C1.Win.C1InputPanel.InputCheckBox();
		this.lblNote = new C1.Win.C1InputPanel.InputLabel();
		this.txtNote = new C1.Win.C1InputPanel.InputTextBox();
		this.ctnMain = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnOk = new C1.Win.C1Input.C1Button();
		this.pnlInfoInput = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1SplitterPanel1 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1InputPanel3 = new C1.Win.C1InputPanel.C1InputPanel();
		this.pnlUserHead = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txbSearch = new C1.Win.C1Input.C1TextBox();
		this.ckbSearch = new C1.Win.C1Input.C1CheckBox();
		this.btnSwitchMode = new C1.Win.C1Input.C1Button();
		this.c1InputPanel2 = new C1.Win.C1InputPanel.C1InputPanel();
		this.inputGroupHeader1 = new C1.Win.C1InputPanel.InputGroupHeader();
		this.pnlEmpty = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1InputPanel1 = new C1.Win.C1InputPanel.C1InputPanel();
		this.pnlUserSelect = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.inputPanel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnMain).BeginInit();
		this.ctnMain.SuspendLayout();
		this.pnlButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnOk).BeginInit();
		this.pnlInfoInput.SuspendLayout();
		this.c1SplitterPanel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel3).BeginInit();
		this.pnlUserHead.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txbSearch).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbSearch).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnSwitchMode).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel2).BeginInit();
		this.pnlEmpty.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel1).BeginInit();
		base.SuspendLayout();
		this.inputPanel.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.inputPanel.DesignScaleFactor = 1.293737f;
		this.inputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.inputPanel.Font = new System.Drawing.Font("微软雅黑", 10.5f);
		this.inputPanel.Items.Add(this.基本信息);
		this.inputPanel.Items.Add(this.lblNumber);
		this.inputPanel.Items.Add(this.txtNumber);
		this.inputPanel.Items.Add(this.lblName);
		this.inputPanel.Items.Add(this.txtName);
		this.inputPanel.Items.Add(this.lblCategory);
		this.inputPanel.Items.Add(this.cboCategory);
		this.inputPanel.Items.Add(this.lblAllMembers);
		this.inputPanel.Items.Add(this.chkAllMembers);
		this.inputPanel.Items.Add(this.lblNote);
		this.inputPanel.Items.Add(this.txtNote);
		this.inputPanel.Location = new System.Drawing.Point(0, 0);
		this.inputPanel.Name = "inputPanel";
		this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 2, 2, 2);
		this.inputPanel.Size = new System.Drawing.Size(637, 547);
		this.inputPanel.TabIndex = 0;
		this.基本信息.Name = "基本信息";
		this.基本信息.Text = "基本信息";
		this.lblNumber.Name = "lblNumber";
		this.lblNumber.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
		this.lblNumber.Text = "模板编号*";
		this.lblNumber.Width = 98;
		this.txtNumber.Name = "txtNumber";
		this.txtNumber.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
		this.txtNumber.Width = 377;
		this.lblName.Name = "lblName";
		this.lblName.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.lblName.Text = "模板名称*";
		this.lblName.Width = 98;
		this.txtName.Name = "txtName";
		this.txtName.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.txtName.Width = 377;
		this.lblCategory.Name = "lblCategory";
		this.lblCategory.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.lblCategory.Text = "模板类别";
		this.lblCategory.Width = 98;
		this.cboCategory.Name = "txtCategory";
		this.cboCategory.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.cboCategory.Width = 377;
		this.lblAllMembers.Name = "lblAllMembers";
		this.lblAllMembers.Width = 98;
		this.chkAllMembers.Name = "chkAllMembers";
		this.chkAllMembers.Text = "所有同事可用";
		this.lblNote.Name = "lblNote";
		this.lblNote.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.lblNote.Text = "模板备注";
		this.lblNote.Width = 98;
		this.txtNote.AcceptsReturn = true;
		this.txtNote.Height = 225;
		this.txtNote.Multiline = true;
		this.txtNote.Name = "txtNote";
		this.txtNote.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.txtNote.VerticalAlign = C1.Win.C1InputPanel.InputContentAlignment.Spread;
		this.txtNote.Width = 377;
		this.ctnMain.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnMain.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctnMain.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnMain.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnMain.FixedLineWidth = 0;
		this.ctnMain.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctnMain.HeaderHeight = 0;
		this.ctnMain.Location = new System.Drawing.Point(0, 0);
		this.ctnMain.Name = "ctnMain";
		this.ctnMain.Panels.Add(this.pnlButtons);
		this.ctnMain.Panels.Add(this.pnlInfoInput);
		this.ctnMain.Panels.Add(this.c1SplitterPanel1);
		this.ctnMain.Panels.Add(this.pnlUserHead);
		this.ctnMain.Panels.Add(this.pnlEmpty);
		this.ctnMain.Panels.Add(this.pnlUserSelect);
		this.ctnMain.Size = new System.Drawing.Size(1100, 625);
		this.ctnMain.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnMain.SplitterWidth = 1;
		this.ctnMain.TabIndex = 1;
		this.ctnMain.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlButtons.Controls.Add(this.btnCancel);
		this.pnlButtons.Controls.Add(this.btnOk);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 82;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point(0, 543);
		this.pnlButtons.MinHeight = 30;
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Size = new System.Drawing.Size(1100, 82);
		this.pnlButtons.TabIndex = 1;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(962, 27);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(91, 34);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOk.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnOk.Location = new System.Drawing.Point(826, 27);
		this.btnOk.Name = "btnOk";
		this.btnOk.Size = new System.Drawing.Size(91, 34);
		this.btnOk.TabIndex = 0;
		this.btnOk.Text = "确定";
		this.btnOk.UseVisualStyleBackColor = true;
		this.btnOk.Click += new System.EventHandler(btnOk_Click);
		this.pnlInfoInput.Controls.Add(this.inputPanel);
		this.pnlInfoInput.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlInfoInput.Height = 541;
		this.pnlInfoInput.KeepRelativeSize = false;
		this.pnlInfoInput.Location = new System.Drawing.Point(0, 0);
		this.pnlInfoInput.Name = "pnlInfoInput";
		this.pnlInfoInput.Size = new System.Drawing.Size(637, 541);
		this.pnlInfoInput.SizeRatio = 59.683;
		this.pnlInfoInput.TabIndex = 0;
		this.pnlInfoInput.Width = 637;
		this.c1SplitterPanel1.Controls.Add(this.c1InputPanel3);
		this.c1SplitterPanel1.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.c1SplitterPanel1.Location = new System.Drawing.Point(640, 0);
		this.c1SplitterPanel1.MinWidth = 5;
		this.c1SplitterPanel1.Name = "c1SplitterPanel1";
		this.c1SplitterPanel1.Size = new System.Drawing.Size(8, 541);
		this.c1SplitterPanel1.SizeRatio = 1.818;
		this.c1SplitterPanel1.TabIndex = 5;
		this.c1SplitterPanel1.Width = 8;
		this.c1InputPanel3.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1InputPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1InputPanel3.Font = new System.Drawing.Font("微软雅黑", 10.5f);
		this.c1InputPanel3.Location = new System.Drawing.Point(0, 0);
		this.c1InputPanel3.Name = "c1InputPanel3";
		this.c1InputPanel3.Size = new System.Drawing.Size(8, 541);
		this.c1InputPanel3.TabIndex = 1;
		this.pnlUserHead.Controls.Add(this.txbSearch);
		this.pnlUserHead.Controls.Add(this.ckbSearch);
		this.pnlUserHead.Controls.Add(this.btnSwitchMode);
		this.pnlUserHead.Controls.Add(this.c1InputPanel2);
		this.pnlUserHead.Height = 52;
		this.pnlUserHead.Location = new System.Drawing.Point(650, 0);
		this.pnlUserHead.MinHeight = 30;
		this.pnlUserHead.Name = "pnlUserHead";
		this.pnlUserHead.Size = new System.Drawing.Size(450, 52);
		this.pnlUserHead.SizeRatio = 9.662;
		this.pnlUserHead.TabIndex = 3;
		this.txbSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txbSearch.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txbSearch.Location = new System.Drawing.Point(83, 5);
		this.txbSearch.Name = "txbSearch";
		this.txbSearch.Size = new System.Drawing.Size(295, 27);
		this.txbSearch.TabIndex = 4;
		this.txbSearch.Tag = null;
		this.txbSearch.Visible = false;
		this.ckbSearch.Appearance = System.Windows.Forms.Appearance.Button;
		this.ckbSearch.BackColor = System.Drawing.Color.Transparent;
		this.ckbSearch.BackgroundImage = Auditai.UI.Platform.IconRes.btnSearch;
		this.ckbSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.ckbSearch.BorderColor = System.Drawing.Color.Transparent;
		this.ckbSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbSearch.FlatAppearance.BorderSize = 0;
		this.ckbSearch.ForeColor = System.Drawing.Color.Black;
		this.ckbSearch.Location = new System.Drawing.Point(386, 4);
		this.ckbSearch.Name = "ckbSearch";
		this.ckbSearch.Padding = new System.Windows.Forms.Padding(1);
		this.ckbSearch.Size = new System.Drawing.Size(31, 29);
		this.ckbSearch.TabIndex = 3;
		this.ckbSearch.UseVisualStyleBackColor = true;
		this.ckbSearch.Value = null;
		this.btnSwitchMode.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnSwitchMode.FlatAppearance.BorderSize = 0;
		this.btnSwitchMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSwitchMode.Image = Auditai.UI.Platform.IconRes.toolListMode;
		this.btnSwitchMode.Location = new System.Drawing.Point(417, 4);
		this.btnSwitchMode.Name = "btnSwitchMode";
		this.btnSwitchMode.Size = new System.Drawing.Size(31, 29);
		this.btnSwitchMode.TabIndex = 3;
		this.btnSwitchMode.UseVisualStyleBackColor = true;
		this.btnSwitchMode.Click += new System.EventHandler(btnSwitchMode_Click);
		this.c1InputPanel2.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1InputPanel2.DesignScaleFactor = 1.293737f;
		this.c1InputPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1InputPanel2.Font = new System.Drawing.Font("微软雅黑", 10.5f);
		this.c1InputPanel2.Items.Add(this.inputGroupHeader1);
		this.c1InputPanel2.Location = new System.Drawing.Point(0, 0);
		this.c1InputPanel2.Name = "c1InputPanel2";
		this.c1InputPanel2.Padding = new System.Windows.Forms.Padding(0, 2, 2, 2);
		this.c1InputPanel2.Size = new System.Drawing.Size(450, 52);
		this.c1InputPanel2.TabIndex = 0;
		this.inputGroupHeader1.Name = "inputGroupHeader1";
		this.inputGroupHeader1.Text = "选择成员";
		this.pnlEmpty.Controls.Add(this.c1InputPanel1);
		this.pnlEmpty.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlEmpty.Height = 21;
		this.pnlEmpty.Location = new System.Drawing.Point(650, 520);
		this.pnlEmpty.MinHeight = 5;
		this.pnlEmpty.Name = "pnlEmpty";
		this.pnlEmpty.Size = new System.Drawing.Size(450, 21);
		this.pnlEmpty.SizeRatio = 4.301;
		this.pnlEmpty.TabIndex = 4;
		this.c1InputPanel1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1InputPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1InputPanel1.Font = new System.Drawing.Font("微软雅黑", 10.5f);
		this.c1InputPanel1.Location = new System.Drawing.Point(0, 0);
		this.c1InputPanel1.Name = "c1InputPanel1";
		this.c1InputPanel1.Size = new System.Drawing.Size(450, 21);
		this.c1InputPanel1.TabIndex = 0;
		this.pnlUserSelect.BorderWidth = 1;
		this.pnlUserSelect.Height = 463;
		this.pnlUserSelect.Location = new System.Drawing.Point(651, 56);
		this.pnlUserSelect.Name = "pnlUserSelect";
		this.pnlUserSelect.Size = new System.Drawing.Size(447, 460);
		this.pnlUserSelect.SizeRatio = 94.334;
		this.pnlUserSelect.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1100, 625);
		base.Controls.Add(this.ctnMain);
		this.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "dlgTemplateEditor";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "dlgTemplateEditor";
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		((System.ComponentModel.ISupportInitialize)this.inputPanel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnMain).EndInit();
		this.ctnMain.ResumeLayout(false);
		this.pnlButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnOk).EndInit();
		this.pnlInfoInput.ResumeLayout(false);
		this.c1SplitterPanel1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel3).EndInit();
		this.pnlUserHead.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txbSearch).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbSearch).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnSwitchMode).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel2).EndInit();
		this.pnlEmpty.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel1).EndInit();
		base.ResumeLayout(false);
	}
}
