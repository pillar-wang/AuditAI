using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using C1.Win.C1Tile;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class dlgTeamSelector : C1RibbonForm
{
	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private C1TileControlEx _tileControl;

	private Template _titleTemplate;

	private Template _teamTemplate;

	private Template _funcTemplate;

	private readonly C1TextBoxEx _txbSearch;

	private readonly C1Button _btnCloseSearch;

	public EventHandler AfterTeamOpened;

	private List<UserTeam> _teamList;

	private bool _isInSearching;

	private bool _isCurrentUseInAnyTeam;

	private const string TAG_CREATETEAM = "create";

	private const string TAG_REFRESHTEAM = "refresh";

	private IContainer components;

	private C1SplitContainer c1SplitContainer1;

	private C1SplitterPanel pnlToolbar;

	private C1SplitterPanel pnlTeamList;

	private C1ToolBar toolbar;

	private C1Command toolCmdOpenTeam;

	private C1CommandLink toolLnkExitTeam;

	private C1Command toolCmdExitTeam;

	private C1CommandHolder c1CommandHolder1;

	private C1CommandLink toolLnkRefresh;

	private C1Command toolCmdRefresh;

	private C1CommandLink toolLnkCreateTeam;

	private C1Command toolCmdCreateTeam;

	private C1CommandLink toollnkSearchTeam;

	private C1Command toolCmdSearchTeam;

	private C1SplitterPanel pnlSearch;

	public string SelectedTeamId { get; private set; }

	public bool PreventExit { get; set; }

	public dlgTeamSelector(List<UserTeam> teamList)
	{
		_isCurrentUseInAnyTeam = teamList != null && teamList.Count > 0;
		_teamList = GetCurrentPlatformVisibleTeam(teamList);
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterScreen;
		_tileControl = new C1TileControlEx
		{
			CellWidth = 10,
			CellHeight = 10,
			AllowChecking = false,
			Dock = DockStyle.Fill,
			CellSpacing = 20,
			Margin = new Padding(0),
			Padding = new Padding(0),
			GroupPadding = new Padding(0),
			Orientation = LayoutOrientation.Vertical,
			TileBorderColor = Color.Transparent,
			GroupSpacing = 5
		};
		_tileControl.DoubleClickTile += C1TileControl1_DoubleClickTile;
		_tileControl.TileClicked += _tileControl_TileClicked;
		pnlTeamList.Controls.Add(_tileControl);
		_tileControl.Groups.Clear();
		_teamTemplate = CreateTeamTemplate();
		_tileControl.Templates.Add(_teamTemplate);
		_titleTemplate = CreateTitleTemplate();
		_tileControl.Templates.Add(_titleTemplate);
		_funcTemplate = CreateFuncTemplate();
		_tileControl.Templates.Add(_funcTemplate);
		base.Shown += DlgTeamSelector_Shown;
		toolCmdSearchTeam.Click += ToolCmdSearchTeam_Click;
		_btnCloseSearch = new C1Button
		{
			Image = Resources.close2,
			Text = "",
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Dock = DockStyle.Right
		};
		_btnCloseSearch.Click += _btnCloseSearch_Click;
		pnlSearch.Controls.Add(_btnCloseSearch);
		_txbSearch = new C1TextBoxEx
		{
			Dock = DockStyle.Fill,
			AutoSize = false,
			TextDetached = true
		};
		_txbSearch.TextChanged += _txbSearch_TextChanged;
		pnlSearch.Controls.Add(_txbSearch);
		foreach (C1CommandLink commandLink in toolbar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		toolCmdExitTeam.Visible = false;
		toolCmdCreateTeam.Visible = false;
		pnlSearch.Visible = false;
		if (User.Current.IsSystemSupporter)
		{
			toolCmdExitTeam.Visible = true;
		}
	}

	private void ToolCmdSearchTeam_Click(object sender, ClickEventArgs e)
	{
		if (_isInSearching)
		{
			_isInSearching = false;
			pnlSearch.Visible = false;
			toolCmdSearchTeam.Text = "搜索组织";
		}
		else
		{
			_isInSearching = true;
			pnlSearch.Visible = true;
			toolCmdSearchTeam.Text = "关闭搜索";
		}
		FilterTeamList();
	}

	private void _txbSearch_TextChanged(object sender, EventArgs e)
	{
		if (_isInSearching)
		{
			FilterTeamList();
		}
	}

	private void _btnCloseSearch_Click(object sender, EventArgs e)
	{
		_isInSearching = false;
		pnlSearch.Visible = false;
		toolCmdSearchTeam.Text = "搜索组织";
		FilterTeamList();
	}

	private List<UserTeam> GetCurrentPlatformVisibleTeam(List<UserTeam> teamList)
	{
		List<UserTeam> list = new List<UserTeam>();
		if (teamList != null)
		{
			foreach (UserTeam team in teamList)
			{
				if (Program.IsClientPlatformMatchToTeamType(team, isAllowUseSystemSupporterRule: false))
				{
					list.Add(team);
				}
			}
		}
		return list;
	}

	private async void _tileControl_TileClicked(object sender, TileEventArgs e)
	{
		object tag = e.Tile.Tag;
		switch (tag as string)
		{
		case "create":
			await CreateTeam();
			break;
		case "refresh":
			await RefreshTeamList();
			break;
		}
	}

	private void SetTheme()
	{
		_tileControl.TileBorderColor = Color.Transparent;
		_tileControl.CustomBorderColor = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.ThemeContext.DarkColor;
		if (Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
			imageProcess.ProcessImage();
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
			imageProcess.ProcessImage();
		}
	}

	private void DlgTeamSelector_Shown(object sender, EventArgs e)
	{
		base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(Resources.TeamManage);
		FilterTeamList();
	}

	private async void toolCmdRefresh_Click(object sender, ClickEventArgs e)
	{
		await RefreshTeamList();
	}

	private async void C1TileControl1_DoubleClickTile(object sender, Tile e)
	{
		Tile selectedTile = _tileControl.SelectedTile;
		if (selectedTile != null && selectedTile.Tag != null && !(selectedTile.Tag.ToString() == "create") && !(selectedTile.Tag.ToString() == "refresh"))
		{
			await OpenTeam();
		}
	}

	private async void toolCmdOpenTeam_Click(object sender, ClickEventArgs e)
	{
		Tile selectedTile = _tileControl.SelectedTile;
		if (selectedTile != null && !(selectedTile.Tag.ToString() == "create") && !(selectedTile.Tag.ToString() == "refresh"))
		{
			await OpenTeam();
		}
	}

	private async void toolCmdExitTeam_Click(object sender, ClickEventArgs e)
	{
		if (await ExitTeam())
		{
			await RefreshTeamList();
		}
	}

	private async Task OpenTeam()
	{
		bool success = false;
		_tileControl.Enabled = false;
		toolCmdOpenTeam.Enabled = false;
		try
		{
			Tile selectedTile = _tileControl.SelectedTile;
			Guid result;
			if (selectedTile == null || string.IsNullOrWhiteSpace(selectedTile.Tag.ToString()))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择组织!");
			}
			else if (Guid.TryParse(selectedTile.Tag.ToString(), out result) && await Program.OpenTeam(result))
			{
				PreventExit = true;
				Close();
				success = true;
				AfterTeamOpened?.Invoke(this, EventArgs.Empty);
			}
		}
		finally
		{
			if (!success)
			{
				_tileControl.Enabled = true;
				toolCmdOpenTeam.Enabled = true;
			}
		}
	}

	private async Task CreateTeam()
	{
		base.Visible = false;
		try
		{
			dlgTeamCreator creator = new dlgTeamCreator();
			if (creator.ShowDialog() == DialogResult.OK && await Program.GetUserTeams() != null && await Program.OpenTeam(creator.TeamId.Value))
			{
				PreventExit = true;
				Close();
				AfterTeamOpened?.Invoke(this, EventArgs.Empty);
			}
		}
		finally
		{
			base.Visible = true;
		}
	}

	private async Task<bool> ExitTeam()
	{
		Tile selectedTile = _tileControl.SelectedTile;
		if (selectedTile == null || string.IsNullOrWhiteSpace(selectedTile.Tag.ToString()))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择组织!");
			return false;
		}
		string input = selectedTile.Tag.ToString();
		if (!Guid.TryParse(input, out var result) || result == Guid.Empty)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择组织!");
			return false;
		}
		if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "确定要退出组织吗？", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return false;
		}
		return await Program.ExitTeam(User.Current.UserName, result);
	}

	private async Task RefreshTeamList()
	{
		List<UserTeam> list = await Program.GetUserTeams();
		_isCurrentUseInAnyTeam = list != null && list.Count > 0;
		_teamList = GetCurrentPlatformVisibleTeam(list);
		FilterTeamList();
	}

	protected void FilterTeamList()
	{
		if (!_isInSearching)
		{
			PopulateTeamList(_teamList);
			return;
		}
		string text = _txbSearch.Text.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			PopulateTeamList(_teamList);
			return;
		}
		List<UserTeam> list = new List<UserTeam>();
		if (_teamList != null)
		{
			foreach (UserTeam team in _teamList)
			{
				if (FuzzySearch.Filter(team.Name, text) > 0)
				{
					list.Add(team);
				}
			}
		}
		PopulateTeamList(list);
	}

	private void PopulateTeamList(List<UserTeam> teams)
	{
		if (teams == null)
		{
			teams = new List<UserTeam>();
		}
		_tileControl.Groups.Clear();
		if (teams.Count == 0 && !_isCurrentUseInAnyTeam)
		{
			C1.Win.C1Tile.Group group = createGroup(string.Empty);
			group.Tiles.Add(CreateFuncTile("create", Resources.toolCreateTeam, "创建一个新组织", "● 若您是组织的系统管理员，请点击此按钮创建一个新组织。\n\n● 系统管理员一般应由组织中职级较高的人员担任，系统管理员创建组织后，具备对组织内人员进行增减的权限。"));
			C1.Win.C1Tile.Group group2 = createGroup(string.Empty);
			group2.Tiles.Add(CreateFuncTile("refresh", Resources.RefreshProject, "刷新已加入的组织", "● 若您不是组织的系统管理员，请在系统管理员创建组织并将您加入后，点此按钮刷新。\n\n● 系统管理员在准备加您至其创建的组织中时，请向系统管理员提供您注册的用户名。"));
			_tileControl.Groups.Add(group);
			_tileControl.Groups.Add(group2);
			_tileControl.SurfaceContentAlignment = ContentAlignment.MiddleCenter;
			_tileControl.Orientation = LayoutOrientation.Horizontal;
			pnlToolbar.Visible = false;
			_tileControl.GroupSpacing = 40;
			return;
		}
		_tileControl.GroupSpacing = 0;
		pnlToolbar.Visible = true;
		_tileControl.Orientation = LayoutOrientation.Vertical;
		_tileControl.SurfaceContentAlignment = ContentAlignment.TopLeft;
		_tileControl.Groups.Add(CreateTitleGroup("请选择进入的组织"));
		C1.Win.C1Tile.Group group3 = createGroup(string.Empty);
		foreach (UserTeam team in teams)
		{
			Tile item = CreateTeamTile(team);
			group3.Tiles.Add(item);
		}
		_tileControl.Groups.Add(group3);
		_tileControl.SurfaceContentAlignment = ContentAlignment.TopLeft;
		static C1.Win.C1Tile.Group createGroup(string title)
		{
			return new C1.Win.C1Tile.Group
			{
				Text = title
			};
		}
	}

	private Tile CreateTeamTile(UserTeam userTeam, int width = 8, int height = 4)
	{
		return new Tile
		{
			HorizontalSize = width,
			VerticalSize = height,
			Tag = userTeam.Id,
			Text = userTeam.Name,
			Image = (AppEditions.Editions.Find((AppEditionBase e) => e.Code == userTeam.Type)?.Icon ?? Resources.imgTeam),
			Template = _teamTemplate
		};
	}

	private Tile CreateFuncTile(string funcId, System.Drawing.Image icon, string title, string content)
	{
		return new Tile
		{
			Tag = funcId,
			Image = icon,
			Text1 = title,
			Text2 = content,
			VerticalSize = 7,
			HorizontalSize = 9,
			Template = _funcTemplate,
			ForeColor1 = Color.Red
		};
	}

	private C1.Win.C1Tile.Group CreateTitleGroup(string title, int width = 5)
	{
		C1.Win.C1Tile.Group group = new C1.Win.C1Tile.Group();
		Tile item = new Tile
		{
			Text = title,
			HorizontalSize = width,
			VerticalSize = 2,
			Template = _titleTemplate,
			BackColor = Color.Transparent
		};
		group.Tiles.Add(item);
		return group;
	}

	private Template CreateTeamTemplate()
	{
		Template template = new Template();
		PanelElement panelElement = new PanelElement();
		panelElement.Dock = DockStyle.Left;
		panelElement.FixedWidth = 80;
		panelElement.AlignmentOfContents = ContentAlignment.MiddleCenter;
		C1.Win.C1Tile.ImageElement item = new C1.Win.C1Tile.ImageElement();
		panelElement.Children.Add(item);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.Dock = DockStyle.Fill;
		panelElement2.AlignmentOfContents = ContentAlignment.MiddleLeft;
		C1.Win.C1Tile.TextElement textElement = new C1.Win.C1Tile.TextElement();
		textElement.SingleLine = false;
		panelElement2.Children.Add(textElement);
		template.Elements.Add(panelElement);
		template.Elements.Add(panelElement2);
		return template;
	}

	private Template CreateTitleTemplate()
	{
		Template template = new Template();
		template.Description = "Subgroup";
		PanelElement panelElement = new PanelElement();
		panelElement.AlignmentOfContents = ContentAlignment.BottomLeft;
		PanelElement panelElement2 = new PanelElement();
		panelElement2.BackColor = Color.FromArgb(0, 73, 92);
		panelElement2.Dock = DockStyle.Bottom;
		panelElement2.FixedHeight = 1;
		C1.Win.C1Tile.TextElement textElement = new C1.Win.C1Tile.TextElement();
		textElement.ForeColor = Color.Black;
		textElement.ForeColorSelector = ForeColorSelector.Unbound;
		textElement.Font = new Font("微软雅黑", 9f, FontStyle.Regular);
		textElement.Margin = new Padding(0, 0, 0, 6);
		textElement.SingleLine = true;
		panelElement.Children.Add(panelElement2);
		panelElement.Children.Add(textElement);
		panelElement.Dock = DockStyle.Fill;
		panelElement.Padding = new Padding(8, 0, 8, 8);
		template.Elements.Add(panelElement);
		template.Enabled = false;
		template.Name = "subgroupTemplate";
		return template;
	}

	private Template CreateFuncTemplate()
	{
		Template template = new Template();
		PanelElement panelElement = new PanelElement();
		panelElement.Dock = DockStyle.Bottom;
		panelElement.AlignmentOfContents = ContentAlignment.TopLeft;
		panelElement.FixedHeight = 125;
		C1.Win.C1Tile.TextElement textElement = new C1.Win.C1Tile.TextElement();
		textElement.SingleLine = false;
		textElement.Margin = new Padding(10, 0, 10, 0);
		textElement.TextSelector = TextSelector.Text2;
		panelElement.Children.Add(textElement);
		template.Elements.Add(panelElement);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.Dock = DockStyle.Left;
		panelElement2.AlignmentOfContents = ContentAlignment.MiddleCenter;
		panelElement2.FixedWidth = 60;
		C1.Win.C1Tile.ImageElement item = new C1.Win.C1Tile.ImageElement();
		panelElement2.Children.Add(item);
		template.Elements.Add(panelElement2);
		PanelElement panelElement3 = new PanelElement();
		panelElement3.Dock = DockStyle.Fill;
		panelElement3.AlignmentOfContents = ContentAlignment.MiddleLeft;
		C1.Win.C1Tile.TextElement textElement2 = new C1.Win.C1Tile.TextElement();
		textElement2.SingleLine = false;
		textElement2.FontSize = 11f;
		textElement2.FontBold = ThreeStateBoolean.True;
		textElement2.TextSelector = TextSelector.Text1;
		textElement2.ForeColorSelector = ForeColorSelector.ForeColor1;
		panelElement3.Children.Add(textElement2);
		template.Elements.Add(panelElement3);
		return template;
	}

	private async void toolCmdCreateTeam_Click(object sender, ClickEventArgs e)
	{
		await CreateTeam();
	}

	private async void dlgTeamSelector_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall && e.CloseReason == CloseReason.UserClosing && !PreventExit)
		{
			await Program.Logout();
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
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlToolbar = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.toolbar = new C1.Win.C1Command.C1ToolBar();
		this.toolLnkCreateTeam = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdCreateTeam = new C1.Win.C1Command.C1Command();
		this.toolLnkRefresh = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdRefresh = new C1.Win.C1Command.C1Command();
		this.toolLnkExitTeam = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdExitTeam = new C1.Win.C1Command.C1Command();
		this.toollnkSearchTeam = new C1.Win.C1Command.C1CommandLink();
		this.toolCmdSearchTeam = new C1.Win.C1Command.C1Command();
		this.pnlTeamList = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.toolCmdOpenTeam = new C1.Win.C1Command.C1Command();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		this.pnlSearch = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.pnlToolbar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		base.SuspendLayout();
		this.c1SplitContainer1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1SplitContainer1.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.c1SplitContainer1.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.c1SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1SplitContainer1.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.c1SplitContainer1.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.c1SplitContainer1.Location = new System.Drawing.Point(0, 0);
		this.c1SplitContainer1.Name = "c1SplitContainer1";
		this.c1SplitContainer1.Panels.Add(this.pnlToolbar);
		this.c1SplitContainer1.Panels.Add(this.pnlSearch);
		this.c1SplitContainer1.Panels.Add(this.pnlTeamList);
		this.c1SplitContainer1.Size = new System.Drawing.Size(792, 519);
		this.c1SplitContainer1.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.c1SplitContainer1.TabIndex = 0;
		this.c1SplitContainer1.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlToolbar.Controls.Add(this.toolbar);
		this.pnlToolbar.Height = 67;
		this.pnlToolbar.KeepRelativeSize = false;
		this.pnlToolbar.Location = new System.Drawing.Point(0, 0);
		this.pnlToolbar.Name = "pnlToolbar";
		this.pnlToolbar.Resizable = false;
		this.pnlToolbar.Size = new System.Drawing.Size(792, 67);
		this.pnlToolbar.SizeRatio = 16.834;
		this.pnlToolbar.TabIndex = 0;
		this.toolbar.AccessibleName = "Tool Bar";
		this.toolbar.AutoSize = false;
		this.toolbar.ButtonLayoutHorz = C1.Win.C1Command.ButtonLayoutEnum.TextBelow;
		this.toolbar.ButtonLookHorz = C1.Win.C1Command.ButtonLookFlags.TextAndImage;
		this.toolbar.CommandHolder = null;
		this.toolbar.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[4] { this.toolLnkCreateTeam, this.toolLnkRefresh, this.toolLnkExitTeam, this.toollnkSearchTeam });
		this.toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
		this.toolbar.Location = new System.Drawing.Point(0, 0);
		this.toolbar.MinButtonSize = 42;
		this.toolbar.Movable = false;
		this.toolbar.Name = "toolbar";
		this.toolbar.Size = new System.Drawing.Size(792, 67);
		this.toolbar.Text = "c1ToolBar1";
		this.toolbar.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.toolbar.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.toolLnkCreateTeam.Command = this.toolCmdCreateTeam;
		this.toolCmdCreateTeam.Image = Leqisoft.UI.Platform.Properties.Resources.toolCreateTeam;
		this.toolCmdCreateTeam.Name = "toolCmdCreateTeam";
		this.toolCmdCreateTeam.ShortcutText = "";
		this.toolCmdCreateTeam.Text = "创建组织";
		this.toolCmdCreateTeam.Click += new C1.Win.C1Command.ClickEventHandler(toolCmdCreateTeam_Click);
		this.toolCmdRefresh.Image = Leqisoft.UI.Platform.Properties.Resources.RefreshProject;
		this.toolCmdRefresh.Name = "toolCmdRefresh";
		this.toolCmdRefresh.ShortcutText = "";
		this.toolCmdRefresh.Text = "刷新组织";
		this.toolCmdRefresh.Click += new C1.Win.C1Command.ClickEventHandler(toolCmdRefresh_Click);
		this.toolLnkRefresh.Command = this.toolCmdRefresh;
		this.toolLnkRefresh.SortOrder = 1;
		this.toolCmdSearchTeam.Image = Leqisoft.UI.Platform.Properties.Resources.SearchTeam;
		this.toolCmdSearchTeam.Name = "toolCmdSearchTeam";
		this.toolCmdSearchTeam.ShortcutText = "";
		this.toolCmdSearchTeam.Text = "搜索组织";
		this.toollnkSearchTeam.Command = this.toolCmdSearchTeam;
		this.toollnkSearchTeam.SortOrder = 2;
		this.toolCmdExitTeam.Image = Leqisoft.UI.Platform.Properties.Resources.toolQuitTeam;
		this.toolCmdExitTeam.Name = "toolCmdExitTeam";
		this.toolCmdExitTeam.ShortcutText = "";
		this.toolCmdExitTeam.Text = "退出组织";
		this.toolCmdExitTeam.Click += new C1.Win.C1Command.ClickEventHandler(toolCmdExitTeam_Click);
		this.toolLnkExitTeam.Command = this.toolCmdExitTeam;
		this.toolLnkExitTeam.SortOrder = 3;
		this.pnlTeamList.Height = 425;
		this.pnlTeamList.Location = new System.Drawing.Point(0, 94);
		this.pnlTeamList.Name = "pnlTeamList";
		this.pnlTeamList.Size = new System.Drawing.Size(792, 425);
		this.pnlTeamList.TabIndex = 2;
		this.toolCmdOpenTeam.Image = Leqisoft.UI.Platform.Properties.Resources.toolOpenTeam;
		this.toolCmdOpenTeam.Name = "toolCmdOpenTeam";
		this.toolCmdOpenTeam.ShortcutText = "";
		this.toolCmdOpenTeam.Text = "进入组织";
		this.toolCmdOpenTeam.Click += new C1.Win.C1Command.ClickEventHandler(toolCmdOpenTeam_Click);
		this.c1CommandHolder1.Commands.Add(this.toolCmdOpenTeam);
		this.c1CommandHolder1.Commands.Add(this.toolCmdRefresh);
		this.c1CommandHolder1.Commands.Add(this.toolCmdCreateTeam);
		this.c1CommandHolder1.Commands.Add(this.toolCmdSearchTeam);
		this.c1CommandHolder1.Commands.Add(this.toolCmdExitTeam);
		this.c1CommandHolder1.Owner = this;
		this.pnlSearch.Height = 25;
		this.pnlSearch.KeepRelativeSize = false;
		this.pnlSearch.Location = new System.Drawing.Point(0, 68);
		this.pnlSearch.MinHeight = 25;
		this.pnlSearch.Name = "pnlSearch";
		this.pnlSearch.Resizable = false;
		this.pnlSearch.Size = new System.Drawing.Size(792, 25);
		this.pnlSearch.SizeRatio = 5.556;
		this.pnlSearch.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(792, 519);
		base.Controls.Add(this.c1SplitContainer1);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "dlgTeamSelector";
		this.Text = "创建或选择组织";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(dlgTeamSelector_FormClosing);
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.pnlToolbar.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		base.ResumeLayout(false);
	}
}
