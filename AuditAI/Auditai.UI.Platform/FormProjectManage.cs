﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using C1.Win.C1Tile;
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

public class FormProjectManage : ISetTheme
{
	protected class TileShowMoreMenuIconRender : C1TileCustomButtonRender
	{
		protected FormProjectManage _owner;

		protected C1TileControlEx _tileControl;

		protected Tile _mouseOverTile;

		protected bool _isMouseOverShowMoreMenuIcon;

		protected SolidBrush _shadowBrush = new SolidBrush(Color.Transparent);

		protected bool _isMouseOverCreateProjectTile;

		public TileShowMoreMenuIconRender(FormProjectManage owner, C1TileControlEx tileControl)
		{
			_owner = owner;
			_tileControl = tileControl;
			tileControl.MouseMove += TileControl_MouseMove;
			tileControl.MouseEnter += TileControl_MouseEnter;
			tileControl.MouseLeave += TileControl_MouseLeave;
		}

		private bool IsAllowShowIcon()
		{
			if (_owner.State == ViewState.Project || _owner.State == ViewState.Template)
			{
				return true;
			}
			return false;
		}

		private void TileControl_MouseEnter(object sender, EventArgs e)
		{
			_isMouseOverShowMoreMenuIcon = false;
			_mouseOverTile = null;
		}

		private void TileControl_MouseLeave(object sender, EventArgs e)
		{
			_isMouseOverShowMoreMenuIcon = false;
			if (_mouseOverTile != null)
			{
				_mouseOverTile = null;
				_tileControl.Invalidate();
			}
		}

		private void TileControl_MouseMove(object sender, MouseEventArgs e)
		{
			if (IsAllowShowIcon())
			{
				bool flag = false;
				Tile tileAt = _tileControl.GetTileAt(e.Location);
				if (tileAt != _mouseOverTile)
				{
					_mouseOverTile = tileAt;
					flag = true;
				}
				if (tileAt != null && tileAt.Tag == _owner.TAG_CREATEPROJECTTILE)
				{
					_isMouseOverCreateProjectTile = true;
				}
				else
				{
					_isMouseOverCreateProjectTile = false;
				}
				bool flag2 = false;
				if (_mouseOverTile != null)
				{
					flag2 = GetShowMoreMenuImageShadowRectangle(_mouseOverTile).Contains(e.Location);
				}
				if (_isMouseOverShowMoreMenuIcon != flag2)
				{
					_isMouseOverShowMoreMenuIcon = flag2;
					flag = true;
				}
				if (flag)
				{
					_tileControl.Invalidate();
				}
			}
		}

		public bool OnTileSingleClicked(TileMouseEventArgs e)
		{
			if (!IsAllowShowIcon())
			{
				return false;
			}
			if (e.MouseEA.Button == MouseButtons.Left && _isMouseOverShowMoreMenuIcon && _mouseOverTile != null && !_isMouseOverCreateProjectTile)
			{
				_owner._ctxShowMore.ShowContextMenu(_tileControl, e.MouseEA.Location);
			}
			return false;
		}

		public void OnPaint(PaintEventArgs e)
		{
			if (IsAllowShowIcon() && _mouseOverTile != null && !_isMouseOverCreateProjectTile)
			{
				if (_isMouseOverShowMoreMenuIcon)
				{
					Rectangle showMoreMenuImageShadowRectangle = GetShowMoreMenuImageShadowRectangle(_mouseOverTile);
					_shadowBrush.Color = _tileControl.HotBorderColor;
					e.Graphics.FillRectangle(_shadowBrush, showMoreMenuImageShadowRectangle);
				}
				Rectangle showMoreMenuImageRectangle = GetShowMoreMenuImageRectangle(_mouseOverTile);
				e.Graphics.DrawImage(Resources.menuMoreOperation, showMoreMenuImageRectangle.Location);
			}
		}

		private Rectangle GetShowMoreMenuImageRectangle(Tile tile)
		{
			Rectangle tileRectangle = _tileControl.GetTileRectangle(tile);
			int x = tileRectangle.Right - Resources.menuMoreOperation.Width - 4;
			int y = tileRectangle.Top + 4;
			return new Rectangle(new Point(x, y), Resources.menuMoreOperation.Size);
		}

		private Rectangle GetShowMoreMenuImageShadowRectangle(Tile tile)
		{
			int num = 2;
			int num2 = 2;
			Rectangle showMoreMenuImageRectangle = GetShowMoreMenuImageRectangle(tile);
			return new Rectangle(showMoreMenuImageRectangle.X - num, showMoreMenuImageRectangle.Y - num2, showMoreMenuImageRectangle.Width + num * 2, showMoreMenuImageRectangle.Height + num2 * 2);
		}
	}

	protected class TilePayStatusIconRender : C1TileCustomButtonRender
	{
		protected FormProjectManage _owner;

		protected C1TileControlEx _tileControl;

		protected Tile _mouseOverTile;

		protected bool _isMouseOverPayStatusIcon;

		protected SolidBrush _shadowBrush = new SolidBrush(Color.Transparent);

		protected bool _isMouseOverCreateProjectTile;

		protected System.Drawing.Image _mouseOverTilePayStatusImage;

		public TilePayStatusIconRender(FormProjectManage owner, C1TileControlEx tileControl)
		{
			_owner = owner;
			_tileControl = tileControl;
			tileControl.MouseMove += TileControl_MouseMove;
			tileControl.MouseEnter += TileControl_MouseEnter;
			tileControl.MouseLeave += TileControl_MouseLeave;
		}

		private bool IsAllowShowIcon()
		{
			if (_owner.State == ViewState.Project || _owner.State == ViewState.Template)
			{
				return true;
			}
			return false;
		}

		private void TileControl_MouseEnter(object sender, EventArgs e)
		{
			_isMouseOverPayStatusIcon = false;
			_mouseOverTile = null;
		}

		private void TileControl_MouseLeave(object sender, EventArgs e)
		{
			_isMouseOverPayStatusIcon = false;
			if (_mouseOverTile != null)
			{
				_mouseOverTile = null;
				_tileControl.Invalidate();
			}
		}

		private void TileControl_MouseMove(object sender, MouseEventArgs e)
		{
			_mouseOverTilePayStatusImage = null;
			if (!IsAllowShowIcon())
			{
				return;
			}
			bool flag = false;
			Tile tileAt = _tileControl.GetTileAt(e.Location);
			if (tileAt != _mouseOverTile)
			{
				_mouseOverTile = tileAt;
				flag = true;
			}
			if (tileAt != null && tileAt.Tag == _owner.TAG_CREATEPROJECTTILE)
			{
				_isMouseOverCreateProjectTile = true;
			}
			else
			{
				_isMouseOverCreateProjectTile = false;
			}
			bool flag2 = false;
			if (_mouseOverTile != null)
			{
				_mouseOverTilePayStatusImage = _tileControl.GetLeftTopImage(_mouseOverTile);
				if (_mouseOverTilePayStatusImage != null)
				{
					flag2 = GetPayStatusImageShadowRectangle(_mouseOverTile, _mouseOverTilePayStatusImage).Contains(e.Location);
				}
			}
			if (_isMouseOverPayStatusIcon != flag2)
			{
				_isMouseOverPayStatusIcon = flag2;
				flag = true;
			}
			if (flag)
			{
				_tileControl.Invalidate();
			}
		}

		public bool OnTileSingleClicked(TileMouseEventArgs e)
		{
			if (!IsAllowShowIcon())
			{
				return false;
			}
			if (e.MouseEA.Button == MouseButtons.Left && _isMouseOverPayStatusIcon && _mouseOverTile != null && !_isMouseOverCreateProjectTile && _mouseOverTilePayStatusImage != null && _mouseOverTile.Tag is Auditai.DTO.Project project)
			{
				if (SoftwareLicenseManager.IsFreeTeam())
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n您是免费版用户，如需升级为正式版，您可致电官方客服电话：400-690-6500，联系购买！");
					return true;
				}
				if (project.ProjectChargeType == ChargeType.Pay)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n当前" + StringConstBase.Current.Project + "为正式" + StringConstBase.Current.Project + "，如需售后支持，您可致电官方客服电话：400-690-6500，寻求支持！");
					return true;
				}
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n当前" + StringConstBase.Current.Project + "为体验" + StringConstBase.Current.Project + "，如需升级为正式" + StringConstBase.Current.Project + "，您可致电官方客服电话：400-690-6500，联系购买！");
				return true;
			}
			return false;
		}

		public void OnPaint(PaintEventArgs e)
		{
			if (IsAllowShowIcon() && _mouseOverTile != null && !_isMouseOverCreateProjectTile && _mouseOverTilePayStatusImage != null)
			{
				if (_isMouseOverPayStatusIcon)
				{
					Rectangle payStatusImageShadowRectangle = GetPayStatusImageShadowRectangle(_mouseOverTile, _mouseOverTilePayStatusImage);
					_shadowBrush.Color = _tileControl.HotBorderColor;
					e.Graphics.FillRectangle(_shadowBrush, payStatusImageShadowRectangle);
				}
				Rectangle payStatusImageRectangle = GetPayStatusImageRectangle(_mouseOverTile, _mouseOverTilePayStatusImage);
				e.Graphics.DrawImage(_mouseOverTilePayStatusImage, payStatusImageRectangle.Location);
			}
		}

		private Rectangle GetPayStatusImageRectangle(Tile tile, System.Drawing.Image payStatusImage)
		{
			Rectangle tileRectangle = _tileControl.GetTileRectangle(tile);
			int x = tileRectangle.Left + 3;
			int y = tileRectangle.Top + 3;
			return new Rectangle(new Point(x, y), payStatusImage.Size);
		}

		private Rectangle GetPayStatusImageShadowRectangle(Tile tile, System.Drawing.Image payStatusImage)
		{
			int num = 2;
			int num2 = 2;
			Rectangle payStatusImageRectangle = GetPayStatusImageRectangle(tile, payStatusImage);
			return new Rectangle(payStatusImageRectangle.X - num, payStatusImageRectangle.Y - num2, payStatusImageRectangle.Width + num * 2, payStatusImageRectangle.Height + num2 * 2);
		}
	}

	private enum ViewState
	{
		None,
		Project,
		Template,
		RecycleProject,
		RecycleTemplate
	}

	private class DisplayStyle
	{
		public int Height { get; set; } = 600;


		public int Width { get; set; } = 1000;


		public FormWindowState WindowState { get; set; }

		public ListTileViewMode ViewMode { get; set; } = ListTileViewMode.Tile;


		public SortKind SortKind { get; set; }

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

	private const string RG_HELP = "RG_HELP";

	private const string RG_TEMPLATE = "RG_TEMPLATE";

	private const string RG_RECYCLE = "RG_RECYCLE";

	private const string RG_VIEWMODE = "RG_VIEWMODE";

	private const string RB_CREATEPROJECT = "RB_CREATEPROJECT";

	private const string RB_OPENPROJECT = "RB_OPENPROJECT";

	private const string RB_MODIFYPROJECT = "RB_MODIFYPROJECT";

	private const string RB_DELETEPROJECT = "RB_DELETEPROJECT";

	private const string RB_DUPLICATEPROJECT = "RB_DUPLICATEPROJECT";

	private const string RB_EXPORTPROJECT = "RB_EXPORTPROJECT";

	private const string RB_SAVEASTEMPLATE = "RB_SAVEASTEMPLATE";

	private const string RB_SHAREPROJECT = "RB_SHAREPROJECT";

	private const string RB_VIEWMODE = "RB_VIEWMODE";

	private const string IL_TILEMODE = "IL_TILEMODE";

	private const string IL_LISTMODE = "IL_LISTMODE";

	private const string RB_REFRESHPROJECT = "RB_REFRESHPROJECT";

	private const string RB_USERMANAGE = "RB_USERMANAGE";

	private const string RB_USERINFO = "RB_USERINFO";

	private const string RB_CHANGEPASSWORD = "RB_CHANGEPASSWORD";

	private const string RB_HELPCENTER = "RB_HELPCENTER";

	private const string RB_USETEMPLATE = "RB_USETEMPLATE";

	private const string RB_CREATETEMPLATE = "RB_CREATETEMPLATE";

	private const string RB_OPENTEMPLATE = "RB_OPENTEMPLATE";

	private const string RB_MODIFYTEMPLATE = "RB_MODIFYTEMPLATE";

	private const string RB_DELETETEMPLATE = "RB_DELETETEMPLATE";

	private const string RB_DUPLICATETEMPLATE = "RB_DUPLICATETEMPLATE";

	private const string RB_REFRESHTEMPLATE = "RB_REFRESHTEMPLATE";

	private const string RB_SEARCH = "RB_SEARCH";

	private const string RB_SHARETEMPLATE = "RB_SHARETEMPLATE";

	private const string RB_EMPTYRECYCLE = "RB_EMPTYRECYCLE";

	private const string RB_DELETESELECT = "RB_DELETESELECT";

	private const string RB_RESTORESELECT = "RB_RESTORESELECT";

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

	private const string TT_PROJECT = "TT_PROJECT";

	private const string CMD_OPENFROMSERVER = "CMD_OPENFROMSERVER";

	private const string CMD_SORT = "CMD_SORT";

	private const string CMD_SORTCREATETIME = "CMD_SORTCREATETIME";

	private const string CMD_SORTOPENTIME = "CMD_SORTOPENTIME";

	private const string CMD_SORTNUMBER = "CMD_SORTNUMBER";

	private const string CMD_SORTNAME = "CMD_SORTNAME";

	private const string CMD_CATEGORY = "CMD_CATEGORY";

	private const string CTX_CMD_OPEN_PROJECT = "CTX_CMD_OPEN_PROJECT";

	private const string CTX_CMD_RENAME_PROJECT = "CTX_CMD_RENAME_PROJECT";

	private const string CTX_CMD_DELETE_PROJECT = "CTX_CMD_DELETE_PROJECT";

	private const string CTX_CMD_DUPLICATE_PROJECT = "CTX_CMD_DUPLICATE_PROJECT";

	private const string CTX_CMD_EXPORT_PROJECT = "CTX_CMD_EXPORT_PROJECT";

	private readonly object TAG_CREATEPROJECTTILE = new object();

	private readonly Dictionary<string, Bitmap> _dicImages;

	private readonly C1RibbonForm _form;

	private readonly C1Ribbon _ribbon;

	private readonly C1SplitContainer _ctn;

	private readonly C1FlexGridEx _grid;

	private readonly C1ToolBar _tbrCategory;

	private readonly C1TileControlEx _tile;

	private readonly C1ContextMenu _ctx;

	private readonly C1ContextMenu _ctxShowMore;

	private readonly C1CommandHolder _cmdh;

	private readonly C1SplitterPanel _pnlSearch;

	private readonly C1SplitterPanel _pnlCategory;

	private readonly C1TextBoxEx _txbSearch;

	private readonly C1Button _btnCloseSearch;

	private ViewState State;

	private readonly List<Auditai.DTO.Project> _projects = new List<Auditai.DTO.Project>();

	private readonly List<Tuple<string, C1Command>> Categories = new List<Tuple<string, C1Command>>();

	private readonly C1Command _cmdEmptyCategory;

	private readonly C1CommandLink _lnkEmptyCategory;

	private readonly DisplayStyle Style = new DisplayStyle();

	private readonly C1CommandLink _lnkNull;

	private bool _noAllowReentry;

	private bool _isClosing = true;

	private bool _isSearch;

	private bool _anyMenuItemClicked;

	private bool _isShowProjectPayStatusIcon;

	protected Dictionary<string, C1Command> _commandDic = new Dictionary<string, C1Command>(StringComparer.OrdinalIgnoreCase);

	private bool _isInOpenProject;

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
			return _tile.SelectedTile?.Tag as Auditai.DTO.Project;
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
				list.AddRange(_tile.SelectedTiles.Select((Tile t) => t.Tag as Auditai.DTO.Project));
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
		_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Projects16, Resources.Projects24);
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
		_btnCloseSearch = new C1Button
		{
			Image = Resources.close2,
			Text = "",
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Dock = DockStyle.Right
		};
		_btnCloseSearch.Click += _btnCloseSearch_Click;
		_pnlSearch.Controls.Add(_btnCloseSearch);
		_txbSearch = new C1TextBoxEx
		{
			Dock = DockStyle.Fill,
			AutoSize = false
		};
		_txbSearch.TextChanged += _txbSearch_TextChanged;
		_pnlSearch.Controls.Add(_txbSearch);
		_pnlCategory = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Resizable = false,
			KeepRelativeSize = false,
			MinHeight = 0,
			Height = 25
		};
		_ctn.Panels.Add(_pnlCategory);
		_tbrCategory = new C1ToolBar
		{
			Dock = DockStyle.Fill
		};
		_cmdEmptyCategory = new C1Command
		{
			Text = "无分类",
			CheckAutoToggle = true
		};
		_cmdEmptyCategory.Click += _cmdEmptyCategory_Click;
		_lnkEmptyCategory = new C1CommandLink(_cmdEmptyCategory)
		{
			ButtonLook = ButtonLookFlags.Text
		};
		_pnlCategory.Controls.Add(_tbrCategory);
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
		c1SplitterPanel.Controls.Add(_grid);
		_tile = new C1TileControlEx
		{
			CellWidth = 20,
			CellHeight = 15,
			Dock = DockStyle.Fill,
			CellSpacing = 20,
			TileBackColor = Color.Transparent
		};
		_tile.Templates.Add(CreateTileTemplate());
		_tile.MouseUp += _tile_MouseUp;
		_tile.TileClicked += _tile_TileClicked;
		_tile.DoubleClickTile += _tile_DoubleClickTile;
		c1SplitterPanel.Controls.Add(_tile);
		_ctn.Panels.Add(c1SplitterPanel);
		_form.Controls.Add(_ctn);
		_dicImages = new Dictionary<string, Bitmap>
		{
			["RB_CREATEPROJECT"] = Resources.CreateTemplate,
			["RB_OPENPROJECT"] = Resources.OpenTemplate,
			["RB_MODIFYPROJECT"] = Resources.ModifyTemplate,
			["RB_DELETEPROJECT"] = Resources.RemoveProject,
			["RB_DUPLICATEPROJECT"] = Resources.DuplicateProject,
			["RB_EXPORTPROJECT"] = Resources.ProjectExport,
			["RB_EXPORTPROJECTFILE"] = Resources.ProjectExport,
			["RB_IMPORTPROJECT"] = Resources.ProjectExport,
			["RB_SAVEASTEMPLATE"] = Resources.SaveAsTemplate,
			["IL_TILEMODE"] = Resources.tileMode,
			["IL_LISTMODE"] = Resources.listMode,
			["RB_REFRESHPROJECT"] = Resources.RefreshProject,
			["RB_USERMANAGE"] = Resources.Users,
			["RB_USERINFO"] = Resources.SwitchUser,
			["RB_CHANGEPASSWORD"] = Resources.PwdEdit,
			["RB_HELPCENTER"] = Resources.HelpCenter,
			["RB_USETEMPLATE"] = Resources.UseTemplate,
			["RB_CREATETEMPLATE"] = Resources.CreateTemplate,
			["RB_OPENTEMPLATE"] = Resources.OpenTemplate,
			["RB_MODIFYTEMPLATE"] = Resources.ModifyTemplate,
			["RB_DELETETEMPLATE"] = Resources.RemoveTemplate,
			["RB_DUPLICATETEMPLATE"] = Resources.DuplicateTemplate,
			["RB_REFRESHTEMPLATE"] = Resources.RefreshTemplate,
			["RB_SEARCH"] = Resources.SearchProject,
			["RB_RESTORESELECT"] = Resources.RestoreProject,
			["RB_DELETESELECT"] = Resources.DeleteProjectFromServer,
			["RB_EMPTYRECYCLE"] = Resources.EmptyRecycle,
			["RB_SHAREPROJECT"] = Resources.ShareProject,
			["RB_SHARETEMPLATE"] = Resources.ShareProject
		};
		_ribbon = new C1Ribbon
		{
			AllowContextMenu = false
		};
		_ribbon.ApplicationMenu.Visible = false;
		_ribbon.Qat.MenuVisible = false;
		RibbonTab tab2 = AddRibbonTab("TAB_PROJECT", StringConstBase.Current.Project + "管理");
		RibbonGroup group2 = AddRibbonGroup(tab2, "RG_PROJECT", StringConstBase.Current.Project + "管理");
		RibbonButton ribbonButton = AddRibbonButton(group2, "RB_CREATEPROJECT", "新建" + StringConstBase.Current.Project);
		ribbonButton.Visible = !(Program.MainForm.CurrentEdition is AppEditionGeneral);
		AddRibbonButton(group2, "RB_OPENPROJECT", "打开" + StringConstBase.Current.Project);
		AddRibbonButton(group2, "RB_MODIFYPROJECT", (Program.MainForm.CurrentEdition is AppEditionGeneral) ? ("重命名" + StringConstBase.Current.Project) : ("修改" + StringConstBase.Current.Project));
		AddRibbonButton(group2, "RB_DELETEPROJECT", "删除" + StringConstBase.Current.Project);
		ribbonButton = AddRibbonButton(group2, "RB_DUPLICATEPROJECT", "复制" + StringConstBase.Current.Project);
		ribbonButton.Visible = !SoftwareLicenseManager.IsDuplicateProjectOutOfLicenseLimit();
		AddRibbonButton(group2, "RB_EXPORTPROJECT", StringConstBase.Current.Project + "导出");
		AddRibbonButton(group2, "RB_EXPORTPROJECTFILE", "导出项目文件");
		AddRibbonButton(group2, "RB_IMPORTPROJECT", "导入项目");
		ribbonButton = AddRibbonButton(group2, "RB_SAVEASTEMPLATE", "另存" + StringConstBase.Current.Template);
		ribbonButton.Visible = !(Program.MainForm.CurrentEdition is AppEditionGeneral);
		AddRibbonButton(group2, "RB_SEARCH", "搜索" + StringConstBase.Current.Project);
		ribbonButton = AddRibbonButton(group2, "RB_SHAREPROJECT", "跨组织分享" + StringConstBase.Current.Project);
		ribbonButton.Visible = SoftwareLicenseManager.IsAllowShowShareProjectButton();
		AddRibbonButton(group2, "RB_REFRESHPROJECT", "刷新" + StringConstBase.Current.Project);
		group2 = AddRibbonGroup(tab2, "RG_VIEWMODE", "视图模式");
		AddRibbonButton(group2, "RB_VIEWMODE", "磁贴模式");
		group2 = AddRibbonGroup(tab2, "RG_USERMANAGE", "人员管理");
		AddRibbonButton(group2, "RB_USERMANAGE", Auditai.Model.User.Current.IsTeamAdmin ? "同事管理" : "我的同事");
		AddRibbonButton(group2, "RB_USERINFO", "用户资料");
		AddRibbonButton(group2, "RB_CHANGEPASSWORD", "修改密码");
		group2 = AddRibbonGroup(tab2, "RG_HELP", "帮助");
		group2.Visible = SoftwareLicenseManager.IsShowHelpDocumentButton();
		AddRibbonButton(group2, "RB_HELPCENTER", "帮助中心");
		tab2 = AddRibbonTab("TAB_TEMPLATE", StringConstBase.Current.Template + "管理");
		group2 = AddRibbonGroup(tab2, "RG_TEMPLATE", StringConstBase.Current.Template + "管理");
		AddRibbonButton(group2, "RB_USETEMPLATE", "基于" + StringConstBase.Current.Template + "创建项目");
		AddRibbonButton(group2, "RB_CREATETEMPLATE", "新建" + StringConstBase.Current.Template);
		AddRibbonButton(group2, "RB_OPENTEMPLATE", "打开" + StringConstBase.Current.Template);
		AddRibbonButton(group2, "RB_MODIFYTEMPLATE", "修改" + StringConstBase.Current.Template);
		AddRibbonButton(group2, "RB_DELETETEMPLATE", "删除" + StringConstBase.Current.Template);
		AddRibbonButton(group2, "RB_DUPLICATETEMPLATE", "复制" + StringConstBase.Current.Template);
		group2.Items.Add(GetRibbonButton("RB_SEARCH"));
		ribbonButton = AddRibbonButton(group2, "RB_SHARETEMPLATE", "跨组织分享" + StringConstBase.Current.Template);
		ribbonButton.Visible = SoftwareLicenseManager.IsAllowShowShareProjectButton();
		AddRibbonButton(group2, "RB_REFRESHTEMPLATE", "刷新" + StringConstBase.Current.Template);
		tab2.Groups.Add((RibbonGroup)_ribbon.GetItemByName("RG_VIEWMODE"));
		tab2.Groups.Add((RibbonGroup)_ribbon.GetItemByName("RG_USERMANAGE"));
		tab2.Groups.Add((RibbonGroup)_ribbon.GetItemByName("RG_HELP"));
		tab2 = AddRibbonTab("TAB_RECYCLEPROJECT", StringConstBase.Current.Project + "回收站");
		group2 = AddRibbonGroup(tab2, "RG_RECYCLE", Auditai.Model.User.Current.IsTeamAdmin ? "恢复及删除" : "恢复");
		AddRibbonButton(group2, "RB_RESTORESELECT", "恢复所选" + StringConstBase.Current.Project);
		ribbonButton = AddRibbonButton(group2, "RB_DELETESELECT", "删除所选" + StringConstBase.Current.Project);
		ribbonButton.Visible = Auditai.Model.User.Current.IsTeamAdmin;
		ribbonButton = AddRibbonButton(group2, "RB_EMPTYRECYCLE", "清空回收站");
		ribbonButton.Visible = Auditai.Model.User.Current.IsTeamAdmin;
		tab2.Groups.Add((RibbonGroup)_ribbon.GetItemByName("RG_VIEWMODE"));
		tab2 = AddRibbonTab("TAB_RECYCLETEMPLATE", StringConstBase.Current.Template + "回收站");
		tab2.Groups.Add((RibbonGroup)_ribbon.GetItemByName("RG_RECYCLE"));
		tab2.Groups.Add((RibbonGroup)_ribbon.GetItemByName("RG_VIEWMODE"));
		_form.Controls.Add(_ribbon);
		_cmdh = C1CommandHolder.CreateCommandHolder(_form);
		_lnkNull = new C1CommandLink();
		_cmdh.CommandClick += _cmdh_CommandClick;
		_ctx = new C1ContextMenu();
		_ctx.Popup += _ctx_Popup;
		_ctx.Closed += _ctx_Closed;
		_ctxShowMore = _ctx;
		_ctxShowMore.Popup += _ctxShowMore_Popup;
		AddCommandWithImage("CTX_CMD_OPEN_PROJECT", "打开" + StringConstBase.Current.Project, _ctxShowMore, Resources.TicketNavTreeListExpanded);
		AddCommandWithImage("CTX_CMD_RENAME_PROJECT", (Program.MainForm.CurrentEdition is AppEditionGeneral) ? ("重命名" + StringConstBase.Current.Project) : ("修改" + StringConstBase.Current.Project), _ctxShowMore, ContextResources.ctxMofify);
		AddCommandWithImage("CTX_CMD_DELETE_PROJECT", "删除" + StringConstBase.Current.Project, _ctxShowMore, Resources.RemoveProject16);
		AddCommandWithImage("CTX_CMD_DUPLICATE_PROJECT", "复制" + StringConstBase.Current.Project, _ctxShowMore, ContextResources.ctxCopy);
		AddCommandWithImage("CTX_CMD_EXPORT_PROJECT", "导出" + StringConstBase.Current.Project, _ctxShowMore, Resources.BatchExport16);
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
		_tile.AddTileCustomButtonRender(new TileShowMoreMenuIconRender(this, _tile));
		if (UserTeam.CurrentTeamIsPayByProject)
		{
			_tile.AddTileCustomButtonRender(new TilePayStatusIconRender(this, _tile));
		}
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
		RibbonButton AddRibbonButton(RibbonGroup group, string name, string text)
		{
			RibbonButton ribbonButton2 = new RibbonButton
			{
				Name = name,
				Text = text,
				TextImageRelation = C1.Win.C1Ribbon.TextImageRelation.ImageAboveText
			};
			if (_dicImages.TryGetValue(name, out var value))
			{
				ribbonButton2.LargeImage = value;
			}
			ribbonButton2.Click += RibbonButton_Click;
			group.Items.Add(ribbonButton2);
			return ribbonButton2;
		}
		static RibbonGroup AddRibbonGroup(RibbonTab tab, string name, string text)
		{
			RibbonGroup ribbonGroup = new RibbonGroup
			{
				Name = name,
				Text = text
			};
			tab.Groups.Add(ribbonGroup);
			return ribbonGroup;
		}
		RibbonTab AddRibbonTab(string name, string text)
		{
			RibbonTab ribbonTab = new RibbonTab
			{
				Name = name,
				Text = text
			};
			_ribbon.Tabs.Add(ribbonTab);
			ribbonTab.Select += Tab_Select;
			return ribbonTab;
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
		_tile.TileBorderColor = Color.Transparent;
		_tile.CustomBorderColor = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
	}

	public async Task Populate()
	{
		_grid.BeginUpdate();
		await FetchModel();
		PopulateModel();
		_grid.EndUpdate();
		PopulateCategory();
		PopulateViewMode();
		PopulateSearch();
		PopulateForm();
		SetRibbonState();
	}

	private void PopulateModel()
	{
		List<Auditai.DTO.Project> projects = SortProjectImpl(_isSearch ? SearchProjects(_projects) : FilterProjects(_projects), Style.SortKind).ToList();
		PopulateGrid(projects);
		PopulateTile(projects);
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
					projectList = recycleProjects.Where((Auditai.DTO.Project p) => p.Type == ProjectType.Project);
				}
				else
				{
					progressForm.SetProgressDisplayValueConverter(new ProgressDisplayValueConverter_SmoothByTime(0.2f));
					progressRuntimeData.UpdateMessage("正在获取回收" + StringConstBase.Current.Template + "信息，请稍候...");
					progressRuntimeData.UpdateProgress(0.8f);
					var recycleProjects = await Auditai.LocalDataStore.StorageRouter.GetRecycleProjects();
					projectList = recycleProjects.Where((Auditai.DTO.Project p) => p.Type == ProjectType.Template);
				}
			});
			_projects.Clear();
			_projects.AddRange(projectList);
			await Task.Delay(1);
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
	}

	private void PopulateForm()
	{
		if (State == ViewState.Project)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Projects24, Resources.Projects16);
			_form.Text = StringConstBase.Current.Project + "管理";
		}
		else if (State == ViewState.Template)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Templates16, Resources.Templates24);
			_form.Text = StringConstBase.Current.Template + "管理";
		}
		else if (State == ViewState.RecycleProject)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.RemoveProject16, Resources.RemoveProject24);
			_form.Text = StringConstBase.Current.Project + "回收站";
		}
		else if (State == ViewState.RecycleTemplate)
		{
			_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.RemoveProject16, Resources.RemoveProject24);
			_form.Text = StringConstBase.Current.Template + "回收站";
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
		_grid.AutoSizeCols();
		_grid.EndUpdate();
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
			}
			else if (State == ViewState.Template || State == ViewState.RecycleTemplate)
			{
				string value;
				IEnumerable<Auditai.DTO.User> value2;
				var users = project.Users ?? Enumerable.Empty<Auditai.DTO.User>();
				if (project.SystemBuild)
				{
					if (Auditai.Model.User.Current.IsSystemSupporter)
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

	private void PopulateTile(List<Auditai.DTO.Project> projects)
	{
		_tile.ClearSelected();
		_tile.BeginUpdate();
		_tile.Groups.Clear();
		_tile.AllowMultiSelect = State == ViewState.RecycleProject || State == ViewState.RecycleTemplate;
		_tile.ClearExternalImage();
		_isShowProjectPayStatusIcon = false;
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			_isShowProjectPayStatusIcon = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_project_tile_pay_status", defaultValue: true);
		}
		if (!(Program.MainForm.CurrentEdition is AppEditionGeneral) && (State == ViewState.Project || State == ViewState.Template))
		{
			IEnumerable<string> recent = ProjectInfoManager.GetInstance().GetRecent();
			C1.Win.C1Tile.Group group = new C1.Win.C1Tile.Group
			{
				Text = "最近使用"
			};
			foreach (Auditai.DTO.Project item2 in from i in recent
				select projects.Find((Auditai.DTO.Project p) => p.Id.ToString() == i) into p
				where p != null
				select p)
			{
				group.Tiles.Add(CreateProjectTile(item2));
			}
			_tile.Groups.Add(group);
		}
		C1.Win.C1Tile.Group group2 = new C1.Win.C1Tile.Group();
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			group2.Text = "";
		}
		else if (State == ViewState.Project)
		{
			group2.Text = "所有" + StringConstBase.Current.Project;
		}
		else if (State == ViewState.Template)
		{
			group2.Text = "所有" + StringConstBase.Current.Template;
		}
		if (Program.MainForm.CurrentEdition is AppEditionGeneral && !SoftwareLicenseManager.IsAddProjectOutOfLicenseLimit())
		{
			Tile item = new Tile
			{
				Tag = TAG_CREATEPROJECTTILE,
				VerticalSize = 4,
				HorizontalSize = 5,
				Text = "新建" + StringConstBase.Current.Project,
				Template = _tile.Templates["TT_PROJECT"],
				Image1 = Resources.CreateModule
			};
			group2.Tiles.Add(item);
		}
		foreach (Auditai.DTO.Project project in projects)
		{
			group2.Tiles.Add(CreateProjectTile(project));
		}
		_tile.Groups.Add(group2);
		_tile.EndUpdate();
	}

	private Tile CreateProjectTile(Auditai.DTO.Project project)
	{
		Tile tile = new Tile
		{
			Tag = project,
			VerticalSize = 4,
			HorizontalSize = 5,
			Text = project.Name,
			Template = _tile.Templates["TT_PROJECT"],
			Image1 = ((project.Type == ProjectType.Project) ? ProjectTileImage : ((!project.SystemBuild) ? CustomTemplateTileImage : ((project.ChargeType == ChargeType.Pay) ? VipSystemTemplateTileImage : SystemTemplateTileImage)))
		};
		if (_isShowProjectPayStatusIcon)
		{
			Point offset = new Point(3, 3);
			if (project.ProjectChargeType == ChargeType.Pay)
			{
				_tile.AddLeftTopImage(tile, Program.MainForm.CurrentEdition.PayedTemplateTileCornerIcon, offset);
			}
			else
			{
				_tile.AddLeftTopImage(tile, Program.MainForm.CurrentEdition.UnPayTemplateTileCornerIcon, offset);
			}
		}
		return tile;
	}

	private void PopulateCategory()
	{
		if (State != ViewState.Project && State != ViewState.Template)
		{
			return;
		}
		_cmdEmptyCategory.Checked = false;
		Dictionary<string, bool> dictionary = Categories.ToDictionary((Tuple<string, C1Command> tup) => tup.Item1, (Tuple<string, C1Command> tup) => tup.Item2.Checked);
		Categories.Clear();
		_tbrCategory.CommandLinks.Clear();
		_tbrCategory.CommandLinks.Add(_lnkEmptyCategory);
		HashSet<string> hashSet = new HashSet<string>(_projects.Select((Auditai.DTO.Project p) => (p.Category ?? "").Split('|')).SelectMany((string[] cats) => cats));
		hashSet.Remove("");
		foreach (string item in hashSet)
		{
			C1Command c1Command = new C1Command
			{
				CheckAutoToggle = true,
				Text = item
			};
			Categories.Add(Tuple.Create(item, c1Command));
			if (dictionary.TryGetValue(item, out var value))
			{
				c1Command.Checked = value;
			}
			c1Command.Click += delegate
			{
				PopulateModel();
			};
			C1CommandLink value2 = new C1CommandLink(c1Command)
			{
				ButtonLook = ButtonLookFlags.Text
			};
			_tbrCategory.CommandLinks.Add(value2);
		}
	}

	private void PopulateViewMode()
	{
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			((RibbonGroup)_ribbon.GetItemByName("RG_VIEWMODE")).Visible = false;
			Style.ViewMode = ListTileViewMode.Tile;
			_grid.Hide();
			_tile.Show();
			return;
		}
		RibbonButton ribbonButton = GetRibbonButton("RB_VIEWMODE");
		if (Style.ViewMode == ListTileViewMode.List)
		{
			_grid.Show();
			_tile.Hide();
			ribbonButton.Text = "磁贴模式";
			ribbonButton.LargeImage = _dicImages["IL_TILEMODE"];
		}
		else if (Style.ViewMode == ListTileViewMode.Tile)
		{
			_grid.Hide();
			_tile.Show();
			ribbonButton.Text = "列表模式";
			ribbonButton.LargeImage = _dicImages["IL_LISTMODE"];
		}
	}

	public IEnumerable<Auditai.DTO.Project> SortProjectImpl(IEnumerable<Auditai.DTO.Project> projects, SortKind kind)
	{
		switch (kind)
		{
		case SortKind.CreateTime:
			return projects.OrderBy((Auditai.DTO.Project i) => i.CreateTime);
		case SortKind.OpenTime:
		{
			Dictionary<Auditai.DTO.Project, DateTime> source = projects.ToDictionary((Auditai.DTO.Project p) => p, (Auditai.DTO.Project p) => ProjectInfoManager.GetInstance().GetProject(p.Id.ToString())?.OpenTime ?? DateTime.MinValue);
			return from i in source
				orderby i.Value descending
				select i.Key;
		}
		case SortKind.Number:
			return projects.OrderBy((Auditai.DTO.Project p) => p.Number);
		case SortKind.Name:
			return projects.OrderBy((Auditai.DTO.Project p) => p.Name);
		case SortKind.Category:
			return projects.OrderBy((Auditai.DTO.Project p) => p.Category);
		default:
			return projects.OrderBy((Auditai.DTO.Project p) => p.Number);
		}
	}

	private IEnumerable<Auditai.DTO.Project> FilterProjects(IEnumerable<Auditai.DTO.Project> projects)
	{
		if (!_cmdEmptyCategory.Checked && Categories.All((Tuple<string, C1Command> tup) => !tup.Item2.Checked))
		{
			return projects;
		}
		List<Tuple<string, C1Command>> selectedCats = Categories.Where((Tuple<string, C1Command> u) => u.Item2.Checked).ToList();
		return projects.Where(delegate(Auditai.DTO.Project p)
		{
			HashSet<string> hashSet = new HashSet<string>(p.Category.Split('|'));
			hashSet.Remove("");
			if (_cmdEmptyCategory.Checked && !string.IsNullOrWhiteSpace(p.Category))
			{
				return false;
			}
			foreach (Tuple<string, C1Command> item in selectedCats)
			{
				if (!hashSet.Contains(item.Item1))
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
			_ribbon.Tabs["TAB_PROJECT"].Selected = true;
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
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
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
		_ = 1;
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
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{project.ProjectLicenseDate:yyyy年MM月dd日}到期，无法同步{StringConstBase.Current.Project}，您可致电官方客服电话：400-690-6500，联系购买或续期！");
						}
						else if (totalDays < 30.0)
						{
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品将于{project.ProjectLicenseDate:yyyy年MM月dd日}到期，建议您致电官方客服电话：400-690-6500，联系购买或续期！");
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
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{project.ProjectLicenseDate:yyyy年MM月dd日}到期，无法同步{StringConstBase.Current.Project}，您可致电官方客服电话：400-690-6500，联系购买或续期！");
							flag = false;
						}
						else if (totalDays2 < 30.0)
						{
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品将于{project.ProjectLicenseDate:yyyy年MM月dd日}到期，建议您致电官方客服电话：400-690-6500，联系购买或续期！");
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
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
			{
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
				return Task.FromResult<object>(null);
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			ProjectInfoManager.GetInstance().UpdateOpenTime(SelectedProject.Id.ToString(), DateTime.Now);
			await Populate();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
	}

	private async Task DeleteProject()
	{
		if (!HasSelectedProject)
		{
			return;
		}
		string text = ((State == ViewState.Project) ? StringConstBase.Current.Project : (StringConstBase.Current.Template ?? ""));
		if (Program.MainForm?.CurrentProject?.Id == SelectedProject.Id)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "不能删除当前正在编辑的" + text);
		}
		else
		{
			if (Program.ClientPlatformType == PlatformType.EnterpriseManagerPlatform || Program.ClientPlatformType == PlatformType.EnterpriseReportPlatform || Program.ClientPlatformType == PlatformType.TableDevelopPlatform || Program.ClientPlatformType == PlatformType.ProductionCostAccountingSystem || Program.ClientPlatformType == PlatformType.ContractLedgerManagementSystem || Program.ClientPlatformType == PlatformType.RDExpenseLedgerSystem || Program.ClientPlatformType == PlatformType.SalesOrderManagementSystem || Program.ClientPlatformType == PlatformType.PSIManagementSystem || Program.ClientPlatformType == PlatformType.ProjectLedgerManagementSystem || Program.ClientPlatformType == PlatformType.Custom)
			{
				if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "警告：此操作将彻底删除所选" + text + "的数据，无法恢复，请谨慎确认此次操作！\n\n若确认要删除【" + SelectedProject.Name + "】，点击“确定”。", MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					return;
				}
				if (TokenTimer.LoginInfo != null && TokenTimer.LoginInfo.LoginMode == LoginMode.SMS)
				{
					try
					{
						if (!StorageRouter.IsLocalMode)
						{
							string text2 = await WebApiClient.GetDeleteProjectValidateCode(UserSet.LoginPhone);
							string text3 = InputForm.Text("删除验证", "请输入您收到的验证码以防止是在您的误操作下删除" + text + "：", null, 256);
							if (text3 == null)
							{
								return;
							}
							if (text3.Trim() != text2)
							{
								Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "输入的验证码不正确，终止删除操作！");
								return;
							}
						}
					}
					catch (HttpRequestException ex)
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
						return;
					}
					catch (Exception ex2)
					{
						ex2.Log();
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
						return;
					}
				}
				else
				{
					string text4 = InputForm.Password("删除验证", "请输入您的登录密码以防止是在您的误操作下删除" + text + "：", null, 256);
					if (text4 == null)
					{
						return;
					}
					if (frmLogin.GetPasswordEncryptValue(text4.Trim()) != UserSet.LoginPassword)
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "输入的密码不正确，终止删除操作！");
						return;
					}
				}
				try
				{
					List<Guid> o = new List<Guid> { SelectedProject.Id };
					JObject jObject = new JObject();
					jObject["Ids"] = JToken.FromObject(o);
					await StorageRouter.DeleteProjectFromServer(jObject);
					await Populate();
					return;
				}
				catch (HttpRequestException ex3)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
					return;
				}
			}
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
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
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
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
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
		SetRibbonState();
	}

	private async Task ExportProject(RibbonButton btn)
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

	/// <summary>导出选中项目为 .lqaudit 归档文件（含项目数据库和元信息）</summary>
	private async Task ExportProjectFile()
	{
		var selected = SelectedProject;
		if (selected == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要导出的" + StringConstBase.Current.Project);
			return;
		}
		string defaultName = string.IsNullOrWhiteSpace(selected.Number)
			? $"{selected.Name}.lqaudit"
			: $"{selected.Number} {selected.Name}.lqaudit";
		foreach (char c in Path.GetInvalidFileNameChars())
			defaultName = defaultName.Replace(c, '_');
		using (var sfd = new SaveFileDialog
		{
			Filter = "项目归档文件 (*.lqaudit)|*.lqaudit",
			FileName = defaultName,
			Title = "导出项目文件"
		})
		{
			if (sfd.ShowDialog() != DialogResult.OK) return;
			try
			{
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

	/// <summary>从 .lqaudit 归档文件导入项目</summary>
	private async Task ImportProject()
	{
		using (var ofd = new OpenFileDialog
		{
			Filter = "项目归档文件 (*.lqaudit)|*.lqaudit",
			Title = "导入项目",
			CheckFileExists = true
		})
		{
			if (ofd.ShowDialog() != DialogResult.OK) return;
			try
			{
				// 读取元信息预览
				var metadata = ProjectArchive.ReadMetadata(ofd.FileName);
				string preview = $"项目名称：{metadata.ProjectName}\n" +
					$"项目编号：{metadata.ProjectNumber}\n" +
					$"项目类别：{metadata.Category}\n" +
					$"被审计单位：{metadata.Auditee}\n" +
					$"创建时间：{metadata.CreateTime:yyyy-MM-dd}\n" +
					$"导出时间：{metadata.ExportTime:yyyy-MM-dd HH:mm}\n" +
					$"导出人：{metadata.ExportedBy}\n" +
					$"版本：v{metadata.SchemaVersion}";
				if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question,
					$"确认导入以下项目？\n\n{preview}", MessageBoxButtons.OKCancel, "导入项目确认") != DialogResult.OK)
					return;

				// 执行导入
				var newProject = await Task.Run(() => ProjectArchive.Import(ofd.FileName));
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
			_ribbon.Tabs["TAB_TEMPLATE"].Selected = true;
			await Populate();
			FindAndSelectRow(newTemplate);
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
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

	private void HelpCenter()
	{
		HelpCenterUtil.OpenHelpCenterHomePage();
	}

	private async Task UseTemplate()
	{
		if (SelectedProject.Type != ProjectType.Template || !SelectedProject.SystemBuild || SelectedProject.ChargeType != ChargeType.Pay || !SoftwareLicenseManager.IsUsePayProjectOutOfLicenseLimit())
		{
			await CreateProject(SelectedProject);
			return;
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您是" + SoftwareLicenseManager.GetUnPayedLicenseDisplayName() + "用户，您选中的" + StringConstBase.Current.Template + "为" + SoftwareLicenseManager.GetPayedLicenseDisplayName() + "用户专用，请联系官方客服升级为" + SoftwareLicenseManager.GetPayedLicenseDisplayName() + "用户后再使用该" + StringConstBase.Current.Template + "创建" + StringConstBase.Current.Project + "！");
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
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
	}

	private async Task ModifyTemplate()
	{
		if (!HasSelectedProject)
		{
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
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
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
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
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
			_pnlCategory.Hide();
			if (_isSearch)
			{
				GetRibbonButton("RB_SEARCH").Text = "关闭搜索";
				_pnlSearch.Show();
			}
			else
			{
				GetRibbonButton("RB_SEARCH").Text = ((State == ViewState.Project) ? ("搜索" + StringConstBase.Current.Project) : ("搜索" + StringConstBase.Current.Template));
				_pnlSearch.Hide();
			}
		}
		else if (State == ViewState.RecycleProject || State == ViewState.RecycleTemplate)
		{
			_pnlSearch.Hide();
			_pnlCategory.Hide();
		}
		else if (_isSearch)
		{
			GetRibbonButton("RB_SEARCH").Text = "关闭搜索";
			_pnlSearch.Show();
			_pnlCategory.Hide();
		}
		else
		{
			GetRibbonButton("RB_SEARCH").Text = ((State == ViewState.Project) ? ("搜索" + StringConstBase.Current.Project) : ("搜索" + StringConstBase.Current.Template));
			_pnlSearch.Hide();
			_pnlCategory.Show();
		}
	}

	private void SetRibbonState()
	{
		if (State == ViewState.Project)
		{
			GetRibbonButton("RB_OPENPROJECT").Enabled = HasSelectedProject;
			GetRibbonButton("RB_MODIFYPROJECT").Enabled = CanModifyProject();
			GetRibbonButton("RB_DELETEPROJECT").Enabled = CanModifyProject();
			GetRibbonButton("RB_DUPLICATEPROJECT").Enabled = CanDuplicateProject();
			GetRibbonButton("RB_EXPORTPROJECT").Enabled = CanDuplicateProject();
			GetRibbonButton("RB_EXPORTPROJECTFILE").Enabled = HasSelectedProject;
			GetRibbonButton("RB_SAVEASTEMPLATE").Enabled = CanDuplicateProject();
			GetRibbonButton("RB_SHAREPROJECT").Visible = CanSeeShareProject();
			GetRibbonButton("RB_SHAREPROJECT").Enabled = CanShareProject();
		}
		else if (State == ViewState.Template)
		{
			GetRibbonButton("RB_USETEMPLATE").Enabled = HasSelectedProject;
			GetRibbonButton("RB_OPENTEMPLATE").Enabled = CanOpenTemplate();
			GetRibbonButton("RB_MODIFYTEMPLATE").Enabled = CanOpenTemplate();
			GetRibbonButton("RB_DELETETEMPLATE").Enabled = CanOpenTemplate();
			GetRibbonButton("RB_DUPLICATETEMPLATE").Enabled = HasSelectedProject;
			GetRibbonButton("RB_SHARETEMPLATE").Visible = CanSeeShareProject();
			GetRibbonButton("RB_SHARETEMPLATE").Enabled = CanOpenTemplate();
		}
		else if (State == ViewState.RecycleProject)
		{
			GetRibbonButton("RB_DELETESELECT").Text = "删除所选" + StringConstBase.Current.Project;
			GetRibbonButton("RB_DELETESELECT").Enabled = SelectedProjects.Count > 0;
			GetRibbonButton("RB_RESTORESELECT").Text = "恢复所选" + StringConstBase.Current.Project;
			GetRibbonButton("RB_RESTORESELECT").Enabled = SelectedProjects.Count > 0;
		}
		else if (State == ViewState.RecycleTemplate)
		{
			GetRibbonButton("RB_DELETESELECT").Text = "删除所选" + StringConstBase.Current.Template;
			GetRibbonButton("RB_DELETESELECT").Enabled = SelectedProjects.Count > 0;
			GetRibbonButton("RB_RESTORESELECT").Text = "恢复所选" + StringConstBase.Current.Template;
			GetRibbonButton("RB_RESTORESELECT").Enabled = SelectedProjects.Count > 0;
		}
	}

	private bool CanModifyProject()
	{
		if (!HasSelectedProject)
		{
			return false;
		}
		if (SelectedProject.Users == null)
		{
			return false;
		}
		Auditai.DTO.User user = SelectedProject.Users.FirstOrDefault((Auditai.DTO.User u) => u.Id == Auditai.Model.User.Current.Id);
		if (user == null)
		{
			return false;
		}
		return user.Role == UserRole.Manager;
	}

	private bool CanDuplicateProject()
	{
		if (!HasSelectedProject)
		{
			return false;
		}
		if (SelectedProject.Users == null)
		{
			return false;
		}
		Auditai.DTO.User user = SelectedProject.Users.FirstOrDefault((Auditai.DTO.User u) => u.Id == Auditai.Model.User.Current.Id);
		if (user == null)
		{
			return false;
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
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (SelectedProject.Users == null)
		{
			return false;
		}
		Auditai.DTO.User user = SelectedProject.Users.FirstOrDefault((Auditai.DTO.User u) => u.Id == Auditai.Model.User.Current.Id);
		if (user == null)
		{
			return false;
		}
		return user.Role == UserRole.Editor;
	}

	private Template CreateTileTemplate()
	{
		Template template = new Template
		{
			Name = "TT_PROJECT"
		};
		PanelElement panelElement = new PanelElement();
		panelElement.Alignment = ContentAlignment.TopCenter;
		ImageElement imageElement = new ImageElement();
		imageElement.AlignmentOfContents = ContentAlignment.TopCenter;
		imageElement.FixedHeight = 50;
		imageElement.FixedWidth = 50;
		imageElement.ImageSelector = ImageSelector.Image1;
		panelElement.Children.Add(imageElement);
		panelElement.FixedHeight = 50;
		panelElement.FixedWidth = 50;
		panelElement.Margin = new Padding(0, 30, 0, 0);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.Alignment = ContentAlignment.MiddleRight;
		panelElement2.FixedHeight = 30;
		panelElement2.FixedWidth = 40;
		panelElement2.Margin = new Padding(0, 0, 3, 0);
		PanelElement panelElement3 = new PanelElement();
		panelElement3.Alignment = ContentAlignment.BottomCenter;
		TextElement textElement = new TextElement();
		textElement.AlignmentOfContents = ContentAlignment.TopCenter;
		textElement.TextTrimming = TextTrimming.EndEllipsis;
		textElement.SingleLine = false;
		textElement.FixedHeight = 50;
		textElement.FixedWidth = 150;
		panelElement3.Children.Add(textElement);
		panelElement3.FixedHeight = 50;
		panelElement3.FixedWidth = 150;
		template.Elements.Add(panelElement);
		template.Elements.Add(panelElement2);
		template.Elements.Add(panelElement3);
		return template;
	}

	private C1Command GetCommand(string name)
	{
		if (_commandDic.TryGetValue(name, out var value))
		{
			return value;
		}
		return _cmdh.Commands[name];
	}

	private RibbonButton GetRibbonButton(string name)
	{
		return (RibbonButton)_ribbon.GetItemByName(name);
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
				_ribbon.Tabs["TAB_PROJECT"].Selected = true;
				break;
			case ViewState.RecycleTemplate:
				_ribbon.Tabs["TAB_TEMPLATE"].Selected = true;
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
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "此操作将彻底删除所选数据，无法恢复，请谨慎确认此次操作！", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		List<Guid> o = SelectedProjects.Select((Auditai.DTO.Project p) => p.Id).ToList();
		try
		{
			JObject jobj = new JObject();
			jobj["Ids"] = JToken.FromObject(o);
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
			{
				iProg.Report(new ProgressInfo
				{
					MainCaption = "正在删除数据，请稍候...",
					MainProgress = 100
				});
				await StorageRouter.DeleteProjectFromServer(jobj);
				return (object)null;
			});
			progressForm.ShowDialog();
			await progressForm.Task;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		await Populate();
	}

	private async Task DeleteAllProjectFromServer()
	{
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "此操作将彻底删除回收站数据，无法恢复，请谨慎确认此次操作！", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		List<Guid> o = _projects.Select((Auditai.DTO.Project p) => p.Id).ToList();
		try
		{
			JObject jobj = new JObject();
			jobj["Ids"] = JToken.FromObject(o);
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
			{
				iProg.Report(new ProgressInfo
				{
					MainCaption = "正在删除数据，请稍候...",
					MainProgress = 100
				});
				await StorageRouter.DeleteProjectFromServer(jobj);
				return (object)null;
			});
			progressForm.ShowDialog();
			await progressForm.Task;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		await Populate();
	}

	private async void _form_Shown(object sender, EventArgs e)
	{
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			_ribbon.HideTabHeaderRow = true;
		}
		PopulateViewMode();
		PopulateSearch();
		Theme.SetCurrentTree(_form);
		SetTheme();
		State = ViewState.Project;
		await Populate();
		if (!(Program.MainForm.CurrentEdition is AppEditionGeneral) && _projects.Count == 0)
		{
			State = ViewState.Template;
			_ribbon.Tabs["TAB_TEMPLATE"].Selected = true;
			await Populate();
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

	private async void Tab_Select(object sender, EventArgs e)
	{
		RibbonTab ribbonTab = sender as RibbonTab;
		switch (ribbonTab.Name)
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
		}
		_ribbon.Enabled = false;
		await Populate();
		_ribbon.Enabled = true;
	}

	private async void _cmdh_CommandClick(object sender, CommandClickEventArgs e)
	{
		string name = e.Command.Name;
		if (name == null)
		{
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

	private void _cmdEmptyCategory_Click(object sender, ClickEventArgs e)
	{
		PopulateModel();
	}

	private async void RibbonButton_Click(object sender, EventArgs e)
	{
		if (_noAllowReentry)
		{
			return;
		}
		_noAllowReentry = true;
		try
		{
			RibbonButton ribbonButton = sender as RibbonButton;
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
				case 'H':
					if (name == "RB_HELPCENTER")
					{
						HelpCenter();
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
			case 'I':
				if (name == "RB_IMPORTPROJECT")
				{
					await ImportProject();
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
		foreach (Tuple<string, C1Command> category in Categories)
		{
			C1Command c1Command = new C1Command();
			c1Command.Text = category.Item1;
			c1Command.CheckAutoToggle = true;
			c1Command.Checked = SelectedProject.Category.Split('|').Contains(category.Item1);
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
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		try
		{
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
			SetRibbonState();
		}
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		if ((State == ViewState.Project || State == ViewState.Template) && e.Button == MouseButtons.Right && _grid.HitTest(e.Location).Type == HitTestTypeEnum.Cell)
		{
			_ctx.ShowContextMenu(_grid, e.Location);
		}
	}

	private void _grid_CellChecked(object sender, RowColEventArgs e)
	{
		SetRibbonState();
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		_grid.DrawFormBorder(e.Graphics);
	}

	private async void _tile_DoubleClickTile(object sender, Tile e)
	{
		_noAllowReentry = true;
		if (_tile.SelectedTile?.Tag == TAG_CREATEPROJECTTILE)
		{
			await CreateProject(null);
		}
		else if (State == ViewState.Project || State == ViewState.Template)
		{
			await OpenProject();
		}
		_noAllowReentry = false;
	}

	private void _tile_TileClicked(object sender, TileEventArgs e)
	{
		if (State == ViewState.RecycleProject || State == ViewState.RecycleTemplate)
		{
			_tile.ToggleTile(e.Tile);
		}
		SetRibbonState();
	}

	private void _tile_MouseUp(object sender, MouseEventArgs e)
	{
		if ((State == ViewState.Project || State == ViewState.Template) && e.Button == MouseButtons.Right)
		{
			Tile tileAt = _tile.GetTileAt(e.Location);
			if (tileAt == null)
			{
				GetCommand("CMD_OPENFROMSERVER").Visible = false;
				GetCommand("CMD_CATEGORY").Visible = false;
				_ctx.ShowContextMenu(_tile, e.Location);
			}
			else if (tileAt.Tag != TAG_CREATEPROJECTTILE)
			{
				GetCommand("CMD_OPENFROMSERVER").Visible = true;
				GetCommand("CMD_CATEGORY").Visible = true;
				_ctx.ShowContextMenu(_tile, e.Location);
			}
		}
	}

	private void _txbSearch_TextChanged(object sender, EventArgs e)
	{
		PopulateModel();
	}
}
