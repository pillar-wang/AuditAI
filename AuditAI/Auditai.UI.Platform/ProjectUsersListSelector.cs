using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Command;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using User = Auditai.DTO.User;

namespace Auditai.UI.Platform;

/// <summary>
/// 项目成员选择-列表视图：复选框勾选成员、右键菜单分配角色、头像+分组展示。
/// 数据加载、搜索过滤、角色设置与校验规则与平铺视图（ProjectUsersTileSelector）保持一致。
/// </summary>
public class ProjectUsersListSelector
{
	private class ItemTag
	{
		public User User;

		public UserRole? Role;

		public ListViewGroup Group;
	}

	private readonly ListView _list;

	private readonly ImageList _avatars;

	private readonly List<ListViewItem> _allItems = new List<ListViewItem>();

	private bool _suspendEvents;

	public ProjectUsersSelectorContext Context { get; set; }

	public ProjectUsersListSelector()
	{
		_avatars = new ImageList
		{
			ImageSize = new Size(20, 20),
			ColorDepth = ColorDepth.Depth32Bit
		};
		_list = new ListView
		{
			View = View.Details,
			CheckBoxes = true,
			FullRowSelect = true,
			HideSelection = false,
			MultiSelect = true,
			SmallImageList = _avatars,
			BackColor = AuditTheme.Surface,
			ForeColor = AuditTheme.Text,
			Font = AuditTheme.FontBody,
			BorderStyle = BorderStyle.None,
			Dock = DockStyle.Fill
		};
		_list.Columns.Add("成员", 220);
		_list.Columns.Add("角色", 140);
		_list.ItemChecked += OnItemChecked;
		_list.MouseUp += OnMouseUp;
		_list.Resize += delegate
		{
			ResizeColumns();
		};
		ResizeColumns();
	}

	public Control GetControl(params object[] args)
	{
		return _list;
	}

	public void SetTheme(params object[] args)
	{
		_list.BackColor = AuditTheme.Surface;
		_list.ForeColor = AuditTheme.Text;
		_list.Font = AuditTheme.FontBody;
	}

	public void PopulateUsers(params object[] args)
	{
		_suspendEvents = true;
		_list.BeginUpdate();
		try
		{
			_list.Groups.Clear();
			_list.Items.Clear();
			_allItems.Clear();
			_avatars.Images.Clear();
			if (Context?.RootUsers == null)
			{
				return;
			}
			foreach (User rootUser in Context.RootUsers)
			{
				_allItems.Add(CreateItem(rootUser, null));
			}
			foreach (UserGroup userGroup in Context.UserGroups ?? new List<UserGroup>())
			{
				AppendGroup(userGroup);
			}
			if (Context.Project?.Users != null)
			{
				foreach (User saved in Context.Project.Users)
				{
					ListViewItem listViewItem = FindItem(saved.Id);
					if (listViewItem == null)
					{
						continue;
					}
					ItemTag itemTag = (ItemTag)listViewItem.Tag;
					itemTag.Role = saved.Role;
					listViewItem.SubItems[1].Text = GetUserRoleName(saved.Role);
					listViewItem.Checked = true;
				}
			}
			foreach (ListViewItem allItem in _allItems)
			{
				_list.Items.Add(allItem);
			}
		}
		finally
		{
			_list.EndUpdate();
			_suspendEvents = false;
		}

		void AppendGroup(UserGroup ug)
		{
			ListViewGroup group = new ListViewGroup(GetGroupFullName(ug));
			_list.Groups.Add(group);
			foreach (User user in ug.Users)
			{
				_allItems.Add(CreateItem(user, group));
			}
			foreach (UserGroup child in ug.Children)
			{
				AppendGroup(child);
			}
		}
	}

	public void Search(params object[] args)
	{
		if (Context?.UserViewStates == null)
		{
			return;
		}
		_list.BeginUpdate();
		try
		{
			foreach (ListViewItem item in _allItems)
			{
				ItemTag itemTag = (ItemTag)item.Tag;
				bool visible = Context.UserViewStates.Any((Tuple<User, bool> t) => t.Item1.Id == itemTag.User.Id && t.Item2);
				if (visible && item.ListView == null)
				{
					item.Group = itemTag.Group;
					_list.Items.Add(item);
				}
				else if (!visible && item.ListView != null)
				{
					item.Remove();
				}
			}
		}
		finally
		{
			_list.EndUpdate();
		}
	}

	public IEnumerable<User> ValidateAndGetUsers(params object[] args)
	{
		List<User> list = new List<User>();
		foreach (ListViewItem item in _allItems.Where((ListViewItem i) => i.Checked))
		{
			ItemTag itemTag = (ItemTag)item.Tag;
			if (!itemTag.Role.HasValue)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户角色不能为空");
				return null;
			}
			itemTag.User.Role = itemTag.Role.Value;
			list.Add(itemTag.User);
		}
		if (list.Select((User u) => u.UserName).Distinct().Count() != list.Count)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户名不允许重复");
			return null;
		}
		if (!list.Any((User u) => u.Role == UserRole.Manager))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "成员至少要包含一名" + StringConstBase.Current.Manager);
			return null;
		}
		return list;
	}

	private ListViewItem CreateItem(User user, ListViewGroup group)
	{
		string key = user.Id.ToString();
		if (!_avatars.Images.ContainsKey(key))
		{
			_avatars.Images.Add(key, Auditai.UI.Controls.Util.GetHeadPic(user, 20, withManagerMark: false));
		}
		ListViewItem item = new ListViewItem(user.Name, key)
		{
			Group = group
		};
		item.SubItems.Add("");
		item.UseItemStyleForSubItems = false;
		item.SubItems[1].ForeColor = AuditTheme.TextSecondary;
		item.Tag = new ItemTag
		{
			User = user,
			Group = group
		};
		return item;
	}

	private ListViewItem FindItem(long userId)
	{
		return _allItems.FirstOrDefault((ListViewItem i) => ((ItemTag)i.Tag).User.Id == userId);
	}

	private void OnItemChecked(object sender, ItemCheckedEventArgs e)
	{
		if (_suspendEvents)
		{
			return;
		}
		if (e.Item.Tag is ItemTag itemTag && !e.Item.Checked)
		{
			itemTag.Role = null;
			e.Item.SubItems[1].Text = "";
		}
	}

	private void OnMouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		ListViewHitTestInfo info = _list.HitTest(e.Location);
		if (info.Item == null)
		{
			return;
		}
		C1ContextMenu menu = new C1ContextMenu();
		AddRoleCommand(menu, UserRole.Manager, info.Item);
		AddRoleCommand(menu, UserRole.Assistant, info.Item);
		AddRoleCommand(menu, UserRole.Checker, info.Item);
		AddRoleCommand(menu, UserRole.Editor, info.Item);
		AddRoleCommand(menu, UserRole.User, info.Item);
		NativeMenuShim.Show(menu, _list, e.Location);
	}

	private void AddRoleCommand(C1ContextMenu menu, UserRole role, ListViewItem item)
	{
		C1CommandLink link = new C1CommandLink();
		C1Command command = new C1Command
		{
			Text = GetUserRoleName(role),
			UserData = Tuple.Create(item, role)
		};
		command.Click += CmdRole_Click;
		link.Command = command;
		menu.CommandLinks.Add(link);
	}

	private void CmdRole_Click(object sender, ClickEventArgs e)
	{
		if (!((sender as C1Command)?.UserData is Tuple<ListViewItem, UserRole> tuple))
		{
			return;
		}
		ListViewItem item = tuple.Item1;
		ItemTag itemTag = (ItemTag)item.Tag;
		itemTag.Role = tuple.Item2;
		item.SubItems[1].Text = GetUserRoleName(tuple.Item2);
		if (!item.Checked)
		{
			item.Checked = true;
		}
		_list.Focus();
	}

	private void ResizeColumns()
	{
		if (_list.Columns.Count < 2)
		{
			return;
		}
		int roleWidth = 140;
		int nameWidth = _list.ClientSize.Width - roleWidth - 30;
		if (nameWidth < 120)
		{
			nameWidth = 120;
		}
		_list.Columns[0].Width = nameWidth;
		_list.Columns[1].Width = roleWidth;
	}

	private static string GetUserRoleName(UserRole role)
	{
		return role switch
		{
			UserRole.Manager => StringConstBase.Current.Manager,
			UserRole.Assistant => StringConstBase.Current.Assistant,
			UserRole.Checker => "复核人",
			UserRole.Editor => "编辑者",
			UserRole.User => "查看者",
			_ => "",
		};
	}

	private static string GetGroupFullName(UserGroup g)
	{
		if (g.ParentGroup != null)
		{
			return GetGroupFullName(g.ParentGroup) + " - " + g.Name;
		}
		return g.Name;
	}
}
