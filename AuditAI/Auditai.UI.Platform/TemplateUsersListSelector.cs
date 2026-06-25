﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.DTO;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class TemplateUsersListSelector
{
	private const int IDX_NAME = 0;

	private const int IDX_CHECK = 1;

	private const int IDX_USERROLE = 2;

	private const string CN_NAME = "UserName";

	private const string CN_CHECK = "Check";

	private const string CN_USERROLE = "UserRole";

	private readonly C1FlexGridEx _grid;

	public ProjectUsersSelectorContext Context { get; set; }

	public TemplateUsersListSelector()
	{
		_grid = new C1FlexGridEx();
		_grid.Dock = DockStyle.Fill;
		_grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		_grid.DrawMode = DrawModeEnum.OwnerDraw;
		_grid.AllowSorting = AllowSortingEnum.None;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.Paint += delegate(object s1, PaintEventArgs e1)
		{
			_grid.DrawFormBorder(e1.Graphics);
		};
		_grid.MouseClick += _grid_MouseClick;
		_grid.Font = new Font("微软雅黑", 9f);
		_grid.ExtendLastCol = true;
		_grid.Resize += _grid_Resize;
		_grid.Tree.Column = 0;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Count = 0;
		_grid.Cols.Fixed = 0;
		_grid.Rows.DefaultSize = 30;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		column.Name = "UserName";
		column.Caption = "用户姓名";
		column.DataType = typeof(string);
		column = _grid.Cols.Add();
		column.Name = "Check";
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.Width = 50;
		column.Caption = "选择";
		column.DataType = typeof(bool);
		column = _grid.Cols.Add();
		column.Name = "UserRole";
		column.Caption = "角色";
		column.DataType = typeof(UserRole);
		Dictionary<UserRole, string> dataMap = new Dictionary<UserRole, string>
		{
			{
				UserRole.Editor,
				"可编辑的用户"
			},
			{
				UserRole.User,
				"可使用的用户"
			}
		};
		column.DataMap = dataMap;
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
			HitTestTypeEnum type = hitTestInfo.Type;
			if (type == HitTestTypeEnum.Cell && hitTestInfo.Column == 0 && hitTestInfo.Row >= _grid.Rows.Fixed)
			{
				_grid.Rows[hitTestInfo.Row].Node.Expanded = !_grid.Rows[hitTestInfo.Row].Node.Expanded;
			}
		}
	}

	private void _grid_Resize(object sender, EventArgs e)
	{
		Resize();
	}

	private void Resize()
	{
		if (_grid.Cols.Contains("UserName") && _grid.Cols.Contains("UserRole"))
		{
			int num = 0;
			if (_grid.Cols.Contains("Check"))
			{
				num += _grid.Cols["Check"].Width;
			}
			int width = (_grid.Width - num) / 2;
			_grid.Cols["UserName"].Width = width;
			_grid.Cols["UserRole"].Width = width;
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
	}

	public Control GetControl()
	{
		return _grid;
	}

	public void PopulateUsers()
	{
		_grid.BeginUpdate();
		try
		{
			foreach (UserGroup userGroup in Context.UserGroups)
			{
				AppendGroupNode(userGroup, null);
			}
			foreach (User rootUser in Context.RootUsers)
			{
				AppendUserNode(rootUser, null);
			}
			Resize();
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	public void Search()
	{
		for (int i = 0; i < _grid.BodyRowsCount; i++)
		{
			C1.Win.C1FlexGrid.Row row = _grid.BodyGetRow(i);
			object userData = row.UserData;
			User user = userData as User;
			if (user != null)
			{
				row.Visible = Context.UserViewStates.Find((Tuple<User, bool> tup) => tup.Item1.Id == user.Id).Item2;
			}
		}
	}

	private void AppendUserNode(User user, Node parentNode)
	{
		C1.Win.C1FlexGrid.Row row = ((parentNode == null) ? _grid.Rows.AddNode(0).Row : parentNode.AddNode(NodeTypeEnum.LastChild, string.Empty).Row);
		row.UserData = user;
		row["UserName"] = user.UserName;
		System.Drawing.Image headPic = Auditai.UI.Controls.Util.GetHeadPic(user, 16, withManagerMark: true);
		_grid.SetCellImage(row.Index, "UserName", headPic);
		row["UserName"] = user.Name;
		if (Context?.Project?.Users == null)
		{
			_grid.SetCellCheck(row.Index, 1, CheckEnum.Unchecked);
			return;
		}
		User user2 = Context.Project.Users.FirstOrDefault((User u) => u.Id == user.Id);
		if (user2 != null)
		{
			row["UserRole"] = user2.Role;
			_grid.SetCellCheck(row.Index, 1, CheckEnum.Checked);
		}
		else
		{
			_grid.SetCellCheck(row.Index, 1, CheckEnum.Unchecked);
		}
	}

	private void AppendGroupNode(UserGroup userGroup, Node parentNode)
	{
		C1.Win.C1FlexGrid.Row row = ((parentNode == null) ? _grid.Rows.AddNode(0).Row : parentNode.AddNode(NodeTypeEnum.LastChild, string.Empty).Row);
		row.UserData = userGroup;
		row["UserName"] = userGroup.Name;
		_grid.SetCellImage(row.Index, "UserName", Auditai.UI.Controls.Util.StandardImage(Resources.imgUserGroup, 16));
		row.AllowEditing = false;
		foreach (UserGroup child in userGroup.Children)
		{
			AppendGroupNode(child, row.Node);
		}
		foreach (User user in userGroup.Users)
		{
			AppendUserNode(user, row.Node);
		}
	}

	public void SetTheme()
	{
		Theme.SetCurrentObject(_grid);
	}

	public IEnumerable<User> ValidateAndGetUsers()
	{
		List<User> list = new List<User>();
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
			if (_grid.GetCellCheck(i, 1) == CheckEnum.Checked && row.UserData is User user)
			{
				UserRole? userRole = (UserRole?)_grid[i, "UserRole"];
				if (!userRole.HasValue)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户角色不能为空");
					return null;
				}
				user.Role = userRole.Value;
				list.Add(user);
			}
		}
		if (list.Select((User u) => u.UserName).Distinct().Count() != list.Count)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户名不允许重复");
			return null;
		}
		if (!list.Any((User u) => u.Role == UserRole.Editor))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "成员至少要包含一名可编辑的用户");
			return null;
		}
		return list;
	}
}
