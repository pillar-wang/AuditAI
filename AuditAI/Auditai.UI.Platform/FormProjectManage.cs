﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Model;
using Auditai.PlatformResource;
using Auditai.SignalR;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using Auditai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Platform;

/// <summary>项目卡片流分组方式（随 DisplayStyle JSON 持久化）。</summary>
public enum CardGroupMode
{
	Recent,
	Category,
	CreateMonth,
	Creator,
	None
}

public class FormProjectManage : ISetTheme
{
	private enum ViewState
	{
		None,
		Project,
		Template,
		RecycleProject,
		RecycleTemplate,
		Review,
		Archive,
		Validation
	}

	private class DisplayStyle
	{
		public int Height { get; set; } = 600;


		public int Width { get; set; } = 1000;


		public FormWindowState WindowState { get; set; }

		public ListTileViewMode ViewMode { get; set; } = ListTileViewMode.Tile;


		public SortKind SortKind { get; set; } = SortKind.OpenTime;

		public CardGroupMode CardGroup { get; set; } = CardGroupMode.Recent;

		public void Load(string config)
		{
			if (!File.Exists(config))
			{
				return;
			}
			try
			{
				string value = File.ReadAllText(config);
				JsonConvert.PopulateObject(value, this);
			}
			catch (Exception)
			{
			}
		}

		public void Save(string config)
		{
			try
			{
				string directoryName = Path.GetDirectoryName(config);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				string contents = JsonConvert.SerializeObject(this);
				File.WriteAllText(config, contents);
			}
			catch (IOException)
			{
			}
		}
	}

	private const string TAB_PROJECT = "TAB_PROJECT";

	private const string TAB_TEMPLATE = "TAB_TEMPLATE";

	private const string TAB_RECYCLEPROJECT = "TAB_RECYCLEPROJECT";

	private const string TAB_RECYCLETEMPLATE = "TAB_RECYCLETEMPLATE";

	private const string RG_PROJECT = "RG_PROJECT";

	private const string RG_USERMANAGE = "RG_USERMANAGE";

	private const string RG_TEMPLATE = "RG_TEMPLATE";

	private const string RG_RECYCLE = "RG_RECYCLE";

	private const string RG_VIEWMODE = "RG_VIEWMODE";

	private const string RB_CREATEPROJECT = "RB_CREATEPROJECT";

	private const string RB_OPENPROJECT = "RB_OPENPROJECT";

	private const string RB_MODIFYPROJECT = "RB_MODIFYPROJECT";

	private const string RB_DELETEPROJECT = "RB_DELETEPROJECT";

	private const string RB_DUPLICATEPROJECT = "RB_DUPLICATEPROJECT";

	private const string RB_EXPORTPROJECT = "RB_EXPORTPROJECT";

	private const string RB_EXPORTPROJECTFILE = "RB_EXPORTPROJECTFILE";

	private const string RB_IMPORTPROJECT = "RB_IMPORTPROJECT";

	private const string RB_SAVEASTEMPLATE = "RB_SAVEASTEMPLATE";

	private const string RB_SHAREPROJECT = "RB_SHAREPROJECT";

	private const string RB_VIEWMODE = "RB_VIEWMODE";

	private const string IL_TILEMODE = "IL_TILEMODE";

	private const string IL_LISTMODE = "IL_LISTMODE";

	private const string RB_REFRESHPROJECT = "RB_REFRESHPROJECT";

	private const string RB_USERMANAGE = "RB_USERMANAGE";

	private const string RB_USERINFO = "RB_USERINFO";

	private const string RB_CHANGEPASSWORD = "RB_CHANGEPASSWORD";

	private const string RB_USETEMPLATE = "RB_USETEMPLATE";

	private const string RB_CREATETEMPLATE = "RB_CREATETEMPLATE";

	private const string RB_OPENTEMPLATE = "RB_OPENTEMPLATE";

	private const string RB_MODIFYTEMPLATE = "RB_MODIFYTEMPLATE";

	private const string RB_DELETETEMPLATE = "RB_DELETETEMPLATE";

	private const string RB_DUPLICATETEMPLATE = "RB_DUPLICATETEMPLATE";

	private const string RB_EXPORTTEMPLATE = "RB_EXPORTTEMPLATE";

	private const string RB_IMPORTTEMPLATE = "RB_IMPORTTEMPLATE";

	private const string RB_REFRESHTEMPLATE = "RB_REFRESHTEMPLATE";

	private const string RB_SEARCH = "RB_SEARCH";

	private const string RB_SHARETEMPLATE = "RB_SHARETEMPLATE";

	private const string RB_EMPTYRECYCLE = "RB_EMPTYRECYCLE";

	private const string RB_DELETESELECT = "RB_DELETESELECT";

	private const string RB_RESTORESELECT = "RB_RESTORESELECT";

	private const string TAB_REVIEW = "TAB_REVIEW";

	private const string TAB_ARCHIVE = "TAB_ARCHIVE";

	private const string TAB_VALIDATION = "TAB_VALIDATION";

	private const string RG_REVIEW = "RG_REVIEW";

	private const string RG_ARCHIVE = "RG_ARCHIVE";

	private const string RG_VALIDATION = "RG_VALIDATION";

	private const string RG_VALRULES = "RG_VALRULES";

	private const string RG_REVIEWFILTER = "RG_REVIEWFILTER";

	private const string RG_ARCHIVEFILTER = "RG_ARCHIVEFILTER";

	private const string RB_SUBMITREVIEW = "RB_SUBMITREVIEW";

	private const string RB_REVIEWTEMPLATES = "RB_REVIEWTEMPLATES";

	private const string RB_APPROVEREVIEW = "RB_APPROVEREVIEW";

	private const string RB_REJECTREVIEW = "RB_REJECTREVIEW";

	private const string RB_OPENREADONLY = "RB_OPENREADONLY";

	private const string RB_REVIEWHISTORY = "RB_REVIEWHISTORY";

	private const string RB_WITHDRAWREVIEW = "RB_WITHDRAWREVIEW";

	private const string RB_REFRESHREVIEW = "RB_REFRESHREVIEW";

	private const string RB_REVIEWFILTERPENDING = "RB_REVIEWFILTERPENDING";

	private const string RB_REVIEWFILTERMINE = "RB_REVIEWFILTERMINE";

	private const string RB_REVIEWFILTERALL = "RB_REVIEWFILTERALL";

	private const string RB_ARCHIVENOW = "RB_ARCHIVENOW";

	private const string RB_UNARCHIVE = "RB_UNARCHIVE";

	private const string RB_EXPORTARCHIVE = "RB_EXPORTARCHIVE";

	private const string RB_REFRESHARCHIVE = "RB_REFRESHARCHIVE";

	private const string RB_ARCHIVEFILTERARCHIVED = "RB_ARCHIVEFILTERARCHIVED";

	private const string RB_ARCHIVEFILTERCANDIDATES = "RB_ARCHIVEFILTERCANDIDATES";

	private const string RB_RUNVALIDATION = "RB_RUNVALIDATION";

	private const string RB_EXPORTREPORT = "RB_EXPORTREPORT";

	private const string RB_REFRESHVALIDATION = "RB_REFRESHVALIDATION";

	private const string RB_ADDRULE = "RB_ADDRULE";

	private const string RB_EDITRULE = "RB_EDITRULE";

	private const string RB_DELETERULE = "RB_DELETERULE";

	private const string RB_IMPORTRULES = "RB_IMPORTRULES";

	private const string RB_EXPORTRULES = "RB_EXPORTRULES";

	private const string CN_CHECK = "CN_CHECK";

	private const string CN_PROJNUM = "CN_PROJNUM";

	private const string CN_PROJNAME = "CN_PROJNAME";

	private const string CN_PROJCAT = "CN_PROJCAT";

	private const string CN_PROJLEADER = "CN_PROJLEADER";

	private const string CN_PROJASSIST = "CN_PROJASSIST";

	private const string CN_PROJCHECK = "CN_PROJCHECK";

	private const string CN_PROJNOTE = "CN_PROJNOTE";

	private const string CN_PROJAUDITEE = "CN_PROJAUDITEE";

	private const string CN_CREATOR = "CN_CREATOR";

	private const string CN_TMPLEDITOR = "CN_TMPLEDITOR";

	private const string CN_TMPLUSER = "CN_TMPLUSER";

	private const string CMD_OPENFROMSERVER = "CMD_OPENFROMSERVER";

	private const string CMD_SORT = "CMD_SORT";

	private const string CMD_SORTCREATETIME = "CMD_SORTCREATETIME";

	private const string CMD_SORTOPENTIME = "CMD_SORTOPENTIME";

	private const string CMD_SORTNUMBER = "CMD_SORTNUMBER";

	private const string CMD_SORTNAME = "CMD_SORTNAME";

	private const string CMD_GROUP = "CMD_GROUP";

	private const string CMD_GROUPRECENT = "CMD_GROUPRECENT";

	private const string CMD_GROUPCATEGORY = "CMD_GROUPCATEGORY";

	private const string CMD_GROUPCREATEMONTH = "CMD_GROUPCREATEMONTH";

	private const string CMD_GROUPCREATOR = "CMD_GROUPCREATOR";

	private const string CMD_GROUPNONE = "CMD_GROUPNONE";

	private const string CMD_CATEGORY = "CMD_CATEGORY";

	private const string CTX_CMD_OPEN_PROJECT = "CTX_CMD_OPEN_PROJECT";

	private const string CTX_CMD_RENAME_PROJECT = "CTX_CMD_RENAME_PROJECT";

	private const string CTX_CMD_DELETE_PROJECT = "CTX_CMD_DELETE_PROJECT";

	private const string CTX_CMD_DUPLICATE_PROJECT = "CTX_CMD_DUPLICATE_PROJECT";

	private const string CTX_CMD_EXPORT_PROJECT = "CTX_CMD_EXPORT_PROJECT";

	private readonly C1RibbonForm _form;

	private readonly SideCommandBar _sidebar;

	private readonly TopCommandBar _topBar;

	private readonly C1SplitContainer _ctn;

	private readonly C1FlexGridEx _grid;

	private readonly ProjectCardFlow _cardFlow;

	private readonly ProjectFavoritesBar _favBar;

	private readonly KpiStrip _kpiStrip;

	/// <summary>KPI 筛选标识（null=无筛选；all/newmonth/reviewing/archived/expiring）</summary>
	private string _kpiFilterTag;

	/// <summary>列表视图当前悬停行号（-1=无；MouseMove/Leave 维护，Paint 叠加淡色高亮）</summary>
	private int _hoverRow = -1;

	/// <summary>状态列药丸字体（微软雅黑 8.25f，静态常驻）</summary>
	private static readonly Font StatusPillFont = new Font("微软雅黑", 8.25f);

	/// <summary>右键/更多菜单命中的卡片（ProjectCardFlow 未提供编程选中 API，经 SelectedProject/SelectedProjects 回退生效，菜单关闭后清空）</summary>
	private ProjectCardItem _ctxCardItem;

	/// <summary>卡片流与收藏行选中互斥同步的保护标志（防 SelectionChanged 递归）</summary>
	private bool _syncingCardSelection;

	private readonly C1ContextMenu _ctx;

	private readonly C1ContextMenu _ctxShowMore;

	/// <summary>原生右键/更多菜单（C1Command 弹出菜单窗口不支持 PerMonitorV2，高 DPI 下文字被位图拉伸发虚）。</summary>
	private ContextMenuStrip _nativeCtxMenu;

	/// <summary>原生菜单"类别"子菜单父项（菜单关闭时按勾选状态保存类别）。</summary>
	private ToolStripMenuItem _nativeCategoryItem;

	/// <summary>原生菜单"类别"勾选发生过变化（菜单关闭时需要保存）。</summary>
	private bool _nativeCategoryChanged;

	private readonly C1CommandHolder _cmdh;

	private readonly C1SplitterPanel _pnlSearch;

	private readonly C1TextBoxEx _txbSearch;

	private readonly System.Windows.Forms.Button _btnCloseSearch;

	private ViewState State;

	private readonly List<Auditai.DTO.Project> _projects = new List<Auditai.DTO.Project>();

	/// <summary>当前视图存在的分类名列表（用于单项目类别右键菜单）。</summary>
	private readonly List<string> KnownCategories = new List<string>();

	/// <summary>已勾选的具名分类（分类筛选合并进 KPI 条后由复选状态维护）。</summary>
	private readonly HashSet<string> _selectedCategories = new HashSet<string>(StringComparer.Ordinal);

	/// <summary>是否勾选"无分类"筛选。</summary>
	private bool _categoryEmptyChecked;

	/// <summary>分类筛选项 Tag 前缀（分隔真实分类与统计项）。</summary>
	private const string CategoryTagPrefix = "cat:";

	private const string CategoryTagEmpty = "cat:__empty__";

	private readonly DisplayStyle Style = new DisplayStyle();

	private readonly C1CommandLink _lnkNull;

	private bool _noAllowReentry;

	private bool _isClosing = true;

	private bool _isSearch;

	private bool _anyMenuItemClicked;

	private bool _isShowProjectPayStatusIcon;

	protected Dictionary<string, C1Command> _commandDic = new Dictionary<string, C1Command>(StringComparer.OrdinalIgnoreCase);

	private bool _isInOpenProject;

	private readonly ListView _lvReview;

	private readonly ListView _lvArchive;

	private readonly ListView _lvValidation;

	private List<ReviewSubmissionDto> _reviewSubmissions = new List<ReviewSubmissionDto>();

	private string _reviewScope = "pending";

	private List<ProjectArchiveDto> _archiveRecords = new List<ProjectArchiveDto>();

	private List<Auditai.DTO.Project> _archiveCandidates = new List<Auditai.DTO.Project>();

	private bool _archiveShowCandidates;

	/// <summary>稽核检查页数据访问器（按当前选中项目懒创建）</summary>
	private ValidationRuleStore _validationStore;

	/// <summary>稽核检查页当前规则清单</summary>
	private ValidationRuleStore.RuleSet _validationRules;

	/// <summary>最近一次一键校验的结果（null 表示尚未执行）</summary>
	private List<ValidationRuleStore.ValidationOutcome> _validationOutcomes;

	/// <summary>稽核检查页当前绑定的项目 Id（切换项目时重置会话）</summary>
	private Guid _validationProjectId = Guid.Empty;

	public Auditai.DTO.Project SelectedProject
	{
		get
		{
			if (Style.ViewMode == ListTileViewMode.List)
			{
				if (_grid.BodyRow >= 0 && _grid.BodyRow < _grid.BodyRowsCount)
				{
					return _grid.BodyGetRow(_grid.BodyRow).UserData as Auditai.DTO.Project;
				}
				return null;
			}
			return _cardFlow.SelectedProject ?? _favBar.SelectedProject ?? _ctxCardItem?.Project;
		}
	}

	public List<Auditai.DTO.Project> SelectedProjects
	{
		get
		{
			List<Auditai.DTO.Project> list = new List<Auditai.DTO.Project>();
			if (Style.ViewMode == ListTileViewMode.List)
			{
				for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
				{
					if (_grid.GetCellCheck(i, _grid.Cols.IndexOf("CN_CHECK")) == CheckEnum.Checked)
					{
						list.Add(_grid.Rows[i].UserData as Auditai.DTO.Project);
					}
				}
			}
			else
			{
				List<Auditai.DTO.Project> cardSelected = _cardFlow.SelectedProjects;
				if (cardSelected.Count > 0)
				{
					list.AddRange(cardSelected);
				}
				else if (_ctxCardItem?.Project != null)
				{
					list.Add(_ctxCardItem.Project);
				}
			}
			return list;
		}
	}

	public bool HasSelectedProject => SelectedProject != null;

	private System.Drawing.Image ProjectTileImage => Program.MainForm.CurrentEdition.ProjectTileIcon;

	private System.Drawing.Image SystemTemplateTileImage => Program.MainForm.CurrentEdition.SystemTemplateTileIcon;

	private System.Drawing.Image VipSystemTemplateTileImage => Program.MainForm.CurrentEdition.VipSystemTemplateTileIcon;

	private System.Drawing.Image CustomTemplateTileImage => Program.MainForm.CurrentEdition.CustomTemplateTileIcon;

	public FormProjectManage()
	{
		Style.Load(ConfigManager.PROJECTMANAGEMENT_VIEWCONFIG);
		_form = FormFactory.Create();
		_form.WindowState = Style.WindowState;
		_form.Size = new Size(Style.Width, Style.Height);
		_form.Text = StringConstBase.Current.Project + "管理";
		_form.Shown += _form_Shown;
		_form.FormClosed += _form_FormClosed;
		_form.Resize += _form_Resize;
		_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.Projects16, IconRes.Projects24);
		_ctn = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_pnlSearch = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Resizable = false,
			KeepRelativeSize = false,
			MinHeight = 0,
			Height = 25
		};
		_ctn.Panels.Add(_pnlSearch);
		_btnCloseSearch = new System.Windows.Forms.Button
		{
			Image = IconRes.close2,
			Text = "",
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Dock = DockStyle.Right
		};
		// 关闭按钮使用最原始样式：透明底灰色 ×（不套用 C1Button 深色默认外观）
		_btnCloseSearch.FlatStyle = FlatStyle.Flat;
		_btnCloseSearch.BackColor = Color.White;
		_btnCloseSearch.UseVisualStyleBackColor = false;
		
		_btnCloseSearch.FlatAppearance.BorderSize = 0;
		_btnCloseSearch.Click += _btnCloseSearch_Click;
		_pnlSearch.Controls.Add(_btnCloseSearch);
		_txbSearch = new C1TextBoxEx
		{
			Dock = DockStyle.Fill,
			AutoSize = false
		};
		_txbSearch.TextChanged += _txbSearch_TextChanged;
		_pnlSearch.Controls.Add(_txbSearch);
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Resizable = false,
			KeepRelativeSize = true
		};
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowResizing = AllowResizingEnum.Both,
			AllowSorting = AllowSortingEnum.None,
			ExtendLastCol = true,
			SelectionMode = SelectionModeEnum.Row,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None
		};
		_grid.Cols.Count = 0;
		_grid.Cols.Fixed = 0;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
		_grid.MouseDoubleClick += _grid_MouseDoubleClick;
		_grid.BodySelectionChanged += _grid_BodySelectionChanged;
		_grid.MouseDown += _grid_MouseDown;
		_grid.CellChecked += _grid_CellChecked;
		_grid.Paint += _grid_Paint;
		_grid.Resize += _grid_Resize;
		_grid.MouseMove += _grid_MouseMove;
		_grid.MouseLeave += _grid_MouseLeave;
		c1SplitterPanel.Controls.Add(_grid);
		_cardFlow = new ProjectCardFlow
		{
			Dock = DockStyle.Fill,
			Visible = false,
			IconResolver = ResolveCardIcon,
			TintResolver = ResolveCardTint,
			PayBadgeResolver = ResolveCardPayBadge,
			IsFavoriteEnabled = true
		};
		_cardFlow.ItemActivated += CardFlow_ItemActivated;
		_cardFlow.ItemRightClicked += Card_ItemRightClicked;
		_cardFlow.ItemMoreClicked += Card_ItemMoreClicked;
		_cardFlow.ItemStarClicked += ToggleFavorite;
		_cardFlow.SelectionChanged += CardFlow_SelectionChanged;
		c1SplitterPanel.Controls.Add(_cardFlow);
		_favBar = new ProjectFavoritesBar
		{
			Dock = DockStyle.Top,
			Visible = false,
			IconResolver = ResolveCardIcon,
			TintResolver = ResolveCardTint,
			PayBadgeResolver = ResolveCardPayBadge,
			IsFavoriteEnabled = true
		};
		_favBar.ItemActivated += FavBar_ItemActivated;
		_favBar.ItemRightClicked += FavBar_ItemRightClicked;
		_favBar.ItemMoreClicked += FavBar_ItemMoreClicked;
		_favBar.ItemStarClicked += ToggleFavorite;
		_favBar.SelectionChanged += FavBar_SelectionChanged;
		c1SplitterPanel.Controls.Add(_favBar);
		_kpiStrip = new KpiStrip
		{
			Dock = DockStyle.Top,
			Visible = false
		};
		_kpiStrip.ItemClicked += KpiStrip_ItemClicked;
		c1SplitterPanel.Controls.Add(_kpiStrip);
		_lvReview = new ListView
		{
			Dock = DockStyle.Fill,
			View = View.Details,
			FullRowSelect = true,
			MultiSelect = false,
			HideSelection = false,
			BorderStyle = BorderStyle.None,
			Font = new Font("微软雅黑", 9f),
			Visible = false
		};
		_lvReview.SelectedIndexChanged += delegate
		{
			SetCommandState();
		};
		_lvReview.DoubleClick += async delegate
		{
			await ReviewRowDoubleClick();
		};
		// SmallImageList 1×36 抬高 Details 行高到 36px；Resize 时按比例重排列宽
		_lvReview.SmallImageList = new ImageList
		{
			ImageSize = new Size(1, 36)
		};
		_lvReview.Resize += delegate
		{
			AutoSizeLvColumns(_lvReview, ReviewColumnRatios);
		};
		c1SplitterPanel.Controls.Add(_lvReview);
		_lvArchive = new ListView
		{
			Dock = DockStyle.Fill,
			View = View.Details,
			FullRowSelect = true,
			MultiSelect = false,
			HideSelection = false,
			BorderStyle = BorderStyle.None,
			Font = new Font("微软雅黑", 9f),
			Visible = false
		};
		_lvArchive.SelectedIndexChanged += delegate
		{
			SetCommandState();
		};
		_lvArchive.DoubleClick += async delegate
		{
			await ArchiveRowDoubleClick();
		};
		_lvArchive.SmallImageList = new ImageList
		{
			ImageSize = new Size(1, 36)
		};
		_lvArchive.Resize += delegate
		{
			AutoSizeLvColumns(_lvArchive, _archiveShowCandidates ? ArchiveCandidateColumnRatios : ArchiveRecordColumnRatios);
		};
		c1SplitterPanel.Controls.Add(_lvArchive);
		_lvValidation = new ListView
		{
			Dock = DockStyle.Fill,
			View = View.Details,
			FullRowSelect = true,
			MultiSelect = true,
			HideSelection = false,
			BorderStyle = BorderStyle.None,
			Font = new Font("微软雅黑", 9f),
			Visible = false,
			ShowGroups = true
		};
		_lvValidation.SelectedIndexChanged += delegate
		{
			SetCommandState();
		};
		_lvValidation.DoubleClick += delegate
		{
			EditSelectedValidationRule();
		};
		c1SplitterPanel.Controls.Add(_lvValidation);
		_ctn.Panels.Add(c1SplitterPanel);
		_form.Controls.Add(_ctn);
		_sidebar = new SideCommandBar();
		_sidebar.ModuleSelected += Sidebar_ModuleSelected;
		_topBar = new TopCommandBar();
		InitializeSideBar();
		_form.Controls.Add(_topBar);
		_form.Controls.Add(_sidebar);
		_cmdh = C1CommandHolder.CreateCommandHolder(_form);
		_lnkNull = new C1CommandLink();
		_cmdh.CommandClick += _cmdh_CommandClick;
		_ctx = new C1ContextMenu();
		// 命令仍注册在 _ctx 上（复用全部状态刷新与点击分发逻辑），
		// 展示改走 ShowNativeContextMenu 的原生 ContextMenuStrip（高 DPI 文字清晰）
		_ctx.Popup += _ctx_Popup;
		_ctx.Closed += _ctx_Closed;
		_ctxShowMore = _ctx;
		_ctxShowMore.Popup += _ctxShowMore_Popup;
		AddCommandWithImage("CTX_CMD_OPEN_PROJECT", "打开" + StringConstBase.Current.Project, _ctxShowMore, IconRes.TicketNavTreeListExpanded);
		AddCommandWithImage("CTX_CMD_RENAME_PROJECT", (Program.MainForm.CurrentEdition is AppEditionGeneral) ? ("重命名" + StringConstBase.Current.Project) : ("修改" + StringConstBase.Current.Project), _ctxShowMore, IconRes.ctxMofify);
		AddCommandWithImage("CTX_CMD_DELETE_PROJECT", "删除" + StringConstBase.Current.Project, _ctxShowMore, IconRes.RemoveProject16);
		AddCommandWithImage("CTX_CMD_DUPLICATE_PROJECT", "复制" + StringConstBase.Current.Project, _ctxShowMore, IconRes.ctxCopy);
		AddCommandWithImage("CTX_CMD_EXPORT_PROJECT", "导出" + StringConstBase.Current.Project, _ctxShowMore, IconRes.BatchExport16);
		AddCommandWithDelimiter("CMD_OPENFROMSERVER", "全新打开" + StringConstBase.Current.Project, _ctx);
		C1CommandMenu c1CommandMenu = AddCommandMenu("CMD_CATEGORY", StringConstBase.Current.Project + "类别", _ctx);
		c1CommandMenu.CloseOnItemClick = false;
		c1CommandMenu.Popup += Menu_Popup;
		c1CommandMenu.CommandLinks.Add(_lnkNull);
		c1CommandMenu = AddCommandMenu("CMD_SORT", "排序方式", _ctx);
		AddCommand("CMD_SORTCREATETIME", "创建时间", c1CommandMenu);
		AddCommand("CMD_SORTOPENTIME", "打开时间", c1CommandMenu);
		AddCommand("CMD_SORTNUMBER", "项目编号", c1CommandMenu);
		AddCommand("CMD_SORTNAME", "项目名称", c1CommandMenu);
		C1CommandMenu groupMenu = AddCommandMenu(CMD_GROUP, "分组方式", c1CommandMenu);
		AddCommand(CMD_GROUPRECENT, "最近使用", groupMenu);
		AddCommand(CMD_GROUPCATEGORY, "按类别", groupMenu);
		AddCommand(CMD_GROUPCREATEMONTH, "按创建月份", groupMenu);
		AddCommand(CMD_GROUPCREATOR, "按创建人", groupMenu);
		AddCommand(CMD_GROUPNONE, "不分组", groupMenu);
		C1Command AddCommand(string name, string text, C1CommandMenu menu)
		{
			C1Command c1Command = _cmdh.CreateCommand();
			c1Command.Name = name;
			c1Command.Text = text;
			menu.CommandLinks.Add(new C1CommandLink(c1Command));
			return c1Command;
		}
		C1CommandMenu AddCommandMenu(string name, string text, C1CommandMenu menu)
		{
			C1CommandMenu c1CommandMenu2 = new C1CommandMenu
			{
				Name = name,
				Text = text
			};
			menu.CommandLinks.Add(new C1CommandLink(c1CommandMenu2));
			_cmdh.Commands.Add(c1CommandMenu2);
			return c1CommandMenu2;
		}
		C1Command AddCommandWithDelimiter(string name, string text, C1CommandMenu menu)
		{
			C1Command c1Command2 = _cmdh.CreateCommand();
			c1Command2.Name = name;
			c1Command2.Text = text;
			menu.CommandLinks.Add(new C1CommandLink(c1Command2)
			{
				Delimiter = true
			});
			return c1Command2;
		}
		C1Command AddCommandWithImage(string name, string text, C1CommandMenu menu, System.Drawing.Image commandImage)
		{
			C1Command ret = new C1Command();
			ret.Name = name;
			ret.Text = text;
			ret.Image = commandImage;
			ret.Click += delegate(object s1, ClickEventArgs e1)
			{
				_cmdh_CommandClick(s1, new CommandClickEventArgs(ret, e1));
			};
			menu.CommandLinks.Add(new C1CommandLink(ret));
			_commandDic[name] = ret;
			return ret;
		}
	}

	/// <summary>注册侧边栏 4 个模块与全部命令（沿用原 Ribbon 页签的组归属与可见性规则）。</summary>
	private void InitializeSideBar()
	{
		EventHandler onClick = CommandBar_Click;
		bool isGeneral = Program.MainForm.CurrentEdition is AppEditionGeneral;
		string projectText = StringConstBase.Current.Project;
		string templateText = StringConstBase.Current.Template;
		string recycleGroupText = Auditai.Model.User.Current.IsTeamAdmin ? "恢复及删除" : "恢复";
		_sidebar.RegisterModule(TAB_PROJECT, projectText + "管理", "folders");
		_sidebar.RegisterModule(TAB_TEMPLATE, templateText + "管理", "browsers");
		_sidebar.RegisterModule(TAB_VALIDATION, "稽核检查", "shield-check");
		_sidebar.RegisterModule(TAB_RECYCLEPROJECT, projectText + "回收站", "trash");
		_sidebar.RegisterModule(TAB_RECYCLETEMPLATE, templateText + "回收站", "trash");
		_topBar.AddCommand(TAB_VALIDATION, RG_VALIDATION, RB_RUNVALIDATION, "执行校验", "check-circle", onClick, "稽核校验");
		_topBar.AddCommand(TAB_VALIDATION, RG_VALIDATION, RB_EXPORTREPORT, "导出报告", "export", onClick);
		_topBar.AddCommand(TAB_VALIDATION, RG_VALIDATION, RB_REFRESHVALIDATION, "刷新", "arrows-clockwise", onClick);
		_topBar.AddCommand(TAB_VALIDATION, RG_VALRULES, RB_ADDRULE, "新增规则", "folder-plus", onClick, "规则管理");
		_topBar.AddCommand(TAB_VALIDATION, RG_VALRULES, RB_EDITRULE, "编辑规则", "pencil", onClick);
		_topBar.AddCommand(TAB_VALIDATION, RG_VALRULES, RB_DELETERULE, "删除规则", "trash-simple", onClick);
		_topBar.AddCommand(TAB_VALIDATION, RG_VALRULES, RB_IMPORTRULES, "导入规则", "tray-arrow-down", onClick, "导入导出");
		_topBar.AddCommand(TAB_VALIDATION, RG_VALRULES, RB_EXPORTRULES, "导出规则", "export", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_CREATEPROJECT, "新建" + projectText, "folder-plus", onClick, projectText + "管理");
		_topBar.SetCommandVisible(RB_CREATEPROJECT, !isGeneral);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_OPENPROJECT, "打开" + projectText, "folder-open", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_MODIFYPROJECT, isGeneral ? ("重命名" + projectText) : ("修改" + projectText), "pencil", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_DELETEPROJECT, "删除" + projectText, "trash", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_DUPLICATEPROJECT, "复制" + projectText, "copy", onClick);
		_topBar.SetCommandVisible(RB_DUPLICATEPROJECT, !SoftwareLicenseManager.IsDuplicateProjectOutOfLicenseLimit());
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_EXPORTPROJECT, projectText + "导出", "export", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_EXPORTPROJECTFILE, "导出项目文件", "export", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_IMPORTPROJECT, "导入项目", "tray-arrow-down", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_SAVEASTEMPLATE, "另存" + templateText, "floppy-disk", onClick);
		_topBar.SetCommandVisible(RB_SAVEASTEMPLATE, !isGeneral);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_SEARCH, "搜索" + projectText, "magnifying-glass", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_SHAREPROJECT, "跨组织分享" + projectText, "share", onClick);
		_topBar.SetCommandVisible(RB_SHAREPROJECT, SoftwareLicenseManager.IsAllowShowShareProjectButton());
		_topBar.AddCommand(TAB_PROJECT, RG_PROJECT, RB_REFRESHPROJECT, "刷新" + projectText, "arrows-clockwise", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_VIEWMODE, RB_VIEWMODE, "卡片模式", "list", onClick, "视图模式");
		_topBar.AddCommand(TAB_PROJECT, RG_USERMANAGE, RB_USERMANAGE, Auditai.Model.User.Current.IsTeamAdmin ? "同事管理" : "我的同事", "users", onClick, "人员管理");
		_topBar.AddCommand(TAB_PROJECT, RG_USERMANAGE, RB_USERINFO, "用户资料", "user", onClick);
		_topBar.AddCommand(TAB_PROJECT, RG_USERMANAGE, RB_CHANGEPASSWORD, "修改密码", "key", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_USETEMPLATE, "基于" + templateText + "创建项目", "browsers", onClick, templateText + "管理");
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_CREATETEMPLATE, "新建" + templateText, "folder-plus", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_OPENTEMPLATE, "打开" + templateText, "folder-open", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_MODIFYTEMPLATE, "修改" + templateText, "pencil", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_DELETETEMPLATE, "删除" + templateText, "trash", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_DUPLICATETEMPLATE, "复制" + templateText, "copy", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_EXPORTTEMPLATE, "导出" + templateText, "export", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_IMPORTTEMPLATE, "导入" + templateText, "tray-arrow-down", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_SEARCH, "搜索" + templateText, "magnifying-glass", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_SHARETEMPLATE, "跨组织分享" + templateText, "share", onClick);
		_topBar.SetCommandVisible(RB_SHARETEMPLATE, SoftwareLicenseManager.IsAllowShowShareProjectButton());
		_topBar.AddCommand(TAB_TEMPLATE, RG_TEMPLATE, RB_REFRESHTEMPLATE, "刷新" + templateText, "arrows-clockwise", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_VIEWMODE, RB_VIEWMODE, "卡片模式", "list", onClick, "视图模式");
		_topBar.AddCommand(TAB_TEMPLATE, RG_USERMANAGE, RB_USERMANAGE, Auditai.Model.User.Current.IsTeamAdmin ? "同事管理" : "我的同事", "users", onClick, "人员管理");
		_topBar.AddCommand(TAB_TEMPLATE, RG_USERMANAGE, RB_USERINFO, "用户资料", "user", onClick);
		_topBar.AddCommand(TAB_TEMPLATE, RG_USERMANAGE, RB_CHANGEPASSWORD, "修改密码", "key", onClick);
		_topBar.AddCommand(TAB_RECYCLEPROJECT, RG_RECYCLE, RB_RESTORESELECT, "恢复所选" + projectText, "arrow-counter-clockwise", onClick, recycleGroupText);
		_topBar.AddCommand(TAB_RECYCLEPROJECT, RG_RECYCLE, RB_DELETESELECT, "删除所选" + projectText, "trash-simple", onClick);
		_topBar.SetCommandVisible(RB_DELETESELECT, Auditai.Model.User.Current.IsTeamAdmin);
		_topBar.AddCommand(TAB_RECYCLEPROJECT, RG_RECYCLE, RB_EMPTYRECYCLE, "清空回收站", "trash", onClick);
		_topBar.SetCommandVisible(RB_EMPTYRECYCLE, Auditai.Model.User.Current.IsTeamAdmin);
		_topBar.AddCommand(TAB_RECYCLEPROJECT, RG_VIEWMODE, RB_VIEWMODE, "卡片模式", "list", onClick, "视图模式");
		_topBar.AddCommand(TAB_RECYCLETEMPLATE, RG_RECYCLE, RB_RESTORESELECT, "恢复所选" + templateText, "arrow-counter-clockwise", onClick, recycleGroupText);
		_topBar.AddCommand(TAB_RECYCLETEMPLATE, RG_RECYCLE, RB_DELETESELECT, "删除所选" + templateText, "trash-simple", onClick);
		_topBar.AddCommand(TAB_RECYCLETEMPLATE, RG_RECYCLE, RB_EMPTYRECYCLE, "清空回收站", "trash", onClick);
		_topBar.AddCommand(TAB_RECYCLETEMPLATE, RG_VIEWMODE, RB_VIEWMODE, "卡片模式", "list", onClick, "视图模式");
		if (!StorageRouter.IsLocalMode)
		{
			_sidebar.RegisterModule(TAB_REVIEW, "上报审核", "paper-plane-tilt");
			_sidebar.RegisterModule(TAB_ARCHIVE, "项目归档", "archive");
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEW, RB_SUBMITREVIEW, "上报审核", "paper-plane-tilt", onClick, "上报审核");
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEW, RB_REVIEWTEMPLATES, "流程模板", "list-bullets", onClick, "审批流程模板");
			_topBar.SetCommandVisible(RB_REVIEWTEMPLATES, Auditai.Model.User.Current.IsTeamAdmin);
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEW, RB_OPENREADONLY, "打开项目(只读)", "eye", onClick);
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEW, RB_APPROVEREVIEW, "通过", "check-circle", onClick);
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEW, RB_REJECTREVIEW, "退回", "arrow-u-up-left", onClick);
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEW, RB_WITHDRAWREVIEW, "撤回", "arrow-counter-clockwise", onClick);
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEW, RB_REVIEWHISTORY, "审批记录", "clock-counter-clockwise", onClick);
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEW, RB_REFRESHREVIEW, "刷新", "arrows-clockwise", onClick);
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEWFILTER, RB_REVIEWFILTERPENDING, "待我审批", "check-circle", onClick, "筛选");
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEWFILTER, RB_REVIEWFILTERMINE, "我提交的", "paper-plane-tilt", onClick);
			_topBar.AddCommand(TAB_REVIEW, RG_REVIEWFILTER, RB_REVIEWFILTERALL, "全部审核单", "list-bullets", onClick);
			_topBar.AddCommand(TAB_ARCHIVE, RG_ARCHIVE, RB_ARCHIVENOW, "归档", "archive", onClick, "项目归档");
			_topBar.AddCommand(TAB_ARCHIVE, RG_ARCHIVE, RB_UNARCHIVE, "取消归档", "lock-open", onClick);
			_topBar.AddCommand(TAB_ARCHIVE, RG_ARCHIVE, RB_EXPORTARCHIVE, "导出归档包", "export", onClick);
			_topBar.AddCommand(TAB_ARCHIVE, RG_ARCHIVE, RB_REFRESHARCHIVE, "刷新", "arrows-clockwise", onClick);
			_topBar.AddCommand(TAB_ARCHIVE, RG_ARCHIVEFILTER, RB_ARCHIVEFILTERARCHIVED, "已归档项目", "archive", onClick, "筛选");
			_topBar.AddCommand(TAB_ARCHIVE, RG_ARCHIVEFILTER, RB_ARCHIVEFILTERCANDIDATES, "可归档项目", "folder-plus", onClick);
		}
	}

	public DialogResult ShowDialog()
	{
		return _form.ShowDialog();
	}

	public void SetTheme()
	{
		_grid.Styles.Normal.TextAlign = TextAlignEnum.LeftCenter;
		_grid.FocusRect = FocusRectEnum.None;
		_grid.Rows.DefaultSize = 40;
		_grid.Styles.SelectedColumnHeader.Clear();
		_cardFlow.ApplyTheme();
		_favBar.ApplyTheme();
		_kpiStrip.ApplyTheme();
		Color dark = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
		// 选中行淡底（DarkColor alpha 20）+ 正常文字色
		_grid.Styles.Highlight.BackColor = Color.FromArgb(20, dark);
		_grid.Styles.Highlight.ForeColor = AuditTheme.Text;
		// 审核/归档列表：白底 + 主文字色
		_lvReview.BackColor = Color.White;
		_lvReview.ForeColor = AuditTheme.Text;
		_lvArchive.BackColor = Color.White;
		_lvArchive.ForeColor = AuditTheme.Text;
		_sidebar.ApplyTheme(Theme.SelectedAuditaiTheme.ThemeContext.BackColor, Color.FromArgb(40, dark), Color.FromArgb(70, dark), Color.FromArgb(71, 85, 105));
		_topBar.ApplyTheme(Theme.SelectedAuditaiTheme.ThemeContext.BackColor, Color.FromArgb(40, dark), Color.FromArgb(70, dark), Color.FromArgb(71, 85, 105));
	}

	public async Task Populate()
	{
		// 稽核页需要读 SelectedProject（_grid 控件属性），必须在 UI 线程、FetchModel 之前完成；
		// FetchModel 的进度窗委托经 ConfigureAwait(false) 后运行在线程池，禁止在其中访问控件
		if (State == ViewState.Validation)
		{
			EnsureValidationStore();
		}
		_grid.BeginUpdate();
		await FetchModel();
		PopulateModel();
		PopulateKpi();
		_grid.EndUpdate();
		PopulateCategory();
		PopulateViewMode();
		PopulateSearch();
		PopulateForm();
		SetCommandState();
	}

	private void PopulateModel()
	{
		if (State == ViewState.Review)
		{
			PopulateReviewList();
			return;
		}
		if (State == ViewState.Archive)
		{
			PopulateArchiveList();
			return;
		}
		if (State == ViewState.Validation)
		{
			PopulateValidationList();
			return;
		}
		List<Auditai.DTO.Project> projects = FilterByKpi(SortProjectImpl(_isSearch ? SearchProjects(_projects) : FilterProjects(_projects), Style.SortKind)).ToList();
		PopulateGrid(projects);
		PopulateCards(projects);
		PopulateFavorites();
	}

	private async Task FetchModel()
	{
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		try
		{
			IEnumerable<Auditai.DTO.Project> projectList = new List<Auditai.DTO.Project>();
			progressRuntimeData.NextStep("正在处理，请稍后...");
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				if (State == ViewState.Project)
					{
						progressRuntimeData.UpdateMessage("正在获取" + StringConstBase.Current.Project + "信息，请稍候...");
						progressRuntimeData.UpdateProgress(0.8f);
						if (!StorageRouter.IsLocalMode)
						{
							projectList = await WebApiClient.GetProjects();
						}
						else
						{
							projectList = await Auditai.LocalDataStore.StorageRouter.GetProjects();
						}
						if (UserTeam.CurrentTeamIsPayByProject && projectList != null)
					{
						if (!StorageRouter.IsLocalMode)
					{
						IEnumerable<Auditai.DTO.Project> enumerable = await WebApiClient.GetTeamPayedProjects(UserTeam.Current.Id);
						Dictionary<Guid, Auditai.DTO.Project> dictionary = new Dictionary<Guid, Auditai.DTO.Project>();
						if (enumerable != null)
						{
							foreach (Auditai.DTO.Project item in enumerable)
							{
								dictionary[item.Id] = item;
							}
							foreach (Auditai.DTO.Project item2 in projectList)
							{
								if (dictionary.TryGetValue(item2.Id, out var value))
								{
									item2.ProjectChargeType = value.ProjectChargeType;
									item2.ProjectLicenseDate = value.ProjectLicenseDate;
								}
							}
						}
					}
					}
				}
				else if (State == ViewState.Template)
				{
					progressForm.SetProgressDisplayValueConverter(new ProgressDisplayValueConverter_SmoothByTime(0.2f));
					progressRuntimeData.UpdateMessage("正在获取" + StringConstBase.Current.Template + "信息，请稍候...");
					progressRuntimeData.UpdateProgress(0.8f);
					if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode)
				{
					projectList = await WebApiClient.GetTemplates();
				}
				else
				{
					projectList = await Auditai.LocalDataStore.StorageRouter.GetTemplates();
				}
				}
				else if (State == ViewState.RecycleProject)
				{
					progressForm.SetProgressDisplayValueConverter(new ProgressDisplayValueConverter_SmoothByTime(0.2f));
					progressRuntimeData.UpdateMessage("正在获取回收" + StringConstBase.Current.Project + "信息，请稍候...");
					progressRuntimeData.UpdateProgress(0.8f);
					var recycleProjects = await Auditai.LocalDataStore.StorageRouter.GetRecycleProjects();
					projectList = (recycleProjects ?? Enumerable.Empty<Auditai.DTO.Project>()).Where((Auditai.DTO.Project p) => p.Type == ProjectType.Project);
				}
				else if (State == ViewState.Review)
				{
					progressForm.SetProgressDisplayValueConverter(new ProgressDisplayValueConverter_SmoothByTime(0.2f));
					progressRuntimeData.UpdateMessage("正在获取审核单信息，请稍候...");
					progressRuntimeData.UpdateProgress(0.8f);
					List<ReviewSubmissionDto> submissions = await WebApiClient.GetReviewSubmissions(_reviewScope);
					_reviewSubmissions = submissions ?? new List<ReviewSubmissionDto>();
				}
				else if (State == ViewState.Archive)
				{
					progressForm.SetProgressDisplayValueConverter(new ProgressDisplayValueConverter_SmoothByTime(0.2f));
					progressRuntimeData.UpdateMessage("正在获取归档信息，请稍候...");
					progressRuntimeData.UpdateProgress(0.8f);
					if (_archiveShowCandidates)
					{
						List<Auditai.DTO.Project> candidates = await WebApiClient.GetArchiveCandidates();
						_archiveCandidates = candidates ?? new List<Auditai.DTO.Project>();
					}
					else
					{
						List<ProjectArchiveDto> archives = await WebApiClient.GetArchives();
						_archiveRecords = archives ?? new List<ProjectArchiveDto>();
					}
				}
				else if (State == ViewState.Validation)
				{
					progressForm.SetProgressDisplayValueConverter(new ProgressDisplayValueConverter_SmoothByTime(0.2f));
					progressRuntimeData.UpdateMessage("正在获取稽核规则，请稍候...");
					progressRuntimeData.UpdateProgress(0.8f);
					if (_validationStore != null)
					{
						_validationRules = await _validationStore.LoadAsync();
					}
					else
					{
						_validationRules = null;
					}
				}
				else
				{
					progressForm.SetProgressDisplayValueConverter(new ProgressDisplayValueConverter_SmoothByTime(0.2f));
					progressRuntimeData.UpdateMessage("正在获取回收" + StringConstBase.Current.Template + "信息，请稍候...");
					progressRuntimeData.UpdateProgress(0.8f);
					var recycleProjects = await Auditai.LocalDataStore.StorageRouter.GetRecycleProjects();
					projectList = (recycleProjects ?? Enumerable.Empty<Auditai.DTO.Project>()).Where((Auditai.DTO.Project p) => p.Type == ProjectType.Template);
				}
			});
			_projects.Clear();
			_projects.AddRange(projectList);
			await Task.Delay(1);
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private void PopulateForm()
	{
		if (State == ViewState.Project)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.Projects24, IconRes.Projects16);
			_form.Text = StringConstBase.Current.Project + "管理";
		}
		else if (State == ViewState.Template)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.Templates16, IconRes.Templates24);
			_form.Text = StringConstBase.Current.Template + "管理";
		}
		else if (State == ViewState.RecycleProject)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.RemoveProject16, IconRes.RemoveProject24);
			_form.Text = StringConstBase.Current.Project + "回收站";
		}
		else if (State == ViewState.RecycleTemplate)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.RemoveProject16, IconRes.RemoveProject24);
			_form.Text = StringConstBase.Current.Template + "回收站";
		}
		else if (State == ViewState.Review)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.ReviewSubmit, IconRes.ReviewSubmit);
			_form.Text = "上报审核";
		}
		else if (State == ViewState.Archive)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.ReviewArchive, IconRes.ReviewArchive);
			_form.Text = "项目归档";
		}
		else if (State == ViewState.Validation)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.ValidationSettings, IconRes.ValidationSettings);
			_form.Text = "稽核检查";
		}
	}

	private void _grid_Resize(object sender, EventArgs e)
	{
		AutoSizeGridColumns();
	}

	private void AutoSizeGridColumns()
	{
		if (_grid == null || _grid.Cols.Count <= _grid.Cols.Fixed || _grid.Visible == false)
		{
			return;
		}
		int clientWidth = _grid.ClientSize.Width;
		if (clientWidth <= 0) return;
		int fixedWidth = 0;
		for (int i = 0; i < _grid.Cols.Fixed; i++)
			fixedWidth += _grid.Cols[i].WidthDisplay;
		int availableWidth = clientWidth - fixedWidth;
		if (availableWidth <= 0) return;
		string[] colNames = { "CN_CHECK", "CN_PROJNUM", "CN_PROJNAME", "CN_PROJCAT", "CN_STATUS", "CN_PROJAUDITEE", "CN_CREATOR", "CN_PROJLEADER", "CN_PROJASSIST", "CN_PROJCHECK", "CN_PROJMEMBERS", "CN_CREATETIME", "CN_OPENTIME", "CN_TMPLEDITOR", "CN_TMPLUSER", "CN_PROJNOTE" };
		double[] ratios = { 0.05, 0.09, 0.17, 0.08, 0.06, 0.11, 0.07, 0.07, 0.07, 0.07, 0.055, 0.08, 0.09, 0.10, 0.10, 0.10 };
		double totalRatio = 0;
		for (int i = 0; i < colNames.Length && i < ratios.Length; i++)
		{
			if (_grid.Cols.Contains(colNames[i]) && _grid.Cols[colNames[i]].Visible && _grid.Cols[colNames[i]].Width > 1)
			{
				totalRatio += ratios[i];
			}
		}
		if (totalRatio <= 0) return;
		_grid.BeginUpdate();
		try
		{
			for (int i = 0; i < colNames.Length && i < ratios.Length; i++)
			{
				if (_grid.Cols.Contains(colNames[i]) && _grid.Cols[colNames[i]].Visible && _grid.Cols[colNames[i]].Width > 1)
				{
					int width = (int)(availableWidth * ratios[i] / totalRatio);
					if (width < 40) width = 40;
					_grid.Cols[colNames[i]].Width = width;
				}
			}
		}
		finally { _grid.EndUpdate(); }
	}

	private void SetMinColumnWidths()
	{
		var minWidths = new Dictionary<string, int>
		{
			{ "CN_PROJNUM", 100 },
			{ "CN_PROJNAME", 150 },
			{ "CN_PROJCAT", 80 },
			{ "CN_STATUS", 70 },
			{ "CN_PROJAUDITEE", 120 },
			{ "CN_CREATOR", 80 },
			{ "CN_PROJLEADER", 80 },
			{ "CN_PROJASSIST", 80 },
			{ "CN_PROJCHECK", 80 },
			{ "CN_PROJMEMBERS", 60 },
			{ "CN_CREATETIME", 90 },
			{ "CN_OPENTIME", 110 },
			{ "CN_TMPLEDITOR", 120 },
			{ "CN_TMPLUSER", 120 },
			{ "CN_PROJNOTE", 80 }
		};
		foreach (var kvp in minWidths)
		{
			var col = _grid.Cols[kvp.Key];
			if (col != null && col.Width < kvp.Value)
			{
				col.Width = kvp.Value;
			}
		}
	}

	/// <summary>审核页列表列宽比例：项目名称/轮次/状态/当前级别/提交人/提交时间/当前审批人</summary>
	private static readonly double[] ReviewColumnRatios = { 0.24, 0.07, 0.09, 0.10, 0.12, 0.18, 0.20 };

	/// <summary>归档候选列表列宽比例：项目名称/项目编号/审核状态</summary>
	private static readonly double[] ArchiveCandidateColumnRatios = { 0.46, 0.30, 0.24 };

	/// <summary>已归档列表列宽比例：项目名称/归档编号/归档人/归档时间/保管年限</summary>
	private static readonly double[] ArchiveRecordColumnRatios = { 0.30, 0.22, 0.14, 0.22, 0.12 };

	/// <summary>列表视图（审核/归档页）列宽按比例自适应，随窗体 Resize 重算</summary>
	private void AutoSizeLvColumns(ListView lv, double[] ratios)
	{
		if (lv == null || ratios == null || lv.Columns.Count == 0 || lv.Visible == false)
		{
			return;
		}
		int availableWidth = lv.ClientSize.Width;
		if (availableWidth <= 0)
		{
			return;
		}
		// 预留竖向滚动条宽度，避免条目增多时出现横向滚动条
		int reserved = SystemInformation.VerticalScrollBarWidth;
		if (availableWidth > reserved)
		{
			availableWidth -= reserved;
		}
		double totalRatio = 0;
		for (int i = 0; i < lv.Columns.Count && i < ratios.Length; i++)
		{
			totalRatio += ratios[i];
		}
		if (totalRatio <= 0)
		{
			return;
		}
		lv.BeginUpdate();
		try
		{
			for (int i = 0; i < lv.Columns.Count && i < ratios.Length; i++)
			{
				int width = (int)(availableWidth * ratios[i] / totalRatio);
				if (width < 40)
				{
					width = 40;
				}
				lv.Columns[i].Width = width;
			}
		}
		finally
		{
			lv.EndUpdate();
		}
	}

	private void PopulateGrid(List<Auditai.DTO.Project> projects)
	{
		_grid.BeginUpdate();
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Count = 0;
		_grid.Cols.Fixed = 0;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		if (State == ViewState.RecycleProject || State == ViewState.RecycleTemplate)
		{
			column.Name = "CN_CHECK";
			column.Caption = "选择";
			column.DataType = typeof(bool);
			column = _grid.Cols.Add();
		}
		column.Name = "CN_PROJNUM";
		column.AllowEditing = false;
		column = _grid.Cols.Add();
		column.Name = "CN_PROJNAME";
		column.AllowEditing = false;
		column = _grid.Cols.Add();
		column.Name = "CN_PROJCAT";
		column.AllowEditing = false;
		Dictionary<Auditai.DTO.Project, bool> marked;
		if (State == ViewState.Project || State == ViewState.RecycleProject)
		{
			_grid.Cols["CN_PROJNUM"].Caption = StringConstBase.Current.Project + "编号";
			_grid.Cols["CN_PROJNAME"].Caption = StringConstBase.Current.Project + "名称";
			_grid.Cols["CN_PROJCAT"].Caption = StringConstBase.Current.Project + "类别";
			column = _grid.Cols.Add();
			column.Name = "CN_STATUS";
			column.Caption = "状态";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_PROJAUDITEE";
			column.Caption = StringConstBase.Current.Auditee;
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_CREATOR";
			column.Caption = "创建者";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_PROJLEADER";
			column.Caption = StringConstBase.Current.Manager;
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_PROJASSIST";
			column.Caption = StringConstBase.Current.Assistant;
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_PROJCHECK";
			column.Caption = "复核人";
			column.AllowEditing = false;
			if (Program.MainForm.CurrentEdition is AppEditionGeneral)
			{
				column.Visible = false;
			}
			column = _grid.Cols.Add();
			column.Name = "CN_PROJMEMBERS";
			column.Caption = "成员数";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_CREATETIME";
			column.Caption = "创建时间";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_OPENTIME";
			column.Caption = "最近打开";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_PROJNOTE";
			column.Caption = "备注";
			column.AllowEditing = false;
			OnlyMyself();
			if (State == ViewState.Project)
			{
				_grid.Tree.Column = _grid.Cols.IndexOf("CN_PROJNUM");
				marked = projects.ToDictionary((Auditai.DTO.Project p) => p, (Auditai.DTO.Project p) => false);
				for (int i = 0; i < projects.Count; i++)
				{
					Auditai.DTO.Project p2 = projects[i];
					AddProject(p2);
				}
			}
			else
			{
				_grid.Tree.Clear();
				foreach (Auditai.DTO.Project project3 in projects)
				{
					C1.Win.C1FlexGrid.Row r2 = _grid.Rows.Add();
					PopulateProject(project3, r2);
				}
			}
		}
		else if (State == ViewState.Template || State == ViewState.RecycleTemplate)
		{
			_grid.Cols["CN_PROJNUM"].Caption = StringConstBase.Current.Template + "编号";
			_grid.Cols["CN_PROJNAME"].Caption = StringConstBase.Current.Template + "名称";
			_grid.Cols["CN_PROJCAT"].Caption = StringConstBase.Current.Template + "类别";
			column = _grid.Cols.Add();
			column.Name = "CN_STATUS";
			column.Caption = "状态";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_CREATOR";
			column.Caption = "创建者";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_TMPLEDITOR";
			column.Caption = "可编辑的用户";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_TMPLUSER";
			column.Caption = "可使用的用户";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_CREATETIME";
			column.Caption = "创建时间";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_OPENTIME";
			column.Caption = "最近打开";
			column.AllowEditing = false;
			column = _grid.Cols.Add();
			column.Name = "CN_PROJNOTE";
			column.Caption = "备注";
			column.AllowEditing = false;
			_grid.Tree.Clear();
			foreach (Auditai.DTO.Project project4 in projects)
			{
				C1.Win.C1FlexGrid.Row r3 = _grid.Rows.Add();
				PopulateProject(project4, r3);
			}
		}
		_grid.Select(-1, -1);
		_grid.Cols["CN_PROJNOTE"].Width = 1;
		_grid.EndUpdate();
		_grid.BeginUpdate();
		SetMinColumnWidths();
		_grid.AutoSizeCols();
		_grid.EndUpdate();
		AutoSizeGridColumns();
		void AddProject(Auditai.DTO.Project p)
		{
			if (!marked[p])
			{
				marked[p] = true;
				if (p.ParentId.HasValue)
				{
					Auditai.DTO.Project project2 = projects.FirstOrDefault(delegate(Auditai.DTO.Project pr)
					{
						Guid id = pr.Id;
						Guid? parentId = p.ParentId;
						return id == parentId;
					});
					if (project2 == null)
					{
						C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
						PopulateProject(p, row);
						row.IsNode = true;
						if (State == ViewState.RecycleProject)
						{
							_grid.SetCellCheck(row.Index, _grid.Cols.IndexOf("CN_CHECK"), CheckEnum.Unchecked);
						}
					}
					else
					{
						if (!marked[project2])
						{
							AddProject(project2);
						}
						int index = FindRowByProject(project2);
						C1.Win.C1FlexGrid.Row row2 = _grid.Rows[index].Node.AddNode(NodeTypeEnum.LastChild, p.Number, p, null).Row;
						PopulateProject(p, row2);
						row2.IsNode = true;
						if (State == ViewState.RecycleProject)
						{
							_grid.SetCellCheck(row2.Index, _grid.Cols.IndexOf("CN_CHECK"), CheckEnum.Unchecked);
						}
					}
				}
				else
				{
					C1.Win.C1FlexGrid.Row row3 = _grid.Rows.Add();
					PopulateProject(p, row3);
					row3.IsNode = true;
					if (State == ViewState.RecycleProject)
					{
						_grid.SetCellCheck(row3.Index, _grid.Cols.IndexOf("CN_CHECK"), CheckEnum.Unchecked);
					}
				}
			}
		}
		int FindRowByProject(Auditai.DTO.Project p)
		{
			for (int j = _grid.Rows.Fixed; j < _grid.Rows.Count; j++)
			{
				if (_grid.Rows[j].UserData == p)
				{
					return j;
				}
			}
			return -1;
		}
		void PopulateProject(Auditai.DTO.Project project, C1.Win.C1FlexGrid.Row r)
		{
			r["CN_PROJNUM"] = project.Number;
			r["CN_PROJNAME"] = project.Name;
			r["CN_PROJCAT"] = project.Category;
			r["CN_STATUS"] = GetProjectStatusText(project);
			r["CN_CREATETIME"] = project.CreateTime.ToString("yyyy-MM-dd");
			DateTime? openTime = ProjectInfoManager.GetInstance().GetProject(project.Id.ToString())?.OpenTime;
			r["CN_OPENTIME"] = ((openTime.HasValue && openTime.Value > DateTime.MinValue) ? openTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
			r["CN_PROJNOTE"] = project.Note;
			r.UserData = project;
			if (State == ViewState.Project || State == ViewState.RecycleProject)
			{
				r["CN_CREATOR"] = project.Creator?.Name ?? "";
				var users = project.Users ?? Enumerable.Empty<Auditai.DTO.User>();
				r["CN_PROJLEADER"] = users.Where((Auditai.DTO.User u) => u.Role == UserRole.Manager);
				r["CN_PROJASSIST"] = users.Where((Auditai.DTO.User u) => u.Role == UserRole.Assistant);
				r["CN_PROJCHECK"] = users.Where((Auditai.DTO.User u) => u.Role == UserRole.Checker);
				r["CN_PROJAUDITEE"] = project.Auditee;
				r["CN_PROJMEMBERS"] = users.Count();
			}
			else if (State == ViewState.Template || State == ViewState.RecycleTemplate)
			{
				string value;
				IEnumerable<Auditai.DTO.User> value2;
				var users = project.Users ?? Enumerable.Empty<Auditai.DTO.User>();
				if (project.SystemBuild)
				{
					if (Auditai.Model.User.Current.IsSystemAdmin || Auditai.Model.User.Current.IsSystemSupporter)
					{
						value = project.Creator?.Name ?? "";
						value2 = users.Where((Auditai.DTO.User u) => u.Role == UserRole.Editor);
					}
					else
					{
						value = string.Empty;
						value2 = null;
					}
				}
				else
				{
					value = project.Creator?.Name ?? "";
					value2 = users.Where((Auditai.DTO.User u) => u.Role == UserRole.Editor);
				}
				r["CN_CREATOR"] = value;
				r["CN_TMPLEDITOR"] = value2;
				r["CN_TMPLUSER"] = users.Where((Auditai.DTO.User u) => u.Role == UserRole.User);
			}
		}
	}

	/// <summary>列表"状态"列文案（注意与审核页 GetReviewStatusText 的 0-3 语义不同：此处为项目 DTO ReviewStatus，0未上报/1审批中/2已通过/3已退回，归档优先）。</summary>
	private static string GetProjectStatusText(Auditai.DTO.Project p)
	{
		if (p == null)
		{
			return "未上报";
		}
		if (p.IsArchived)
		{
			return "已归档";
		}
		switch (p.ReviewStatus)
		{
		case 1:
			return "审批中";
		case 2:
			return "已通过";
		case 3:
			return "已退回";
		default:
			return "未上报";
		}
	}

	private void PopulateCards(List<Auditai.DTO.Project> projects)
	{
		_isShowProjectPayStatusIcon = false;
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			_isShowProjectPayStatusIcon = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_project_tile_pay_status", defaultValue: true);
		}
		_cardFlow.AllowMultiSelect = State == ViewState.RecycleProject || State == ViewState.RecycleTemplate;
		_ctxCardItem = null;
		_cardFlow.SetGroups(BuildCardGroups(projects));
		// 收藏行数据在 PopulateModel 末尾的 PopulateFavorites 中统一构建（含可见性控制）
	}

	private System.Drawing.Image ResolveCardIcon(Auditai.DTO.Project p)
		{
			if (p == null)
			{
				return null;
			}
			if (p.Type == ProjectType.Project)
			{
				return ProjectCardIconProvider.Project;
			}
			if (!p.SystemBuild)
			{
				return ProjectCardIconProvider.CustomTemplate;
			}
			return (p.ChargeType == ChargeType.Pay) ? ProjectCardIconProvider.VipTemplate : ProjectCardIconProvider.SystemTemplate;
		}

	/// <summary>卡片类型强调色：项目=蓝色、系统模板=黄色，VIP/自定义模板返回 Color.Empty 保持白卡。</summary>
	private Color ResolveCardTint(Auditai.DTO.Project p)
	{
		if (p == null)
		{
			return Color.Empty;
		}
		if (p.Type == ProjectType.Project)
		{
			return Color.FromArgb(59, 130, 246);
		}
		if (p.SystemBuild && p.ChargeType != ChargeType.Pay)
		{
			return Color.FromArgb(245, 158, 11);
		}
		return Color.Empty;
	}

	private System.Drawing.Image ResolveCardPayBadge(Auditai.DTO.Project p)
	{
		if (p == null || !UserTeam.CurrentTeamIsPayByProject || !_isShowProjectPayStatusIcon)
		{
			return null;
		}
		return (p.ProjectChargeType == ChargeType.Pay) ? Program.MainForm.CurrentEdition.PayedTemplateTileCornerIcon : Program.MainForm.CurrentEdition.UnPayTemplateTileCornerIcon;
	}

	/// <summary>按 Style.CardGroup 构建卡片分组（General 版一律单组；组内顺序保持传入 projects 的既有排序）。</summary>
	private List<ProjectCardFlow.CardGroupData> BuildCardGroups(List<Auditai.DTO.Project> projects)
	{
		bool isGeneral = Program.MainForm.CurrentEdition is AppEditionGeneral;
		List<ProjectCardFlow.CardGroupData> groups = new List<ProjectCardFlow.CardGroupData>();
		if (isGeneral)
		{
			groups.Add(CreateCardGroup(null, projects));
		}
		else
		{
			switch (Style.CardGroup)
			{
			case CardGroupMode.Category:
				groups.AddRange(from p in projects
					group p by ((p.Category ?? "").Split('|')[0]).Trim() into g
					orderby g.Key
					select CreateCardGroup(string.IsNullOrEmpty(g.Key) ? "未分类" : g.Key, g.ToList()));
				break;
			case CardGroupMode.CreateMonth:
				groups.AddRange(from p in projects
					group p by new { p.CreateTime.Year, p.CreateTime.Month } into g
					orderby g.Key.Year descending, g.Key.Month descending
					select CreateCardGroup(string.Format("{0}年{1}月", g.Key.Year, g.Key.Month), g.ToList()));
				break;
			case CardGroupMode.Creator:
				groups.AddRange(from p in projects
					group p by (string.IsNullOrWhiteSpace(p.Creator?.Name) ? "未知" : p.Creator.Name.Trim()) into g
					orderby g.Key
					select CreateCardGroup(g.Key, g.ToList()));
				break;
			case CardGroupMode.None:
				groups.Add(CreateCardGroup(null, projects));
				break;
			default:
			{
				if (State == ViewState.Project || State == ViewState.Template)
				{
					List<Auditai.DTO.Project> recentProjects = (from i in ProjectInfoManager.GetInstance().GetRecent()
						select projects.Find((Auditai.DTO.Project p) => p.Id.ToString() == i) into p
						where p != null
						select p).ToList();
					if (recentProjects.Count > 0)
					{
						groups.Add(CreateCardGroup("最近使用", recentProjects));
					}
				}
				if (State == ViewState.Project)
				{
					groups.Add(CreateCardGroup("所有" + StringConstBase.Current.Project, projects));
				}
				else if (State == ViewState.Template)
				{
					groups.Add(CreateCardGroup("所有" + StringConstBase.Current.Template, projects));
				}
				else
				{
					groups.Add(CreateCardGroup(null, projects));
				}
				break;
			}
			}
		}
		if (isGeneral && groups.Count > 0 && !SoftwareLicenseManager.IsAddProjectOutOfLicenseLimit())
		{
			List<ProjectCardItem> firstItems = (groups[0].Items as List<ProjectCardItem>) ?? groups[0].Items.ToList();
			firstItems.Insert(0, new ProjectCardItem
			{
				IsCreateTile = true
			});
			groups[0].Items = firstItems;
		}
		return groups;
	}

	private ProjectCardFlow.CardGroupData CreateCardGroup(string title, IEnumerable<Auditai.DTO.Project> source)
	{
		return new ProjectCardFlow.CardGroupData
		{
			Title = title,
			Items = source.Select((Auditai.DTO.Project p) => CreateCardItem(p)).ToList()
		};
	}

	private ProjectCardItem CreateCardItem(Auditai.DTO.Project project)
	{
		return new ProjectCardItem
		{
			Project = project,
			OpenTime = ProjectInfoManager.GetInstance().GetProject(project.Id.ToString())?.OpenTime,
			IsFavorite = ProjectFavoritesStore.IsFavorite(project.Id)
		};
	}

	/// <summary>
	/// 构建"已收藏"卡片行：从当前视图数据源 _projects 筛选收藏项（项目/模板通用）。
	/// 可见性统一在此控制：卡片模式 && 项目/模板视图 && 有收藏（其余场景隐藏，Dock 自动回收空间）。
	/// </summary>
	private void PopulateFavorites()
	{
		List<ProjectCardItem> items = new List<ProjectCardItem>();
		foreach (Auditai.DTO.Project p in _projects)
		{
			if (p != null && ProjectFavoritesStore.IsFavorite(p.Id))
			{
				ProjectCardItem item = CreateCardItem(p);
				item.IsFavorite = true;
				items.Add(item);
			}
		}
		_favBar.SetFavorites(items);
		_favBar.Visible = Style.ViewMode != ListTileViewMode.List
			&& (State == ViewState.Project || State == ViewState.Template)
			&& _favBar.Controls.Count > 0;
	}

	/// <summary>构建统计条（仅项目/模板视图显示；分类筛选与 KPI 统计合并为同一条，数据基于当前视图数据源 _projects 实时统计）。</summary>
	private void PopulateKpi()
	{
		if (State != ViewState.Project && State != ViewState.Template)
		{
			_kpiStrip.Visible = false;
			return;
		}
		// 统计项品牌色板（选中填充色，用于区分不同统计维度；分类项不设置保持主题色）
		Color kpiAllColor = Color.FromArgb(59, 130, 246);
		Color kpiNewMonthColor = Color.FromArgb(16, 185, 129);
		Color kpiReviewColor = Color.FromArgb(245, 158, 11);
		Color kpiArchiveColor = Color.FromArgb(139, 92, 246);
		Color kpiExpireColor = Color.FromArgb(239, 68, 68);
		DateTime now = DateTime.Now;
		List<KpiStrip.KpiItem> items = new List<KpiStrip.KpiItem>();
		if (State == ViewState.Project)
		{
			items.Add(new KpiStrip.KpiItem
			{
				Label = "全部" + StringConstBase.Current.Project,
				Value = _projects.Count,
				Tag = "all",
				AccentColor = kpiAllColor
			});
			items.Add(new KpiStrip.KpiItem
			{
				Label = "本月新建",
				Value = _projects.Count((Auditai.DTO.Project p) => p.CreateTime.Year == now.Year && p.CreateTime.Month == now.Month),
				Tag = "newmonth",
				AccentColor = kpiNewMonthColor
			});
			items.Add(new KpiStrip.KpiItem
			{
				Label = "审批中",
				Value = _projects.Count((Auditai.DTO.Project p) => p.ReviewStatus == 1),
				Tag = "reviewing",
				AccentColor = kpiReviewColor
			});
			items.Add(new KpiStrip.KpiItem
			{
				Label = "已归档",
				Value = _projects.Count((Auditai.DTO.Project p) => p.IsArchived),
				Tag = "archived",
				AccentColor = kpiArchiveColor
			});
			items.Add(new KpiStrip.KpiItem
			{
				Label = "即将到期",
				Value = _projects.Count((Auditai.DTO.Project p) => IsProjectExpiringSoon(p)),
				Tag = "expiring",
				AccentColor = kpiExpireColor
			});
		}
		else
		{
			items.Add(new KpiStrip.KpiItem
			{
				Label = "全部" + StringConstBase.Current.Template,
				Value = _projects.Count,
				Tag = "all",
				AccentColor = kpiAllColor
			});
			items.Add(new KpiStrip.KpiItem
			{
				Label = "本月新建",
				Value = _projects.Count((Auditai.DTO.Project p) => p.CreateTime.Year == now.Year && p.CreateTime.Month == now.Month),
				Tag = "newmonth",
				AccentColor = kpiNewMonthColor
			});
		}
		// 末尾：分类筛选项（计数 + 名称，与统计项同款选中样式，复选切换）
		AddCategoryItems(items);
		_kpiStrip.SetItems(items);
		_kpiStrip.SelectedTag = _kpiFilterTag;
		_kpiStrip.Visible = true;
	}

	/// <summary>在统计条最前追加分类筛选项（"无分类" + 各具名分类），沿用复选状态。</summary>
	private void AddCategoryItems(List<KpiStrip.KpiItem> items)
	{
		Dictionary<string, int> catCounts = new Dictionary<string, int>(StringComparer.Ordinal);
		int emptyCount = 0;
		foreach (Auditai.DTO.Project p in _projects)
		{
			if (string.IsNullOrWhiteSpace(p.Category))
			{
				emptyCount++;
				continue;
			}
			foreach (string cat in p.Category.Split('|'))
			{
				if (string.IsNullOrEmpty(cat))
				{
					continue;
				}
				catCounts.TryGetValue(cat, out int c);
				catCounts[cat] = c + 1;
			}
		}
		if (emptyCount > 0)
		{
			items.Add(new KpiStrip.KpiItem
			{
				Label = "无分类",
				Value = emptyCount,
				Tag = CategoryTagEmpty,
				IsCategory = true,
				Checked = _categoryEmptyChecked
			});
		}
		foreach (KeyValuePair<string, int> kv in catCounts.OrderBy((KeyValuePair<string, int> kv) => kv.Key))
		{
			items.Add(new KpiStrip.KpiItem
			{
				Label = kv.Key,
				Value = kv.Value,
				Tag = CategoryTagPrefix + kv.Key,
				IsCategory = true,
				Checked = _selectedCategories.Contains(kv.Key)
			});
		}
	}

	/// <summary>按 KPI 筛选标识过滤（null 或 all 不过滤；"全部"项 Tag=all 作为可点击的筛选重置项）。</summary>
	private IEnumerable<Auditai.DTO.Project> FilterByKpi(IEnumerable<Auditai.DTO.Project> projects)
	{
		if (string.IsNullOrEmpty(_kpiFilterTag) || _kpiFilterTag == "all")
		{
			return projects;
		}
		switch (_kpiFilterTag)
		{
		case "newmonth":
			return projects.Where((Auditai.DTO.Project p) => p.CreateTime.Year == DateTime.Now.Year && p.CreateTime.Month == DateTime.Now.Month);
		case "reviewing":
			return projects.Where((Auditai.DTO.Project p) => p.ReviewStatus == 1);
		case "archived":
			return projects.Where((Auditai.DTO.Project p) => p.IsArchived);
		case "expiring":
			return projects.Where((Auditai.DTO.Project p) => IsProjectExpiringSoon(p));
		default:
			return projects;
		}
	}

	/// <summary>临期判断：与打开项目处口径一致（服务器模式 + 按项目付费团队 + 未过期且距到期不足 30 天）。</summary>
	private static bool IsProjectExpiringSoon(Auditai.DTO.Project p)
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode || !UserTeam.CurrentTeamIsPayByProject)
		{
			return false;
		}
		double totalDays = (p.ProjectLicenseDate - DateTime.Now).TotalDays;
		return totalDays >= 0.0 && totalDays < 30.0;
	}

	private void PopulateCategory()
	{
		if (State != ViewState.Project && State != ViewState.Template)
		{
			return;
		}
		// 汇总当前视图全部分类名（供单项目"类别"右键菜单使用），并清理已失效的勾选状态
		KnownCategories.Clear();
		HashSet<string> valid = new HashSet<string>(StringComparer.Ordinal);
		bool hasEmpty = false;
		foreach (Auditai.DTO.Project p in _projects)
		{
			if (string.IsNullOrWhiteSpace(p.Category))
			{
				hasEmpty = true;
				continue;
			}
			foreach (string cat in p.Category.Split('|'))
			{
				if (!string.IsNullOrEmpty(cat))
				{
					valid.Add(cat);
				}
			}
		}
		KnownCategories.AddRange(valid.OrderBy((string c) => c));
		_selectedCategories.IntersectWith(valid);
		if (!hasEmpty)
		{
			_categoryEmptyChecked = false;
		}
	}

	private void PopulateViewMode()
	{
		if (State == ViewState.Review || State == ViewState.Archive || State == ViewState.Validation)
		{
			_grid.Hide();
			_cardFlow.Hide();
			_favBar.Hide();
			_kpiStrip.Hide();
			_lvReview.Visible = State == ViewState.Review;
			_lvArchive.Visible = State == ViewState.Archive;
			_lvValidation.Visible = State == ViewState.Validation;
			return;
		}
		_lvReview.Visible = false;
		_lvArchive.Visible = false;
		_lvValidation.Visible = false;
		bool isCardView = Style.ViewMode != ListTileViewMode.List;
		// 收藏行仅卡片模式 + 项目/模板视图显示（内容为空时 SetFavorites 已自动隐藏，避免空白条）
		_favBar.Visible = isCardView && (State == ViewState.Project || State == ViewState.Template) && _favBar.Controls.Count > 0;
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			_topBar.SetCommandVisible(RB_VIEWMODE, false);
			Style.ViewMode = ListTileViewMode.Tile;
			_grid.Hide();
			_cardFlow.Show();
			return;
		}
		SimpleCommand ribbonButton = GetSideCommand(RB_VIEWMODE);
		if (Style.ViewMode == ListTileViewMode.List)
		{
			_grid.Show();
			_cardFlow.Hide();
			ribbonButton.Text = "卡片模式";
			ribbonButton.Image = IconRes.tileMode;
		}
		else if (Style.ViewMode == ListTileViewMode.Tile)
		{
			_grid.Hide();
			_cardFlow.Show();
			ribbonButton.Text = "列表模式";
			ribbonButton.Image = IconRes.listMode;
		}
	}

	public IEnumerable<Auditai.DTO.Project> SortProjectImpl(IEnumerable<Auditai.DTO.Project> projects, SortKind kind)
	{
		switch (kind)
		{
		case SortKind.CreateTime:
			return projects.OrderBy((Auditai.DTO.Project i) => i.CreateTime);
		case SortKind.Number:
			return projects.OrderBy((Auditai.DTO.Project p) => p.Number);
		case SortKind.Name:
			return projects.OrderBy((Auditai.DTO.Project p) => p.Name);
		case SortKind.Category:
			return projects.OrderBy((Auditai.DTO.Project p) => p.Category);
		default:
			return SortByOpenTime(projects);
		}
	}

	/// <summary>按最近打开时间倒序（从未打开的排最后；默认排序分支与 OpenTime 分支共用）。</summary>
	private static IEnumerable<Auditai.DTO.Project> SortByOpenTime(IEnumerable<Auditai.DTO.Project> projects)
	{
		Dictionary<Auditai.DTO.Project, DateTime> source = projects.ToDictionary((Auditai.DTO.Project p) => p, (Auditai.DTO.Project p) => ProjectInfoManager.GetInstance().GetProject(p.Id.ToString())?.OpenTime ?? DateTime.MinValue);
		return from i in source
			orderby i.Value descending
			select i.Key;
	}

	private IEnumerable<Auditai.DTO.Project> FilterProjects(IEnumerable<Auditai.DTO.Project> projects)
	{
		if (!_categoryEmptyChecked && _selectedCategories.Count == 0)
		{
			return projects;
		}
		return projects.Where(delegate(Auditai.DTO.Project p)
		{
			HashSet<string> hashSet = new HashSet<string>(p.Category.Split('|'));
			hashSet.Remove("");
			if (_categoryEmptyChecked && !string.IsNullOrWhiteSpace(p.Category))
			{
				return false;
			}
			foreach (string cat in _selectedCategories)
			{
				if (!hashSet.Contains(cat))
				{
					return false;
				}
			}
			return true;
		});
	}

	private IEnumerable<Auditai.DTO.Project> SearchProjects(IEnumerable<Auditai.DTO.Project> projects)
	{
		string keyword = _txbSearch.Text;
		if (_isSearch && keyword != "")
		{
			Dictionary<Auditai.DTO.Project, int> source = projects.ToDictionary((Auditai.DTO.Project p) => p, (Auditai.DTO.Project p) => FuzzySearch.Filter(p.Name, keyword) + FuzzySearch.Filter(p.Number, keyword) + FuzzySearch.Filter(p.Category.Replace("|", ","), keyword) + FuzzySearch.Filter(p.Auditee, keyword));
			return from t in source
				where t.Value > 0
				orderby t.Value
				select t.Key;
		}
		return projects;
	}

	private bool OnlyMyself()
	{
		if (Auditai.Model.User.Current.IsTeamAdmin)
		{
			return MemberManager.GetInstance().GetMembers().Count() == 1;
		}
		return false;
	}

	private async Task CreateProject(Auditai.DTO.Project usedTemplate)
	{
		if (Auditai.Model.User.Current.TeamId == Guid.Empty)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您还未加入组织，请点击“同事管理”，创建一个组织，或者让同事邀请您进入现有的组织，开始体验协同办公！");
			return;
		}
		Auditai.DTO.Project project = Auditai.Model.User.Current.GetNewProjectCandidate();
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			if (usedTemplate == null)
			{
				FormSelectTemplate formSelectTemplate = new FormSelectTemplate();
				if (formSelectTemplate.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				project.TemplateId = formSelectTemplate.ResultTemplate?.Id;
				if (Program.ClientPlatformType == PlatformType.Custom)
				{
					project.Name = GetNextProjectName(formSelectTemplate.ResultTemplate?.Name);
				}
				else
				{
					project.Name = formSelectTemplate.ResultTemplate?.Name ?? GetNextProjectName(null);
				}
			}
			else
			{
				project.TemplateId = usedTemplate.Id;
			}
			FormProjectMembers formProjectMembers = new FormProjectMembers();
			formProjectMembers.Project = project;
			if (formProjectMembers.ShowDialog() != DialogResult.OK)
			{
				return;
			}
		}
		else
		{
			project.TemplateId = usedTemplate?.Id;
			dlgProjectEditor dlgProjectEditor2 = new dlgProjectEditor();
			dlgProjectEditor2.Project = project;
			if (!dlgProjectEditor2.ShowCreate())
			{
				return;
			}
			_sidebar.SelectModule(TAB_PROJECT);
		}
		try
		{
			ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStepIfProgressNotZero("正在创建" + StringConstBase.Current.Project + "，请稍后...");
			progressRuntimeData.UpdateProgress(0.8f);
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				await Auditai.LocalDataStore.StorageRouter.CreateProject(project);
			});
			ProjectInfoManager.GetInstance().UpdateOpenTime(project.Id.ToString(), DateTime.Now);
			await Populate();
			FindAndSelectRow(project);
			// P2 协同增强 Task 8：新建项目成功后广播通知团队成员
			try
			{
				var msg = new NotifyMessage { Kind = "newproject", Value = project.Id.ToString() };
				_ = SignalRClient.BroadcastToTeamUsers(msg.ToString());
			}
			catch (Exception)
			{
			}
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private void FindAndSelectRow(Auditai.DTO.Project p)
	{
		for (int i = 0; i < _grid.BodyRowsCount; i++)
		{
			if ((_grid.BodyGetRow(i).UserData as Auditai.DTO.Project)?.Id == p.Id)
			{
				_grid.BodySelect(i, 0);
				break;
			}
		}
	}

	private string GetNextProjectName(string templateName)
	{
		HashSet<string> hashSet = new HashSet<string>(_projects.Select((Auditai.DTO.Project p) => p.Name));
		int i = 1;
		if (!string.IsNullOrWhiteSpace(templateName))
		{
			for (; hashSet.Contains($"新{StringConstBase.Current.Project} {i}-{templateName}"); i++)
			{
			}
		}
		else
		{
			for (; hashSet.Contains($"新{StringConstBase.Current.Project} {i}"); i++)
			{
			}
		}
		if (!string.IsNullOrWhiteSpace(templateName))
		{
			return $"新{StringConstBase.Current.Project} {i}-{templateName}";
		}
		return $"新{StringConstBase.Current.Project} {i}";
	}

	private async Task OpenProject()
	{
		try
		{
			if (!_isInOpenProject)
			{
				_isInOpenProject = true;
				Auditai.DTO.Project sp = SelectedProject;
				if (sp == null)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择要打开的" + StringConstBase.Current.Project);
					return;
				}
				if (SelectedProject.Type == ProjectType.Template && !CanOpenTemplate())
				{
					return;
				}
				string willOpenProjectTypeName = null;
				if (SelectedProject.Type == ProjectType.Template)
				{
					willOpenProjectTypeName = StringConstBase.Current.Template;
				}
				else if (SelectedProject.Type == ProjectType.Project)
				{
					willOpenProjectTypeName = StringConstBase.Current.Project;
				}
				Task<Auditai.Model.Project> task = Program.MainForm.OpenOrSwitchToProject(sp.Id, willOpenProjectTypeName);
				if (task == null)
				{
					return;
				}
				Auditai.Model.Project project = await task;
				if (project == null)
				{
					return;
				}
				if (project != null)
				{
					_isClosing = false;
					_form.DialogResult = DialogResult.OK;
					ProjectInfoManager.GetInstance().UpdateOpenTime(sp.Id.ToString(), DateTime.Now);
				}
				if (!project.IsNeedSyncDataOnOpen)
				{
					project.IsNeedSyncDataOnOpen = true;
					if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && UserTeam.CurrentTeamIsPayByProject && project != null)
					{
						double totalDays = (project.ProjectLicenseDate - DateTime.Now).TotalDays;
						if (totalDays < 0.0)
						{
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{project.ProjectLicenseDate:yyyy年MM月dd日}到期，无法同步{StringConstBase.Current.Project}，请联系管理员购买或续期！");
						}
						else if (totalDays < 30.0)
						{
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品将于{project.ProjectLicenseDate:yyyy年MM月dd日}到期，建议您及时联系管理员购买或续期！");
						}
					}
				}
				else
				{
					bool flag = true;
					if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && UserTeam.CurrentTeamIsPayByProject && project != null)
					{
						double totalDays2 = (project.ProjectLicenseDate - DateTime.Now).TotalDays;
						if (totalDays2 < 0.0)
						{
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{project.ProjectLicenseDate:yyyy年MM月dd日}到期，无法同步{StringConstBase.Current.Project}，请联系管理员购买或续期！");
							flag = false;
						}
						else if (totalDays2 < 30.0)
						{
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品将于{project.ProjectLicenseDate:yyyy年MM月dd日}到期，建议您及时联系管理员购买或续期！");
						}
					}
					if (flag && await Program.MainForm.SyncProject(project))
					{
						Program.MainForm.ProjectHierarchy.Populate();
					}
				}
			}
			Program.MainForm.RefreshProjectsSyncTwinkle();
		}
		finally
		{
			_isInOpenProject = false;
		}
	}

	private async Task OpenProjectFromServer()
	{
		if (!HasSelectedProject)
		{
			return;
		}
		Auditai.DTO.Project selectedProject = SelectedProject;
		string text = ((selectedProject.Type == ProjectType.Project) ? StringConstBase.Current.Project : (StringConstBase.Current.Template ?? ""));
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "全新打开" + text + "将自云端全新下载，您本地未上传的数据将被放弃，确定要全新打开" + text + "吗？", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		string dbPathByGuid = MainForm.GetDbPathByGuid(selectedProject.Id);
		try
		{
			try
			{
				string localDataCacheDirectory = FileCacheManager.GetLocalDataCacheDirectory(selectedProject.Id);
				if (Directory.Exists(localDataCacheDirectory))
				{
					Directory.Delete(localDataCacheDirectory, recursive: true);
				}
				File.Delete(dbPathByGuid);
			}
			catch (Exception exception)
			{
				exception.Log("全新打开" + text + "时，删除本地文件失败");
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该" + text + "处于打开状态，请重新登录再全新打开该" + text + "！");
				return;
			}
			MainForm.RecentProjects.Remove(selectedProject.Id);
			await OpenProject();
		}
		catch (Exception arg)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"删除本地文件过程中发生异常，详细信息：\n{arg}");
		}
	}

	private async Task ModifyProject()
	{
		if (!HasSelectedProject)
		{
			return;
		}
		Auditai.DTO.Project clone = SelectedProject.Clone();
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			string text = InputForm.Text("重命名" + StringConstBase.Current.Project, "选定的" + StringConstBase.Current.Project + "重命名为：", SelectedProject.Name, 370);
			if (text == null)
			{
				return;
			}
			clone.Name = text;
		}
		else
		{
			dlgProjectEditor dlgProjectEditor2 = new dlgProjectEditor();
			dlgProjectEditor2.Project = clone;
			if (!dlgProjectEditor2.ShowModify())
			{
				return;
			}
		}
		try
		{
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			ProgressForm2 progressForm = new ProgressForm2();
			progressForm.ShowDialogOnUiThread(progressRuntimeData, async delegate
			{
				IProgress<ProgressInfo> iProg = new ProgressRuntimeDataReporter(progressRuntimeData);
				iProg.Report(new ProgressInfo
				{
					MainCaption = "正在修改" + StringConstBase.Current.Project + "信息...",
					MainProgress = 100
				});
				await Auditai.LocalDataStore.StorageRouter.UpdateProject(clone);
				if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode)
				{
					await SignalRClient.ChangeProjectMember(clone.Id.ToString());
				}
			});
			ProjectInfoManager.GetInstance().UpdateOpenTime(SelectedProject.Id.ToString(), DateTime.Now);
			await Populate();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private async Task DeleteProject()
	{
		if (!HasSelectedProject)
		{
			return;
		}
		// 服务器不可用时阻止删除操作
		if (!Program.MainForm.EnsureServerAvailable()) return;
		string text = ((State == ViewState.Project) ? StringConstBase.Current.Project : (StringConstBase.Current.Template ?? ""));
		if (Program.MainForm?.CurrentProject?.Id == SelectedProject.Id)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "不能删除当前正在编辑的" + text);
		}
		else
		{
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "确定要删除" + text + "吗？", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				try
				{
					// 模板视图下调用 DeleteTemplate（删除 .db 文件），项目视图下调用 DeleteProject（软删除）
					if (State == ViewState.Template)
					{
						await StorageRouter.DeleteTemplate(SelectedProject.Id);
					}
					else
					{
						await StorageRouter.DeleteProject(SelectedProject.Id);
					}
					await Populate();
				}
				catch (HttpRequestException ex4)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.InnerException?.Message ?? ex4.Message);
				}
				catch (Exception ex5)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "删除失败：" + ex5.Message);
				}
			}
		}
	}

	private async Task DuplicateProject()
	{
		if (!HasSelectedProject)
		{
			return;
		}
		Guid id = SelectedProject.Id;
		Auditai.DTO.Project newProject = SelectedProject.Clone();
		newProject.Id = Guid.NewGuid();
		newProject.Number += " - 副本";
		newProject.Name += " - 副本";
		newProject.Users = new List<Auditai.DTO.User>
		{
			new Auditai.DTO.User
			{
				Id = Auditai.Model.User.Current.Id,
				Name = Auditai.Model.User.Current.Name,
				UserName = Auditai.Model.User.Current.UserName,
				Role = UserRole.Manager
			}
		};
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			FormProjectMembers formProjectMembers = new FormProjectMembers();
			formProjectMembers.Project = newProject;
			if (formProjectMembers.ShowDialog() != DialogResult.OK)
			{
				return;
			}
		}
		else
		{
			dlgProjectEditor dlgProjectEditor2 = new dlgProjectEditor();
			dlgProjectEditor2.Project = newProject;
			if (!dlgProjectEditor2.ShowDuplicate())
			{
				return;
			}
		}
		try
		{
			JObject jObj = new JObject();
			jObj["OldProject"] = id;
			jObj["NewProject"] = JToken.FromObject(newProject);
			jObj["ClearPermissions"] = false;
			ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStepIfProgressNotZero("正在执行复制，请稍候...");
			progressRuntimeData.UpdateProgress(0.8f);
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode)
				{
					await WebApiClient.DuplicateProject(jObj);
				}
				else
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持该操作");
				}
			});
			ProjectInfoManager.GetInstance().UpdateOpenTime(newProject.Id.ToString(), DateTime.Now);
			await Populate();
			FindAndSelectRow(newProject);
			// P2 协同增强 Task 8：复制项目成功后广播通知团队成员
			try
			{
				var msg = new NotifyMessage { Kind = "newproject", Value = newProject.Id.ToString() };
				_ = SignalRClient.BroadcastToTeamUsers(msg.ToString());
			}
			catch (Exception)
			{
			}
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private async Task ShareProject()
	{
		if (!HasSelectedProject)
		{
			return;
		}
		if (SelectedProject.Type == ProjectType.Template && SelectedProject.SystemBuild && SelectedProject.ChargeType == ChargeType.Pay && SoftwareLicenseManager.IsSharePayProjectOutOfLicenseLimit())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您是" + SoftwareLicenseManager.GetUnPayedLicenseDisplayName() + "用户，您选中的" + StringConstBase.Current.Template + "为" + SoftwareLicenseManager.GetPayedLicenseDisplayName() + "用户专用，请联系官方客服升级为" + SoftwareLicenseManager.GetPayedLicenseDisplayName() + "用户后再跨组织分享该" + StringConstBase.Current.Template + "！");
			return;
		}
		Guid id = SelectedProject.Id;
		string text = InputForm.Text("跨组织分享", "请输入其他组织的系统管理员用户名");
		if (text == null)
		{
			return;
		}
		try
		{
			// 本地模式不支持跨组织分享
			if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持跨组织分享。");
				return;
			}
			JObject jObj = new JObject();
			jObj["OldProject"] = id;
			jObj["SharedUsername"] = text;
			ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStepIfProgressNotZero("正在执行跨组织分享" + ((State == ViewState.Project) ? StringConstBase.Current.Project : (StringConstBase.Current.Template ?? "")) + "，请稍候...");
			progressRuntimeData.UpdateProgress(0.8f);
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				await WebApiClient.ShareProject(jObj);
			});
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "跨组织分享成功。");
			await Task.Delay(1);
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private void ToggleViewMode()
	{
		if (Style.ViewMode == ListTileViewMode.List)
		{
			Style.ViewMode = ListTileViewMode.Tile;
		}
		else
		{
			Style.ViewMode = ListTileViewMode.List;
		}
		PopulateViewMode();
		SetCommandState();
	}

	private async Task ExportProject(SimpleCommand btn)
	{
		await ExportProjectImpl(btn.Text);
	}

	private async Task ExportProject(C1Command btn)
	{
		await ExportProjectImpl(btn.Text);
	}

	private async Task ExportProjectImpl(string buttonName)
	{
		if (SoftwareLicenseManager.IsExportProjectOutOfLicenseLimit(buttonName))
		{
			return;
		}
		if (SelectedProject == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择要导出的" + StringConstBase.Current.Project);
			return;
		}
		try
		{
			Auditai.Model.Project project = await Program.MainForm.OpenProjectDb_DownloadIfNotExist(SelectedProject);
			if (project == null)
			{
				return;
			}
			bool flag = project.GetAllTableNodes().Any((TreeTableNode n) => !n.Table.LocalExists);
			if (!flag)
			{
				flag = project.GetAllDocumentNodes().Any((TreeDocumentNode n) => !n.Document.LocalExists);
			}
			if (!flag || Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, StringConstBase.Current.Project + "中有表格或文档还未同步，确定仍要继续导出？", MessageBoxButtons.OKCancel) != DialogResult.Cancel)
			{
				ProjectExport projectExport = new ProjectExport();
				projectExport.Project = project;
				if (DialogResult.OK == await projectExport.SaveDialog())
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出成功");
				}
				ProjectInfoManager.GetInstance().UpdateOpenTime(project.Id.ToString(), DateTime.Now);
				await Populate();
			}
		}
		catch (IOException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出失败!" + ex.Message);
		}
		catch (Exception ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出失败!失败原因:" + ex2.Message);
		}
	}

	/// <summary>导出选中项目为 .auditai 归档文件（含项目数据库和元信息）</summary>
	private async Task ExportProjectFile()
	{
		await ExportProjectFileCore(SelectedProject);
	}

	/// <summary>导出指定项目为 .auditai 归档文件（"导出归档包"复用此路径）</summary>
	private async Task ExportProjectFileCore(Auditai.DTO.Project selected)
	{
		if (selected == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要导出的" + StringConstBase.Current.Project);
			return;
		}
		string defaultName = string.IsNullOrWhiteSpace(selected.Number)
			? $"{selected.Name}.auditai"
			: $"{selected.Number} {selected.Name}.auditai";
		foreach (char c in Path.GetInvalidFileNameChars())
			defaultName = defaultName.Replace(c, '_');
		using (var sfd = new SaveFileDialog
		{
			Filter = "项目归档文件 (*.auditai)|*.auditai",
			FileName = defaultName,
			Title = "导出项目文件"
		})
		{
			if (sfd.ShowDialog() != DialogResult.OK) return;
			try
			{
				// 服务端模式下，本地可能无 .db 缓存或缓存已过期（OpenProjectDb 会强制删除重下），
				// 必须先调用 OpenProjectDb_DownloadIfNotExist 拉取最新 .db 到本地，再执行归档导出。
				if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode)
				{
					var downloaded = await Program.MainForm.OpenProjectDb_DownloadIfNotExist(selected);
					if (downloaded == null)
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
							"无法从服务器下载项目数据，导出已取消。", MessageBoxButtons.OK, "导出失败");
						return;
					}
				}
				await Task.Run(() => ProjectArchive.Export(selected, sfd.FileName));
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					$"项目导出成功！\n文件路径：{sfd.FileName}\n\n可将此文件发送给其他人员，通过\"导入项目\"功能打开。",
					MessageBoxButtons.OK, "导出完成");
			}
			catch (Exception ex)
			{
				ex.Log();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"导出失败！失败原因：" + ex.Message, MessageBoxButtons.OK, "导出失败");
			}
		}
	}

	/// <summary>从 .auditai 归档文件导入项目</summary>
	private async Task ImportProject()
	{
		using (var ofd = new OpenFileDialog
		{
			Filter = "项目归档文件 (*.auditai)|*.auditai",
			Title = "导入项目",
			CheckFileExists = true
		})
		{
			if (ofd.ShowDialog() != DialogResult.OK) return;
			try
			{
				// 读取元信息预览
				var metadata = ProjectArchive.ReadMetadata(ofd.FileName);

				// 占位创建时间（2000-01-01）显示为"未设置"
				string createTimeDisplay;
				var placeholder = new DateTime(2000, 1, 1);
				if (metadata.CreateTime == default ||
					(metadata.CreateTime >= placeholder && metadata.CreateTime < placeholder.AddDays(1)))
				{
					createTimeDisplay = "未设置（将使用当前时间）";
				}
				else
				{
					createTimeDisplay = metadata.CreateTime.ToString("yyyy-MM-dd");
				}

				string currentUser = Auditai.Model.User.Current?.Name ??
									Auditai.Model.User.Current?.UserName ??
									"当前登录用户";

				string preview = $"项目名称：{metadata.ProjectName}\n" +
					$"项目编号：{metadata.ProjectNumber}\n" +
					$"项目类别：{metadata.Category}\n" +
					$"被审计单位：{metadata.Auditee}\n" +
					$"创建时间：{createTimeDisplay}\n" +
					$"导出时间：{metadata.ExportTime:yyyy-MM-dd HH:mm}\n" +
					$"导出人：{metadata.ExportedBy}\n" +
					$"版本：v{metadata.SchemaVersion}\n" +
					$"——————\n" +
					$"创建者：{currentUser}（导入操作人）\n" +
					$"项目经理：{currentUser}（导入操作人，管理员权限）";

				if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question,
					$"确认导入以下项目？\n\n{preview}", MessageBoxButtons.OKCancel, "导入项目确认") != DialogResult.OK)
					return;

				// 执行导入（本地模式：File.Copy + SQLite 写入；服务端模式：HTTP 上传 .db 流）
				var newProject = await Task.Run(async () => await ProjectArchive.ImportAsync(ofd.FileName));
				await Populate();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					$"项目导入成功！\n新项目名称：{newProject.Name}", MessageBoxButtons.OK, "导入完成");
			}
			catch (Exception ex)
			{
				ex.Log();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"导入失败！失败原因：" + ex.Message, MessageBoxButtons.OK, "导入失败");
			}
		}
	}

	/// <summary>导出选中模板为 .auditaitemplate 归档文件（含模板数据库和元信息）</summary>
	private async Task ExportTemplateFile()
	{
		var selected = SelectedProject;
		if (selected == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要导出的" + StringConstBase.Current.Template);
			return;
		}
		if (selected.Type != ProjectType.Template)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
				"请选择一个" + StringConstBase.Current.Template + "（当前选择的是" + StringConstBase.Current.Project + "）");
			return;
		}
		string defaultName = string.IsNullOrWhiteSpace(selected.Number)
			? $"{selected.Name}.auditaitemplate"
			: $"{selected.Number} {selected.Name}.auditaitemplate";
		foreach (char c in Path.GetInvalidFileNameChars())
			defaultName = defaultName.Replace(c, '_');
		using (var sfd = new SaveFileDialog
		{
			Filter = "模板归档文件 (*.auditaitemplate)|*.auditaitemplate",
			FileName = defaultName,
			Title = "导出" + StringConstBase.Current.Template
		})
		{
			if (sfd.ShowDialog() != DialogResult.OK) return;
			try
			{
				// 服务端模式下，本地可能无 .db 缓存或缓存已过期，
				// 必须先调用 OpenProjectDb_DownloadIfNotExist 拉取最新 .db 到本地，再执行归档导出。
				if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode)
				{
					var downloaded = await Program.MainForm.OpenProjectDb_DownloadIfNotExist(selected);
					if (downloaded == null)
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
							"无法从服务器下载" + StringConstBase.Current.Template + "数据，导出已取消。", MessageBoxButtons.OK, "导出失败");
						return;
					}
				}
				await Task.Run(() => TemplateArchive.Export(selected, sfd.FileName));
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					$"{StringConstBase.Current.Template}导出成功！\n文件路径：{sfd.FileName}\n\n可将此文件发送给其他人员，通过\"导入{StringConstBase.Current.Template}\"功能打开。",
					MessageBoxButtons.OK, "导出完成");
			}
			catch (Exception ex)
			{
				ex.Log();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"导出失败！失败原因：" + ex.Message, MessageBoxButtons.OK, "导出失败");
			}
		}
	}

	/// <summary>从 .auditaitemplate 归档文件导入模板（弹出 dlgTemplateEditor 让用户编辑模板信息）</summary>
	private async Task ImportTemplate()
	{
		using (var ofd = new OpenFileDialog
		{
			Filter = "模板归档文件 (*.auditaitemplate)|*.auditaitemplate",
			Title = "导入" + StringConstBase.Current.Template,
			CheckFileExists = true
		})
		{
			if (ofd.ShowDialog() != DialogResult.OK) return;
			TemplateImportContext ctx = null;
			try
			{
				// 1. 读取元信息预览
				var metadata = TemplateArchive.ReadMetadata(ofd.FileName);

				// 占位创建时间（2000-01-01）显示为"未设置"
				string createTimeDisplay;
				var placeholder = new DateTime(2000, 1, 1);
				if (metadata.CreateTime == default ||
					(metadata.CreateTime >= placeholder && metadata.CreateTime < placeholder.AddDays(1)))
				{
					createTimeDisplay = "未设置（将使用当前时间）";
				}
				else
				{
					createTimeDisplay = metadata.CreateTime.ToString("yyyy-MM-dd");
				}

				string currentUser = Auditai.Model.User.Current?.Name ??
									Auditai.Model.User.Current?.UserName ??
									"当前登录用户";

				string preview = $"{StringConstBase.Current.Template}名称：{metadata.TemplateName}\n" +
					$"{StringConstBase.Current.Template}编号：{metadata.TemplateNumber}\n" +
					$"{StringConstBase.Current.Template}类别：{metadata.Category}\n" +
					$"创建时间：{createTimeDisplay}\n" +
					$"导出时间：{metadata.ExportTime:yyyy-MM-dd HH:mm}\n" +
					$"导出人：{metadata.ExportedBy}\n" +
					$"版本：v{metadata.SchemaVersion}\n" +
					$"——————\n" +
					$"创建者：{currentUser}（导入操作人）";

				if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question,
					$"确认导入以下{StringConstBase.Current.Template}？\n\n{preview}",
					MessageBoxButtons.OKCancel, $"导入{StringConstBase.Current.Template}确认") != DialogResult.OK)
					return;

				// 2. 解压到临时目录（不执行注册）
				ctx = await Task.Run(() => TemplateArchive.ExtractAsync(ofd.FileName));

				// 3. 弹出 dlgTemplateEditor 让用户编辑模板信息（名称/编号/类别/备注/成员/TeamVisible）
				//    复用 ShowFromProject 模式（与"另存为模板"流程一致），避免跨团队导入时成员失效
				var newTemplate = ctx.ProjectDto.Clone();
				newTemplate.Id = ctx.NewTemplateId;
				newTemplate.Type = ProjectType.Template;
				newTemplate.ChargeType = ChargeType.None;
				newTemplate.Users = new Auditai.DTO.User[1]
				{
					new Auditai.DTO.User
					{
						Id = Auditai.Model.User.Current.Id,
						UserName = Auditai.Model.User.Current.UserName,
						Role = UserRole.Editor,
						Name = Auditai.Model.User.Current.Name
					}
				};

				var editor = new dlgTemplateEditor();
				editor.Template = newTemplate;
				if (!editor.ShowFromProject())
				{
					return; // 用户取消
				}

				// 4. 将编辑后的信息写入临时 .db（重置 Id/Name/Number 等 + 同步状态）
				TemplateArchive.ApplyEditorResult(ctx, newTemplate);

				// 5. 执行注册（本地模式：File.Copy + 主库注册；服务端模式：HTTP 上传 .db 流）
				var created = await TemplateArchive.ImportAsync(ctx, newTemplate);

				_sidebar.SelectModule(TAB_TEMPLATE);
				await Populate();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					$"{StringConstBase.Current.Template}导入成功！\n新{StringConstBase.Current.Template}名称：{created.Name}",
					MessageBoxButtons.OK, "导入完成");
			}
			catch (Exception ex)
			{
				ex.Log();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"导入失败！失败原因：" + ex.Message, MessageBoxButtons.OK, "导入失败");
			}
			finally
			{
				ctx?.Dispose(); // 清理临时目录
			}
		}
	}

	private async Task SaveAsTemplate()
	{
		if (!HasSelectedProject)
		{
			return;
		}
		Guid id = SelectedProject.Id;
		Auditai.DTO.Project newTemplate = SelectedProject.Clone();
		newTemplate.Id = Guid.NewGuid();
		newTemplate.Number = string.Empty;
		newTemplate.Name = string.Empty;
		newTemplate.Category = string.Empty;
		newTemplate.Users = new Auditai.DTO.User[1]
		{
			new Auditai.DTO.User
			{
				Id = Auditai.Model.User.Current.Id,
				UserName = Auditai.Model.User.Current.UserName,
				Role = UserRole.Editor,
				Name = Auditai.Model.User.Current.Name
			}
		};
		newTemplate.Type = ProjectType.Template;
		newTemplate.ChargeType = ChargeType.None;
		dlgTemplateEditor dlgTemplateEditor2 = new dlgTemplateEditor();
		dlgTemplateEditor2.Template = newTemplate;
		if (!dlgTemplateEditor2.ShowFromProject())
		{
			return;
		}
		try
		{
			JObject jObj = new JObject();
			jObj["OldProject"] = id;
			jObj["NewProject"] = JToken.FromObject(newTemplate);
			jObj["ClearPermissions"] = true;
			ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStepIfProgressNotZero("正在另存为" + StringConstBase.Current.Template + "，请稍候...");
			progressRuntimeData.UpdateProgress(0.8f);
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				// 本地模式：调用 StorageRouter.SaveProjectAsTemplate 复制项目 .db 到 Data\Templates
				// 远程模式：调用 WebApiClient.DuplicateProject
				await Auditai.LocalDataStore.StorageRouter.SaveProjectAsTemplate(id, newTemplate);
			});
			ProjectInfoManager.GetInstance().UpdateOpenTime(newTemplate.Id.ToString(), DateTime.Now);
			_sidebar.SelectModule(TAB_TEMPLATE);
			await Populate();
			FindAndSelectRow(newTemplate);
			// P2 协同增强 Task 8：另存为模板成功后广播通知团队成员
			try
			{
				var msg = new NotifyMessage { Kind = "newproject", Value = newTemplate.Id.ToString() };
				_ = SignalRClient.BroadcastToTeamUsers(msg.ToString());
			}
			catch (Exception)
			{
			}
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private async Task ManageUsers()
	{
		Program.ManageUsers();
		await Populate();
	}

	private void UserInfo()
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持修改用户信息");
			return;
		}
		frmAlterInfo frmAlterInfo2 = new frmAlterInfo();
		if (frmAlterInfo2.ShowDialog() == DialogResult.OK)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改成功！");
		}
	}

	private void ChangePassword()
	{
		Program.MainForm.AlterPwd();
	}

	private async Task UseTemplate()
	{
		if (SelectedProject.Type != ProjectType.Template || !SelectedProject.SystemBuild || SelectedProject.ChargeType != ChargeType.Pay || !SoftwareLicenseManager.IsUsePayProjectOutOfLicenseLimit())
		{
			// 非系统模板使用权限校验：TeamVisible=false 时只有成员列表中的用户可使用
			if (SelectedProject.Type == ProjectType.Template
				&& !SelectedProject.SystemBuild
				&& !await CanUseTemplateAsync())
			{
				return;
			}
			await CreateProject(SelectedProject);
			return;
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您是" + SoftwareLicenseManager.GetUnPayedLicenseDisplayName() + "用户，您选中的" + StringConstBase.Current.Template + "为" + SoftwareLicenseManager.GetPayedLicenseDisplayName() + "用户专用，请联系官方客服升级为" + SoftwareLicenseManager.GetPayedLicenseDisplayName() + "用户后再使用该" + StringConstBase.Current.Template + "创建" + StringConstBase.Current.Project + "！");
	}

	/// <summary>
	/// 非系统模板使用权限校验。
	/// TeamVisible=true：全体团队成员可用；TeamVisible=false：只有成员列表中的用户才能使用。
	/// 系统管理员/系统支持人员/团队管理员可绕过。
	/// 列表接口返回的 Users 可能为空，远程模式下拉取完整模板详情后再校验。
	/// 拉取失败或仍无 Users 时，按可见即允许处理（与 CanModifyProject 一致，服务端最终鉴权）。
	/// </summary>
	private async Task<bool> CanUseTemplateAsync()
	{
		var current = Auditai.Model.User.Current;
		if (current == null) return true;

		// 系统管理员/系统支持人员/团队管理员可绕过
		if (current.IsSystemAdmin || current.IsSystemSupporter || current.IsTeamAdmin) return true;

		// TeamVisible=true：全体团队成员可用
		if (SelectedProject.TeamVisible) return true;

		// TeamVisible=false：检查成员列表
		var users = SelectedProject.Users;

		// 列表接口返回的 Users 可能为空，远程模式下拉取完整模板详情
		if ((users == null || !users.Any()) && !Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			try
			{
				var fullTemplate = await WebApiClient.GetProjectDto(SelectedProject.Id);
				users = fullTemplate?.Users;
			}
			catch (HttpRequestException)
			{
				return true; // 拉取失败，按可见即允许（服务端最终鉴权）
			}
		}

		// 仍无 Users 信息，按可见即允许处理（与 CanModifyProject 一致）
		if (users == null || !users.Any()) return true;

		// 检查当前用户是否在成员列表中（Editor 或 User 角色均可使用）
		if (!users.Any(u => u.Id == current.Id))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
				"您不在该" + StringConstBase.Current.Template + "的成员列表中，无法使用此" +
				StringConstBase.Current.Template + "。请联系" + StringConstBase.Current.Template +
				"管理员将您添加为成员，或使用其他" + StringConstBase.Current.Template + "。");
			return false;
		}
		return true;
	}

	private async Task CreateTemplate()
	{
		if (Auditai.Model.User.Current.TeamId == Guid.Empty)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您还未加入组织，请点击“同事管理”，创建一个组织，或者让同事邀请您进入现有的组织，开始体验协同办公！");
			return;
		}
		Auditai.DTO.Project template = Auditai.Model.User.Current.GetNewTemplateCandidate();
		dlgTemplateEditor dlgTemplateEditor2 = new dlgTemplateEditor();
		dlgTemplateEditor2.Template = template;
		if (!dlgTemplateEditor2.ShowCreate())
		{
			return;
		}
		try
		{
			ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStepIfProgressNotZero("正在创建" + StringConstBase.Current.Template + "，请稍后...");
			progressRuntimeData.UpdateProgress(0.8f);
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				await Auditai.LocalDataStore.StorageRouter.CreateProject(template);
			});
			ProjectInfoManager.GetInstance().UpdateOpenTime(template.Id.ToString(), DateTime.Now);
			await Populate();
			FindAndSelectRow(template);
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private async Task ModifyTemplate()
	{
		if (!HasSelectedProject)
		{
			return;
		}
		// 防御性兜底：系统下发的模板仅系统管理员/系统支持人员可编辑
		// 即使 CanOpenTemplate 漏判，这里也阻止误操作
		if (SelectedProject.SystemBuild && !Auditai.Model.User.Current.IsSystemAdmin && !Auditai.Model.User.Current.IsSystemSupporter)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
				"系统下发的模板仅系统管理员可编辑，请使用「复制" + StringConstBase.Current.Template + "」创建团队副本后编辑");
			return;
		}
		Auditai.DTO.Project clone = SelectedProject.Clone();
		dlgTemplateEditor dlgTemplateEditor2 = new dlgTemplateEditor();
		dlgTemplateEditor2.Template = clone;
		if (!dlgTemplateEditor2.ShowModify())
		{
			return;
		}
		try
		{
			// 本地模式：调用 StorageRouter.UpdateTemplate 更新模板 .db 文件
			// 远程模式：调用 WebApiClient.UpdateProject
			await Auditai.LocalDataStore.StorageRouter.UpdateTemplate(clone);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改" + StringConstBase.Current.Template + "成功");
			ProjectInfoManager.GetInstance().UpdateOpenTime(clone.Id.ToString(), DateTime.Now);
			await Populate();
			await SignalRClient.ChangeProjectMember(clone.Id.ToString());
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private async Task DuplicateTemplate()
	{
		if (!HasSelectedProject)
		{
			return;
		}
		if (SelectedProject.Type == ProjectType.Template && SelectedProject.SystemBuild && SelectedProject.ChargeType == ChargeType.Pay && SoftwareLicenseManager.IsUsePayProjectOutOfLicenseLimit())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您是" + SoftwareLicenseManager.GetUnPayedLicenseDisplayName() + "用户，您选中的" + StringConstBase.Current.Template + "为" + SoftwareLicenseManager.GetPayedLicenseDisplayName() + "用户专用，请连续官方客服升级为" + SoftwareLicenseManager.GetPayedLicenseDisplayName() + "用户后再复制该" + StringConstBase.Current.Template + "！");
			return;
		}
		Guid id = SelectedProject.Id;
		Auditai.DTO.Project newTemplate = SelectedProject.Clone();
		newTemplate.Id = Guid.NewGuid();
		newTemplate.Number += " - 副本";
		newTemplate.Name += " - 副本";
		newTemplate.Users = new List<Auditai.DTO.User>
		{
			new Auditai.DTO.User
			{
				Id = Auditai.Model.User.Current.Id,
				Name = Auditai.Model.User.Current.Name,
				UserName = Auditai.Model.User.Current.UserName,
				Role = UserRole.Editor
			}
		};
		dlgTemplateEditor dlgTemplateEditor2 = new dlgTemplateEditor();
		dlgTemplateEditor2.Template = newTemplate;
		if (!dlgTemplateEditor2.ShowDuplicate())
		{
			return;
		}
		try
		{
			JObject jObj = new JObject();
			jObj["OldProject"] = id;
			jObj["NewProject"] = JToken.FromObject(newTemplate);
			jObj["ClearPermissions"] = false;
			ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStepIfProgressNotZero("正在执行复制，请稍候...");
			progressRuntimeData.UpdateProgress(0.8f);
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				// 本地模式：调用 StorageRouter.DuplicateTemplate 复制模板 .db 文件
				// 远程模式：调用 WebApiClient.DuplicateProject
				await Auditai.LocalDataStore.StorageRouter.DuplicateTemplate(id, newTemplate);
			});
			ProjectInfoManager.GetInstance().UpdateOpenTime(newTemplate.Id.ToString(), DateTime.Now);
			await Populate();
			FindAndSelectRow(newTemplate);
			// P2 协同增强 Task 8：复制模板成功后广播通知团队成员
			try
			{
				var msg = new NotifyMessage { Kind = "newproject", Value = newTemplate.Id.ToString() };
				_ = SignalRClient.BroadcastToTeamUsers(msg.ToString());
			}
			catch (Exception)
			{
			}
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
	}

	private void ToggleSearch()
	{
		_isSearch = !_isSearch;
		PopulateSearch();
		PopulateModel();
	}

	private void PopulateSearch()
	{
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			// 项目/模板：分类筛选已合并进 KPI 条，搜索时隐藏分类项（保留统计项）
			if (State == ViewState.Project || State == ViewState.Template)
			{
				_kpiStrip.ShowCategories = !_isSearch;
			}
			if (_isSearch)
			{
				GetSideCommand(RB_SEARCH).Text = "关闭搜索";
				_pnlSearch.Show();
			}
			else
			{
				GetSideCommand(RB_SEARCH).Text = ((State == ViewState.Project) ? ("搜索" + StringConstBase.Current.Project) : ("搜索" + StringConstBase.Current.Template));
				_pnlSearch.Hide();
			}
		}
		else if (State == ViewState.RecycleProject || State == ViewState.RecycleTemplate)
		{
			_pnlSearch.Hide();
		}
		else if (State == ViewState.Review || State == ViewState.Archive || State == ViewState.Validation)
		{
			// 稽核页与审核/归档页一样无搜索面板：漏掉 Hide 会把项目管理页开启的搜索面板泄漏到本页
			_pnlSearch.Hide();
		}
		else if (_isSearch)
		{
			GetSideCommand(RB_SEARCH).Text = "关闭搜索";
			_pnlSearch.Show();
		}
		else
		{
			GetSideCommand(RB_SEARCH).Text = ((State == ViewState.Project) ? ("搜索" + StringConstBase.Current.Project) : ("搜索" + StringConstBase.Current.Template));
			_pnlSearch.Hide();
		}
	}

	private void SetCommandState()
	{
		if (State == ViewState.Project)
		{
			GetSideCommand(RB_OPENPROJECT).Enabled = HasSelectedProject;
			GetSideCommand(RB_MODIFYPROJECT).Enabled = CanModifyProject();
			GetSideCommand(RB_DELETEPROJECT).Enabled = CanModifyProject();
			GetSideCommand(RB_DUPLICATEPROJECT).Enabled = CanDuplicateProject();
			GetSideCommand(RB_EXPORTPROJECT).Enabled = CanDuplicateProject();
			GetSideCommand(RB_EXPORTPROJECTFILE).Enabled = HasSelectedProject;
			GetSideCommand(RB_SAVEASTEMPLATE).Enabled = CanDuplicateProject();
			GetSideCommand(RB_SHAREPROJECT).Visible = CanSeeShareProject();
			GetSideCommand(RB_SHAREPROJECT).Enabled = CanShareProject();
		}
		else if (State == ViewState.Template)
		{
			GetSideCommand(RB_USETEMPLATE).Enabled = HasSelectedProject;
			GetSideCommand(RB_OPENTEMPLATE).Enabled = CanOpenTemplate();
			GetSideCommand(RB_MODIFYTEMPLATE).Enabled = CanOpenTemplate();
			GetSideCommand(RB_DELETETEMPLATE).Enabled = CanOpenTemplate();
			GetSideCommand(RB_DUPLICATETEMPLATE).Enabled = HasSelectedProject;
			GetSideCommand(RB_EXPORTTEMPLATE).Enabled = HasSelectedProject;
			GetSideCommand(RB_SHARETEMPLATE).Visible = CanSeeShareProject();
			GetSideCommand(RB_SHARETEMPLATE).Enabled = CanOpenTemplate();
		}
		else if (State == ViewState.RecycleProject)
		{
			GetSideCommand(RB_DELETESELECT).Text = "删除所选" + StringConstBase.Current.Project;
			GetSideCommand(RB_DELETESELECT).Enabled = SelectedProjects.Count > 0;
			GetSideCommand(RB_RESTORESELECT).Text = "恢复所选" + StringConstBase.Current.Project;
			GetSideCommand(RB_RESTORESELECT).Enabled = SelectedProjects.Count > 0;
		}
		else if (State == ViewState.RecycleTemplate)
		{
			GetSideCommand(RB_DELETESELECT).Text = "删除所选" + StringConstBase.Current.Template;
			GetSideCommand(RB_DELETESELECT).Enabled = SelectedProjects.Count > 0;
			GetSideCommand(RB_RESTORESELECT).Text = "恢复所选" + StringConstBase.Current.Template;
			GetSideCommand(RB_RESTORESELECT).Enabled = SelectedProjects.Count > 0;
		}
		else if (State == ViewState.Review)
		{
			ReviewSubmissionDto sel = SelectedReviewSubmission;
			bool pending = sel != null && sel.Status == 0;
			bool isCurrentReviewer = pending && IsCurrentReviewer(sel);
			GetSideCommand(RB_SUBMITREVIEW).Enabled = true;
			GetSideCommand(RB_APPROVEREVIEW).Enabled = isCurrentReviewer;
			GetSideCommand(RB_REJECTREVIEW).Enabled = isCurrentReviewer;
			GetSideCommand(RB_WITHDRAWREVIEW).Enabled = pending && sel.SubmitterId == Auditai.Model.User.Current.Id;
			GetSideCommand(RB_OPENREADONLY).Enabled = isCurrentReviewer;
			GetSideCommand(RB_REVIEWHISTORY).Enabled = sel != null;
		}
		else if (State == ViewState.Archive)
		{
			GetSideCommand(RB_ARCHIVENOW).Enabled = SelectedArchiveCandidate != null;
			GetSideCommand(RB_UNARCHIVE).Enabled = SelectedArchiveRecord != null;
			GetSideCommand(RB_EXPORTARCHIVE).Enabled = SelectedArchiveRecord != null;
		}
		else if (State == ViewState.Validation)
		{
			bool hasProject = _validationStore != null;
			bool hasRule = SelectedValidationRule != null;
			GetSideCommand(RB_RUNVALIDATION).Enabled = hasProject;
			GetSideCommand(RB_EXPORTREPORT).Enabled = hasProject && _validationOutcomes != null;
			GetSideCommand(RB_ADDRULE).Enabled = hasProject;
			GetSideCommand(RB_EDITRULE).Enabled = hasRule;
			GetSideCommand(RB_DELETERULE).Enabled = hasRule;
			GetSideCommand(RB_IMPORTRULES).Enabled = hasProject;
			GetSideCommand(RB_EXPORTRULES).Enabled = hasProject;
		}
	}

	private bool CanModifyProject()
	{
		if (!HasSelectedProject)
		{
			return false;
		}
		// 服务端列表接口（QueryProjectsAsync/ReadProject）返回的 Users 可能为空集合，
		// 此时若当前用户可见该项目，视为 Manager（实际删除/修改由服务端最终鉴权）。
		var users = SelectedProject.Users;
		if (users == null || !users.Any())
		{
			return true;
		}
		Auditai.DTO.User user = users.FirstOrDefault((Auditai.DTO.User u) => u.Id == Auditai.Model.User.Current.Id);
		if (user == null)
		{
			// 当前用户不在成员列表中（服务端未返回完整成员），按可见即允许处理
			return true;
		}
		return user.Role == UserRole.Manager;
	}

	private bool CanDuplicateProject()
	{
		if (!HasSelectedProject)
		{
			return false;
		}
		// 同 CanModifyProject：服务端返回空 Users 时按可见即允许处理。
		var users = SelectedProject.Users;
		if (users == null || !users.Any())
		{
			return true;
		}
		Auditai.DTO.User user = users.FirstOrDefault((Auditai.DTO.User u) => u.Id == Auditai.Model.User.Current.Id);
		if (user == null)
		{
			return true;
		}
		if (user.Role != 0)
		{
			return user.Role == UserRole.Checker;
		}
		return true;
	}

	private bool CanSeeShareProject()
	{
		if (Program.IsOnPremise)
		{
			return false;
		}
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (!Auditai.Model.User.Current.IsTeamAdmin)
		{
			return false;
		}
		if (!SoftwareLicenseManager.IsAllowShowShareProjectButton())
		{
			return false;
		}
		return true;
	}

	private bool CanShareProject()
	{
		if (!HasSelectedProject)
		{
			return false;
		}
		return true;
	}

	private bool CanOpenTemplate()
	{
		if (!HasSelectedProject)
		{
			return false;
		}
		// 系统下发的模板（SystemBuild=true）：非系统管理员/系统支持人员以只读模式打开查看，
		// 表格只读由 HasWritePermission（系统模板 Users 为空）保证；编辑需通过「复制模板」创建团队副本
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		// 同 CanModifyProject：服务端返回空 Users 时按可见即允许处理。
		var users = SelectedProject.Users;
		if (users == null || !users.Any())
		{
			return true;
		}
		Auditai.DTO.User user = users.FirstOrDefault((Auditai.DTO.User u) => u.Id == Auditai.Model.User.Current.Id);
		if (user == null)
		{
			return true;
		}
		return user.Role == UserRole.Editor;
	}

	#region 上报审核与项目归档（仅服务器模式）

	private ReviewSubmissionDto SelectedReviewSubmission
	{
		get
		{
			if (_lvReview == null || _lvReview.SelectedItems.Count == 0)
			{
				return null;
			}
			return _lvReview.SelectedItems[0].Tag as ReviewSubmissionDto;
		}
	}

	private ProjectArchiveDto SelectedArchiveRecord
	{
		get
		{
			if (_archiveShowCandidates || _lvArchive == null || _lvArchive.SelectedItems.Count == 0)
			{
				return null;
			}
			return _lvArchive.SelectedItems[0].Tag as ProjectArchiveDto;
		}
	}

	private Auditai.DTO.Project SelectedArchiveCandidate
	{
		get
		{
			if (!_archiveShowCandidates || _lvArchive == null || _lvArchive.SelectedItems.Count == 0)
			{
				return null;
			}
			return _lvArchive.SelectedItems[0].Tag as Auditai.DTO.Project;
		}
	}

	private static string GetReviewStatusText(int status)
	{
		switch (status)
		{
		case 0: return "审批中";
		case 1: return "已通过";
		case 2: return "已退回";
		case 3: return "已撤回";
		default: return "未知";
		}
	}

	private static string GetCurrentReviewerName(ReviewSubmissionDto s)
	{
		if (s.Status != 0 || s.Nodes == null)
		{
			return "—";
		}
		ReviewNodeDto node = s.Nodes.FirstOrDefault((ReviewNodeDto n) => n.Level == s.CurrentLevel && n.Status == 0);
		return node?.ReviewerName ?? "—";
	}

	private static bool IsCurrentReviewer(ReviewSubmissionDto s)
	{
		if (s.Status != 0 || s.Nodes == null)
		{
			return false;
		}
		return s.Nodes.Any((ReviewNodeDto n) => n.Level == s.CurrentLevel && n.Status == 0 && n.ReviewerId == Auditai.Model.User.Current.Id);
	}

	private static string FormatServerTime(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return "—";
		}
		if (DateTime.TryParse(value, out DateTime dt))
		{
			return dt.ToString("yyyy-MM-dd HH:mm");
		}
		return value;
	}

	private void PopulateReviewList()
	{
		_lvReview.BeginUpdate();
		_lvReview.Columns.Clear();
		_lvReview.Items.Clear();
		_lvReview.Columns.Add("项目名称", 180);
		_lvReview.Columns.Add("轮次", 50, HorizontalAlignment.Center);
		_lvReview.Columns.Add("状态", 70, HorizontalAlignment.Center);
		_lvReview.Columns.Add("当前级别", 70, HorizontalAlignment.Center);
		_lvReview.Columns.Add("提交人", 90);
		_lvReview.Columns.Add("提交时间", 120);
		_lvReview.Columns.Add("当前审批人", 110);
		foreach (ReviewSubmissionDto s in _reviewSubmissions)
		{
			ListViewItem item = new ListViewItem(string.IsNullOrWhiteSpace(s.ProjectName) ? s.ProjectId : s.ProjectName)
			{
				Tag = s,
				UseItemStyleForSubItems = false
			};
			item.SubItems.Add(s.Round.ToString());
			item.SubItems.Add(GetReviewStatusText(s.Status));
			item.SubItems.Add(s.Status == 0 ? $"{s.CurrentLevel}/{s.TotalLevel}" : "—");
			item.SubItems.Add(s.SubmitterName ?? "—");
			item.SubItems.Add(FormatServerTime(s.SubmitTime));
			item.SubItems.Add(GetCurrentReviewerName(s));
			item.SubItems[2].ForeColor = GetReviewStatusColor(s.Status);
			_lvReview.Items.Add(item);
		}
		_lvReview.EndUpdate();
		AutoSizeLvColumns(_lvReview, ReviewColumnRatios);
	}

	/// <summary>审核页状态色（GetReviewStatusText 的 0-3 语义：0审批中/1已通过/2已退回/3已撤回）。</summary>
	private static Color GetReviewStatusColor(int status)
	{
		switch (status)
		{
		case 0:
			return AuditTheme.Brand;
		case 1:
			return AuditTheme.SuccessText;
		case 2:
			return AuditTheme.ErrorText;
		default:
			return AuditTheme.Slate;
		}
	}

	private void PopulateArchiveList()
	{
		_lvArchive.BeginUpdate();
		_lvArchive.Columns.Clear();
		_lvArchive.Items.Clear();
		if (_archiveShowCandidates)
		{
			_lvArchive.Columns.Add("项目名称", 220);
			_lvArchive.Columns.Add("项目编号", 140);
			_lvArchive.Columns.Add("审核状态", 90, HorizontalAlignment.Center);
			foreach (Auditai.DTO.Project p in _archiveCandidates)
			{
				ListViewItem item = new ListViewItem(p.Name ?? p.Id.ToString())
				{
					Tag = p,
					UseItemStyleForSubItems = false
				};
				item.SubItems.Add(p.Number ?? "—");
				item.SubItems.Add("已通过");
				item.SubItems[2].ForeColor = AuditTheme.SuccessText;
				_lvArchive.Items.Add(item);
			}
		}
		else
		{
			_lvArchive.Columns.Add("项目名称", 200);
			_lvArchive.Columns.Add("归档编号", 130);
			_lvArchive.Columns.Add("归档人", 90);
			_lvArchive.Columns.Add("归档时间", 130);
			_lvArchive.Columns.Add("保管年限", 70, HorizontalAlignment.Center);
			foreach (ProjectArchiveDto a in _archiveRecords)
			{
				ListViewItem item = new ListViewItem(string.IsNullOrWhiteSpace(a.ProjectName) ? a.ProjectId : a.ProjectName)
				{
					Tag = a
				};
				item.SubItems.Add(a.ArchiveNo ?? "—");
				item.SubItems.Add(a.OperatorName ?? "—");
				item.SubItems.Add(FormatServerTime(a.ArchiveTime));
				item.SubItems.Add(a.RetentionYears.ToString());
				_lvArchive.Items.Add(item);
			}
		}
		_lvArchive.EndUpdate();
		AutoSizeLvColumns(_lvArchive, _archiveShowCandidates ? ArchiveCandidateColumnRatios : ArchiveRecordColumnRatios);
	}

	// ============= 稽核检查页 =============

	/// <summary>确保 _validationStore 对应当前选中的项目（切换项目时重建并重置校验结果缓存）。须在 UI 线程调用（读取 SelectedProject）。</summary>
	private void EnsureValidationStore()
	{
		Auditai.DTO.Project sp = SelectedProject;
		if (sp == null || sp.Type == ProjectType.Template)
		{
			_validationStore = null;
			_validationProjectId = Guid.Empty;
			return;
		}
		if (_validationStore != null && _validationProjectId == sp.Id)
		{
			return;
		}
		_validationStore?.Release();
		_validationStore = new ValidationRuleStore(sp);
		_validationProjectId = sp.Id;
		_validationOutcomes = null;
	}

	private void PopulateValidationList()
	{
		_lvValidation.BeginUpdate();
		_lvValidation.Columns.Clear();
		_lvValidation.Groups.Clear();
		_lvValidation.Items.Clear();
		if (_validationStore == null || _validationRules == null)
		{
			_lvValidation.Columns.Add("提示", 620);
			ListViewItem hint = new ListViewItem("请先在左侧选择\"" + StringConstBase.Current.Project + "管理\"并选中一个" + StringConstBase.Current.Project + "，再回到本页查看稽核规则");
			hint.ForeColor = Color.Gray;
			_lvValidation.Items.Add(hint);
			_lvValidation.EndUpdate();
			return;
		}
		_lvValidation.Columns.Add("类型", 80, HorizontalAlignment.Center);
		_lvValidation.Columns.Add("归属", 130);
		_lvValidation.Columns.Add("左值", 150);
		_lvValidation.Columns.Add("运算符", 55, HorizontalAlignment.Center);
		_lvValidation.Columns.Add("右值", 150);
		_lvValidation.Columns.Add("说明", 120);
		_lvValidation.Columns.Add("校验结果", 100, HorizontalAlignment.Center);
		ListViewGroup grpTable = new ListViewGroup("表格校验规则");
		ListViewGroup grpDoc = new ListViewGroup("文档稽核规则");
		_lvValidation.Groups.AddRange(new ListViewGroup[2] { grpTable, grpDoc });
		Dictionary<Id64, List<ValidationRuleStore.ValidationOutcome>> outcomeMap = new Dictionary<Id64, List<ValidationRuleStore.ValidationOutcome>>();
		if (_validationOutcomes != null)
		{
			foreach (ValidationRuleStore.ValidationOutcome oc in _validationOutcomes)
			{
				if (!outcomeMap.TryGetValue(oc.RuleId, out var list))
				{
					list = new List<ValidationRuleStore.ValidationOutcome>();
					outcomeMap[oc.RuleId] = list;
				}
				list.Add(oc);
			}
		}
		foreach (Auditai.DTO.ValidationFormula vf in _validationRules.Formulas)
		{
			bool isDocRule = !vf.DocumentFieldId.IsZero();
			ListViewItem item = new ListViewItem(isDocRule ? "稽核规则" : "表格校验")
			{
				Tag = vf,
				Group = isDocRule ? grpDoc : grpTable
			};
			item.SubItems.Add(_validationRules.ResolveOwner(vf));
			item.SubItems.Add(vf.LeftExpr ?? "");
			item.SubItems.Add(ValidationRuleStore.ValidationOutcome.OperatorSymbol(vf.Operator));
			item.SubItems.Add(vf.RightExpr ?? "");
			item.SubItems.Add(vf.Note ?? "");
			if (outcomeMap.TryGetValue(vf.Id, out var ocList))
			{
				int failed = ocList.Count((ValidationRuleStore.ValidationOutcome o) => !o.Passed);
				int errCount = ocList.Count((ValidationRuleStore.ValidationOutcome o) => o.Error != null);
				if (errCount > 0)
				{
					item.SubItems.Add($"求值错误 {errCount} 项");
					item.ForeColor = Color.FromArgb(180, 90, 0);
				}
				else if (failed > 0)
				{
					item.SubItems.Add($"未通过 {failed}/{ocList.Count}");
					item.ForeColor = Color.FromArgb(200, 60, 60);
				}
				else
				{
					item.SubItems.Add($"通过 {ocList.Count} 项");
					item.ForeColor = Color.FromArgb(60, 140, 70);
				}
			}
			else if (_validationOutcomes != null)
			{
				item.SubItems.Add(isDocRule ? "需在文档中校验" : "未执行");
				item.ForeColor = Color.Gray;
			}
			else
			{
				item.SubItems.Add("—");
			}
			_lvValidation.Items.Add(item);
		}
		if (_lvValidation.Items.Count == 0)
		{
			ListViewItem empty = new ListViewItem("当前" + StringConstBase.Current.Project + "还没有稽核规则，可点击\"新增规则\"或打开表格/文档添加")
			{
				ForeColor = Color.Gray
			};
			_lvValidation.Items.Add(empty);
		}
		_lvValidation.EndUpdate();
	}

	private Auditai.DTO.ValidationFormula SelectedValidationRule => _lvValidation.SelectedItems.Count > 0 ? _lvValidation.SelectedItems[0].Tag as Auditai.DTO.ValidationFormula : null;

	private List<Auditai.DTO.ValidationFormula> SelectedValidationRules => _lvValidation.SelectedItems.Cast<ListViewItem>().Select((ListViewItem i) => i.Tag as Auditai.DTO.ValidationFormula).Where((Auditai.DTO.ValidationFormula v) => v != null).ToList();

	private async Task RunProjectValidation()
	{
		if (_validationStore == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选中一个" + StringConstBase.Current.Project);
			return;
		}
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		List<ValidationRuleStore.ValidationOutcome> outcomes = null;
		string runError = null;
		progressRuntimeData.NextStep("正在准备校验，请稍后...");
		progressForm.ShowDialog(progressRuntimeData, async delegate
		{
			await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
			try
			{
				outcomes = await _validationStore.RunValidationAsync((string msg) => progressRuntimeData.UpdateMessage(msg));
			}
			catch (Exception ex)
			{
				runError = ex.InnerException?.Message ?? ex.Message;
			}
		});
		if (runError != null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "校验执行失败：" + runError);
			return;
		}
		_validationOutcomes = outcomes ?? new List<ValidationRuleStore.ValidationOutcome>();
		PopulateValidationList();
		int passed = _validationOutcomes.Count((ValidationRuleStore.ValidationOutcome o) => o.Passed);
		int failed = _validationOutcomes.Count((ValidationRuleStore.ValidationOutcome o) => !o.Passed && o.Error == null);
		int errors = _validationOutcomes.Count((ValidationRuleStore.ValidationOutcome o) => o.Error != null);
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"稽核校验完成。\r\n\r\n通过：{passed} 项\r\n未通过：{failed} 项\r\n求值错误：{errors} 项\r\n\r\n文档域稽核规则请在打开文档后执行\"文档校验\"。");
	}

	private void ExportValidationReport()
	{
		if (_validationOutcomes == null || _validationStore == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先执行校验后再导出报告");
			return;
		}
		string reportName = string.Join("_", (SelectedProject?.Name ?? "项目").Split(Path.GetInvalidFileNameChars()));
		using SaveFileDialog dlg = new SaveFileDialog
		{
			Filter = "HTML 报告|*.html",
			FileName = reportName + "-稽核校验报告.html"
		};
		if (dlg.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		StringBuilder html = new StringBuilder();
		html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>稽核校验报告</title>");
		html.Append("<style>body{font-family:'微软雅黑',sans-serif;margin:24px;color:#222}h1{font-size:20px}h2{font-size:15px;margin-top:24px}");
		html.Append("table{border-collapse:collapse;width:100%;font-size:12px}th,td{border:1px solid #ccc;padding:6px 8px;text-align:left}");
		html.Append("th{background:#f0f4f8}.pass{color:#2c8c46}.fail{color:#c83c3c}.err{color:#b45a00}.muted{color:#888;font-size:12px}</style></head><body>");
		html.Append("<h1>稽核校验报告</h1>");
		Auditai.DTO.Project sp = SelectedProject;
		html.Append($"<p class=\"muted\">项目：{System.Web.HttpUtility.HtmlEncode(sp?.Name)}　生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}　执行人：{Auditai.Model.User.Current.Name}</p>");
		int passed = _validationOutcomes.Count((ValidationRuleStore.ValidationOutcome o) => o.Passed);
		int failed = _validationOutcomes.Count((ValidationRuleStore.ValidationOutcome o) => !o.Passed && o.Error == null);
		int errors = _validationOutcomes.Count((ValidationRuleStore.ValidationOutcome o) => o.Error != null);
		html.Append($"<h2>统计：通过 {passed} 项，未通过 {failed} 项，求值错误 {errors} 项</h2>");
		html.Append("<table><tr><th>归属</th><th>校验式</th><th>左值</th><th>右值</th><th>结果</th><th>说明</th></tr>");
		foreach (ValidationRuleStore.ValidationOutcome o in _validationOutcomes)
		{
			string expr = $"{System.Web.HttpUtility.HtmlEncode(o.LeftExpr)} {ValidationRuleStore.ValidationOutcome.OperatorSymbol(o.OperatorCode)} {System.Web.HttpUtility.HtmlEncode(o.RightExpr)}";
			string cls = o.Error != null ? "err" : (o.Passed ? "pass" : "fail");
			string result = o.Error != null ? "求值错误" : (o.Passed ? "通过" : "未通过");
			if (o.RowIndex >= 0 && !o.Passed && o.Error == null)
			{
				result += $"（第 {o.RowIndex + 1} 行）";
			}
			html.Append($"<tr><td>{System.Web.HttpUtility.HtmlEncode(o.Owner)}</td><td>{expr}</td><td>{System.Web.HttpUtility.HtmlEncode(o.LeftValue ?? "")}</td><td>{System.Web.HttpUtility.HtmlEncode(o.RightValue ?? "")}</td><td class=\"{cls}\">{result}</td><td>{System.Web.HttpUtility.HtmlEncode(o.Note ?? "")}</td></tr>");
		}
		html.Append("</table></body></html>");
		File.WriteAllText(dlg.FileName, html.ToString(), Encoding.UTF8);
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "报告已导出：" + dlg.FileName);
	}

	private async Task ImportValidationRules()
	{
		if (_validationStore == null || _validationRules == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选中一个" + StringConstBase.Current.Project);
			return;
		}
		using OpenFileDialog dlg = new OpenFileDialog
		{
			Filter = "稽核规则文件|*.json"
		};
		if (dlg.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		List<ValidationRuleExportItem> importedItems;
		try
		{
			importedItems = JsonConvert.DeserializeObject<List<ValidationRuleExportItem>>(File.ReadAllText(dlg.FileName)) ?? new List<ValidationRuleExportItem>();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "规则文件解析失败：" + ex.Message);
			return;
		}
		importedItems = importedItems.Where((ValidationRuleExportItem i) => !string.IsNullOrEmpty(i.LeftExpr) || !string.IsNullOrEmpty(i.RightExpr)).ToList();
		if (importedItems.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "规则文件中没有有效规则");
			return;
		}
		// 按表格名匹配归属：跨项目导入时 TableId 不同，需按源文件记录的表格名重新映射
		Dictionary<string, Id64> nameToId = _validationRules.NodeNames
			.Where((KeyValuePair<long, string> kv) => kv.Value != null)
			.GroupBy((KeyValuePair<long, string> kv) => kv.Value)
			.ToDictionary((IGrouping<string, KeyValuePair<long, string>> g) => g.Key, (IGrouping<string, KeyValuePair<long, string>> g) => new Id64(g.First().Key));
		List<Auditai.DTO.ValidationFormula> toSave = new List<Auditai.DTO.ValidationFormula>();
		try
		{
			foreach (ValidationRuleExportItem item in importedItems)
			{
				Auditai.DTO.ValidationFormula f = new Auditai.DTO.ValidationFormula
				{
					// clamp 运算符：ValidationOperator.FromCode 对越界值会 IndexOutOfRange
					Operator = Math.Min(Math.Max(item.Operator, 0), 5),
					LeftExpr = item.LeftExpr,
					RightExpr = item.RightExpr,
					Note = item.Note,
					TableId = (!string.IsNullOrEmpty(item.TableName) && nameToId.TryGetValue(item.TableName, out var mapped)) ? mapped : Id64.Zero,
					DocumentFieldId = Id64.Zero,
					Status = 0
				};
				f.Id = await _validationStore.NewIdAsync();
				toSave.Add(f);
			}
			await _validationStore.SaveAsync(toSave);
			_validationStore.SyncChangesToSession(toSave);
			_validationStore.InvalidateSession();
			_validationRules = await _validationStore.LoadAsync();
			PopulateValidationList();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"已导入 {toSave.Count} 条规则。未匹配到同名表格的规则归属显示为\"未指定\"，请编辑修正。");
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导入规则失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
	}

	private void ExportValidationRules()
	{
		if (_validationStore == null || _validationRules == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选中一个" + StringConstBase.Current.Project);
			return;
		}
		if (_validationRules.Formulas.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前" + StringConstBase.Current.Project + "没有可导出的规则");
			return;
		}
		using SaveFileDialog dlg = new SaveFileDialog
		{
			Filter = "稽核规则文件|*.json",
			FileName = "稽核规则.json"
		};
		if (dlg.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		Dictionary<long, string> nodeNames = _validationRules.NodeNames;
		List<ValidationRuleExportItem> items = _validationRules.Formulas.Select((Auditai.DTO.ValidationFormula f) => ValidationRuleExportItem.FromFormula(f, nodeNames.TryGetValue(f.TableId.Value, out var tn) ? tn : "")).ToList();
		File.WriteAllText(dlg.FileName, JsonConvert.SerializeObject(items, Formatting.Indented), Encoding.UTF8);
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已导出 " + items.Count + " 条规则：" + dlg.FileName);
	}

	private async Task AddValidationRule()
	{
		if (_validationStore == null || _validationRules == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选中一个" + StringConstBase.Current.Project);
			return;
		}
		try
		{
			Id64 newId = await _validationStore.NewIdAsync();
			Auditai.DTO.ValidationFormula rule = new Auditai.DTO.ValidationFormula
			{
				Id = newId,
				Operator = 0,
				TableId = Id64.Zero,
				DocumentFieldId = Id64.Zero,
				Status = 0
			};
			Dictionary<long, string> tableNames = _validationRules.NodeNames;
			using frmValidationRuleEditor dlg = new frmValidationRuleEditor(rule, tableNames, allowChangeOwner: true);
			if (dlg.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			await _validationStore.SaveAsync(new List<Auditai.DTO.ValidationFormula> { rule });
			// 同步进复用的编辑器会话内存（如有），避免一键校验用旧 ValidationManager
			_validationStore.SyncChangesToSession(new List<Auditai.DTO.ValidationFormula> { rule });
			_validationStore.InvalidateSession();
			_validationRules = await _validationStore.LoadAsync();
			PopulateValidationList();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "新增规则失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
	}

	private async void EditSelectedValidationRule()
	{
		if (State != ViewState.Validation)
		{
			return;
		}
		Auditai.DTO.ValidationFormula rule = SelectedValidationRule;
		if (rule == null)
		{
			return;
		}
		bool isDocRule = !rule.DocumentFieldId.IsZero();
		if (isDocRule)
		{
			// 文档稽核规则的锚点在文档域 Parameters 内，只改库会造成规则库与文档不一致
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文档稽核规则保存在文档域中，请在打开文档后通过\"校验域管理\"修改，避免规则库与文档内容不一致");
			return;
		}
		try
		{
			Dictionary<long, string> tableNames = _validationRules?.NodeNames ?? new Dictionary<long, string>();
			using frmValidationRuleEditor dlg = new frmValidationRuleEditor(rule, tableNames, allowChangeOwner: true);
			if (dlg.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			await _validationStore.SaveAsync(new List<Auditai.DTO.ValidationFormula> { rule });
			_validationStore.SyncChangesToSession(new List<Auditai.DTO.ValidationFormula> { rule });
			_validationStore.InvalidateSession();
			_validationRules = await _validationStore.LoadAsync();
			PopulateValidationList();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "保存规则失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
	}

	private async Task DeleteValidationRules()
	{
		List<Auditai.DTO.ValidationFormula> rules = SelectedValidationRules;
		if (rules.Count == 0 || _validationStore == null)
		{
			return;
		}
		int docRuleCount = rules.Count((Auditai.DTO.ValidationFormula f) => !f.DocumentFieldId.IsZero());
		string extra = docRuleCount > 0 ? $"\r\n\r\n注意：其中 {docRuleCount} 条为文档稽核规则，删除规则库记录后文档域内的锚点仍会保留，建议同时在文档编辑器的\"校验域管理\"中清除。" : "";
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, $"确定删除选中的 {rules.Count} 条稽核规则吗？删除后不可恢复。{extra}", MessageBoxButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}
		try
		{
			List<Id64> removedIds = rules.Select((Auditai.DTO.ValidationFormula f) => f.Id).ToList();
			await _validationStore.DeleteAsync(removedIds);
			_validationStore.SyncChangesToSession(null, removedIds);
			_validationStore.InvalidateSession();
			_validationRules = await _validationStore.LoadAsync();
			PopulateValidationList();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "删除规则失败：" + (ex.InnerException?.Message ?? ex.Message));
		}
	}

	/// <summary>稽核规则导入导出条目：记录表格名而非 TableId，便于跨项目复用</summary>
	private class ValidationRuleExportItem
	{
		public string TableName { get; set; }

		public string LeftExpr { get; set; }

		public int Operator { get; set; }

		public string RightExpr { get; set; }

		public string Note { get; set; }

		public Auditai.DTO.ValidationFormula ToFormula()
		{
			return new Auditai.DTO.ValidationFormula
			{
				Operator = Operator,
				LeftExpr = LeftExpr,
				RightExpr = RightExpr,
				Note = Note,
				TableId = Id64.Zero,
				DocumentFieldId = Id64.Zero,
				Status = 0
			};
		}

		public static ValidationRuleExportItem FromFormula(Auditai.DTO.ValidationFormula f, string tableName)
		{
			return new ValidationRuleExportItem
			{
				TableName = tableName,
				LeftExpr = f.LeftExpr,
				Operator = f.Operator,
				RightExpr = f.RightExpr,
				Note = f.Note
			};
		}
	}

	private async Task ReviewRowDoubleClick()
	{
		if (State != ViewState.Review)
		{
			return;
		}
		ReviewSubmissionDto sel = SelectedReviewSubmission;
		if (sel == null)
		{
			return;
		}
		if (sel.Status == 0)
		{
			await ApproveOrRejectReview(isReject: false);
		}
		else
		{
			await ShowReviewHistory();
		}
	}

	private async Task ArchiveRowDoubleClick()
	{
		if (State != ViewState.Archive || _archiveShowCandidates)
		{
			return;
		}
		await OpenArchivedProject();
	}

	/// <summary>上报审核：要求先在项目管理视图选中项目</summary>
	private async Task SubmitForReview()
	{
		if (State != ViewState.Review)
		{
			return;
		}
		if (!Program.MainForm.EnsureServerAvailable())
		{
			return;
		}
		Auditai.DTO.Project sp = SelectedProject;
		if (sp == null || sp.Type != ProjectType.Project)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在项目管理中选择项目");
			return;
		}
		DialogResult dr = frmReviewSubmit.ShowSubmit(_form, sp);
		if (dr == DialogResult.OK)
		{
			// 上报成功提示已由 frmReviewSubmit 内部给出，此处仅刷新列表避免双重弹窗
			await Populate();
		}
	}

	/// <summary>通过/退回：当前节点审批人本人操作</summary>
	private async Task ApproveOrRejectReview(bool isReject)
	{
		if (State != ViewState.Review)
		{
			return;
		}
		if (!Program.MainForm.EnsureServerAvailable())
		{
			return;
		}
		ReviewSubmissionDto sel = SelectedReviewSubmission;
		if (sel == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择要审批的审核单");
			return;
		}
		if (sel.Status != 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该审核单已结束，不能再审批");
			return;
		}
		if (!IsCurrentReviewer(sel))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您不是该审核单当前节点审批人");
			return;
		}
		DialogResult dr = frmReviewApprove.ShowApprove(_form, sel, isReject);
		if (dr == DialogResult.OK)
		{
			await Populate();
		}
	}

	/// <summary>撤回：仅提交人且审批中</summary>
	private async Task WithdrawSubmission()
	{
		if (State != ViewState.Review)
		{
			return;
		}
		if (!Program.MainForm.EnsureServerAvailable())
		{
			return;
		}
		ReviewSubmissionDto sel = SelectedReviewSubmission;
		if (sel == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择要撤回的审核单");
			return;
		}
		if (sel.Status != 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "仅审批中的审核单可以撤回");
			return;
		}
		if (sel.SubmitterId != Auditai.Model.User.Current.Id)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "仅提交人可以撤回审核单");
			return;
		}
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "确定撤回该审核单吗？撤回后可修改后重新上报。", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		try
		{
			await WebApiClient.WithdrawReview(sel.Id);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "审核单已撤回");
			await Populate();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
		catch (Exception ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "撤回失败：" + ex2.Message);
		}
	}

	/// <summary>审批流程模板管理（仅团队管理员入口，服务端二次校验）</summary>
	private void ShowReviewFlowTemplates()
	{
		if (State != ViewState.Review)
		{
			return;
		}
		using (frmReviewFlowTemplates dlg = new frmReviewFlowTemplates())
		{
			dlg.ShowDialog(_form);
		}
	}

	/// <summary>审批记录：查看所选审核单项目的全部轮次</summary>
	private async Task ShowReviewHistory()
	{
		if (State != ViewState.Review)
		{
			return;
		}
		ReviewSubmissionDto sel = SelectedReviewSubmission;
		if (sel == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择要查看的审核单");
			return;
		}
		await Task.FromResult(frmReviewApprove.ShowHistory(_form, sel.ProjectId, sel.ProjectName));
	}

	/// <summary>审批人以只读方式打开待审项目（服务端对当前节点审批人放行）</summary>
	private async Task OpenReviewProjectReadonly()
	{
		if (State != ViewState.Review)
		{
			return;
		}
		if (!Program.MainForm.EnsureServerAvailable())
		{
			return;
		}
		ReviewSubmissionDto sel = SelectedReviewSubmission;
		if (sel == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择待我审批的审核单");
			return;
		}
		if (!IsCurrentReviewer(sel))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "仅当前节点审批人可以只读打开待审项目");
			return;
		}
		if (!Guid.TryParse(sel.ProjectId, out Guid projectId))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "审核单项目标识无效，无法打开");
			return;
		}
		ReviewSession.Begin(projectId);
		await OpenProjectByIdCore(projectId);
		if (_form.DialogResult != DialogResult.OK)
		{
			// 打开失败时结束只读会话，避免会话悬挂
			ReviewSession.End();
		}
	}

	/// <summary>按项目 Id 走既有服务器模式打开流程（不依赖列表选中项）</summary>
	private async Task OpenProjectByIdCore(Guid projectId)
	{
		Task<Auditai.Model.Project> task = Program.MainForm.OpenOrSwitchToProject(projectId, StringConstBase.Current.Project);
		if (task == null)
		{
			return;
		}
		Auditai.Model.Project project = await task;
		if (project == null)
		{
			return;
		}
		_isClosing = false;
		_form.DialogResult = DialogResult.OK;
		ProjectInfoManager.GetInstance().UpdateOpenTime(projectId.ToString(), DateTime.Now);
		Program.MainForm.RefreshProjectsSyncTwinkle();
	}

	/// <summary>归档：对"可归档项目"筛选中的选中项目执行归档</summary>
	private async Task ArchiveSelectedProject()
	{
		if (State != ViewState.Archive)
		{
			return;
		}
		if (!Program.MainForm.EnsureServerAvailable())
		{
			return;
		}
		Auditai.DTO.Project sp = SelectedArchiveCandidate;
		if (sp == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请在\"可归档项目\"筛选中选择要归档的项目");
			return;
		}
		string note = InputForm.Text("归档" + StringConstBase.Current.Project, "归档备注（可留空）：", "", 370);
		if (note == null)
		{
			return;
		}
		try
		{
			await WebApiClient.ArchiveProject(sp.Id.ToString(), note);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "项目归档成功");
			await Populate();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
		catch (Exception ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "归档失败：" + ex2.Message);
		}
	}

	/// <summary>取消归档：服务端仅允许 TeamAdmin</summary>
	private async Task CancelArchiveSelected()
	{
		if (State != ViewState.Archive)
		{
			return;
		}
		if (!Program.MainForm.EnsureServerAvailable())
		{
			return;
		}
		ProjectArchiveDto rec = SelectedArchiveRecord;
		if (rec == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请在\"已归档项目\"筛选中选择要取消归档的项目");
			return;
		}
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "取消归档后项目恢复可编辑，继续？", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		try
		{
			await WebApiClient.CancelArchive(rec.ProjectId);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已取消归档");
			await Populate();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
		catch (Exception ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "取消归档失败：" + ex2.Message);
		}
	}

	/// <summary>导出归档包：复用 ExportProjectFileCore（ProjectArchive.Export）</summary>
	private async Task ExportArchivePackage()
	{
		if (State != ViewState.Archive)
		{
			return;
		}
		ProjectArchiveDto rec = SelectedArchiveRecord;
		if (rec == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请在\"已归档项目\"筛选中选择要导出的归档记录");
			return;
		}
		if (!Program.MainForm.EnsureServerAvailable())
		{
			return;
		}
		Auditai.DTO.Project sp = await ResolveProjectByIdAsync(rec.ProjectId);
		if (sp == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "未找到归档项目信息，无法导出");
			return;
		}
		await ExportProjectFileCore(sp);
	}

	/// <summary>双击已归档行：提示后打开项目查看（服务端拒绝推送，查看不阻断）</summary>
	private async Task OpenArchivedProject()
	{
		ProjectArchiveDto rec = SelectedArchiveRecord;
		if (rec == null)
		{
			return;
		}
		if (!Guid.TryParse(rec.ProjectId, out Guid projectId))
		{
			return;
		}
		if (!Program.MainForm.EnsureServerAvailable())
		{
			return;
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该项目已归档，内容为只读");
		await OpenProjectByIdCore(projectId);
	}

	private async Task<Auditai.DTO.Project> ResolveProjectByIdAsync(string projectId)
	{
		if (!Guid.TryParse(projectId, out Guid gid))
		{
			return null;
		}
		Auditai.DTO.Project cached = _projects.FirstOrDefault((Auditai.DTO.Project p) => p.Id == gid);
		if (cached != null)
		{
			return cached;
		}
		return await WebApiClient.GetProjectDto(gid);
	}

	#endregion

	private C1Command GetCommand(string name)
	{
		if (_commandDic.TryGetValue(name, out var value))
		{
			return value;
		}
		return _cmdh.Commands[name];
	}

	private SimpleCommand GetSideCommand(string name)
	{
		return _topBar.GetCommand(name);
	}

	private async Task RestoreProjects()
	{
		ViewState previousState = State;
		List<Guid> o = SelectedProjects.Select((Auditai.DTO.Project p) => p.Id).ToList();
		try
		{
			JObject jObject = new JObject();
			jObject["Ids"] = JToken.FromObject(o);
			await StorageRouter.RestoreProjects(jObject);
			switch (previousState)
			{
			case ViewState.RecycleProject:
				_sidebar.SelectModule(TAB_PROJECT);
				break;
			case ViewState.RecycleTemplate:
				_sidebar.SelectModule(TAB_TEMPLATE);
				break;
			}
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		await Populate();
	}

	private async Task DeleteProjectFromServer()
	{
		// 服务器不可用时阻止删除操作
		if (!Program.MainForm.EnsureServerAvailable()) return;
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "此操作将彻底删除所选数据，无法恢复，请谨慎确认此次操作！", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		List<Guid> o = SelectedProjects.Select((Auditai.DTO.Project p) => p.Id).ToList();
		try
		{
			JObject jobj = new JObject();
			jobj["Ids"] = JToken.FromObject(o);
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			ProgressForm2 progressForm = new ProgressForm2();
			progressForm.ShowDialogOnUiThread(progressRuntimeData, async delegate
			{
				IProgress<ProgressInfo> iProg = new ProgressRuntimeDataReporter(progressRuntimeData);
				iProg.Report(new ProgressInfo
				{
					MainCaption = "正在删除数据，请稍候...",
					MainProgress = 100
				});
				await StorageRouter.DeleteProjectFromServer(jobj);
			});
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		await Populate();
	}

	private async Task DeleteAllProjectFromServer()
	{
		// 服务器不可用时阻止删除操作
		if (!Program.MainForm.EnsureServerAvailable()) return;
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "此操作将彻底删除回收站数据，无法恢复，请谨慎确认此次操作！", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		List<Guid> o = _projects.Select((Auditai.DTO.Project p) => p.Id).ToList();
		try
		{
			JObject jobj = new JObject();
			jobj["Ids"] = JToken.FromObject(o);
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			ProgressForm2 progressForm = new ProgressForm2();
			progressForm.ShowDialogOnUiThread(progressRuntimeData, async delegate
			{
				IProgress<ProgressInfo> iProg = new ProgressRuntimeDataReporter(progressRuntimeData);
				iProg.Report(new ProgressInfo
				{
					MainCaption = "正在删除数据，请稍候...",
					MainProgress = 100
				});
				await StorageRouter.DeleteProjectFromServer(jobj);
			});
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		await Populate();
	}

	private async void _form_Shown(object sender, EventArgs e)
	{
		try
		{
			if (Program.MainForm.CurrentEdition is AppEditionGeneral)
			{
				_sidebar.SetModuleNavVisible(visible: false);
			}
			PopulateViewMode();
			PopulateSearch();
			Theme.SetCurrentTree(_form);
			SetTheme();
			State = ViewState.Project;
			_sidebar.SetSelectedModule(TAB_PROJECT);
			_topBar.SetModule(TAB_PROJECT);
			await Populate();
			if (!(Program.MainForm.CurrentEdition is AppEditionGeneral) && _projects.Count == 0)
			{
				_sidebar.SelectModule(TAB_TEMPLATE);
			}
		}
		catch (Exception ex)
		{
			ex.Log("FormProjectManage._form_Shown 异常");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "加载项目管理界面异常：" + ex.Message + "\n" + ex.StackTrace);
			_form.Close();
		}
	}

	private void _form_Resize(object sender, EventArgs e)
	{
		Style.WindowState = _form.WindowState;
		Style.Height = _form.Height;
		Style.Width = _form.Width;
	}

	private async void _form_FormClosed(object sender, FormClosedEventArgs e)
	{
		// 注意：审批只读会话（ReviewSession）不在窗体关闭时结束——
		// 只读打开成功会设置 DialogResult.OK 关闭本窗体，会话需延续到项目在主界面关闭/切离时
		//（见 MainForm.CloseProject / OpenOrSwitchToProject）；打开失败的路径由 OpenReviewProjectReadonly 自行清理。
		// 稽核页的项目会话引用（可能指向复用的编辑器会话）在窗体关闭后不再需要，及时释放
		_validationStore?.Release();
		Style.Save(ConfigManager.PROJECTMANAGEMENT_VIEWCONFIG);
		if (e.CloseReason == CloseReason.UserClosing && _isClosing)
		{
			_form.DialogResult = DialogResult.Cancel;
			if (Application.OpenForms.Count == 0)
			{
				await Program.Logout();
			}
		}
	}

	private async void Sidebar_ModuleSelected(string moduleName)
	{
		switch (moduleName)
		{
		case "TAB_PROJECT":
			State = ViewState.Project;
			break;
		case "TAB_TEMPLATE":
			State = ViewState.Template;
			break;
		case "TAB_RECYCLEPROJECT":
			State = ViewState.RecycleProject;
			break;
		case "TAB_RECYCLETEMPLATE":
			State = ViewState.RecycleTemplate;
			break;
		case "TAB_REVIEW":
			State = ViewState.Review;
			break;
		case "TAB_ARCHIVE":
			State = ViewState.Archive;
			break;
		case "TAB_VALIDATION":
			State = ViewState.Validation;
			break;
		}
		_kpiFilterTag = null;
		_topBar.SetModule(moduleName);
		_sidebar.Enabled = false;
		_topBar.Enabled = false;
		try
		{
			await Populate();
		}
		finally
		{
			_sidebar.Enabled = true;
			_topBar.Enabled = true;
		}
	}

	private async void _cmdh_CommandClick(object sender, CommandClickEventArgs e)
	{
		string name = e.Command.Name;
		if (name == null)
		{
			return;
		}
		switch (name)
		{
		case "RB_SUBMITREVIEW":
			await SubmitForReview();
			return;
		case "RB_REVIEWTEMPLATES":
			ShowReviewFlowTemplates();
			return;
		case "RB_APPROVEREVIEW":
			await ApproveOrRejectReview(isReject: false);
			return;
		case "RB_REJECTREVIEW":
			await ApproveOrRejectReview(isReject: true);
			return;
		case "RB_WITHDRAWREVIEW":
			await WithdrawSubmission();
			return;
		case "RB_REVIEWHISTORY":
			await ShowReviewHistory();
			return;
		case "RB_OPENREADONLY":
			await OpenReviewProjectReadonly();
			return;
		case "RB_REFRESHREVIEW":
		case "RB_REFRESHARCHIVE":
			await Populate();
			return;
		case "RB_REVIEWFILTERPENDING":
			_reviewScope = "pending";
			await Populate();
			return;
		case "RB_REVIEWFILTERMINE":
			_reviewScope = "mine";
			await Populate();
			return;
		case "RB_REVIEWFILTERALL":
			_reviewScope = "all";
			await Populate();
			return;
		case "RB_ARCHIVENOW":
			await ArchiveSelectedProject();
			return;
		case "RB_UNARCHIVE":
			await CancelArchiveSelected();
			return;
		case "RB_EXPORTARCHIVE":
			await ExportArchivePackage();
			return;
		case "RB_ARCHIVEFILTERARCHIVED":
			_archiveShowCandidates = false;
			await Populate();
			return;
		case "RB_ARCHIVEFILTERCANDIDATES":
			_archiveShowCandidates = true;
			await Populate();
			return;
		case "RB_RUNVALIDATION":
			await RunProjectValidation();
			return;
		case "RB_EXPORTREPORT":
			ExportValidationReport();
			return;
		case "RB_REFRESHVALIDATION":
			await Populate();
			return;
		case "RB_ADDRULE":
			await AddValidationRule();
			return;
		case "RB_EDITRULE":
			EditSelectedValidationRule();
			return;
		case "RB_DELETERULE":
			await DeleteValidationRules();
			return;
		case "RB_IMPORTRULES":
			await ImportValidationRules();
			return;
		case "RB_EXPORTRULES":
			ExportValidationRules();
			return;
		}
		switch (name.Length)
		{
		case 18:
			switch (name[4])
			{
			case 'O':
				if (name == "CMD_OPENFROMSERVER")
				{
					await OpenProjectFromServer();
				}
				break;
			case 'S':
				if (name == "CMD_SORTCREATETIME")
				{
					Style.SortKind = SortKind.CreateTime;
					await Populate();
				}
				break;
			}
			break;
		case 22:
			switch (name[8])
			{
			case 'R':
				if (name == "CTX_CMD_RENAME_PROJECT")
				{
					if (State == ViewState.Project)
					{
						await ModifyProject();
					}
					else if (State == ViewState.Template)
					{
						await ModifyTemplate();
					}
				}
				break;
			case 'D':
				if (name == "CTX_CMD_DELETE_PROJECT")
				{
					await DeleteProject();
				}
				break;
			case 'E':
				if (name == "CTX_CMD_EXPORT_PROJECT")
				{
					await ExportProject(GetCommand("CTX_CMD_EXPORT_PROJECT"));
				}
				break;
			}
			break;
		case 16:
			if (name == "CMD_SORTOPENTIME")
			{
				Style.SortKind = SortKind.OpenTime;
				await Populate();
			}
			else if (name == "CMD_GROUPCREATOR")
			{
				Style.CardGroup = CardGroupMode.Creator;
				PopulateModel();
			}
			break;
		case 14:
			if (name == "CMD_SORTNUMBER")
			{
				Style.SortKind = SortKind.Number;
				await Populate();
			}
			break;
		case 12:
			if (name == "CMD_SORTNAME")
			{
				Style.SortKind = SortKind.Name;
				await Populate();
			}
			break;
		case 20:
			if (name == "CTX_CMD_OPEN_PROJECT")
			{
				await OpenProject();
			}
			else if (name == "CMD_GROUPCREATEMONTH")
			{
				Style.CardGroup = CardGroupMode.CreateMonth;
				PopulateModel();
			}
			break;
		case 25:
			if (name == "CTX_CMD_DUPLICATE_PROJECT")
			{
				if (State == ViewState.Project)
				{
					await DuplicateProject();
				}
				else if (State == ViewState.Template)
				{
					await DuplicateTemplate();
				}
			}
			break;
		case 15:
			if (name == "CMD_GROUPRECENT")
			{
				Style.CardGroup = CardGroupMode.Recent;
				PopulateModel();
			}
			break;
		case 17:
			if (name == "CMD_GROUPCATEGORY")
			{
				Style.CardGroup = CardGroupMode.Category;
				PopulateModel();
			}
			break;
		case 13:
			if (name == "CMD_GROUPNONE")
			{
				Style.CardGroup = CardGroupMode.None;
				PopulateModel();
			}
			break;
		}
	}

	private void _ctxShowMore_Popup(object sender, EventArgs e)
	{
		if (State == ViewState.Project)
		{
			GetCommand("CTX_CMD_OPEN_PROJECT").Text = "打开" + StringConstBase.Current.Project;
			GetCommand("CTX_CMD_RENAME_PROJECT").Text = ((Program.MainForm.CurrentEdition is AppEditionGeneral) ? ("重命名" + StringConstBase.Current.Project) : ("修改" + StringConstBase.Current.Project));
			GetCommand("CTX_CMD_DELETE_PROJECT").Text = "删除" + StringConstBase.Current.Project;
			GetCommand("CTX_CMD_DUPLICATE_PROJECT").Text = "复制" + StringConstBase.Current.Project;
			GetCommand("CTX_CMD_EXPORT_PROJECT").Text = "导出" + StringConstBase.Current.Project;
			GetCommand("CTX_CMD_OPEN_PROJECT").Enabled = HasSelectedProject;
			GetCommand("CTX_CMD_RENAME_PROJECT").Enabled = CanModifyProject();
			GetCommand("CTX_CMD_DELETE_PROJECT").Enabled = CanDuplicateProject();
			GetCommand("CTX_CMD_DUPLICATE_PROJECT").Enabled = CanDuplicateProject();
			GetCommand("CTX_CMD_DUPLICATE_PROJECT").Visible = !SoftwareLicenseManager.IsDuplicateProjectOutOfLicenseLimit();
		}
		else if (State == ViewState.Template)
		{
			GetCommand("CTX_CMD_OPEN_PROJECT").Text = "打开" + StringConstBase.Current.Template;
			GetCommand("CTX_CMD_RENAME_PROJECT").Text = ((Program.MainForm.CurrentEdition is AppEditionGeneral) ? ("重命名" + StringConstBase.Current.Template) : ("修改" + StringConstBase.Current.Template));
			GetCommand("CTX_CMD_DELETE_PROJECT").Text = "删除" + StringConstBase.Current.Template;
			GetCommand("CTX_CMD_DUPLICATE_PROJECT").Text = "复制" + StringConstBase.Current.Template;
			GetCommand("CTX_CMD_EXPORT_PROJECT").Text = "导出" + StringConstBase.Current.Template;
			GetCommand("CTX_CMD_OPEN_PROJECT").Enabled = CanOpenTemplate();
			GetCommand("CTX_CMD_RENAME_PROJECT").Enabled = CanOpenTemplate();
			GetCommand("CTX_CMD_DELETE_PROJECT").Enabled = CanOpenTemplate();
			GetCommand("CTX_CMD_DUPLICATE_PROJECT").Enabled = HasSelectedProject;
			GetCommand("CTX_CMD_DUPLICATE_PROJECT").Visible = !SoftwareLicenseManager.IsDuplicateProjectOutOfLicenseLimit();
		}
	}

	private void _ctx_Popup(object sender, EventArgs e)
	{
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			GetCommand("CMD_CATEGORY").Visible = false;
			GetCommand("CMD_SORT").Visible = false;
		}
		else if (State == ViewState.Project)
		{
			GetCommand("CMD_OPENFROMSERVER").Enabled = HasSelectedProject;
			GetCommand("CMD_OPENFROMSERVER").Text = "全新打开" + StringConstBase.Current.Project;
			GetCommand("CMD_CATEGORY").Enabled = CanModifyProject();
			GetCommand("CMD_CATEGORY").Text = StringConstBase.Current.Project + "类别";
			GetCommand("CMD_SORTNUMBER").Text = StringConstBase.Current.Project + "编号";
			GetCommand("CMD_SORTNAME").Text = StringConstBase.Current.Project + "名称";
		}
		else if (State == ViewState.Template)
		{
			GetCommand("CMD_OPENFROMSERVER").Enabled = CanOpenTemplate();
			GetCommand("CMD_OPENFROMSERVER").Text = "全新打开" + StringConstBase.Current.Template;
			GetCommand("CMD_CATEGORY").Enabled = CanOpenTemplate();
			GetCommand("CMD_CATEGORY").Text = StringConstBase.Current.Template + "类别";
			GetCommand("CMD_SORTNUMBER").Text = StringConstBase.Current.Template + "编号";
			GetCommand("CMD_SORTNAME").Text = StringConstBase.Current.Template + "名称";
		}
	}

	private async void CommandBar_Click(object sender, EventArgs e)
	{
		if (_noAllowReentry)
		{
			return;
		}
		_noAllowReentry = true;
		try
		{
			SimpleCommand ribbonButton = sender as SimpleCommand;
			string name = ribbonButton.Name;
			if (name == null)
			{
				return;
			}
			switch (name.Length)
			{
			case 16:
				switch (name[3])
				{
				case 'C':
					if (name == "RB_CREATEPROJECT")
					{
						await CreateProject(null);
					}
					break;
				case 'M':
					if (name == "RB_MODIFYPROJECT")
					{
						await ModifyProject();
					}
					break;
				case 'D':
					if (name == "RB_DELETEPROJECT")
					{
						await DeleteProject();
					}
					break;
				case 'E':
					if (name == "RB_EXPORTPROJECT")
					{
						await ExportProject(ribbonButton);
					}
					break;
				case 'R':
					if (name == "RB_RESTORESELECT")
					{
						await RestoreProjects();
					}
					break;
				case 'S':
					if (name == "RB_SHARETEMPLATE")
					{
						await ShareProject();
					}
					break;
				case 'I':
					if (name == "RB_IMPORTPROJECT")
					{
						await ImportProject();
					}
					break;
				}
				break;
			case 14:
				switch (name[3])
				{
				case 'O':
					if (name == "RB_OPENPROJECT")
					{
						await OpenProject();
					}
					break;
				case 'U':
					if (name == "RB_USETEMPLATE")
					{
						await UseTemplate();
					}
					break;
				}
				break;
			case 17:
				switch (name[5])
				{
				case 'V':
					if (name == "RB_SAVEASTEMPLATE")
					{
						await SaveAsTemplate();
					}
					break;
				case 'F':
					if (name == "RB_REFRESHPROJECT")
					{
						await Populate();
					}
					break;
				case 'A':
					if (name == "RB_CHANGEPASSWORD")
					{
						ChangePassword();
					}
					break;
				case 'E':
					if (name == "RB_CREATETEMPLATE")
					{
						await CreateTemplate();
					}
					break;
				case 'D':
					if (name == "RB_MODIFYTEMPLATE")
					{
						await ModifyTemplate();
					}
					break;
				case 'L':
					if (name == "RB_DELETETEMPLATE")
					{
						await DeleteProject();
					}
					break;
				case 'P':
					if (name == "RB_EXPORTTEMPLATE")
					{
						await ExportTemplateFile();
					}
					else if (name == "RB_IMPORTTEMPLATE")
					{
						await ImportTemplate();
					}
					break;
				}
				break;
			case 11:
				switch (name[3])
				{
				case 'V':
					if (name == "RB_VIEWMODE")
					{
						ToggleViewMode();
					}
					break;
				case 'U':
					if (name == "RB_USERINFO")
					{
						UserInfo();
					}
					break;
				}
				break;
			case 13:
				switch (name[3])
				{
				case 'U':
					if (name == "RB_USERMANAGE")
					{
						await ManageUsers();
					}
					break;
				}
				break;
			case 15:
				switch (name[3])
				{
				case 'O':
					if (name == "RB_OPENTEMPLATE")
					{
						await OpenProject();
					}
					break;
				case 'D':
					if (name == "RB_DELETESELECT")
					{
						await DeleteProjectFromServer();
					}
					break;
				case 'E':
					if (name == "RB_EMPTYRECYCLE")
					{
						await DeleteAllProjectFromServer();
					}
					break;
				case 'S':
				if (name == "RB_SHAREPROJECT")
				{
					await ShareProject();
				}
				break;
			}
			break;
		case 19:
				if (name == "RB_DUPLICATEPROJECT")
				{
					await DuplicateProject();
				}
				break;
			case 20:
			if (name == "RB_DUPLICATETEMPLATE")
			{
				await DuplicateTemplate();
			}
			else if (name == "RB_EXPORTPROJECTFILE")
			{
				await ExportProjectFile();
			}
			break;
			case 18:
				if (name == "RB_REFRESHTEMPLATE")
				{
					await Populate();
				}
				break;
			case 9:
				if (name == "RB_SEARCH")
				{
					ToggleSearch();
				}
				break;
			case 10:
			case 12:
				break;
			}
		}
		finally
		{
			_noAllowReentry = false;
		}
	}

	private void _btnCloseSearch_Click(object sender, EventArgs e)
	{
		_isSearch = false;
		PopulateSearch();
		PopulateModel();
	}

	private void Menu_Popup(object sender, EventArgs e)
	{
		_anyMenuItemClicked = false;
		C1CommandMenu c1CommandMenu = (C1CommandMenu)GetCommand("CMD_CATEGORY");
		c1CommandMenu.CommandLinks.Clear();
		foreach (string cat in KnownCategories)
		{
			C1Command c1Command = new C1Command();
			c1Command.Text = cat;
			c1Command.CheckAutoToggle = true;
			c1Command.Checked = SelectedProject.Category.Split('|').Contains(cat);
			C1Command c1Command2 = c1Command;
			c1Command2.Click += delegate
			{
				_anyMenuItemClicked = true;
			};
			C1CommandLink value = new C1CommandLink(c1Command2);
			c1CommandMenu.CommandLinks.Add(value);
		}
	}

	private async void _ctx_Closed(object sender, EventArgs e)
	{
		C1CommandMenu menu = (C1CommandMenu)GetCommand("CMD_CATEGORY");
		if (_anyMenuItemClicked)
		{
			List<C1Command> list = new List<C1Command>();
			foreach (C1CommandLink commandLink in menu.CommandLinks)
			{
				if (commandLink.Command.Checked)
				{
					list.Add(commandLink.Command);
				}
			}
			string category = string.Join("|", list.Select((C1Command c) => c.Text));
			Auditai.DTO.Project selectedProject = SelectedProject;
			selectedProject.Category = category;
			if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode)
			{
				await WebApiClient.UpdateProject(selectedProject);
			}
			await Populate();
		}
		menu.CommandLinks.Clear();
		menu.CommandLinks.Add(_lnkNull);
		_ctxCardItem = null;
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		try
		{
			if (_grid.Cols.Contains("CN_STATUS") && e.Col == _grid.Cols["CN_STATUS"].Index && e.Row >= _grid.Rows.Fixed)
			{
				DrawStatusPill(e);
				return;
			}
			if (State == ViewState.Project || State == ViewState.RecycleProject)
			{
				if (e.Col == _grid.Cols["CN_PROJLEADER"].Index || e.Col == _grid.Cols["CN_PROJASSIST"].Index || e.Col == _grid.Cols["CN_PROJCHECK"].Index)
				{
					IEnumerable<Auditai.DTO.User> source = (IEnumerable<Auditai.DTO.User>)_grid.BodyGetData(e.Row, e.Col);
					e.Text = (source.Any() ? string.Join(",", source.Select((Auditai.DTO.User u) => u.Name)) : "(空)");
				}
			}
			else
			{
				if (State != ViewState.Template && State != ViewState.RecycleTemplate)
				{
					return;
				}
				if (e.Col == _grid.Cols["CN_TMPLEDITOR"].Index)
				{
					IEnumerable<Auditai.DTO.User> enumerable = (IEnumerable<Auditai.DTO.User>)_grid.BodyGetData(e.Row, e.Col);
					if (enumerable == null)
					{
						e.Text = string.Empty;
						return;
					}
					e.Text = (enumerable.Any() ? string.Join(",", enumerable.Select((Auditai.DTO.User u) => u.Name)) : "(空)");
				}
				else
				{
					if (e.Col != _grid.Cols["CN_TMPLUSER"].Index)
					{
						return;
					}
					Auditai.DTO.Project project = _grid.BodyGetRow(e.Row).UserData as Auditai.DTO.Project;
					if (project.TeamVisible)
					{
						e.Text = "(所有同事)";
						return;
					}
					IEnumerable<Auditai.DTO.User> source2 = (IEnumerable<Auditai.DTO.User>)_grid.BodyGetData(e.Row, e.Col);
					e.Text = (source2.Any() ? string.Join(",", source2.Select((Auditai.DTO.User u) => u.Name)) : "(空)");
				}
			}
		}
		catch
		{
		}
	}

	private async void _grid_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (State == ViewState.Project || State == ViewState.Template)
		{
			_grid.Enabled = false;
			HitTestInfo hitTestInfo = _grid.HitTest();
			if (_grid.Rows.Fixed <= hitTestInfo.Row && hitTestInfo.Row < _grid.Rows.Count)
			{
				await OpenProject();
			}
			_grid.Enabled = true;
		}
	}

	private void _grid_BodySelectionChanged(object sender, EventArgs e)
	{
		if (State == ViewState.Project || State == ViewState.Template)
		{
			SetCommandState();
		}
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		if ((State == ViewState.Project || State == ViewState.Template) && e.Button == MouseButtons.Right && _grid.HitTest(e.Location).Type == HitTestTypeEnum.Cell)
		{
			ShowNativeContextMenu(_grid, e.Location);
		}
	}

	private void _grid_CellChecked(object sender, RowColEventArgs e)
	{
		SetCommandState();
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		_grid.DrawFormBorder(e.Graphics);
		DrawHoverRowOverlay(e.Graphics);
	}

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
		// 行号变化时仅重绘一次，Paint 阶段叠加淡色底纹
		C1.Win.C1FlexGrid.HitTestInfo hit = _grid.HitTest(e.X, e.Y);
		int row = hit.Row;
		if (row < _grid.Rows.Fixed || row >= _grid.Rows.Count)
		{
			row = -1;
		}
		if (row == _hoverRow)
		{
			return;
		}
		_hoverRow = row;
		_grid.Invalidate();
	}

	private void _grid_MouseLeave(object sender, EventArgs e)
	{
		if (_hoverRow == -1)
		{
			return;
		}
		_hoverRow = -1;
		_grid.Invalidate();
	}

	/// <summary>行悬停高亮：Paint 事件在单元格绘制之后触发，对悬停行叠加 DarkColor alpha 8（含自绘药丸单元格，保持观感一致）。</summary>
	private void DrawHoverRowOverlay(Graphics g)
	{
		if (_hoverRow < _grid.Rows.Fixed || _hoverRow >= _grid.Rows.Count)
		{
			return;
		}
		try
		{
			Rectangle first = _grid.GetCellRect(_hoverRow, 0);
			Rectangle last = _grid.GetCellRect(_hoverRow, _grid.Cols.Count - 1);
			Rectangle area = Rectangle.FromLTRB(first.Left, first.Top, Math.Max(first.Right, last.Right), first.Bottom);
			using SolidBrush brush = new SolidBrush(Color.FromArgb(8, Theme.SelectedAuditaiTheme.ThemeContext.DarkColor));
			g.FillRectangle(brush, area);
		}
		catch
		{
		}
	}

	/// <summary>状态列圆角药丸：审批中=Brand、已通过=SuccessText、已退回=ErrorText、已归档=Slate（tint 底 alpha 20 + 同色文字）；未上报走默认文本绘制。</summary>
	private void DrawStatusPill(OwnerDrawCellEventArgs e)
	{
		Auditai.DTO.Project project = _grid.Rows[e.Row].UserData as Auditai.DTO.Project;
		if (project == null)
		{
			return;
		}
		string text = GetProjectStatusText(project);
		Color color;
		if (text == "审批中")
		{
			color = AuditTheme.Brand;
		}
		else if (text == "已通过")
		{
			color = AuditTheme.SuccessText;
		}
		else if (text == "已退回")
		{
			color = AuditTheme.ErrorText;
		}
		else if (text == "已归档")
		{
			color = AuditTheme.Slate;
		}
		else
		{
			// 未上报：普通文字色，交给网格默认绘制
			return;
		}
		e.DrawCell(DrawCellFlags.Background);
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		int textWidth = TextRenderer.MeasureText(text, StatusPillFont).Width;
		Rectangle bounds = e.Bounds;
		Rectangle pill = new Rectangle(bounds.X + 4, bounds.Y + (bounds.Height - 20) / 2, Math.Max(12, Math.Min(textWidth + 16, bounds.Width - 8)), 20);
		using (GraphicsPath path = CreateRoundedPath(pill, 10))
		using (SolidBrush pillBack = new SolidBrush(Color.FromArgb(20, color)))
		{
			e.Graphics.FillPath(pillBack, path);
		}
		TextRenderer.DrawText(e.Graphics, text, StatusPillFont, pill, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
		e.DrawCell(DrawCellFlags.Border);
		e.Handled = true;
	}

	/// <summary>圆角矩形路径（与 ProjectCardView 同手法）。</summary>
	private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
	{
		GraphicsPath path = new GraphicsPath();
		int d = radius * 2;
		path.AddArc(rect.X, rect.Y, d, d, 180f, 90f);
		path.AddArc(rect.Right - d, rect.Y, d, d, 270f, 90f);
		path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0f, 90f);
		path.AddArc(rect.X, rect.Bottom - d, d, d, 90f, 90f);
		path.CloseFigure();
		return path;
	}

	private async void CardFlow_ItemActivated(ProjectCardItem item)
	{
		_noAllowReentry = true;
		try
		{
			if (item != null && item.IsCreateTile)
			{
				// "新建项目"卡片：走原磁贴双击的创建流程
				await CreateProject(null);
			}
			else if (item != null && (State == ViewState.Project || State == ViewState.Template))
			{
				// 原磁贴双击的打开流程（SelectedProject 已由卡片单击选中建立）
				await OpenProject();
			}
		}
		finally
		{
			_noAllowReentry = false;
		}
	}

	private async void FavBar_ItemActivated(ProjectCardItem item)
	{
		if (item?.Project == null)
		{
			return;
		}
		_noAllowReentry = true;
		try
		{
			if (State == ViewState.Project || State == ViewState.Template)
			{
				await OpenProject();
			}
		}
		finally
		{
			_noAllowReentry = false;
		}
	}

	private void Card_ItemRightClicked(ProjectCardItem item, Point screenPoint)
	{
		if (item == null || item.IsCreateTile || item.Project == null)
		{
			return;
		}
		if (State == ViewState.RecycleProject || State == ViewState.RecycleTemplate)
		{
			// 多选视图（回收站）：右键未选中的卡片时将其设为唯一选中；照原磁贴行为不弹右键菜单
			if (!_cardFlow.SelectedProjects.Contains(item.Project))
			{
				_cardFlow.ClearSelection();
				_ctxCardItem = item;
			}
			SetCommandState();
			return;
		}
		ShowCardContextMenu(item, screenPoint, _cardFlow);
	}

	private void FavBar_ItemRightClicked(ProjectCardItem item, Point screenPoint)
	{
		ShowCardContextMenu(item, screenPoint, _favBar);
	}

	private void Card_ItemMoreClicked(ProjectCardItem item, Point screenPoint)
	{
		ShowCardContextMenu(item, screenPoint, _cardFlow);
	}

	private void FavBar_ItemMoreClicked(ProjectCardItem item, Point screenPoint)
	{
		ShowCardContextMenu(item, screenPoint, _favBar);
	}

	/// <summary>卡片右键/更多菜单（仅项目/模板视图）：先让该卡片成为当前选中（容器无可编程选中 API，经 _ctxCardItem 回退生效），再按屏幕坐标弹出菜单。</summary>
	private void ShowCardContextMenu(ProjectCardItem item, Point screenPoint, Control host)
	{
		if (item == null || item.IsCreateTile || item.Project == null)
		{
			return;
		}
		if (State != ViewState.Project && State != ViewState.Template)
		{
			return;
		}
		if (_cardFlow.SelectedProject != null)
		{
			_cardFlow.ClearSelection();
		}
		if (_favBar.SelectedProject != null)
		{
			_favBar.ClearSelection();
		}
		_ctxCardItem = item;
		GetCommand("CMD_OPENFROMSERVER").Visible = true;
		GetCommand("CMD_CATEGORY").Visible = true;
		ShowNativeContextMenu(host, host.PointToClient(screenPoint));
	}

	/// <summary>以原生 ContextMenuStrip 展示 _ctx 命令集：文字走系统 GDI/ClearType 渲染，PerMonitorV2 高 DPI 下清晰（C1 菜单会被位图拉伸发虚）。</summary>
	private void ShowNativeContextMenu(Control host, Point clientPoint)
	{
		// 复用 C1 菜单的弹出前状态刷新逻辑（两个 Popup 处理器原本都挂在 _ctx.Popup 上）
		_ctxShowMore_Popup(null, EventArgs.Empty);
		_ctx_Popup(null, EventArgs.Empty);
		_nativeCtxMenu?.Dispose();
		_nativeCategoryChanged = false;
		_nativeCategoryItem = null;
		_nativeCtxMenu = new ContextMenuStrip
		{
			Font = new Font("微软雅黑", 9.5f, FontStyle.Regular, GraphicsUnit.Point, 134),
			ShowImageMargin = true
		};
		AppendNativeLinks(_ctx.CommandLinks, _nativeCtxMenu.Items);
		_nativeCtxMenu.Closed += NativeCtxMenu_Closed;
		_nativeCtxMenu.Show(host, clientPoint);
	}

	/// <summary>把 C1 命令链接集合转换为原生菜单项（文本/启用/可见/勾选/图片/子菜单一一镜像；点击转 PerformClick 走原命令分发）。</summary>
	private void AppendNativeLinks(System.Collections.IEnumerable links, ToolStripItemCollection target)
	{
		foreach (object obj in links)
		{
			if (!(obj is C1CommandLink link) || link.Command == null || !link.Command.Visible)
			{
				continue;
			}
			C1Command cmd = link.Command;
			if (link.Delimiter)
			{
				target.Add(new ToolStripSeparator());
			}
			if (cmd is C1CommandMenu menu)
			{
				ToolStripMenuItem parent = new ToolStripMenuItem(cmd.Text)
				{
					Enabled = cmd.Enabled,
					Image = cmd.Image
				};
				if (menu.Name == "CMD_CATEGORY")
				{
					BuildNativeCategoryItems(parent);
				}
				else
				{
					AppendNativeLinks(menu.CommandLinks, parent.DropDownItems);
				}
				target.Add(parent);
			}
			else
			{
				ToolStripMenuItem item = new ToolStripMenuItem(cmd.Text)
				{
					Enabled = cmd.Enabled,
					Image = cmd.Image,
					Checked = cmd.Checked,
					CheckOnClick = cmd.CheckAutoToggle
				};
				C1Command captured = cmd;
				item.Click += delegate
				{
					captured.PerformClick();
				};
				target.Add(item);
			}
		}
	}

	/// <summary>构建"类别"子菜单：按当前项目勾选状态生成复选项；点击不收起子菜单（与原 C1 CloseOnItemClick=false 一致），菜单最终关闭时统一保存。</summary>
	private void BuildNativeCategoryItems(ToolStripMenuItem parent)
	{
		_nativeCategoryItem = parent;
		Auditai.DTO.Project selected = SelectedProject;
		foreach (string cat in KnownCategories)
		{
			ToolStripMenuItem item = new ToolStripMenuItem(cat)
			{
				CheckOnClick = true,
				Checked = selected != null && (selected.Category ?? "").Split('|').Contains(cat)
			};
			item.Click += delegate
			{
				_nativeCategoryChanged = true;
			};
			parent.DropDownItems.Add(item);
		}
		// 点击类别项时保持子菜单展开（支持多选），点击外部/ESC 才收起
		parent.DropDown.Closing += delegate(object s, ToolStripDropDownClosingEventArgs e)
		{
			if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
			{
				e.Cancel = true;
			}
		};
	}

	private void NativeCtxMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
	{
		try
		{
			if (_nativeCategoryChanged && _nativeCategoryItem != null)
			{
				SaveNativeCategorySelection();
			}
		}
		finally
		{
			_ctxCardItem = null;
		}
	}

	/// <summary>按"类别"子菜单勾选项保存当前项目的类别（对应原 _ctx_Closed 的保存逻辑）。</summary>
	private async void SaveNativeCategorySelection()
	{
		try
		{
			List<string> selectedCats = new List<string>();
			foreach (ToolStripItem it in _nativeCategoryItem.DropDownItems)
			{
				if (it is ToolStripMenuItem mi && mi.Checked)
				{
					selectedCats.Add(mi.Text);
				}
			}
			Auditai.DTO.Project selectedProject = SelectedProject;
			if (selectedProject == null)
			{
				return;
			}
			selectedProject.Category = string.Join("|", selectedCats);
			if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode)
			{
				await WebApiClient.UpdateProject(selectedProject);
			}
			await Populate();
		}
		catch (Exception)
		{
		}
	}

	private void CardFlow_SelectionChanged()
	{
		_ctxCardItem = null;
		if (!_syncingCardSelection)
		{
			_syncingCardSelection = true;
			try
			{
				if (_favBar.SelectedProject != null)
				{
					_favBar.ClearSelection();
				}
			}
			finally
			{
				_syncingCardSelection = false;
			}
		}
		SetCommandState();
	}

	private void FavBar_SelectionChanged()
	{
		if (!_syncingCardSelection)
		{
			_syncingCardSelection = true;
			try
			{
				_cardFlow.ClearSelection();
			}
			finally
			{
				_syncingCardSelection = false;
			}
		}
		SetCommandState();
	}

	private void KpiStrip_ItemClicked(KpiStrip.KpiItem item)
	{
		if (item == null)
		{
			return;
		}
		if (item.IsCategory)
		{
			// 分类筛选项（复选）：更新选中状态后重刷列表与 KPI 条（计数/勾选保持同步）
			if (item.Tag == CategoryTagEmpty)
			{
				_categoryEmptyChecked = item.Checked;
			}
			else if (item.Tag != null && item.Tag.StartsWith(CategoryTagPrefix, StringComparison.Ordinal))
			{
				string cat = item.Tag.Substring(CategoryTagPrefix.Length);
				if (item.Checked)
				{
					_selectedCategories.Add(cat);
				}
				else
				{
					_selectedCategories.Remove(cat);
				}
			}
			PopulateModel();
			PopulateKpi();
			return;
		}
		_kpiFilterTag = ((string.Equals(_kpiFilterTag, item.Tag, StringComparison.Ordinal)) ? null : item.Tag);
		PopulateModel();
		_kpiStrip.SelectedTag = _kpiFilterTag;
	}

	private void ToggleFavorite(ProjectCardItem item)
	{
		if (item?.Project == null)
		{
			return;
		}
		ProjectFavoritesStore.Toggle(item.Project.Id);
		// 重建卡片流（星标即时反映）并同步收藏行；KPI 同步刷新
		PopulateModel();
		PopulateKpi();
	}

	private void _txbSearch_TextChanged(object sender, EventArgs e)
	{
		PopulateModel();
	}
}

