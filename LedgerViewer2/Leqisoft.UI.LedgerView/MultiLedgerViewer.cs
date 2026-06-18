using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1SplitContainer;
using C1.Win.C1Tile;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.LedgerView.Properties;

namespace Leqisoft.UI.LedgerView;

public class MultiLedgerViewer
{
	private readonly C1SplitContainer _spc;

	private readonly C1SplitterPanel _pnlExplorer;

	private readonly C1SplitterPanel _pnlList;

	private readonly C1SplitterPanel _pnlViewer;

	private readonly C1TileControlEx _tileList;

	private readonly Group _tileGroup;

	private readonly Template _template;

	public LedgerViewer CurrentLedgerViewer;

	public Dictionary<string, LedgerViewer> OpenedLedgerViewerDic = new Dictionary<string, LedgerViewer>();

	private bool shouldShowFillToTable = true;

	public C1SplitContainer View => _spc;

	public LedgerDefaultPanel LedgerDefaultPanel { get; }

	public bool IsShowToolBar { get; set; }

	public Project CurrentProject { get; set; }

	public Func<string, C1CommandLink[]> GetOnlineMemberContextMenu { get; set; }

	public event EventHandler<LedgerEventArgs> AfterOpenLedger;

	public event EventHandler<LedgerEventArgs> AfterCloseLedger;

	public event EventHandler<LedgerEventArgs> AfterLedgerDataChanged;

	public event EventHandler<LedgerSelectionChangedEventArgs> LedgerSelectionChanged;

	public event EventHandler<LedgerCollectEventArgs> AfterCollect;

	public event EventHandler<LedgerShareEventArgs> AfterShare;

	public event EventHandler LedgerNotFound;

	public MultiLedgerViewer()
	{
		_spc = new C1SplitContainer();
		_pnlExplorer = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			Collapsible = false,
			Resizable = false,
			SizeRatio = 100.0,
			MinWidth = 0
		};
		_spc.Panels.Add(_pnlExplorer);
		LedgerDefaultPanel = new LedgerDefaultPanel(this);
		_pnlExplorer.Controls.Add(LedgerDefaultPanel.GetView());
		LedgerDefaultPanel.GetTileControl.DoubleClickTile += LedgerDefaultPanel_DoubleClickTile;
		_pnlList = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			Collapsible = false,
			Resizable = true,
			Width = 160,
			KeepRelativeSize = false,
			MinWidth = 0
		};
		_spc.Panels.Add(_pnlList);
		_tileList = new C1TileControlEx
		{
			CellWidth = 10,
			CellHeight = 10,
			AllowChecking = false,
			CellSpacing = 20,
			Margin = new Padding(0),
			Padding = new Padding(0),
			GroupPadding = new Padding(0),
			Orientation = LayoutOrientation.Vertical,
			TileBorderColor = Color.Transparent,
			GroupSpacing = 5,
			ShowToolTips = false,
			AllowCloseButton = true
		};
		_pnlList.Controls.Add(_tileList);
		_template = CreateFileTemplate();
		_tileList.Templates.Add(_template);
		_tileList.Dock = DockStyle.Fill;
		_tileGroup = new Group();
		_tileList.Groups.Add(_tileGroup);
		_tileList.TileClicked += _tileList_TileClicked;
		_tileList.TileCloseClick += _tileList_TileCloseClick;
		_pnlViewer = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			Collapsible = false,
			Resizable = false,
			SizeRatio = 100.0,
			KeepRelativeSize = true,
			MinWidth = 0
		};
		_spc.Panels.Add(_pnlViewer);
	}

	public void OnAfterOpenLedger(LedgerEventArgs e)
	{
		this.AfterOpenLedger?.Invoke(this, e);
	}

	public void OnAfterCloseLedger(LedgerEventArgs e)
	{
		this.AfterCloseLedger?.Invoke(this, e);
	}

	public void OnAfterLedgerDataChanged(LedgerEventArgs e)
	{
		this.AfterLedgerDataChanged?.Invoke(this, e);
	}

	public void OnLedgerSelectionChanged(LedgerSelectionChangedEventArgs e)
	{
		this.LedgerSelectionChanged?.Invoke(this, e);
	}

	public void OnAfterCollect(LedgerCollectEventArgs e)
	{
		this.AfterCollect?.Invoke(this, e);
	}

	public void OnAfterShare(LedgerShareEventArgs e)
	{
		this.AfterShare?.Invoke(this, e);
	}

	public void OnLedgerNotFound()
	{
		this.LedgerNotFound?.Invoke(this, EventArgs.Empty);
	}

	public void ToLedgerPanel()
	{
		_pnlExplorer.Show();
		_pnlList.Hide();
		_pnlViewer.Hide();
	}

	public void ToLedgerViewer()
	{
		_pnlExplorer.Hide();
		_pnlList.Show();
		_pnlViewer.Show();
	}

	public void ShowFillToTable(bool isShow)
	{
		shouldShowFillToTable = isShow;
	}

	public async Task OpenLedger(string ledger, bool userCache)
	{
		if (!UserTeam.CurrentTeamIsPayByProject && User.Current.IsLicenseOutOfDate)
		{
			string text = $"尊敬的用户：\r\n您的产品已于{User.Current.LicenseDate:yyyy年MM月dd日}到期，建议您致电官方客服电话：400-690-6500，联系购买或续期！";
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text);
		}
		else
		{
			if (LedgerViewer.LicenseCheckHandleIsOpenedLedgerCountInLimit != null && !LedgerViewer.LicenseCheckHandleIsOpenedLedgerCountInLimit(OpenedLedgerViewerDic.Count + 1))
			{
				return;
			}
			if (Path.GetExtension(ledger) == ".001")
			{
				ledger = LedgerViewer.Parse001File(ledger);
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账套文件转换完成，保存在：" + ledger);
			}
			LedgerViewer viewer;
			if (userCache)
			{
				if (OpenedLedgerViewerDic.ContainsKey(ledger))
				{
					LedgerViewer ledgerViewer = (CurrentLedgerViewer = OpenedLedgerViewerDic[ledger]);
					OnAfterOpenLedger(new LedgerEventArgs
					{
						Viewer = ledgerViewer
					});
					ledgerViewer.BringToFront();
					ToLedgerViewer();
					return;
				}
				viewer = new LedgerViewer(this);
				if (await viewer.OpenLedger(ledger))
				{
					OpenedLedgerViewerDic.Add(ledger, viewer);
					C1SplitContainer mainView = viewer.GetMainView();
					mainView.Dock = DockStyle.Fill;
					_pnlViewer.Controls.Add(mainView);
					CurrentLedgerViewer = viewer;
					CurrentLedgerViewer.ShowFillToTable(shouldShowFillToTable);
					OnAfterOpenLedger(new LedgerEventArgs
					{
						Viewer = viewer
					});
					mainView.BringToFront();
					ToLedgerViewer();
				}
				return;
			}
			if (OpenedLedgerViewerDic.ContainsKey(ledger))
			{
				LedgerViewer ledgerViewer2 = OpenedLedgerViewerDic[ledger];
				_pnlViewer.Controls.Remove(ledgerViewer2.GetMainView());
				OpenedLedgerViewerDic.Remove(ledger);
				OnAfterOpenLedger(new LedgerEventArgs
				{
					Viewer = ledgerViewer2
				});
				ToLedgerViewer();
			}
			viewer = new LedgerViewer(this);
			if (await viewer.OpenLedger(ledger))
			{
				OpenedLedgerViewerDic.Add(ledger, viewer);
				C1SplitContainer mainView2 = viewer.GetMainView();
				mainView2.Dock = DockStyle.Fill;
				_pnlViewer.Controls.Add(mainView2);
				CurrentLedgerViewer = viewer;
				CurrentLedgerViewer.ShowFillToTable(shouldShowFillToTable);
				OnAfterOpenLedger(new LedgerEventArgs
				{
					Viewer = viewer
				});
				mainView2.BringToFront();
				ToLedgerViewer();
			}
		}
	}

	public async Task MergeLedger()
	{
		if (CurrentLedgerViewer == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
			return;
		}
		string previous = CurrentLedgerViewer.CurrentFilePath;
		string text = CurrentLedgerViewer.MergeLedger();
		if (text != null)
		{
			await OpenLedger(text, userCache: false);
			await CloseLedger(previous);
		}
	}

	public async Task CloseLedger(string file1)
	{
		if (file1 == null)
		{
			return;
		}
		if (!OpenedLedgerViewerDic.ContainsKey(file1))
		{
			PopulateOpenedLedgers();
			return;
		}
		LedgerViewer ledgerViewer = OpenedLedgerViewerDic[file1];
		OnAfterCloseLedger(new LedgerEventArgs
		{
			Viewer = ledgerViewer
		});
		ledgerViewer.Dispose();
		_pnlViewer.Controls.Remove(ledgerViewer.GetMainView());
		OpenedLedgerViewerDic.Remove(file1);
		if (file1 == CurrentLedgerViewer?.CurrentFilePath)
		{
			if (OpenedLedgerViewerDic.Count == 0)
			{
				CurrentLedgerViewer = null;
				LedgerDefaultPanel.BringToFront();
			}
			else
			{
				KeyValuePair<string, LedgerViewer> kv = OpenedLedgerViewerDic.First();
				if (File.Exists(kv.Key))
				{
					await OpenLedger(kv.Key, userCache: true);
					CurrentLedgerViewer = kv.Value;
				}
				else
				{
					CurrentLedgerViewer = null;
					LedgerDefaultPanel.BringToFront();
				}
			}
		}
		PopulateOpenedLedgers();
	}

	public void PopulateOpenedLedgers()
	{
		_tileGroup.Tiles.Clear();
		Bitmap ledger = Resources.ledger2;
		Bitmap currentLedger = Resources.currentLedger;
		foreach (string key in OpenedLedgerViewerDic.Keys)
		{
			if (File.Exists(key))
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(key);
				string text = ((fileNameWithoutExtension.Length > 20) ? (fileNameWithoutExtension.Substring(0, 20) + "...") : fileNameWithoutExtension);
				Tile tile = new Tile
				{
					Template = _template,
					Text1 = text,
					Text = key,
					VerticalSize = 4,
					HorizontalSize = 5
				};
				_tileGroup.Tiles.Add(tile);
				tile.Image1 = ((key == CurrentLedgerViewer?.CurrentFilePath) ? currentLedger : ledger);
			}
		}
	}

	public bool IsLedgerEmpty()
	{
		if (CurrentLedgerViewer != null && OpenedLedgerViewerDic.Count != 0)
		{
			return OpenedLedgerViewerDic.All((KeyValuePair<string, LedgerViewer> v) => v.Value.Ledger == null);
		}
		return true;
	}

	public void ShowToolbar()
	{
		IsShowToolBar = true;
		foreach (KeyValuePair<string, LedgerViewer> item in OpenedLedgerViewerDic)
		{
			item.Value.ShowSideToolbar();
		}
		LedgerDefaultPanel.ShowSideToolbar();
	}

	public void HideToolbar()
	{
		IsShowToolBar = false;
		foreach (KeyValuePair<string, LedgerViewer> item in OpenedLedgerViewerDic)
		{
			item.Value.HideSideToolbar();
		}
		LedgerDefaultPanel.HideSideToolbar();
	}

	public void SetTheme()
	{
		_tileList.TileBorderColor = Color.Transparent;
		foreach (KeyValuePair<string, LedgerViewer> item in OpenedLedgerViewerDic)
		{
			item.Value.SetTheme();
		}
	}

	private Template CreateFileTemplate()
	{
		Template template = new Template();
		template.Description = "Win32";
		PanelElement panelElement = new PanelElement();
		panelElement.Alignment = ContentAlignment.TopRight;
		panelElement.FixedHeight = 30;
		panelElement.FixedWidth = 40;
		panelElement.Margin = new Padding(0, 3, 3, 0);
		ImageElement imageElement = new ImageElement();
		imageElement.AlignmentOfContents = ContentAlignment.TopRight;
		imageElement.ColumnIndex = 30;
		imageElement.FixedWidth = 40;
		imageElement.ImageSelector = ImageSelector.Image2;
		panelElement.Children.Add(imageElement);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.FixedHeight = 40;
		panelElement2.FixedWidth = 130;
		panelElement2.Alignment = ContentAlignment.TopCenter;
		TextElement textElement = new TextElement();
		textElement.AlignmentOfContents = ContentAlignment.TopCenter;
		textElement.TextTrimming = TextTrimming.EndEllipsis;
		textElement.SingleLine = false;
		textElement.FixedHeight = 40;
		textElement.FixedWidth = 130;
		textElement.Margin = new Padding(0, 5, 0, 0);
		textElement.TextSelector = TextSelector.Text1;
		textElement.ForeColorSelector = ForeColorSelector.ForeColor1;
		panelElement2.Children.Add(textElement);
		PanelElement panelElement3 = new PanelElement();
		panelElement3.FixedHeight = 50;
		panelElement3.FixedWidth = 50;
		panelElement3.Margin = new Padding(0, 28, 0, 0);
		panelElement3.Alignment = ContentAlignment.TopCenter;
		ImageElement imageElement2 = new ImageElement();
		imageElement2.AlignmentOfContents = ContentAlignment.TopCenter;
		imageElement2.FixedHeight = 50;
		imageElement2.FixedWidth = 50;
		imageElement2.ImageSelector = ImageSelector.Image1;
		panelElement3.Children.Add(imageElement2);
		PanelElement panelElement4 = new PanelElement();
		panelElement4.FixedHeight = 40;
		panelElement4.FixedWidth = 130;
		panelElement4.Alignment = ContentAlignment.BottomCenter;
		TextElement textElement2 = new TextElement();
		textElement2.TextSelector = TextSelector.Text1;
		textElement2.AlignmentOfContents = ContentAlignment.TopCenter;
		textElement2.TextTrimming = TextTrimming.EndEllipsis;
		textElement2.SingleLine = false;
		textElement2.FixedHeight = 40;
		textElement2.FixedWidth = 130;
		panelElement4.Children.Add(textElement2);
		template.Elements.Add(panelElement);
		template.Elements.Add(panelElement2);
		template.Elements.Add(panelElement3);
		template.Elements.Add(panelElement4);
		template.Name = "mapImgTemplate";
		return template;
	}

	private void _tileList_TileClicked(object sender, TileEventArgs e)
	{
		try
		{
			string text = e.Tile.Text;
			if (OpenedLedgerViewerDic.ContainsKey(text))
			{
				LedgerViewer ledgerViewer = OpenedLedgerViewerDic[text];
				ledgerViewer.GetMainView().BringToFront();
				CurrentLedgerViewer = ledgerViewer;
			}
			PopulateOpenedLedgers();
		}
		catch (FileNotFoundException)
		{
			OnLedgerNotFound();
		}
		catch (Exception ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private async void _tileList_TileCloseClick(object sender, TileEventArgs e)
	{
		await CloseLedger(e.Tile.Text);
	}

	private async void LedgerDefaultPanel_DoubleClickTile(object sender, Tile e)
	{
		try
		{
			await OpenLedger((e.Tag as TileInfo).LocalFile, userCache: false);
		}
		catch (FileNotFoundException)
		{
			OnLedgerNotFound();
		}
		catch (Exception ex2)
		{
			ex2.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}
}
