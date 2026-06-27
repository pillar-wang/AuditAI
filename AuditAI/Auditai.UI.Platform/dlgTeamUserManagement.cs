﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using C1.Win.C1Tile;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Model;
using Auditai.SignalR;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;
using Auditai.Util;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Platform;

public class dlgTeamUserManagement : C1RibbonForm
{
	private const string CN_INDEX = "CN_INDEX";

	private const string CN_USERNAME = "CN_USERNAME";

	private const string CN_NAME = "CN_NAME";

	private const string CN_GENDER = "CN_GENDER";

	private const string CN_PHONE = "CN_PHONE";

	private const string CN_JOBTITLE = "CN_JOBTITLE";

	private const string CN_PERMISSIONS = "CN_PERMISSIONS";

	private readonly bool _isAdmin;

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private C1ContextMenu userContextMenu = new C1ContextMenu();

	private C1CommandLink lnkAddUserGroup1 = new C1CommandLink();

	private C1Command cmdAddUserGroup1 = new C1Command();

	private C1CommandLink lnkMoveUserGroup1 = new C1CommandLink();

	private C1Command cmdMoveUserGroup1 = new C1Command();

	private C1CommandLink lnkRemoveUser1 = new C1CommandLink();

	private C1Command cmdRemoveUser1 = new C1Command();

	private C1ContextMenu groupContextMenu = new C1ContextMenu();

	private C1CommandLink lnkAddUserGroup2 = new C1CommandLink();

	private C1Command cmdAddUserGroup2 = new C1Command();

	private C1CommandLink lnkAddChildGroup2 = new C1CommandLink();

	private C1Command cmdAddChildGroup2 = new C1Command();

	private C1CommandLink lnkDeleteUserGroup2 = new C1CommandLink();

	private C1Command cmdDeleteUserGroup2 = new C1Command();

	private C1CommandLink lnkRenameUserGroup2 = new C1CommandLink();

	private C1Command cmdRenameUserGroup2 = new C1Command();

	private readonly MemberManager _mm;

	private readonly ListDropDown _listDropDown;

	private TooltipManager tooltipManager = new TooltipManager();

	private C1TileControlEx _tileControl;

	private Template _userTemplate;

	private IContainer components;

	private C1SplitContainer c1SplitContainer1;

	private C1SplitterPanel c1SplitterPanel2;

	private C1FlexGrid _grid;

	private C1SplitterPanel pnlToolbar;

	private C1CommandHolder c1CommandHolder1;

	private C1CommandDock commandDock;

	private C1ToolBar toolbar;

	private C1CommandLink lnkToolAddTeamUser;

	private C1CommandLink toolLnkRemoveTeamUser;

	private C1CommandLink toolLnkAddUserGroup;

	private C1CommandLink toolLnkAddChildGroup;

	private C1CommandLink toolLnkRemoveUserGroup;

	private C1CommandLink toolLnkRenameUserGroup;

	private C1CommandLink toolLnkDisplayMode;

	private C1CommandLink toolLnkDismissTeam;

	private C1CommandLink toolLnkMergeTeam;

	private C1CommandLink toolLnkLeaveTeam;

	private C1Command toolCmdAddTeamUser;

	private C1Command toolCmdRemoveTeamUser;

	private C1Command toolCmdAddUserGroup;

	private C1Command toolCmdAddChildGroup;

	private C1Command toolCmdRemoveUserGroup;

	private C1Command toolCmdRenameUserGroup;

	private C1Command toolCmdDisplayMode;

	private C1Command toolCmdLeaveTeam;

	private C1Command toolCmdDismissTeam;

	private C1Command toolCmdMergeTeam;

	private C1CommandLink toolLnkRenameTeam;

	private C1Command toolCmdRenameTeam;

	private ListTileViewMode _Mode
	{
		get
		{
			return (Auditai.UI.Platform.ListTileViewMode)UserSet.Config.TeamUsersViewMode;
		}
		set
		{
			UserSet.Config.TeamUsersViewMode = (dynamic)value;
		}
	}

	internal dlgTeamUserManagement()
	{
		InitializeComponent();
		toolCmdRemoveTeamUser.Text = (Program.IsOnPremise ? "停用同事" : "移除同事");
		base.Load += DlgTeamUserManagement_Load;
		base.Shown += DlgTeamUserManagement_Shown;
		InitializeTileControl();
		c1SplitterPanel2.Controls.Add(_tileControl);
		base.Resize += DlgTeamUserManagement_Resize;
		_isAdmin = Auditai.Model.User.Current.IsTeamAdmin;
		_listDropDown = new ListDropDown(_grid);
		_listDropDown.SimpleCheckedList.Op = new ValueSetOperand(from v in UserTeamPermissions.GetValues()
			select Tuple.Create<Auditai.Model.Row, ValueOperand>(null, ValueOperand.FromObject(v)));
		_listDropDown.SimpleCheckedList.Populate();
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Count = 0;
		_grid.ExtendLastCol = true;
		_grid.AllowEditing = _isAdmin;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		_grid.Cols.Fixed = 1;
		column.Name = "CN_INDEX";
		column.Caption = "序号";
		column = _grid.Cols.Add();
		column.Name = "CN_USERNAME";
		column.Caption = "用户名";
		column.TextAlign = TextAlignEnum.LeftCenter;
		column.AllowEditing = false;
		column = _grid.Cols.Add();
		column.Name = "CN_NAME";
		column.Caption = "姓名";
		column.TextAlign = TextAlignEnum.LeftCenter;
		column.AllowEditing = false;
		column = _grid.Cols.Add();
		column.Name = "CN_GENDER";
		column.Caption = "性别";
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.AllowEditing = false;
		column = _grid.Cols.Add();
		column.Name = "CN_PHONE";
		column.Caption = "手机";
		column.TextAlign = TextAlignEnum.LeftCenter;
		column.AllowEditing = false;
		column = _grid.Cols.Add();
		column.Name = "CN_JOBTITLE";
		column.Caption = "职务";
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.AllowEditing = _isAdmin;
		column.Editor = _listDropDown.DropDown;
		column = _grid.Cols.Add();
		column.Name = "CN_PERMISSIONS";
		column.Caption = "权限选项";
		column.AllowEditing = _isAdmin;
		column.Editor = _listDropDown.DropDown;
		_grid.Tree.Column = 1;
		_grid.Rows.DefaultSize = 40;
		_grid.DrawMode = DrawModeEnum.OwnerDraw;
		_grid.SelectionMode = SelectionModeEnum.Row;
		_grid.AllowAddNew = false;
		_grid.AllowDelete = false;
		_grid.AllowDragging = AllowDraggingEnum.None;
		_grid.AllowFiltering = false;
		_grid.AllowFreezing = AllowFreezingEnum.None;
		_grid.AllowResizing = AllowResizingEnum.Both;
		_grid.AllowSorting = AllowSortingEnum.None;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.Paint += _grid_Paint;
		_grid.AfterEdit += _grid_AfterEdit;
		_grid.MouseDown += _grid_MouseDown;
		_grid.MouseClick += _grid_MouseClick;
		_grid.RowColChange += _grid_RowColChange;
		_grid.SetupEditor += _grid_SetupEditor;
		_grid.ValidateEdit += _grid_ValidateEdit;
		cmdAddUserGroup1.Text = "新建分组";
		cmdAddUserGroup1.Click += CmdAddUserGroup1_Click;
		cmdAddUserGroup1.CommandStateQuery += CmdAddUserGroup1_CommandStateQuery;
		lnkAddUserGroup1.Command = cmdAddUserGroup1;
		userContextMenu.CommandLinks.Add(lnkAddUserGroup1);
		cmdMoveUserGroup1.Text = "分组调至";
		cmdMoveUserGroup1.Click += CmdMoveUserGroup1_Click;
		cmdMoveUserGroup1.CommandStateQuery += CmdMoveUserGroup1_CommandStateQuery;
		lnkMoveUserGroup1.Command = cmdMoveUserGroup1;
		userContextMenu.CommandLinks.Add(lnkMoveUserGroup1);
		cmdRemoveUser1.Text = (Program.IsOnPremise ? "停用同事" : "移除同事");
		cmdRemoveUser1.Click += CmdRemoveUser1_Click;
		cmdRemoveUser1.CommandStateQuery += CmdRemoveUser1_CommandStateQuery;
		lnkRemoveUser1.Command = cmdRemoveUser1;
		userContextMenu.CommandLinks.Add(lnkRemoveUser1);
		cmdAddUserGroup2.Text = "新建分组";
		cmdAddUserGroup2.Click += CmdAddUserGroup2_Click;
		cmdAddUserGroup2.CommandStateQuery += CmdAddUserGroup2_CommandStateQuery;
		lnkAddUserGroup2.Command = cmdAddUserGroup2;
		groupContextMenu.CommandLinks.Add(lnkAddUserGroup2);
		cmdAddChildGroup2.Text = "下增分组";
		cmdAddChildGroup2.Click += CmdAddChildGroup2_Click;
		cmdAddChildGroup2.CommandStateQuery += CmdAddChildGroup2_CommandStateQuery;
		lnkAddChildGroup2.Command = cmdAddChildGroup2;
		groupContextMenu.CommandLinks.Add(lnkAddChildGroup2);
		cmdDeleteUserGroup2.Text = "删除分组";
		cmdDeleteUserGroup2.Click += CmdDeleteUserGroup2_Click;
		cmdDeleteUserGroup2.CommandStateQuery += CmdDeleteUserGroup2_CommandStateQuery;
		lnkDeleteUserGroup2.Command = cmdDeleteUserGroup2;
		groupContextMenu.CommandLinks.Add(lnkDeleteUserGroup2);
		cmdRenameUserGroup2.Text = "重命名分组";
		cmdRenameUserGroup2.Click += CmdRenameUserGroup2_Click;
		cmdRenameUserGroup2.CommandStateQuery += CmdRenameUserGroup2_CommandStateQuery;
		lnkRenameUserGroup2.Command = cmdRenameUserGroup2;
		groupContextMenu.CommandLinks.Add(lnkRenameUserGroup2);
		foreach (C1CommandLink commandLink in toolbar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		AttachTooltip();
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		Auditai.UI.Controls.Theme.SetCurrentObject(_listDropDown.DropDown);
		SetTheme();
		_mm = MemberManager.GetInstance();
	}

	private async void DlgTeamUserManagement_Load(object sender, EventArgs e)
	{
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Auditai.UI.Platform.Properties.Resources.Users);
		Text = (Auditai.Model.User.Current.IsTeamAdmin ? "同事管理" : "我的同事") + " 【" + UserTeam.Current.Name + "】";
		toolLnkDisplayMode.Delimiter = Auditai.Model.User.Current.IsTeamAdmin;
		await Populate();
	}

	private void DlgTeamUserManagement_Shown(object sender, EventArgs e)
	{
	}

	private void SetTheme()
	{
		_tileControl.TileBorderColor = Color.Transparent;
		_tileControl.CustomBorderColor = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
		if (Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
			imageProcess.ProcessImage();
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
			imageProcess.ProcessImage();
		}
		_grid.Styles.SelectedColumnHeader.Clear();
	}

	private void CmdAddUserGroup1_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
	}

	private void CmdMoveUserGroup1_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = currentRow();
		Auditai.DTO.User user = row?.UserData as Auditai.DTO.User;
		if (!(e.Visible = user != null && _isAdmin))
		{
			return;
		}
		C1ContextMenu c1ContextMenu = new C1ContextMenu();
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "(空组)";
		c1Command.Click += commandClick;
		c1CommandLink.Command = c1Command;
		c1ContextMenu.CommandLinks.Add(c1CommandLink);
		Node[] nodes = _grid.Nodes;
		foreach (Node node in nodes)
		{
			if (node.Key is UserGroup userGroup3)
			{
				addUserGroup(c1ContextMenu, userGroup3);
			}
		}
		c1ContextMenu.Text = "分组调至";
		c1ContextMenu.CommandStateQuery += CmdMoveUserGroup1_CommandStateQuery;
		lnkMoveUserGroup1.Command = c1ContextMenu;
		void addUserGroup(C1ContextMenu contextMenu2, UserGroup userGroup2)
		{
			C1CommandLink c1CommandLink2 = new C1CommandLink();
			C1Command c1Command2 = new C1Command();
			c1Command2.UserData = userGroup2;
			c1Command2.Text = fullName(userGroup2);
			c1Command2.Click += commandClick;
			c1CommandLink2.Command = c1Command2;
			contextMenu2.CommandLinks.Add(c1CommandLink2);
			foreach (UserGroup child in userGroup2.Children)
			{
				C1CommandLink c1CommandLink3 = new C1CommandLink();
				C1Command c1Command3 = new C1Command();
				c1Command3.UserData = child;
				c1Command3.Text = fullName(child);
				c1Command3.Click += commandClick;
				c1CommandLink3.Command = c1Command3;
				contextMenu2.CommandLinks.Add(c1CommandLink3);
			}
		}
		async void commandClick(object s2, ClickEventArgs e2)
		{
			try
			{
				if (s2 is C1Command { UserData: var userData })
				{
					if (userData is UserGroup userGroup5)
					{
						await WebApiClient.MoveUserToGroup(user.Id.ToString(), userGroup5.Id.ToString());
						if (user.UserGroup != null)
						{
							user.UserGroup.Users.Remove(user);
						}
						user.UserGroup = userGroup5;
						userGroup5.Users.Add(user);
					}
					else
					{
						await WebApiClient.MoveUserToGroup(user.Id.ToString(), null);
						if (user.UserGroup != null)
						{
							user.UserGroup.Users.Remove(user);
						}
						user.UserGroup = null;
					}
				}
				else if (s2 is C1ContextMenu { UserData: var userData2 })
				{
					if (userData2 is UserGroup userGroup4)
					{
						await WebApiClient.MoveUserToGroup(user.Id.ToString(), userGroup4.Id.ToString());
						if (user.UserGroup != null)
						{
							user.UserGroup.Users.Remove(user);
						}
						user.UserGroup = userGroup4;
						userGroup4.Users.Add(user);
					}
					else
					{
						await WebApiClient.MoveUserToGroup(user.Id.ToString(), null);
						if (user.UserGroup != null)
						{
							user.UserGroup.Users.Remove(user);
						}
						user.UserGroup = null;
					}
				}
				if (user.UserGroup == null)
				{
					Node node2 = _grid.Rows.AddNode(0);
					node2.Row.UserData = user;
					node2.Row["CN_USERNAME"] = user.UserName;
					System.Drawing.Image cellImage = _grid.GetCellImage(row.Index, "CN_USERNAME");
					_grid.SetCellImage(node2.Row.Index, "CN_USERNAME", cellImage);
					node2.Row["CN_GENDER"] = ((user.Sex == "f") ? "女" : "男");
					node2.Row["CN_JOBTITLE"] = user.JobTitle;
					node2.Row["CN_PHONE"] = user.Phone;
					node2.Row["CN_NAME"] = user.Name;
					_grid.Rows.Remove(row);
				}
				else
				{
					for (int j = 0; j < _grid.Rows.Count; j++)
					{
						C1.Win.C1FlexGrid.Row row2 = _grid.Rows[j];
						if (row2.UserData is UserGroup userGroup6 && userGroup6 == user.UserGroup)
						{
							Node node3 = row2.Node.AddNode(NodeTypeEnum.LastChild, string.Empty);
							node3.Row.UserData = user;
							node3.Row["CN_USERNAME"] = user.UserName;
							System.Drawing.Image cellImage2 = _grid.GetCellImage(row.Index, "CN_USERNAME");
							_grid.SetCellImage(node3.Row.Index, "CN_USERNAME", cellImage2);
							node3.Row["CN_GENDER"] = ((user.Sex == "f") ? "女" : "男");
							node3.Row["CN_JOBTITLE"] = user.JobTitle;
							node3.Row["CN_PHONE"] = user.Phone;
							node3.Row["CN_NAME"] = user.Name;
							_grid.Rows.Remove(row);
							break;
						}
					}
				}
			}
			catch (HttpRequestException ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
			}
			catch (Exception ex2)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			}
		}
		static string fullName(UserGroup userGroup)
		{
			List<string> list = new List<string> { userGroup.Name };
			for (UserGroup parentGroup = userGroup.ParentGroup; parentGroup != null; parentGroup = parentGroup.ParentGroup)
			{
				list.Add(parentGroup.Name);
			}
			list.Reverse();
			return string.Join("-", list);
		}
	}

	private void CmdRemoveUser1_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
	}

	private void CmdAddUserGroup2_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
	}

	private void CmdAddChildGroup2_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin && currentRow()?.UserData is UserGroup;
	}

	private void CmdDeleteUserGroup2_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin && currentRow()?.UserData is UserGroup userGroup && userGroup.DescendantsUsers().Count == 0;
	}

	private void CmdRenameUserGroup2_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
	}

	private C1.Win.C1FlexGrid.Row currentRow()
	{
		int row = _grid.Row;
		if (row >= 0 && row < _grid.Rows.Count)
		{
			return _grid.Rows[row];
		}
		return null;
	}

	internal async Task<bool> Populate()
	{
		if (_Mode == ListTileViewMode.Tile)
		{
			_tileControl.BringToFront();
			return await PopulateUserTile();
		}
		_grid.BringToFront();
		return await PopulateUserList();
	}

	internal async Task<bool> PopulateUserList()
	{
		List<Member> members = _mm.GetMembers().ToList();
		bool success = false;
		_ = Auditai.Model.User.Current;
		long managerId = UserTeam.Current.ManagerId;
		try
		{
			if (StorageRouter.IsLocalMode)
			{
				var users = (await StorageRouter.GetTeamUsersWithPic()).ToList();
				_grid.Rows.Count = _grid.Rows.Fixed;
				foreach (Auditai.DTO.User user in users.OrderByDescending((Auditai.DTO.User u) => u.Id == managerId))
				{
					AppendUserNode(user, null);
				}
				SetViewState();
				success = true;
			}
			else
			{
				Tuple<List<Auditai.DTO.User>, List<UserGroup>> tuple = await WebApiClient.GetTeamUserGroups();
				_grid.Rows.Count = _grid.Rows.Fixed;
				foreach (UserGroup item in tuple.Item2)
				{
					AppendGroupNode(item, null);
				}
				foreach (Auditai.DTO.User item2 in tuple.Item1.OrderByDescending((Auditai.DTO.User u) => u.Id == managerId))
				{
					AppendUserNode(item2, null);
				}
				SetViewState();
				success = true;
			}
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (Exception ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		return success;
		void AppendGroupNode(UserGroup userGroup, Node parentNode)
		{
			C1.Win.C1FlexGrid.Row row2 = ((parentNode == null) ? _grid.Rows.AddNode(0).Row : parentNode.AddNode(NodeTypeEnum.LastChild, string.Empty).Row);
			row2.UserData = userGroup;
			row2["CN_USERNAME"] = userGroup.Name;
			_grid.SetCellImage(row2.Index, "CN_USERNAME", Auditai.UI.Platform.Properties.Resources.imgUserGroup);
			row2.AllowEditing = false;
			foreach (UserGroup child in userGroup.Children)
			{
				AppendGroupNode(child, row2.Node);
			}
			foreach (Auditai.DTO.User user in userGroup.Users)
			{
				AppendUserNode(user, row2.Node);
			}
		}
		void AppendUserNode(Auditai.DTO.User user, Node parentNode)
		{
			C1.Win.C1FlexGrid.Row row = ((parentNode == null) ? _grid.Rows.AddNode(0).Row : parentNode.AddNode(NodeTypeEnum.LastChild, string.Empty).Row);
			row.UserData = user;
			bool flag = members.FirstOrDefault((Member m) => m.Id == user.Id.ToString())?.IsOnline ?? false;
			System.Drawing.Image image = Auditai.UI.Controls.Util.GetHeadPic(user, 32, withManagerMark: true);
			if (!flag)
			{
				image = ((Bitmap)image).ToGray();
			}
			row["CN_USERNAME"] = user.UserName;
			_grid.SetCellImage(row.Index, "CN_USERNAME", image);
			row["CN_NAME"] = user.Name;
			row["CN_GENDER"] = ((user.Sex == "f") ? "女" : "男");
			row["CN_JOBTITLE"] = user.JobTitle;
			row["CN_PHONE"] = user.Phone;
			row["CN_PERMISSIONS"] = user.Permissions.GetDisplay();
		}
	}

	internal async Task<bool> PopulateUserTile()
	{
		List<Member> members = _mm.GetMembers().ToList();
		bool success = false;
		_ = Auditai.Model.User.Current;
		_tileControl.BringToFront();
		_tileControl.Groups.Clear();
		try
		{
			C1.Win.C1Tile.Group group = new C1.Win.C1Tile.Group();
			IEnumerable<Auditai.DTO.User> users = StorageRouter.IsLocalMode 
				? await StorageRouter.GetTeamUsersWithPic() 
				: await WebApiClient.GetTeamUsersWithPic();
			foreach (Auditai.DTO.User item in users.OrderByDescending((Auditai.DTO.User u) => u.IsTeamAdmin))
			{
				group.Tiles.Add(createUserTile(item));
			}
			_tileControl.Groups.Add(group);
			success = true;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		return success;
		Tile createUserTile(Auditai.DTO.User user)
		{
			bool flag = members.FirstOrDefault((Member m) => m.Id == user.Id.ToString())?.IsOnline ?? false;
			System.Drawing.Image image = Auditai.UI.Controls.Util.GetHeadPic(user, 32, withManagerMark: true);
			if (!flag)
			{
				image = ((Bitmap)image).ToGray();
			}
			return new Tile
			{
				Template = _userTemplate,
				Image1 = image,
				Text1 = "(" + user.UserName + ")",
				Text2 = user.Name,
				Tag = user,
				ForeColor1 = (flag ? Color.Black : Color.Gray)
			};
		}
	}

	private void DlgTeamUserManagement_Resize(object sender, EventArgs e)
	{
		if (_grid.Cols.Count > 1)
		{
			_grid.AutoSizeCols(0, 0, 10);
			int num = _grid.Cols[0].Width;
			int num2 = (base.Width - num) / (_grid.Cols.Count - 1);
			for (int i = 1; i < _grid.Cols.Count; i++)
			{
				_grid.Cols[i].Width = num2 - 1;
			}
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Row > 0 && e.Col == 0)
		{
			e.Text = e.Row.ToString();
		}
	}

	private void _grid_RowColChange(object sender, EventArgs e)
	{
		SetViewState();
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		_grid.DrawFormBorder(e.Graphics);
	}

	private void _grid_SetupEditor(object sender, RowColEventArgs e)
	{
		if (e.Col == _grid.Cols.IndexOf("CN_JOBTITLE"))
		{
			_listDropDown.ViewKind = DropDownViewKind.SimpleList;
			_listDropDown.SkipTextChanged = false;
			UpdateJobTitleComboList();
		}
		else if (e.Col == _grid.Cols.IndexOf("CN_PERMISSIONS"))
		{
			_listDropDown.ViewKind = DropDownViewKind.SimpleCheckList;
			_listDropDown.SkipTextChanged = false;
			_listDropDown.SimpleCheckedList.SetInitValue(_grid.GetData(e.Row, e.Col) as string);
		}
	}

	private void _grid_ValidateEdit(object sender, ValidateEditEventArgs e)
	{
		_listDropDown.SkipTextChanged = true;
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (_grid.Row < 0)
		{
			return;
		}
		if (e.Button == MouseButtons.Right)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[_grid.Row];
			HitTestTypeEnum type = _grid.HitTest(e.Location).Type;
			if (type == HitTestTypeEnum.Cell)
			{
				if (row.UserData is Auditai.DTO.User)
				{
					userContextMenu.ShowContextMenu(_grid, e.Location);
				}
				else if (row.UserData is UserGroup)
				{
					groupContextMenu.ShowContextMenu(_grid, e.Location);
				}
			}
		}
		else if (e.Button == MouseButtons.Left)
		{
			HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
			HitTestTypeEnum type2 = hitTestInfo.Type;
			if (type2 == HitTestTypeEnum.Cell && hitTestInfo.Column == _grid.Cols.IndexOf("CN_USERNAME") && hitTestInfo.Row >= _grid.Rows.Fixed)
			{
				_grid.Rows[hitTestInfo.Row].Node.Expanded = !_grid.Rows[hitTestInfo.Row].Node.Expanded;
			}
		}
	}

	private async void _grid_AfterEdit(object sender, RowColEventArgs e)
	{
		if (!_isAdmin || e.Row < _grid.Rows.Fixed || !(_grid.Rows[e.Row].UserData is Auditai.DTO.User user))
		{
			return;
		}
		if (e.Col == _grid.Cols.IndexOf("CN_JOBTITLE"))
		{
			string text = _grid.Rows[e.Row]["CN_JOBTITLE"]?.ToString();
			if (text == null)
			{
				return;
			}
			if (text.Length <= 20)
			{
				try
				{
					user.JobTitle = text;
					await WebApiClient.UpdateJobTitle(user.Id, text);
					return;
				}
				catch (HttpRequestException ex)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
					return;
				}
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "职务名称不能超过20个字符");
		}
		else if (e.Col == _grid.Cols.IndexOf("CN_PERMISSIONS"))
		{
			string text2 = ((string)_grid[e.Row, e.Col]) ?? "";
			user.Permissions = UserTeamPermissions.Parse(text2);
			await WebApiClient.SetUserTeamPermissions(new Auditai.DTO.User
			{
				Id = user.Id,
				TeamId = Auditai.Model.User.Current.TeamId,
				Permissions = user.Permissions
			});
		}
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		switch (hitTestInfo.Type)
		{
		case HitTestTypeEnum.ColumnHeader:
			if (_grid.Rows.Count > _grid.Rows.Fixed)
			{
				_grid.Select(new C1.Win.C1FlexGrid.CellRange
				{
					r1 = _grid.Rows.Fixed,
					r2 = _grid.Rows.Count - 1,
					c1 = hitTestInfo.Column,
					c2 = hitTestInfo.Column
				});
			}
			break;
		case HitTestTypeEnum.RowHeader:
			if (_grid.Cols.Count > _grid.Cols.Fixed)
			{
				_grid.Select(new C1.Win.C1FlexGrid.CellRange
				{
					r1 = hitTestInfo.Row,
					r2 = hitTestInfo.Row,
					c1 = _grid.Cols.Fixed,
					c2 = _grid.Cols.Count - 1
				});
			}
			break;
		case HitTestTypeEnum.Cell:
			if (!_grid.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
			{
				_grid.Select(hitTestInfo.Row, hitTestInfo.Column);
			}
			break;
		case HitTestTypeEnum.ColumnResize:
		case HitTestTypeEnum.ColumnFreeze:
			break;
		}
	}

	public void AttachTooltip()
	{
		TipInfo tip = TipInfo.Parse(TipResource.同事管理界面);
		_grid.MouseMove += delegate(object s1, MouseEventArgs e1)
		{
			if (tooltipManager.ShouldDisplay)
			{
				tooltipManager.Show(tip, _grid, e1.X, e1.Y);
			}
		};
		_grid.MouseLeave += delegate
		{
			tooltipManager.Hide();
		};
	}

	private void SetViewState()
	{
		if (_grid.Row >= _grid.Rows.Fixed && _grid.Row < _grid.Rows.Count && _grid.Rows[_grid.Row].UserData is Auditai.DTO.User user)
		{
			_ = user.Id;
			_ = Auditai.Model.User.Current.Id;
		}
	}

	private void UpdateJobTitleComboList()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)_grid.Rows)
		{
			if (item.UserData is Auditai.DTO.User user && !string.IsNullOrWhiteSpace(user.JobTitle))
			{
				hashSet.Add(user.JobTitle);
			}
		}
		ValueSetOperand op = new ValueSetOperand(hashSet.Select((string j) => Tuple.Create<Auditai.Model.Row, ValueOperand>(null, ValueOperand.FromObject(j))));
		_listDropDown.SimpleList.Op = op;
		_listDropDown.SimpleList.Populate();
	}

	private async void CmdAddUserGroup1_Click(object sender, ClickEventArgs e)
	{
		await AppendGroupImpl();
	}

	private void CmdMoveUserGroup1_Click(object sender, ClickEventArgs e)
	{
	}

	private async void CmdRemoveUser1_Click(object sender, ClickEventArgs e)
	{
		await RemoveUserFromTeamImpl();
	}

	private async void CmdAddUserGroup2_Click(object sender, ClickEventArgs e)
	{
		await AppendGroupImpl();
	}

	private async void CmdAddChildGroup2_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = currentRow();
		if (row?.UserData is UserGroup)
		{
			await AddChildGroupImpl(row.Node);
		}
		else
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选中节点不是用户组！无法进行该操作");
		}
	}

	private async void CmdDeleteUserGroup2_Click(object sender, ClickEventArgs e)
	{
		if (currentRow()?.UserData is UserGroup)
		{
			await DeleteUserGroup();
		}
		else
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选中节点不是用户组！无法进行该操作");
		}
	}

	private async void CmdRenameUserGroup2_Click(object sender, ClickEventArgs e)
	{
		if (currentRow()?.UserData is UserGroup)
		{
			await RenameUserGroupImpl();
		}
		else
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选中节点不是用户组！无法进行该操作");
		}
	}

	private async void cmdToolAddTeamUser_Click(object sender, ClickEventArgs e)
	{
		await AddUserToTeamImpl();
	}

	private async void cmdToolRemoveTeamUser_Click(object sender, ClickEventArgs e)
	{
		await RemoveUserFromTeamImpl();
	}

	private async void cmdToolAddUserGroup_Click(object sender, ClickEventArgs e)
	{
		await AppendGroupImpl();
	}

	private async void cmdToolAddChildGroup_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = currentRow();
		if (row?.UserData is UserGroup)
		{
			await AddChildGroupImpl(row.Node);
		}
		else
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选中节点不是用户组！无法进行该操作");
		}
	}

	private async void cmdToolRemoveUserGroup_Click(object sender, ClickEventArgs e)
	{
		if (currentRow()?.UserData is UserGroup)
		{
			await DeleteUserGroup();
		}
		else
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选中节点不是用户组！无法进行该操作");
		}
	}

	private async void cmdToolRenameUserGroup_Click(object sender, ClickEventArgs e)
	{
		if (currentRow()?.UserData is UserGroup)
		{
			await RenameUserGroupImpl();
		}
		else
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选中节点不是用户组！无法进行该操作");
		}
	}

	private async void cmdToolLeaveTeam_Click(object sender, ClickEventArgs e)
	{
		await LeaveTeamImpl();
	}

	private async void cmdToolDismissTeam_Click(object sender, ClickEventArgs e)
	{
		await DismissTeamImpl();
	}

	private async Task AddUserToTeamImpl()
	{
		string text = InputForm.Text("新增同事", "请输入同事用户名或手机号：");
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		try
		{
			Auditai.DTO.User ret = await WebApiClient.AddUserToTeam(text);
			ret.TeamId = Auditai.Model.User.Current.TeamId;
			await Populate();
			if (ret != null)
			{
				await SignalRClient.ChangeTeamMember(ret.Id.ToString(), Guid.Empty.ToString(), Auditai.Model.User.Current.TeamId.ToString());
			}
			SetViewState();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
	}

	private async Task RemoveUserFromTeamImpl()
	{
		toolCmdRemoveTeamUser.Enabled = false;
		try
		{
			Auditai.Model.User current = Auditai.Model.User.Current;
			if (!(((_Mode != ListTileViewMode.Tile) ? currentRow()?.UserData : _tileControl.SelectedTile?.Tag) is Auditai.DTO.User user))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择移除的同事");
			}
			else if (user.Id == current.Id)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "不能移除自己。");
			}
			else if (await Program.ExitTeam(user.UserName, current.TeamId))
			{
				await Populate();
			}
		}
		finally
		{
			toolCmdRemoveTeamUser.Enabled = true;
		}
	}

	private async Task AppendGroupImpl()
	{
		string groupName = InputForm.Text("新建分组", "请输入新建分组名称：");
		if (string.IsNullOrWhiteSpace(groupName))
		{
			return;
		}
		try
		{
			if (groupName.Length > 20)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "分组名称不能超过20个字符");
				return;
			}
			long id = await WebApiClient.AddUserGroup(groupName, 0L);
			Node node = _grid.Nodes.LastOrDefault((Node n) => n.Key is UserGroup)?.AddNode(NodeTypeEnum.NextSibling, string.Empty) ?? _grid.Rows.AddNode(0);
			_grid.SetCellImage(node.Row.Index, "CN_USERNAME", Auditai.UI.Platform.Properties.Resources.imgUserGroup);
			node.Row["CN_USERNAME"] = groupName;
			node.Row.UserData = new UserGroup
			{
				Id = id,
				Name = groupName,
				ParentId = null
			};
			node.Row.AllowEditing = false;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
	}

	private async Task AddChildGroupImpl(Node parentNode)
	{
		object obj = parentNode?.Key;
		if (!(obj is UserGroup userGroup))
		{
			return;
		}
		string groupName = InputForm.Text("下增分组", "请输入下增分组名称：");
		if (string.IsNullOrWhiteSpace(groupName))
		{
			return;
		}
		if (groupName.Length > 20)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "分组名称不能超过20个字符");
			return;
		}
		try
		{
			long id = await WebApiClient.AddUserGroup(groupName, userGroup.Id);
			Node node = parentNode.AddNode(NodeTypeEnum.FirstChild, string.Empty);
			_grid.SetCellImage(node.Row.Index, "CN_USERNAME", Auditai.UI.Platform.Properties.Resources.imgUserGroup);
			node.Row["CN_USERNAME"] = groupName;
			UserGroup userGroup2 = new UserGroup
			{
				Id = id,
				Name = groupName,
				ParentId = userGroup.Id,
				ParentGroup = userGroup
			};
			userGroup.Children.Add(userGroup2);
			node.Row.UserData = userGroup2;
			node.Row.AllowEditing = false;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
	}

	private async Task DeleteUserGroup()
	{
		object obj = currentRow()?.UserData;
		if (!(obj is UserGroup userGroup))
		{
			return;
		}
		if (userGroup.DescendantsUsers().Count > 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "分组中有人员，无法删除");
			return;
		}
		_grid.BeginUpdate();
		try
		{
			await WebApiClient.DeleteUserGroup(userGroup.Id);
			if (userGroup.ParentGroup != null)
			{
				userGroup.ParentGroup.Children.Remove(userGroup);
			}
			List<UserGroup> source = userGroup.DescendantsAndSelfGroup();
			HashSet<long> hashSet = new HashSet<long>(source.Select((UserGroup g) => g.Id));
			for (int num = _grid.Rows.Count - 1; num >= 0; num--)
			{
				C1.Win.C1FlexGrid.Row row = _grid.Rows[num];
				if (row.UserData is UserGroup userGroup2 && hashSet.Contains(userGroup2.Id))
				{
					_grid.Rows.Remove(num);
				}
			}
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	private async Task RenameUserGroupImpl()
	{
		C1.Win.C1FlexGrid.Row row = currentRow();
		object obj = row?.UserData;
		if (!(obj is UserGroup userGroup))
		{
			return;
		}
		string groupName = InputForm.Text("修改分组名称", "请输入修改分组名称：", userGroup.Name);
		if (string.IsNullOrWhiteSpace(groupName) || !(groupName != userGroup.Name))
		{
			return;
		}
		if (groupName.Length > 20)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "分组名称不能超过20个字符");
			return;
		}
		try
		{
			await WebApiClient.RenameUserGroup(userGroup.Id, groupName);
			row["CN_USERNAME"] = groupName;
			userGroup.Name = groupName;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
	}

	private async Task LeaveTeamImpl()
	{
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "确定要退出组织吗？", MessageBoxButtons.OKCancel) == DialogResult.OK)
		{
			Auditai.Model.User current = Auditai.Model.User.Current;
			if (await Program.ExitTeam(current.UserName, current.TeamId))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "成功退出组织");
				Close();
			}
		}
	}

	private async Task DismissTeamImpl()
	{
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "警告：此次操作将会解散组织，组织的所有" + StringConstBase.Current.Project + "和模板将会无法使用，请谨慎确认后再操作！\n\n若确认要解散组织，点击“确定”。", MessageBoxButtons.OKCancel) == DialogResult.OK)
		{
			string text = InputForm.Text("解散验证", "请输入您的用户名以防止是在您的误操作下解散组织：", null, 256);
			if (text != Auditai.Model.User.Current.UserName)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "输入用户名不正确，终止解散组织！");
			}
			else if (await Program.DismissTeam())
			{
				await Program.GetUserTeams(withNotice: false);
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Asterisk, "组织已被解散。");
				Close();
			}
		}
	}

	private async Task MergeTeamRequest()
	{
		FormMergeTeam formMergeTeam = new FormMergeTeam();
		if (formMergeTeam.ShowDialog() == DialogResult.OK)
		{
			try
			{
				JObject jObject = new JObject();
				jObject.Add("Username", formMergeTeam.Username);
				jObject.Add("Password", Encrypts.SHA256Encrypt(formMergeTeam.Password, isUrl: false));
				await WebApiClient.TeamMergeRequest(jObject);
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "合并组织成功。");
				await Populate();
			}
			catch (HttpRequestException ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
			}
		}
	}

	private async void cmdToolMergeTeam_Click(object sender, ClickEventArgs e)
	{
		await MergeTeamRequest();
	}

	private async void cmdToolDisplayMode_Click(object sender, ClickEventArgs e)
	{
		switch (_Mode)
		{
		case ListTileViewMode.List:
			_Mode = ListTileViewMode.Tile;
			break;
		case ListTileViewMode.Tile:
			_Mode = ListTileViewMode.List;
			break;
		}
		await Populate();
	}

	private void InitializeTileControl()
	{
		_tileControl = new C1TileControlEx
		{
			CellWidth = 120,
			CellHeight = 90,
			AllowChecking = false,
			Dock = DockStyle.Fill,
			CellSpacing = 20,
			Margin = new Padding(0),
			Padding = new Padding(0),
			GroupPadding = new Padding(0, 10, 0, 0),
			Orientation = LayoutOrientation.Vertical,
			TileBorderColor = Color.White,
			GroupSpacing = 5,
			ShowToolTips = false
		};
		_userTemplate = CreateUserTemplate();
		_tileControl.Templates.Add(_userTemplate);
		static Template CreateUserTemplate()
		{
			Template template = new Template();
			PanelElement panelElement = new PanelElement
			{
				Dock = DockStyle.Fill,
				AlignmentOfContents = ContentAlignment.MiddleCenter
			};
			C1.Win.C1Tile.ImageElement item = new C1.Win.C1Tile.ImageElement
			{
				Alignment = ContentAlignment.MiddleCenter,
				AlignmentOfContents = ContentAlignment.MiddleCenter,
				ImageSelector = ImageSelector.Image1
			};
			panelElement.Children.Add(item);
			PanelElement panelElement2 = new PanelElement
			{
				FixedHeight = 20,
				FixedWidth = 130,
				AlignmentOfContents = ContentAlignment.MiddleCenter,
				Dock = DockStyle.Bottom
			};
			C1.Win.C1Tile.TextElement item2 = new C1.Win.C1Tile.TextElement
			{
				TextTrimming = TextTrimming.EndEllipsis,
				SingleLine = false,
				Alignment = ContentAlignment.MiddleCenter,
				AlignmentOfContents = ContentAlignment.MiddleCenter,
				TextSelector = TextSelector.Text1,
				ForeColorSelector = ForeColorSelector.ForeColor1
			};
			panelElement2.Children.Add(item2);
			PanelElement panelElement3 = new PanelElement
			{
				FixedHeight = 20,
				FixedWidth = 130,
				AlignmentOfContents = ContentAlignment.MiddleCenter,
				Dock = DockStyle.Bottom
			};
			C1.Win.C1Tile.TextElement item3 = new C1.Win.C1Tile.TextElement
			{
				TextTrimming = TextTrimming.EndEllipsis,
				SingleLine = false,
				Alignment = ContentAlignment.MiddleCenter,
				AlignmentOfContents = ContentAlignment.MiddleCenter,
				TextSelector = TextSelector.Text2,
				ForeColorSelector = ForeColorSelector.ForeColor1
			};
			panelElement3.Children.Add(item3);
			template.Elements.Add(panelElement3);
			template.Elements.Add(panelElement2);
			template.Elements.Add(panelElement);
			return template;
		}
	}

	private void cmdToolDisplayMode_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		switch (_Mode)
		{
		case ListTileViewMode.List:
			toolCmdDisplayMode.Text = "磁贴模式";
			toolCmdDisplayMode.Image = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedBitmap(Auditai.UI.Platform.Properties.Resources.tileMode);
			break;
		case ListTileViewMode.Tile:
			toolCmdDisplayMode.Text = "列表模式";
			toolCmdDisplayMode.Image = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedBitmap(Auditai.UI.Platform.Properties.Resources.listMode);
			break;
		}
	}

	private void cmdToolAddTeamUser_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
	}

	private void cmdToolRemoveTeamUser_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
		switch (_Mode)
		{
		case ListTileViewMode.List:
			e.Enabled = currentRow()?.UserData is Auditai.DTO.User;
			break;
		case ListTileViewMode.Tile:
			e.Enabled = _tileControl.SelectedTile != null;
			break;
		}
	}

	private void cmdToolAddUserGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
		e.Enabled = _Mode == ListTileViewMode.List;
	}

	private void cmdToolAddChildGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
		e.Enabled = _Mode == ListTileViewMode.List && currentRow()?.UserData is UserGroup;
	}

	private void cmdToolRemoveUserGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
		e.Enabled = _Mode == ListTileViewMode.List && currentRow()?.UserData is UserGroup;
	}

	private void cmdToolRenameUserGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
		e.Enabled = _Mode == ListTileViewMode.List && currentRow()?.UserData is UserGroup;
	}

	private void cmdToolLeaveTeam_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = false;
	}

	private void cmdToolDismissTeam_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (Program.IsOnPremise)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = _isAdmin;
		}
	}

	private void cmdToolMergeTeam_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (Program.IsOnPremise)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = _isAdmin;
		}
	}

	private async void toolCmdRenameTeam_Click(object sender, ClickEventArgs e)
	{
		_ = Auditai.Model.User.Current;
		UserTeam userTeam = UserTeam.Current;
		string newTeamName = InputForm.Text("重命名组织", "请输入组织名称：", userTeam.Name, 256);
		if (string.IsNullOrWhiteSpace(newTeamName) || !(newTeamName != userTeam.Name))
		{
			return;
		}
		if (newTeamName.Length > 50)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "组织名称不能超过50个字符");
			return;
		}
		try
		{
			await WebApiClient.UpdateTeamName(newTeamName);
			userTeam.Name = newTeamName;
			Text = "同事管理 【" + newTeamName + "】";
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "组织名称修改成功！");
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
	}

	private void toolCmdRenameTeam_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _isAdmin;
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
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlToolbar = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.commandDock = new C1.Win.C1Command.C1CommandDock();
		this.toolbar = new C1.Win.C1Command.C1ToolBar();
		this.lnkToolAddTeamUser = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdAddTeamUser = new C1.Win.C1Command.C1Command();
		this.toolLnkRemoveTeamUser = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdRemoveTeamUser = new C1.Win.C1Command.C1Command();
		this.toolLnkAddUserGroup = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdAddUserGroup = new C1.Win.C1Command.C1Command();
		this.toolLnkAddChildGroup = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdAddChildGroup = new C1.Win.C1Command.C1Command();
		this.toolLnkRemoveUserGroup = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdRemoveUserGroup = new C1.Win.C1Command.C1Command();
		this.toolLnkRenameUserGroup = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdRenameUserGroup = new C1.Win.C1Command.C1Command();
		this.toolLnkDisplayMode = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdDisplayMode = new C1.Win.C1Command.C1Command();
		this.toolLnkRenameTeam = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdRenameTeam = new C1.Win.C1Command.C1Command();
		this.toolLnkMergeTeam = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdMergeTeam = new C1.Win.C1Command.C1Command();
		this.toolLnkDismissTeam = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdDismissTeam = new C1.Win.C1Command.C1Command();
		this.toolLnkLeaveTeam = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdLeaveTeam = new C1.Win.C1Command.C1Command();
		this.c1SplitterPanel2 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this._grid = new C1.Win.C1FlexGrid.C1FlexGrid();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.pnlToolbar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.commandDock).BeginInit();
		this.commandDock.SuspendLayout();
		this.c1SplitterPanel2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this._grid).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		base.SuspendLayout();
		this.c1SplitContainer1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1SplitContainer1.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.c1SplitContainer1.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.c1SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1SplitContainer1.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.c1SplitContainer1.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.c1SplitContainer1.HeaderHeight = 27;
		this.c1SplitContainer1.Location = new System.Drawing.Point(0, 0);
		this.c1SplitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.c1SplitContainer1.Name = "c1SplitContainer1";
		this.c1SplitContainer1.Panels.Add(this.pnlToolbar);
		this.c1SplitContainer1.Panels.Add(this.c1SplitterPanel2);
		this.c1SplitContainer1.Size = new System.Drawing.Size(892, 619);
		this.c1SplitContainer1.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.c1SplitContainer1.SplitterWidth = 0;
		this.c1SplitContainer1.TabIndex = 0;
		this.c1SplitContainer1.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlToolbar.Controls.Add(this.commandDock);
		this.pnlToolbar.Height = 66;
		this.pnlToolbar.KeepRelativeSize = false;
		this.pnlToolbar.Location = new System.Drawing.Point(0, 0);
		this.pnlToolbar.Name = "pnlToolbar";
		this.pnlToolbar.Resizable = false;
		this.pnlToolbar.Size = new System.Drawing.Size(892, 66);
		this.pnlToolbar.SizeRatio = 15.752;
		this.pnlToolbar.TabIndex = 2;
		this.commandDock.Controls.Add(this.toolbar);
		this.commandDock.Dock = System.Windows.Forms.DockStyle.Fill;
		this.commandDock.Id = 2;
		this.commandDock.Location = new System.Drawing.Point(0, 0);
		this.commandDock.Name = "commandDock";
		this.commandDock.Size = new System.Drawing.Size(892, 66);
		this.toolbar.AccessibleName = "Tool Bar";
		this.toolbar.AutoSize = false;
		this.toolbar.ButtonLayoutHorz = C1.Win.C1Command.ButtonLayoutEnum.TextBelow;
		this.toolbar.ButtonLookHorz = C1.Win.C1Command.ButtonLookFlags.TextAndImage;
		this.toolbar.CommandHolder = null;
		this.toolbar.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[11]
		{
			this.lnkToolAddTeamUser, this.toolLnkRemoveTeamUser, this.toolLnkAddUserGroup, this.toolLnkAddChildGroup, this.toolLnkRemoveUserGroup, this.toolLnkRenameUserGroup, this.toolLnkDisplayMode, this.toolLnkRenameTeam, this.toolLnkMergeTeam, this.toolLnkDismissTeam,
			this.toolLnkLeaveTeam
		});
		this.toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
		this.toolbar.Location = new System.Drawing.Point(0, 0);
		this.toolbar.MinButtonSize = 42;
		this.toolbar.Movable = false;
		this.toolbar.Name = "toolbar";
		this.toolbar.Size = new System.Drawing.Size(805, 66);
		this.toolbar.Text = "c1ToolBar1";
		this.toolbar.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.toolbar.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.lnkToolAddTeamUser.Command = this.toolCmdAddTeamUser;
		this.toolCmdAddTeamUser.Image = Auditai.UI.Platform.Properties.Resources.toolAddUserToTeam;
		this.toolCmdAddTeamUser.Name = "toolCmdAddTeamUser";
		this.toolCmdAddTeamUser.ShortcutText = "";
		this.toolCmdAddTeamUser.Text = "新增同事";
		this.toolCmdAddTeamUser.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolAddTeamUser_Click);
		this.toolCmdAddTeamUser.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolAddTeamUser_CommandStateQuery);
		this.toolLnkRemoveTeamUser.Command = this.toolCmdRemoveTeamUser;
		this.toolLnkRemoveTeamUser.SortOrder = 1;
		this.toolCmdRemoveTeamUser.Image = Auditai.UI.Platform.Properties.Resources.toolRemoveUserFromTeam;
		this.toolCmdRemoveTeamUser.Name = "toolCmdRemoveTeamUser";
		this.toolCmdRemoveTeamUser.ShortcutText = "";
		this.toolCmdRemoveTeamUser.Text = "移除同事";
		this.toolCmdRemoveTeamUser.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolRemoveTeamUser_Click);
		this.toolCmdRemoveTeamUser.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolRemoveTeamUser_CommandStateQuery);
		this.toolLnkAddUserGroup.Command = this.toolCmdAddUserGroup;
		this.toolLnkAddUserGroup.Delimiter = true;
		this.toolLnkAddUserGroup.SortOrder = 2;
		this.toolCmdAddUserGroup.Image = Auditai.UI.Platform.Properties.Resources.addUserGroup;
		this.toolCmdAddUserGroup.Name = "toolCmdAddUserGroup";
		this.toolCmdAddUserGroup.ShortcutText = "";
		this.toolCmdAddUserGroup.Text = "新建分组";
		this.toolCmdAddUserGroup.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolAddUserGroup_Click);
		this.toolCmdAddUserGroup.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolAddUserGroup_CommandStateQuery);
		this.toolLnkAddChildGroup.Command = this.toolCmdAddChildGroup;
		this.toolLnkAddChildGroup.SortOrder = 3;
		this.toolCmdAddChildGroup.Image = Auditai.UI.Platform.Properties.Resources.addChildGroup;
		this.toolCmdAddChildGroup.Name = "toolCmdAddChildGroup";
		this.toolCmdAddChildGroup.ShortcutText = "";
		this.toolCmdAddChildGroup.Text = "下增分组";
		this.toolCmdAddChildGroup.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolAddChildGroup_Click);
		this.toolCmdAddChildGroup.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolAddChildGroup_CommandStateQuery);
		this.toolLnkRemoveUserGroup.Command = this.toolCmdRemoveUserGroup;
		this.toolLnkRemoveUserGroup.SortOrder = 4;
		this.toolCmdRemoveUserGroup.Image = Auditai.UI.Platform.Properties.Resources.toolDeleteGroup;
		this.toolCmdRemoveUserGroup.Name = "toolCmdRemoveUserGroup";
		this.toolCmdRemoveUserGroup.ShortcutText = "";
		this.toolCmdRemoveUserGroup.Text = "删除分组";
		this.toolCmdRemoveUserGroup.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolRemoveUserGroup_Click);
		this.toolCmdRemoveUserGroup.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolRemoveUserGroup_CommandStateQuery);
		this.toolLnkRenameUserGroup.Command = this.toolCmdRenameUserGroup;
		this.toolLnkRenameUserGroup.SortOrder = 5;
		this.toolCmdRenameUserGroup.Image = Auditai.UI.Platform.Properties.Resources.toolRenameGroup;
		this.toolCmdRenameUserGroup.Name = "toolCmdRenameUserGroup";
		this.toolCmdRenameUserGroup.ShortcutText = "";
		this.toolCmdRenameUserGroup.Text = "重命名分组";
		this.toolCmdRenameUserGroup.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolRenameUserGroup_Click);
		this.toolCmdRenameUserGroup.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolRenameUserGroup_CommandStateQuery);
		this.toolLnkDisplayMode.Command = this.toolCmdDisplayMode;
		this.toolLnkDisplayMode.Delimiter = true;
		this.toolLnkDisplayMode.SortOrder = 6;
		this.toolCmdDisplayMode.Image = Auditai.UI.Platform.Properties.Resources.listMode;
		this.toolCmdDisplayMode.Name = "toolCmdDisplayMode";
		this.toolCmdDisplayMode.ShortcutText = "";
		this.toolCmdDisplayMode.Text = "列表模式";
		this.toolCmdDisplayMode.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolDisplayMode_Click);
		this.toolCmdDisplayMode.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolDisplayMode_CommandStateQuery);
		this.toolLnkRenameTeam.Command = this.toolCmdRenameTeam;
		this.toolLnkRenameTeam.SortOrder = 7;
		this.toolCmdRenameTeam.Image = Auditai.UI.Platform.Properties.Resources.toolRenameGroup;
		this.toolCmdRenameTeam.Name = "toolCmdRenameTeam";
		this.toolCmdRenameTeam.ShortcutText = "";
		this.toolCmdRenameTeam.Text = "重命名组织";
		this.toolCmdRenameTeam.Click += new C1.Win.C1Command.ClickEventHandler(toolCmdRenameTeam_Click);
		this.toolCmdRenameTeam.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(toolCmdRenameTeam_CommandStateQuery);
		this.toolLnkMergeTeam.Command = this.toolCmdMergeTeam;
		this.toolLnkMergeTeam.SortOrder = 8;
		this.toolCmdMergeTeam.Image = Auditai.UI.Platform.Properties.Resources.toolMergeTeam;
		this.toolCmdMergeTeam.Name = "toolCmdMergeTeam";
		this.toolCmdMergeTeam.ShortcutText = "";
		this.toolCmdMergeTeam.Text = "合并组织";
		this.toolCmdMergeTeam.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolMergeTeam_Click);
		this.toolCmdMergeTeam.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolMergeTeam_CommandStateQuery);
		this.toolLnkDismissTeam.Command = this.toolCmdDismissTeam;
		this.toolLnkDismissTeam.SortOrder = 9;
		this.toolCmdDismissTeam.Image = Auditai.UI.Platform.Properties.Resources.toolDismissTeam;
		this.toolCmdDismissTeam.Name = "toolCmdDismissTeam";
		this.toolCmdDismissTeam.ShortcutText = "";
		this.toolCmdDismissTeam.Text = "解散组织";
		this.toolCmdDismissTeam.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolDismissTeam_Click);
		this.toolCmdDismissTeam.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolDismissTeam_CommandStateQuery);
		this.toolLnkLeaveTeam.Command = this.toolCmdLeaveTeam;
		this.toolLnkLeaveTeam.SortOrder = 10;
		this.toolCmdLeaveTeam.Image = Auditai.UI.Platform.Properties.Resources.toolQuitTeam;
		this.toolCmdLeaveTeam.Name = "toolCmdLeaveTeam";
		this.toolCmdLeaveTeam.ShortcutText = "";
		this.toolCmdLeaveTeam.Text = "退出组织";
		this.toolCmdLeaveTeam.Click += new C1.Win.C1Command.ClickEventHandler(cmdToolLeaveTeam_Click);
		this.toolCmdLeaveTeam.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToolLeaveTeam_CommandStateQuery);
		this.c1SplitterPanel2.Controls.Add(this._grid);
		this.c1SplitterPanel2.Height = 552;
		this.c1SplitterPanel2.Location = new System.Drawing.Point(0, 67);
		this.c1SplitterPanel2.MinHeight = 52;
		this.c1SplitterPanel2.MinWidth = 52;
		this.c1SplitterPanel2.Name = "c1SplitterPanel2";
		this.c1SplitterPanel2.Size = new System.Drawing.Size(892, 552);
		this.c1SplitterPanel2.TabIndex = 1;
		this.c1SplitterPanel2.Width = 892;
		this._grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this._grid.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this._grid.ExtendLastCol = true;
		this._grid.Location = new System.Drawing.Point(0, 0);
		this._grid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._grid.Name = "_grid";
		this._grid.Rows.DefaultSize = 20;
		this._grid.Size = new System.Drawing.Size(892, 552);
		this._grid.TabIndex = 0;
		this.c1CommandHolder1.Commands.Add(this.toolCmdAddTeamUser);
		this.c1CommandHolder1.Commands.Add(this.toolCmdRemoveTeamUser);
		this.c1CommandHolder1.Commands.Add(this.toolCmdAddUserGroup);
		this.c1CommandHolder1.Commands.Add(this.toolCmdAddChildGroup);
		this.c1CommandHolder1.Commands.Add(this.toolCmdRemoveUserGroup);
		this.c1CommandHolder1.Commands.Add(this.toolCmdRenameUserGroup);
		this.c1CommandHolder1.Commands.Add(this.toolCmdDisplayMode);
		this.c1CommandHolder1.Commands.Add(this.toolCmdMergeTeam);
		this.c1CommandHolder1.Commands.Add(this.toolCmdDismissTeam);
		this.c1CommandHolder1.Commands.Add(this.toolCmdLeaveTeam);
		this.c1CommandHolder1.Commands.Add(this.toolCmdRenameTeam);
		this.c1CommandHolder1.Owner = this;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(892, 619);
		base.Controls.Add(this.c1SplitContainer1);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "dlgTeamUserManagement";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "同事管理";
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.pnlToolbar.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.commandDock).EndInit();
		this.commandDock.ResumeLayout(false);
		this.c1SplitterPanel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this._grid).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		base.ResumeLayout(false);
	}
}
