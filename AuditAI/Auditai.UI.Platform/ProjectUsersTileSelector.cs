﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1Tile;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class ProjectUsersTileSelector
{
	private class TileTag
	{
		public Auditai.DTO.User User { get; set; }

		public UserRole? UserRole { get; set; }

		public TileTag(Auditai.DTO.User user)
		{
			User = user;
		}
	}

	private C1TileControlEx _tileControl;

	private Template _userTemplate;

	private Template _titleTemplate;

	private C1ContextMenu contextMenu;

	// 模板字体/画笔按主题 Token 缓存复用，避免每次重绘重复创建 GDI 对象
	private static readonly Font GroupCaptionFont = new Font(AuditTheme.FontFamilySans, 9f);

	private static readonly Font UserNameFont = new Font(AuditTheme.FontFamilySans, 9f);

	private static readonly Font UserRoleFont = new Font(AuditTheme.FontFamilySans, 8.5f);

	private static readonly Pen GroupCaptionPen = new Pen(AuditTheme.Brand, 1f);

	public ProjectUsersSelectorContext Context { get; set; }

	public ProjectUsersTileSelector()
	{
		_tileControl = InitializeTileControl();
		_userTemplate = CreateUserTemplate();
		_titleTemplate = CreateTemplateTitle();
		_tileControl.Templates.Add(_userTemplate);
		_tileControl.Templates.Add(_titleTemplate);
		_tileControl.TileUnCheckedEvent += _tileControl_TileUnCheckedEvent;
		_tileControl.TileClicked += _tileControl_TileClicked;
		_tileControl.MouseWheel += _tileControl_MouseWheel;
	}

	private void _tileControl_MouseWheel(object sender, MouseEventArgs e)
	{
		if (contextMenu != null && contextMenu.Visible)
		{
			contextMenu.CloseContextMenu();
		}
	}

	private void _tileControl_TileUnCheckedEvent(object sender, Tile e)
	{
		if (e.Tag is TileTag tileTag)
		{
			tileTag.UserRole = null;
			e.Text1 = tileTag.User.Name;
			e.Text2 = null;
		}
	}

	private void _tileControl_TileClicked(object sender, TileEventArgs e)
	{
		Tile tile = e.Tile;
		contextMenu = new C1ContextMenu();

		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "项目经理";
		c1Command.UserData = Tuple.Create(tile, UserRole.Manager);
		c1Command.Click += CmdUserRole_Click;
		c1CommandLink.Command = c1Command;
		contextMenu.CommandLinks.Add(c1CommandLink);

		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "项目助理";
		c1Command2.UserData = Tuple.Create(tile, UserRole.Assistant);
		c1Command2.Click += CmdUserRole_Click;
		c1CommandLink2.Command = c1Command2;
		contextMenu.CommandLinks.Add(c1CommandLink2);

		C1CommandLink c1CommandLink3 = new C1CommandLink();
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "复核人";
		c1Command3.UserData = Tuple.Create(tile, UserRole.Checker);
		c1Command3.Click += CmdUserRole_Click;
		c1CommandLink3.Command = c1Command3;
		contextMenu.CommandLinks.Add(c1CommandLink3);

		C1CommandLink c1CommandLink4 = new C1CommandLink();
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = "编辑者";
		c1Command4.UserData = Tuple.Create(tile, UserRole.Editor);
		c1Command4.Click += CmdUserRole_Click;
		c1CommandLink4.Command = c1Command4;
		contextMenu.CommandLinks.Add(c1CommandLink4);

		C1CommandLink c1CommandLink5 = new C1CommandLink();
		C1Command c1Command5 = new C1Command();
		c1Command5.Text = "查看者";
		c1Command5.UserData = Tuple.Create(tile, UserRole.User);
		c1Command5.Click += CmdUserRole_Click;
		c1CommandLink5.Command = c1Command5;
		contextMenu.CommandLinks.Add(c1CommandLink5);

		NativeMenuShim.Show(contextMenu, _tileControl, new Point(tile.Group.X + tile.X, tile.Group.Y + tile.Y + tile.Height + _tileControl.ScrollOffset));
	}

	private void CmdUserRole_Click(object sender, ClickEventArgs e)
	{
		if ((sender as C1Command)?.UserData is Tuple<Tile, UserRole> tuple)
		{
			// 五种角色统一走 GetUserRoleName（原实现漏了编辑者/查看者，菜单项点击无反应）
			string text = GetUserRoleName(tuple.Item2);
			if (!string.IsNullOrEmpty(text) && tuple.Item1.Tag is TileTag tileTag)
			{
				tuple.Item1.Text1 = tileTag.User.Name;
				tuple.Item1.Text2 = text;
				tileTag.UserRole = tuple.Item2;
				_tileControl.SelectTile(tuple.Item1);
			}
		}
	}

	public Control GetControl()
	{
		return _tileControl;
	}

	public void Search()
	{
		foreach (C1.Win.C1Tile.Group group in _tileControl.Groups)
		{
			foreach (Tile tile in group.Tiles)
			{
				if (tile.Tag is TileTag tileTag)
				{
					tile.Visible = IsUserVisible(tileTag.User);
				}
			}
		}
	}

	private bool IsUserVisible(Auditai.DTO.User u)
	{
		return Context.UserViewStates.Find((Tuple<Auditai.DTO.User, bool> tup) => tup.Item1.Id == u.Id).Item2;
	}

	public void PopulateUsers()
	{
		_tileControl.ClearSelected();
		_tileControl.Groups.Clear();
		// 本地模式等无团队数据的场景：Context 尚未填充，直接保持空白，避免空引用
		if (Context?.RootUsers == null)
		{
			return;
		}
		C1.Win.C1Tile.Group group = new C1.Win.C1Tile.Group();
		foreach (Auditai.DTO.User rootUser in Context.RootUsers)
		{
			Tile item = createUserTile(rootUser);
			group.Tiles.Add(item);
		}
		_tileControl.Groups.Add(group);
		foreach (UserGroup userGroup in Context.UserGroups)
		{
			AppendGroup(userGroup);
		}
		void AppendGroup(UserGroup ug)
		{
			C1.Win.C1Tile.Group group2 = new C1.Win.C1Tile.Group();
			Tile tt = new Tile
			{
				Text = GetGroupFullName(ug),
				Template = _titleTemplate,
				HorizontalSize = 2,
				VerticalSize = 1,
				BackColor = Color.Transparent
			};
			tt.Paint += delegate(object s1, PaintEventArgs e1)
			{
				SizeF sizeF = e1.Graphics.MeasureString(tt.Text, GroupCaptionFont);
				e1.Graphics.DrawLine(GroupCaptionPen, new Point(8, tt.Height - 9), new Point(8 + (int)sizeF.Width, tt.Height - 9));
			};
			group2.Tiles.Add(tt);
			_tileControl.Groups.Add(group2);
			C1.Win.C1Tile.Group group3 = new C1.Win.C1Tile.Group();
			_tileControl.Groups.Add(group3);
			foreach (Auditai.DTO.User user3 in ug.Users)
			{
				Tile item2 = createUserTile(user3);
				group3.Tiles.Add(item2);
			}
			foreach (UserGroup child in ug.Children)
			{
				AppendGroup(child);
			}
		}
		static string GetGroupFullName(UserGroup g)
		{
			if (g.ParentGroup != null)
			{
				return GetGroupFullName(g.ParentGroup) + " - " + g.Name;
			}
			return g.Name;
		}
		Tile createUserTile(Auditai.DTO.User user)
		{
			TileTag tileTag = new TileTag(user);
			Tile tile = new Tile
			{
				Template = _userTemplate,
				Image1 = Auditai.UI.Controls.Util.GetHeadPic(user, 42, withManagerMark: false),
				Text1 = user.Name,
				Text2 = null,
				Tag = tileTag,
				HorizontalSize = 1,
				VerticalSize = 2
			};
			if (Context?.Project?.Users == null)
			{
				return tile;
			}
			Auditai.DTO.User user2 = Context.Project.Users.FirstOrDefault((Auditai.DTO.User u) => u.Id == user.Id);
			if (user2 != null)
			{
				tile.Text1 = user2.Name;
				tile.Text2 = GetUserRoleName(user2.Role);
				tileTag.UserRole = user2.Role;
				_tileControl.SelectTile(tile);
			}
			return tile;
		}
	}

	private string GetUserRoleName(UserRole role)
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

	public void SetTheme()
	{
		Theme.SetCurrentObject(_tileControl);
		_tileControl.TileBorderColor = Color.Transparent;
		_tileControl.CustomBorderColor = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
		_tileControl.HotBorderColor = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
	}

	public IEnumerable<Auditai.DTO.User> ValidateAndGetUsers()
	{
		List<Auditai.DTO.User> list = new List<Auditai.DTO.User>();
		foreach (Tile selectedTile in _tileControl.SelectedTiles)
		{
			if (selectedTile.Tag is TileTag { UserRole: var userRole } tileTag)
			{
				if (!userRole.HasValue)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户角色不能为空");
					return null;
				}
				tileTag.User.Role = userRole.Value;
				list.Add(tileTag.User);
			}
		}
		if (list.Select((Auditai.DTO.User u) => u.UserName).Distinct().Count() != list.Count)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户名不允许重复");
			return null;
		}
		if (!list.Any((Auditai.DTO.User u) => u.Role == UserRole.Manager))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "成员至少要包含一名" + StringConstBase.Current.Manager);
			return null;
		}
		return list;
	}

	private C1TileControlEx InitializeTileControl()
	{
		return new C1TileControlEx
		{
			CellWidth = 130,
			CellHeight = 50,
			AllowChecking = false,
			Dock = DockStyle.Fill,
			CellSpacing = 20,
			Margin = new Padding(0),
			Padding = new Padding(0),
			GroupPadding = new Padding(0, 10, 0, 0),
			Orientation = LayoutOrientation.Vertical,
			TileBorderColor = Color.White,
			GroupSpacing = 5,
			ShowToolTips = false,
			AllowMultiSelect = true
		};
	}

	private Template CreateTemplateTitle()
	{
		Template template = new Template();
		template.Description = "Subgroup";
		PanelElement panelElement = new PanelElement();
		panelElement.AlignmentOfContents = ContentAlignment.BottomLeft;
		PanelElement panelElement2 = new PanelElement();
		panelElement2.BackColor = AuditTheme.Brand;
		panelElement2.Dock = DockStyle.Bottom;
		panelElement2.FixedHeight = 1;
		TextElement textElement = new TextElement();
		textElement.ForeColor = AuditTheme.TextSecondary;
		textElement.ForeColorSelector = ForeColorSelector.Unbound;
		textElement.Font = GroupCaptionFont;
		textElement.Margin = new Padding(0, 0, 0, 6);
		textElement.SingleLine = true;
		panelElement.Children.Add(textElement);
		panelElement.Dock = DockStyle.Fill;
		panelElement.Padding = new Padding(8, 0, 8, 8);
		template.Elements.Add(panelElement);
		template.Enabled = false;
		template.Name = "subgroupTemplate";
		return template;
	}

	private Template CreateUserTemplate()
	{
		Template template = new Template();
		template.Description = "Win32";
		PanelElement panelElement = new PanelElement();
		panelElement.FixedWidth = 50;
		panelElement.Margin = new Padding(0, 8, 0, 0);
		panelElement.Alignment = ContentAlignment.TopCenter;
		ImageElement imageElement = new ImageElement();
		imageElement.AlignmentOfContents = ContentAlignment.TopCenter;
		imageElement.FixedHeight = 52;
		imageElement.FixedWidth = 50;
		imageElement.ImageSelector = ImageSelector.Image1;
		panelElement.Children.Add(imageElement);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.FixedHeight = 18;
		panelElement2.FixedWidth = 130;
		panelElement2.Alignment = ContentAlignment.BottomCenter;
		TextElement textElement = new TextElement();
		textElement.AlignmentOfContents = ContentAlignment.MiddleCenter;
		textElement.TextTrimming = TextTrimming.EndEllipsis;
		textElement.SingleLine = true;
		textElement.FixedHeight = 18;
		textElement.FixedWidth = 130;
		textElement.ForeColor = AuditTheme.Text;
		textElement.ForeColorSelector = ForeColorSelector.Unbound;
		textElement.Font = UserNameFont;
		textElement.TextSelector = TextSelector.Text1;
		panelElement2.Children.Add(textElement);
		panelElement2.Dock = DockStyle.Bottom;
		PanelElement panelElement3 = new PanelElement();
		panelElement3.FixedHeight = 18;
		panelElement3.FixedWidth = 130;
		panelElement3.Alignment = ContentAlignment.BottomCenter;
		TextElement textElement2 = new TextElement();
		textElement2.AlignmentOfContents = ContentAlignment.MiddleCenter;
		textElement2.TextTrimming = TextTrimming.EndEllipsis;
		textElement2.SingleLine = true;
		textElement2.FixedHeight = 18;
		textElement2.FixedWidth = 130;
		textElement2.ForeColor = AuditTheme.TextSecondary;
		textElement2.ForeColorSelector = ForeColorSelector.Unbound;
		textElement2.Font = UserRoleFont;
		textElement2.TextSelector = TextSelector.Text2;
		panelElement3.Children.Add(textElement2);
		panelElement3.Dock = DockStyle.Bottom;
		template.Elements.Add(panelElement3);
		template.Elements.Add(panelElement2);
		template.Elements.Add(panelElement);
		template.Name = "mapImgTemplate";
		return template;
	}
}
