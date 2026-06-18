﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.C1Preview.Export;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using C1.Win.C1SuperTooltip;
using C1.Win.C1Tile;
using FileTransferModel;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.PlatformResource;
using Leqisoft.SignalR;
using Leqisoft.UI.CommonControls;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.CellCollect;
using Leqisoft.UI.Controls.CollectCell;
using Leqisoft.UI.Controls.CollectTable;
using Leqisoft.UI.LedgerView;
using Leqisoft.UI.Platform.Chat;
using Leqisoft.UI.Platform.Properties;
using Leqisoft.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Leqisoft.LocalDataStore;

namespace Leqisoft.UI.Platform;

public class MainForm
{
	private class RecentProjectInfo
	{
		public Leqisoft.DTO.Project Dto { get; set; }

		public TreeNodeBase LastNode { get; set; }
	}

	private const int ANIMATESPEED = 100;

	private C1SplitContainer ctnRibbonFormClientArea;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlLedger;

	private C1SplitterPanel pnlMain;

	private C1SplitContainer ctnMain;

	private C1SplitterPanel pnlHelp;

	private C1SplitterPanel pnlContent;

	private C1SplitterPanel pnlFormula;

	private C1StatusBar staMain;

	private C1SplitterPanel pnlTree;

	private C1SplitterPanel pnlNav;

	private C1SplitterPanel pnlCtnAllAboveSpace;

	private C1SplitterPanel pnlCtnAllParent;

	private bool _isSplitterMoving;

	private bool _ispnlMainShow;

	private bool _closeByCode;

	private readonly int _animateScreenWidth = Screen.PrimaryScreen.Bounds.Width;

	private Timer _animateTimer = new Timer
	{
		Interval = 1
	};

	private bool _animateToRight = true;

	private bool _firstTimeOpen = true;

	private Form _animateForm = new Form
	{
		StartPosition = FormStartPosition.Manual,
		FormBorderStyle = FormBorderStyle.None,
		ShowInTaskbar = false,
		TopLevel = true
	};

	private bool _isPreview;

	private readonly Lazy<ReportPreview> _lazyPreview;

	private int _currentErrorIndex = -1;

	public static RibbonImageProcess ImageProcess = new RibbonImageProcess();

	private StepRecorder<Tuple<Guid, long>> stepRecorder;

	private bool _shouldRecord = true;

	private Lazy<ReportExportToExcel> _lazyToExcel;

	private Lazy<TableEditor> _lazyTableEditor;

	private Lazy<TicketInputEditor2> _lazyTicketEditor;

	private Lazy<TicketPrinter> _lazyTicketPrinter;

	protected readonly TooltipBox _ttpFormulaHint = new TooltipBox
	{
		Opacity = 0.8,
		IsBalloon = true
	};

	private readonly SoundPlayer _soundPlayer = new SoundPlayer();

	private Hashtable _serverDataChangedProject = Hashtable.Synchronized(new Hashtable());

	private AppEditionBase _currentEdition;

	private DocumentEditor _currentDocumentEditor;

	private bool _isInSyncingProject;

	private object _syncLock = new object();

	private double _pnlNavLastSizeRatio = 15.0;

	private bool _isDelayChangeNavigationPanelVisibleStatus;

	private bool _isNavigationPanelVisible;

	private int _mainPandelSuspendDepth;

	private List<Control> _suspendControlList = new List<Control>();

	private MainFormView _preView;

	private static HashSet<string> _controlFormulaDic;

	private static HashSet<string> _obsoletedFormulaDic;

	private bool _showSideToolbar = true;

	private MessageHandle messageHandle;

	private AppCommandTab _previousTableTab;

	private AppCommandTab _previousTicketTab;

	private AppCommandTab _previousDocumentTab;

	private bool openedRelate;

	private TooltipBox ttpRelated = new TooltipBox();

	public bool FormShowned { get; private set; }

	internal static Dictionary<Guid, Leqisoft.Model.Project> RecentProjects { get; } = new Dictionary<Guid, Leqisoft.Model.Project>();


	public C1RibbonForm View { get; private set; }

	public MainFormView CurrentView { get; private set; }

	public FormulaMap FormulaMap { get; } = new FormulaMap();


	public FormulaEditor FormulaEditor { get; } = new FormulaEditor();


	public SplitContainerRibbonControlHost StatusBar { get; private set; }

	private BrowserWrapper _browser;
	public BrowserWrapper Browser => _browser ?? (_browser = CreateBrowserSafe());

	private BrowserWrapper CreateBrowserSafe()
	{
		try { return new BrowserWrapper(); }
		catch { return null; }
	}


	public TableEditor TableEditor => _lazyTableEditor.Value;

	public TicketDesignEditor2 TicketDesignEditor { get; } = new TicketDesignEditor2();


	public TicketInputEditor2 TicketInputEditor => _lazyTicketEditor.Value;

	public TicketPrinter TicketPrinter => _lazyTicketPrinter.Value;

	public MultiLedgerViewer MultiLedgerViewer { get; } = new MultiLedgerViewer();


	public LedgerViewer CurrentLedgerViewer => MultiLedgerViewer.CurrentLedgerViewer;

	public Dictionary<string, LedgerViewer> OpenedLedgerViewerDic => MultiLedgerViewer.OpenedLedgerViewerDic;

	public bool RibbonAdded { get; set; }

	public AppEditionBase CurrentEdition
	{
		get
		{
			return _currentEdition;
		}
		set
		{
			if (_currentEdition != null)
			{
				_currentEdition.DetachRibbon();
				View.Controls.Remove(_currentEdition.Ribbon);
			}
			_currentEdition = value;
			_currentEdition.GenerateRibbon();
			ImageProcess.Register(new ImageControl[11]
			{
				new RibbonItemSmallAdapter(AppCommands.Information.Button),
				new RibbonItemSmallAdapter(AppCommands.Back.Button),
				new RibbonItemSmallAdapter(AppCommands.Forward.Button),
				new RibbonItemSmallAdapter(AppCommands.Reload.Button),
				new RibbonItemSmallAdapter(AppCommands.SaveProject.Button),
				new RibbonItemSmallAdapter(AppCommands.SyncProjectSmall.Button),
				new RibbonItemSmallAdapter(AppCommands.ShowTooltipSmall.ToggleButton),
				new RibbonItemSmallAdapter(AppCommands.ToggleFullscreenSmall.ToggleButton),
				new RibbonItemSmallAdapter(AppCommands.Theme.Button),
				new RibbonItemSmallAdapter(AppCommands.ProjectMemberEditSmall.Button),
				new RibbonItemSmallAdapter(AppCommands.ContactWay.Button)
			});
			MultiLedgerViewer.ShowFillToTable(!(value is AppEditionGeneral));
			SyncTwinkle = new RibbonFlickerProxy(AppCommands.SyncProjectSmall.Button);
			SyncTwinkle.UpdateEmptyImage(Resources.Empty16);
			SyncTwinkle.SetTimer(SecondTrigger.Trigger);
			View.Controls.Add(_currentEdition.Ribbon);
			EmptyView.SetQQ();
		}
	}

	public DocumentEditor CurrentDocumentEditor
	{
		get
		{
			return _currentDocumentEditor;
		}
		set
		{
			if (_currentDocumentEditor != null)
			{
				pnlContent.Controls.Remove(_currentDocumentEditor.View);
			}
			_currentDocumentEditor = value;
			if (value != null)
			{
				pnlContent.Controls.Add(value.View);
			}
		}
	}

	public Dictionary<Leqisoft.Model.Document, DocumentEditor> DocumentEditors { get; } = new Dictionary<Leqisoft.Model.Document, DocumentEditor>();


	public ImageEditor ImageEditor { get; private set; }

	public PdfViewer PdfViewer { get; } = new PdfViewer();


	public Leqisoft.Model.Project CurrentProject
	{
		get
		{
			return Leqisoft.Model.Project.Current;
		}
		set
		{
			if (Leqisoft.Model.Project.Current != null)
			{
				TableEditor.Table = null;
			}
			Leqisoft.Model.Project.Current = value;
			value.FormulaMapDirty = true;
			this.CurrentProjectChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public ProjectHierarchy ProjectHierarchy { get; } = new ProjectHierarchy();


	public Leqisoft.Model.Table CurrentTable => TableEditor.Table;

	public ReportPreview Preview
	{
		get
		{
			ReportPreview value = _lazyPreview.Value;
			TreeNodeBase selectedNode = ProjectHierarchy.SelectedNode;
			if (!(selectedNode is TreeTableNode treeTableNode))
			{
				if (selectedNode is TreeImageNode treeImageNode)
				{
					value.Image = value.Image ?? treeImageNode.Image;
				}
			}
			else
			{
				value.Table = value.Table ?? treeTableNode.Table;
			}
			return value;
		}
	}

	public ReportExportToExcel ToExcel => _lazyToExcel.Value;

	public C1SuperLabel SelectionStats => StatusBar.SelectionStatsLabel;

	public ThemeForm ThemeEditor { get; private set; }

	public bool ShowHelperTooltip { get; set; } = true;


	public Dictionary<TreeTableNode, TableValidationInfo> TableValidationResults { get; } = new Dictionary<TreeTableNode, TableValidationInfo>();


	public Dictionary<TreeNodeBase, int> NodeValidationResults { get; } = new Dictionary<TreeNodeBase, int>();


	public Dictionary<Leqisoft.Model.TreeGroup, int> GroupValidationResults { get; } = new Dictionary<Leqisoft.Model.TreeGroup, int>();


	public Dictionary<Id64, List<ValidationResult>> ValidationResultCache { get; } = new Dictionary<Id64, List<ValidationResult>>();


	public Dictionary<Id64, List<ValidationResult>> DocumentValidationResultCache { get; } = new Dictionary<Id64, List<ValidationResult>>();


	public RibbonFlickerProxy SyncTwinkle { get; private set; }

	public TabMember TabMember { get; set; }

	public C1SplitterPanel MainPanel => pnlMain;

	public C1SplitterPanel ContentPanel => pnlContent;

	public C1SplitterPanel ProjectHierarchyTreePandel => pnlTree;

	private bool IsPlatformEnableLedger
	{
		get
		{
			switch (Program.ClientPlatformType)
			{
			case PlatformType.AuditPlatform:
			case PlatformType.EnterpriseReportPlatform:
				return true;
			case PlatformType.EnterpriseManagerPlatform:
			case PlatformType.TableDevelopPlatform:
			case PlatformType.ProductionCostAccountingSystem:
			case PlatformType.ContractLedgerManagementSystem:
			case PlatformType.RDExpenseLedgerSystem:
			case PlatformType.SalesOrderManagementSystem:
			case PlatformType.PSIManagementSystem:
			case PlatformType.ProjectLedgerManagementSystem:
				return false;
			case PlatformType.Custom:
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("enable_ledger", defaultValue: false);
			default:
				return false;
			}
		}
	}

	public bool IsFormActived { get; protected set; }

	public bool IsInSyncingProject
	{
		get
		{
			lock (_syncLock)
			{
				return _isInSyncingProject;
			}
		}
		set
		{
			lock (_syncLock)
			{
				_isInSyncingProject = value;
			}
		}
	}

	public C1SplitterPanel NavigationPanel => pnlNav;

	public AppState State { get; } = new AppState();


	public event EventHandler CurrentProjectChanged;

	public TableEditor GetCreatedTableEditor()
	{
		if (!_lazyTableEditor.IsValueCreated)
		{
			return null;
		}
		return _lazyTableEditor.Value;
	}

	protected static void LedgerCloseEventProcessHandle(object sender, LedgerEventArgs e)
	{
		Ledger ledger = e.Viewer.Ledger;
		if (ledger != null)
		{
			LedgerVirtualTableUtils.ClearLederVirtualTable(e.Viewer.Ledger);
		}
	}

	protected static void LedgerDataChangeEventProcessHandle(object sender, LedgerEventArgs e)
	{
		Ledger ledger = e.Viewer.Ledger;
		if (ledger != null)
		{
			LedgerVirtualTableUtils.ClearLederVirtualTable(e.Viewer.Ledger);
		}
	}

	public MainForm()
	{
		_lazyPreview = new Lazy<ReportPreview>(delegate
		{
			ReportPreview reportPreview = new ReportPreview();
			pnlContent.Controls.Add(reportPreview.View);
			return reportPreview;
		});
		_lazyToExcel = new Lazy<ReportExportToExcel>(() => new ReportExportToExcel());
		_lazyTableEditor = new Lazy<TableEditor>(delegate
		{
			TableEditor tableEditor = new TableEditor(this);
			View.SuspendPainting();
			pnlContent.Controls.Add(tableEditor.View);
			Theme.SetCurrentTree(tableEditor.View);
			View.ResumePainting();
			return tableEditor;
		});
		_lazyTicketEditor = new Lazy<TicketInputEditor2>(delegate
		{
			TicketInputEditor2 ticketInputEditor = new TicketInputEditor2(this);
			pnlContent.Controls.Add(ticketInputEditor.View);
			Theme.SetCurrentTree(ticketInputEditor.View);
			ticketInputEditor.SetTheme();
			return ticketInputEditor;
		});
		_lazyTicketPrinter = new Lazy<TicketPrinter>(delegate
		{
			TicketPrinter ticketPrinter = new TicketPrinter();
			pnlContent.Controls.Add(ticketPrinter.View);
			return ticketPrinter;
		});
		View = new C1RibbonForm
		{
			WindowState = FormWindowState.Maximized,
			Font = new Font("微软雅黑", Control.DefaultFont.Size),
			Icon = Resources.icon,
			VisualStyle = C1.Win.C1Ribbon.VisualStyle.Custom,
			Size = new Size(1024, 768)
		};
		View.Shown += View_Shown;
		View.FormClosing += View_FormClosing;
		View.FormClosed += View_FormClosed;
		View.KeyDown += View_KeyDown;
		View.Activated += View_Activated;
		View.Deactivate += View_Deactivate;
		View.KeyPreview = true;
		ctnRibbonFormClientArea = new C1SplitContainer
		{
			Dock = DockStyle.Fill,
			SplitterWidth = 0,
			BackColor = Color.Transparent
		};
		pnlCtnAllAboveSpace = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			KeepRelativeSize = false,
			Resizable = false,
			Height = 2,
			BackColor = Color.White
		};
		pnlCtnAllParent = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			SizeRatio = 100.0,
			BackColor = Color.Transparent,
			KeepRelativeSize = true
		};
		ctnRibbonFormClientArea.Panels.Add(pnlCtnAllAboveSpace);
		ctnRibbonFormClientArea.Panels.Add(pnlCtnAllParent);
		ctnAll = new C1SplitContainer
		{
			Dock = DockStyle.Fill,
			SplitterWidth = 2,
			SplitterColor = Color.Red,
			BackColor = Color.Transparent,
			BorderWidth = 0
		};
		pnlLedger = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			MinWidth = 0,
			SizeRatio = 0.0,
			BackColor = Color.Transparent,
			Name = "pnlLedger",
			Visible = IsPlatformEnableLedger
		};
		pnlLedger.Controls.Add(MultiLedgerViewer.View);
		MultiLedgerViewer.View.Dock = DockStyle.Fill;
		ctnMain = new C1SplitContainer
		{
			Dock = DockStyle.Fill,
			SplitterWidth = 2,
			BackColor = Color.Transparent,
			BorderWidth = 0
		};
		pnlHelp = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Right,
			Resizable = true,
			Collapsible = false,
			Collapsed = false,
			KeepRelativeSize = false,
			MinWidth = 0
		};
		if (Browser != null && Browser.Control != null)
		{
			pnlHelp.Controls.Add(Browser.Control);
			Browser.OpenPage += Browser_OpenPage;
			Browser.Close += Browser_Close;
		}
		pnlFormula = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Resizable = false,
			Height = 31,
			KeepRelativeSize = false,
			Collapsible = false,
			MinHeight = 31
		};
		pnlFormula.Controls.Add(FormulaEditor.View);
		pnlFormula.Paint += PnlFormula_Paint;
		pnlContent = new C1SplitterPanel
		{
			DoubleBuffered = true,
			BackColor = Color.Transparent
		};
		pnlMain = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			MinWidth = 0,
			BackColor = Color.Transparent
		};
		pnlContent.Controls.Add(EmptyView.View);
		pnlContent.Controls.Add(PdfViewer.View);
		if (TicketDesignEditor.View is Control viewControl) pnlContent.Controls.Add(viewControl);
		EmptyView.View.BringToFront();
		Initialize();
		MemberManager.GetInstance().ProjectSynced += MainForm_ProjectSynced;
		MemberManager.GetInstance().ProjectMemberChanged += MainForm_ProjectMemberChanged;
		MemberManager.GetInstance().MemberInfoChanged += MainForm_MemberInfoChanged;
		MemberManager.GetInstance().PushTreeNode += MainForm_PushTreeNode;
		MemberManager.GetInstance().RecieveFileMesssage += MainForm_RecieveFileMesssage;
		MemberManager.GetInstance().MessageArrived += MainForm_MessageArrived;
		try { _soundPlayer.Stream = Resources.NotifySound1; } catch { }
		MultiLedgerViewer.IsShowToolBar = _showSideToolbar;
		MultiLedgerViewer.AfterOpenLedger += MultiLedgerViewer_AfterOpenLedger;
		MultiLedgerViewer.LedgerSelectionChanged += MultiLedgerViewer_SelectionNumberChanged;
		MultiLedgerViewer.AfterCollect += MultiLedgerViewer_AfterCollect;
		MultiLedgerViewer.AfterShare += MultiLedgerViewer_AfterShare;
		MultiLedgerViewer.LedgerNotFound += MultiLedgerViewer_LedgerNotFound;
		MultiLedgerViewer.GetOnlineMemberContextMenu = GetOnlineMemberContextMenu;
		MultiLedgerViewer.AfterCloseLedger += LedgerCloseEventProcessHandle;
		MultiLedgerViewer.AfterLedgerDataChanged += LedgerDataChangeEventProcessHandle;
		MemberManager.GetInstance().AfterSendSection += MultiLedgerViewer.LedgerDefaultPanel.LedgerDefaultPanel_AfterSendSection;
		MemberManager.GetInstance().AfterSendComplete += MultiLedgerViewer.LedgerDefaultPanel.LedgerDefaultPanel_AfterSendComplete;
		MemberManager.GetInstance().AfterSendCancel += MultiLedgerViewer.LedgerDefaultPanel.LedgerDefaultPanel_AfterSendCancel;
		MemberManager.GetInstance().AfterRecieveCancel += MultiLedgerViewer.LedgerDefaultPanel.LedgerDefaultPanel_AfterRecieveCancel;
	}

	private void View_Deactivate(object sender, EventArgs e)
	{
		IsFormActived = false;
	}

	private void View_Activated(object sender, EventArgs e)
	{
		try
		{
			IsFormActived = true;
			if (CurrentView == MainFormView.TicketInput)
			{
				TicketInputEditor.Invalidate();
			}
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private void PnlFormula_Paint(object sender, PaintEventArgs e)
	{
		using Pen pen = new Pen(Color.FromArgb(200, 200, 200));
		e.Graphics.DrawLine(pen, 0, pnlFormula.Height - 1, pnlFormula.Width, pnlFormula.Height - 1);
	}

	private void Browser_Close(object sender, EventArgs e)
	{
		pnlHelp.Hide();
	}

	private void Browser_OpenPage(object sender, OpenPageEventArgs e)
	{
		if (!FormHelpCenter.IsOpen)
		{
			FormHelpCenter formHelpCenter = new FormHelpCenter();
			formHelpCenter.Text = "帮助中心";
			formHelpCenter.Url = e.url;
			formHelpCenter.RootPage = HelpCenterUtil.GetHelpCenterHomePage();
			formHelpCenter.Show();
		}
	}

	private void MainForm_MessageArrived(object sender, Tuple<string, string, NotifyMessage> e)
	{
		MemberManager instance = MemberManager.GetInstance();
		Member member = instance.GetMember(e.Item1);
		if (member != null)
		{
			Bullet bullet = new Bullet(member.Name + "发来消息：" + e.Item3.Text, member.Image);
			if (ChatManager.ChatForm.Value.IsBulletEnabled)
			{
				ChatManager.BulletLauncher?.Launch(bullet);
			}
			if (ChatManager.ChatForm.Value.IsSoundEnabled)
			{
				_soundPlayer.Play();
			}
		}
	}

	private void MainForm_RecieveFileMesssage(object sender, Tuple<Member, NotifyMessage> e)
	{
		if (e.Item1 != null && e.Item2 != null)
		{
			e.Item1.UnhandleActionMessage.Enqueue(new SendFileMessage(e.Item1.Id, e.Item1.Name, e.Item2));
			ChatManager.BulletLauncher?.Launch(new Bullet(e.Item2.Text));
		}
	}

	private void MainForm_PushTreeNode(object sender, Tuple<long, long, string, string, string> e)
	{
		string userId = e.Item1.ToString();
		long nodeId = e.Item2;
		string item = e.Item3;
		string item2 = e.Item4;
		string item3 = e.Item5;
		TreeNodeBase treeNode = ProjectHierarchy.FindNode(null)?.UserData as TreeNodeBase;
		MemberManager instance = MemberManager.GetInstance();
		Member member = instance.GetMember(userId);
		if (member != null)
		{
			PushNodeMessage item4 = new PushNodeMessage(item2, nodeId, item, treeNode);
			member.UnhandleActionMessage.Enqueue(item4);
			ChatManager.BulletLauncher?.Launch(new Bullet(item3));
		}
	}

	private void MainForm_MemberInfoChanged(object sender, string e)
	{
		if (!(e == CurrentProject?.Id.ToString()))
		{
			return;
		}
		MemberManager instance = MemberManager.GetInstance();
		Group group = instance.GetGroup(SignalRClient.UserState.ProjectId);
		if (group != null)
		{
			CurrentProject.Users = (from m in @group.Members()
				select m.ToDto()).ToDictionary((Leqisoft.DTO.User u) => u, (Leqisoft.DTO.User u) => u.Role);
		}
	}

	private async void MainForm_ProjectMemberChanged(object sender, string e)
	{
		Guid id = Guid.Parse(e);
		if (!RecentProjects.TryGetValue(id, out var proj))
		{
			return;
		}
		try
		{
			Leqisoft.DTO.Project dto;
			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
			{
				var projects = await Leqisoft.LocalDataStore.StorageRouter.GetProjects();
				dto = projects.FirstOrDefault(p => p.Id == id);
			}
			else
			{
				dto = await WebApiClient.GetProjectDto(id);
			}
			proj.PopulateFieldsFromDto(dto);
			PopulateRecents();
			if (id == CurrentProject.Id)
			{
				View.Text = CurrentProject.Name + " - " + GetAppName();
			}
		}
		catch (HttpRequestException)
		{
		}
	}

	private void MainForm_ProjectSynced(object sender, string e)
	{
		_serverDataChangedProject[e] = true;
		SyncTwinkle.Start();
		if (e == CurrentProject?.Id.ToString())
		{
			HandleSyncMessage(e);
		}
	}

	public async Task<Leqisoft.Model.Project> OpenOrSwitchToProject(Guid id, string willOpenProjectTypeName = null)
	{
		if (CurrentProject != null && RecentProjects.TryGetValue(CurrentProject.Id, out var value))
		{
			value.LastNode = ProjectHierarchy.SelectedNode;
		}
		if (!RecentProjects.TryGetValue(id, out var toOpen))
		{
			try
			{
				if (string.IsNullOrWhiteSpace(willOpenProjectTypeName))
				{
					willOpenProjectTypeName = StringConstBase.Current.Project;
				}
				bool isReturnNull = false;
				ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
				ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
				progressForm.ShowDialog(progressRuntimeData, async delegate
				{
					await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
					progressRuntimeData.UpdateMessage("正在打开" + willOpenProjectTypeName + "...");
					progressRuntimeData.UpdateProgress(20, 100);
					Tuple<int, int> tup = await OpenProjectImpl(id);
					if (tup == null)
					{
						isReturnNull = true;
					}
					else
					{
						progressRuntimeData.UpdateProgress(40, 100);
					Leqisoft.DTO.Project dto;
					if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
					{
						// 本地模式：先查项目，查不到再查模板
						var projects = await Leqisoft.LocalDataStore.StorageRouter.GetProjects();
						dto = projects.FirstOrDefault(p => p.Id == id);
						if (dto == null)
						{
							dto = await Leqisoft.LocalDataStore.StorageRouter.GetTemplateById(id);
						}
					}
					else
					{
						dto = await WebApiClient.GetProjectDto(id);
					}
						progressRuntimeData.UpdateProgress(80, 100);
						toOpen = await OpenProjectDb_DownloadIfNotExist(dto, progressForm);
						if (toOpen == null)
						{
							isReturnNull = true;
							Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开" + StringConstBase.Current.Project + "过程中发生异常，请重新尝试。");
						}
						else
						{
							toOpen.ServerVersionOnOpen = tup.Item2;
							toOpen.SetIdBase(tup.Item1);
							RecentProjects[dto.Id] = toOpen;
						}
					}
				});
				if (isReturnNull)
				{
					return null;
				}
			}
			catch (HttpRequestException ex)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
				return null;
			}
		}
		View.ActiveControl = null;
		CurrentProject = toOpen;
		PopulateRecents();
		FormulaEditor.Context.Project = CurrentProject;
		try
		{
			if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
				await SignalRClient.OpenProject(id.ToString());
		}
		catch (Exception)
		{
		}
		AppCommands.ProjectEdit.Enabled = CurrentProject.IsCurrentUserManager();
		AppCommands.ProjectMemberEditSmall.Enabled = CurrentProject.IsCurrentUserManager();
		AppCommands.ControlFormula.Enabled = CurrentProject.IsCurrentUserManager();
		AppCommands.TicketDesign.Enabled = CurrentProject.IsCurrentUserManager();
		AppCommands.TicketMode.Enabled = CurrentProject.IsCurrentUserManager();
		AppCommands.AccessControl.Enabled = CanAccessControl();
		PopulateProject();
		if (toOpen.LastNode == null)
		{
			SwitchToEmptyView();
		}
		else
		{
			ProjectHierarchy.FindAndSelectNode(toOpen.LastNode);
		}
		if (_firstTimeOpen)
		{
			_firstTimeOpen = false;
			HideFormulaMap();
		}
		else
		{
			HideFormulaMap();
		}
		RefreshProjectsSyncTwinkle();
		return toOpen;
	}

	private async Task<Tuple<int, int>> OpenProjectImpl(Guid projectId)
	{
		try
		{
			return await Leqisoft.LocalDataStore.StorageRouter.OpenProject(projectId);
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			ex.Log("打开项目" + projectId.ToString("D") + "时发生了未预期的异常");
			return null;
		}
	}

	public async Task<Leqisoft.Model.Project> OpenProjectDb_DownloadIfNotExist(Leqisoft.DTO.Project dto, ProgressForm2 progressForm = null)
	{
		if (progressForm == null)
		{
			progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStep("正在准备" + ((dto.Type == ProjectType.Project) ? StringConstBase.Current.Project : "模板") + "数据，请稍候...");
			progressRuntimeData.UpdateProgress(0.6f);
			TaskProgressValueUpdater progressValueUpdater = new TaskProgressValueUpdater(0.6f, 0.4f, progressRuntimeData.UpdateProgress);
			Leqisoft.Model.Project ret = null;
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				ret = await LoadProjectFileImpl(progressValueUpdater);
			});
			return ret;
		}
		progressForm.SetProgressDisplayValueConverter(new ProgressDisplayValueConverter_SmoothByTime(0.05f));
		TaskProgressValueUpdater progressUpdater2 = new TaskProgressValueUpdater(0f, 1f, null);
		return await LoadProjectFileImpl(progressUpdater2);
		async Task<Leqisoft.Model.Project> LoadProjectFileImpl(TaskProgressValueUpdater progressUpdater)
		{
			_ = 2;
			try
			{
				Leqisoft.Model.Project ret2 = new Leqisoft.Model.Project();
				// 本地模式下，如果是模板，从 Data\Templates 目录加载
				string fileName = GetDbPathByGuid(dto.Id);
				if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && dto.Type == ProjectType.Template)
				{
					string templatePath = Leqisoft.LocalDataStore.LocalDataStore.GetTemplateDbPath(dto.Id);
					if (!string.IsNullOrEmpty(templatePath) && File.Exists(templatePath))
					{
						fileName = templatePath;
					}
				}
				if (File.Exists(fileName))
				{
					try
					{
						ret2.Dal = new ProjectDAL(fileName);
						ret2.Load(progressUpdater.UpdateProgress);
					}
					catch (DbException)
					{
						File.Delete(fileName);
						throw;
					}
				}
				else
				{
					try
					{
						TaskProgressValueUpdater taskProgressValueUpdater = new TaskProgressValueUpdater(0f, 0.8f, progressUpdater.UpdateProgress);
						TaskProgressValueUpdater downloadProgressUpdater = new TaskProgressValueUpdater(0.8f, 0.1f, progressUpdater.UpdateProgress);
						TaskProgressValueUpdater openProgressUpdater = new TaskProgressValueUpdater(0.9f, 0.1f, progressUpdater.UpdateProgress);
						ret2.IsNeedSyncDataOnOpen = false;
						if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
						{
							// 本地模式：创建空的项目数据库文件
							string dir = Path.GetDirectoryName(fileName);
							if (!string.IsNullOrEmpty(dir))
							{
								Directory.CreateDirectory(dir);
							}
							ret2.Dal = new ProjectDAL(fileName);
							ret2.Dal.SaveProject(new Leqisoft.DTO.Project
							{
								Id = dto.Id,
								Name = dto.Name,
								ParentId = dto.Id,
								Version = 0,
								Number = "",
								Category = "",
								Note = "",
								CreateTime = DateTime.Now
							});
							ret2.Dal.SaveTreeGroups(new[]
							{
								new Leqisoft.DTO.TreeGroup
								{
									Id = new Leqisoft.DTO.Id64(1),
									Name = "工作底稿",
									Index = 0,
									Status = 0,
									Dirty = 0,
									ServerIndex = 0
								}
							});
							ret2.Load(openProgressUpdater.UpdateProgress);
						}
						else
						{
							Tuple<Stream, int> tup = await WebApiClient.PullProjectDirect(dto.Id, taskProgressValueUpdater.UpdateProgress).ConfigureAwait(continueOnCapturedContext: false);
							string tempFile = Path.GetTempFileName();
							using (Stream stream = tup.Item1)
							{
								using FileStream fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
								byte[] buf = new byte[4096];
								int total = 0;
								int displayPercent = 0;
								int lenRead;
								do
								{
									lenRead = await stream.ReadAsync(buf, 0, 4096);
									await fs.WriteAsync(buf, 0, lenRead);
									total += lenRead;
									int num = (int)((double)total / (double)tup.Item2 * 100.0);
									if (num > displayPercent)
									{
										displayPercent = num;
										downloadProgressUpdater.UpdateProgress(num, 100L);
									}
								}
								while (lenRead > 0);
							}
							using (FileStream stream2 = new FileStream(tempFile, FileMode.Open))
							{
								using FileStream destination = new FileStream(fileName, FileMode.Create);
								using GZipStream gZipStream = new GZipStream(stream2, CompressionMode.Decompress);
								gZipStream.CopyTo(destination);
							}
							File.Delete(tempFile);
							ret2.Dal = new ProjectDAL(fileName);
							ret2.Load(openProgressUpdater.UpdateProgress);
						}
					}
					catch (IOException)
					{
						File.Delete(fileName);
						throw;
					}
					catch (DbException)
					{
						File.Delete(fileName);
						throw;
					}
				}
				ret2.PopulateFieldsFromDto(dto);
				return ret2;
			}
			catch (HttpRequestException ex4)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.InnerException.Message);
				return null;
			}
			catch (IOException ex5)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
				return null;
			}
			catch (DbException ex6)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex6.Message);
				return null;
			}
		}
	}

	public static string GetDbPathByGuid(Guid id)
	{
		return Path.Combine("data", Leqisoft.Model.User.Current.Id.ToString(), $"{id}.db");
	}

	public void OpenTable()
	{
		Leqisoft.Model.Table table = (ProjectHierarchy.SelectedNode as TreeTableNode).Table;
		if (TreeNodeStateCache.Contains(ProjectHierarchy.SelectedNode.Id))
		{
			if (TreeNodeStateCache.Get(ProjectHierarchy.SelectedNode.Id).Kind == TreeNodeCacheKind.TicketInput && table.Ticket.IsCurrentLevelSupported() && !table.Ticket.IsEmpty())
			{
				if (_isPreview)
				{
					SwitchToTicketPrintView(table);
				}
				else
				{
					SwitchToTicketInputView();
				}
				return;
			}
			TableEditor.PopulateTable();
			if (_isPreview)
			{
				SwitchToTablePreviewView();
			}
			else
			{
				SwitchToTableView();
			}
		}
		else if (table.Ticket.IsCurrentLevelSupported() && !table.Ticket.IsEmpty())
		{
			if (_isPreview)
			{
				SwitchToTicketPrintView(table);
			}
			else
			{
				SwitchToTicketInputView();
			}
		}
		else
		{
			TableEditor.PopulateTable();
			if (_isPreview)
			{
				SwitchToTablePreviewView();
			}
			else
			{
				SwitchToTableView();
			}
		}
	}

	public void OpenImage()
	{
		if (_isPreview)
		{
			SwitchToImagePreviewView();
		}
		else
		{
			SwitchToImageView();
		}
	}

	public void OpenDocument()
	{
		if (_isPreview)
		{
			SwitchToDocPreviewView();
		}
		else
		{
			AppCommands.DocumentLock.IsPressed = CurrentDocumentEditor.Document.Locker != 0;
			SwitchToDocumentView();
		}
		CurrentDocumentEditor.LoadDocPrint();
	}

	public void OpenPdf()
	{
		if (_isPreview)
		{
			SwitchToPdfPreviewView();
		}
		else
		{
			SwitchToPdfView();
		}
	}

	public void SwitchToDocumentView()
	{
		HideAllPnlContent();
		CurrentDocumentEditor.View.Show();
		CurrentDocumentEditor.View.BringToFront();
		CurrentDocumentEditor.LeavePreviewMode();
		FormulaEditor.View.Enabled = !CurrentDocumentEditor.IsDocumentLocked();
		CurrentDocumentEditor.Tx_InputPositionChanged(null, EventArgs.Empty);
		SwitchStateTo(MainFormView.Document);
		_isPreview = false;
		FormulaEditor.UpdatePanelColorToView(MainFormView.Document);
		UpdateSplitFormBodyAndRibbionSpaceColorToView(MainFormView.Document);
	}

	public void SwitchToBadDocView()
	{
		ctnMain.SuspendLayout();
		foreach (DocumentEditor value in DocumentEditors.Values)
		{
			if (value._ttpComment != null) value._ttpComment.Hide();
		}
		SwitchToEmptyView();
		ctnMain.ResumeLayout(performLayout: true);
	}

	public void SwitchToEmptyView()
	{
		HideAllPnlContent();
		EmptyView.View.Show();
		EmptyView.View.BringToFront();
		SwitchStateTo(MainFormView.Empty);
	}

	public void SwitchToTableView()
	{
		pnlNav.SuspendDrawing();
		try
		{
			TrySelectRibbonTab(_previousTableTab);
			HideAllPnlContent();
			TableEditor.View.Show();
			TableEditor.View.BringToFront();
			LoadPrintSetup(TableEditor?.Table.PageSetup);
			SwitchStateTo(MainFormView.Table);
			_isPreview = false;
			FormulaEditor.UpdatePanelColorToView(MainFormView.Table);
			UpdateSplitFormBodyAndRibbionSpaceColorToView(MainFormView.Table);
		}
		finally
		{
			pnlNav.ResumeDrawing();
		}
	}

	public bool IsAllowToDesignTicket(Leqisoft.Model.Table table)
	{
		if (!table.TreeNode.HasWritePermission())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您没有表格的编辑权限，因此也不具备设计表单的权限!");
			return false;
		}
		if (!table.TreeNode.HasSchemaPermission())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您没有表格的结构调整权限，因此也不具备设计表单的权限!");
			return false;
		}
		if (table.RowOwnerExclusive || table.RowOwnerLoad)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "增行独占保护模式下，禁止对表单样式进行修改!");
			return false;
		}
		return true;
	}

	public void UpdateSplitFormBodyAndRibbionSpaceColorToView(MainFormView view)
	{
		Color backColor = Color.White;
		switch (view)
		{
		case MainFormView.Table:
			backColor = Color.White;
			break;
		case MainFormView.Document:
			if (CurrentDocumentEditor != null)
			{
				backColor = Color.White; // was: Program.MainForm.CurrentDocumentEditor.GetEmptyAreaBackgroundColor();
			}
			break;
		}
		pnlCtnAllAboveSpace.BackColor = backColor;
	}

	public void SwitchToTicketDesignView()
	{
		if (IsAllowToDesignTicket(TableEditor.Table))
		{
			HideAllPnlContent();
			((System.Windows.Forms.Control)TicketDesignEditor.View).Show();
			((System.Windows.Forms.Control)TicketDesignEditor.View).BringToFront();
			TicketDesignEditor.Table = TableEditor.Table;
			TicketDesignEditor.Populate();
			SwitchStateTo(MainFormView.TicketDesign);
			FormulaEditor.UpdatePanelColorToView(MainFormView.TicketDesign);
			UpdateSplitFormBodyAndRibbionSpaceColorToView(MainFormView.TicketDesign);
		}
	}

	private void TrySelectRibbonTab(AppCommandTab tab)
	{
		if (tab != null && CurrentEdition.Ribbon != null && !CurrentEdition.Ribbon.Minimized)
		{
			tab.Select();
		}
	}

	public void SwitchToTicketInputView()
	{
		pnlNav.SuspendDrawing();
		try
		{
			TrySelectRibbonTab(_previousTicketTab);
			_isPreview = false;
			TicketInputEditor.SuspendPanelResizeEvent();
			HideAllPnlContent();
			TicketInputEditor.SaveGridSelectRangeAndScrollPosition();
			TicketInputEditor.SuspendDrawing();
			TicketInputEditor.View.Show();
			TicketInputEditor.View.BringToFront();
			TicketInputEditor.ResumePanelResizeEvent();
			TicketInputEditor.Table = TableEditor.Table;
			TicketInputEditor.Populate();
			LoadPrintSetup(TicketInputEditor.Ticket.PageSetup);
			SwitchStateTo(MainFormView.TicketInput);
			TicketInputEditor.ResumeDrawing();
			UpdateSplitFormBodyAndRibbionSpaceColorToView(MainFormView.TicketInput);
		}
		catch (TableModelException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			pnlNav.ResumeDrawing();
		}
	}

	public void ShowFormulaMap()
	{
		if (CurrentProject.TreeGroups.Count != 0 && !CurrentProject.GetAllTreeNodes().All((TreeNodeBase n) => n is TreeDirectoryNode))
		{
			SwitchMainView();
			SwitchStateTo(MainFormView.Empty);
			if (CurrentProject.FormulaMapDirty)
			{
				FormulaMap.Draw();
				CurrentProject.FormulaMapDirty = false;
			}
			FormulaMap.View.Show();
			FormulaMap.View.Focus();
		}
	}

	public void HideFormulaMap()
	{
		FormulaMap.View.Hide();
	}

	public void SwitchToPreview()
	{
		SuspendNavPanelVisible();
		try
		{
			switch (State.ViewKind)
			{
			case MainFormView.Table:
				SwitchToTablePreviewView();
				break;
			case MainFormView.Document:
				SwitchToDocPreviewView();
				break;
			case MainFormView.Image:
				SwitchToImagePreviewView();
				break;
			case MainFormView.TicketInput:
				SwitchToTicketPrintView();
				break;
			}
		}
		finally
		{
			ResumeNavPanelVisible();
		}
	}

	public void SwitchToNormalView()
	{
		SuspendNavPanelDrawing();
		SuspendNavPanelVisible();
		try
		{
			switch (State.ViewKind)
			{
			case MainFormView.TablePreview:
				SwitchToTableView();
				TableEditor.PopulateRibbon();
				break;
			case MainFormView.DocumentPreview:
				SwitchToDocumentView();
				break;
			case MainFormView.ImagePreview:
				SwitchToImageView();
				break;
			case MainFormView.TicketPrint:
				SwitchToTicketInputView();
				break;
			}
		}
		finally
		{
			ResumeNavPanelVisible();
			ResumeNavPanelDrawing();
		}
	}

	public void SwitchToTablePreviewView()
	{
		Preview.Table = CurrentTable;
		Preview.CreatePaper();
		HideAllPnlContent();
		Preview.View.Show();
		Preview.View.BringToFront();
		LoadPrintSetup(TableEditor?.Table.PageSetup);
		SwitchStateTo(MainFormView.TablePreview);
		TableEditor._ttpComment.Hide();
		_isPreview = true;
	}

	public void SwitchToImagePreviewView()
	{
		if (ProjectHierarchy.SelectedNode is TreeImageNode treeImageNode)
		{
			Preview.Image = treeImageNode.Image;
			Preview.CreatePaper();
			HideAllPnlContent();
			Preview.View.Show();
			Preview.View.BringToFront();
			Preview.Image = ImageEditor?.Image;
			LoadPrintSetup(ImageEditor?.Image.PageSetup);
			SwitchStateTo(MainFormView.ImagePreview);
			_isPreview = true;
		}
	}

	public void SwitchToDocPreviewView()
	{
		HideAllPnlContent();
		CurrentDocumentEditor.View.Show();
		CurrentDocumentEditor.View.BringToFront();
		CurrentDocumentEditor.EnterPreviewMode();
		SwitchStateTo(MainFormView.DocumentPreview);
		_isPreview = true;
	}

	public void SwitchToImageView()
	{
		HideAllPnlContent();
		ImageEditor.View.Show();
		ImageEditor.View.BringToFront();
		SwitchStateTo(MainFormView.Image);
		LoadPrintSetup(ImageEditor?.Image.PageSetup);
		_isPreview = false;
	}

	public void SwitchToPdfView()
	{
		HideAllPnlContent();
		PdfViewer.View.Show();
		PdfViewer.View.BringToFront();
		SwitchStateTo(MainFormView.Pdf);
		_isPreview = false;
		PdfViewer.AfterBecomeVisible();
	}

	public void SwitchToPdfPreviewView()
	{
		HideAllPnlContent();
		PdfViewer.View.Show();
		PdfViewer.View.BringToFront();
		SwitchStateTo(MainFormView.PdfPreview);
		_isPreview = true;
	}

	public void SwitchToTicketPrintView(Leqisoft.Model.Table table = null)
	{
		HideAllPnlContent();
		_isPreview = true;
		if (table == null)
		{
			TicketPrinter.Ticket = TicketInputEditor.Ticket;
			TicketPrinter.SetVm();
		}
		else
		{
			TicketPrinter.Ticket = table.Ticket;
			if (TicketInputEditor.Table != table || TicketInputEditor.Ticket != table.Ticket)
			{
				TicketInputEditor.Table = table;
				TicketInputEditor.Populate();
				TicketInputEditor.OnEnterView();
			}
			TicketPrinter.SetVm();
		}
		TicketPrinter.View.Show();
		TicketPrinter.View.BringToFront();
		TicketPrinter.Populate();
		SwitchStateTo(MainFormView.TicketPrint);
		LoadPrintSetup(((dynamic)TicketPrinter.Ticket).PageSetup);
	}

	public void ExportExcelDialog()
	{
		string displayValue = CurrentTable.Title.TitleCell.GetDisplayValue();
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			FileName = (string.IsNullOrEmpty(displayValue) ? (CurrentTable.TreeNode.Number + " " + CurrentTable.TreeNode.Name) : displayValue),
			DefaultExt = ".xlsx",
			Filter = "xlsx files(*.xlsx)|*.xlsx"
		};
		DialogResult dialogResult = saveFileDialog.ShowDialog();
		if (dialogResult != DialogResult.OK)
		{
			return;
		}
		try
		{
			if (CurrentTable.Rows.Count > 65535)
			{
				using BigExcelExporter bigExcelExporter = new BigExcelExporter();
				bigExcelExporter.Table = CurrentTable;
				bigExcelExporter.Export(saveFileDialog.FileName);
			}
			else
			{
				ExportExcel(saveFileDialog.FileName);
			}
			ExportTableAttachmentsAsync(CurrentTable, saveFileDialog.FileName).ContinueWith(delegate
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件导出成功");
			});
		}
		catch (IOException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因文件被占用或其他原因，导出失败。");
		}
		catch (Exception ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	public void TableExportPdf()
	{
		if (CurrentTable == null)
		{
			return;
		}
		Preview.Table = CurrentTable;
		Preview.CreatePaper();
		if (Preview.PrintDocument == null)
		{
			return;
		}
		string displayValue = CurrentTable.Title.TitleCell.GetDisplayValue();
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			FileName = (string.IsNullOrEmpty(displayValue) ? (CurrentTable.TreeNode.Number + " " + CurrentTable.TreeNode.Name) : displayValue),
			DefaultExt = ".pdf",
			Filter = "PDF文件(*.pdf)|*.pdf"
		};
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			using FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate);
			Preview.PrintDocument.Export(stream, new PdfExportProvider(), showProgress: true);
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件导出成功");
		}
		catch (IOException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件导出失败，失败原因: " + ex.Message);
		}
		catch (Exception ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件导出失败，失败原因: " + ex2.Message);
		}
	}

	public PageSetupWaterMark GenerateExcelExportWaterMarkSetting()
	{
		PageSetupWaterMark pageSetupWaterMark = null;
		if (SoftwareLicenseManager.IsNeedPrintWaterMark())
		{
			string platformName = Program.MainForm.CurrentEdition.PlatformName;
			string empty = string.Empty;
			empty = ((!SoftwareLicenseManager.IsPayByProject()) ? "非正式版用户" : ((!SoftwareLicenseManager.IsFreeTeam()) ? ("体验" + StringConstBase.Current.Project) : "非正式版用户"));
			PageSetupWaterMark.WaterMarkSetting waterMarkSetting = new PageSetupWaterMark.WaterMarkSetting();
			waterMarkSetting.LeftText = platformName;
			waterMarkSetting.RightText = empty;
			waterMarkSetting.FontName = "微软雅黑";
			waterMarkSetting.Height = 12.0;
			pageSetupWaterMark = new PageSetupWaterMark();
			pageSetupWaterMark.Header = waterMarkSetting;
			pageSetupWaterMark.Footer = waterMarkSetting;
		}
		return pageSetupWaterMark;
	}

	private void ExportExcel(string fullPath)
	{
		PageSetup pageSetup = new PageSetup();
		ToExcel.Table = CurrentTable;
		ToExcel.PageSetup = pageSetup;
		ToExcel.WaterMarkPageSetup = GenerateExcelExportWaterMarkSetting();
		ExcelContex excelContex = new ExcelContex();
		ToExcel.SaveValue(fullPath, excelContex);
		ToExcel.SetFormula(excelContex);
		ToExcel.Save(excelContex);
	}

	protected async Task ExportTableAttachmentsAsync(Leqisoft.Model.Table table, string tableFilePath)
	{
		if (table.CellPropManager.DicCellAttachments.Count == 0)
		{
			return;
		}
		try
		{
			string text = Path.GetFileNameWithoutExtension(tableFilePath) + "的单元格附件";
			char[] invalidPathChars = Path.GetInvalidPathChars();
			foreach (char oldChar in invalidPathChars)
			{
				text = text.Replace(oldChar, '-');
			}
			string finalDirName = Path.Combine(Path.GetDirectoryName(tableFilePath), text);
			while (!Directory.Exists(finalDirName))
			{
				Directory.CreateDirectory(finalDirName);
			}
			foreach (Leqisoft.Model.CellAttachments value in table.CellPropManager.DicCellAttachments.Values)
			{
				if (value.Status == SyncStatus.LocalDeleted || value.Status == SyncStatus.ServerDeleted)
				{
					continue;
				}
				foreach (CellAttachment attachment in value.Attachments)
				{
					Guid fileId = attachment.FileId;
					await table.Project.FileCacheManager.DownloadIfNotExist(fileId);
					string text2 = ((attachment.Name == null) ? string.Empty : attachment.Name);
					char[] invalidPathChars2 = Path.GetInvalidPathChars();
					foreach (char oldChar2 in invalidPathChars2)
					{
						text2 = text2.Replace(oldChar2, '-');
					}
					string extension = Path.GetExtension(text2);
					text2 = Path.GetFileNameWithoutExtension(text2);
					string text3 = Path.Combine(finalDirName, text2) + extension;
					int num = 0;
					while (File.Exists(text3))
					{
						num++;
						text3 = $"{Path.Combine(finalDirName, text2)}({num}){extension}";
						if (num == int.MaxValue)
						{
							break;
						}
					}
					table.Project.FileCacheManager.DuplicateTo(fileId, text3);
				}
			}
		}
		catch (Exception exception)
		{
			exception.Log("导出表格" + tableFilePath + "的附件时发生了未预期的异常");
		}
	}

	public void ExportImageDialog()
	{
		if (ImageEditor.Image == null)
		{
			return;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			FileName = ImageEditor.Image.TreeNode.Number + " " + ImageEditor.Image.TreeNode.Name,
			DefaultExt = ".jpg",
			Filter = "支持的图片格式|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tif;*.tiff|bmp|*.bmp|gif|*.gif|jpg|*.jpg;*.jpeg|png|*.png|tiff|*.tif;*.tiff"
		};
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			ExportImage(saveFileDialog.FileName);
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件导出成功");
		}
		catch (Exception)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因文件被占用或其他原因，导出失败。");
		}
	}

	private void ExportImage(string fullPath)
	{
		ImageEditor.Image.LoadAndReturn();
		ImageEditor.Image.GetGraphicsImage().Save(fullPath);
	}

	private string GetAppName()
	{
		return "AuditAI";
	}

	public void PopulateProject()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
		if (CurrentProject?.Dal == null)
		{
			return;
		}
		CurrentProject.Load();
		View.Text = CurrentProject.Name + " - " + GetAppName();
		ProjectHierarchy.Project = CurrentProject;
		ProjectHierarchy.Populate();
		if (ProjectHierarchy._cutCopyMode == ProjectHierarchy.CutCopyModeEnum.Cut)
		{
			ProjectHierarchy._cutCopyMode = ProjectHierarchy.CutCopyModeEnum.None;
			ClipboardManager.Instance.Clear();
		}
		StatusBar.Populate();
	}

	public void PopulateRecents()
	{
		AppCommandGroups.RecentProjects.Visible = false;
		AppCommandGroups.RecentTemplates.Visible = false;
		AppCommandGroups.RecentProjects.RibbonGroup.Items.Clear();
		AppCommandGroups.RecentTemplates.RibbonGroup.Items.Clear();
		foreach (Leqisoft.Model.Project proj in RecentProjects.Values)
		{
			System.Drawing.Image largeImage;
			RibbonGroup ribbonGroup;
			if (proj.Kind == ProjectType.Project)
			{
				largeImage = ((Leqisoft.Model.Project.Current != proj) ? CurrentEdition.ProjectTileIcon : CurrentEdition.CurrentProjectIcon);
				ribbonGroup = AppCommandGroups.RecentProjects.RibbonGroup;
			}
			else
			{
				largeImage = (proj.SystemBuild ? ((Leqisoft.Model.Project.Current != proj) ? CurrentEdition.SystemTemplateTileIcon : CurrentEdition.CurrentSystemTemplateIcon) : ((Leqisoft.Model.Project.Current != proj) ? CurrentEdition.CustomTemplateTileIcon : CurrentEdition.CurrentCustomTemplateIcon));
				ribbonGroup = AppCommandGroups.RecentTemplates.RibbonGroup;
			}
			RibbonSplitButton ribbonSplitButton = new RibbonSplitButton
			{
				Text = ((proj.Name.Length > 20) ? (proj.Name.Substring(0, 20) + "...") : proj.Name),
				LargeImage = largeImage,
				TextImageRelation = C1.Win.C1Ribbon.TextImageRelation.ImageAboveText
			};
			ribbonSplitButton.Click += async delegate
			{
				if (proj.Id != CurrentProject.Id)
				{
					await OpenOrSwitchToProject(proj.Id);
				}
			};
			RibbonButton ribbonButton = new RibbonButton
			{
				Text = ((proj.Kind == ProjectType.Project) ? ("关闭" + StringConstBase.Current.Project) : "关闭模板")
			};
			ribbonButton.Click += async delegate
			{
				await CloseProject(proj);
			};
			ribbonSplitButton.Items.Add(ribbonButton);
			ribbonGroup.Items.Add(ribbonSplitButton);
		}
		if (AppCommandGroups.RecentProjects.RibbonGroup.Items.Count > 0)
		{
			AppCommandGroups.RecentProjects.Visible = true;
		}
		if (AppCommandGroups.RecentTemplates.RibbonGroup.Items.Count > 0)
		{
			AppCommandGroups.RecentTemplates.Visible = true;
		}
	}

	public async Task CloseProject(Leqisoft.Model.Project p)
	{
		if (RecentProjects.Count == 1)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "不能关闭已打开的仅存的一个" + StringConstBase.Current.Project + "。");
			return;
		}
		string closeProjMsg = Leqisoft.LocalDataStore.StorageRouter.IsLocalMode
			? "是否要保存" + StringConstBase.Current.Project + " " + p.Name + " ？"
			: "是否要保存并同步" + StringConstBase.Current.Project + " " + p.Name + " ？";
		switch (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, closeProjMsg, MessageBoxButtons.YesNoCancel))
		{
		case DialogResult.Cancel:
			return;
		case DialogResult.Yes:
			await SyncProject(p);
			break;
		}
		RecentProjects.Remove(p.Id);
		PopulateRecents();
		if (p == CurrentProject)
		{
			await OpenOrSwitchToProject(RecentProjects.Last().Key);
		}
		TicketNavTreeStatusDataCacher.RemoveTicket(p);
		TableNavTreeStatusDataCacher.RemoveTable(p);
	}

	public void GenerateLedger()
	{
		try
		{
			// 预加载账套生成器所需的 C1 程序集，确保许可证 patch 在组件初始化之前完成
			PreloadC1AssembliesForLedgerImport();

			frmImport frmImport = new frmImport();
			frmImport.FormClosed += delegate
			{
				View.BringToFront();
			};
			frmImport.AfterGenerateSuccess += async delegate(object s1, string e1)
			{
				await OpenLedger(e1, userCache: false);
			};
			frmImport.Show();
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	private void PreloadC1AssembliesForLedgerImport()
	{
		try
		{
			// 账套生成器(frmImport)使用的 C1 程序集列表
			string[] c1Dlls = new[]
			{
				"C1.Win.C1Ribbon.4.dll",
				"C1.Win.C1FlexGrid.4.dll",
				"C1.Win.C1Command.4.dll",
				"C1.Win.C1SplitContainer.4.dll",
				"C1.Win.C1Input.4.dll",
				"C1.Win.C1TileControl.4.dll",
				"C1.Win.C1Report.4.dll",
				"C1.Win.FlexChart.4.dll",
				"C1.C1Excel.4.dll",
				"C1.C1Report.4.dll",
				"C1.Win.4.dll",
			};

			string appDir = AppDomain.CurrentDomain.BaseDirectory;
			foreach (var dll in c1Dlls)
			{
				try
				{
					string path = System.IO.Path.Combine(appDir, dll);
					if (System.IO.File.Exists(path))
					{
						var asm = System.Reflection.Assembly.LoadFrom(path);
						Program.PatchC1LicenseFieldsPublic(asm);
					}
				}
				catch { }
			}
		}
		catch { }
	}

	public void Forward()
	{
		FinishCurrentEditorInputStatus();
		if (stepRecorder.CanForward)
		{
			stepRecorder.Forward();
		}
	}

	public void Back()
	{
		FinishCurrentEditorInputStatus();
		if (stepRecorder.CanBack)
		{
			stepRecorder.Back();
		}
	}

	public void FinishCurrentEditorInputStatus(bool isCancelInput = false)
	{
		if (CurrentView == MainFormView.TicketInput)
		{
			TicketInputEditor.FinishEditorInputStatus(isCancelInput);
		}
		else if (CurrentView == MainFormView.Table)
		{
			TableEditor.FinishEditorInputStatus(isCancelInput);
		}
		ProjectHierarchy.FinishEditorInputStatus(isCancelInput);
	}

	public async void RemoveNodes()
	{
		TreeNodeBase sn = ProjectHierarchy.SelectedNode;
		frmNodeSelector form = new frmNodeSelector();
		form.Project = CurrentProject;
		if (form.ShowRemoveNodes() != DialogResult.OK || Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "您正在执行批量删除操作，会将您选定的文件夹、子文件夹、表格、文档等一并删除，请谨慎操作，确定要执行删除操作吗？", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		ProgressForm<object> progressForm = new ProgressForm<object>(delegate(IProgress<ProgressInfo> iProg)
		{
			iProg.Report(new ProgressInfo
			{
				MainCaption = "正在删除，可能耗时较长，请耐心等待..."
			});
			Application.DoEvents();
			foreach (TreeNodeBase item in form.Selected)
			{
				item.Remove();
			}
			return Task.FromResult<object>(null);
		});
		progressForm.ShowDialog();
		await progressForm.Task;
		ProjectHierarchy.Populate();
		if (sn != null)
		{
			if (sn.Status == SyncStatus.LocalDeleted)
			{
				SwitchToEmptyView();
			}
			else
			{
				ProjectHierarchy.FindAndSelectNode(sn);
			}
		}
	}

	public async Task SaveProjects()
	{
		FinishCurrentEditorInputStatus();
		if (CurrentView == MainFormView.TicketInput)
		{
			TicketInputEditor.SaveRecord();
		}
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime());
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		TaskProgressValueUpdater saveProgressUpdater = new TaskProgressValueUpdater(0f, 1f, progressRuntimeData.UpdateProgress, progressRuntimeData.UpdateMessage, progressRuntimeData.NextStepIfProgressNotZero);
		progressForm.ShowDialog(progressRuntimeData, async delegate
		{
			await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
			foreach (Leqisoft.Model.Project value in RecentProjects.Values)
			{
				progressRuntimeData.NextStepIfProgressNotZero("正在保存项目信息...");
				await SaveProjectImpl(value, saveProgressUpdater);
			}
		});
		if (ProjectHierarchy.SelectedNode is TreeDocumentNode)
		{
			ProjectHierarchy_TreeNodeSelected(ProjectHierarchy, EventArgs.Empty);
		}
		await Task.Delay(1);
	}

	public async Task SaveProject(Leqisoft.Model.Project proj)
	{
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.05f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		TaskProgressValueUpdater saveProgressUpdater = new TaskProgressValueUpdater(0f, 1f, progressRuntimeData.UpdateProgress, progressRuntimeData.UpdateMessage, null);
		progressRuntimeData.NextStep("正在保存" + StringConstBase.Current.Project + "数据...");
		progressForm.ShowDialog(progressRuntimeData, async delegate
		{
			await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
			await SaveProjectImpl(proj, saveProgressUpdater);
		});
		await Task.Delay(1);
	}

	private async Task SaveProjectImpl(Leqisoft.Model.Project proj, TaskProgressValueUpdater progressUpdater = null)
	{
		if (progressUpdater == null)
		{
			progressUpdater = new TaskProgressValueUpdater(0f, 1f, null);
		}
		List<Leqisoft.Model.Table> tables = (from n in proj.GetAllTableNodes()
			select n.Table into t
			where t.NeedSave
			select t).ToList();
		int totalCount = tables.Count + DocumentEditors.Where((KeyValuePair<Leqisoft.Model.Document, DocumentEditor> d) => d.Value.NeedSave).Count() + (from u in proj.GetAllDocumentNodes()
			where u.Document.FromDuplicationButNotSaved
			select u).Count() + 1;
		int hasProcessCount = 0;
		float partPercent = 1f / (float)((totalCount == 0) ? 1 : totalCount);
		for (int j = 0; j < tables.Count; j++)
		{
			progressUpdater.UpdateMessage("正在保存表格 " + tables[j].TreeNode.Name);
			progressUpdater.UpdateProgress(hasProcessCount++, totalCount);
			new TaskProgressValueUpdater(progressUpdater.CurrentProgressValue, partPercent, progressUpdater.UpdateProgress);
			tables[j].TreeNode.IsEntityDirty = true;
			await Task.Delay(10);
			if (tables[j].Project == null || tables[j].Project.Dal == null)
			{
				throw new InvalidOperationException($"无法保存表格 \"{tables[j].TreeNode.Name}\"：项目或数据访问层为空");
			}
			try
			{
				tables[j].Save(null, bypassMapRowIndex: false, progressUpdater);
			}
			catch (Exception ex2)
			{
				Exception inner = ex2;
				while (inner.InnerException != null)
					inner = inner.InnerException;
				throw;
			}
		}
		foreach (var pair in DocumentEditors.Select((KeyValuePair<Leqisoft.Model.Document, DocumentEditor> kv, int i) => new { kv, i }).ToList())
		{
			if (pair.kv.Value.NeedSave)
			{
				progressUpdater.UpdateMessage("正在保存文档 " + pair.kv.Key.TreeNode.Name);
				progressUpdater.UpdateProgress(hasProcessCount++, totalCount);
				pair.kv.Key.TreeNode.IsEntityDirty = true;
				await Task.Delay(10);
				try
				{
					Progress<Tuple<int, int>> iProg = new Progress<Tuple<int, int>>(delegate
					{
					});
					pair.kv.Value.SaveToModel(iProg);
					pair.kv.Value.Document.Save();
				}
				catch (ParagraphTooLongException ex)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
				}
			}
		}
		foreach (TreeDocumentNode allDocumentNode in proj.GetAllDocumentNodes())
		{
			Leqisoft.Model.Document document = allDocumentNode.Document;
			if (document.FromDuplicationButNotSaved)
			{
				progressUpdater.UpdateMessage("正在保存文档 " + document.TreeNode.Name);
				progressUpdater.UpdateProgress(hasProcessCount++, totalCount);
				document.Save();
			}
		}
		List<Leqisoft.Model.Image> list = (from n in proj.GetAllImageNodes()
			select n.Image into i
			where i.NeedSave
			select i).ToList();
		foreach (Leqisoft.Model.Image item in list)
		{
			item.Save();
			item.TreeNode.IsEntityDirty = true;
		}
		List<Leqisoft.Model.Pdf> list2 = (from n in proj.GetAllPdfNodes()
			select n.Pdf.LoadAndReturn()).ToList();
		foreach (Leqisoft.Model.Pdf item2 in list2)
		{
			item2.Save();
		}
		proj.Save();
		progressUpdater.UpdateProgress(1f);
	}

	public async Task SyncProjects()
	{
		// 本地模式下跳过同步
		if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return;
		}
		FinishCurrentEditorInputStatus();
		if (TicketInputEditor != null && CurrentView == MainFormView.TicketInput)
		{
			try
			{
				TicketInputEditor.SaveRecord();
			}
			catch (Exception ex)
			{
				ex.Log();
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表单内容保存失败，请重新保存表单内容, 失败原因描述: " + ex.Message);
				return;
			}
		}
		TreeNodeBase previousNode = ProjectHierarchy.SelectedNode;
		bool anyNodeUpdated = false;
		List<Leqisoft.Model.Project> projectList = new List<Leqisoft.Model.Project>();
		foreach (Leqisoft.Model.Project value in RecentProjects.Values)
		{
			if (SoftwareLicenseManager.IsPayByProjectReachExpireDate(value))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{value.ProjectLicenseDate:yyyy年MM月dd日}到期，无法同步{StringConstBase.Current.Project}，您可致电官方客服电话：400-690-6500，联系购买或续期！");
			}
			else
			{
				projectList.Add(value);
			}
		}
		if (projectList.Count == 0)
		{
			return;
		}
		ProgressForm2 progressFrom = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime());
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		try
		{
			progressRuntimeData.UpdateMessage("准备开始处理...");
			progressFrom.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				foreach (Leqisoft.Model.Project proj in projectList)
				{
					try
					{
						IsInSyncingProject = true;
						bool flag = await SyncProjectImpl(proj, progressRuntimeData, progressFrom);
						if (proj == CurrentProject)
						{
							anyNodeUpdated = flag;
						}
					}
					finally
					{
						IsInSyncingProject = false;
					}
				}
				progressRuntimeData.Finish();
			});
			await Task.Delay(1);
		}
		catch (HttpRequestException ex2)
		{
			if (ex2.InnerException is TimeoutException)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因网络问题，可能数据未完全同步成功，请再同步一次！");
			}
			else
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.InnerException.Message);
			}
		}
		if (anyNodeUpdated)
		{
			PopulateProject();
		}
		if (ProjectHierarchy.FindAndSelectNode(previousNode))
		{
			ProjectHierarchy_TreeNodeSelected(null, null);
		}
		else
		{
			SwitchToEmptyView();
		}
	}

	public async Task SyncCurrentProject()
	{
		TreeNodeBase previousNode = ProjectHierarchy.SelectedNode;
		if (await SyncProject(CurrentProject))
		{
			PopulateProject();
		}
		if (ProjectHierarchy.FindAndSelectNode(previousNode))
		{
			ProjectHierarchy_TreeNodeSelected(null, null);
		}
		else
		{
			SwitchToEmptyView();
		}
	}

	public async Task<bool> SyncProject(Leqisoft.Model.Project proj)
	{
		// 本地模式下跳过同步
		if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return false;
		}
		if (SoftwareLicenseManager.IsPayByProjectReachExpireDate(proj))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{proj.ProjectLicenseDate:yyyy年MM月dd日}到期，无法同步{StringConstBase.Current.Project}，您可致电官方客服电话：400-690-6500，联系购买或续期！");
			return false;
		}
		bool anyNodeUpdated = false;
		ProgressForm2 progressFrom = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime());
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		try
		{
			progressRuntimeData.UpdateMessage("准备开始处理...");
			progressFrom.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				try
				{
					IsInSyncingProject = true;
					anyNodeUpdated = await SyncProjectImpl(proj, progressRuntimeData, progressFrom);
				}
				finally
				{
					IsInSyncingProject = false;
				}
			});
			await Task.Delay(1);
		}
		catch (HttpRequestException ex)
		{
			if (ex.InnerException is TimeoutException)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因网络问题，可能数据未完全同步成功，请再同步一次！");
			}
			else
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
			}
		}
		return anyNodeUpdated;
	}

	public void RefreshProjectsSyncTwinkle()
	{
		if (_serverDataChangedProject.Count == 0)
		{
			SyncTwinkle.Stop();
		}
		else if (!SyncTwinkle.Status())
		{
			SyncTwinkle.Start();
		}
	}

	private async Task<bool> SyncProjectImpl(Leqisoft.Model.Project proj, ProgressRuntimeData progressRuntimeData = null, ProgressForm2 progressForm = null)
	{
		// 本地模式下跳过同步
		if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return false;
		}
		if (SoftwareLicenseManager.IsPayByProjectReachExpireDate(proj))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{proj.ProjectLicenseDate:yyyy年MM月dd日}到期，无法同步{StringConstBase.Current.Project}，您可致电官方客服电话：400-690-6500，联系购买或续期！");
			return true;
		}
		if (progressRuntimeData == null)
		{
			progressRuntimeData = new ProgressRuntimeData();
		}
		TaskProgressValueUpdater progressUpdater = new TaskProgressValueUpdater(0f, 0.2f, progressRuntimeData.UpdateProgress);
		TaskProgressValueUpdater synnDetailDataProgressUpdater = new TaskProgressValueUpdater(0.2f, 0.75f, progressRuntimeData.UpdateProgress);
		new TaskProgressValueUpdater(0.95f, 0.05f, progressRuntimeData.UpdateProgress);
		ProgressDisplayValueConverter_SmoothByTime progressDisplayValueConverter = new ProgressDisplayValueConverter_SmoothByTime(0.05f);
		progressRuntimeData.NextStep("正在保存" + StringConstBase.Current.Project + "数据...");
		progressForm.SetProgressDisplayValueConverter(progressDisplayValueConverter);
		await SaveProjectImpl(proj, progressUpdater);
		List<Tuple<TreeNodeBase, Exception>> le = new List<Tuple<TreeNodeBase, Exception>>();
		bool anyNodeUpdated = (await Syncer.Pull(proj)).Item2;
		PushResult pushProjectResult = await Syncer.Push(proj);
		proj.Save();
		progressRuntimeData.UpdateMessage("准备开始同步数据...");
		IEnumerable<TreeTableNode> tableNodes = proj.GetAllTableNodes();
		Dictionary<Id64, int> tableVersions = (await Syncer.QueryVersion(proj.Id, tableNodes)).ToDictionary((Tuple<Id64, int> tup) => tup.Item1, (Tuple<Id64, int> tup) => tup.Item2);
		List<Leqisoft.Model.Table> tables = (from n in tableNodes
			where n.IsEntityDirty || tableVersions[n.Id] > n.Version
			select n.Table into t
			where !t.IsCorrupted
			select t).ToList();
		IEnumerable<TreeDocumentNode> docNodes = proj.GetAllDocumentNodes();
		Dictionary<Id64, int> docVersions = (await Syncer.QueryVersion(proj.Id, docNodes)).ToDictionary((Tuple<Id64, int> tup) => tup.Item1, (Tuple<Id64, int> tup) => tup.Item2);
		List<Leqisoft.Model.Document> documents = (from n in docNodes
			where n.IsEntityDirty || docVersions[n.Id] > n.Version
			select n.Document).ToList();
		IEnumerable<TreeImageNode> imageNodes = proj.GetAllImageNodes();
		Dictionary<Id64, int> imageVersions = (await Syncer.QueryVersion(proj.Id, imageNodes)).ToDictionary((Tuple<Id64, int> tup) => tup.Item1, (Tuple<Id64, int> tup) => tup.Item2);
		List<Leqisoft.Model.Image> images = (from n in imageNodes
			where n.IsEntityDirty || imageVersions[n.Id] > n.Version
			select n.Image).ToList();
		IEnumerable<TreePdfNode> pdfNodes = proj.GetAllPdfNodes();
		Dictionary<Id64, int> pdfVersions = (await Syncer.QueryVersion(proj.Id, pdfNodes)).ToDictionary((Tuple<Id64, int> tup) => tup.Item1, (Tuple<Id64, int> tup) => tup.Item2);
		List<Leqisoft.Model.Pdf> pdfs = (from n in pdfNodes
			where n.IsEntityDirty || pdfVersions[n.Id] > n.Version
			select n.Pdf).ToList();
		int syncTotalCount = tables.Count + documents.Count + images.Count + pdfs.Count;
		int hasProcessCount = 0;
		TaskProgressValueUpdater syncProgressValueRefresher = new TaskProgressValueUpdater(0f, 1f, synnDetailDataProgressUpdater.UpdateProgress);
		bool anyEntityPushed = false;
		for (int l = 0; l < tables.Count; l++)
		{
			Leqisoft.Model.Table table2 = tables[l];
			progressRuntimeData.UpdateMessage("正在同步表格 " + table2.TreeNode.Name);
			int num = hasProcessCount + 1;
			hasProcessCount = num;
			syncProgressValueRefresher.UpdateProgress(num, syncTotalCount);
			Task<Leqisoft.Model.Table> task = GetTableTask(table2);
			ContinueWithTable(await task);
		}
		for (int l = 0; l < documents.Count; l++)
		{
			Leqisoft.Model.Document doc = documents[l];
			progressRuntimeData.UpdateMessage("正在同步文档 " + doc.TreeNode.Name);
			int num = hasProcessCount + 1;
			hasProcessCount = num;
			syncProgressValueRefresher.UpdateProgress(num, syncTotalCount);
			if (docVersions[doc.Id] > doc.Version)
			{
				try
				{
					await Syncer.Pull(doc);
					CloseDocument(doc);
				}
				catch (Exception ex)
				{
					le.Add(Tuple.Create((TreeNodeBase)doc.TreeNode, ex));
					ex.Log("文档同步时Pull操作失败");
				}
			}
			if (doc.TreeNode.IsEntityDirty)
			{
				doc.LoadAndReturn();
				try
				{
					await Syncer.Push(doc);
					anyEntityPushed = true;
				}
				catch (Exception ex2)
				{
					le.Add(Tuple.Create((TreeNodeBase)doc.TreeNode, ex2));
					ex2.Log("文档同步时Push操作失败");
				}
			}
			if (doc._isLoaded)
			{
				doc.Save();
			}
		}
		for (int l = 0; l < images.Count; l++)
		{
			Leqisoft.Model.Image image = images[l];
			progressRuntimeData.UpdateMessage("正在同步图片 " + image.TreeNode.Name);
			int num = hasProcessCount + 1;
			hasProcessCount = num;
			syncProgressValueRefresher.UpdateProgress(num, syncTotalCount);
			if (imageVersions[image.Id] > image.Version)
			{
				try
				{
					await Syncer.Pull(image);
				}
				catch (Exception item)
				{
					le.Add(Tuple.Create((TreeNodeBase)image.TreeNode, item));
				}
			}
			if (image.TreeNode.IsEntityDirty)
			{
				image.LoadAndReturn();
				try
				{
					await Syncer.Push(image);
					anyEntityPushed = true;
				}
				catch (FileNotFoundException)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "要上传的图片文件已在本地被删除，无法上传图片。图片节点 " + image.TreeNode.Name + " 已经无效，请删除此节点。");
				}
				catch (Exception item2)
				{
					le.Add(Tuple.Create((TreeNodeBase)image.TreeNode, item2));
				}
			}
			if (image._isLoaded)
			{
				image.Save();
			}
		}
		for (int l = 0; l < pdfs.Count; l++)
		{
			Leqisoft.Model.Pdf pdf = pdfs[l];
			progressRuntimeData.UpdateMessage("正在同步PDF " + pdf.TreeNode.Name);
			int num = hasProcessCount + 1;
			hasProcessCount = num;
			syncProgressValueRefresher.UpdateProgress(num, syncTotalCount);
			if (pdfVersions[pdf.Id] > pdf.Version)
			{
				try
				{
					await Syncer.Pull(pdf);
				}
				catch (Exception item3)
				{
					le.Add(Tuple.Create((TreeNodeBase)pdf.TreeNode, item3));
				}
			}
			if (pdf.TreeNode.IsEntityDirty)
			{
				pdf.LoadAndReturn();
				try
				{
					await Syncer.Push(pdf);
					anyEntityPushed = true;
				}
				catch (FileNotFoundException ex4)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
				}
				catch (Exception item4)
				{
					le.Add(Tuple.Create((TreeNodeBase)pdf.TreeNode, item4));
				}
			}
			if (pdf._isLoaded)
			{
				pdf.Save();
			}
		}
		if (pushProjectResult == PushResult.NoContent && anyEntityPushed)
		{
			await Syncer.UpdateDataVersion(proj);
		}
		proj.Save();
		if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && (pushProjectResult != PushResult.NoContent || anyEntityPushed))
		{
			await SignalRClient.SyncProject(proj.Id.ToString());
		}
		if (le.Any())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, string.Concat("同步过程中出现异常，可能数据未完全同步成功，请再同步一次！异常详细信息：", string.Concat(le.Select((Tuple<TreeNodeBase, Exception> tup) => "\n同步\"" + tup.Item1.Name + "\"时出现异常：" + tup.Item2.Message))));
		}
		_serverDataChangedProject.Remove(proj.Id.ToString());
		return anyNodeUpdated;
		static void ContinueWithTable(Leqisoft.Model.Table table)
		{
			if (table == null || !table._loaded)
			{
				return;
			}
			try
			{
				table.Save(null, bypassMapRowIndex: true);
				table._loaded = false;
			}
			catch (TableModelException ex5)
			{
				table._loaded = false;
				ex5.Log(table.GetDebugInfo());
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
			}
		}
		async Task<Leqisoft.Model.Table> GetTableTask(Leqisoft.Model.Table table)
		{
			table._loaded = false;
			table.LoadAndReturn(bypassRowOwnerLoad: true);
			if (tableVersions[table.Id] > table.Version)
			{
				try
				{
					await Syncer.Pull(table);
				}
				catch (Exception ex6)
				{
					le.Add(Tuple.Create((TreeNodeBase)table.TreeNode, ex6));
					ex6.Log("表格同步时Pull操作失败");
					return null;
				}
			}
			if (table.TreeNode.IsEntityDirty)
			{
				try
				{
					await Syncer.Push(table);
					anyEntityPushed = true;
				}
				catch (Exception ex7)
				{
					le.Add(Tuple.Create((TreeNodeBase)table.TreeNode, ex7));
					ex7.Log("表格同步时Push操作失败");
					return null;
				}
			}
			return table;
		}
	}

	public void ManageProjects()
	{
		FormProjectManage formProjectManage = new FormProjectManage();
		formProjectManage.ShowDialog();
		_ = 1;
	}

	public void ShowSettings()
	{
		FormUserSetting formUserSetting = new FormUserSetting();
		if (formUserSetting.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		foreach (KeyValuePair<string, LedgerViewer> item in OpenedLedgerViewerDic)
		{
			item.Value.LoadSetting(UserSet.Config.BooksStyle);
		}
		ApplyConfig();
	}

	public void ShowHelpCenter()
	{
		HelpCenterUtil.OpenHelpCenterHomePage();
	}

	public void AboutForm()
	{
		PolicyForm policyForm = new PolicyForm();
		policyForm.SetTitle(ApplicationNameManager.GetApplicationName(Program.ClientPlatformType));
		policyForm.ShowDialog();
	}

	public void EditReferences(string focus = null)
	{
		ReferenceEditor referenceEditor = new ReferenceEditor();
		referenceEditor.ShowEdit(focus);
	}

	public void SelectTheme()
	{
		ThemeEditor.ShowForm();
	}

	public void ShowContactWayForm()
	{
		Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Asterisk, "尊敬的用户：\r\n购买咨询、寻求支持以及提出改进建议，欢迎您致电官方客服电话：400-690-6500。", MessageBoxButtons.OK, "联系方式");
	}

	public void LoadPrintSetup(PageSetup pageSetup)
	{
		if (pageSetup != null)
		{
			AppCommands.Paper.SelectPaper(pageSetup.PaperKind);
			AppCommands.PaperDirection.SelectPaperDirection(pageSetup.Direction);
			AppCommands.WidthScale.Value = (decimal)pageSetup.HorizontalZoom;
			AppCommands.HeightScale.Value = (decimal)pageSetup.VerticalZoom;
			AppCommands.MarginTop.Value = (decimal)pageSetup.TopMargin;
			AppCommands.MarginBottom.Value = (decimal)pageSetup.BottomMargin;
			AppCommands.MarginLeft.Value = (decimal)pageSetup.LeftMargin;
			AppCommands.MarginRight.Value = (decimal)pageSetup.RightMargin;
			AppCommands.HeaderMargin.Enabled = true;
			AppCommands.HeaderMargin.Value = (decimal)pageSetup.HeaderMargin;
			AppCommands.FooterMargin.Enabled = true;
			AppCommands.FooterMargin.Value = (decimal)pageSetup.FooterMargin;
			RtfHelper rtfHelper = new RtfHelper();
			AppCommands.HeaderLeft.Text = rtfHelper.Load(pageSetup.PageHeader.LeftValue).GetPlainText();
			AppCommands.HeaderCenter.Text = rtfHelper.Load(pageSetup.PageHeader.CenterValue).GetPlainText();
			AppCommands.HeaderRight.Text = rtfHelper.Load(pageSetup.PageHeader.RightValue).GetPlainText();
			AppCommands.FooterLeft.Text = rtfHelper.Load(pageSetup.PageFooter.LeftValue).GetPlainText();
			AppCommands.FooterCenter.Text = rtfHelper.Load(pageSetup.PageFooter.CenterValue).GetPlainText();
			AppCommands.FooterRight.Text = rtfHelper.Load(pageSetup.PageFooter.RightValue).GetPlainText();
			AppCommands.FixedColumns.Value = pageSetup.FixedPrintColsNum;
			AppCommands.FootBorder.IsChecked = pageSetup.HasNoteBorder;
			AppCommands.Monochrome.IsChecked = pageSetup.OneColor;
			AppCommands.ScalePageWidth.IsChecked = pageSetup.FitPageWidth;
			AppCommands.ScalePageHeight.IsChecked = pageSetup.FitPageHeight;
			AppCommands.StartPage.Value = pageSetup.StartPageNo;
		}
	}

	public async Task TicketCollectSet(TicketCollectFillTable ticket)
	{
		try
		{
			if (ticket.IsLocked)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格已锁定，不能执行采账填充");
				return;
			}
			if (IsLedgerEmpty())
			{
				ShowOpenLedgerTip();
				return;
			}
			if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.TableCollector.Version == 0)
			{
				try
				{
					await DictionarySync.CheckTableCollectVersionAndUpdate();
				}
				catch (WebException)
				{
					if (DictionarySync.TableCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因网络问题，字典更新失败！");
					}
				}
				catch (TimeoutException)
				{
					if (DictionarySync.TableCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
					}
				}
				catch (Exception ex3)
				{
					if (DictionarySync.TableCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex3.Message + ",请重试");
					}
				}
			}
			if (ticket == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开表格");
				return;
			}
			frmTableCollect2 frmTableCollect = new frmTableCollect2(CurrentLedgerViewer.Ledger, ticket, View);
			frmTableCollect.LoadCollectSetting(ticket.CollectSource);
			frmTableCollect.Text = "表单采账填充";
			if (frmTableCollect.ShowDialog() == DialogResult.OK)
			{
				TableCollectorAbstract collector = frmTableCollect.Collector;
				collector.Setting.FillTargetType = CollectFillTargetType.Ticket;
				string cs = collector?.Serialize();
				ticket.UpdateCollectSource(cs);
				TicketInputEditor.FillCollectResult(frmTableCollect.Result, ticket);
			}
		}
		catch (InvalidAuditYearException ex4)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
		}
		catch (InvalidCollectSettingException ex5)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex7)
		{
			ex7.Log("打开采账设置窗口时出现异常");
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex7.Message);
		}
	}

	public async Task TableCollectSet()
	{
		_ = 1;
		try
		{
			if (TableEditor.IsTableLocked)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格已锁定，不能执行采账设置");
				return;
			}
			if (TableEditor.Table != null && TableEditor.Table.TreeNode != null)
			{
				if (!TableEditor.Table.TreeNode.HasReadPermission())
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表格没有查看权限，不能执行采账设置");
					return;
				}
				if (!TableEditor.Table.TreeNode.HasWritePermission())
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表格没有编辑权限，不能执行采账设置");
					return;
				}
			}
			if (IsLedgerEmpty())
			{
				ShowOpenLedgerTip();
				return;
			}
			if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.TableCollector.Version == 0)
			{
				try
				{
					await DictionarySync.CheckTableCollectVersionAndUpdate();
				}
				catch (WebException)
				{
					if (DictionarySync.TableCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因网络问题，字典更新失败！");
					}
				}
				catch (TimeoutException)
				{
					if (DictionarySync.TableCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
					}
				}
				catch (Exception ex3)
				{
					if (DictionarySync.TableCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex3.Message + ",请重试");
					}
				}
			}
			if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.CellCollector.Version == 0)
			{
				try
				{
					await DictionarySync.CheckCellCollectDicVersionAndUpdate();
				}
				catch (WebException)
				{
					if (DictionarySync.CellCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因网络问题，字典更新失败！");
					}
				}
				catch (TimeoutException)
				{
					if (DictionarySync.CellCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
					}
				}
				catch (Exception ex6)
				{
					if (DictionarySync.CellCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex6.Message + ",请重试");
					}
				}
			}
			Leqisoft.Model.Table table = TableEditor.Table;
			if (table == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开表格");
				return;
			}
			frmTableCollect2 frmTableCollect = new frmTableCollect2(CurrentLedgerViewer.Ledger, table, View);
			frmTableCollect.LoadCollectSetting(table.CollectSource);
			if (frmTableCollect.ShowDialog() == DialogResult.OK)
			{
				string cs = frmTableCollect.Collector?.Serialize();
				table.UpdateCollectSource(cs);
				FillEditorTable(frmTableCollect.Result);
			}
			TableEditor.PopulateToolbar();
		}
		catch (InvalidAuditYearException ex7)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex7.Message);
		}
		catch (InvalidCollectSettingException ex8)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex8.Message);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex10)
		{
			ex10.Log("打开采账设置窗口时出现异常");
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex10.Message);
		}
	}

	public void CellCollectSet()
	{
		try
		{
			if (TableEditor.IsTableLocked)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格已锁定，不能执行采账设置");
				return;
			}
			if (TableEditor.Table != null && TableEditor.Table.TreeNode != null)
			{
				if (!TableEditor.Table.TreeNode.HasReadPermission())
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "没有表格的查看权限，不能执行采账设置");
					return;
				}
				if (!TableEditor.Table.TreeNode.HasWritePermission())
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "没有表格的编辑权限，不能执行采账设置");
					return;
				}
			}
			if (IsLedgerEmpty())
			{
				ShowOpenLedgerTip();
				return;
			}
			Leqisoft.Model.Table table = TableEditor.Table;
			if (table == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开表格");
				return;
			}
			TableEditor.CellCollect(CurrentLedgerViewer.Ledger);
			TableEditor.PopulateToolbar();
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public async Task OneClickCollect()
	{
		bool tempShowTooltip = ShowHelperTooltip;
		ShowHelperTooltip = false;
		Ledger ledger = CurrentLedgerViewer.Ledger;
		List<TreeTableNode> tableNodes = Leqisoft.Model.Project.Current.GetAllTableNodes().ToList();
		int count = tableNodes.Count();
		ProgressForm<object> pf = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
		{
			for (int j = 0; j < count; j++)
			{
				iProg.Report(new ProgressInfo
				{
					MainCaption = $"正在一键批量生成底稿... ({j}/{count})",
					MainProgress = (int)((double)j / (double)count * 100.0)
				});
				await Task.Yield();
				string tableName = tableNodes[j].Name;
				CollectObjectEnum? collectObjectEnum = DictionarySync.TableCollector.IntellegenceObject(tableName);
				if (collectObjectEnum != CollectObjectEnum.Balance && collectObjectEnum.GetValueOrDefault() != CollectObjectEnum.Summary && collectObjectEnum.GetValueOrDefault() != CollectObjectEnum.Subsidiary && !DictionarySync.CellCollector.IntelligenceFillingTable(tableName))
				{
					
					continue;
				}
				
				await Task.Delay(1);
				Leqisoft.Model.Table table = tableNodes[j].Table.LoadAndReturn();
				table.TryApplyFormula(evalLqDistinct: true);
				
				if (table.TreeNode == null || !table.TreeNode.HasReadPermission() || !table.TreeNode.HasWritePermission())
				{
					
					continue;
				}
				if (TableCollectorAbstract.CanCollect(table))
				{
					
					string collectSource = table.CollectSource;
					TableCollectorAbstract tableCollectorAbstract = null;
					try
					{
						tableCollectorAbstract = TableCollectorAbstract.Deserialize(collectSource, ledger, table);
					}
					catch (InvalidAuditYearException)
					{
					}
					if (tableCollectorAbstract == null || tableCollectorAbstract.Maps.Count == 0)
					{
						try
						{
							tableCollectorAbstract = TableCollectorAbstract.Intelligence(ledger, table);
						}
						catch (InvalidAuditYearException)
						{
						}
						if (tableCollectorAbstract == null || tableCollectorAbstract.Maps.Count == 0)
						{
							goto IL_03e8;
						}
						collectSource = tableCollectorAbstract.Serialize();
						table.UpdateCollectSource(collectSource);
					}
					if (tableCollectorAbstract.Maps.Count != 0)
					{
						try
						{
							if (tableCollectorAbstract.IsAbleUseCurrentSettingToCollectData())
							{
								if (tableCollectorAbstract.CollectObject != CollectObjectEnum.Subsidiary)
								{
									goto IL_02fc;
								}
								if (!DictionarySync.TableCollector.IntellegenceIsNeedSelectDetailAccount(tableName))
								{
									tableCollectorAbstract.Setting.IsOnlyMyMark = true;
									if (DictionarySync.TableCollector.IntellegenceIsNeedSelectAllAccount(tableName))
									{
										tableCollectorAbstract.Setting.CollectAllAccount = true;
									}
									collectSource = tableCollectorAbstract.Serialize();
									table.UpdateCollectSource(collectSource);
									goto IL_02fc;
								}
							}
							goto end_IL_0293;
							IL_02fc:
							if (tableCollectorAbstract.CollectObject == CollectObjectEnum.Balance)
							{
								if (DictionarySync.TableCollector.IntellegenceIsNeedSelectAllAccount(tableName))
								{
									tableCollectorAbstract.Setting.CollectAllAccount = true;
								}
								if (DictionarySync.TableCollector.IntellegenceIsNeedSelectSomeAccount(tableName, out var _, out var selectFilter))
								{
									tableCollectorAbstract.Setting.CollectAllAccount = true;
									tableCollectorAbstract.Setting.CollectingFilter = selectFilter;
								}
								tableCollectorAbstract.Setting.CollectingFilter = new CollectItemShouldSelectFilter_ExcludeEmptyAccount(tableCollectorAbstract, tableCollectorAbstract.Setting.CollectingFilter);
							}
							TableCollectResult tableCollectResult = tableCollectorAbstract.Collect(tableCollectorAbstract.TitlePeriod.Item1.Year);
							bool hasData = tableCollectResult != null && tableCollectResult.Values.Count > 0;
							bool isActiveTable = TableEditor.Table == table;
							
							if (hasData)
							{
								ProjectHierarchy.FindAndSelectNode(table.TreeNode);
								FillEditorTable(tableCollectResult, table);
							}
							end_IL_0293:;
						}
						catch (InvalidCollectSettingException)
						{
							
						}
						catch (InvalidAuditYearException)
						{
							
						}
					}
				}
				goto IL_03e8;
				IL_03e8:
				bool canCellsCollect = CollectManager.CanCollect(table);
				
				if (canCellsCollect)
				{
					ProjectHierarchy.FindAndSelectNode(table.TreeNode);
					CellsCollect(ledger, table);
				}
			}
			return new object();
		});
		if (pf == null)
		{
			return;
		}
		pf.ShowDialog();
		for (int i = 0; i < 10; i++)
		{
			if (pf.Task != null)
			{
				break;
			}
			if (i == 9)
			{
				return;
			}
			await Task.Delay(100);
		}
		await pf.Task;
		ShowHelperTooltip = tempShowTooltip;
	}

	public async Task AutoImport()
	{
		_ = 2;
		try
		{
			if (TableEditor.IsTableLocked)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格已锁定，不能执行采账设置");
				return;
			}
			if (TableEditor != null && TableEditor.Table != null)
			{
				if (!TableEditor.Table.TreeNode.HasReadPermission())
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表格没有查看权限，不能执行采账设置");
					return;
				}
				if (!TableEditor.Table.TreeNode.HasWritePermission())
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表格没有编辑权限，不能执行采账设置");
					return;
				}
			}
			if (!SoftwareLicenseManager.IsAllowAddTableRows())
			{
				return;
			}
			if (IsLedgerEmpty())
			{
				ShowOpenLedgerTip();
				return;
			}
			bool canTableCollect = TableEditor.CanTableCollect();
			bool canCellCollect = TableEditor.CanCellCollect();
			if (!canTableCollect && !canCellCollect)
			{
				if (!canTableCollect)
				{
					await TableCollectSet();
					return;
				}
				AppCommands.Information.ShowInformation(delegate(TooltipBox ttp)
				{
					XElement xElement = new XElement("div", new XElement("p", new XAttribute("style", "color:red"), "未能智能识别当前表格与账套数据的采账填充关系，请对当前表格进行采账设置。"), new XElement("a", new XAttribute("href", "tableCollect"), "列对应采账设置"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XElement("a", new XAttribute("href", "cellCollect"), "单元格采账设置"));
					ttp.SetText("信息提示", xElement.ToString());
					Dictionary<string, object> tagDic = new Dictionary<string, object>
					{
						{ "tableCollect", null },
						{ "cellCollect", null }
					};
					ttp.SetTagDic(tagDic);
					ttp.LinkClicked -= Ttp_LinkClicked;
					ttp.LinkClicked += Ttp_LinkClicked;
				}, 5000);
				return;
			}
			if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.TableCollector.Version == 0)
			{
				try
				{
					await DictionarySync.CheckTableCollectVersionAndUpdate();
				}
				catch (WebException)
				{
					if (DictionarySync.TableCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因网络问题，字典更新失败！");
					}
				}
				catch (TimeoutException)
				{
					if (DictionarySync.TableCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
					}
				}
				catch (Exception ex3)
				{
					if (DictionarySync.TableCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex3.Message + ",请重试");
					}
				}
			}
			if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.CellCollector.Version == 0)
			{
				try
				{
					await DictionarySync.CheckCellCollectDicVersionAndUpdate();
				}
				catch (WebException)
				{
					if (DictionarySync.CellCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因网络问题，字典更新失败！");
					}
				}
				catch (TimeoutException)
				{
					if (DictionarySync.CellCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
					}
				}
				catch (Exception ex6)
				{
					if (DictionarySync.CellCollector.Version == 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex6.Message + ",请重试");
					}
				}
			}
			Leqisoft.Model.Table table = TableEditor.Table;
			if (table == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开表格");
				return;
			}
			Ledger ledger = CurrentLedgerViewer.Ledger;
			if (canTableCollect)
			{
				frmTableCollect2 frmTableCollect = new frmTableCollect2(ledger, table, View);
				frmTableCollect.LoadCollectSetting(table.CollectSource);
				if (frmTableCollect.ShowDialog() == DialogResult.OK)
				{
					string cs = frmTableCollect.Collector?.Serialize();
					table.UpdateCollectSource(cs);
					FillEditorTable(frmTableCollect.Result);
				}
			}
			if (canCellCollect)
			{
				TableEditor.BeginBatchUpdateValue();
				try
				{
					CellsCollect(ledger, table);
				}
				catch (InvalidAuditYearException)
				{
				}
				finally
				{
					TableEditor.EndBatchUpdateValue();
				}
			}
			TableEditor.PopulateToolbar();
		}
		catch (InvalidAuditYearException ex8)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex8.Message);
		}
		catch (InvalidCollectSettingException ex9)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex9.Message);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex11)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex11.Message);
		}
	}

	public void SetOpenModeToTicketMode(TreeTableNode treeTable)
	{
		if (treeTable != null && !treeTable.Table.Ticket.IsEmpty())
		{
			if (!TreeNodeStateCache.Contains(treeTable.Id))
			{
				TreeNodeStateCache.Set(treeTable.Id, new TreeNodeCacheState
				{
					Kind = TreeNodeCacheKind.TicketInput,
					ScrollPosition = Point.Empty,
					Selection = Rectangle.Empty
				});
			}
			else
			{
				TreeNodeCacheState treeNodeCacheState = TreeNodeStateCache.Get(treeTable.Id);
				treeNodeCacheState.Kind = TreeNodeCacheKind.TicketInput;
			}
		}
	}

	public bool SetOpenModeToTableMode(TreeTableNode treeTable)
	{
		if (treeTable == null)
		{
			return false;
		}
		bool result = false;
		if (!treeTable.Table.Ticket.IsEmpty())
		{
			if (!TreeNodeStateCache.Contains(treeTable.Id))
			{
				TreeNodeStateCache.Set(treeTable.Id, new TreeNodeCacheState
				{
					Kind = TreeNodeCacheKind.Table,
					ScrollPosition = Point.Empty,
					Selection = Rectangle.Empty
				});
			}
			else
			{
				TreeNodeCacheState treeNodeCacheState = TreeNodeStateCache.Get(treeTable.Id);
				result = treeNodeCacheState.Kind == TreeNodeCacheKind.TicketInput;
				treeNodeCacheState.Kind = TreeNodeCacheKind.Table;
			}
		}
		return result;
	}

	protected bool CollectFromLedger(TreeTableNode treeTable, LedgerCollectEventArgs e, out AppCommandTab shouldSelectTable)
	{
		shouldSelectTable = null;
		if (treeTable == null)
		{
			return false;
		}
		if (IsLedgerEmpty())
		{
			return false;
		}
		if (!treeTable.HasReadPermission() || !treeTable.HasWritePermission())
		{
			return false;
		}
		Account account = e.Account;
		Account account2 = account;
		if (account != null)
		{
			while (account2.Parent != null)
			{
				account2 = account2.Parent;
			}
		}
		CollectObjectEnum collectObject = e.CollectObject;
		string name = treeTable.Name;
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		if (DictionarySync.TableCollector.IntellegenceObject(name) != collectObject)
		{
			return false;
		}
		TableCollectResult tableCollectResult = null;
		frmTableCollect2 frmTableCollect = new frmTableCollect2(Program.MainForm.CurrentLedgerViewer.Ledger, treeTable.Table, View);
		switch (collectObject)
		{
		case CollectObjectEnum.Subsidiary:
		{
			List<Voucher> list = null;
			if (e.Source != null && e.Source.Count > 0)
			{
				list = new List<Voucher>();
				list.AddRange(e.Source.Select((object u) => u as Voucher));
			}
			if (!e.IsSourceComeFromMyMark)
			{
				frmTableCollect.InitSubsidiaryCollectByAccount(treeTable.Table.CollectSource, e.Account, list);
			}
			else
			{
				frmTableCollect.InitSubsidiaryCollectByMyMark(treeTable.Table.CollectSource, e.Account, e.Auxiliary, list);
			}
			break;
		}
		case CollectObjectEnum.Balance:
		{
			HashSet<Account> hashSet2 = null;
			if (e.Source != null && e.Source.Count > 0)
			{
				hashSet2 = new HashSet<Account>();
				foreach (object item2 in e.Source)
				{
					if (item2 is Account item)
					{
						hashSet2.Add(item);
					}
				}
			}
			frmTableCollect.InitBalanceCollectByAccount(treeTable.Table.CollectSource, account2, hashSet2);
			break;
		}
		case CollectObjectEnum.Summary:
		{
			if (account2 == null)
			{
				return false;
			}
			HashSet<Account> hashSet = null;
			if (e.Account != account2)
			{
				hashSet = new HashSet<Account>();
				for (Account account3 = e.Account; account3 != null; account3 = account3.Parent)
				{
					hashSet.Add(account3);
				}
			}
			frmTableCollect.InitSummaryCollectByAccount(treeTable.Table.CollectSource, account2, hashSet);
			break;
		}
		default:
			return false;
		}
		if (frmTableCollect.ShowDialog() != DialogResult.OK)
		{
			return true;
		}
		string cs = frmTableCollect.Collector?.Serialize();
		treeTable.Table.UpdateCollectSource(cs);
		tableCollectResult = frmTableCollect.Result;
		bool flag = SetOpenModeToTableMode(treeTable);
		TreeNodeBase selectedNode = ProjectHierarchy.SelectedNode;
		bool flag2 = selectedNode == treeTable && flag;
		bool flag3 = false;
		ProjectHierarchy.FindAndSelectNode(treeTable);
		TableEditor.BeginBatchUpdateValue();
		try
		{
			if (tableCollectResult != null && tableCollectResult.Values.Count > 0)
			{
				FillEditorTable(tableCollectResult);
				if (State.SelectedTab != null)
				{
					shouldSelectTable = State.SelectedTab;
				}
				else if (AppCommandTabs.View.Visible)
				{
					shouldSelectTable = AppCommandTabs.View;
				}
				flag3 = true;
				return true;
			}
		}
		catch (InvalidCollectSettingException)
		{
			return false;
		}
		finally
		{
			TableEditor.EndBatchUpdateValue();
			if (flag3 && flag2)
			{
				OpenTable();
			}
		}
		return false;
	}

	private void CellsCollect(Ledger ledger, Leqisoft.Model.Table table)
	{
		try
		{
			Tuple<DateTime, DateTime> auditYear = DictionarySync.GetAuditYear(table);
			if (auditYear == null)
			{
				throw new InvalidAuditYearException("未在当前表格的标题区发现截止日或期间信息，请在标题区完善这些信息。");
			}
			foreach (Leqisoft.Model.Cell cell in table.Cells)
			{
				if (cell.HasColumnFormula() || cell.HasFormula)
				{
					continue;
				}
				try
				{
					CollectorManager collectorManager = new CollectorManager(ledger, table, auditYear);
					collectorManager.LoadFormula(cell.CollectSource);
					if (collectorManager.Collector.CollectItems.Count == 0)
					{
						collectorManager.Intelligence(cell.Row.Index, cell.Column.Index);
					}
					collectorManager.Apply(out var value, out var formula);
					cell.UpdateCollectSource(formula);
					if (value.HasValue)
					{
						cell.UpdateValue(value);
					}
				}
				catch (InvalidCollectSettingException)
				{
					foreach (Leqisoft.Model.Cell cell2 in table.Cells)
					{
						cell2.UpdateCollectSource(null);
					}
					CellsCollect(ledger, table);
					break;
				}
				catch (UnExpectAuditYearException)
				{
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void FillEditorTable(TableCollectResult collectResult, Leqisoft.Model.Table table = null)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		if (collectResult == null || collectResult.Values.Count == 0 || collectResult.Values.First().Value.Count == 0)
		{
			return;
		}
		if (table == null)
		{
			table = TableEditor.Table;
		}
		int upperBoundRow = -1;
		int lowerBoundRow = table.Rows.Count;
		Leqisoft.Model.Row row = table.Rows.FirstOrDefault((Leqisoft.Model.Row r) => r.Role == RowRole.Normal || r.Role == RowRole.Minus || r.Role == RowRole.Among);
		upperBoundRow = ((row == null) ? (table.Rows.Count - 1) : (row.Index - 1));
		lowerBoundRow = table.Rows.Skip(upperBoundRow + 1).FirstOrDefault((Leqisoft.Model.Row r) => r.Role == RowRole.Fixed || r.Role == RowRole.Header || r.Role == RowRole.Subtotal || r.Role == RowRole.Total)?.Index ?? table.Rows.Count;
		List<int> list = null;
		TableCollectorAbstract tableCollector = collectResult.TableCollector;
		TableCollectorBalance tableCollectorBalance = tableCollector as TableCollectorBalance;
		TableCollectorSummary tableCollectorSummary;
		List<Leqisoft.Model.Column> mapCols;
		List<Leqisoft.Model.Column> decimalCols;
		int j;
		if (tableCollectorBalance == null)
		{
			if (!(tableCollector is TableCollectorSubsidiary tableCollectorSubsidiary))
			{
				tableCollectorSummary = tableCollector as TableCollectorSummary;
				if (tableCollectorSummary != null)
				{
					if (!CanFillSummary())
					{
						return;
					}
					long key = tableCollectorSummary.Maps.First((KeyValuePair<long, string> m) => m.Value.Equals("科目名称")).Key;
					Leqisoft.Model.Column byId = table.Columns.GetById(new Id64(key));
					if (byId != null)
					{
						list = Enumerable.Range(0, collectResult.Values.First().Value.Count).ToList();
						if (collectResult.Values.ContainsKey(byId))
						{
							List<object> list2 = collectResult.Values[byId];
							List<Leqisoft.Model.Cell> list3 = (from c in byId.GetCells()
								where c.Row.Index > upperBoundRow && c.Row.Index < lowerBoundRow
								select c).ToList();
							int num = list3.LastOrDefault((Leqisoft.Model.Cell c) => !string.IsNullOrWhiteSpace(c.GetDisplayValue()) && c.Row.Role == RowRole.Normal)?.Row.Index ?? upperBoundRow;
							for (int k = 0; k < list.Count; k++)
							{
								object data2 = list2[k];
								if (data2 == null)
								{
									num = (list[k] = num + 1);
									continue;
								}
								Leqisoft.Model.Cell cell = list3.Find((Leqisoft.Model.Cell c) => data2.Equals(c.Value));
								if (cell == null)
								{
									num = (list[k] = num + 1);
								}
								else
								{
									list[k] = cell.Row.Index;
								}
							}
						}
					}
				}
			}
			else
			{
				mapCols = (from m in tableCollectorSubsidiary.Maps
					where new string[7] { "日期", "字号", "字", "号", "摘要", "借方金额", "贷方金额" }.Contains(m.Value)
					select m into kv
					select table.Columns.GetById(new Id64(kv.Key))).ToList();
				decimalCols = (from m in tableCollectorSubsidiary.Maps
					where new string[2] { "借方金额", "贷方金额" }.Contains(m.Value)
					select m into kv
					select table.Columns.GetById(new Id64(kv.Key))).ToList();
				list = new List<int>();
				Func<int> func = delegate
				{
					for (int num16 = lowerBoundRow - 1; num16 > upperBoundRow; num16--)
					{
						foreach (Leqisoft.Model.Column item in mapCols)
						{
							if (!object.Equals(table[num16, item.Index].Value, "") && (!decimalCols.Contains(item) || !table[num16, item.Index].IsValueEmpty))
							{
								return num16 + 1;
							}
						}
					}
					return upperBoundRow + 1;
				};
				int num4 = func();
				int count = collectResult.Values.First().Value.Count;
				for (j = 0; j < count; j++)
				{
					int num5 = GetRecordMatchRow();
					if (num5 == -1)
					{
						list.Add(num4);
						num4++;
					}
					else
					{
						list.Add(num5);
					}
				}
			}
		}
		else
		{
			if (!CanFillBalance())
			{
				return;
			}
			long key2 = tableCollectorBalance.Maps.First((KeyValuePair<long, string> m) => m.Value.Equals("科目名称")).Key;
			Leqisoft.Model.Column byId2 = table.Columns.GetById(new Id64(key2));
			if (byId2 != null)
			{
				list = Enumerable.Range(0, collectResult.Values.First().Value.Count).ToList();
				if (collectResult.Values.ContainsKey(byId2))
				{
					List<object> list4 = collectResult.Values[byId2];
					List<Leqisoft.Model.Cell> list5 = (from c in byId2.GetCells()
						where c.Row.Index > upperBoundRow && c.Row.Index < lowerBoundRow
						select c).ToList();
					int num6 = list5.LastOrDefault((Leqisoft.Model.Cell c) => !string.IsNullOrWhiteSpace(c.GetDisplayValue()) && c.Row.Role == RowRole.Normal)?.Row.Index ?? upperBoundRow;
					for (int l = 0; l < list.Count; l++)
					{
						object data = list4[l];
						if (data == null)
						{
							num6 = (list[l] = num6 + 1);
							continue;
						}
						Leqisoft.Model.Cell cell2 = list5.Find((Leqisoft.Model.Cell c) => data.Equals(c.Value));
						if (cell2 == null)
						{
							num6 = (list[l] = num6 + 1);
						}
						else
						{
							list[l] = cell2.Row.Index;
						}
					}
				}
			}
		}
		if (list == null)
		{
			list = Enumerable.Range(0, collectResult.Values.First().Value.Count).ToList();
			for (int n = 0; n < list.Count; n++)
			{
				list[n] = upperBoundRow + 1 + n;
			}
		}
		table.BeginBatchUpdateValue();
		try
		{
			int count2 = list.Count((int i) => i >= lowerBoundRow);
			table.Rows.Insert(lowerBoundRow, count2);
			foreach (KeyValuePair<Leqisoft.Model.Column, List<object>> value2 in collectResult.Values)
			{
				int index = value2.Key.Index;
				Leqisoft.Model.Column column = table.Columns[index];
				if (!TableEditor.CanEditColumn(column) || column.IsLocked || column.HasFormula)
				{
					continue;
				}
				for (int num9 = 0; num9 < list.Count; num9++)
				{
					int row2 = list[num9];
					Leqisoft.Model.Cell cell3 = table[row2, index];
					if (!cell3.HasFormula && !cell3.HasColumnFormula())
					{
						object obj = value2.Value[num9];
						if ((obj is double num10 && num10 == 0.0) || (obj is decimal num11 && num11 == 0m) || obj is DBNull)
						{
							obj = ((!cell3.IsSetToNumberFormat()) ? "" : ((object)0.0));
						}
						cell3.UpdateValue(obj);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			table.EndBatchUpdateValue();
		}
		if (TableEditor.Table == table)
		{
			TableEditor.PopulateTable();
		}
		bool CanFillBalance()
		{
			if (!tableCollectorBalance.Maps.Any((KeyValuePair<long, string> m) => m.Value.Equals("科目名称")))
			{
				return false;
			}
			return tableCollectorBalance.Maps.Where((KeyValuePair<long, string> kv) => kv.Value.EndsWith("额")).Any(delegate(KeyValuePair<long, string> kv)
			{
				Leqisoft.Model.Column byId3 = table.Columns.GetById(new Id64(kv.Key));
				return byId3 != null && !byId3.HasFormula;
			});
		}
		bool CanFillSummary()
		{
			if (!tableCollectorSummary.Maps.Any((KeyValuePair<long, string> m) => m.Value.Equals("科目名称")))
			{
				return false;
			}
			return tableCollectorSummary.Maps.Where((KeyValuePair<long, string> kv) => kv.Value.EndsWith("月") || kv.Value == "合计").Any(delegate(KeyValuePair<long, string> kv)
			{
				Leqisoft.Model.Column byId4 = table.Columns.GetById(new Id64(kv.Key));
				return byId4 != null && !byId4.HasFormula;
			});
		}
		int GetRecordMatchRow()
		{
			int iRow;
			for (iRow = 0; iRow < table.Rows.Count; iRow++)
			{
				if (RecordRowMatch(iRow))
				{
					return iRow;
				}
			}
			return -1;
		}
		bool RecordRowMatch(int iRow)
		{
			foreach (Leqisoft.Model.Column item2 in mapCols)
			{
				object obj2 = collectResult.Values[item2][j];
				object value = table[iRow, item2.Index].Value;
				if (!object.Equals(obj2, value))
				{
					if (!decimalCols.Contains(item2))
					{
						return false;
					}
					double num12 = 0.0;
					if (obj2 is double num13)
					{
						num12 = num13;
					}
					else if (obj2 is decimal num14)
					{
						num12 = (double)num14;
					}
					else
					{
						if (!(obj2 is DBNull))
						{
							return false;
						}
						num12 = 0.0;
					}
					if (num12 == 0.0)
					{
						if (!table[iRow, item2.Index].IsValueEmpty)
						{
							return false;
						}
					}
					else if (value is double num15)
					{
						if (num12 != num15)
						{
							return false;
						}
					}
					else
					{
						if (!(value is string s))
						{
							return false;
						}
						if (!double.TryParse(s, out var result))
						{
							return false;
						}
						if (num12 != result)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}

	public bool IsLedgerEmpty()
	{
		return MultiLedgerViewer.IsLedgerEmpty();
	}

	private async void Ttp_LinkClicked(object sender, object e)
	{
		if ((string)sender == "tableCollect")
		{
			await TableCollectSet();
		}
		else if ((string)sender == "cellCollect")
		{
			CellCollectSet();
		}
	}

	public void ShowOpenLedgerTip()
	{
		AppCommands.Information.ShowInformation(delegate(TooltipBox ttp)
		{
			ttp.LinkClicked += delegate(object s1, object e1)
			{
				if (e1?.ToString() == "openledger")
				{
					AppCommandTabs.Ledger.Select();
				}
			};
			XElement xElement = new XElement("p");
			xElement.Add(new XElement("span", new XAttribute("style", "color: red"), "请先打开账套"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XElement("a", new XAttribute("href", "openledger"), "打开账套"));
			ttp.SetText("信息提示", xElement.ToString(), canClose: true);
			ttp.SetTagDic(new Dictionary<string, object> { ["openledger"] = "openledger" });
		}, 5000);
	}

	public void ShowLedgerNotFoundTip()
	{
		AppCommands.Information.ShowInformation(delegate(TooltipBox ttp)
		{
			ttp.LinkClicked += delegate(object s1, object e1)
			{
				if (e1?.ToString() == "openledger")
				{
					AppCommandTabs.Ledger.Select();
				}
			};
			XElement xElement = new XElement("p");
			xElement.Add(new XElement("span", new XAttribute("style", "color:red;"), "未找到该" + StringConstBase.Current.Project + "自动关联的账套"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XElement("a", new XAttribute("href", "openledger"), "打开账套"));
			ttp.SetText("信息提示", xElement.ToString(), canClose: true);
			ttp.SetTagDic(new Dictionary<string, object> { ["openledger"] = "openledger" });
		}, 5000);
	}

	public void ShowHelpSidebar()
	{
		HelpCenterUtil.OpenHelpCenterHomePage();
	}

	public void ConfirmationSetting()
	{
		MergeForm instance = MergeForm.GetInstance();
		instance.Show(CurrentProject);
		instance.AfterSelected += delegate(object s, Leqisoft.Model.Column e)
		{
			CurrentDocumentEditor.InsertMergeField(e);
		};
	}

	public async Task GenerateConfirmationFromDocument()
	{
		if (!(ProjectHierarchy.SelectedNode is TreeDocumentNode { Document: var document } selectDocumentNode))
		{
			return;
		}
		if (!CurrentDocumentEditor.TryGetMergeTable(out var _, out var tableId) || tableId == 0L)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先进行函证设置");
			return;
		}
		Leqisoft.Model.Table tableById = document.Project.GetTableById(new Id64(tableId));
		if (tableById == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "未找到当前绑定表，请重新进行设置");
			return;
		}
		List<Dictionary<string, Leqisoft.Model.Cell>> dataTable = getDataTable(tableById);
		if (dataTable.Count >= 100)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "一次最多生成一百张凭证！");
			return;
		}
		DocumentEditor de = DocumentEditors[selectDocumentNode.Document];
		await de.RefreshAllTablesAndFormulas();
		byte[] source = de.Export();
		int index = selectDocumentNode.Index;
		TreeDirectoryNode treeDirectoryNode = (selectDocumentNode.IsRoot ? selectDocumentNode.Group.InsertRootDirectory(index) : selectDocumentNode.Parent.InsertChildDirectory(index));
		treeDirectoryNode.UpdateName("函证列表");
		Node node = ProjectHierarchy.FindNode(selectDocumentNode).Node;
		Node node2 = node.AddNode(NodeTypeEnum.PreviousSibling, treeDirectoryNode.Name, treeDirectoryNode, Resources.TreeDir);
		foreach (Dictionary<string, Leqisoft.Model.Cell> item in dataTable)
		{
			if (!item.All((KeyValuePair<string, Leqisoft.Model.Cell> t) => t.Value.IsEmpty))
			{
				TreeDocumentNode treeDocumentNode = treeDirectoryNode.InsertChildDocument(treeDirectoryNode.Children.Count);
				treeDocumentNode.UpdateName(item.FirstOrDefault().Value.GetDisplayValue());
				node2.AddNode(NodeTypeEnum.LastChild, treeDocumentNode.Name, treeDocumentNode, Resources.TreeDoc);
				DocumentEditor documentEditor = new DocumentEditor();
				treeDocumentNode.IsEntityDirty = true;
				documentEditor.Document = treeDocumentNode.Document;
				documentEditor.PopulateDocument();
				documentEditor.Import(source);
				documentEditor.GenerateConfirmation(item);
				AddDocumentEditor(documentEditor);
			}
		}
		static List<Dictionary<string, Leqisoft.Model.Cell>> getDataTable(Leqisoft.Model.Table ctable)
		{
			List<Dictionary<string, Leqisoft.Model.Cell>> list = new List<Dictionary<string, Leqisoft.Model.Cell>>();
			foreach (Leqisoft.Model.Row row in ctable.Rows)
			{
				if (row.Role == RowRole.Normal || row.Role == RowRole.Among || row.Role == RowRole.Minus)
				{
					Dictionary<string, Leqisoft.Model.Cell> dictionary = new Dictionary<string, Leqisoft.Model.Cell>();
					foreach (Leqisoft.Model.Column column in ctable.Columns)
					{
						dictionary.Add(column.Id.ToString(), ctable.Cells.Get(row.Index, column.Index));
					}
					list.Add(dictionary);
				}
			}
			return list;
		}
	}

	public void ShowFormulaPane()
	{
		// Word 文档视图不显示水平公式编辑栏
		if (CurrentView == MainFormView.Document || CurrentView == MainFormView.DocumentPreview)
		{
			return;
		}
		// Collapsed=true → Visible=true → 显示
		pnlFormula.Collapsed = true;
	}

	public void HideFormulaPane()
	{
		// Collapsed=false → Visible=false → 隐藏
		pnlFormula.Collapsed = false;
	}

	public void ShowNavigationPanel()
	{
		if (_isDelayChangeNavigationPanelVisibleStatus)
		{
			_isNavigationPanelVisible = true;
			return;
		}
		pnlNav.SizeRatio = _pnlNavLastSizeRatio;
		pnlNav.Resizable = true;
	}

	public void HideNavigationPanel()
	{
		if (_isDelayChangeNavigationPanelVisibleStatus)
		{
			_isNavigationPanelVisible = false;
			return;
		}
		if (pnlNav.Resizable)
		{
			_pnlNavLastSizeRatio = pnlNav.SizeRatio;
		}
		pnlNav.SizeRatio = 0.0;
		pnlNav.Resizable = false;
	}

	public void SuspendNavPanelVisible()
	{
		if (_mainPandelSuspendDepth > 0)
		{
			_mainPandelSuspendDepth++;
			return;
		}
		_mainPandelSuspendDepth++;
		_isDelayChangeNavigationPanelVisibleStatus = true;
		_isNavigationPanelVisible = pnlNav.Visible;
	}

	public void ResumeNavPanelVisible()
	{
		_mainPandelSuspendDepth--;
		if (_mainPandelSuspendDepth <= 0)
		{
			_isDelayChangeNavigationPanelVisibleStatus = false;
			if (_isNavigationPanelVisible)
			{
				ShowNavigationPanel();
			}
			else
			{
				HideNavigationPanel();
			}
		}
	}

	public void SuspendMainPanelDrawing()
	{
		pnlMain.SuspendDrawing();
	}

	public void ResumeMainPanelDrawing()
	{
		pnlMain.ResumeDrawing();
	}

	public void SuspendNavPanelDrawing()
	{
		pnlNav.SuspendDrawing();
	}

	public void ResumeNavPanelDrawing()
	{
		pnlNav.ResumeDrawing();
	}

	public void BindControlToNavigationPanel(Control control)
	{
		if (pnlNav.Controls.Count == 0)
		{
			pnlNav.Controls.Add(control);
		}
		else if (pnlNav.Controls[0] != control)
		{
			pnlNav.SuspendLayout();
			try
			{
				pnlNav.Controls.Clear();
				pnlNav.Controls.Add(control);
			}
			finally
			{
				pnlNav.ResumeLayout(performLayout: true);
			}
		}
	}

	public async void AlterInfo()
	{
		frmAlterInfo frmAlterInfo2 = new frmAlterInfo();
		if (frmAlterInfo2.ShowDialog() == DialogResult.OK)
		{
			if (frmAlterInfo2.UserNameChanged)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因您修改了用户名，需要重新登录");
				await Exit();
			}
			else
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新成功");
			}
		}
	}

	public void SwitchTeam()
	{
		if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持切换组织");
			return;
		}
		using dlgTeamSelector dlg = new dlgTeamSelector(UserTeam.Teams);
		dlg.ShowDialog();
	}

	public void AlterPwd()
	{
		if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持修改密码");
			return;
		}
		LoginMode loginMode = TokenTimer.LoginInfo.LoginMode;
		if ((uint)(loginMode - 1) <= 1u)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "第三方账号登录不支持修改密码操作！");
			return;
		}
		frmAlterPwd frmAlterPwd2 = new frmAlterPwd();
		frmAlterPwd2.ShowDialog();
	}

	public void AccessControl()
	{
		if (CanAccessControl())
		{
			frmAccessManage frmAccessManage2 = new frmAccessManage();
			frmAccessManage2.Project = CurrentProject;
			frmAccessManage2.ShowDialog();
		}
	}

	public Task CheckUpdate()
	{
		Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "程序更新已禁用");
		return Task.CompletedTask;
	}

	public void Fullscreen()
	{
		CurrentEdition.Ribbon.Minimized = true;
		staMain.Hide();
		pnlTree.Hide();
		AppCommands.ToggleFullscreen.IsPressed = true;
		AppCommands.ToggleFullscreenSmall.IsPressed = true;
	}

	public void QuitFullscreen()
	{
		CurrentEdition.Ribbon.Minimized = UserSet.Config.HideTab;
		staMain.Show();
		pnlTree.Show();
		AppCommands.ToggleFullscreen.IsPressed = false;
		AppCommands.ToggleFullscreenSmall.IsPressed = false;
	}

	public void ToggleTooltip()
	{
		UserSet.Config.Tooltip = !UserSet.Config.Tooltip;
		AppCommands.ShowHelp.IsPressed = UserSet.Config.Tooltip;
		AppCommands.ShowHelpSmall.IsPressed = UserSet.Config.Tooltip;
	}

	public void LedgerPrint()
	{
		try
		{
			CurrentLedgerViewer.Print();
		}
		catch (ArgumentOutOfRangeException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ParamName);
		}
		catch (Exception ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	public void LedgerExport()
	{
		try
		{
			CurrentLedgerViewer.SaveToExcel();
		}
		catch (ArgumentOutOfRangeException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ParamName);
		}
		catch (Exception ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	public void PreviewDirection(bool landscape)
	{
		CurrentLedgerViewer.ChangePreviewDirection(landscape);
	}

	public void LedgerPreview(bool preview)
	{
		try
		{
			CurrentLedgerViewer.PrintPreview(preview);
		}
		catch (ArgumentOutOfRangeException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ParamName);
		}
		catch (Exception ex2) when (!(ex2 is PreviewNotSupport))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	public void UpdateState(Action<AppState> newState)
	{
		newState(State);
		CurrentEdition.OnAppStateChanged(State);
	}

	public void SwitchStateTo(MainFormView to)
	{
		MainFormView currentView = CurrentView;
		UpdateState(delegate(AppState s)
		{
			s.ViewKind = to;
		});
		CurrentView = to;
		if (_lazyTableEditor.IsValueCreated)
		{
			TableEditor.ValidationEditor.View.Enabled = CurrentView == MainFormView.Table || CurrentView == MainFormView.EditingValidation || CurrentView == MainFormView.EditingColHeader || CurrentView == MainFormView.EditingTitle || CurrentView == MainFormView.EditingFoot || CurrentView == MainFormView.EditingNote;
			if (CurrentView == MainFormView.Table)
			{
				TableEditor.ToolBar.Enabled = true;
			}
			else
			{
				TableEditor.ToolBar.Enabled = false;
			}
			if (CurrentDocumentEditor != null)
			{
				if (CurrentView == MainFormView.Document)
				{
					if (CurrentDocumentEditor.ToolBar != null)
						CurrentDocumentEditor.ToolBar.Enabled = true;
				}
				else
				{
					if (CurrentDocumentEditor.ToolBar != null)
						CurrentDocumentEditor.ToolBar.Enabled = false;
				}
			}
		}
		StatusBar.Enabled = CurrentView == MainFormView.Table || CurrentView == MainFormView.Document;
		bool isWordView = CurrentView == MainFormView.Document || CurrentView == MainFormView.DocumentPreview;
		bool shouldShowFormula = !isWordView && (CurrentView == MainFormView.Table || CurrentView == MainFormView.EditingFormula || CurrentView == MainFormView.EditingTitle || CurrentView == MainFormView.EditingColHeader || CurrentView == MainFormView.EditingFoot || CurrentView == MainFormView.EditingNote || CurrentView == MainFormView.FormatBrush || CurrentView == MainFormView.EditingValidation || CurrentView == MainFormView.TicketDesign || CurrentView == MainFormView.TicketFormula);
		// Collapsed 属性直接映射到 Visible（Collapsed=true → Visible=true → 显示）
		// Word 文档视图不显示公式栏，其他视图正常显示
		pnlFormula.Collapsed = shouldShowFormula;
		pnlTree.Collapsed = CurrentView != MainFormView.TicketDesign && CurrentView != MainFormView.TicketFormula;
		_preView = currentView;
		HideNavigationPanel();
		switch (currentView)
		{
		case MainFormView.Table:
			TableEditor.OnLeaveView();
			break;
		case MainFormView.TicketInput:
			TicketInputEditor.OnLeaveView();
			break;
		}
		if (currentView == MainFormView.TicketInput && to != MainFormView.TicketInput)
		{
			TicketInputEditor.SaveGridSelectRangeAndScrollPosition();
		}
		switch (to)
		{
		case MainFormView.Table:
			TableEditor.OnEnterView();
			break;
		case MainFormView.TicketInput:
			TicketInputEditor.OnEnterView();
			break;
		case MainFormView.Document:
			CurrentDocumentEditor.OnEnterView(false);
			break;
		case MainFormView.DocumentPreview:
			CurrentDocumentEditor.OnEnterView(true);
			break;
		}
		switch (to)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
		case MainFormView.EditingColHeader:
		case MainFormView.EditingTitle:
		case MainFormView.EditingNote:
		case MainFormView.FormatBrush:
		case MainFormView.EditingValidation:
		case MainFormView.EditingFoot:
			TableEditor.KeepNavigationPanelVisibleIfNecessary();
			break;
		case MainFormView.TicketPrint:
			ShowNavigationPanel();
			break;
		}
		if (to == MainFormView.EditingFormula)
		{
			switch (currentView)
			{
			case MainFormView.Table:
			case MainFormView.EditingColHeader:
			case MainFormView.EditingTitle:
			case MainFormView.EditingNote:
			case MainFormView.FormatBrush:
			case MainFormView.EditingValidation:
			case MainFormView.EditingFoot:
				TableEditor.KeepNavigationPanelVisibleIfNecessary();
				break;
			case MainFormView.Document:
				ShowNavigationPanel();
				break;
			case MainFormView.TablePreview:
			case MainFormView.DocumentPreview:
			case MainFormView.EditingFormula:
			case MainFormView.Image:
			case MainFormView.Pdf:
			case MainFormView.ImagePreview:
			case MainFormView.PdfPreview:
				break;
			}
		}
	}

	public void MakeUsbCollector()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "乐其采数器");
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.Description = "请选择生成采数器的文件夹位置：";
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			string text = Path.Combine(folderBrowserDialog.SelectedPath, "乐其采数器");
			try
			{
				Directory.CreateDirectory(text);
				HashSet<string> except = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "账套文件" };
				CopyFilesRecursively(new DirectoryInfo(path), new DirectoryInfo(text), except);
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "采数器生成完毕");
				Process.Start(text);
			}
			catch (Exception ex)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
		}
	}

	public async Task ValidateAll()
	{
		if (SoftwareLicenseManager.IsValidateAllTableOutOfLicenseLimit())
		{
			return;
		}
		await TableEditor.CalcAllTables();
		ValidationResultCache.Clear();
		ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> progress)
		{
			Dictionary<Id64, string> dicIdName = (from n in CurrentProject.GetAllTableNodes()
				where n.Visible
				select n).ToDictionary((TreeTableNode n) => n.Id, (TreeTableNode n) => n.Name);
			for (int i = 0; i < CurrentProject.ValidationManager.Formulas.Count; i++)
			{
				Leqisoft.Model.ValidationFormula vf = CurrentProject.ValidationManager.Formulas[i];
				if (dicIdName.TryGetValue(vf.TableId, out var value))
				{
					progress.Report(new ProgressInfo
					{
						MainCaption = "正在校验 " + value,
						MainProgress = (int)((double)(i + 1) / (double)CurrentProject.ValidationManager.Formulas.Count * 100.0)
					});
					List<ValidationResult> value2 = await Task.Run(() => CurrentProject.ValidationManager.Validate(vf));
					ValidationResultCache.Add(vf.Id, value2);
				}
			}
			return Task.FromResult<object>(null);
		});
		progressForm.ShowDialog();
		UpdateTableValidationCache();
		if (TableValidationResults.Sum((KeyValuePair<TreeTableNode, TableValidationInfo> tvi) => tvi.Value.ErrorRefs.Count) == 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "恭喜您，校验已完成，未发现错误！");
		}
		else
		{
			StatusBar.ShowComment();
		}
		SetShowHelperTooltipAndRefresh(show: true);
	}

	public void ValidateCurrentTable()
	{
		if (!CurrentProject.ValidationManager.Formulas.Where((Leqisoft.Model.ValidationFormula u) => u.TableId == CurrentTable.Id).Any())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表格未设置校验公式！");
			return;
		}
		HashSet<Id64> hashSet = new HashSet<Id64>();
		FormulaManagerTransitional formulaManagerTransitional = new FormulaManagerTransitional(Leqisoft.Model.Project.Current);
		try
		{
			foreach (Leqisoft.Model.ValidationFormula formula in CurrentProject.ValidationManager.Formulas)
			{
				HashSet<Id64> referredTables = CurrentProject.ValidationManager.GetReferredTables(formula);
				if (!referredTables.Contains(CurrentTable.Id))
				{
					continue;
				}
				foreach (Id64 item in referredTables)
				{
					hashSet.Add(item);
				}
			}
			formulaManagerTransitional.CalculateTable(hashSet);
		}
		catch (FormulaException)
		{
		}
		foreach (Id64 item2 in ValidationResultCache.Keys.Where((Id64 k) => !CurrentProject.ValidationManager.Formulas.Exists((Leqisoft.Model.ValidationFormula vf) => vf.Id == k)).ToList())
		{
			ValidationResultCache.Remove(item2);
		}
		foreach (Leqisoft.Model.ValidationFormula formula2 in CurrentProject.ValidationManager.Formulas)
		{
			try
			{
				if (CurrentProject.ValidationManager.GetReferredTables(formula2).Contains(CurrentTable.Id))
				{
					if (ValidationResultCache.TryGetValue(formula2.Id, out var _))
					{
						ValidationResultCache[formula2.Id] = CurrentProject.ValidationManager.Validate(formula2);
						continue;
					}
					List<ValidationResult> value2 = CurrentProject.ValidationManager.Validate(formula2);
					ValidationResultCache.Add(formula2.Id, value2);
				}
			}
			catch (FormulaException)
			{
			}
		}
		UpdateTableValidationCache();
		if (!TableValidationResults.TryGetValue(CurrentTable.TreeNode, out var value3) || value3.ErrorRefs.Count == 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "恭喜您，校验已完成，未发现错误！");
		}
		else
		{
			StatusBar.ShowComment();
		}
		SetShowHelperTooltipAndRefresh(show: true);
	}

	public void ValidateOne(Leqisoft.Model.ValidationFormula vf)
	{
		if (CurrentProject.ValidationManager.Formulas.Contains(vf))
		{
			ValidationResultCache[vf.Id] = CurrentProject.ValidationManager.Validate(vf, rethrow: true);
		}
		UpdateTableValidationCache();
		SetShowHelperTooltipAndRefresh(show: true);
	}

	protected bool IsAllowShowFunctionInfoAtCurrentView(string funName, bool isControlFormulaForm, bool isCollectFormulaForm)
	{
		if (_controlFormulaDic == null)
		{
			_controlFormulaDic = new HashSet<string>(FunctionEvaluator.GetFunctionOnlyShowInControlFormulaEditWindow());
		}
		if (!isControlFormulaForm && _controlFormulaDic.Contains(funName))
		{
			return false;
		}
		if (!isCollectFormulaForm && funName.Equals("Cancel", StringComparison.InvariantCultureIgnoreCase))
		{
			return false;
		}
		MainFormView currentView = CurrentView;
		if (currentView != MainFormView.TicketDesign && currentView != MainFormView.TicketFormula && (funName.Equals("TicketTitle", StringComparison.InvariantCultureIgnoreCase) || funName.Equals("TicketFoot", StringComparison.InvariantCultureIgnoreCase)))
		{
			return false;
		}
		return true;
	}

	public bool IsAllowShowFunctionInfoInFunctionList(string funName)
	{
		if (_obsoletedFormulaDic == null)
		{
			_obsoletedFormulaDic = new HashSet<string>(FunctionEvaluator.GetObsoletedFunction(), StringComparer.OrdinalIgnoreCase);
		}
		if (_obsoletedFormulaDic.Contains(funName))
		{
			return false;
		}
		return IsAllowShowFunctionInfoAtCurrentView(funName, isControlFormulaForm: false, isCollectFormulaForm: false);
	}

	public bool IsAllowShowFunctionInfoAtCurrentView(FunctionInfo funInfo, bool isControlFormulaForm = false, bool isCollectFormulaForm = false)
	{
		if (_obsoletedFormulaDic == null)
		{
			_obsoletedFormulaDic = new HashSet<string>(FunctionEvaluator.GetObsoletedFunction(), StringComparer.OrdinalIgnoreCase);
		}
		if (_obsoletedFormulaDic.Contains(funInfo.Name))
		{
			return false;
		}
		return IsAllowShowFunctionInfoAtCurrentView(funInfo.Name, isControlFormulaForm, isCollectFormulaForm);
	}

	public void ShowFunctionHint(string formulaText, int pos)
	{
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(formulaText);
			Tuple<string, int> tup = formulaDisplay.GetFuncNameAtPos(pos);
			if (tup.Item1 == null)
			{
				DisplayEditingFormula();
				return;
			}
			FunctionInfo functionInfo = FunctionInfo.AllFunctionInfos.FirstOrDefault((FunctionInfo f) => f.Name.Equals(tup.Item1, StringComparison.InvariantCultureIgnoreCase));
			if (functionInfo == null || !IsAllowShowFunctionInfoAtCurrentView(functionInfo))
			{
				DisplayEditingFormula();
				return;
			}
			AppCommands.FormulaTip1.Label.Text = "函数语法：" + functionInfo.Name + "(" + string.Join(", ", functionInfo.Parameters.Select((Leqisoft.Model.ParameterInfo p) => p.Name)) + ")";
			AppCommands.FormulaTip2.Label.Text = "函数功能：" + functionInfo.Description;
			AppCommands.FormulaTip3.Label.Text = "参数说明：" + string.Join("；", functionInfo.Parameters.Select((Leqisoft.Model.ParameterInfo p) => p.Name + "：" + p.Description));
		}
		catch (FormulaException)
		{
			DisplayEditingFormula();
		}
		static void DisplayEditingFormula()
		{
			AppCommands.FormulaTip1.Label.Text = "状态提示：当前处于公式编辑状态，主菜单已被禁用。";
			AppCommands.FormulaTip2.Label.Text = "保存公式：当公式编辑完成后，按回车键保存公式，并退出公式编辑状态。";
			AppCommands.FormulaTip3.Label.Text = "放弃公式：当拟放弃保存公式时，按esc键可放弃保存公式，并退出公式编辑状态。";
		}
	}

	private void _ttpFormulaHint_CloseClick(object sender, EventArgs e)
	{
		SetShowHelperTooltipAndRefresh(show: false);
		AppCommands.Information.HideInformation();
	}

	public void HideFunctionHint()
	{
		_ttpFormulaHint.Hide();
		AppCommands.Information.Visible = false;
	}

	public void ToggleSideToolbar()
	{
		if (_showSideToolbar)
		{
			TableEditor.HideToolbar();
			foreach (KeyValuePair<Leqisoft.Model.Document, DocumentEditor> documentEditor in DocumentEditors)
			{
				documentEditor.Value.HideToolbar();
			}
			ImageEditor.HideToolbar();
			PdfViewer.HideToolbar();
			MultiLedgerViewer.HideToolbar();
			LeqiTheme selectedLeqiTheme = Theme.SelectedLeqiTheme;
			if (AppCommands.ShowSidebar.Button != null)
			{
				if (selectedLeqiTheme != null && selectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
				{
					AppCommands.ShowSidebar.Button.SmallImage = new WhiteImageStrategy().ProcessImage(Resources.ShowSideToolbar16);
				}
				else
				{
					AppCommands.ShowSidebar.Button.SmallImage = Resources.ShowSideToolbar16;
				}
			}
		}
		else
		{
			TableEditor.ShowToolbar();
			foreach (KeyValuePair<Leqisoft.Model.Document, DocumentEditor> documentEditor2 in DocumentEditors)
			{
				documentEditor2.Value.ShowToolbar();
			}
			ImageEditor.ShowToolbar();
			PdfViewer.ShowToolbar();
			MultiLedgerViewer.ShowToolbar();
			LeqiTheme selectedLeqiTheme2 = Theme.SelectedLeqiTheme;
			if (AppCommands.ShowSidebar.Button != null)
			{
				if (selectedLeqiTheme2 != null && selectedLeqiTheme2.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
				{
					AppCommands.ShowSidebar.Button.SmallImage = new WhiteImageStrategy().ProcessImage(Resources.HideSideToolbar16);
				}
				else
				{
					AppCommands.ShowSidebar.Button.SmallImage = Resources.HideSideToolbar16;
				}
			}
		}
		_showSideToolbar = !_showSideToolbar;
	}

	public bool CanAccessControl()
	{
		if (CurrentProject.Kind == ProjectType.Template)
		{
			return false;
		}
		if (!CurrentProject.Users.Any((KeyValuePair<Leqisoft.DTO.User, UserRole> kv) => kv.Key.Id == Leqisoft.Model.User.Current.Id))
		{
			return false;
		}
		if (CurrentProject.Users.First((KeyValuePair<Leqisoft.DTO.User, UserRole> kv) => kv.Key.Id == Leqisoft.Model.User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		return false;
	}

	public void Test()
	{
	}

	public async Task RevertTableDialog()
	{
		if (SoftwareLicenseManager.IsRevertHistoryDataOutOfLicenseLimit())
		{
			return;
		}
		if (!ProjectHierarchy.HasWritePermission())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您没有该表格的编辑权限。");
			return;
		}
		int latestVersion = 0;
		Leqisoft.Model.Table clone;
		int version;
		if (TableEditor.Table.NeedSave || TableEditor.Table.TreeNode.IsEntityDirty)
		{
			clone = new Leqisoft.Model.Table
			{
				TreeNode = TableEditor.Table.TreeNode,
				_loaded = true
			};
			version = -1;
		}
		else
		{
			clone = TableEditor.Table.TemporaryClone();
			version = clone.Version;
		}
		JObject request = JObject.FromObject(new
		{
			Action = "PullTable",
			Id = clone.Id.Value,
			ProjectId = clone.Project.Id,
			Version = version
		});
		try
		{
			PullTable pullTable = await Leqisoft.LocalDataStore.StorageRouter.PullTable(request);
			if (pullTable.Result == "NeedUpdate")
			{
				Syncer.Merge(pullTable, clone);
				latestVersion = pullTable.Version;
			}
			else if (pullTable.Result == "Latest")
			{
				latestVersion = clone.Version;
			}
		}
		catch (HttpRequestException ex)
		{
			ex.Log();
			if (ex.InnerException is NormalException ex2)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			}
			else if (ex.InnerException is TimeoutException ex3)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
			}
			else
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "发生了未预期的异常:\r\n" + ex.ToString());
			}
			return;
		}
		FormTimelineViewer formTimelineViewer = new FormTimelineViewer();
		formTimelineViewer.TemporaryTable = clone;
		formTimelineViewer.LatestVersion = latestVersion;
		formTimelineViewer.ShowDialog();
	}

	public async Task RevertDocumentDialog()
	{
		if (SoftwareLicenseManager.IsRevertHistoryDataOutOfLicenseLimit())
		{
			return;
		}
		if (!ProjectHierarchy.HasWritePermission())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您没有该文档的编辑权限。");
			return;
		}
		if (CurrentDocumentEditor.Document.TreeNode.IsEntityDirty)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前文件未同步，请同步后再使用历史版本功能！");
			return;
		}
		FormTimelineViewer form = new FormTimelineViewer();
		Leqisoft.Model.Document clone = CurrentDocumentEditor.Document.TemporaryClone();
		JObject request = JObject.FromObject(new
		{
			Action = "PullDocument",
			Id = clone.Id.Value,
			ProjectId = clone.Project.Id,
			Version = clone.Version
		});
		try
		{
			PullDocument pullDocument = await Leqisoft.LocalDataStore.StorageRouter.PullDocument(request);
			if (pullDocument.Result == "NeedUpdate")
			{
				Syncer.Merge(pullDocument, clone);
				form.LatestVersion = pullDocument.Version;
			}
			else if (pullDocument.Result == "Latest")
			{
				form.LatestVersion = clone.Version;
			}
		}
		catch (HttpRequestException ex)
		{
			ex.Log();
			if (ex.InnerException is NormalException ex2)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			}
			else if (ex.InnerException is TimeoutException ex3)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
			}
			else
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "发生了未预期的异常:\r\n" + ex.ToString());
			}
			return;
		}
		form.TemporaryDocument = clone;
		form.ShowDialog();
	}

	public void HandleSyncMessage(string projectId)
	{
		SyncTwinkle.Start();
		RibbonButton button = AppCommands.SyncProjectSmall.Button;
		Rectangle itemBounds = button.Ribbon.GetItemBounds(button);
		TooltipBox tooltipBox = new TooltipBox
		{
			Duration = 5000,
			IsBalloon = true
		};
		XElement xElement = new XElement("p", new XAttribute("style", "color:red;"), StringConstBase.Current.Project + "数据有更新，您可以请点击这里同步数据");
		tooltipBox.SetText("同步提示", xElement.ToString());
		tooltipBox.Show(button.Ribbon, new Point((itemBounds.Left + itemBounds.Right) / 2, itemBounds.Bottom));
	}

	public void HandleSystemMessage()
	{
		TabMember.PopulateSystemGroup();
	}

	public void AddDocumentEditor(DocumentEditor editor)
	{
		DocumentEditors.Add(editor.Document, editor);
		Theme.SetCurrentTree(editor.View);
	}

	public void CloseDocument(Leqisoft.Model.Document d)
	{
		if (DocumentEditors.TryGetValue(d, out var value))
		{
			value.View.Dispose();
			DocumentEditors.Remove(d);
		}
	}

	public void Print()
	{
		switch (CurrentView)
		{
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			CurrentDocumentEditor.Print();
			break;
		case MainFormView.Pdf:
		case MainFormView.PdfPreview:
			try
			{
				PdfViewer.Print();
				break;
			}
			catch
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打印失败，pdf文件异常");
				break;
			}
		case MainFormView.Table:
		case MainFormView.TablePreview:
		{
			Leqisoft.Model.Table table = (ProjectHierarchy.SelectedNode as TreeTableNode)?.Table;
			if (table != null)
			{
				Preview.Table = table;
				Preview.PrintDialog();
			}
			break;
		}
		case MainFormView.Image:
		case MainFormView.ImagePreview:
		{
			Leqisoft.Model.Image image = (ProjectHierarchy.SelectedNode as TreeImageNode)?.Image;
			if (image != null)
			{
				Preview.Image = image;
				Preview.PrintDialog();
			}
			break;
		}
		case MainFormView.TicketInput:
			TicketPrinter.Ticket = TicketInputEditor.Ticket;
			TicketPrinter.SetVm();
			TicketPrinter.Populate();
			TicketPrinter.Print();
			break;
		case MainFormView.TicketPrint:
			TicketPrinter.Print();
			break;
		}
	}

	public async void ExportPdfDocumentDialog()
	{
		try
		{
			switch (CurrentView)
			{
			case MainFormView.Document:
			case MainFormView.DocumentPreview:
				CurrentDocumentEditor.ExportPdfDocumentDialog();
				break;
			case MainFormView.Pdf:
			case MainFormView.PdfPreview:
				if (await PdfViewer.SaveDialog() == DialogResult.OK)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件导出成功");
				}
				break;
			}
		}
		catch (IOException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因文件被占用或其他原因，导出失败。");
		}
		catch (Exception ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因文件被占用或其他原因，导出失败。错误信息：" + ex2.Message);
		}
	}

	public void TtpCommentClosedShowInformation()
	{
		SetShowHelperTooltipAndRefresh(show: false);
		TooltipBox tooltipBox = new TooltipBox
		{
			IsBalloon = true
		};
		Rectangle itemBounds = AppCommands.ShowTooltipSmall.RibbonItem.Ribbon.GetItemBounds(AppCommands.ShowTooltipSmall.RibbonItem);
		tooltipBox.Duration = 5000;
		XElement xElement = new XElement("p", new XAttribute("style", "color: red;"), "动态提示框功能被关闭，点击这里，可以恢复动态提示框的显示。");
		tooltipBox.SetText("信息提示", xElement.ToString());
		tooltipBox.Show(AppCommands.ShowTooltipSmall.RibbonItem.Ribbon, new Point(itemBounds.Left + itemBounds.Width / 2, itemBounds.Bottom));
	}

	public void SetShowHelperTooltipAndRefresh(bool show)
	{
		ShowHelperTooltip = show;
		AppCommands.ShowTooltip.IsPressed = show;
		AppCommands.ShowTooltipSmall.IsPressed = show;
		if (!show)
		{
			TableEditor._ttpComment.Hide();
		}
		NodeValidationResults.Clear();
		GroupValidationResults.Clear();
		if (show)
		{
			foreach (Leqisoft.Model.TreeGroup treeGroup in CurrentProject.TreeGroups)
			{
				int num = 0;
				foreach (TreeNodeBase rootNode in treeGroup.RootNodes)
				{
					num += AddNodeErrorCount(rootNode);
				}
				GroupValidationResults.Add(treeGroup, num);
			}
		}
		ProjectHierarchy.Invalidate();
		TableEditor.Invalidate();
		TicketInputEditor.RefreshValidationResult();
		int AddNodeErrorCount(TreeNodeBase n)
		{
			if (n is TreeTableNode key)
			{
				if (TableValidationResults.TryGetValue(key, out var value))
				{
					int count = value.ErrorRefs.Count;
					NodeValidationResults.Add(key, count);
					return count;
				}
				return 0;
			}
			if (n is TreeDocumentNode treeDocumentNode)
			{
				// 文档校验错误计数：后续 Task 10 会接入 DocumentValidationResultCache
				// 当前暂返回 0，避免读取未赋值的 ValidationErrors 导致 NullReferenceException
				return 0;
			}
			if (n is TreeDirectoryNode treeDirectoryNode)
			{
				int num2 = treeDirectoryNode.Children.Sum((TreeNodeBase c) => AddNodeErrorCount(c));
				if (num2 > 0)
				{
					NodeValidationResults.Add(treeDirectoryNode, num2);
				}
				return num2;
			}
			TreeImageNode treeImageNode = n as TreeImageNode;
			if (treeImageNode == null)
			{
				TreePdfNode treePdfNode = n as TreePdfNode;
				if (treePdfNode == null)
				{
					throw new ArgumentOutOfRangeException();
				}
			}
			return 0;
		}
	}

	public void PreviousError()
	{
		object previousErrorRef = GetPreviousErrorRef(CurrentTable.TreeNode);
		if (previousErrorRef != null)
		{
			GotoErrorRef(previousErrorRef);
		}
	}

	public void NextError()
	{
		object nextErrorRef = GetNextErrorRef(CurrentTable.TreeNode);
		if (nextErrorRef != null)
		{
			GotoErrorRef(nextErrorRef);
		}
	}

	public async Task Exit()
	{
		string exitMsg = Leqisoft.LocalDataStore.StorageRouter.IsLocalMode
			? "程序即将退出，是否保存" + StringConstBase.Current.Project + "？"
			: "程序即将退出，是否保存并同步" + ((RecentProjects.Count > 1) ? $"已打开的 {RecentProjects.Count} 个" : "当前") + StringConstBase.Current.Project + "？";
		switch (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, exitMsg, MessageBoxButtons.YesNoCancel))
		{
		case DialogResult.Yes:
			await SyncProjects();
			break;
		case DialogResult.Cancel:
			return;
		}
		await Program.Logout();
		_closeByCode = true;
		View.Close();
	}

	public void Undo()
	{
		switch (CurrentView)
		{
		case MainFormView.Table:
			TableEditor.Undo();
			break;
		case MainFormView.Document:
			CurrentDocumentEditor.Undo();
			break;
		}
		UpdateUndoRedoButtonState();
	}

	public void Redo()
	{
		switch (CurrentView)
		{
		case MainFormView.Table:
			TableEditor.Redo();
			break;
		case MainFormView.Document:
			CurrentDocumentEditor.Redo();
			break;
		}
		UpdateUndoRedoButtonState();
	}

	/// <summary>
	/// 根据当前编辑器的 CanUndo/CanRedo 状态更新撤销/恢复按钮的 Enabled 状态
	/// </summary>
	public void UpdateUndoRedoButtonState()
	{
		bool canUndo = false;
		bool canRedo = false;
		switch (CurrentView)
		{
		case MainFormView.Table:
			// TableEditor 使用 CommandsManager，根据其 CanUndo/CanRedo 设置按钮状态
			if (TableEditor != null && TableEditor.Table != null)
			{
				canUndo = TableEditor.Table.CommandsManager.CanUndo;
				canRedo = TableEditor.Table.CommandsManager.CanRedo;
			}
			break;
		case MainFormView.Document:
			if (CurrentDocumentEditor != null)
			{
				canUndo = CurrentDocumentEditor.Tx.CanUndo;
				canRedo = CurrentDocumentEditor.Tx.CanRedo;
			}
			break;
		}
		AppCommands.Undo.Enabled = canUndo;
		AppCommands.Redo.Enabled = canRedo;
	}

	public async Task ModifyCurrentProjectMembers()
	{
		if (!CanAccessControl())
		{
			return;
		}
		Leqisoft.DTO.Project dto;
		if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
		{
			var projects = await Leqisoft.LocalDataStore.StorageRouter.GetProjects();
			dto = projects.FirstOrDefault(p => p.Id == CurrentProject.Id);
		}
		else
		{
			dto = await WebApiClient.GetProjectDto(CurrentProject.Id);
		}
		if (CurrentEdition is AppEditionGeneral)
		{
			FormProjectMembers formProjectMembers = new FormProjectMembers();
			formProjectMembers.Project = dto;
			if (formProjectMembers.ShowDialog() != DialogResult.OK)
			{
				return;
			}
		}
		else if (dto.Type == ProjectType.Project)
		{
			dlgProjectEditor dlgProjectEditor2 = new dlgProjectEditor();
			dlgProjectEditor2.Project = dto;
			if (!dlgProjectEditor2.ShowModify())
			{
				return;
			}
		}
		else if (dto.Type == ProjectType.Template)
		{
			dlgTemplateEditor dlgTemplateEditor2 = new dlgTemplateEditor();
			dlgTemplateEditor2.Template = dto;
			if (!dlgTemplateEditor2.ShowModify())
			{
				return;
			}
		}
		try
		{
			if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
			{
				await WebApiClient.UpdateProject(dto);
				await SignalRClient.ChangeProjectMember(dto.Id.ToString());
			}
		}
		catch (Exception ex) when (ex is NormalException || ex.InnerException is NormalException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public Point PnlMainRelativePosition(Point ptScreen)
	{
		Point point = pnlMain.Parent.PointToScreen(pnlMain.Location);
		return new Point(ptScreen.X - point.X, ptScreen.Y - point.Y);
	}

	private static string GetViewKindDescription(MainFormView viewKind)
	{
		return viewKind switch
		{
			MainFormView.EditingTitle => "编辑表头状态",
			MainFormView.EditingFoot => "编辑表尾状态",
			MainFormView.EditingFormula => "编辑公式状态",
			MainFormView.EditingValidation => "编辑审核公式状态",
			MainFormView.EditingColHeader => "编辑列头状态",
			MainFormView.EditingNote => "编辑附注状态",
			MainFormView.FormatBrush => "格式刷状态",
			MainFormView.DocFormatBrush => "文档格式刷状态",
			MainFormView.DocumentPreview => "文档预览状态",
			MainFormView.PdfPreview => "PDF预览状态",
			MainFormView.ImagePreview => "图片预览状态",
			MainFormView.TablePreview => "表格预览状态",
			MainFormView.TicketPrint => "单据打印状态",
			MainFormView.TicketFormula => "单据公式状态",
			MainFormView.TicketDesign => "单据设计状态",
			MainFormView.Empty => "空白视图状态",
			MainFormView.Ledger => "账簿视图状态",
			_ => "编辑状态(" + viewKind + ")",
		};
	}

	private async void View_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (State.ViewKind != MainFormView.Table && State.ViewKind != MainFormView.Document && State.ViewKind != 0 && State.ViewKind != MainFormView.Image && State.ViewKind != MainFormView.Pdf && State.ViewKind != MainFormView.TicketInput && State.ViewKind != MainFormView.Empty && State.ViewKind != MainFormView.Ledger)
		{
			string stateDesc = GetViewKindDescription(State.ViewKind);
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前正处于" + stateDesc + "，无法关闭程序，请先退出该编辑状态。");
			e.Cancel = true;
		}
		else if ((e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown) && !_closeByCode)
		{
			string closeMsg = Leqisoft.LocalDataStore.StorageRouter.IsLocalMode
				? "程序即将退出，是否保存" + StringConstBase.Current.Project + "？"
				: "程序即将退出，是否保存并同步" + ((RecentProjects.Count > 1) ? $"已打开的 {RecentProjects.Count} 个" : "当前") + StringConstBase.Current.Project + "？";
			switch (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, closeMsg, MessageBoxButtons.YesNoCancel))
			{
			case DialogResult.Yes:
				e.Cancel = true;
				await SyncProjects();
				await Program.Logout();
				_closeByCode = true;
				View.Close();
				break;
			case DialogResult.No:
				await Program.Logout();
				e.Cancel = false;
				break;
			case DialogResult.Cancel:
				e.Cancel = true;
				break;
			}
			CurrentLedgerViewer?.SaveConfig();
		}
	}

	private void View_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.Q)
		{
			if (_ispnlMainShow)
			{
				SwitchMainView();
			}
			else if (CurrentEdition != null && CurrentEdition.EnableLedger)
			{
				SwitchFinanceView();
			}
		}
		else if (e.KeyCode == Keys.Escape && State.ViewKind == MainFormView.FormatBrush)
		{
			TableEditor.EndFormatBrush();
		}
	}

	private async void View_Shown(object sender, EventArgs e)
	{
		ThemeEditor.SelectTheme(UserSet.Config.CurrentTheme);
		Theme.SetCurrentTree(staMain);
		MultiLedgerViewer.SetTheme();
		SwitchToEmptyView();
		Application.DoEvents();
		ApplyConfig();
		ChatManager.BulletLauncher = new BulletLauncher(View);
		#region debug-point D:first-touch
		// ★ DictionarySync 首次访问
		_ = DictionarySync.TableCollector;
		try
		{
			await ChatManager.UpdateTeamMember();
		}
		catch (Exception)
		{
		}
		try
		{
			await ChatManager.UpdateProjectMember(CurrentProject.Id);
		}
		catch (Exception)
		{
		}
		CheckProjectUpdate();
		FormShowned = true;
		TabMember = new TabMember(AppCommandTabs.Members.RibbonTab);
		TabMember.Populate();
		messageHandle = new MessageHandle();
		HandleSystemMessage();
		#endregion
	}

	private void StepRecorder_AfterOperation(object sender, EventArgs e)
	{
		AppCommands.Back.Enabled = stepRecorder.CanBack;
		AppCommands.Forward.Enabled = stepRecorder.CanForward;
	}

	private void MultiLedgerViewer_SelectionNumberChanged(object sender, LedgerSelectionChangedEventArgs e)
	{
		if (e.Numbers.Count > 0)
		{
			SelectionStats.Text = $"求和：{e.Numbers.Sum():#,0.##############################}  计数：{e.Numbers.Count}  平均值：{e.Numbers.Average():#,0.##############################}";
		}
	}

	private async void MainForm_CurrentProjectChanged(object sender, EventArgs e)
	{
		SignalRClient.UserState.ProjectId = CurrentProject.Id.ToString();
		await ChangeProject();
		openedRelate = false;
		if (FormShowned)
		{
			CheckProjectUpdate();
		}
		if (!IsLedgerEmpty())
		{
			MultiLedgerViewer.LedgerDefaultPanel?.Populate(CurrentLedgerViewer?.CurrentFilePath);
		}
	}

	private async void MultiLedgerViewer_AfterCollect(object sender, LedgerCollectEventArgs e)
	{
		if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.TableCollector.Version == 0)
		{
			try
			{
				await DictionarySync.CheckTableCollectVersionAndUpdate();
			}
			catch (WebException)
			{
				if (DictionarySync.TableCollector.Version == 0)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因网络问题，字典更新失败！");
				}
			}
			catch (TimeoutException)
			{
				if (DictionarySync.TableCollector.Version == 0)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
				}
			}
			catch (Exception ex3)
			{
				if (DictionarySync.TableCollector.Version == 0)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex3.Message + ",请重试");
				}
			}
		}
		if (IsLedgerEmpty())
		{
			ShowOpenLedgerTip();
			return;
		}
		try
		{
			TableEditor.IsNotShowComment = true;
			frmSelectImport frmSelectImport2 = new frmSelectImport(CurrentProject);
			if (frmSelectImport2.ShowDialog(e) != DialogResult.OK || frmSelectImport2 == null || frmSelectImport2.Selects.Count == 0)
			{
				return;
			}
			bool flag = false;
			AppCommandTab shouldSelectTable = null;
			foreach (TreeTableNode select in frmSelectImport2.Selects)
			{
				flag = CollectFromLedger(select, e, out shouldSelectTable) || flag;
			}
			if (flag)
			{
				shouldSelectTable?.Select();
			}
			else
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "未能智能识别出填充的底稿的填充列，请检查该科目底稿列名设置是否正确。");
			}
		}
		catch (InvalidOperationException ex4)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
		}
		catch (InvalidAuditYearException ex5)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
		}
		catch (Exception ex6)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex6.Message);
		}
		finally
		{
			TableEditor.IsNotShowComment = false;
		}
	}

	private void View_FormClosed(object sender, FormClosedEventArgs e)
	{
		Program.ApplicationExit();
	}

	private void CtnAll_SplitterMoved(object sender, SplitterEventArgs e)
	{
		_isSplitterMoving = false;
	}

	private void CtnAll_SplitterMoving(object sender, SplitterCancelEventArgs e)
	{
		_isSplitterMoving = true;
	}

	private void CtnAll_MouseUp(object sender, MouseEventArgs e)
	{
		if (_isSplitterMoving)
		{
			if (_ispnlMainShow)
			{
				AnimateStart(ctnAll.Panels[0], ToRight: false);
				_animateTimer.Start();
				_animateForm.Show();
			}
			else
			{
				AnimateStart(ctnAll.Panels[1]);
				_animateTimer.Start();
				_animateForm.Show();
			}
		}
	}

	private void _animateTimer_Tick(object s1, EventArgs e1)
	{
		if (_animateToRight)
		{
			if (_animateScreenWidth - _animateForm.Left < 100)
			{
				_animateForm.Left = _animateScreenWidth;
				_animateTimer.Stop();
				if (_ispnlMainShow)
				{
					ctnAll.Panels[0].Width = 0;
					_ispnlMainShow = !_ispnlMainShow;
				}
				else
				{
					ctnAll.Panels[0].Width = View.Width;
					_ispnlMainShow = !_ispnlMainShow;
				}
				_animateForm.Visible = false;
				_animateForm.Left = 0;
				AppCommandTabs.Ledger.Select();
			}
			else
			{
				if (!_animateForm.Visible)
				{
					_animateForm.Visible = true;
				}
				_animateForm.Left += 100;
			}
		}
		else if (_animateForm.Right - 100 < 0)
		{
			_animateForm.Left = -_animateForm.Width;
			_animateTimer.Stop();
			if (_ispnlMainShow)
			{
				ctnAll.Panels[0].Width = 0;
				_ispnlMainShow = !_ispnlMainShow;
			}
			else
			{
				ctnAll.Panels[0].Width = View.Width;
				_ispnlMainShow = !_ispnlMainShow;
			}
			_animateForm.Visible = false;
			_animateForm.Left = 0;
			State.SelectedTab?.Select();
		}
		else
		{
			if (!_animateForm.Visible)
			{
				_animateForm.Visible = true;
			}
			_animateForm.Left -= 100;
		}
	}

	private void cmdDuplicateDocument_Click(object sender, ClickEventArgs e)
	{
		TreeNodeBase selectedNode = ProjectHierarchy.SelectedNode;
		if (selectedNode == null)
		{
			return;
		}
		if (!ProjectHierarchy.HasWritePermission())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您没有编辑权限，无法复制文档。");
			return;
		}
		try
		{
			TreeNodeBase clonedNode = null;
			if (selectedNode is TreeDocumentNode docNode)
			{
				clonedNode = docNode.DuplicateDocument();
			}
			else if (selectedNode is TreeTableNode tableNode)
			{
				clonedNode = tableNode.DuplicateTable();
			}
			else if (selectedNode is TreeImageNode imgNode)
			{
				clonedNode = imgNode.DuplicateImage();
			}
			if (clonedNode != null && selectedNode.Parent != null)
			{
				selectedNode.Parent.InsertChildNode(clonedNode, selectedNode.Index + 1);
				ProjectHierarchy.Populate();
				ProjectHierarchy.FindAndSelectNode(clonedNode);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, "复制文档失败:\r\n" + ex.Message, MessageBoxButtons.OK, "错误!");
		}
	}

	public bool IsInAllowDragTreeNodeView()
	{
		if (FormulaEditor.IsEditing || TableEditor.ValidationEditor.IsEditing || TableEditor.AuxEditor.IsEditing || TableEditor.TitleEditor.AuxEditor.IsEditing || TableEditor.FootEditor.AuxEditor.IsEditing || TableEditor.FormControlFormula.IsEditing || TableEditor.LedgerCollectFormulaEditor.IsEditing)
		{
			return false;
		}
		return true;
	}

	public bool IsInEditingFormula()
	{
		if (FormulaEditor.IsEditing || TableEditor.ValidationEditor.IsEditing || TableEditor.AuxEditor.IsEditing || TableEditor.TitleEditor.AuxEditor.IsEditing || TableEditor.FootEditor.AuxEditor.IsEditing || TableEditor.FormControlFormula.IsEditing || TableEditor.LedgerCollectFormulaEditor.IsEditing)
		{
			return true;
		}
		return false;
	}

	private async void ProjectHierarchy_TreeNodeSelected(object sender, EventArgs e)
	{
		if (FormulaEditor.IsEditing || TableEditor.ValidationEditor.IsEditing || TableEditor.AuxEditor.IsEditing || TableEditor.TitleEditor.AuxEditor.IsEditing || TableEditor.FootEditor.AuxEditor.IsEditing || TableEditor.FormControlFormula.IsEditing || TableEditor.LedgerCollectFormulaEditor.IsEditing)
		{
			TreeNodeSelected_EditingFormula();
		}
		else
		{
			await TreeNodeSelected_Normal();
		}
	}

	public void SetPreviousTableTab()
	{
		AppCommandTab appCommandTab = State.SelectedTab;
		if (appCommandTab == AppCommandTabs.Document || appCommandTab == AppCommandTabs.TicketInput)
		{
			appCommandTab = null;
		}
		if (appCommandTab != null)
		{
			_previousTableTab = appCommandTab;
		}
	}

	public void SetPreviousTicketTab()
	{
		AppCommandTab appCommandTab = State.SelectedTab;
		if (appCommandTab == AppCommandTabs.Document || appCommandTab == AppCommandTabs.Table || appCommandTab == AppCommandTabs.Advanced)
		{
			appCommandTab = null;
		}
		if (appCommandTab != null)
		{
			_previousTicketTab = appCommandTab;
		}
	}

	public void SetPreviousDocumentTab()
	{
		AppCommandTab appCommandTab = State.SelectedTab;
		if (appCommandTab == AppCommandTabs.TicketInput)
		{
			appCommandTab = null;
		}
		if (appCommandTab != null)
		{
			_previousDocumentTab = appCommandTab;
		}
	}

	private async Task TreeNodeSelected_Normal()
	{
		ProjectHierarchy.IsInOpeningSomeTreeNode = true;
		pnlMain.SuspendDrawing();
		SuspendNavPanelDrawing();
		SuspendNavPanelVisible();
		try
		{
			await TreeNodeSelected_NormalImpl();
		}
		finally
		{
			ResumeNavPanelVisible();
			ResumeNavPanelDrawing();
			pnlMain.ResumeDrawing();
			ProjectHierarchy.IsInOpeningSomeTreeNode = false;
		}
	}

	#pragma warning disable CS1998
	private async Task TreeNodeSelected_NormalImpl()
	{
		if (State.ViewKind == MainFormView.Table)
		{
			TableEditor.FinishEditorInputStatus();
			SetPreviousTableTab();
		}
		else if (State.ViewKind == MainFormView.TicketInput)
		{
			SetPreviousTicketTab();
		}
		else if (State.ViewKind == MainFormView.Document)
		{
			SetPreviousDocumentTab();
		}
		if (CurrentView == MainFormView.EditingTitle)
		{
			TableEditor.TitleEditor.LeaveEdit();
		}
		else if (CurrentView == MainFormView.EditingColHeader)
		{
			TableEditor.EndEditColHeaders();
		}
		else if (CurrentView == MainFormView.EditingFoot)
		{
			TableEditor.FootEditor.LeaveEdit();
		}
		else if (CurrentView == MainFormView.FormatBrush)
		{
			TableEditor.EndFormatBrush();
		}
		else if (CurrentView == MainFormView.DocFormatBrush)
		{
			CurrentDocumentEditor.EndFormatPainter();
		}
		else if (CurrentView == MainFormView.TicketInput)
		{
			TicketInputEditor.SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
		}
		ctnMain.SuspendLayout();
		TableEditor._ttpComment.Hide();
		TicketInputEditor.HideTooltip();
		foreach (DocumentEditor value in DocumentEditors.Values)
		{
			if (value._ttpComment != null) value._ttpComment.Hide();
		}
		if (ProjectHierarchy.SelectedNode != null && !ProjectHierarchy.SelectedNode.HasReadPermission())
		{
			AppCommands.Information.ShowInformation("信息提示", "该文件被" + StringConstBase.Current.Manager + "设置了查看权限，您没有查看该文件的权限。", 5000);
			SwitchToEmptyView();
			return;
		}
		if (CurrentView == MainFormView.Table)
		{
			TableEditor.SaveTableNavTreeStatusData();
		}
		FormulaEditor.SetSourceCellValue(string.Empty);
		FormulaEditor.SetFormulaText(string.Empty);
		TreeNodeBase selectedNode = ProjectHierarchy.SelectedNode;
		if (!(selectedNode is TreeDirectoryNode) && selectedNode != null)
		{
			if (!(selectedNode is TreeTableNode treeTableNode))
			{
				if (!(selectedNode is TreeDocumentNode treeDocumentNode))
				{
					TreeImageNode treeImageNode = selectedNode as TreeImageNode;
					if (treeImageNode == null)
					{
						TreePdfNode treePdfNode = selectedNode as TreePdfNode;
						if (treePdfNode != null)
						{
							treePdfNode.Pdf.LoadAndReturn();
							if (!CurrentProject.FileCacheManager.Exists(treePdfNode.Pdf.FileId))
							{
								try
								{
									ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
									ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
									progressRuntimeData.NextStep("正在同步 PDF 文件，请稍候...");
									progressRuntimeData.UpdateProgress(0.8f);
									progressForm.ShowDialog(progressRuntimeData, async delegate
									{
										await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
										await CurrentProject.FileCacheManager.DownloadIfNotExist(treePdfNode.Pdf.FileId);
									});
								}
								catch (HttpRequestException ex)
								{
									Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
									return;
								}
								catch (IOException)
								{
									Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "下载过程中出错，请稍候重试");
									return;
								}
							}
							PdfViewer.Pdf = treePdfNode.Pdf;
							PdfViewer.Populate();
							OpenPdf();
						}
					}
					else
					{
						treeImageNode.Image.LoadAndReturn();
						if (!CurrentProject.FileCacheManager.Exists(treeImageNode.Image.FileId))
						{
							try
							{
								ProgressForm2 progressForm2 = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
								ProgressRuntimeData progressRuntimeData2 = new ProgressRuntimeData();
								progressRuntimeData2.NextStep("正在同步图片文件，请稍候...");
								progressRuntimeData2.UpdateProgress(0.8f);
								progressForm2.ShowDialog(progressRuntimeData2, async delegate
								{
									await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
									await CurrentProject.FileCacheManager.DownloadIfNotExist(treeImageNode.Image.FileId);
								});
							}
							catch (HttpRequestException ex3)
							{
								Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
							}
							catch (IOException)
							{
								Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "下载过程中出错，请稍候重试");
							}
						}
						ImageEditor.Image = treeImageNode.Image;
						ImageEditor.Populate();
						OpenImage();
					}
				}
				else
				{
					if (DocumentEditors.ContainsKey(treeDocumentNode.Document))
					{
						CurrentDocumentEditor = DocumentEditors[treeDocumentNode.Document];
						if (CurrentDocumentEditor.DocBadFlag)
						{
							SwitchToBadDocView();
							return;
						}
						OpenDocument();
					}
					else
					{
						CurrentDocumentEditor = new DocumentEditor();
						CurrentDocumentEditor.Document = treeDocumentNode.Document;
						AddDocumentEditor(CurrentDocumentEditor);
						CurrentDocumentEditor.PopulateDocument();
						if (CurrentDocumentEditor.DocBadFlag)
						{
							SwitchToBadDocView();
							return;
						}
						OpenDocument();
					}
					if (State.ViewKind == MainFormView.Document)
					{
						TrySelectRibbonTab(_previousDocumentTab);
					}
				}
			}
			else
			{
				TableEditor.Table = treeTableNode.Table.LoadAndReturn();
				FormulaEditor.Context.Table = TableEditor.Table;
				FormulaEditor.Context.Kind = FormulaContextKind.None;
				OpenTable();
			}
		}
		else
		{
			SwitchToEmptyView();
		}
		TreeNodeBase selectedNode2 = ProjectHierarchy.SelectedNode;
		if (selectedNode2 != null && !(selectedNode2 is TreeDirectoryNode))
		{
			if (_shouldRecord)
			{
				stepRecorder.New(Tuple.Create(CurrentProject.Id, selectedNode2.Id.Value));
			}
			SendOpenTreeNode_SignalR(CurrentProject.Id.ToString(), selectedNode2.Id.ToString());
		}
		ctnMain.ResumeLayout(performLayout: true);
	}

	private void SendOpenTreeNode_SignalR(string projectId, string nodeId)
	{
		if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
			SignalRClient.OpenTreeNode(projectId, nodeId);
	}

	private void TreeNodeSelected_EditingFormula()
	{
		TableEditor._ttpComment.Hide();
		if (FormulaEditor.IsInsideINDEX())
		{
			if (ProjectHierarchy.SelectedNode != null)
			{
				FormulaEditor.InsertRefText("{" + ProjectHierarchy.SelectedNode.FormulaUniqueName + "}");
			}
		}
		else if (ProjectHierarchy.SelectedNode is TreeTableNode treeTableNode)
		{
			TableEditor.Table = treeTableNode.Table;
			TableEditor.PopulateTable();
			TableEditor.View.Show();
			TableEditor.View.BringToFront();
			if (FormulaEditor.IsEditing)
			{
				FormulaEditor.SetFocus();
			}
			else if (TableEditor.ValidationEditor.IsEditing || TableEditor.AuxEditor.IsEditing || TableEditor.TitleEditor.AuxEditor.IsEditing || TableEditor.FootEditor.AuxEditor.IsEditing || TableEditor.FormControlFormula.IsEditing || TableEditor.LedgerCollectFormulaEditor.IsEditing)
			{
				FormulaEditor.View.Enabled = false;
			}
		}
		else
		{
			_ = ProjectHierarchy.SelectedNode is TreeDirectoryNode;
		}
	}

	public void OpendLedger_Click()
	{
		try
		{
			MultiLedgerViewer.LedgerDefaultPanel.BringToFront();
		}
		catch (Exception)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开失败！文件损坏或其他异常");
		}
	}

	private void FormulaEditor_ResizeRequest(object sender, EventArgs e)
	{
		if (FormulaEditor.IsEditing)
		{
			pnlFormula.Height = FormulaEditor.DesiredHeight;
		}
		else
		{
			pnlFormula.Height = 31;
		}
	}

	private void Initialize()
	{
		CurrentProjectChanged += MainForm_CurrentProjectChanged;
		ctnAll.SplitterMoving += CtnAll_SplitterMoving;
		ctnAll.SplitterMoved += CtnAll_SplitterMoved;
		ctnAll.MouseUp += CtnAll_MouseUp;
		ctnAll.Panels.Add(pnlLedger);
		pnlMain.Controls.Add(FormulaMap.View);
		pnlMain.Controls.Add(ctnMain);
		ctnAll.Panels.Add(pnlMain);
		pnlCtnAllParent.Controls.Add(ctnAll);
		View.Controls.Add(ctnRibbonFormClientArea);
		View.Controls.SetChildIndex(ctnRibbonFormClientArea, 0);
		pnlTree = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			SizeRatio = 15.0,
			KeepRelativeSize = true,
			BackColor = Color.Transparent,
			MinWidth = 0
		};
		_pnlNavLastSizeRatio = 15.0;
		pnlNav = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			SizeRatio = _pnlNavLastSizeRatio,
			KeepRelativeSize = true,
			Resizable = true,
			BackColor = Color.Transparent,
			MinWidth = 0,
			MinHeight = 0
		};
		pnlNav.SizeChanged += PnlNav_SizeChanged;
		pnlTree.Controls.Add(ProjectHierarchy.View);
		ctnMain.Panels.Add(pnlTree);
		ctnMain.Panels.Add(pnlNav);
		ctnMain.Panels.Add(pnlHelp);
		ctnMain.Panels.Add(pnlFormula);
		ctnMain.Panels.Add(pnlContent);
		ProjectHierarchy.TreeNodeSelected += ProjectHierarchy_TreeNodeSelected;
		FormulaEditor.ResizeRequest += FormulaEditor_ResizeRequest;
		staMain = new C1StatusBar();
		View.Controls.Add(staMain);
		StatusBar = new SplitContainerRibbonControlHost(this);
		staMain.LeftPaneItems.Add(StatusBar);
		ThemeEditor = new ThemeForm();
		ThemeForm themeEditor = ThemeEditor;
		themeEditor.SelectedThemeChanged = (EventHandler<LeqiTheme>)Delegate.Combine(themeEditor.SelectedThemeChanged, new EventHandler<LeqiTheme>(ThemeEditor_SelectedThemeChanged));
		_animateTimer.Tick += _animateTimer_Tick;
		stepRecorder = new StepRecorder<Tuple<Guid, long>>(new StepContext<Tuple<Guid, long>>
		{
			Restore = async delegate(Tuple<Guid, long> tp)
			{
				await SkipToTreeNode(tp);
			}
		});
		stepRecorder.AfterOperation += StepRecorder_AfterOperation;
		ImageEditor = new ImageEditor(View);
		pnlContent.Controls.Add(ImageEditor.View);
	}

	private void PnlNav_SizeChanged(object sender, EventArgs e)
	{
		if (pnlNav.Visible && !(pnlNav.SizeRatio <= 0.0))
		{
			_pnlNavLastSizeRatio = pnlNav.SizeRatio;
		}
	}

	private void MultiLedgerViewer_AfterShare(object sender, LedgerShareEventArgs e)
	{
		ShareClick(e.Link, e.File);
	}

	private void MultiLedgerViewer_LedgerNotFound(object sender, EventArgs e)
	{
		ShowLedgerNotFoundTip();
	}

	public void ShareClick(C1CommandLink shareLink, string file)
	{
		if (string.IsNullOrEmpty(file) || !File.Exists(file))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账套文件不存在！");
			return;
		}
		C1CommandLink[] onlineMemberContextMenu = GetOnlineMemberContextMenu(file);
		if (onlineMemberContextMenu == null || onlineMemberContextMenu.Length == 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "项目中没有在线成员！");
			return;
		}
		C1ContextMenu c1ContextMenu = new C1ContextMenu();
		c1ContextMenu.CommandLinks.AddRange(onlineMemberContextMenu);
		c1ContextMenu.ShowContextMenu(shareLink.Owner as C1ToolBar, new Point(shareLink.Bounds.Left, shareLink.Bounds.Bottom));
	}

	public C1CommandLink[] GetOnlineMemberContextMenu(string file)
	{
		Leqisoft.Model.User current = Leqisoft.Model.User.Current;
		IEnumerable<MemTab> enumerable = MemberManager.GetInstance().GetGroup(current.TeamId.ToString())?.GetSelfAndMembers()?.Where((MemTab m) => m is Member member2 && member2.Id != current.Id.ToString() && member2.IsOnline);
		if (enumerable == null || enumerable.Count() == 0)
		{
			return null;
		}
		List<C1CommandLink> list = new List<C1CommandLink>();
		foreach (MemTab member in enumerable)
		{
			C1CommandLink c1CommandLink = new C1CommandLink();
			C1Command c1Command = new C1Command();
			c1Command.Text = member.Name;
			c1Command.Image = member.Image;
			c1Command.UserData = member;
			c1CommandLink.Command = c1Command;
			c1Command.Click += async delegate
			{
				if (!File.Exists(file))
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该账套文件不存在！");
				}
				else
				{
					await PushFileImpl(member, file);
					MultiLedgerViewer.LedgerDefaultPanel.BringToFront(file);
				}
			};
			list.Add(c1CommandLink);
		}
		return list.ToArray();
		static async Task PushFileImpl(MemTab toMember, string file2)
		{
			if (file2 == null)
			{
				return;
			}
			try
			{
				FileTransferModel.FileInfo fileInfo = FileUtil.GetFileInfo(file2);
				NotifyMessage notifyMessage = new NotifyMessage
				{
					Kind = "sendfilerequest",
					Bullet = true,
					Value = JsonConvert.SerializeObject(fileInfo),
					Text = MemberManager.GetInstance().GetMember(Leqisoft.Model.User.Current.Id.ToString()).Name + "向您发送来 " + Path.GetFileName(file2)
				};
				FileTranferManager.GetInstance().FileCacheMap.Add(fileInfo.Id, new FileCache
				{
					SendUserId = Leqisoft.Model.User.Current.Id.ToString(),
					RecieveUserId = toMember.Id.ToString(),
					FileInfo = fileInfo,
					LocalFile = file2,
					FileState = FileState.SendWaitAccept
				});
				if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
					await SignalRClient.SendToUser(toMember.Id.ToString(), notifyMessage.ToString());
			}
			catch (Exception)
			{
			}
		}
	}

	private void MultiLedgerViewer_AfterOpenLedger(object sender, LedgerEventArgs e)
	{
		if (IsLedgerEmpty())
		{
			AppCommands.AgeAnalysis.Visible = true;
			return;
		}
		Ledger ledger = CurrentLedgerViewer.Ledger;
		DateTime endDate = ledger.GetEndDate();
		if (new DateTime(endDate.Year, endDate.Month, endDate.Day).AddYears(-1).AddDays(1.0) < ledger.StartDate)
		{
			AppCommands.AgeAnalysis.Visible = false;
		}
		else
		{
			AppCommands.AgeAnalysis.Visible = true;
		}
		if (Path.GetExtension(e.Viewer.CurrentFilePath) == ".db")
		{
			LedgerHistory2.OpenHistory.Add(CurrentProject?.Id.ToString(), e.Viewer.CurrentFilePath);
		}
		MultiLedgerViewer.LedgerDefaultPanel.Populate(CurrentLedgerViewer?.CurrentFilePath);
		MultiLedgerViewer.PopulateOpenedLedgers();
	}

	public async Task BatchPrint_Click(string functionDescription)
	{
		if (!SoftwareLicenseManager.IsCurrentLiceneseAllowBatchPrint(functionDescription))
		{
			return;
		}
		frmNodeSelectorWithTicketRecord frm = new frmNodeSelectorWithTicketRecord();
		frm.Project = CurrentProject;
		if (frm.ShowBatchPrinter() != DialogResult.OK || SoftwareLicenseManager.IsBatchPrintOutOfLicenseLimit(frm.Selected.Count))
		{
			return;
		}
		PrintDialog printDialog = new PrintDialog();
		if (printDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		PrinterSettings printerSettings = printDialog.PrinterSettings;
		ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> progress)
		{
			await Task.Delay(10);
			int totalCount = frm.Selected.Count;
			int current = 0;
			foreach (TreeNodeBase node in frm.Selected)
			{
				try
				{
					if (!(node is TreeDocumentNode treeDoc))
					{
						if (!(node is TreePdfNode treePdf))
						{
							if (!(node is TreeTableNode treeTable))
							{
								if (!(node is TreeImageNode treeImage))
								{
									if (node is frmNodeSelectorWithTicketRecord.TicketNavTreeRecordNode ticketNode)
									{
										ProgressInfo obj = new ProgressInfo
										{
											MainCaption = "正在打印" + ticketNode.Name + "..."
										};
										int num = current + 1;
										current = num;
										obj.MainProgress = num / totalCount;
										progress.Report(obj);
										await Task.Delay(100);
										if (ProjectHierarchy.FindAndSelectNode(ticketNode.TableNode))
										{
											TicketPrinter.Ticket = ticketNode.TableNode.Table.Ticket;
											TicketPrinter.SetVm(TicketPrinter.Ticket, ticketNode.Record);
											TicketPrinter.Populate(isUpdateAppCommandStatus: false);
											TicketPrinter.Print(printerSettings);
										}
									}
								}
								else
								{
									ProgressInfo obj2 = new ProgressInfo
									{
										MainCaption = "正在打印" + treeImage.Name + "..."
									};
									int num = current + 1;
									current = num;
									obj2.MainProgress = num / totalCount;
									progress.Report(obj2);
									await Task.Delay(100);
									if (ProjectHierarchy.FindAndSelectNode(treeImage))
									{
										Preview.Image = treeImage.Image;
										Preview.Print(printerSettings);
									}
								}
							}
							else
							{
								ProgressInfo obj3 = new ProgressInfo
								{
									MainCaption = "正在打印" + treeTable.Name + "..."
								};
								int num = current + 1;
								current = num;
								obj3.MainProgress = num / totalCount;
								progress.Report(obj3);
								await Task.Delay(100);
								if (ProjectHierarchy.FindAndSelectNode(treeTable))
								{
									Preview.Table = treeTable.Table;
									Preview.Print(printerSettings);
								}
							}
						}
						else
						{
							ProgressInfo obj4 = new ProgressInfo
							{
								MainCaption = "正在打印" + treePdf.Name + "..."
							};
							int num = current + 1;
							current = num;
							obj4.MainProgress = num / totalCount;
							progress.Report(obj4);
							await Task.Delay(100);
							if (ProjectHierarchy.FindAndSelectNode(treePdf))
							{
								PdfViewer.Pdf = treePdf.Pdf;
								PdfViewer.Print(printerSettings);
							}
						}
					}
					else
					{
						ProgressInfo obj5 = new ProgressInfo
						{
							MainCaption = "正在打印" + treeDoc.Name + "..."
						};
						int num = current + 1;
						current = num;
						obj5.MainProgress = num / totalCount;
						progress.Report(obj5);
						await Task.Delay(100);
						if (ProjectHierarchy.FindAndSelectNode(treeDoc))
						{
							PrintDocument pd = new PrintDocument
							{
								DocumentName = treeDoc.Name,
								PrinterSettings = printerSettings
							};
							CurrentDocumentEditor.Print(pd);
						}
					}
				}
				catch (Exception ex)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, node.Name + "打印失败！失败原因：" + ex.Message);
				}
			}
			return Task.FromResult(new object());
		});
		progressForm.ShowDialog();
		await progressForm.Task;
	}

	private string ContactNumberAndName(string number, string name)
	{
		if (string.IsNullOrEmpty(number))
		{
			return name;
		}
		return number + " " + name;
	}

	public async Task BatchExport(string functionDescription)
	{
		if (!SoftwareLicenseManager.IsCurrentLiceneseAllowBatchExport(functionDescription))
		{
			return;
		}
		frmNodeSelectorWithTicketRecord frm = new frmNodeSelectorWithTicketRecord();
		frm.Project = CurrentProject;
		if (frm.ShowBatchExporter() != DialogResult.OK)
		{
			return;
		}
		if (SoftwareLicenseManager.IsBatchExportOutOfLicenseLimit(frm.Selected.Count))
		{
			return;
		}
		if (frm.Selected.Any((TreeNodeBase n) => n is TreeDocumentNode treeDocumentNode2 && DocumentEditors.TryGetValue(treeDocumentNode2.Document, out var value) && value.NeedSave))
		{
			if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "选择导出的文件需保存后方可导出，需要保存后导出吗？", MessageBoxButtons.OKCancel) != DialogResult.OK)
			{
				return;
			}
			await SaveProject(CurrentProject);
		}
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string rootPath = folderBrowserDialog.SelectedPath;
		rootPath = standardPath(rootPath);
		ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> progress)
		{
			await Task.Delay(10);
			int totalCount = frm.Selected.Count;
			int current = 0;
			Node[] nodes = frm.Grid.Nodes;
			foreach (Node node2 in nodes)
			{
				await forEachNodes(node2, rootPath);
			}
			return Task.FromResult(new object());
			async Task forEachNodes(Node node, string path)
			{
				if (frm.Grid.GetCellCheck(node.Row.Index, 1) == CheckEnum.Checked)
				{
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
					try
					{
						object key = node.Key;
						if (!(key is frmNodeSelectorWithTicketRecord.TicketNavTreePageNode ticketNavTreePageNode))
						{
							if (!(key is frmNodeSelectorWithTicketRecord.TicketNavTreeFolderNode ticketNavTreeFolderNode))
							{
								if (!(key is Leqisoft.Model.TreeGroup treeGroup))
								{
									if (!(key is TreeDirectoryNode treeDirectoryNode))
									{
										if (key is TreeTableNode treeTable)
										{
											string tableFile2 = Path.Combine(path, standardFile(ContactNumberAndName(treeTable.Number, treeTable.Name) + ".xlsx"));
											path = Path.Combine(path, standardPath(treeTable.Name));
											Node[] nodes2 = node.Nodes;
											foreach (Node node3 in nodes2)
											{
												await forEachNodes(node3, path);
											}
											progress.Report(new ProgressInfo
											{
												MainCaption = "正在导出" + treeTable.Name + "...",
												MainProgress = (int)(100.0 * (double)(++current) / (double)totalCount)
											});
											Application.DoEvents();
											await exportTable(treeTable, nonRepetitiveFile(tableFile2));
											return;
										}
										if (key is TreeDocumentNode treeDocumentNode)
										{
											progress.Report(new ProgressInfo
											{
												MainCaption = "正在导出" + treeDocumentNode.Name + "...",
												MainProgress = (int)(100.0 * (double)(++current) / (double)totalCount)
											});
											Application.DoEvents();
											string file2 = Path.Combine(path, standardFile(ContactNumberAndName(treeDocumentNode.Number, treeDocumentNode.Name) + ".docx"));
											exportDoc(treeDocumentNode, nonRepetitiveFile(file2));
											return;
										}
										if (key is TreeImageNode treeImageNode)
										{
											progress.Report(new ProgressInfo
											{
												MainCaption = "正在导出" + treeImageNode.Name + "...",
												MainProgress = (int)(100.0 * (double)(++current) / (double)totalCount)
											});
											Application.DoEvents();
											string file3 = Path.Combine(path, standardFile(ContactNumberAndName(treeImageNode.Number, treeImageNode.Name) ?? ""));
											exportImage(treeImageNode, nonRepetitiveFile(file3));
											return;
										}
										if (key is TreePdfNode treePdfNode)
										{
											progress.Report(new ProgressInfo
											{
												MainCaption = "正在导出" + treePdfNode.Name + "...",
												MainProgress = (int)(100.0 * (double)(++current) / (double)totalCount)
											});
											Application.DoEvents();
											string file4 = Path.Combine(path, standardFile(ContactNumberAndName(treePdfNode.Number, treePdfNode.Name) + ".pdf"));
											exportPdf(treePdfNode, nonRepetitiveFile(file4));
											return;
										}
										if (key is frmNodeSelectorWithTicketRecord.TicketNavTreeRecordNode ticketNavTreeRecordNode)
										{
											progress.Report(new ProgressInfo
											{
												MainCaption = "正在导出" + ticketNavTreeRecordNode.Name + "...",
												MainProgress = (int)(100.0 * (double)(++current) / (double)totalCount)
											});
											Application.DoEvents();
											string file5 = Path.Combine(path, standardFile(ticketNavTreeRecordNode.Name + ".xlsx"));
											exportTicketRecord(ticketNavTreeRecordNode.TableNode, ticketNavTreeRecordNode.Record, nonRepetitiveFile(file5));
											return;
										}
									}
									else
									{
										path = Path.Combine(path, standardPath(ContactNumberAndName(treeDirectoryNode.Number, treeDirectoryNode.Name)));
									}
								}
								else
								{
									path = Path.Combine(path, standardPath(treeGroup.Name));
								}
							}
							else if (!frm.SameDirExcelSaveToOneFile || !node.Nodes.Any((Node n) => n.Key is frmNodeSelectorWithTicketRecord.TicketNavTreeRecordNode))
							{
								path = Path.Combine(path, standardPath(ticketNavTreeFolderNode.Name));
							}
						}
						else if (!frm.SameDirExcelSaveToOneFile || !node.Nodes.Any((Node n) => n.Key is frmNodeSelectorWithTicketRecord.TicketNavTreeRecordNode))
						{
							path = ((frm.Grid.GetCellCheck(node.Parent.Row.Index, 1) == CheckEnum.Checked) ? Path.Combine(path, standardPath(ticketNavTreePageNode.Name)) : Path.Combine(path, standardPath(ticketNavTreePageNode.ParentNode.Name), standardPath(ticketNavTreePageNode.Name)));
						}
						else if (frm.Grid.GetCellCheck(node.Parent.Row.Index, 1) != CheckEnum.Checked)
						{
							path = Path.Combine(path, standardPath(ticketNavTreePageNode.ParentNode.Name));
						}
					}
					catch (Exception ex2)
					{
						string text = "";
						try
						{
							text = frm.Grid.GetDataDisplay(node.Row.Index, 0);
						}
						catch (Exception)
						{
						}
						ex2.Log("批量导出文件时发生了未预期的异常，节点名称: " + text);
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text + "导出失败！失败原因：" + ex2.Message);
					}
				}
				if (frm.SameDirExcelSaveToOneFile)
				{
					IEnumerable<Node> enumerable = node.Nodes.Where((Node n) => n.Key is TreeTableNode);
					foreach (Node item in enumerable)
					{
						TreeTableNode treeTableNode = item.Key as TreeTableNode;
						string tableFile2 = path;
						if (frm.Grid.GetCellCheck(item.Row.Index, 1) == CheckEnum.Checked)
						{
							tableFile2 = Path.Combine(path, standardPath(ContactNumberAndName(treeTableNode.Number, treeTableNode.Name) ?? ""));
						}
						Node[] nodes2 = item.Nodes;
						foreach (Node node4 in nodes2)
						{
							await forEachNodes(node4, tableFile2);
						}
					}
					IEnumerable<frmNodeSelectorWithTicketRecord.TicketNavTreeRecordNode> source = from n in node.Nodes
						where frm.Grid.GetCellCheck(n.Row.Index, 1) == CheckEnum.Checked && n.Key is frmNodeSelectorWithTicketRecord.TicketNavTreeRecordNode
						select n.Key as frmNodeSelectorWithTicketRecord.TicketNavTreeRecordNode;
					if (source.Count() > 0)
					{
						_ = string.Empty;
						if (!(node.Key is frmNodeSelectorWithTicketRecord.TicketNavTreeFolderNode { Name: var name }))
						{
							return;
						}
						try
						{
							progress.Report(new ProgressInfo
							{
								MainCaption = "正在导出" + name + "...",
								MainProgress = (int)(100.0 * (double)(current += source.Count()) / (double)totalCount)
							});
							Application.DoEvents();
							if (!Directory.Exists(path))
							{
								Directory.CreateDirectory(path);
							}
							List<Tuple<TicketTable, TicketRecord, string>> ticketRecordsList = source.Select((frmNodeSelectorWithTicketRecord.TicketNavTreeRecordNode t) => Tuple.Create(t.TableNode.Table.Ticket, t.Record, t.Name)).ToList();
							string file6 = Path.Combine(path, standardFile(name + ".xlsx"));
							TicketExportXlsx ticketExportXlsx2 = new TicketExportXlsx();
							ticketExportXlsx2.BatchExportToFile(nonRepetitiveFile(file6), ticketRecordsList);
						}
						catch (Exception ex4)
						{
							string text2 = "";
							try
							{
								text2 = frm.Grid.GetDataDisplay(node.Row.Index, 0);
							}
							catch (Exception)
							{
							}
							ex4.Log("批量导出文件时发生了未预期的异常，节点名称: " + text2);
							Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text2 + "导出失败！失败原因：" + ex4.Message);
						}
					}
					IEnumerable<TreeTableNode> source2 = from n in node.Nodes
						where frm.Grid.GetCellCheck(n.Row.Index, 1) == CheckEnum.Checked && n.Key is TreeTableNode treeTableNode2 && treeTableNode2.Table.LocalExists && !treeTableNode2.Table.LoadAndReturn().IsCorrupted
						select n.Key as TreeTableNode;
					if (source2.Count() > 0)
					{
						_ = string.Empty;
						string text3;
						if (node.Key is Leqisoft.Model.TreeGroup treeGroup2)
						{
							text3 = treeGroup2.Name;
						}
						else
						{
							if (!(node.Key is TreeDirectoryNode treeDirectoryNode2))
							{
								return;
							}
							text3 = ContactNumberAndName(treeDirectoryNode2.Number, treeDirectoryNode2.Name) ?? "";
						}
						try
						{
							progress.Report(new ProgressInfo
							{
								MainCaption = "正在导出" + text3 + "...",
								MainProgress = (int)(100.0 * (double)(current += source2.Count()) / (double)totalCount)
							});
							Application.DoEvents();
							if (!Directory.Exists(path))
							{
								Directory.CreateDirectory(path);
							}
							List<Tuple<Leqisoft.Model.Table, PageSetup>> list = source2.Select((TreeTableNode t) => Tuple.Create(t.Table, t.Table.PageSetup)).ToList();
							string tableFile2 = Path.Combine(path, standardFile(text3 + ".xlsx"));
							ReportExportToExcel.BatchExportToFile(nonRepetitiveFile(tableFile2), list);
							foreach (Tuple<Leqisoft.Model.Table, PageSetup> item2 in list)
							{
								await ExportTableAttachmentsAsync(item2.Item1, tableFile2);
							}
						}
						catch (Exception ex6)
						{
							string text4 = "";
							try
							{
								text4 = frm.Grid.GetDataDisplay(node.Row.Index, 0);
							}
							catch (Exception)
							{
							}
							ex6.Log("批量导出文件时发生了未预期的异常，节点名称: " + text4);
							Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text4 + "导出失败！失败原因：" + ex6.Message);
						}
					}
					IEnumerable<Node> enumerable2 = node.Nodes.Where((Node n) => !(n.Key is TreeTableNode) && !(n.Key is frmNodeSelectorWithTicketRecord.TicketNavTreeRecordNode));
					foreach (Node item3 in enumerable2)
					{
						await forEachNodes(item3, path);
					}
				}
				else
				{
					Node[] nodes2 = node.Nodes;
					foreach (Node node5 in nodes2)
					{
						await forEachNodes(node5, path);
					}
				}
			}
		});
		progressForm.ShowDialog();
		await progressForm.Task;
		static void exportDoc(TreeDocumentNode _treeDoc, string _fullPath)
		{
			try
			{
				_treeDoc.Document.LoadAndReturn();
				if (_treeDoc.Document.LocalExists && !_treeDoc.Document.IsCorrupted)
				{
					ReportExportToWord2 reportExportToWord = new ReportExportToWord2
					{
						Document = _treeDoc.Document
					};
					reportExportToWord.Save(_fullPath);
				}
			}
			catch (Exception exception4)
			{
				exception4.Log();
			}
		}
		static async void exportImage(TreeImageNode _treeImage, string _fullPath)
		{
			try
			{
				Guid fileId2 = _treeImage.Image.LoadAndReturn().FileId;
				FileCacheManager fcm2 = _treeImage.Project.FileCacheManager;
				await fcm2.DownloadIfNotExist(fileId2);
				fcm2.GetPath(fileId2);
				System.Drawing.Image graphicsImage = _treeImage.Image.GetGraphicsImage();
				graphicsImage.Save(_fullPath + getExtension(graphicsImage));
			}
			catch (Exception exception3)
			{
				exception3.Log();
			}
		}
		static async void exportPdf(TreePdfNode _treePdf, string _fullPath)
		{
			try
			{
				Guid fileId = _treePdf.Pdf.LoadAndReturn().FileId;
				FileCacheManager fcm = _treePdf.Project.FileCacheManager;
				await fcm.DownloadIfNotExist(fileId);
				string path2 = fcm.GetPath(fileId);
				File.Copy(path2, _fullPath, overwrite: true);
			}
			catch (Exception exception2)
			{
				exception2.Log();
			}
		}
		async Task exportTable(TreeTableNode _treeTable, string _fullPath)
		{
			try
			{
				_treeTable.Table.LoadAndReturn();
				if (_treeTable.Table.LocalExists && !_treeTable.Table.IsCorrupted)
				{
					_treeTable.Table.CalculateRecursive();
					if (_treeTable.Table.Rows.Count > 65535)
					{
						using BigExcelExporter bigExcelExporter = new BigExcelExporter();
						bigExcelExporter.Table = _treeTable.Table;
						bigExcelExporter.Export(_fullPath);
					}
					else
					{
						ReportExportToExcel reportExportToExcel = new ReportExportToExcel
						{
							Table = _treeTable.Table,
							PageSetup = _treeTable.Table.PageSetup,
							WaterMarkPageSetup = null
						};
						ExcelContex excelContex = new ExcelContex();
						reportExportToExcel.SaveValue(_fullPath, excelContex);
						reportExportToExcel.SetFormula(excelContex);
						reportExportToExcel.Save(excelContex);
					}
					await ExportTableAttachmentsAsync(_treeTable.Table, _fullPath);
				}
			}
			catch (Exception exception5)
			{
				exception5.Log();
			}
		}
		static void exportTicketRecord(TreeTableNode tableNode, TicketRecord ticketRecord, string fullPath)
		{
			try
			{
				TicketExportXlsx ticketExportXlsx = new TicketExportXlsx();
				ticketExportXlsx.Ticket = tableNode.Table.Ticket;
				ticketExportXlsx.VM = new TicketInputTableVM(ticketExportXlsx.Ticket, ticketRecord);
				ticketExportXlsx.VM.CalculateTicket();
				ticketExportXlsx.Generate();
				ticketExportXlsx.Save(fullPath);
			}
			catch (Exception exception)
			{
				exception.Log();
			}
		}
		static string getExtension(System.Drawing.Image image)
		{
			if (image.RawFormat.Equals(ImageFormat.Bmp))
			{
				return ".bmp";
			}
			if (image.RawFormat.Equals(ImageFormat.Gif))
			{
				return ".gif";
			}
			if (image.RawFormat.Equals(ImageFormat.Jpeg))
			{
				return ".jpeg";
			}
			if (image.RawFormat.Equals(ImageFormat.Png))
			{
				return ".png";
			}
			if (image.RawFormat.Equals(ImageFormat.Tiff))
			{
				return ".tiff";
			}
			if (image.RawFormat.Equals(ImageFormat.Icon))
			{
				return ".icon";
			}
			if (image.RawFormat.Equals(ImageFormat.Emf))
			{
				return ".emf";
			}
			if (image.RawFormat.Equals(ImageFormat.Exif))
			{
				return ".exif";
			}
			if (image.RawFormat.Equals(ImageFormat.Wmf))
			{
				return ".wmf";
			}
			return ".jpg";
		}
		static string nonRepetitiveFile(string file)
		{
			int num = 1;
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
			string directoryName = Path.GetDirectoryName(file);
			string extension = Path.GetExtension(file);
			while (File.Exists(file))
			{
				file = Path.Combine(directoryName, $"{fileNameWithoutExtension}({num++}){extension}");
			}
			return file;
		}
		static string standardFile(string fileName)
		{
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			foreach (char c in invalidFileNameChars)
			{
				fileName = fileName.Replace(c.ToString(), "");
			}
			fileName = fileName.Replace("、", "");
			return fileName;
		}
		static string standardPath(string path)
		{
			char[] invalidPathChars = Path.GetInvalidPathChars();
			foreach (char c2 in invalidPathChars)
			{
				path = path.Replace(c2.ToString(), "");
			}
			path = path.Replace("、", "");
			return path;
		}
	}

	public void ShowHideNodes(int mode)
	{
		TreeNodeBase selectedNode = ProjectHierarchy.SelectedNode;
		frmNodeSelector frmNodeSelector2 = new frmNodeSelector();
		frmNodeSelector2.Project = CurrentProject;
		if (frmNodeSelector2.ShowHideNode(mode) != DialogResult.OK)
		{
			return;
		}
		foreach (TreeNodeBase item in frmNodeSelector2.Selected)
		{
			item.UpdateVisible(v: true);
		}
		foreach (TreeNodeBase item2 in frmNodeSelector2.Unselected)
		{
			item2.UpdateVisible(v: false);
		}
		ProjectHierarchy.Populate();
		if (selectedNode != null)
		{
			if (selectedNode.Visible)
			{
				ProjectHierarchy.FindAndSelectNode(selectedNode);
			}
			else
			{
				SwitchToEmptyView();
			}
		}
	}

	private void ThemeEditor_SelectedThemeChanged(object sender, LeqiTheme theme)
	{
		if (theme == null)
		{
			return;
		}
		FormulaEditor.SuspendSourceCellPanelDrawing();
		try
		{
			Theme.SetCurrentTree(View);
			Browser?.SetTheme();
			MultiLedgerViewer.SetTheme();
			if (theme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
			{
				ImageProcess.SetImageStrategy(new WhiteImageStrategy());
				ImageProcess.ProcessImage();
				WhiteImageStrategy whiteImageStrategy = new WhiteImageStrategy();
				SyncTwinkle.UpdateOrignImage(whiteImageStrategy.ProcessImage(Resources.SyncProject_S));
				SyncTwinkle.UpdateTwinkleImage(whiteImageStrategy.ProcessImage(Resources.SyncProject_S));
			}
			else
			{
				ImageProcess.SetImageStrategy(new DefaultImageStrategy());
				ImageProcess.ProcessImage();
				SyncTwinkle.UpdateOrignImage(Resources.SyncProject_S);
				SyncTwinkle.UpdateTwinkleImage(Resources.SyncProject_S);
			}
			ThemeManager.GetInstance().ApplyTheme();
			View.Update();
		}
		finally
		{
			FormulaEditor.ResumeSourceCellPanelDrawing();
		}
	}

	private void UpdateTableValidationCache()
	{
		FormulaEvaluator.ClearCache();
		TableValidationResults.Clear();
		IEnumerable<ValidationResult> enumerable = ValidationResultCache.SelectMany((KeyValuePair<Id64, List<ValidationResult>> kv) => kv.Value);
		foreach (ValidationResult item in enumerable)
		{
			if (item.Refs == null)
			{
				continue;
			}
			foreach (Leqisoft.Model.Cell cellReference in item.Refs.CellReferences)
			{
				if (!cellReference.IsExisting || item.HasWildcard)
				{
					continue;
				}
				TreeTableNode treeNode = cellReference.Column.Table.TreeNode;
				if (!treeNode.Visible)
				{
					continue;
				}
				if (!TableValidationResults.ContainsKey(treeNode))
				{
					TableValidationResults.Add(treeNode, new TableValidationInfo());
				}
				TableValidationInfo tableValidationInfo = TableValidationResults[treeNode];
				bool flag = false;
				foreach (Tuple<Leqisoft.Model.Cell, ValidationResult> cell3 in tableValidationInfo.Cells)
				{
					if (cell3.Item1 == cellReference && cell3.Item2 != null && cell3.Item2.Source == item.Source)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					tableValidationInfo.Cells.Add(Tuple.Create(cellReference, item));
					if (!item.Passed)
					{
						tableValidationInfo.ErrorRefs.Add(cellReference);
					}
				}
			}
			foreach (RangeOperand rangeReference in item.Refs.RangeReferences)
			{
				if (item.HasWildcard)
				{
					continue;
				}
				TreeTableNode treeNode2 = rangeReference.Table.TreeNode;
				if (treeNode2.Visible)
				{
					if (!TableValidationResults.ContainsKey(treeNode2))
					{
						TableValidationResults.Add(treeNode2, new TableValidationInfo());
					}
					TableValidationInfo tableValidationInfo2 = TableValidationResults[treeNode2];
					tableValidationInfo2.Ranges.Add(Tuple.Create(rangeReference, item));
					if (!item.Passed)
					{
						tableValidationInfo2.ErrorRefs.Add(rangeReference);
					}
				}
			}
			foreach (Leqisoft.Model.Column columnReference in item.Refs.ColumnReferences)
			{
				if (item.HasWildcard)
				{
					continue;
				}
				TreeTableNode treeNode3 = columnReference.Table.TreeNode;
				if (treeNode3.Visible)
				{
					if (!TableValidationResults.ContainsKey(treeNode3))
					{
						TableValidationResults.Add(treeNode3, new TableValidationInfo());
					}
					TableValidationInfo tableValidationInfo3 = TableValidationResults[treeNode3];
					tableValidationInfo3.Columns.Add(Tuple.Create(columnReference, item));
					if (!item.Passed)
					{
						tableValidationInfo3.ErrorRefs.Add(columnReference);
					}
				}
			}
			foreach (Leqisoft.Model.Column columnWildcardReference in item.Refs.ColumnWildcardReferences)
			{
				TreeTableNode treeNode4 = columnWildcardReference.Table.TreeNode;
				if (!treeNode4.Visible)
				{
					continue;
				}
				if (!TableValidationResults.ContainsKey(treeNode4))
				{
					TableValidationResults.Add(treeNode4, new TableValidationInfo());
				}
				TableValidationInfo tableValidationInfo4 = TableValidationResults[treeNode4];
				if (item.RowIndex < columnWildcardReference.Table.Rows.Count)
				{
					Leqisoft.Model.Cell cell = columnWildcardReference.Table[item.RowIndex, columnWildcardReference.Index];
					tableValidationInfo4.Cells.Add(Tuple.Create(cell, item));
					if (!item.Passed)
					{
						tableValidationInfo4.ErrorRefs.Add(cell);
					}
				}
			}
			foreach (Leqisoft.Model.Cell headerCellWildcardReference in item.Refs.HeaderCellWildcardReferences)
			{
				Leqisoft.Model.Table table = headerCellWildcardReference.Column.Table;
				TreeTableNode treeNode5 = table.TreeNode;
				if (!treeNode5.Visible)
				{
					continue;
				}
				if (!TableValidationResults.ContainsKey(treeNode5))
				{
					TableValidationResults.Add(treeNode5, new TableValidationInfo());
				}
				TableValidationInfo tableValidationInfo5 = TableValidationResults[treeNode5];
				if (item.RowIndex < table.Rows.Count)
				{
					Leqisoft.Model.Cell cell2 = table[item.RowIndex, headerCellWildcardReference.Column.Index];
					tableValidationInfo5.Cells.Add(Tuple.Create(cell2, item));
					if (!item.Passed)
					{
						tableValidationInfo5.ErrorRefs.Add(cell2);
					}
				}
			}
			foreach (Leqisoft.Model.Cell headerCellReference in item.Refs.HeaderCellReferences)
			{
				if (item.HasWildcard)
				{
					continue;
				}
				TreeTableNode treeNode6 = headerCellReference.Column.Table.TreeNode;
				if (treeNode6.Visible)
				{
					if (!TableValidationResults.ContainsKey(treeNode6))
					{
						TableValidationResults.Add(treeNode6, new TableValidationInfo());
					}
					TableValidationInfo tableValidationInfo6 = TableValidationResults[treeNode6];
					tableValidationInfo6.HeaderCells.Add(Tuple.Create(headerCellReference, item));
					if (!item.Passed)
					{
						tableValidationInfo6.ErrorRefs.Add(headerCellReference);
					}
				}
			}
			foreach (Tuple<Leqisoft.Model.Table, int, int> titleReference in item.Refs.TitleReferences)
			{
				TreeTableNode treeNode7 = titleReference.Item1.TreeNode;
				if (treeNode7.Visible)
				{
					if (!TableValidationResults.ContainsKey(treeNode7))
					{
						TableValidationResults.Add(treeNode7, new TableValidationInfo());
					}
					TableValidationInfo tableValidationInfo7 = TableValidationResults[treeNode7];
					tableValidationInfo7.Titles.Add(Tuple.Create(titleReference.Item1, titleReference.Item2, titleReference.Item3, item));
					if (!item.Passed)
					{
						tableValidationInfo7.ErrorRefs.Add(titleReference);
					}
				}
			}
			foreach (Tuple<Leqisoft.Model.Table, int, int> footReference in item.Refs.FootReferences)
			{
				TreeTableNode treeNode8 = footReference.Item1.TreeNode;
				if (treeNode8.Visible)
				{
					if (!TableValidationResults.ContainsKey(treeNode8))
					{
						TableValidationResults.Add(treeNode8, new TableValidationInfo());
					}
					TableValidationInfo tableValidationInfo8 = TableValidationResults[treeNode8];
					tableValidationInfo8.Foots.Add(Tuple.Create(footReference.Item1, footReference.Item2, footReference.Item3, item));
					if (!item.Passed)
					{
						tableValidationInfo8.ErrorRefs.Add(footReference);
					}
				}
			}
		}
	}

	public void SwitchMainView()
	{
		pnlLedger.Width = 0;
		pnlMain.Width = View.Width;
		_ispnlMainShow = false;
		HideRelatedLedgerTip();
		AppCommands.Undo.Visible = true;
		AppCommands.Redo.Visible = true;
		State.SelectedTab?.Select();
	}

	public void SwitchFinanceView()
	{
		pnlMain.Width = 0;
		pnlLedger.Width = View.Width;
		_ispnlMainShow = true;
		if (CurrentLedgerViewer == null)
		{
			MultiLedgerViewer.LedgerDefaultPanel.Populate();
		}
		else
		{
			CurrentLedgerViewer.GetMainView().Focus();
		}
		MultiLedgerViewer.LedgerDefaultPanel.GetTileControl.Update();
		ShowRelatedLedgerTip(CurrentProject);
		TableEditor?._ttpComment.Hide();
		AppCommands.Undo.Visible = false;
		AppCommands.Redo.Visible = false;
		AppCommandTabs.Ledger.Select();
	}

	public void NodesIndexEdit()
	{
		FormNodesIndexEdit formNodesIndexEdit = new FormNodesIndexEdit();
		formNodesIndexEdit.Project = Leqisoft.Model.Project.Current;
		if (formNodesIndexEdit.ShowDialog() == DialogResult.OK)
		{
			ProjectHierarchy.Invalidate();
		}
	}

	private async Task ChangeProject()
	{
		MemberManager manager = MemberManager.GetInstance();
		try
		{
			await ChatManager.UpdateProjectMember(CurrentProject.Id);
		}
		catch (Exception)
		{
		}
		if (ChatManager.ChatForm.IsValueCreated)
		{
			ChatForm value = ChatManager.ChatForm.Value;
			Group group = manager.GetGroup(Leqisoft.Model.User.Current.TeamId.ToString());
			Group group2 = manager.GetGroup(SignalRClient.UserState.ProjectId);
			value.TeamMemberViewEditor.Populate(group?.GetSelfAndMembers());
			value.ProjectMemberViewEditor.Populate(group2?.GetSelfAndMembers());
		}
		TabMember?.Populate();
	}

	private void GotoErrorRef(object errorRef)
	{
		if (!(errorRef is Leqisoft.Model.Cell cell))
		{
			if (!(errorRef is RangeOperand rangeOperand))
			{
				if (errorRef is Leqisoft.Model.Column column)
				{
					if (column.Table != CurrentTable)
					{
						ProjectHierarchy.FindAndSelectNode(column.Table.TreeNode);
					}
					TableEditor.SelectColumn(column.Index);
				}
			}
			else
			{
				if (rangeOperand.Table != CurrentTable)
				{
					ProjectHierarchy.FindAndSelectNode(rangeOperand.Table.TreeNode);
				}
				TableEditor.Select(rangeOperand.TopLeft.Row.Index, rangeOperand.TopLeft.Column.Index, rangeOperand.BottomRight.Row.Index, rangeOperand.BottomRight.Column.Index);
			}
		}
		else
		{
			if (cell.Column.Table != CurrentTable)
			{
				ProjectHierarchy.FindAndSelectNode(cell.Column.Table.TreeNode);
			}
			TableEditor.Select(cell.Row.Index, cell.Column.Index);
		}
	}

	private object GetNextErrorRef(TreeTableNode tableNode)
	{
		if (tableNode == null)
		{
			return null;
		}
		try
		{
			if (TableValidationResults.TryGetValue(tableNode, out var value))
			{
				if (_currentErrorIndex < -1)
				{
					_currentErrorIndex = -1;
				}
				_currentErrorIndex++;
				if (_currentErrorIndex < value.ErrorRefs.Count)
				{
					return value.ErrorRefs[_currentErrorIndex];
				}
				int currentErrorIndex = _currentErrorIndex;
				_currentErrorIndex = -1;
				TreeTableNode tableNode2 = CurrentProject.GetAllTableNodes().SkipWhile((TreeTableNode n) => n != tableNode).ElementAtOrDefault(1);
				object nextErrorRef = GetNextErrorRef(tableNode2);
				if (nextErrorRef == null)
				{
					_currentErrorIndex = currentErrorIndex;
					return null;
				}
				return nextErrorRef;
			}
			_currentErrorIndex = -1;
			TreeTableNode tableNode3 = CurrentProject.GetAllTableNodes().SkipWhile((TreeTableNode n) => n != tableNode).ElementAtOrDefault(1);
			return GetNextErrorRef(tableNode3);
		}
		catch (ArgumentOutOfRangeException)
		{
			return null;
		}
	}

	private object GetPreviousErrorRef(TreeTableNode tableNode)
	{
		if (tableNode == null)
		{
			return null;
		}
		if (TableValidationResults.TryGetValue(tableNode, out var value))
		{
			if (_currentErrorIndex > value.ErrorRefs.Count)
			{
				_currentErrorIndex = value.ErrorRefs.Count;
			}
			_currentErrorIndex--;
			if (_currentErrorIndex >= 0)
			{
				return value.ErrorRefs[_currentErrorIndex];
			}
			_currentErrorIndex = int.MaxValue;
			TreeTableNode tableNode2 = CurrentProject.GetAllTableNodes().Reverse().SkipWhile((TreeTableNode n) => n != tableNode)
				.ElementAtOrDefault(1);
			object previousErrorRef = GetPreviousErrorRef(tableNode2);
			if (previousErrorRef == null)
			{
				_currentErrorIndex = -1;
				return null;
			}
			return previousErrorRef;
		}
		_currentErrorIndex = int.MaxValue;
		TreeTableNode tableNode3 = CurrentProject.GetAllTableNodes().Reverse().SkipWhile((TreeTableNode n) => n != tableNode)
			.ElementAtOrDefault(1);
		return GetPreviousErrorRef(tableNode3);
	}

	private void ApplyConfig()
	{
		CurrentEdition.Ribbon.Minimized = UserSet.Config.HideTab;
		if (ctnAll.Panels.Contains(pnlLedger) && !SoftwareLicenseManager.IsLedgerModuleEnable())
		{
			ctnAll.Panels.Remove(pnlLedger);
			UpdateState(delegate
			{
			});
		}
		else if (!ctnAll.Panels.Contains(pnlLedger) && SoftwareLicenseManager.IsLedgerModuleEnable())
		{
			ctnAll.Panels.Insert(ctnAll.Panels.IndexOf(pnlMain), pnlLedger);
			UpdateState(delegate
			{
			});
		}
	}

	private async Task SkipToTreeNode(Tuple<Guid, long> tp)
	{
		if (CurrentProject.Id == tp.Item1)
		{
			try
			{
				_shouldRecord = false;
				TreeNodeBase nodeById = CurrentProject.GetNodeById(new Id64(tp.Item2));
				ProjectHierarchy.FindAndSelectNode(nodeById);
				return;
			}
			finally
			{
				_shouldRecord = true;
			}
		}
		Leqisoft.Model.Project project = await OpenOrSwitchToProject(tp.Item1);
		if (project == null)
		{
			return;
		}
		TreeNodeBase treeNodeBase = project.GetAllTreeNodes().FirstOrDefault((TreeNodeBase t) => t.Id.Value == tp.Item2);
		if (treeNodeBase == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "数据源文件不存在");
			return;
		}
		try
		{
			_shouldRecord = false;
			ProjectHierarchy.FindAndSelectNode(treeNodeBase);
		}
		finally
		{
			_shouldRecord = true;
		}
	}

	private void HideRelatedLedgerTip()
	{
		ttpRelated.Hide();
	}

	private void ShowRelatedLedgerTip(Leqisoft.Model.Project project)
	{
		if (openedRelate || project == null)
		{
			return;
		}
		string projectId = project.Id.ToString();
		Dictionary<string, DateTime> history = LedgerHistory2.OpenHistory.GetProject(projectId);
		if (history == null || history.Count == 0 || OpenedLedgerViewerDic.Any((KeyValuePair<string, LedgerViewer> k) => history.ContainsKey(k.Key)))
		{
			return;
		}
		string ledger = history.OrderBy((KeyValuePair<string, DateTime> h) => h.Value).Last().Key;
		if (ledger == null || !File.Exists(ledger))
		{
			return;
		}
		Tile tile = MultiLedgerViewer.LedgerDefaultPanel?.GetTileByTag(ledger);
		if (tile == null)
		{
			return;
		}
		ttpRelated.IsBalloon = true;
		ttpRelated.Duration = 5000;
		ttpRelated.LinkClicked += async delegate(object s1, object e1)
		{
			if (e1?.ToString() == "openledger")
			{
				await OpenLedger(ledger, userCache: true);
				openedRelate = true;
			}
		};
		XElement xElement = new XElement("p");
		xElement.Add(new XElement("span", new XAttribute("style", "color: red"), "是否打开当前" + StringConstBase.Current.Project + "相关联的账套？"), new XElement("a", new XAttribute("href", "openledger"), "打开关联账套"));
		ttpRelated.SetText("信息提示", xElement.ToString(), canClose: true);
		ttpRelated.SetTagDic(new Dictionary<string, object> { ["openledger"] = "openledger" });
		ttpRelated.Show(tile.TileControl, new Point(tile.Group.X + tile.X + tile.Width / 2, tile.Group.Y + tile.Y));
	}

	public async Task OpenLedger(string ledger, bool userCache)
	{
		await MultiLedgerViewer.OpenLedger(ledger, userCache);
	}

	public async Task MergeLedger()
	{
		if (!SoftwareLicenseManager.IsMergeLedgerOutOfLicenseLimit())
		{
			await MultiLedgerViewer.MergeLedger();
		}
	}

	private void AnimateStart(Control control, bool ToRight = true)
	{
		Point point = control.Parent.PointToScreen(control.Location);
		Bitmap bitmap = control.GetScreenshot();
		if (bitmap == null)
		{
			bitmap = new Bitmap(control.Width, control.Height);
			using Graphics graphics = Graphics.FromImage(bitmap);
			graphics.CopyFromScreen(point, new Point(0, 0), control.Size);
		}
		if (_animateForm.BackgroundImage != null)
		{
			_animateForm.BackgroundImage.Dispose();
		}
		_animateForm.BackgroundImage = bitmap;
		_animateForm.Location = point;
		_animateForm.Size = control.Size;
		_animateToRight = ToRight;
	}

	private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, HashSet<string> except)
	{
		DirectoryInfo[] directories = source.GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			if (!except.Contains(directoryInfo.Name))
			{
				CopyFilesRecursively(directoryInfo, target.CreateSubdirectory(directoryInfo.Name), except);
			}
		}
		System.IO.FileInfo[] files = source.GetFiles();
		foreach (System.IO.FileInfo fileInfo in files)
		{
			fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name));
		}
	}

	private void HideAllPnlContent()
	{
		TableEditor.TitleEditor.View.Enabled = false;
		foreach (Control control in pnlContent.Controls)
		{
			control.Hide();
		}
		HideNavigationPanel();
		TableEditor.TitleEditor.View.Enabled = true;
	}

	private void CheckProjectUpdate()
	{
		if (CurrentProject != null)
		{
			if (CurrentProject.ServerVersionOnOpen > CurrentProject.Version)
			{
				SyncTwinkle.Start();
				RibbonButton button = AppCommands.SyncProjectSmall.Button;
				Rectangle itemBounds = button.Ribbon.GetItemBounds(button);
				TooltipBox tooltipBox = new TooltipBox();
				tooltipBox.Duration = 5000;
				tooltipBox.IsBalloon = true;
				XElement xElement = new XElement("p", new XAttribute("style", "color: red;"), StringConstBase.Current.Project + "数据有更新，您可以请点击这里同步数据");
				tooltipBox.SetText("同步提示", xElement.ToString());
				tooltipBox.Show(button.Ribbon, new Point((itemBounds.Left + itemBounds.Right) / 2, itemBounds.Bottom));
			}
			else
			{
				SyncTwinkle.Stop();
			}
		}
		RefreshProjectsSyncTwinkle();
	}
}
