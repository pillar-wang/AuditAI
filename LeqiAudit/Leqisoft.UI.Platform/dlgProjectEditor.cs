﻿﻿﻿﻿﻿using System;
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
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;
using Leqisoft.LocalDataStore;
using Leqisoft.Util;
using Newtonsoft.Json.Linq;

namespace Leqisoft.UI.Platform;

public class dlgProjectEditor : C1RibbonForm
{
	private enum Mode
	{
		Create,
		Duplicate,
		Modify
	}

	private TooltipManager tooltipManager = new TooltipManager();

	private List<Leqisoft.DTO.Project> templates;

	private Mode _mode;

	private readonly ProjectUsersListSelector projectUsersListSelector;

	private readonly ProjectUsersTileSelector projectUsersTileSelector;

	private static readonly Regex _rxSplit = new Regex("[,，]");

	private IContainer components;

	private C1InputPanel inputPanel;

	private InputLabel lblNumber;

	private InputLabel lblCategory;

	private InputLabel lblName;

	private InputLabel lblParent;

	private InputLabel lblTemplate;

	private InputLabel lblNote;

	private InputTextBox txtNumber;

	private InputComboBox cboCategory;

	private InputTextBox txtName;

	private InputComboBox cboParent;

	private InputComboBox cboTemplate;

	private InputTextBox txtNote;

	private C1SplitContainer ctnMain;

	private C1SplitterPanel pnlProjectInfo;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancel;

	private C1Button btnOk;

	private InputLabel lblAuditee;

	private InputTextBox txtAuditee;

	private C1SplitterPanel pnlUserSelector;

	private C1SplitterPanel pnlUserHeader;

	private C1Button btnSwitchUserSelector;

	private C1InputPanel c1InputPanel1;

	private InputGroupHeader inputGroupHeader2;

	private C1SplitterPanel pnlEmpty;

	private C1SplitterPanel c1SplitterPanel1;

	private C1InputPanel c1InputPanel2;

	private C1InputPanel c1InputPanel3;

	private C1CheckBox ckbSearch;

	private C1TextBox txbSearch;

	private InputGroupHeader inputGroupHeader1;

	private ListTileViewMode _ListOrTile
	{
		get
		{
			return (Leqisoft.UI.Platform.ListTileViewMode)UserSet.Config.ProjectMembersViewMode;
		}
		set
		{
			UserSet.Config.ProjectMembersViewMode = (dynamic)value;
		}
	}

	public Leqisoft.DTO.Project Project { get; set; }

	public ProjectUsersSelectorContext Context { get; } = new ProjectUsersSelectorContext();


	public dlgProjectEditor()
	{
		base.ShowInTaskbar = false;
		InitializeComponent();
		base.Load += DlgProjectEditor_Load;
		ckbSearch.CheckedChanged += CkbSearch_CheckedChanged;
		txbSearch.TextChanged += TxbSearch_TextChanged;
		projectUsersListSelector = new ProjectUsersListSelector();
		pnlUserSelector.Controls.Add(projectUsersListSelector.GetControl());
		projectUsersTileSelector = new ProjectUsersTileSelector();
		pnlUserSelector.Controls.Add(projectUsersTileSelector.GetControl());
		inputPanel.ForceScrollBars = false;
		lblNumber.Text = StringConstBase.Current.Project + "编号*";
		lblName.Text = StringConstBase.Current.Project + "名称*";
		lblCategory.Text = StringConstBase.Current.Project + "类别";
		lblParent.Text = "上级" + StringConstBase.Current.Project;
		lblAuditee.Text = StringConstBase.Current.Auditee + "*";
		lblNote.Text = StringConstBase.Current.Project + "备注";
		base.Shown += DlgProjectEditor_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		AttachTooltip();
	}

	private void DlgProjectEditor_Load(object sender, EventArgs e)
	{
		ToListOrTile(_ListOrTile);
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
			Tuple<Leqisoft.DTO.User, bool> uvs = Context.UserViewStates[i];
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

	internal bool ShowCreate()
	{
		_mode = Mode.Create;
		if (Project.TemplateId.HasValue)
		{
			base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(Leqisoft.UI.Platform.Properties.Resources.CreateProjectFromTemplate16);
		}
		else
		{
			base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(Leqisoft.UI.Platform.Properties.Resources.CreateProject16);
		}
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		return ShowDialog() == DialogResult.OK;
	}

	internal bool ShowDuplicate()
	{
		_mode = Mode.Duplicate;
		base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(Leqisoft.UI.Platform.Properties.Resources.DuplicateProject16);
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		return ShowDialog() == DialogResult.OK;
	}

	internal bool ShowModify()
	{
		_mode = Mode.Modify;
		base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(ContextResources.ctxMofify);
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		return ShowDialog() == DialogResult.OK;
	}

	private void SetTheme()
	{
		ctnMain.SplitterWidth = 0;
		pnlUserSelector.BorderWidth = 1;
		pnlUserSelector.BorderColor = Color.DarkGray;
		pnlUserHeader.BackColor = Color.White;
		btnSwitchUserSelector.FlatStyle = FlatStyle.Flat;
		btnSwitchUserSelector.FlatAppearance.BorderSize = 0;
		btnSwitchUserSelector.BackColor = Color.Transparent;
		projectUsersListSelector.SetTheme();
		projectUsersTileSelector.SetTheme();
		ckbSearch.FlatStyle = FlatStyle.Flat;
		ckbSearch.FlatAppearance.BorderSize = 0;
		ckbSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
		ckbSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
	}

	internal async Task PopulateProject()
	{
		cboCategory.Text = Project.Category;
		txtName.Text = Project.Name;
		txtNote.Text = Project.Note;
		txtNumber.Text = Project.Number;
		txtAuditee.Text = Project.Auditee;
		cboParent.Items.Clear();
		cboParent.Items.AddText("(上级" + StringConstBase.Current.Project + "为空)");
		try
		{
			IEnumerable<Leqisoft.DTO.Project> enumerable = (await StorageRouter.GetProjects()).Where((Leqisoft.DTO.Project p) => p.Id != Project.Id);
			foreach (Leqisoft.DTO.Project item in enumerable)
			{
				int index = cboParent.Items.AddText(item.Number + " " + item.Name);
				cboParent.Items[index].Tag = item;
			}
			if (Project.ParentId.HasValue)
			{
				foreach (InputComponent item2 in cboParent.Items)
				{
					if (item2.Tag is Leqisoft.DTO.Project project && project.Id == Project.ParentId.Value)
					{
						cboParent.SelectedItem = item2;
					}
				}
				return;
			}
			cboParent.SelectedIndex = 0;
		}
		catch (Exception ex) when (ex is HttpRequestException || ex is System.IO.IOException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private async Task<bool> ValidateProject()
	{
		if (string.IsNullOrWhiteSpace(txtNumber.Text))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, StringConstBase.Current.Project + "编号为必填项，请填写！");
			return false;
		}
		if (string.IsNullOrWhiteSpace(txtName.Text))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, StringConstBase.Current.Project + "名称为必填项，请填写！");
			return false;
		}
		if (string.IsNullOrWhiteSpace(txtAuditee.Text))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, StringConstBase.Current.Auditee + "为必填项，请填写！");
			return false;
		}
		IEnumerable<Leqisoft.DTO.User> users = (IEnumerable<Leqisoft.DTO.User>)((_ListOrTile == ListTileViewMode.Tile) ? projectUsersTileSelector.ValidateAndGetUsers() : projectUsersListSelector.ValidateAndGetUsers());
		if (users == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, StringConstBase.Current.Project + "成员为必填项，请设置并勾选！");
			return false;
		}
		if (users.Count() == 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, StringConstBase.Current.Project + "成员为必填项，请设置并勾选！");
			return false;
		}
		if (!StorageRouter.IsLocalMode)
		{
			JObject jObject = new JObject();
			jObject.Add("TeamId", Leqisoft.Model.User.Current.TeamId);
			IEnumerable<Leqisoft.DTO.User> source = await WebApiClient.GetTeamUserPermissions(jObject);
			HashSet<long> userIds = new HashSet<long>(users.Select((Leqisoft.DTO.User u) => u.Id));
			List<Leqisoft.DTO.User> list = (from u in source
				where u.Permissions.MustInclude
				where !userIds.Contains(u.Id)
				select u).ToList();
			if (list.Count > 0)
			{
				string text = string.Join("、", list.Select((Leqisoft.DTO.User u) => u.Name));
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "根据系统管理员设置的权限规则，您必须选择" + text + "加入" + StringConstBase.Current.Project + "。");
				return false;
			}
		}
		int num = cboTemplate.SelectedIndex - 1;
		if (num < 0)
		{
			if (_mode == Mode.Create)
			{
				if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "您未选择使用" + StringConstBase.Current.Template + "，这将会为您创建一个无任何文件的空" + StringConstBase.Current.Project + "，确定要创建一个空" + StringConstBase.Current.Project + "吗？", MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					return false;
				}
				Project.TemplateId = null;
			}
		}
		else
		{
			Project.TemplateId = templates[num].Id;
		}
		Project.Category = cboCategory.Text;
		Project.Name = txtName.Text;
		Project.Note = txtNote.Text;
		Project.Number = txtNumber.Text;
		Project.Auditee = txtAuditee.Text;
		Project.TeamVisible = false;
		Project.Users = users.ToList();
		if (cboParent.SelectedIndex > 0)
		{
			Project.ParentId = ((Leqisoft.DTO.Project)((InputComponent)cboParent.SelectedItem).Tag).Id;
		}
		else
		{
			Project.ParentId = null;
		}
		return true;
	}

	private async void DlgProjectEditor_Shown(object sender, EventArgs e)
	{
		ctnMain.Enabled = false;
		await PopulateCategoryCombo();
		await PopulateTeamUsers();
		await PopulateProject();
		ctnMain.Enabled = true;
	}

	private async void btnOk_Click(object sender, EventArgs e)
	{
		btnOk.Enabled = false;
		if (await ValidateProject())
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
		btnOk.Enabled = true;
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	protected override async void OnShown(EventArgs e)
	{
		base.OnShown(e);
		switch (_mode)
		{
		case Mode.Create:
			await PopulateCreate();
			break;
		case Mode.Duplicate:
			PopulateDuplicate();
			break;
		case Mode.Modify:
			PopulateModify();
			break;
		}
	}

	private void Cancel()
	{
		base.DialogResult = DialogResult.Cancel;
		Hide();
	}

	private async Task PopulateCreate()
	{
		Text = "新建" + StringConstBase.Current.Project;
		cboTemplate.Enabled = true;
		cboTemplate.Items.Clear();
		cboTemplate.Items.AddText("(" + StringConstBase.Current.NotUseTemplate + ")");
		cboTemplate.SelectedIndex = 0;
		try
		{
			if (!StorageRouter.IsLocalMode)
			{
				templates = (await WebApiClient.GetTemplates()).ToList();
			}
			else
			{
				templates = (await StorageRouter.GetTemplates()).ToList();
			}
			foreach (Leqisoft.DTO.Project template in templates)
				{
					cboTemplate.Items.AddText(template.Number + " " + template.Name);
					if (Project.TemplateId.HasValue && template.Id == Project.TemplateId.Value)
					{
						cboTemplate.SelectedIndex = cboTemplate.Items.Count - 1;
					}
				}
		}
		catch (Exception ex) when (ex is HttpRequestException || ex is System.IO.IOException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
			Cancel();
		}
	}

	private void PopulateModify()
	{
		Text = "修改" + StringConstBase.Current.Project;
		cboTemplate.Enabled = false;
	}

	private void PopulateDuplicate()
	{
		Text = "复制" + StringConstBase.Current.Project;
		cboTemplate.Enabled = false;
	}

	private async Task PopulateCategoryCombo()
	{
		var projects = await StorageRouter.GetProjects();
		HashSet<string> hashSet = new HashSet<string>(projects.Select((Leqisoft.DTO.Project p) => p.Category?.Split('|')).SelectMany((string[] cats) => cats ?? new string[0]));
		hashSet.Remove("");
		cboCategory.Items.AddText(hashSet.ToArray());
	}

	private async Task PopulateTeamUsers()
	{
		_ = 1;
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
			Leqisoft.Model.User current = Leqisoft.Model.User.Current;
			if (current.TeamId == Guid.Empty)
			{
				await WebApiClient.GetUserById(current.Id);
				projectUsersListSelector.PopulateUsers();
				projectUsersTileSelector.PopulateUsers();
				return;
			}
			Context.Project = Project;
			Context.ManagerId = UserTeam.Current.ManagerId;
			Tuple<List<Leqisoft.DTO.User>, List<UserGroup>> tuple = await WebApiClient.GetTeamUserGroups();
			Context.RootUsers = tuple.Item1;
			Context.UserGroups = tuple.Item2;
			Context.UserViewStates = (from u in Context.RootUsers.Concat(Context.UserGroups.SelectMany((UserGroup g) => g.DescendantsUsers()))
				select Tuple.Create(u, item2: true)).ToList();
			projectUsersListSelector.Context = Context;
			projectUsersTileSelector.Context = Context;
			projectUsersListSelector.PopulateUsers();
			projectUsersTileSelector.PopulateUsers();
		}
		catch (Exception ex) when (ex is HttpRequestException || ex is System.IO.IOException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
			Cancel();
		}
	}

	public void AttachTooltip()
	{
		attachTooltip(cboParent, TipInfo.Parse(TipResource.上级Project));
		attachTooltip(cboTemplate, TipInfo.Parse(TipResource.使用模板));
		var projectUsersControl = projectUsersListSelector.GetControl();
		if (projectUsersControl != null)
		{
			projectUsersControl.MouseEnter += delegate
			{
				if (tooltipManager.ShouldDisplay)
				{
					Rectangle bounds2 = projectUsersListSelector.GetControl().Bounds;
					int left2 = bounds2.Left;
					int top2 = bounds2.Top;
					TipInfo tipInfo2 = TipInfo.Parse(TipResource.Project成员);
					if (tipInfo2 != null)
					{
						tooltipManager.Show(tipInfo2, this, left2, top2);
					}
				}
			};
			projectUsersControl.MouseLeave += delegate
			{
				tooltipManager.Hide();
			};
		}
		void attachTooltip(Component component, TipInfo tipInfo)
		{
			InputComponent cpt = component as InputComponent;
			if (cpt != null)
			{
				cpt.MouseEnter += delegate
				{
					if (tooltipManager.ShouldDisplay)
					{
						Rectangle bounds = cpt.Bounds;
						int left = bounds.Left;
						int top = bounds.Top;
						tooltipManager.Show(tipInfo, this, left, top);
					}
				};
				cpt.MouseLeave += delegate
				{
					tooltipManager.Hide();
				};
			}
			tooltipManager.Attach(component, tipInfo);
		}
	}

	private void btnSwitchUserSelector_Click(object sender, EventArgs e)
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
			btnSwitchUserSelector.Image = Leqisoft.UI.Platform.Properties.Resources.toolTileMode;
			break;
		case ListTileViewMode.Tile:
			projectUsersTileSelector.GetControl().BringToFront();
			btnSwitchUserSelector.Image = Leqisoft.UI.Platform.Properties.Resources.toolListMode;
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
		this.lblNumber = new C1.Win.C1InputPanel.InputLabel();
		this.txtNumber = new C1.Win.C1InputPanel.InputTextBox();
		this.lblName = new C1.Win.C1InputPanel.InputLabel();
		this.txtName = new C1.Win.C1InputPanel.InputTextBox();
		this.lblCategory = new C1.Win.C1InputPanel.InputLabel();
		this.cboCategory = new C1.Win.C1InputPanel.InputComboBox();
		this.lblAuditee = new C1.Win.C1InputPanel.InputLabel();
		this.txtAuditee = new C1.Win.C1InputPanel.InputTextBox();
		this.lblParent = new C1.Win.C1InputPanel.InputLabel();
		this.cboParent = new C1.Win.C1InputPanel.InputComboBox();
		this.lblTemplate = new C1.Win.C1InputPanel.InputLabel();
		this.cboTemplate = new C1.Win.C1InputPanel.InputComboBox();
		this.lblNote = new C1.Win.C1InputPanel.InputLabel();
		this.txtNote = new C1.Win.C1InputPanel.InputTextBox();
		this.ctnMain = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnOk = new C1.Win.C1Input.C1Button();
		this.pnlProjectInfo = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1SplitterPanel1 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1InputPanel2 = new C1.Win.C1InputPanel.C1InputPanel();
		this.pnlUserHeader = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txbSearch = new C1.Win.C1Input.C1TextBox();
		this.ckbSearch = new C1.Win.C1Input.C1CheckBox();
		this.btnSwitchUserSelector = new C1.Win.C1Input.C1Button();
		this.c1InputPanel1 = new C1.Win.C1InputPanel.C1InputPanel();
		this.inputGroupHeader2 = new C1.Win.C1InputPanel.InputGroupHeader();
		this.pnlEmpty = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1InputPanel3 = new C1.Win.C1InputPanel.C1InputPanel();
		this.pnlUserSelector = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.inputGroupHeader1 = new C1.Win.C1InputPanel.InputGroupHeader();
		((System.ComponentModel.ISupportInitialize)this.inputPanel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnMain).BeginInit();
		this.ctnMain.SuspendLayout();
		this.pnlButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnOk).BeginInit();
		this.pnlProjectInfo.SuspendLayout();
		this.c1SplitterPanel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel2).BeginInit();
		this.pnlUserHeader.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txbSearch).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbSearch).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnSwitchUserSelector).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel1).BeginInit();
		this.pnlEmpty.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel3).BeginInit();
		base.SuspendLayout();
		this.inputPanel.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.inputPanel.DesignScaleFactor = 1.293737f;
		this.inputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.inputPanel.Font = new System.Drawing.Font("Microsoft YaHei UI", 9f);
		this.inputPanel.Items.Add(this.inputGroupHeader1);
		this.inputPanel.Items.Add(this.lblNumber);
		this.inputPanel.Items.Add(this.txtNumber);
		this.inputPanel.Items.Add(this.lblName);
		this.inputPanel.Items.Add(this.txtName);
		this.inputPanel.Items.Add(this.lblCategory);
		this.inputPanel.Items.Add(this.cboCategory);
		this.inputPanel.Items.Add(this.lblAuditee);
		this.inputPanel.Items.Add(this.txtAuditee);
		this.inputPanel.Items.Add(this.lblParent);
		this.inputPanel.Items.Add(this.cboParent);
		this.inputPanel.Items.Add(this.lblTemplate);
		this.inputPanel.Items.Add(this.cboTemplate);
		this.inputPanel.Items.Add(this.lblNote);
		this.inputPanel.Items.Add(this.txtNote);
		this.inputPanel.Location = new System.Drawing.Point(0, 0);
		this.inputPanel.Margin = new System.Windows.Forms.Padding(0, 4, 3, 4);
		this.inputPanel.Name = "inputPanel";
		this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 2, 2, 2);
		this.inputPanel.Size = new System.Drawing.Size(490, 421);
		this.inputPanel.TabIndex = 0;
		this.lblNumber.Name = "lblNumber";
		this.lblNumber.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
		this.lblNumber.Width = 75;
		this.txtNumber.Name = "txtNumber";
		this.txtNumber.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
		this.txtNumber.Width = 290;
		this.lblName.Name = "lblName";
		this.lblName.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.lblName.Width = 75;
		this.txtName.Name = "txtName";
		this.txtName.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.txtName.Width = 290;
		this.lblCategory.Name = "lblCategory";
		this.lblCategory.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.lblCategory.Width = 75;
		this.cboCategory.Name = "txtCategory";
		this.cboCategory.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.cboCategory.Width = 290;
		this.lblAuditee.Name = "lblAuditee";
		this.lblAuditee.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.lblAuditee.Width = 75;
		this.txtAuditee.Name = "txtAuditee";
		this.txtAuditee.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.txtAuditee.Width = 290;
		this.lblParent.Name = "lblParent";
		this.lblParent.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.lblParent.Width = 75;
		this.cboParent.DropDownStyle = C1.Win.C1InputPanel.InputComboBoxStyle.DropDownList;
		this.cboParent.Name = "cboParent";
		this.cboParent.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.cboParent.Width = 290;
		this.lblTemplate.Name = "lblTemplate";
		this.lblTemplate.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.lblTemplate.Text = "使用模板";
		this.lblTemplate.Width = 75;
		this.cboTemplate.DropDownStyle = C1.Win.C1InputPanel.InputComboBoxStyle.DropDownList;
		this.cboTemplate.Name = "cboTemplate";
		this.cboTemplate.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.cboTemplate.Width = 290;
		this.lblNote.Name = "lblNote";
		this.lblNote.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.lblNote.Width = 75;
		this.txtNote.AcceptsReturn = true;
		this.txtNote.Height = 100;
		this.txtNote.Multiline = true;
		this.txtNote.Name = "txtNote";
		this.txtNote.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
		this.txtNote.VerticalAlign = C1.Win.C1InputPanel.InputContentAlignment.Spread;
		this.txtNote.Width = 290;
		this.ctnMain.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnMain.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctnMain.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnMain.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnMain.FixedLineWidth = 0;
		this.ctnMain.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctnMain.HeaderHeight = 27;
		this.ctnMain.Location = new System.Drawing.Point(0, 0);
		this.ctnMain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctnMain.Name = "ctnMain";
		this.ctnMain.Panels.Add(this.pnlButtons);
		this.ctnMain.Panels.Add(this.pnlProjectInfo);
		this.ctnMain.Panels.Add(this.c1SplitterPanel1);
		this.ctnMain.Panels.Add(this.pnlUserHeader);
		this.ctnMain.Panels.Add(this.pnlEmpty);
		this.ctnMain.Panels.Add(this.pnlUserSelector);
		this.ctnMain.Size = new System.Drawing.Size(846, 481);
		this.ctnMain.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnMain.SplitterWidth = 1;
		this.ctnMain.TabIndex = 1;
		this.ctnMain.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlButtons.Controls.Add(this.btnCancel);
		this.pnlButtons.Controls.Add(this.btnOk);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 60;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point(0, 421);
		this.pnlButtons.MinHeight = 39;
		this.pnlButtons.MinWidth = 52;
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Resizable = false;
		this.pnlButtons.Size = new System.Drawing.Size(846, 60);
		this.pnlButtons.SizeRatio = 5.946;
		this.pnlButtons.TabIndex = 1;
		this.pnlButtons.Width = 846;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(749, 21);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOk.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnOk.Location = new System.Drawing.Point(656, 21);
		this.btnOk.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnOk.Name = "btnOk";
		this.btnOk.Size = new System.Drawing.Size(70, 26);
		this.btnOk.TabIndex = 0;
		this.btnOk.Text = "确定";
		this.btnOk.UseVisualStyleBackColor = true;
		this.btnOk.Click += new System.EventHandler(btnOk_Click);
		this.pnlProjectInfo.Controls.Add(this.inputPanel);
		this.pnlProjectInfo.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlProjectInfo.Height = 421;
		this.pnlProjectInfo.Location = new System.Drawing.Point(0, 0);
		this.pnlProjectInfo.MinHeight = 52;
		this.pnlProjectInfo.MinWidth = 52;
		this.pnlProjectInfo.Name = "pnlProjectInfo";
		this.pnlProjectInfo.Size = new System.Drawing.Size(490, 421);
		this.pnlProjectInfo.SizeRatio = 57.988;
		this.pnlProjectInfo.TabIndex = 0;
		this.pnlProjectInfo.Width = 490;
		this.c1SplitterPanel1.Controls.Add(this.c1InputPanel2);
		this.c1SplitterPanel1.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.c1SplitterPanel1.Location = new System.Drawing.Point(491, 0);
		this.c1SplitterPanel1.MinWidth = 5;
		this.c1SplitterPanel1.Name = "c1SplitterPanel1";
		this.c1SplitterPanel1.Size = new System.Drawing.Size(8, 421);
		this.c1SplitterPanel1.SizeRatio = 2.286;
		this.c1SplitterPanel1.TabIndex = 5;
		this.c1SplitterPanel1.Width = 8;
		this.c1InputPanel2.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1InputPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1InputPanel2.Font = new System.Drawing.Font("Microsoft YaHei UI", 9f);
		this.c1InputPanel2.Location = new System.Drawing.Point(0, 0);
		this.c1InputPanel2.Name = "c1InputPanel2";
		this.c1InputPanel2.Size = new System.Drawing.Size(8, 421);
		this.c1InputPanel2.TabIndex = 0;
		this.pnlUserHeader.Controls.Add(this.txbSearch);
		this.pnlUserHeader.Controls.Add(this.ckbSearch);
		this.pnlUserHeader.Controls.Add(this.btnSwitchUserSelector);
		this.pnlUserHeader.Controls.Add(this.c1InputPanel1);
		this.pnlUserHeader.Height = 40;
		this.pnlUserHeader.Location = new System.Drawing.Point(500, 0);
		this.pnlUserHeader.MinHeight = 30;
		this.pnlUserHeader.MinWidth = 52;
		this.pnlUserHeader.Name = "pnlUserHeader";
		this.pnlUserHeader.Resizable = false;
		this.pnlUserHeader.Size = new System.Drawing.Size(346, 40);
		this.pnlUserHeader.SizeRatio = 9.501;
		this.pnlUserHeader.TabIndex = 3;
		this.pnlUserHeader.Width = 346;
		this.txbSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txbSearch.Location = new System.Drawing.Point(64, 4);
		this.txbSearch.Name = "txbSearch";
		this.txbSearch.Size = new System.Drawing.Size(227, 21);
		this.txbSearch.TabIndex = 3;
		this.txbSearch.Tag = null;
		this.txbSearch.Visible = false;
		this.ckbSearch.Appearance = System.Windows.Forms.Appearance.Button;
		this.ckbSearch.BackColor = System.Drawing.Color.Transparent;
		this.ckbSearch.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.btnSearch;
		this.ckbSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.ckbSearch.BorderColor = System.Drawing.Color.Transparent;
		this.ckbSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbSearch.FlatAppearance.BorderSize = 0;
		this.ckbSearch.ForeColor = System.Drawing.Color.Black;
		this.ckbSearch.Location = new System.Drawing.Point(297, 3);
		this.ckbSearch.Name = "ckbSearch";
		this.ckbSearch.Padding = new System.Windows.Forms.Padding(1);
		this.ckbSearch.Size = new System.Drawing.Size(24, 22);
		this.ckbSearch.TabIndex = 2;
		this.ckbSearch.UseVisualStyleBackColor = true;
		this.ckbSearch.Value = null;
		this.btnSwitchUserSelector.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnSwitchUserSelector.FlatAppearance.BorderSize = 0;
		this.btnSwitchUserSelector.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSwitchUserSelector.Image = Leqisoft.UI.Platform.Properties.Resources.toolListMode;
		this.btnSwitchUserSelector.Location = new System.Drawing.Point(321, 3);
		this.btnSwitchUserSelector.Name = "btnSwitchUserSelector";
		this.btnSwitchUserSelector.Size = new System.Drawing.Size(24, 22);
		this.btnSwitchUserSelector.TabIndex = 1;
		this.btnSwitchUserSelector.UseVisualStyleBackColor = true;
		this.btnSwitchUserSelector.Click += new System.EventHandler(btnSwitchUserSelector_Click);
		this.c1InputPanel1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1InputPanel1.DesignScaleFactor = 1.293737f;
		this.c1InputPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1InputPanel1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9f);
		this.c1InputPanel1.Items.Add(this.inputGroupHeader2);
		this.c1InputPanel1.Location = new System.Drawing.Point(0, 0);
		this.c1InputPanel1.Margin = new System.Windows.Forms.Padding(0, 4, 3, 4);
		this.c1InputPanel1.Name = "c1InputPanel1";
		this.c1InputPanel1.Padding = new System.Windows.Forms.Padding(0, 2, 2, 2);
		this.c1InputPanel1.Size = new System.Drawing.Size(346, 40);
		this.c1InputPanel1.TabIndex = 0;
		this.inputGroupHeader2.Name = "inputGroupHeader2";
		this.inputGroupHeader2.Text = "选择成员";
		this.pnlEmpty.Controls.Add(this.c1InputPanel3);
		this.pnlEmpty.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlEmpty.Height = 16;
		this.pnlEmpty.Location = new System.Drawing.Point(500, 405);
		this.pnlEmpty.MinHeight = 5;
		this.pnlEmpty.Name = "pnlEmpty";
		this.pnlEmpty.Size = new System.Drawing.Size(346, 16);
		this.pnlEmpty.SizeRatio = 4.211;
		this.pnlEmpty.TabIndex = 4;
		this.c1InputPanel3.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1InputPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1InputPanel3.Font = new System.Drawing.Font("Microsoft YaHei UI", 9f);
		this.c1InputPanel3.Location = new System.Drawing.Point(0, 0);
		this.c1InputPanel3.Name = "c1InputPanel3";
		this.c1InputPanel3.Size = new System.Drawing.Size(346, 16);
		this.c1InputPanel3.TabIndex = 0;
		this.pnlUserSelector.BorderWidth = 1;
		this.pnlUserSelector.Height = 364;
		this.pnlUserSelector.Location = new System.Drawing.Point(501, 41);
		this.pnlUserSelector.MinHeight = 52;
		this.pnlUserSelector.MinWidth = 52;
		this.pnlUserSelector.Name = "pnlUserSelector";
		this.pnlUserSelector.Size = new System.Drawing.Size(344, 362);
		this.pnlUserSelector.SizeRatio = 97.872;
		this.pnlUserSelector.TabIndex = 2;
		this.pnlUserSelector.Width = 346;
		this.inputGroupHeader1.Name = "inputGroupHeader1";
		this.inputGroupHeader1.Text = "基本信息";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(846, 481);
		base.Controls.Add(this.ctnMain);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "dlgProjectEditor";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "dlgProjectInfo";
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		((System.ComponentModel.ISupportInitialize)this.inputPanel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnMain).EndInit();
		this.ctnMain.ResumeLayout(false);
		this.pnlButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnOk).EndInit();
		this.pnlProjectInfo.ResumeLayout(false);
		this.c1SplitterPanel1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel2).EndInit();
		this.pnlUserHeader.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txbSearch).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbSearch).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnSwitchUserSelector).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel1).EndInit();
		this.pnlEmpty.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1InputPanel3).EndInit();
		base.ResumeLayout(false);
	}
}
