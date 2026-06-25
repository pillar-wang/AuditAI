﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using C1.Win.C1Tile;
using Resources = global::CrawlerForm.Properties.Resources;
using DbAccess;
using Auditai.Model;

namespace CrawlerForm;

public class CrawlerForm : Form
{
	public const int WM_SYSCOMMAND = 274;

	public const int SC_MOVE = 61456;

	public const int HTCAPTION = 2;

	private readonly string CONFIGPATH = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config/config.json");

	private readonly string DIRECTORY = "./账套文件/";

	private const string logfile = "./Logs/log.txt";

	private const string bakPath = "./Logs/";

	private const string CN_COMPANY = "Company";

	private const string CN_INDEX = "Index";

	private const string CN_STATE = "State";

	private const string CN_CHECK = "Check";

	private const string CN_YEAR = "Year";

	private const string CN_NUM = "Num";

	private C1DockingTab dktMain;

	private C1DockingTabPage tabPageBrands;

	private C1DockingTabPage tabPageLedgers;

	private C1SplitterPanel pnlDataSource;

	private C1DockingTab dktDataSource;

	private C1DockingTabPage tabPageDatabase;

	private C1CheckBox cckNoSecretConnect;

	private ComboBox txtHostAddress;

	private TextBox txtUserName;

	private TextBox txtPassword;

	private C1DockingTabPage tabPageLocalFile;

	private C1TextBox txtLocalFile;

	private C1DockingTabPage tabPageOracle;

	private ComboBox cmbOracleSource;

	private TextBox txtOracleInstance;

	private TextBox txtOracleUserName;

	private TextBox txtOraclePassword;

	private C1FlexGrid grdLedgers;

	private C1TileControl tileBrands;

	private C1Button btnLocalFile;

	private Template _defaultTileTemplate;

	private List<CrawlerBase> Crawlers;

	private StatusEnum currentStatus = StatusEnum.ManualScanPrepare;

	private MainViewEnum currentMainView = MainViewEnum.Brand;

	private DataSourceViewEnum currentDataSourceView = DataSourceViewEnum.LocalFile;

	private Dictionary<LedgerInfo, CrawlerBase> LedgerInfoCrawler;

	private Pen _brdpen = new Pen(Color.FromArgb(0, 195, 245), 1f);

	private System.Windows.Forms.Timer _angleTimer = new System.Windows.Forms.Timer
	{
		Interval = 50
	};

	private Tile _currentSelectTile;

	private CrawlerBase _currentCrawler;

	private DatabaseInfo _lastDataBase;

	private Circle _circle;

	private int _angle = 0;

	private Pen _pen;

	private Bitmap _bmp;

	private Random _rdm = new Random();

	private Config Config = new Config();

	private Logger logger = new Logger("./Logs/log.txt");

	private List<IPAddress> lanAddress = new List<IPAddress>();

	private BackgroundWorker intelligenceScanWorker;

	private int currentIntelligenceScanThreadId = -1;

	private BackgroundWorker backWorker;

	private bool hasLoaded = false;

	private const int MS_EXCEPTION = -1;

	private const int MS_IO_EXCEPTION = -2;

	private const int MS_CRAWLER_EXCEPTION = -9;

	private const int MS_ERRORSOFT = -4;

	private const int MS_SCAN_COMPLETE = -5;

	private const int MS_CRAWLER_COMPLETE = -6;

	private const int MS_SCAN_MESSAGE = -7;

	private const int MS_CRAWLER_MESSAGE = -8;

	private const int MS_ANALYZY_COMPLETE = -10;

	private const int MS_ANALYZY_ING = -11;

	private bool _allowCheckTile = false;

	private bool _fileNoticeText = true;

	private IContainer components = null;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlHeader;

	private C1SplitterPanel pnlTabDock;

	private C1Label lblMainNotice;

	private C1Label lblTitle;

	private C1Button btnClose;

	private ProgressBar progressBar1;

	private C1Button btnScan;

	private C1Button btnMinBox;

	private C1Button btnBack;

	private C1PictureBox imgMainNotice;

	private C1PictureBox imgSubNotice;

	private C1PictureBox c1PictureBox1;

	private C1Label lblSubNotice;

	private C1Button btnCrawler;

	private Panel pnlMainNotice;

	private LinkLabel lnkException;

	private Color auditaiBlue => Color.FromArgb(0, 195, 245);

	private Color buttonGreen => Color.FromArgb(43, 213, 67);

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

	public CrawlerForm()
	{
		QueryExe();
		InitializeComponent();
		Initialize();
	}

	private void Initialize()
	{
		base.TopMost = false;
		DoubleBuffered = true;
		base.StartPosition = FormStartPosition.CenterScreen;
		btnBack.Visible = false;
		progressBar1.Visible = false;
		lblTitle.BackColor = Color.Transparent;
		lblTitle.ForeColor = Color.White;
		pnlMainNotice.BackColor = Color.Transparent;
		lnkException.BackColor = Color.Transparent;
		lnkException.LinkClicked += LnkException_LinkClicked;
		lblMainNotice.Font = new Font("微软雅黑", 12f);
		lblMainNotice.BackColor = Color.Transparent;
		lblMainNotice.ForeColor = Color.White;
		lblMainNotice.Font = new Font("微软雅黑", 15f);
		lblSubNotice.BackColor = Color.Transparent;
		lblSubNotice.ForeColor = Color.White;
		lblSubNotice.Font = new Font("微软雅黑", 10f);
		btnBack.BackColor = Color.Transparent;
		btnClose.BackColor = Color.Transparent;
		btnMinBox.BackColor = Color.Transparent;
		Color borderColor = Color.FromArgb(0, 155, 232);
		btnBack.FlatAppearance.BorderColor = borderColor;
		btnClose.FlatAppearance.BorderColor = borderColor;
		btnMinBox.FlatAppearance.BorderColor = borderColor;
		btnScan.Font = new Font("微软雅黑", 10.5f);
		btnScan.ForeColor = Color.White;
		btnScan.FlatStyle = FlatStyle.Flat;
		btnScan.FlatAppearance.BorderSize = 0;
		btnScan.BackColor = buttonGreen;
		btnScan.Text = "智能扫描";
		btnCrawler.Font = new Font("微软雅黑", 10.5f);
		btnCrawler.ForeColor = Color.White;
		btnCrawler.FlatStyle = FlatStyle.Flat;
		btnCrawler.FlatAppearance.BorderSize = 0;
		btnCrawler.BackColor = buttonGreen;
		btnCrawler.Text = "采集账套";
		btnCrawler.Visible = false;
		_circle = new Circle(imgMainNotice.Location.X, imgMainNotice.Location.Y - 20, 5f);
		Point anglePoint = _circle.GetAnglePoint(_angle);
		imgMainNotice.BackColor = Color.Transparent;
		imgSubNotice.BackColor = Color.Transparent;
		imgSubNotice.Location = anglePoint;
		imgMainNotice.Controls.Add(imgSubNotice);
		pnlHeader.Resizable = true;
		pnlHeader.Paint += pnlHeader_Paint;
		pnlHeader.MouseDown += CrawlerForm_MouseDown;
		_angleTimer.Tick += _angleTimer_Tick;
		_defaultTileTemplate = new Template();
		C1.Win.C1Tile.TextElement item = new C1.Win.C1Tile.TextElement
		{
			Font = new Font("微软雅黑", 9f),
			Margin = new Padding(0, 60, 0, 0),
			Alignment = ContentAlignment.TopCenter,
			AlignmentOfContents = ContentAlignment.TopCenter
		};
		C1.Win.C1Tile.ImageElement item2 = new C1.Win.C1Tile.ImageElement
		{
			Margin = new Padding(0, 20, 0, 0),
			Alignment = ContentAlignment.TopCenter
		};
		_defaultTileTemplate.Elements.Add(item);
		_defaultTileTemplate.Elements.Add(item2);
		Crawlers = new List<CrawlerBase>();
		LedgerInfoCrawler = new Dictionary<LedgerInfo, CrawlerBase>();
		Config.Load(CONFIGPATH);
		CrawlerBase.ScanProgressChanged += CrawlerBase_ScanProgressChanged;
		btnBack.Enabled = false;
		btnClose.Enabled = false;
		btnMinBox.Enabled = false;
		btnScan.Hide();
		btnCrawler.Enabled = false;
	}

	private void LnkException_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		if (lnkException.Tag is Exception ex)
		{
			ExceptionForm exceptionForm = new ExceptionForm();
			exceptionForm.ShowDialog((ex == null) ? "null" : ex.ToString());
		}
	}

	private void ShownInitialize()
	{
		InitializeDockTab();
		InitializeProgress();
		string location = Assembly.GetExecutingAssembly().Location;
		string directoryName = Path.GetDirectoryName(location);
		CrawlerInstanceFromDir(directoryName, delegate(List<CrawlerBase> crawlers)
		{
			Crawlers = crawlers;
			PopulateBrandList(Crawlers);
			PopulateLedgerList(null);
			SwitchMainViewTo(MainViewEnum.Brand);
			SwitchStatusTo(StatusEnum.IntelligencePrepare);
			btnBack.Enabled = true;
			btnClose.Enabled = true;
			btnMinBox.Enabled = true;
			btnScan.Enabled = true;
			btnCrawler.Enabled = true;
		});
	}

	private void InitializeDockTab()
	{
		cckNoSecretConnect = new C1CheckBox();
		cckNoSecretConnect.BackColor = SystemColors.Control;
		cckNoSecretConnect.BorderStyle = BorderStyle.None;
		cckNoSecretConnect.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		cckNoSecretConnect.ForeColor = SystemColors.ControlText;
		cckNoSecretConnect.Location = new Point(12, 10);
		cckNoSecretConnect.Size = new Size(100, 24);
		cckNoSecretConnect.TabIndex = 1;
		cckNoSecretConnect.Text = "本机免密连接";
		cckNoSecretConnect.UseVisualStyleBackColor = true;
		cckNoSecretConnect.Value = null;
		cckNoSecretConnect.CheckedChanged += CckNoSecretConnectCheckedChanged;
		cckNoSecretConnect.BackColor = Color.Transparent;
		C1Label c1Label = new C1Label();
		c1Label.AutoSize = true;
		c1Label.BorderStyle = BorderStyle.None;
		c1Label.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		c1Label.Location = new Point(129, 13);
		c1Label.Size = new Size(56, 17);
		c1Label.TabIndex = 2;
		c1Label.TextDetached = true;
		c1Label.Text = "数据源：";
		c1Label.Font = new Font("微软雅黑", 9f);
		txtHostAddress = new ComboBox();
		txtHostAddress.AutoSize = false;
		txtHostAddress.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		txtHostAddress.Location = new Point(186, 10);
		txtHostAddress.TabIndex = 10;
		txtHostAddress.ForeColor = Color.Black;
		C1Label c1Label2 = new C1Label();
		c1Label2.AutoSize = true;
		c1Label2.BorderStyle = BorderStyle.None;
		c1Label2.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		c1Label2.Location = new Point(330, 13);
		c1Label2.Size = new Size(92, 17);
		c1Label2.TabIndex = 4;
		c1Label2.TextDetached = true;
		c1Label2.Text = "数据库用户名：";
		c1Label2.Font = new Font("微软雅黑", 9f);
		txtUserName = new TextBox();
		txtUserName.AutoSize = false;
		txtUserName.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		txtUserName.Location = new Point(423, 9);
		txtUserName.Multiline = true;
		txtUserName.Size = new Size(100, 26);
		txtUserName.TabIndex = 1;
		C1Label c1Label3 = new C1Label();
		c1Label3.AutoSize = true;
		c1Label3.BorderStyle = BorderStyle.None;
		c1Label3.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		c1Label3.Location = new Point(540, 13);
		c1Label3.Size = new Size(80, 17);
		c1Label3.TabIndex = 3;
		c1Label3.TextDetached = true;
		c1Label3.Text = "数据库密码：";
		c1Label3.Font = new Font("微软雅黑", 9f);
		txtPassword = new TextBox();
		txtPassword.AutoSize = false;
		txtPassword.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		txtPassword.Location = new Point(621, 9);
		txtPassword.Multiline = true;
		txtPassword.Size = new Size(100, 26);
		txtPassword.TabIndex = 2;
		tabPageDatabase = new C1DockingTabPage();
		tabPageDatabase.BackColor = Color.White;
		tabPageDatabase.Controls.Add(cckNoSecretConnect);
		tabPageDatabase.Controls.Add(txtHostAddress);
		tabPageDatabase.Controls.Add(c1Label);
		tabPageDatabase.Controls.Add(c1Label3);
		tabPageDatabase.Controls.Add(c1Label2);
		tabPageDatabase.Controls.Add(txtPassword);
		tabPageDatabase.Controls.Add(txtUserName);
		tabPageDatabase.Location = new Point(0, 0);
		tabPageDatabase.Size = new Size(733, 39);
		tabPageDatabase.TabIndex = 0;
		tabPageDatabase.Text = "第1页";
		tabPageDatabase.Paint += delegate(object s, PaintEventArgs e1)
		{
			Rectangle rect6 = new Rectangle(0, -1, tabPageDatabase.Width - 1, tabPageDatabase.Height + 2);
			e1.Graphics.DrawRectangle(_brdpen, rect6);
		};
		txtLocalFile = new C1TextBox();
		txtLocalFile.AutoSize = false;
		txtLocalFile.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		txtLocalFile.Location = new Point(8, 6);
		txtLocalFile.Size = new Size(622, 25);
		txtLocalFile.TabIndex = 1;
		txtLocalFile.VerticalAlign = VerticalAlignEnum.Middle;
		txtLocalFile.Enter += txtLocalFile_Enter;
		txtLocalFile.Leave += txtLocalFile_Leave;
		txtLocalFile.BorderStyle = BorderStyle.FixedSingle;
		txtLocalFile.BorderColor = Color.LightGray;
		txtLocalFile.TextDetached = true;
		txtLocalFile.Text = "选择账套文件所在文件夹";
		txtLocalFile.ForeColor = Color.LightGray;
		txtLocalFile.MouseEnter += textBox_Enter;
		txtLocalFile.MouseLeave += textBox_Leave;
		btnLocalFile = new C1Button();
		btnLocalFile.FlatAppearance.BorderSize = 0;
		btnLocalFile.FlatStyle = FlatStyle.Flat;
		btnLocalFile.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		btnLocalFile.Location = new Point(630, 6);
		btnLocalFile.Size = new Size(75, 25);
		btnLocalFile.TabIndex = 2;
		btnLocalFile.Text = "浏览...";
		btnLocalFile.UseVisualStyleBackColor = true;
		btnLocalFile.Click += btnOpenFile_Click;
		btnLocalFile.Font = new Font("微软雅黑", 9f);
		btnLocalFile.ForeColor = Color.White;
		btnLocalFile.FlatStyle = FlatStyle.Flat;
		btnLocalFile.FlatAppearance.BorderSize = 0;
		btnLocalFile.BackColor = auditaiBlue;
		tabPageLocalFile = new C1DockingTabPage();
		tabPageLocalFile.Controls.Add(btnLocalFile);
		tabPageLocalFile.Controls.Add(txtLocalFile);
		tabPageLocalFile.Location = new Point(0, 0);
		tabPageLocalFile.Size = new Size(733, 39);
		tabPageLocalFile.TabIndex = 1;
		tabPageLocalFile.Text = "第2页";
		tabPageLocalFile.Paint += delegate(object s, PaintEventArgs e1)
		{
			Rectangle rect5 = new Rectangle(0, -1, tabPageLocalFile.Width - 1, tabPageLocalFile.Height + 2);
			e1.Graphics.DrawRectangle(_brdpen, rect5);
		};
		C1Label c1Label4 = new C1Label();
		c1Label4.AutoSize = true;
		c1Label4.BorderStyle = BorderStyle.None;
		c1Label4.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		c1Label4.Location = new Point(4, 12);
		c1Label4.Size = new Size(56, 17);
		c1Label4.TabIndex = 11;
		c1Label4.TextDetached = true;
		c1Label4.Text = "数据源：";
		cmbOracleSource = new ComboBox();
		cmbOracleSource.AutoSize = false;
		cmbOracleSource.Cursor = Cursors.IBeam;
		cmbOracleSource.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		cmbOracleSource.Location = new Point(55, 9);
		cmbOracleSource.Size = new Size(120, 22);
		cmbOracleSource.TabIndex = 12;
		C1Label c1Label5 = new C1Label();
		c1Label5.AutoSize = true;
		c1Label5.BorderStyle = BorderStyle.None;
		c1Label5.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		c1Label5.Location = new Point(193, 12);
		c1Label5.Size = new Size(92, 17);
		c1Label5.TabIndex = 18;
		c1Label5.TextDetached = true;
		c1Label5.Text = "数据库实例名：";
		txtOracleInstance = new TextBox();
		txtOracleInstance.AutoSize = false;
		txtOracleInstance.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		txtOracleInstance.Location = new Point(291, 9);
		txtOracleInstance.Multiline = true;
		txtOracleInstance.Size = new Size(100, 26);
		txtOracleInstance.TabIndex = 17;
		C1Label c1Label6 = new C1Label();
		c1Label6.AutoSize = true;
		c1Label6.BorderStyle = BorderStyle.None;
		c1Label6.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		c1Label6.Location = new Point(402, 12);
		c1Label6.Size = new Size(56, 17);
		c1Label6.TabIndex = 14;
		c1Label6.TextDetached = true;
		c1Label6.Text = "用户名：";
		txtOracleUserName = new TextBox();
		txtOracleUserName.AutoSize = false;
		txtOracleUserName.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		txtOracleUserName.Location = new Point(458, 9);
		txtOracleUserName.Multiline = true;
		txtOracleUserName.Size = new Size(100, 26);
		txtOracleUserName.TabIndex = 13;
		C1Label c1Label7 = new C1Label();
		c1Label7.AutoSize = true;
		c1Label7.BorderStyle = BorderStyle.None;
		c1Label7.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		c1Label7.Location = new Point(580, 12);
		c1Label7.Size = new Size(44, 17);
		c1Label7.TabIndex = 16;
		c1Label7.TextDetached = true;
		c1Label7.Text = "密码：";
		txtOraclePassword = new TextBox();
		txtOraclePassword.AutoSize = false;
		txtOraclePassword.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		txtOraclePassword.Location = new Point(630, 9);
		txtOraclePassword.Multiline = true;
		txtOraclePassword.Size = new Size(91, 26);
		txtOraclePassword.TabIndex = 15;
		tabPageOracle = new C1DockingTabPage();
		tabPageOracle.Controls.Add(c1Label5);
		tabPageOracle.Controls.Add(txtOracleInstance);
		tabPageOracle.Controls.Add(c1Label7);
		tabPageOracle.Controls.Add(txtOraclePassword);
		tabPageOracle.Controls.Add(c1Label6);
		tabPageOracle.Controls.Add(txtOracleUserName);
		tabPageOracle.Controls.Add(cmbOracleSource);
		tabPageOracle.Controls.Add(c1Label4);
		tabPageOracle.Location = new Point(0, 0);
		tabPageOracle.Size = new Size(733, 39);
		tabPageOracle.TabIndex = 2;
		tabPageOracle.Text = "第1页";
		tabPageOracle.Paint += delegate(object s, PaintEventArgs e1)
		{
			Rectangle rect4 = new Rectangle(0, -1, tabPageLocalFile.Width - 1, tabPageLocalFile.Height + 2);
			e1.Graphics.DrawRectangle(_brdpen, rect4);
		};
		dktDataSource = new C1DockingTab();
		dktDataSource.Alignment = TabAlignment.Bottom;
		dktDataSource.BackColor = Color.White;
		dktDataSource.BorderStyle = BorderStyle.None;
		dktDataSource.Controls.Add(tabPageDatabase);
		dktDataSource.Controls.Add(tabPageLocalFile);
		dktDataSource.Controls.Add(tabPageOracle);
		dktDataSource.Dock = DockStyle.Fill;
		dktDataSource.Location = new Point(0, 0);
		dktDataSource.ShowTabs = false;
		dktDataSource.Size = new Size(733, 40);
		dktDataSource.TabIndex = 0;
		dktDataSource.TabsSpacing = 0;
		dktDataSource.TabStyle = TabStyleEnum.WindowsXP;
		dktDataSource.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		dktDataSource.VisualStyleBase = C1.Win.C1Command.VisualStyle.WindowsXP;
		dktDataSource.ShowTabs = false;
		pnlDataSource = new C1SplitterPanel();
		pnlDataSource.BackColor = Color.White;
		pnlDataSource.Controls.Add(dktDataSource);
		pnlDataSource.Height = 40;
		pnlDataSource.KeepRelativeSize = false;
		pnlDataSource.Location = new Point(0, 0);
		pnlDataSource.Resizable = false;
		pnlDataSource.Size = new Size(733, 40);
		pnlDataSource.SizeRatio = 10.23;
		pnlDataSource.TabIndex = 0;
		pnlDataSource.Visible = false;
		tileBrands = new C1TileControl();
		tileBrands.AllowChecking = true;
		tileBrands.BackColor = Color.White;
		tileBrands.CheckBackColor = Color.FromArgb(0, 195, 245);
		tileBrands.CheckBorderColor = Color.FromArgb(0, 195, 254);
		tileBrands.Dock = DockStyle.Fill;
		tileBrands.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		tileBrands.GroupFont = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		tileBrands.GroupForeColor = Color.Black;
		tileBrands.GroupTextSize = 11f;
		tileBrands.GroupTextY = 3;
		tileBrands.HotBackColor = Color.WhiteSmoke;
		tileBrands.HotBorderColor = Color.White;
		tileBrands.Location = new Point(0, 0);
		tileBrands.Orientation = LayoutOrientation.Vertical;
		tileBrands.Padding = new Padding(0);
		tileBrands.Size = new Size(733, 463);
		tileBrands.TabIndex = 0;
		tileBrands.TextSize = 0f;
		tileBrands.TextX = 0;
		tileBrands.TextY = 0;
		tileBrands.CellWidth = 150;
		tileBrands.CellHeight = 100;
		tileBrands.Padding = default(Padding);
		tileBrands.TileChecked += TileBrands_TileChecked;
		tileBrands.TileUnchecked += TclTileControl_TileUnchecked;
		tileBrands.Paint += delegate(object s, PaintEventArgs e1)
		{
			Rectangle rect3 = new Rectangle(0, -1, tileBrands.Width - 1, tileBrands.Height + 2);
			e1.Graphics.DrawRectangle(_brdpen, rect3);
		};
		tileBrands.Groups.Clear();
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel();
		c1SplitterPanel.Controls.Add(tileBrands);
		c1SplitterPanel.Height = 463;
		c1SplitterPanel.Location = new Point(0, 41);
		c1SplitterPanel.Size = new Size(733, 463);
		c1SplitterPanel.TabIndex = 1;
		C1SplitContainer c1SplitContainer = new C1SplitContainer();
		c1SplitContainer.AutoSizeElement = AutoSizeElement.Both;
		c1SplitContainer.BackColor = Color.FromArgb(240, 240, 240);
		c1SplitContainer.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		c1SplitContainer.Dock = DockStyle.Fill;
		c1SplitContainer.ForeColor = Color.FromArgb(0, 0, 0);
		c1SplitContainer.Location = new Point(0, 0);
		c1SplitContainer.Panels.Add(pnlDataSource);
		c1SplitContainer.Panels.Add(c1SplitterPanel);
		c1SplitContainer.Size = new Size(733, 504);
		c1SplitContainer.SplitterWidth = 0;
		c1SplitContainer.TabIndex = 0;
		tabPageBrands = new C1DockingTabPage();
		tabPageBrands.Controls.Add(c1SplitContainer);
		tabPageBrands.Location = new Point(0, 0);
		tabPageBrands.Size = new Size(733, 504);
		tabPageBrands.TabIndex = 0;
		tabPageBrands.Text = "第1页";
		grdLedgers = new C1FlexGrid();
		grdLedgers.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdLedgers.ColumnInfo = "10,1,0,0,0,100,Columns:";
		grdLedgers.Dock = DockStyle.Fill;
		grdLedgers.ExtendLastCol = true;
		grdLedgers.Location = new Point(0, 0);
		grdLedgers.Rows.DefaultSize = 20;
		grdLedgers.ScrollBars = ScrollBars.Vertical;
		grdLedgers.Size = new Size(733, 504);
		grdLedgers.TabIndex = 0;
		grdLedgers.OwnerDrawCell += grdLedgers_OwnerDrawCell;
		grdLedgers.MouseMove += grdLedgers_MouseMove;
		grdLedgers.AllowEditing = true;
		grdLedgers.Styles.Focus.BackColor = Color.White;
		grdLedgers.Styles.Highlight.BackColor = Color.White;
		grdLedgers.Styles.Highlight.ForeColor = Color.Black;
		grdLedgers.Click += GrdLedgersOpenFolder_Click;
		grdLedgers.Paint += delegate(object s, PaintEventArgs e1)
		{
			Rectangle rect2 = new Rectangle(0, -1, grdLedgers.Width - 1, grdLedgers.Height + 2);
			e1.Graphics.DrawRectangle(_brdpen, rect2);
		};
		tabPageLedgers = new C1DockingTabPage();
		tabPageLedgers.Controls.Add(grdLedgers);
		tabPageLedgers.Location = new Point(0, 0);
		tabPageLedgers.Size = new Size(733, 504);
		tabPageLedgers.TabIndex = 1;
		tabPageLedgers.Text = "第2页";
		dktMain = new C1DockingTab();
		dktMain.Alignment = TabAlignment.Bottom;
		dktMain.BorderStyle = BorderStyle.None;
		dktMain.Controls.Add(tabPageBrands);
		dktMain.Controls.Add(tabPageLedgers);
		dktMain.Dock = DockStyle.Fill;
		dktMain.Location = new Point(0, 0);
		dktMain.Size = new Size(733, 529);
		dktMain.TabIndex = 0;
		dktMain.TabsSpacing = 0;
		dktMain.TabStyle = TabStyleEnum.WindowsXP;
		dktMain.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		dktMain.VisualStyleBase = C1.Win.C1Command.VisualStyle.WindowsXP;
		dktMain.ShowTabs = false;
		dktMain.Paint += delegate(object s, PaintEventArgs e1)
		{
			Rectangle rect = new Rectangle(0, 0, dktMain.Width - 1, dktMain.Height - 1);
			e1.Graphics.DrawRectangle(_brdpen, rect);
		};
		pnlTabDock.Controls.Add(dktMain);
		txtHostAddress.Items.Clear();
		LANEnumerator lANEnumerator = new LANEnumerator();
		lANEnumerator.EnumerateCallback += delegate(object s1, LANEnumerateEventArgs e1)
		{
			if (cckNoSecretConnect.Checked)
			{
				lanAddress.Add(e1.IP);
			}
			else
			{
				lanAddress.Add(e1.IP);
				txtHostAddress.Items.Add(e1.IP);
			}
		};
		lANEnumerator.BeginEnumerate();
	}

	private void InitializeProgress()
	{
		grdLedgers.DrawMode = DrawModeEnum.OwnerDraw;
		_pen = new Pen(Color.FromArgb(150, Color.White), 1f);
		_bmp = new Bitmap(100, 20);
		Rectangle rectangle = default(Rectangle);
		rectangle.Size = _bmp.Size;
		Rectangle rect = rectangle;
		Color color = Color.FromArgb(150, Color.White);
		using LinearGradientBrush brush = new LinearGradientBrush(rect, color, auditaiBlue, LinearGradientMode.Horizontal);
		using Graphics graphics = Graphics.FromImage(_bmp);
		graphics.FillRectangle(brush, rect);
	}

	private void PopulateBrandList(IEnumerable<CrawlerBase> crawlers)
	{
		IEnumerable<IGrouping<string, CrawlerBase>> enumerable = from cr in crawlers
			group cr by cr.Brand;
		IGrouping<string, CrawlerBase> grouping = enumerable.FirstOrDefault((IGrouping<string, CrawlerBase> t) => t.Key.Contains("金蝶"));
		AppendGroup(grouping?.AsEnumerable());
		IGrouping<string, CrawlerBase> grouping2 = enumerable.FirstOrDefault((IGrouping<string, CrawlerBase> t) => t.Key.Contains("用友"));
		AppendGroup(grouping2?.AsEnumerable());
		foreach (IGrouping<string, CrawlerBase> item in enumerable)
		{
			if (item.Key != grouping.Key && item.Key != grouping2.Key)
			{
				AppendGroup(item.AsEnumerable());
			}
		}
		void AppendGroup(IEnumerable<CrawlerBase> craws)
		{
			if (craws != null)
			{
				CrawlerBase crawlerBase = craws.FirstOrDefault();
				if (crawlerBase != null)
				{
					Group group = new Group
					{
						Text = crawlerBase.Brand
					};
					group.Paint += delegate(object s1, PaintEventArgs e1)
					{
						Group_Paint(group, e1);
					};
					foreach (CrawlerBase craw in craws)
					{
						ReportNotice("加载采数模块", "正在加载" + craw.FriendlyName + "采数模块...");
						group.Tiles.Add(CreateTile(craw));
					}
					tileBrands.Groups.Add(group);
				}
			}
		}
	}

	private void PopulateLedgerList(Dictionary<LedgerInfo, CrawlerBase> ledgerDic)
	{
		grdLedgers.Font = new Font("微软雅黑", 9f);
		grdLedgers.Rows.DefaultSize = 30;
		grdLedgers.Cols.Count = 0;
		grdLedgers.Rows.Count = 1;
		grdLedgers.Rows.Fixed = 1;
		Column column = grdLedgers.Cols.Add();
		column.Name = "Check";
		column.Caption = "选择";
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.ImageAlign = ImageAlignEnum.CenterCenter;
		column = grdLedgers.Cols.Add();
		column.Name = "Index";
		column.Caption = "序号";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.AllowEditing = false;
		column = grdLedgers.Cols.Add();
		column.Name = "Num";
		column.Caption = "账套号";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.LeftCenter;
		column.AllowEditing = false;
		column = grdLedgers.Cols.Add();
		column.Name = "Company";
		column.Caption = "单位名称";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.LeftCenter;
		column.AllowEditing = false;
		column = grdLedgers.Cols.Add();
		column.Name = "Year";
		column.Caption = "账套年度";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.AllowEditing = false;
		column = grdLedgers.Cols.Add();
		column.Name = "State";
		column.Caption = "采集进度";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.AllowEditing = false;
		CellStyle cellStyle = grdLedgers.Styles.Add("state");
		cellStyle.TextAlign = TextAlignEnum.CenterCenter;
		Font font = grdLedgers.Styles.Normal.Font;
		cellStyle.Font = new Font(font, FontStyle.Underline);
		cellStyle.ForeColor = Color.Blue;
		column.Style = cellStyle;
		grdLedgers.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		grdLedgers.AutoSizeCols(0, grdLedgers.Cols.Count, 5);
		grdLedgers.ExtendLastCol = true;
		if (ledgerDic == null)
		{
			return;
		}
		foreach (KeyValuePair<LedgerInfo, CrawlerBase> item in ledgerDic)
		{
			LedgerInfo key = item.Key;
			CrawlerBase value = item.Value;
			Row row = grdLedgers.Rows.Add();
			row["Index"] = row.Index + 1 - grdLedgers.Rows.Fixed;
			row["Company"] = key.CompanyName;
			row["Num"] = key.LedgerNumber;
			row["Year"] = key.Year;
			row.UserData = new ProgressInfo
			{
				LedgerInfo = key,
				Crawler = value,
				Current = 0
			};
			int index = grdLedgers.Cols["Check"].Index;
			grdLedgers.SetCellCheck(row.Index, index, CheckEnum.Unchecked);
		}
		grdLedgers.AutoSizeCols(0, grdLedgers.Cols.Count, 5);
		grdLedgers.ExtendLastCol = true;
	}

	private void SwitchStatusTo(StatusEnum to)
	{
		currentStatus = to;
		if (currentStatus != StatusEnum.LedgerScaning && currentStatus != StatusEnum.IntelligenceScaning)
		{
			_angleTimer.Stop();
		}
		EnableControls(enable: true);
		switch (currentStatus)
		{
		case StatusEnum.ManualScanPrepare:
			btnScan.Visible = true;
			btnScan.Text = "扫描账套";
			imgMainNotice.Visible = true;
			imgSubNotice.Visible = true;
			imgMainNotice.Image = Resources.扁平电脑72;
			imgSubNotice.Image = Resources.扁平放大镜48;
			ReportNotice("准备扫描账套", "请指定采数路径或设置连接数据库参数后点按扫描账套");
			break;
		case StatusEnum.LedgerScaning:
			btnScan.Visible = true;
			btnScan.Text = "停止扫描";
			imgMainNotice.Visible = true;
			imgSubNotice.Visible = true;
			imgMainNotice.Image = Resources.扁平电脑72;
			imgSubNotice.Image = Resources.扁平放大镜48;
			ReportNotice("扫描状态", "正在扫描账套 请稍候...");
			EnableControls(enable: false);
			_angleTimer.Start();
			break;
		case StatusEnum.ScanComplete:
			btnScan.Visible = true;
			btnScan.Text = "采集账套";
			imgMainNotice.Visible = true;
			imgSubNotice.Visible = false;
			imgMainNotice.Image = Resources.绿账本72;
			break;
		case StatusEnum.LedgerCralering:
			btnScan.Visible = false;
			imgMainNotice.Visible = true;
			imgSubNotice.Visible = false;
			imgMainNotice.Image = Resources.采集数据72;
			break;
		case StatusEnum.CralerComplete:
			imgMainNotice.Visible = true;
			imgSubNotice.Visible = false;
			imgMainNotice.Image = Resources.完成72;
			ReportNotice("采集完成", "当前选择账套已采集完成，您可以继续采集或退出");
			break;
		case StatusEnum.Warning:
			imgMainNotice.Visible = true;
			imgSubNotice.Visible = false;
			imgMainNotice.Image = Resources.警告72;
			break;
		case StatusEnum.IntelligencePrepare:
			btnScan.Visible = true;
			btnScan.Text = "智能扫描";
			imgMainNotice.Visible = true;
			imgSubNotice.Visible = true;
			imgMainNotice.Image = Resources.扁平电脑72;
			imgSubNotice.Image = Resources.扁平放大镜48;
			ReportNotice("智能扫描", "您可以直接智能扫描账套，也可以手动选择软件品牌及版本后再行扫描");
			break;
		case StatusEnum.IntelligenceScaning:
			btnScan.Visible = true;
			btnScan.Text = "停止扫描";
			imgMainNotice.Visible = true;
			imgSubNotice.Visible = true;
			imgMainNotice.Image = Resources.扁平电脑72;
			imgSubNotice.Image = Resources.扁平放大镜48;
			EnableControls(enable: false);
			_angleTimer.Start();
			break;
		}
		Update();
	}

	private void SwitchMainViewTo(MainViewEnum to)
	{
		currentMainView = to;
		switch (currentMainView)
		{
		case MainViewEnum.Brand:
			dktMain.SelectedTab = tabPageBrands;
			progressBar1.Visible = false;
			if (_currentSelectTile != null)
			{
				tileBrands.ScrollToTile(_currentSelectTile, immediate: true);
			}
			btnBack.Visible = false;
			btnScan.Visible = true;
			btnCrawler.Visible = false;
			break;
		case MainViewEnum.Ledger:
			dktMain.SelectedTab = tabPageLedgers;
			progressBar1.Visible = true;
			btnBack.Visible = true;
			btnScan.Visible = false;
			btnCrawler.Visible = true;
			break;
		}
	}

	private void SwitchDataSourceViewTo(LSDb.DbProvider provider)
	{
		switch (provider)
		{
		case LSDb.DbProvider.Oracle:
			SwitchDataSourceViewTo(DataSourceViewEnum.Oracle);
			break;
		case LSDb.DbProvider.Jet:
		case LSDb.DbProvider.Paradox:
			SwitchDataSourceViewTo(DataSourceViewEnum.LocalFile);
			break;
		default:
			SwitchDataSourceViewTo(DataSourceViewEnum.Database);
			break;
		}
	}

	private void SwitchDataSourceViewTo(DataSourceViewEnum to)
	{
		currentDataSourceView = to;
		switch (currentDataSourceView)
		{
		case DataSourceViewEnum.LocalFile:
			pnlDataSource.Visible = true;
			dktDataSource.SelectedTab = tabPageLocalFile;
			break;
		case DataSourceViewEnum.Database:
			pnlDataSource.Visible = true;
			cckNoSecretConnect.Checked = true;
			dktDataSource.SelectedTab = tabPageDatabase;
			break;
		case DataSourceViewEnum.Oracle:
			pnlDataSource.Visible = true;
			dktDataSource.SelectedTab = tabPageOracle;
			break;
		case DataSourceViewEnum.None:
			pnlDataSource.Visible = false;
			break;
		}
	}

	private CrawlerBase CrawlerInstanceFromDll(string assemblyFile)
	{
		Assembly assembly = Assembly.LoadFrom(assemblyFile);
		Type type = assembly.GetTypes().SingleOrDefault((Type t) => t.IsSubclassOf(typeof(CrawlerBase)) && !t.Equals(typeof(CrawlerBase)));
		if (type == null)
		{
			return null;
		}
		object[] customAttributes = type.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: true);
		if (customAttributes != null && customAttributes.Length != 0)
		{
			return null;
		}
		return Activator.CreateInstance(type) as CrawlerBase;
	}

	private void CrawlerInstanceFromDir(string dir, Action<List<CrawlerBase>> continuation)
	{
		List<CrawlerBase> ret = new List<CrawlerBase>();
		Updater updater = new Updater();
		updater.BeginGetCrawlerList(delegate(List<CrawlerModuleInfo> crawlers)
		{
			Queue<CrawlerModuleInfo> queue = new Queue<CrawlerModuleInfo>(crawlers);
			GetCrawler();
			void GetCrawler()
			{
				if (queue.Any())
				{
					CrawlerModuleInfo crawlerModuleInfo = queue.Dequeue();
					string text = Path.Combine(dir, crawlerModuleInfo.Name + ".dll");
					if (File.Exists(text))
					{
						FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text);
						if (string.IsNullOrEmpty(versionInfo.FileVersion) || new Version(versionInfo.FileVersion) < crawlerModuleInfo.Version)
						{
							updater.BeginDownloadCrawler(crawlerModuleInfo, text, delegate
							{
								GetCrawler();
							});
						}
						else
						{
							GetCrawler();
						}
					}
					else
					{
						updater.BeginDownloadCrawler(crawlerModuleInfo, text, delegate
						{
							GetCrawler();
						});
					}
				}
				else
				{
					string[] files = Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly);
					string[] array = files;
					foreach (string assemblyFile in array)
					{
						try
						{
							CrawlerBase crawlerBase = CrawlerInstanceFromDll(assemblyFile);
							if (crawlerBase != null)
							{
								ret.Add(crawlerBase);
							}
						}
						catch (BadImageFormatException)
						{
						}
						catch (ReflectionTypeLoadException)
						{
						}
						catch (Exception)
						{
						}
					}
					continuation(ret);
				}
			}
		});
	}

	private void IntelligenceScanLocalAsync()
	{
		SwitchStatusTo(StatusEnum.IntelligenceScaning);
		intelligenceScanWorker = new BackgroundWorker();
		intelligenceScanWorker.WorkerReportsProgress = true;
		intelligenceScanWorker.WorkerSupportsCancellation = true;
		intelligenceScanWorker.DoWork += delegate
		{
			string[] sqlServerInstanceNames = Util.GetSqlServerInstanceNames();
			string[] array = sqlServerInstanceNames;
			foreach (string text in array)
			{
				try
				{
					DatabaseInfo dbInfo2 = new DatabaseInfo
					{
						DatabaseType = LSDb.DbProvider.SqlServer,
						DataSource = ".\\" + text,
						IntegratedSecurity = true
					};
					LSDb lSDb = LSDb.Create(dbInfo2.DatabaseType);
					lSDb.DataSource = dbInfo2.DataSource;
					lSDb.IntegratedSecurity = dbInfo2.IntegratedSecurity;
					lSDb.ConnectionTimeout = 5;
					try
					{
						Invoke((InvokeDelegate)delegate
						{
							ReportNotice("智能扫描", "正在扫描本地数据库，请稍候……");
						});
						lSDb.GetDatabaseNames();
						foreach (CrawlerBase crawler2 in Crawlers)
						{
							if (crawler2.DbProvider == LSDb.DbProvider.SqlServer)
							{
								Invoke((InvokeDelegate)delegate
								{
									ReportNotice("智能扫描", "正在智能扫描" + crawler2.FriendlyName + "账套，请稍候……");
								});
								DatabaseInfo dbInfo3 = dbInfo2.Clone();
								try
								{
									IEnumerable<LedgerInfo> source = crawler2.ScanRemote(dbInfo3);
									if (source.Any())
									{
										Invoke((InvokeDelegate)delegate
										{
											ScanLedgerAsync(crawler2, dbInfo2);
										});
										return;
									}
								}
								catch
								{
								}
							}
						}
					}
					catch
					{
					}
				}
				catch
				{
				}
			}
			List<string> list = (from d in DriveInfo.GetDrives()
				where d.DriveType == DriveType.Fixed
				select d.RootDirectory.FullName).Concat(new string[1] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) }).ToList();
			foreach (JetCrawlerBase crawler in Crawlers.OfType<JetCrawlerBase>())
			{
				Invoke((InvokeDelegate)delegate
				{
					ReportNotice("智能扫描", "正在智能扫描" + crawler.FriendlyName + "账套，请稍候……");
				});
				foreach (string item in list)
				{
					try
					{
						FileInfo[] files = new DirectoryInfo(Path.Combine(item, crawler.ScanPath)).GetFiles(crawler.ScanFilePattern, SearchOption.AllDirectories);
						FileInfo[] array2 = files;
						foreach (FileInfo fileInfo in array2)
						{
							logger.Writer(fileInfo.ToString());
							try
							{
								DatabaseInfo dbInfo = new DatabaseInfo
								{
									DatabaseType = LSDb.DbProvider.Jet,
									DataSource = fileInfo.Directory.FullName
								};
								IEnumerable<LedgerInfo> source2 = crawler.ScanRemote(dbInfo);
								if (source2.Any())
								{
									Invoke((InvokeDelegate)delegate
									{
										ScanLedgerAsync(crawler, dbInfo);
									});
									return;
								}
							}
							catch
							{
							}
						}
					}
					catch
					{
					}
				}
			}
			Invoke((InvokeDelegate)delegate
			{
				SwitchStatusTo(StatusEnum.IntelligencePrepare);
				ReportNotice("未扫描到账套信息", "因财务软件未安装在默认位置或本机不是数据库服务器，程序未能智能扫描到财务账套，请手动选择财务软件及版本后再行扫描");
			});
		};
		intelligenceScanWorker.RunWorkerCompleted += delegate
		{
			currentIntelligenceScanThreadId = -1;
			intelligenceScanWorker?.Dispose();
		};
		intelligenceScanWorker.RunWorkerAsync();
	}

	private Tuple<CrawlerBase, DatabaseInfo> IntelligenceScanLocalImpl()
	{
		string location = Assembly.GetExecutingAssembly().Location;
		string directoryName = Path.GetDirectoryName(location);
		CrawlerBase.ScanLocalSqlServer(out var module, out var info);
		if (module != null)
		{
			CrawlerBase item = CrawlerInstanceFromDll(Path.Combine(directoryName, module + ".dll"));
			return Tuple.Create(item, info);
		}
		CrawlerBase.ScanLocalDesktop(out module, out var path);
		if (module != null)
		{
			CrawlerBase item2 = CrawlerInstanceFromDll(Path.Combine(directoryName, module + ".dll"));
			return Tuple.Create(item2, new DatabaseInfo
			{
				DatabaseType = LSDb.DbProvider.Jet,
				DataSource = path
			});
		}
		return null;
	}

	private void ApplyLedgerScan()
	{
		try
		{
			bool @checked = cckNoSecretConnect.Checked;
			if (!(_currentSelectTile?.Tag is CrawlerBase crawlerBase))
			{
				return;
			}
			bool flag = crawlerBase.DbProvider == LSDb.DbProvider.SqlServer;
			if (!CheckDataSource(currentDataSourceView, flag, @checked))
			{
				return;
			}
			DatabaseInfo databaseInfo = null;
			switch (crawlerBase.DbProvider)
			{
			case LSDb.DbProvider.Oracle:
				databaseInfo = new DatabaseInfo
				{
					DatabaseType = LSDb.DbProvider.Oracle,
					DataSource = cmbOracleSource.Text.Trim(),
					Name = txtOracleInstance.Text.Trim(),
					User = txtOracleUserName.Text.Trim(),
					Password = txtOraclePassword.Text.Trim(),
					IntegratedSecurity = false
				};
				break;
			case LSDb.DbProvider.Jet:
				databaseInfo = new DatabaseInfo
				{
					DatabaseType = LSDb.DbProvider.Jet,
					DataSource = txtLocalFile.Text.Trim()
				};
				break;
			case LSDb.DbProvider.Paradox:
				databaseInfo = new DatabaseInfo
				{
					DatabaseType = LSDb.DbProvider.Paradox,
					DataSource = txtLocalFile.Text.Trim()
				};
				break;
			default:
				if (flag && @checked)
				{
					string text = txtHostAddress.Text.Trim();
					databaseInfo = new DatabaseInfo
					{
						DatabaseType = crawlerBase.DbProvider,
						IntegratedSecurity = true,
						DataSource = (string.IsNullOrEmpty(text) ? "." : text)
					};
				}
				else
				{
					databaseInfo = new DatabaseInfo
					{
						DatabaseType = crawlerBase.DbProvider,
						DataSource = txtHostAddress.Text.Trim(),
						User = txtUserName.Text.Trim(),
						Password = txtPassword.Text.Trim(),
						IntegratedSecurity = false
					};
				}
				break;
			}
			ScanLedgerAsync(crawlerBase, databaseInfo);
		}
		catch (Exception ex)
		{
			SwitchStatusTo(StatusEnum.Warning);
			logger.Writer(ex.ToString());
			ReportNotice("执行错误，错误信息：", ex.Message, ex);
		}
	}

	private void ScanLedgerAsync(CrawlerBase crawler, DatabaseInfo database)
	{
		try
		{
			_currentCrawler = crawler;
			SwitchStatusTo(StatusEnum.LedgerScaning);
			LedgerInfoCrawler.Clear();
			backWorker = new BackgroundWorker();
			backWorker.WorkerReportsProgress = true;
			backWorker.WorkerSupportsCancellation = true;
			backWorker.DoWork += delegate(object s1, DoWorkEventArgs e1)
			{
				WorkerContext workerContext = new WorkerContext(backWorker, e1);
				try
				{
					_lastDataBase = database;
					IEnumerable<LedgerInfo> enumerable = crawler.ScanRemote(database);
					workerContext.ThrowOperationCanceledExceptionIfCanceled();
					if (enumerable == null || enumerable.Count() == 0)
					{
						workerContext.Worker.ReportProgress(-7, new NoticeMessage("扫描完成", "未扫描到账套信息"));
					}
					else
					{
						Dictionary<LedgerInfo, CrawlerBase> second = enumerable.ToDictionary((LedgerInfo t) => t, (LedgerInfo t) => crawler);
						workerContext.ThrowOperationCanceledExceptionIfCanceled();
						LedgerInfoCrawler = LedgerInfoCrawler.Union(second).ToDictionary((KeyValuePair<LedgerInfo, CrawlerBase> t) => t.Key, (KeyValuePair<LedgerInfo, CrawlerBase> t) => t.Value);
						workerContext.ThrowOperationCanceledExceptionIfCanceled();
						backWorker.ReportProgress(-5);
					}
				}
				catch (OperationCanceledException)
				{
				}
				catch (SqlException ex3) when (ex3.Number == 0)
				{
					if (!workerContext.IsCanceled())
					{
						logger.Writer(ex3.ToString());
						backWorker.ReportProgress(-1, new NoticeMessage("异常原因:" + ex3.Message + "!请检查用户名或密码是否正确", ex3));
					}
				}
				catch (SqlException ex4)
				{
					if (!workerContext.IsCanceled())
					{
						logger.Writer(ex4.ToString());
						backWorker.ReportProgress(-1, new NoticeMessage("异常原因:" + ex4.Message, ex4));
					}
				}
				catch (Exception ex5)
				{
					if (!workerContext.IsCanceled())
					{
						logger.Writer(ex5.ToString());
						backWorker.ReportProgress(-1, new NoticeMessage("异常原因:" + ex5.Message, ex5));
					}
				}
			};
			backWorker.ProgressChanged += delegate(object s1, ProgressChangedEventArgs e1)
			{
				backWorker_ProgressChanged(e1);
			};
			backWorker.RunWorkerCompleted += delegate
			{
				backWorker?.Dispose();
			};
			backWorker.RunWorkerAsync();
		}
		catch (Exception ex)
		{
			SwitchStatusTo(StatusEnum.IntelligencePrepare);
			SwitchStatusTo(StatusEnum.Warning);
			logger.Writer(ex.ToString());
			ReportNotice("执行错误，错误信息：", ex.Message, ex);
		}
	}

	private bool CheckDataSource(DataSourceViewEnum dataSourceEnum, bool sqlServer, bool noSecretConnect)
	{
		switch (dataSourceEnum)
		{
		case DataSourceViewEnum.Database:
			if (!(sqlServer && noSecretConnect) && string.IsNullOrEmpty(txtHostAddress.Text.Trim()))
			{
				ReportNotice("配置参数", "请填写数据源地址");
				SwitchStatusTo(StatusEnum.Warning);
				return false;
			}
			if (!(sqlServer && noSecretConnect) && string.IsNullOrEmpty(txtUserName.Text.Trim()))
			{
				ReportNotice("配置参数", "请填写数据库名称");
				SwitchStatusTo(StatusEnum.Warning);
				return false;
			}
			break;
		case DataSourceViewEnum.LocalFile:
			if (string.IsNullOrEmpty(txtLocalFile.Text.Trim()))
			{
				ReportNotice("配置参数", "请选择账套所在文件夹");
				SwitchStatusTo(StatusEnum.Warning);
				return false;
			}
			break;
		case DataSourceViewEnum.Oracle:
			if (string.IsNullOrEmpty(cmbOracleSource.Text.Trim()))
			{
				ReportNotice("配置参数", "请填写数据源信息");
				SwitchStatusTo(StatusEnum.Warning);
				return false;
			}
			if (string.IsNullOrEmpty(txtOracleInstance.Text.Trim()))
			{
				ReportNotice("配置参数", "请填写实例名称");
				SwitchStatusTo(StatusEnum.Warning);
				return false;
			}
			break;
		}
		return true;
	}

	private void CrawLedger()
	{
		try
		{
			SwitchStatusTo(StatusEnum.LedgerCralering);
			List<Row> selectedRows = SelectedRows();
			CrawPrepare(selectedRows);
			CrawLedgerAsync(selectedRows);
		}
		catch (Exception ex)
		{
			SwitchStatusTo(StatusEnum.Warning);
			logger.Writer(ex.ToString());
			ReportNotice("执行错误，错误信息：", ex.Message, ex);
		}
	}

	private void CrawPrepare(IEnumerable<Row> selectedRows)
	{
		if (!Directory.Exists(DIRECTORY))
		{
			Directory.CreateDirectory(DIRECTORY);
		}
		int maximum = selectedRows.Sum((Row t) => (t.UserData as ProgressInfo).Max);
		selectedRows.Count(delegate(Row t)
		{
			(t.UserData as ProgressInfo).Current = 0;
			return true;
		});
		progressBar1.Value = 0;
		progressBar1.Maximum = maximum;
	}

	private void CrawLedgerAsync(IEnumerable<Row> selectedRows)
	{
		backWorker = new BackgroundWorker();
		backWorker.WorkerReportsProgress = true;
		backWorker.WorkerSupportsCancellation = true;
		backWorker.DoWork += delegate(object obj, DoWorkEventArgs args)
		{
			WorkerContext workerContext = new WorkerContext(backWorker, args);
			try
			{
				List<Tuple<CrawlerBase, LedgerInfo>> list = new List<Tuple<CrawlerBase, LedgerInfo>>();
				foreach (Row selectedRow in selectedRows)
				{
					try
					{
						if (selectedRow.UserData is ProgressInfo { Crawler: var crawler } progressInfo)
						{
							Ledger ledger = null;
							try
							{
								ledger = crawler.GetLedger(progressInfo.LedgerInfo);
							}
							catch (IOException)
							{
								throw;
							}
							catch (Exception ex2)
							{
								logger.Writer(ex2.ToString());
								progressInfo.Failed = true;
								grdLedgers.Invalidate(selectedRow.Index, grdLedgers.Cols["State"].Index);
								DatabaseInfo dbInfo = progressInfo.LedgerInfo.DbInfo;
								if (dbInfo.DatabaseType != LSDb.DbProvider.SqlServer)
								{
									throw;
								}
								list.Add(Tuple.Create(crawler, progressInfo.LedgerInfo));
								goto end_IL_0034;
							}
							string text = $"{ledger.LedgerNumber}_{ledger.CompanyName}_{ledger.Year}.db";
							char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
							foreach (char c in invalidFileNameChars)
							{
								text = text.Replace(c.ToString(), "");
							}
							string text2 = Path.Combine(DIRECTORY, text);
							if (File.Exists(text2))
							{
								File.Delete(text2);
							}
							backWorker.ReportProgress(-8, new NoticeMessage("正在保存账套文件...", "保存文件时间稍长，请耐心等待."));
							ledger.SaveAsSqlite(text2);
							backWorker.ReportProgress(progressInfo.Max - 1, "账套" + ledger.CompanyName + "保存完成");
							progressInfo.FilePath = text2;
							progressInfo.Current = progressInfo.Max;
							grdLedgers.Invalidate(selectedRow.Index, grdLedgers.Cols["State"].Index);
						}
						end_IL_0034:;
					}
					catch (SqlException ex3)
					{
						logger.Writer(ex3.ToString());
						backWorker.ReportProgress(-9, new NoticeMessage("异常原因:" + ex3.Message, ex3));
						return;
					}
					catch (IOException ex4)
					{
						logger.Writer(ex4.ToString());
						backWorker.ReportProgress(-2, new NoticeMessage("异常原因:" + ex4.Message, ex4));
						return;
					}
					catch (Exception ex5)
					{
						logger.Writer(ex5.ToString());
						backWorker.ReportProgress(-9, new NoticeMessage("异常原因:" + ex5.Message, ex5));
						return;
					}
				}
				if (list.Count > 0)
				{
					backWorker.ReportProgress(-11, null);
					foreach (Tuple<CrawlerBase, LedgerInfo> item2 in list)
					{
						string text3 = null;
						CrawlerBase item = item2.Item1;
						DatabaseInfo dbInfo2 = item2.Item2.DbInfo;
						try
						{
							text3 = ((!dbInfo2.IntegratedSecurity) ? SQLServerHelper.GetSqlServerConnectionString(dbInfo2.DataSource ?? "127.0.0.1", dbInfo2.Name, dbInfo2.User, dbInfo2.Password) : SQLServerHelper.GetSqlServerConnectionString(dbInfo2.DataSource ?? "127.0.0.1", dbInfo2.Name));
							string text4 = "auditai" + DateTime.Now.ToString("yyyyMMddHHmmss");
							string outPath = Path.Combine("./Logs/", text4 + "_.analyze");
							BakDatabase(item, text3, outPath);
						}
						catch (Exception ex6)
						{
							logger.Writer(ex6.ToString());
						}
					}
					backWorker.ReportProgress(-10, null);
				}
				else
				{
					backWorker.ReportProgress(-6);
				}
			}
			catch (DbException ex7)
			{
				logger.Writer(ex7.ToString());
				backWorker.ReportProgress(-4, new NoticeMessage("异常原因:" + ex7.Message, ex7));
			}
		};
		backWorker.ProgressChanged += delegate(object s1, ProgressChangedEventArgs e1)
		{
			backWorker_ProgressChanged(e1);
		};
		backWorker.RunWorkerCompleted += delegate
		{
			btnScan.Visible = true;
			backWorker.Dispose();
		};
		backWorker.RunWorkerAsync();
	}

	private void QueryExe()
	{
		string processName = Process.GetCurrentProcess().ProcessName;
		if (Process.GetProcessesByName(processName).Length > 1)
		{
			Close();
			Application.Exit();
		}
	}

	private List<Row> SelectedRows()
	{
		List<Row> list = new List<Row>();
		int index = grdLedgers.Cols["Check"].Index;
		foreach (Row item in (IEnumerable)grdLedgers.Rows)
		{
			CheckEnum cellCheck = grdLedgers.GetCellCheck(item.Index, index);
			if (cellCheck == CheckEnum.Checked && item.UserData is ProgressInfo)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private void ReportNotice(string main, string sub = "", Exception ex = null)
	{
		lblMainNotice.Text = main;
		lblSubNotice.Text = sub;
		lblMainNotice.Update();
		lblSubNotice.Update();
		if (ex == null)
		{
			lnkException.Visible = false;
			return;
		}
		lnkException.Visible = true;
		lnkException.Tag = ex;
	}

	private Tile CreateTile(CrawlerBase crawler)
	{
		Tile tile = new Tile
		{
			Text = crawler.FriendlyName,
			Image = crawler.Icon,
			Template = _defaultTileTemplate,
			BackColor = Color.White,
			ForeColor = Color.Black,
			Tag = crawler
		};
		tile.Click += delegate
		{
			if ((currentStatus != StatusEnum.IntelligenceScaning && currentStatus != StatusEnum.LedgerScaning) || _allowCheckTile)
			{
				if (_currentSelectTile == tile)
				{
					_currentSelectTile = null;
					tile.Checked = false;
					SwitchDataSourceViewTo(DataSourceViewEnum.None);
					SwitchStatusTo(StatusEnum.IntelligencePrepare);
				}
				else
				{
					_currentSelectTile = tile;
					_allowCheckTile = true;
					tile.Checked = true;
					SwitchStatusTo(StatusEnum.ManualScanPrepare);
					SwitchDataSourceViewTo(crawler.DbProvider);
					if (crawler.FriendlyName == Config.FriendlyName && !hasLoaded)
					{
						LoadDatabase(crawler.DbProvider, Config.DatabaseInfo);
						hasLoaded = true;
					}
					else if (crawler.DbProvider == LSDb.DbProvider.Jet || crawler.DbProvider == LSDb.DbProvider.Paradox)
					{
						txtLocalFile.Text = "";
					}
				}
			}
		};
		return tile;
	}

	private bool LoadFromConfig(Config config)
	{
		_allowCheckTile = true;
		try
		{
			if (config == null)
			{
				return false;
			}
			foreach (Tile item in tileBrands.Groups.SelectMany((Group g) => g.Tiles))
			{
				if (item.Tag is CrawlerBase crawlerBase && crawlerBase.FriendlyName == config.FriendlyName)
				{
					item.PerformClick();
					LoadDatabase(crawlerBase.DbProvider, config.DatabaseInfo);
					tileBrands.ScrollToTile(item, immediate: true);
					if (item.Group.Y > tileBrands.Height - 200)
					{
						tileBrands.ScrollOffset -= tileBrands.Height / 2;
					}
					return true;
				}
			}
			return false;
		}
		finally
		{
			_allowCheckTile = false;
		}
	}

	private void LoadDatabase(LSDb.DbProvider provider, DatabaseInfo database)
	{
		if (database == null)
		{
			return;
		}
		switch (provider)
		{
		case LSDb.DbProvider.Oracle:
			if (database.IntegratedSecurity)
			{
				cckNoSecretConnect.Checked = true;
				break;
			}
			cckNoSecretConnect.Checked = false;
			EnableControls(enable: true);
			cmbOracleSource.Text = database.DataSource;
			txtOracleInstance.Text = database.Name;
			txtOracleUserName.Text = database.User;
			txtOraclePassword.Text = database.Password;
			break;
		case LSDb.DbProvider.Jet:
		case LSDb.DbProvider.Paradox:
			txtLocalFile.Text = database.DataSource;
			txtLocalFile.ForeColor = Color.Black;
			_fileNoticeText = false;
			break;
		default:
			if (database.IntegratedSecurity)
			{
				cckNoSecretConnect.Checked = true;
				txtHostAddress.Text = string.Empty;
				break;
			}
			cckNoSecretConnect.Checked = false;
			EnableControls(enable: true);
			txtHostAddress.Text = database.DataSource;
			txtPassword.Text = database.Password;
			txtUserName.Text = database.User;
			break;
		}
	}

	private void EnableControls(bool enable)
	{
		txtLocalFile.Enabled = enable;
		btnLocalFile.Enabled = enable;
		cckNoSecretConnect.Enabled = enable;
		txtHostAddress.Enabled = enable;
		txtUserName.Enabled = !cckNoSecretConnect.Checked && enable;
		txtPassword.Enabled = !cckNoSecretConnect.Checked && enable;
		cmbOracleSource.Enabled = enable;
		txtOracleInstance.Enabled = enable;
		txtOracleUserName.Enabled = enable;
		txtOraclePassword.Enabled = enable;
	}

	private void BakDatabase(CrawlerBase crawler, string sqlConnStr, string outPath)
	{
		List<string> validTables = crawler.ValidTables;
		if (validTables != null)
		{
			string directoryName = Path.GetDirectoryName(outPath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			SqlServerToSQLite.ConvertSqlServerToSQLiteDatabase(sqlConnStr, outPath, new ConvertConfig
			{
				CreatePrimarykey = false,
				CreateForignkey = false,
				CreateCollate = false,
				CreateIndex = false,
				CreateTriggers = false,
				CreateViews = false,
				progressHandler = null,
				tableSelectionHandler = (string t) => validTables.Contains(t),
				UsePassword = null,
				viewFailedHandler = null
			});
		}
	}

	private void CrawlerForm_Load(object sender, EventArgs e)
	{
		currentDataSourceView = DataSourceViewEnum.None;
		Update();
	}

	private void CrawlerForm_Shown(object sender, EventArgs e)
	{
		Update();
		ShownInitialize();
	}

	private void btnScan_Click(object sender, EventArgs e)
	{
		switch (currentStatus)
		{
		case StatusEnum.IntelligencePrepare:
			IntelligenceScanLocalAsync();
			break;
		case StatusEnum.IntelligenceScaning:
			currentIntelligenceScanThreadId = -1;
			intelligenceScanWorker.CancelAsync();
			intelligenceScanWorker.Dispose();
			SwitchStatusTo(StatusEnum.IntelligencePrepare);
			break;
		case StatusEnum.LedgerScaning:
		{
			LSDb.DbProvider dbProvider = _currentCrawler.DbProvider;
			LSDb.DbProvider dbProvider2 = dbProvider;
			if (dbProvider2 == LSDb.DbProvider.Jet || dbProvider2 == LSDb.DbProvider.Paradox)
			{
				Util.Cancel = true;
				if (_currentSelectTile == null)
				{
					SwitchStatusTo(StatusEnum.IntelligencePrepare);
				}
				else
				{
					SwitchStatusTo(StatusEnum.ManualScanPrepare);
				}
			}
			else
			{
				backWorker.CancelAsync();
				if (_currentSelectTile == null)
				{
					SwitchStatusTo(StatusEnum.IntelligencePrepare);
				}
				else
				{
					SwitchStatusTo(StatusEnum.ManualScanPrepare);
				}
			}
			break;
		}
		default:
			ApplyLedgerScan();
			break;
		}
	}

	private void btnCrawler_Click(object sender, EventArgs e)
	{
		if (SelectedRows().Count > 0 && currentStatus != StatusEnum.LedgerCralering)
		{
			CrawLedger();
		}
	}

	private void btnBack_Click(object sender, EventArgs e)
	{
		SwitchMainViewTo(MainViewEnum.Brand);
		if (_currentSelectTile == null)
		{
			SwitchStatusTo(StatusEnum.IntelligencePrepare);
		}
		else
		{
			SwitchStatusTo(StatusEnum.ManualScanPrepare);
		}
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		if (_currentCrawler != null && _lastDataBase != null)
		{
			Config.DatabaseInfo = _lastDataBase;
			Config.Brand = _currentCrawler.Brand;
			Config.FriendlyName = _currentCrawler.FriendlyName;
			Config.Save(CONFIGPATH);
		}
		Close();
	}

	private void btnOpenFile_Click(object sender, EventArgs e)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			string selectedPath = folderBrowserDialog.SelectedPath;
			txtLocalFile.Text = selectedPath;
			txtLocalFile.ForeColor = Color.Black;
			_fileNoticeText = false;
		}
	}

	private void GrdLedgersOpenFolder_Click(object sender, EventArgs e)
	{
		int mouseRow = grdLedgers.MouseRow;
		int mouseCol = grdLedgers.MouseCol;
		if (mouseCol < 0 || mouseRow < 0 || mouseCol != grdLedgers.Cols["State"].Index)
		{
			return;
		}
		Row row = grdLedgers.Rows[mouseRow];
		if (!(row.UserData is ProgressInfo progressInfo) || progressInfo.Current < progressInfo.Max)
		{
			return;
		}
		try
		{
			Process.Start("Explorer.exe", "/Select," + Path.GetFullPath(progressInfo.FilePath));
		}
		catch
		{
		}
	}

	private void backWorker_ProgressChanged(ProgressChangedEventArgs e1)
	{
		switch (e1.ProgressPercentage)
		{
		case -6:
			SwitchStatusTo(StatusEnum.CralerComplete);
			break;
		case -11:
			SwitchStatusTo(StatusEnum.Warning);
			ReportNotice("提示：", "采数过程出现异常，正在分析异常原因！分析时间稍长，请耐心等待.");
			break;
		case -10:
			SwitchStatusTo(StatusEnum.CralerComplete);
			ReportNotice("提示：", "分析完成，请致电官方客服电话400-690-6500寻求支持。");
			break;
		case -4:
			SwitchStatusTo(StatusEnum.Warning);
			SwitchMainViewTo(MainViewEnum.Brand);
			SwitchStatusTo(StatusEnum.ManualScanPrepare);
			if (e1.UserState is NoticeMessage noticeMessage2)
			{
				ReportNotice("提示：", "采集过程出错，请检查是否选择了正确的财务软件及相应版本", noticeMessage2.Exception);
			}
			break;
		case -2:
			SwitchStatusTo(StatusEnum.Warning);
			if (e1.UserState is NoticeMessage noticeMessage6)
			{
				ReportNotice("采数异常，异常信息：", noticeMessage6.Message, noticeMessage6.Exception);
			}
			break;
		case -8:
			if (e1.UserState is NoticeMessage noticeMessage4)
			{
				ReportNotice(noticeMessage4.Title, noticeMessage4.Message);
			}
			break;
		case -9:
		{
			NoticeMessage noticeMessage3 = e1.UserState as NoticeMessage;
			SwitchStatusTo(StatusEnum.ScanComplete);
			SwitchStatusTo(StatusEnum.Warning);
			if (noticeMessage3 != null)
			{
				ReportNotice("采数发生异常：", noticeMessage3.Message, noticeMessage3.Exception);
			}
			break;
		}
		case -7:
		{
			if (_currentSelectTile == null)
			{
				SwitchStatusTo(StatusEnum.IntelligencePrepare);
			}
			else
			{
				SwitchStatusTo(StatusEnum.ManualScanPrepare);
			}
			NoticeMessage noticeMessage5 = e1.UserState as NoticeMessage;
			SwitchStatusTo(StatusEnum.Warning);
			if (noticeMessage5 != null)
			{
				ReportNotice(noticeMessage5.Title, noticeMessage5.Message);
			}
			break;
		}
		case -5:
			SwitchMainViewTo(MainViewEnum.Ledger);
			SwitchStatusTo(StatusEnum.ScanComplete);
			foreach (LedgerInfo ledgerInfo in LedgerInfoCrawler.Keys)
			{
				ledgerInfo.ProgressChanged += delegate(object s2, GetLedgerProgressEventArgs e2)
				{
					foreach (Row item in (IEnumerable)grdLedgers.Rows)
					{
						ProgressInfo progressInfo = item.UserData as ProgressInfo;
						if (ledgerInfo.Equals(progressInfo?.LedgerInfo))
						{
							Thread.Sleep(100);
							(item.UserData as ProgressInfo).Current = e2.Progress / progressInfo.Max + e2.Progress % progressInfo.Max;
							grdLedgers.Invalidate(item.Index, grdLedgers.Cols["State"].Index);
							backWorker.ReportProgress(e2.Progress, e2.Message);
						}
					}
				};
			}
			ReportNotice("扫描完成", $"扫描到 {LedgerInfoCrawler.Count} 个账套，请选择要采集的账套");
			PopulateLedgerList(LedgerInfoCrawler);
			break;
		case -1:
			if (_currentSelectTile == null)
			{
				SwitchStatusTo(StatusEnum.IntelligencePrepare);
			}
			else
			{
				SwitchStatusTo(StatusEnum.ManualScanPrepare);
			}
			SwitchStatusTo(StatusEnum.Warning);
			if (e1.UserState is NoticeMessage noticeMessage)
			{
				ReportNotice("执行错误，错误信息", noticeMessage.Message, noticeMessage.Exception);
			}
			break;
		default:
			if (progressBar1.Value < progressBar1.Maximum)
			{
				progressBar1.Value++;
				ReportNotice("采集进度：", e1.UserState.ToString());
			}
			break;
		}
	}

	private void CrawlerBase_ScanProgressChanged(object sender, CrawlerScanProgressEventArgs e)
	{
		if (Thread.CurrentThread.ManagedThreadId == currentIntelligenceScanThreadId && btnScan.InvokeRequired)
		{
			btnScan.Invoke((InvokeDelegate)delegate
			{
				ReportNotice("智能扫描", "正在智能扫描" + e.ModuleName + "账套，请稍候…….");
			});
		}
	}

	private void _angleTimer_Tick(object sender, EventArgs e)
	{
		_angle = (_angle + 20) % 360;
		Point anglePoint = _circle.GetAnglePoint(_angle);
		imgSubNotice.Location = anglePoint;
		imgSubNotice.Update();
		imgMainNotice.Update();
		Update();
	}

	private void Group_Paint(Group group, PaintEventArgs e1)
	{
		Rectangle rect = new Rectangle(e1.ClipRectangle.Left, e1.ClipRectangle.Top, group.Width - 10, 21);
		e1.Graphics.FillRectangle(Brushes.WhiteSmoke, rect);
		Font font = new Font("微软雅黑", 11f);
		int left = e1.ClipRectangle.Left;
		int top = e1.ClipRectangle.Top;
		e1.Graphics.DrawString(group.Text, font, Brushes.Black, left, top);
	}

	private void grdLedgers_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (!(grdLedgers.Cols[e.Col].Name == "State") || e.Row < grdLedgers.Rows.Fixed)
		{
			return;
		}
		Row row = grdLedgers.Rows[e.Row];
		if (row.UserData is ProgressInfo { Current: not 0 } progressInfo)
		{
			if (progressInfo.Current >= progressInfo.Max)
			{
				e.Text = "采集完成，打开文件夹";
				return;
			}
			if (progressInfo.Failed)
			{
				e.Text = "采集失败";
				return;
			}
			Rectangle bounds = e.Bounds;
			e.Graphics.DrawRectangle(_pen, bounds);
			double num = (double)progressInfo.Current / (double)progressInfo.Max;
			bounds.Width = (int)((double)bounds.Width * num);
			e.Graphics.DrawImage(_bmp, bounds);
			e.Text = $"当前采集 {(int)(num * 100.0)}%";
			e.DrawCell(DrawCellFlags.Content);
			e.Handled = true;
		}
	}

	private void TileBrands_TileChecked(object sender, TileEventArgs e)
	{
		if (!_allowCheckTile)
		{
			e.Tile.Checked = false;
		}
		_allowCheckTile = false;
	}

	private void TclTileControl_TileUnchecked(object sender, TileEventArgs e)
	{
		if (_currentSelectTile != null)
		{
			_allowCheckTile = true;
			_currentSelectTile.Checked = true;
		}
	}

	private void pnlHeader_Paint(object sender, PaintEventArgs e1)
	{
		Rectangle rect = new Rectangle(0, 0, pnlHeader.Width - 1, pnlHeader.Height - 1);
		e1.Graphics.DrawRectangle(_brdpen, rect);
	}

	private void CrawlerForm_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(base.Handle, 274, 61458, 0);
	}

	private void lblMainNotice_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(base.Handle, 274, 61458, 0);
	}

	private void lblSubNotice_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(base.Handle, 274, 61458, 0);
	}

	private void textBox_Enter(object sender, EventArgs e)
	{
		((C1TextBox)sender).BorderColor = auditaiBlue;
	}

	private void textBox_Leave(object sender, EventArgs e)
	{
		((C1TextBox)sender).BorderColor = Color.LightGray;
	}

	private void btnMinBox_Click(object sender, EventArgs e)
	{
		base.WindowState = FormWindowState.Minimized;
	}

	private void btnCertain_Enter(object sender, EventArgs e)
	{
	}

	private void txtLocalFile_Enter(object sender, EventArgs e)
	{
		if (_fileNoticeText)
		{
			txtLocalFile.ForeColor = Color.Black;
			txtLocalFile.Text = string.Empty;
		}
	}

	private void txtLocalFile_Leave(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(txtLocalFile.Text))
		{
			_fileNoticeText = true;
			txtLocalFile.ForeColor = Color.LightGray;
			txtLocalFile.Text = "选择账套文件所在文件夹";
		}
		else
		{
			_fileNoticeText = false;
		}
	}

	private void CckNoSecretConnectCheckedChanged(object sender, EventArgs e)
	{
		if (cckNoSecretConnect.Checked)
		{
			txtUserName.Enabled = false;
			txtPassword.Enabled = false;
			txtHostAddress.Text = string.Empty;
			txtHostAddress.Items.Clear();
			string[] sqlServerInstanceNames = Util.GetSqlServerInstanceNames();
			if (sqlServerInstanceNames.Length != 1 || !string.IsNullOrEmpty(sqlServerInstanceNames[0]))
			{
				string[] array = sqlServerInstanceNames;
				foreach (string item in array)
				{
					txtHostAddress.Items.Add(item);
				}
			}
			return;
		}
		EnableControls(enable: true);
		txtHostAddress.Items.Clear();
		foreach (IPAddress item2 in lanAddress)
		{
			txtHostAddress.Items.Add(item2);
		}
	}

	private void CrawlerForm_EnabledChanged(object sender, EventArgs e)
	{
		btnBack.Enabled = base.Enabled;
		btnMinBox.Enabled = base.Enabled;
		btnClose.Enabled = base.Enabled;
	}

	private void group1_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
	}

	private void grdLedgers_MouseMove(object sender, MouseEventArgs e)
	{
		int mouseRow = grdLedgers.MouseRow;
		int mouseCol = grdLedgers.MouseCol;
		if (mouseCol == grdLedgers.Cols["State"].Index && mouseRow > 0 && grdLedgers.Rows[mouseRow].UserData is ProgressInfo progressInfo && progressInfo.Current >= progressInfo.Max)
		{
			grdLedgers.Cursor = Cursors.Hand;
		}
		else
		{
			grdLedgers.Cursor = Cursors.Default;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(global::CrawlerForm.CrawlerForm));
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlHeader = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.pnlMainNotice = new System.Windows.Forms.Panel();
		this.lblMainNotice = new C1.Win.C1Input.C1Label();
		this.lnkException = new System.Windows.Forms.LinkLabel();
		this.btnCrawler = new C1.Win.C1Input.C1Button();
		this.lblSubNotice = new C1.Win.C1Input.C1Label();
		this.c1PictureBox1 = new C1.Win.C1Input.C1PictureBox();
		this.imgSubNotice = new C1.Win.C1Input.C1PictureBox();
		this.imgMainNotice = new C1.Win.C1Input.C1PictureBox();
		this.btnMinBox = new C1.Win.C1Input.C1Button();
		this.btnBack = new C1.Win.C1Input.C1Button();
		this.btnScan = new C1.Win.C1Input.C1Button();
		this.lblTitle = new C1.Win.C1Input.C1Label();
		this.btnClose = new C1.Win.C1Input.C1Button();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.pnlTabDock = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlHeader.SuspendLayout();
		this.pnlMainNotice.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lblMainNotice).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCrawler).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblSubNotice).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1PictureBox1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.imgSubNotice).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.imgMainNotice).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnMinBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnBack).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnScan).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblTitle).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).BeginInit();
		base.SuspendLayout();
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlHeader);
		this.ctnAll.Panels.Add(this.pnlTabDock);
		this.ctnAll.Size = new System.Drawing.Size(733, 649);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		this.pnlHeader.BackgroundImage = (System.Drawing.Image)resources.GetObject("pnlHeader.BackgroundImage");
		this.pnlHeader.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.pnlHeader.Controls.Add(this.pnlMainNotice);
		this.pnlHeader.Controls.Add(this.btnCrawler);
		this.pnlHeader.Controls.Add(this.lblSubNotice);
		this.pnlHeader.Controls.Add(this.c1PictureBox1);
		this.pnlHeader.Controls.Add(this.imgSubNotice);
		this.pnlHeader.Controls.Add(this.imgMainNotice);
		this.pnlHeader.Controls.Add(this.btnMinBox);
		this.pnlHeader.Controls.Add(this.btnBack);
		this.pnlHeader.Controls.Add(this.btnScan);
		this.pnlHeader.Controls.Add(this.lblTitle);
		this.pnlHeader.Controls.Add(this.btnClose);
		this.pnlHeader.Controls.Add(this.progressBar1);
		this.pnlHeader.Height = 120;
		this.pnlHeader.KeepRelativeSize = false;
		this.pnlHeader.Location = new System.Drawing.Point(0, 0);
		this.pnlHeader.Name = "pnlHeader";
		this.pnlHeader.Size = new System.Drawing.Size(733, 120);
		this.pnlHeader.SizeRatio = 18.49;
		this.pnlHeader.TabIndex = 0;
		this.pnlMainNotice.BackColor = System.Drawing.Color.Transparent;
		this.pnlMainNotice.Controls.Add(this.lblMainNotice);
		this.pnlMainNotice.Controls.Add(this.lnkException);
		this.pnlMainNotice.Location = new System.Drawing.Point(128, 31);
		this.pnlMainNotice.Name = "pnlMainNotice";
		this.pnlMainNotice.Size = new System.Drawing.Size(449, 33);
		this.pnlMainNotice.TabIndex = 13;
		this.lblMainNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMainNotice.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblMainNotice.Font = new System.Drawing.Font("Microsoft YaHei", 15f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblMainNotice.Location = new System.Drawing.Point(63, 0);
		this.lblMainNotice.Name = "lblMainNotice";
		this.lblMainNotice.Size = new System.Drawing.Size(386, 33);
		this.lblMainNotice.TabIndex = 3;
		this.lblMainNotice.Tag = null;
		this.lblMainNotice.Text = "准备采数模块";
		this.lblMainNotice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.lblMainNotice.TextDetached = true;
		this.lblMainNotice.MouseDown += new System.Windows.Forms.MouseEventHandler(lblMainNotice_MouseDown);
		this.lnkException.BackColor = System.Drawing.Color.Transparent;
		this.lnkException.Dock = System.Windows.Forms.DockStyle.Left;
		this.lnkException.Location = new System.Drawing.Point(0, 0);
		this.lnkException.Name = "lnkException";
		this.lnkException.Size = new System.Drawing.Size(63, 33);
		this.lnkException.TabIndex = 12;
		this.lnkException.TabStop = true;
		this.lnkException.Text = "查看详细";
		this.lnkException.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lnkException.Visible = false;
		this.btnCrawler.FlatAppearance.BorderSize = 0;
		this.btnCrawler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCrawler.Font = new System.Drawing.Font("Microsoft YaHei", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCrawler.Location = new System.Drawing.Point(605, 49);
		this.btnCrawler.Name = "btnCrawler";
		this.btnCrawler.Size = new System.Drawing.Size(100, 40);
		this.btnCrawler.TabIndex = 11;
		this.btnCrawler.Text = "扫描本地";
		this.btnCrawler.UseVisualStyleBackColor = true;
		this.btnCrawler.Click += new System.EventHandler(btnCrawler_Click);
		this.lblSubNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblSubNotice.Font = new System.Drawing.Font("Microsoft YaHei", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSubNotice.Location = new System.Drawing.Point(128, 66);
		this.lblSubNotice.MaximumSize = new System.Drawing.Size(450, 58);
		this.lblSubNotice.Name = "lblSubNotice";
		this.lblSubNotice.Size = new System.Drawing.Size(449, 50);
		this.lblSubNotice.TabIndex = 10;
		this.lblSubNotice.Tag = null;
		this.lblSubNotice.Text = "正在准备加载程序的各采数模块……";
		this.lblSubNotice.TextDetached = true;
		this.lblSubNotice.MouseDown += new System.Windows.Forms.MouseEventHandler(lblSubNotice_MouseDown);
		this.c1PictureBox1.BackColor = System.Drawing.Color.Transparent;
		this.c1PictureBox1.Image = Resources.数据采集;
		this.c1PictureBox1.Location = new System.Drawing.Point(10, 8);
		this.c1PictureBox1.Name = "c1PictureBox1";
		this.c1PictureBox1.Size = new System.Drawing.Size(16, 16);
		this.c1PictureBox1.TabIndex = 9;
		this.c1PictureBox1.TabStop = false;
		this.imgSubNotice.BackColor = System.Drawing.Color.Transparent;
		this.imgSubNotice.Image = Resources.扁平放大镜48;
		this.imgSubNotice.Location = new System.Drawing.Point(45, 55);
		this.imgSubNotice.Name = "imgSubNotice";
		this.imgSubNotice.Size = new System.Drawing.Size(48, 48);
		this.imgSubNotice.TabIndex = 8;
		this.imgSubNotice.TabStop = false;
		this.imgMainNotice.BackColor = System.Drawing.Color.Transparent;
		this.imgMainNotice.Image = Resources.扁平电脑72;
		this.imgMainNotice.Location = new System.Drawing.Point(21, 31);
		this.imgMainNotice.Name = "imgMainNotice";
		this.imgMainNotice.Size = new System.Drawing.Size(72, 72);
		this.imgMainNotice.TabIndex = 7;
		this.imgMainNotice.TabStop = false;
		this.btnMinBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnMinBox.FlatAppearance.BorderSize = 0;
		this.btnMinBox.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.btnMinBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnMinBox.Image = Resources.min;
		this.btnMinBox.Location = new System.Drawing.Point(670, 0);
		this.btnMinBox.Name = "btnMinBox";
		this.btnMinBox.Size = new System.Drawing.Size(30, 30);
		this.btnMinBox.TabIndex = 6;
		this.btnMinBox.UseVisualStyleBackColor = true;
		this.btnMinBox.Click += new System.EventHandler(btnMinBox_Click);
		this.btnBack.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnBack.FlatAppearance.BorderSize = 0;
		this.btnBack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnBack.Image = Resources.back;
		this.btnBack.Location = new System.Drawing.Point(637, 0);
		this.btnBack.Name = "btnBack";
		this.btnBack.Size = new System.Drawing.Size(30, 30);
		this.btnBack.TabIndex = 5;
		this.btnBack.UseVisualStyleBackColor = true;
		this.btnBack.Click += new System.EventHandler(btnBack_Click);
		this.btnScan.FlatAppearance.BorderSize = 0;
		this.btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnScan.Font = new System.Drawing.Font("Microsoft YaHei", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnScan.Location = new System.Drawing.Point(605, 49);
		this.btnScan.Name = "btnScan";
		this.btnScan.Size = new System.Drawing.Size(100, 40);
		this.btnScan.TabIndex = 4;
		this.btnScan.Text = "扫描本地";
		this.btnScan.UseVisualStyleBackColor = true;
		this.btnScan.Click += new System.EventHandler(btnScan_Click);
		this.btnScan.Enter += new System.EventHandler(btnCertain_Enter);
		this.lblTitle.AutoSize = true;
		this.lblTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblTitle.Location = new System.Drawing.Point(31, 8);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(68, 17);
		this.lblTitle.TabIndex = 2;
		this.lblTitle.Tag = null;
		this.lblTitle.Text = "AuditAI采数器";
		this.lblTitle.TextDetached = true;
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Image = Resources.close;
		this.btnClose.Location = new System.Drawing.Point(703, 0);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(30, 30);
		this.btnClose.TabIndex = 1;
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.progressBar1.Location = new System.Drawing.Point(0, 117);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(733, 3);
		this.progressBar1.Step = 1;
		this.progressBar1.TabIndex = 0;
		this.pnlTabDock.Height = 529;
		this.pnlTabDock.Location = new System.Drawing.Point(0, 120);
		this.pnlTabDock.Name = "pnlTabDock";
		this.pnlTabDock.Size = new System.Drawing.Size(733, 529);
		this.pnlTabDock.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(733, 649);
		base.Controls.Add(this.ctnAll);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "CrawlerForm";
		this.Text = "AuditAI采数器";
		base.TopMost = true;
		base.Load += new System.EventHandler(CrawlerForm_Load);
		base.Shown += new System.EventHandler(CrawlerForm_Shown);
		base.EnabledChanged += new System.EventHandler(CrawlerForm_EnabledChanged);
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlHeader.ResumeLayout(false);
		this.pnlHeader.PerformLayout();
		this.pnlMainNotice.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.lblMainNotice).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCrawler).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblSubNotice).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1PictureBox1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.imgSubNotice).EndInit();
		((System.ComponentModel.ISupportInitialize)this.imgMainNotice).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnMinBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnBack).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnScan).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblTitle).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).EndInit();
		base.ResumeLayout(false);
	}
}
