﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1Sizer;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using Auditai.Util;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Platform;

public class FormProjectMembers
{
	private readonly C1RibbonForm _form;

	private readonly C1Sizer _szMain;

	private readonly C1Sizer _szTop;

	private readonly C1Sizer _szBottom;

	private readonly C1Button _btnToggleMode;

	private readonly C1CheckBox _ckbSearch;

	private readonly C1TextBox _txbSearch;

	private readonly C1Button _btnAddUser;

	private readonly C1Button _btnOk;

	private readonly C1Button _btnCancel;

	private readonly ProjectUsersListSelector projectUsersListSelector;

	private readonly ProjectUsersTileSelector projectUsersTileSelector;

	private static readonly Regex _rxSplit = new Regex("[,，]");

	public ProjectUsersSelectorContext Context { get; } = new ProjectUsersSelectorContext();


	public Auditai.DTO.Project Project { get; set; }

	private Auditai.UI.Platform.ListTileViewMode _Mode
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

	public FormProjectMembers()
	{
		_form = FormFactory.Create();
		_form.Text = "选择成员";
		_form.Size = new Size(900, 650);
		_form.ShowInTaskbar = false;
		_form.Load += _form_Load;
		_form.Shown += _form_Shown;
		_szMain = new C1Sizer
		{
			Dock = DockStyle.Fill,
			SplitterWidth = 0,
			Padding = Padding.Empty
		};
		_szMain.Grid.Rows.Count = 3;
		_szMain.Grid.Columns.Count = 1;
		_szMain.Grid.Rows.SetSizes(new int[3] { 24, 1, 50 });
		_szMain.Grid.Columns.SetSizes(new int[1] { 1 });
		_szMain.Grid.Rows.SetFixed(0, 2);
		_form.Controls.Add(_szMain);
		_szTop = new C1Sizer
		{
			SplitterWidth = 0,
			Padding = Padding.Empty
		};
		_szTop.Grid.Rows.Count = 1;
		_szTop.Grid.Rows.SetSizes(new int[1] { 1 });
		_szTop.Grid.Columns.Count = 4;
		_szTop.Grid.Columns.SetSizes(new int[4] { 26, 26, 4, 1 });
		_szTop.Grid.Columns.SetFixed(0, 1, 2);
		_szMain.AddControl(_szTop, 0, 0);
		_btnToggleMode = new C1Button
		{
			Image = IconRes.listMode16
		};
		_btnToggleMode.Click += _btnToggleMode_Click;
		_szTop.AddControl(_btnToggleMode, 0, 0);
		_ckbSearch = new C1CheckBox
		{
			BackColor = Color.Transparent,
			BackgroundImage = IconRes.ctxSearch,
			BackgroundImageLayout = ImageLayout.Zoom,
			Appearance = Appearance.Button,
			FlatStyle = FlatStyle.Flat
		};
		_ckbSearch.FlatAppearance.BorderSize = 0;
		_ckbSearch.CheckedChanged += _ckbSearch_CheckedChanged;
		_szTop.AddControl(_ckbSearch, 0, 1);
		_txbSearch = new C1TextBox
		{
			AutoSize = false,
			VerticalAlign = VerticalAlignEnum.Middle
		};
		_txbSearch.TextChanged += _txbSearch_TextChanged;
		_szTop.AddControl(_txbSearch, 0, 3);
		projectUsersListSelector = new ProjectUsersListSelector();
		projectUsersTileSelector = new ProjectUsersTileSelector();
		_szMain.AddControl(projectUsersListSelector.GetControl(), 1, 0);
		_szMain.AddControl(projectUsersTileSelector.GetControl(), 1, 0);
		projectUsersListSelector.GetControl().Dock = DockStyle.None;
		projectUsersTileSelector.GetControl().Dock = DockStyle.None;
		_szBottom = new C1Sizer
		{
			SplitterWidth = 0,
			Padding = Padding.Empty
		};
		_szBottom.Grid.Rows.Count = 3;
		_szBottom.Grid.Columns.Count = 5;
		_szBottom.Grid.Rows.SetSizes(new int[3] { 8, 1, 8 });
		_szBottom.Grid.Rows.SetFixed(0, 2);
		// 底部按钮区：弹性左间距 + 确定/取消两个 80px 按钮右对齐。
		// "增加同事"入口已迁移到团队管理界面，不再占用栅格槽位（避免出现空白按钮位）。
		_szBottom.Grid.Columns.SetSizes(new int[5] { 1, 80, 8, 80, 12 });
		_szBottom.Grid.Columns.SetFixed(1, 2, 3, 4);
		_szMain.AddControl(_szBottom, 2, 0);
		_btnAddUser = new C1Button
		{
			Text = "增加同事"
		};
		_btnAddUser.Click += _btnAddUser_Click;
		_btnOk = new C1Button
		{
			Text = "确定"
		};
		_btnOk.Click += _btnOk_Click;
		_szBottom.AddControl(_btnOk, 1, 1);
		_btnCancel = new C1Button
		{
			Text = "取消"
		};
		_btnCancel.Click += _btnCancel_Click;
		_szBottom.AddControl(_btnCancel, 1, 3);
	}

	public DialogResult ShowDialog()
	{
		return _form.ShowDialog();
	}

	private void _txbSearch_TextChanged(object sender, EventArgs e)
	{
		string[] array = _rxSplit.Split(_txbSearch.Text);
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

	private void _ckbSearch_CheckedChanged(object sender, EventArgs e)
	{
		_txbSearch.Visible = _ckbSearch.Checked;
	}

	private async void _btnAddUser_Click(object sender, EventArgs e)
	{
		Program.ManageUsers();
		await PopulateTeamUsers();
	}

	private async void _form_Load(object sender, EventArgs e)
	{
		Theme.SetCurrentTree(_form);
		_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.ProjectEditor);
		projectUsersTileSelector.SetTheme();
		projectUsersListSelector.SetTheme();
		// 顶部图标按钮：无边框透明底，悬停淡蓝反馈，与轻盈清新风格一致
		_btnToggleMode.FlatStyle = FlatStyle.Flat;
		_btnToggleMode.FlatAppearance.BorderSize = 0;
		_btnToggleMode.BackColor = Color.Transparent;
		_btnToggleMode.Cursor = Cursors.Hand;
		_btnToggleMode.FlatAppearance.MouseOverBackColor = AuditTheme.BrandSubtle;
		_btnToggleMode.FlatAppearance.MouseDownBackColor = AuditTheme.SurfaceHover;
		_ckbSearch.Cursor = Cursors.Hand;
		_ckbSearch.FlatAppearance.MouseOverBackColor = AuditTheme.BrandSubtle;
		_ckbSearch.FlatAppearance.CheckedBackColor = Theme.SelectedAuditaiTheme.ThemeContext.TileColor;
		_ckbSearch.BackColor = Color.Transparent;
		// 搜索框统一正文字号（24px 工具条行内垂直居中，不强制改高度）
		_txbSearch.Font = AuditTheme.FontBody;
		_ckbSearch.Checked = false;
		_txbSearch.Hide();
		ToMode(_Mode);
		_btnAddUser.Visible = false;
		await PopulateTeamUsers();
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		_form.DialogResult = DialogResult.Cancel;
	}

	private async void _btnOk_Click(object sender, EventArgs e)
	{
		if (await Validate())
		{
			_form.DialogResult = DialogResult.OK;
		}
	}

	private async Task<bool> Validate()
	{
		IEnumerable<Auditai.DTO.User> users = (IEnumerable<Auditai.DTO.User>)((_Mode == Auditai.UI.Platform.ListTileViewMode.Tile) ? projectUsersTileSelector.ValidateAndGetUsers() : projectUsersListSelector.ValidateAndGetUsers());
		if (users == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, StringConstBase.Current.Project + "成员为必填项，请设置并勾选！");
			return false;
		}
		if (users.Count() == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, StringConstBase.Current.Project + "成员为必填项，请设置并勾选！");
			return false;
		}
		if (!StorageRouter.IsLocalMode)
		{
			JObject jObject = new JObject();
			jObject.Add("TeamId", Auditai.Model.User.Current.TeamId);
			IEnumerable<Auditai.DTO.User> source = await WebApiClient.GetTeamUserPermissions(jObject);
			HashSet<long> userIds = new HashSet<long>(users.Select((Auditai.DTO.User u) => u.Id));
			List<Auditai.DTO.User> list = (from u in source
				where u.Permissions.MustInclude
				where !userIds.Contains(u.Id)
				select u).ToList();
			if (list.Count > 0)
			{
				string text = string.Join("、", list.Select((Auditai.DTO.User u) => u.Name));
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "根据系统管理员设置的权限规则，您必须选择" + text + "加入" + StringConstBase.Current.Project + "。");
				return false;
			}
		}
		Project.Users = users.ToList();
		return true;
	}

	private void _btnToggleMode_Click(object sender, EventArgs e)
	{
		switch (_Mode)
		{
		case Auditai.UI.Platform.ListTileViewMode.List:
			ToMode(Auditai.UI.Platform.ListTileViewMode.Tile);
			_Mode = Auditai.UI.Platform.ListTileViewMode.Tile;
			break;
		case Auditai.UI.Platform.ListTileViewMode.Tile:
			ToMode(Auditai.UI.Platform.ListTileViewMode.List);
			_Mode = Auditai.UI.Platform.ListTileViewMode.List;
			break;
		}
	}

	private void _form_Shown(object sender, EventArgs e)
	{
	}

	private async Task PopulateTeamUsers()
	{
		try
		{
			Auditai.Model.User current = Auditai.Model.User.Current;
			if (current.TeamId == Guid.Empty)
			{
				if (!StorageRouter.IsLocalMode)
				{
					await WebApiClient.GetUserById(current.Id);
				}
				// 本地模式同样下发 Context（选择器内部对空数据做保护，显示空白而非报错）
				projectUsersListSelector.Context = Context;
				projectUsersTileSelector.Context = Context;
				projectUsersListSelector.PopulateUsers();
				projectUsersTileSelector.PopulateUsers();
				return;
			}
			Context.Project = Project;
			Context.ManagerId = UserTeam.Current.ManagerId;
			if (!StorageRouter.IsLocalMode)
			{
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
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
		catch (TimeoutException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private void ToMode(ListTileViewMode mode)
	{
		switch (mode)
		{
		case ListTileViewMode.List:
			projectUsersListSelector.GetControl().BringToFront();
			_btnToggleMode.Image = IconRes.tileMode16;
			break;
		case ListTileViewMode.Tile:
			projectUsersTileSelector.GetControl().BringToFront();
			_btnToggleMode.Image = IconRes.listMode16;
			break;
		}
	}
}
