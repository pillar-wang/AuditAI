﻿﻿﻿using System;
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
		c1Command.Text = StringConstBase.Current.Manager;
		c1Command.UserData = Tuple.Create(tile, UserRole.Manager);
		c1Command.Click += CmdUserRole_Click;
		c1CommandLink.Command = c1Command;
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = StringConstBase.Current.Assistant;
		c1Command2.UserData = Tuple.Create(tile, UserRole.Assistant);
		c1Command2.Click += CmdUserRole_Click;
		c1CommandLink2.Command = c1Command2;
		contextMenu.CommandLinks.Add(c1CommandLink);
		contextMenu.CommandLinks.Add(c1CommandLink2);
		if (!(Program.MainForm.CurrentEdition is AppEditionGeneral))
		{
			C1CommandLink c1CommandLink3 = new C1CommandLink();
			C1Command c1Command3 = new C1Command();
			c1Command3.Text = "复核人";
			c1Command3.UserData = Tuple.Create(tile, UserRole.Checker);
			c1Command3.Click += CmdUserRole_Click;
			c1CommandLink3.Command = c1Command3;
			contextMenu.CommandLinks.Add(c1CommandLink3);
		}
		contextMenu.ShowContextMenu(_tileControl, new Point(tile.Group.X + tile.X, tile.Group.Y + tile.Y + tile.Height + _tileControl.ScrollOffset));
	}

	private void CmdUserRole_Click(object sender, ClickEventArgs e)
	{
		if ((sender as C1Command)?.UserData is Tuple<Tile, UserRole> tuple)
		{
			string text = null;
			switch (tuple.Item2)
			{
			case UserRole.Manager:
				text = StringConstBase.Current.Manager;
				break;
			case UserRole.Assistant:
				text = StringConstBase.Current.Assistant;
				break;
			case UserRole.Checker:
				text = "复核人";
				break;
			}
			if (text != null && tuple.Item1.Tag is TileTag tileTag)
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
				SizeF sizeF = e1.Graphics.MeasureString(tt.Text, new Font("微软雅黑", 9f));
				e1.Graphics.DrawLine(new Pen(Color.FromArgb(0, 73, 92), 1f), new Point(8, tt.Height - 9), new Point(8 + (int)sizeF.Width, tt.Height - 9));
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
				Image1 = Auditai.UI.Controls.Util.GetHeadPic(user, 32, withManagerMark: false),
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
			UserRole.Checker => "复核人", 
			UserRole.Assistant => StringConstBase.Current.Assistant, 
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
			CellWidth = 90,
			CellHeight = 30,
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
		panelElement2.BackColor = Color.FromArgb(0, 73, 92);
		panelElement2.Dock = DockStyle.Bottom;
		panelElement2.FixedHeight = 1;
		TextElement textElement = new TextElement();
		textElement.ForeColor = Color.Black;
		textElement.ForeColorSelector = ForeColorSelector.Unbound;
		textElement.Font = new Font("微软雅黑", 9f, FontStyle.Regular);
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
		panelElement.Margin = new Padding(0, 10, 0, 0);
		panelElement.Alignment = ContentAlignment.TopCenter;
		ImageElement imageElement = new ImageElement();
		imageElement.AlignmentOfContents = ContentAlignment.TopCenter;
		imageElement.FixedHeight = 60;
		imageElement.FixedWidth = 50;
		imageElement.ImageSelector = ImageSelector.Image1;
		panelElement.Children.Add(imageElement);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.FixedHeight = 20;
		panelElement2.FixedWidth = 130;
		panelElement2.Alignment = ContentAlignment.BottomCenter;
		TextElement textElement = new TextElement();
		textElement.AlignmentOfContents = ContentAlignment.MiddleCenter;
		textElement.TextTrimming = TextTrimming.EndEllipsis;
		textElement.SingleLine = false;
		textElement.FixedHeight = 20;
		textElement.FixedWidth = 130;
		textElement.TextSelector = TextSelector.Text1;
		panelElement2.Children.Add(textElement);
		panelElement2.Dock = DockStyle.Bottom;
		PanelElement panelElement3 = new PanelElement();
		panelElement3.FixedHeight = 20;
		panelElement3.FixedWidth = 130;
		panelElement3.Alignment = ContentAlignment.BottomCenter;
		TextElement textElement2 = new TextElement();
		textElement2.AlignmentOfContents = ContentAlignment.MiddleCenter;
		textElement2.TextTrimming = TextTrimming.EndEllipsis;
		textElement2.SingleLine = false;
		textElement2.FixedHeight = 20;
		textElement2.FixedWidth = 130;
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
