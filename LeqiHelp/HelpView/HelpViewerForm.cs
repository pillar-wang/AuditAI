using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using HelpView.Properties;
using Leqisoft.UI.Controls;
using PdfiumViewer;

namespace HelpView;

public class HelpViewerForm : C1RibbonForm
{
	private readonly DocFindFactory docReplaceFactory = new DocFindFactory();

	private readonly PdfViewer pdfViewer = new PdfViewer();

	private string _filePath;

	private PdfMatches _matches;

	private string _keyword;

	private int _current;

	private IContainer components = null;

	private C1ToolBar c1ToolBar1;

	private C1CommandHolder c1CommandHolder1;

	private C1CommandLink lnkFind;

	private C1Command cmdNextPage;

	private C1Command cmdPreviousPage;

	private C1Command cmdNavigate;

	private C1CommandLink lnkNavigate;

	private C1CommandLink lnkPreviousPage;

	private C1CommandLink lnkNextPage;

	private C1Command cmdFind;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlToolbar;

	private C1SplitterPanel pnlPdfView;

	private C1Command cmdExport;

	private C1Command cmdPrint;

	private C1CommandLink lnkExport;

	private C1CommandLink lnkPrint;

	public HelpViewerForm()
	{
		InitializeComponent();
		c1ToolBar1.ButtonWidth = 60;
		pdfViewer.ZoomMode = PdfViewerZoomMode.FitBest;
		pdfViewer.ShowToolbar = false;
		pdfViewer.Dock = DockStyle.Fill;
		pdfViewer.Load += PdfViewer_Load;
		pdfViewer.Paint += PdfViewer_Paint;
		pnlPdfView.Controls.Add(pdfViewer);
		base.Shown += HelpViewerForm_Shown;
		cmdNavigate.Checked = true;
		cmdNavigate.CheckAutoToggle = true;
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
	}

	private void HelpViewerForm_Shown(object sender, EventArgs e)
	{
		using (FileStream stream = new FileStream("./aaa.txt", FileMode.Create, FileAccess.Write))
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			string location = Assembly.GetExecutingAssembly().Location;
			if (!LoadPdf(Path.GetDirectoryName(location) + "\\帮助文档.pdf"))
			{
				return;
			}
			streamWriter.WriteLine("defaultBookmark:" + Program.DefaultBookmark);
			if (string.IsNullOrWhiteSpace(Program.DefaultBookmark))
			{
				return;
			}
			PdfBookmarkCollection bookmarks = pdfViewer.Document.Bookmarks;
			foreach (PdfBookmark item in bookmarks)
			{
				PdfBookmark pdfBookmark = findBookmark(item, Program.DefaultBookmark);
				if (pdfBookmark != null)
				{
					streamWriter.WriteLine($"pageIndex:{pdfBookmark.PageIndex}");
					pdfViewer.Renderer.Page = pdfBookmark.PageIndex;
					break;
				}
			}
		}
		static PdfBookmark findBookmark(PdfBookmark bookmark, string title)
		{
			if (bookmark == null)
			{
				return null;
			}
			if (bookmark.Title.Replace(" ", "") == title?.Replace(" ", ""))
			{
				return bookmark;
			}
			foreach (PdfBookmark child in bookmark.Children)
			{
				PdfBookmark pdfBookmark2 = findBookmark(child, title);
				if (pdfBookmark2 != null)
				{
					return pdfBookmark2;
				}
			}
			return null;
		}
	}

	private void PdfViewer_Load(object sender, EventArgs e)
	{
		pdfViewer.Renderer.Page = 0;
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
	}

	public bool LoadPdf(string file)
	{
		if (!File.Exists(file))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "数据源不存在！", MessageBoxButtons.OK, "", false);
			return false;
		}
		try
		{
			pdfViewer.Document = PdfDocument.Load(new FileStream(file, FileMode.Open, FileAccess.Read));
			pdfViewer.Renderer.Page = 0;
			_matches = null;
			_keyword = null;
			_current = 0;
			_filePath = file;
			pdfViewer.Renderer.Zoom = 1.5;
			return true;
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开文件失败！" + ex.Message, MessageBoxButtons.OK, "", false);
			return false;
		}
	}

	private void PdfViewer_Paint(object sender, PaintEventArgs e)
	{
	}

	private void cmdNavigate_Click(object sender, ClickEventArgs e)
	{
		pdfViewer.ShowBookmarks = cmdNavigate.Checked;
	}

	private void cmdPreviousPage_Click(object sender, ClickEventArgs e)
	{
		if (pdfViewer.Document == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前文档为空！", MessageBoxButtons.OK, "", false);
		}
		else if (pdfViewer.Renderer.Page <= 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已到第一页！", MessageBoxButtons.OK, "", false);
		}
		else
		{
			pdfViewer.Renderer.Page--;
		}
	}

	private void cmdNextPage_Click(object sender, ClickEventArgs e)
	{
		if (pdfViewer.Document == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前文档为空！", MessageBoxButtons.OK, "", false);
			return;
		}
		int pageCount = pdfViewer.Document.PageCount;
		if (pdfViewer.Renderer.Page >= pageCount - 1)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已到最后一页！", MessageBoxButtons.OK, "", false);
		}
		else
		{
			pdfViewer.Renderer.Page++;
		}
	}

	private void cmdFind_Click(object sender, ClickEventArgs e)
	{
		if (pdfViewer.Document == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前文档为空！", MessageBoxButtons.OK, "", false);
			return;
		}
		DocFindInstance docFindInstance = docReplaceFactory.Get();
		docFindInstance.Find_NextKeyDown += Form_Find_NextKeyDown;
		docFindInstance.ShowOnlyFind();
	}

	private void Form_Find_NextKeyDown(object sender, FindReplaceEventArgs e)
	{
		if (pdfViewer.Document == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前文档为空！", MessageBoxButtons.OK, "", false);
			return;
		}
		IPdfDocument document = pdfViewer.Document;
		if (_matches == null || e.SearchText != _keyword)
		{
			_matches = document.Search(e.SearchText, e.SearchFlag.HasFlag(SearchFlag.WholeWord), e.SearchFlag.HasFlag(SearchFlag.Search));
			_keyword = e.SearchText;
			_current = 0;
		}
		if (_matches.Items.Count == 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "未找到匹配项！", MessageBoxButtons.OK, "", false);
		}
		else if (_current >= _matches.Items.Count)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已到最后一个项！", MessageBoxButtons.OK, "", false);
			_current = 0;
		}
		else
		{
			PdfMatch pdfMatch = _matches.Items[_current++];
			IList<PdfRectangle> textBounds = document.GetTextBounds(pdfMatch.TextSpan);
			pdfViewer.Renderer.ScrollIntoView(textBounds[0]);
		}
	}

	private void cmdExport_Click(object sender, ClickEventArgs e)
	{
		if (pdfViewer.Document == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前文档为空！", MessageBoxButtons.OK, "", false);
		}
		else if (!string.IsNullOrEmpty(_filePath) && File.Exists(_filePath))
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "PDF文件|*.pdf"
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				File.Copy(_filePath, saveFileDialog.FileName, overwrite: true);
			}
		}
	}

	private void cmdPrint_Click(object sender, ClickEventArgs e)
	{
		if (pdfViewer.Document == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前文档为空！", MessageBoxButtons.OK, "", false);
			return;
		}
		PrintDialog printDialog = new PrintDialog();
		if (printDialog.ShowDialog() == DialogResult.OK)
		{
			PrintDocument printDocument = pdfViewer.Document?.CreatePrintDocument();
			if (printDocument != null)
			{
				printDocument.PrinterSettings = printDialog.PrinterSettings;
				printDocument.Print();
			}
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpView.HelpViewerForm));
		this.c1ToolBar1 = new C1.Win.C1Command.C1ToolBar();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		this.cmdNextPage = new C1.Win.C1Command.C1Command();
		this.cmdPreviousPage = new C1.Win.C1Command.C1Command();
		this.cmdNavigate = new C1.Win.C1Command.C1Command();
		this.cmdFind = new C1.Win.C1Command.C1Command();
		this.cmdExport = new C1.Win.C1Command.C1Command();
		this.cmdPrint = new C1.Win.C1Command.C1Command();
		this.lnkNavigate = new C1.Win.C1Command.C1CommandLink();
		this.lnkPreviousPage = new C1.Win.C1Command.C1CommandLink();
		this.lnkNextPage = new C1.Win.C1Command.C1CommandLink();
		this.lnkFind = new C1.Win.C1Command.C1CommandLink();
		this.lnkExport = new C1.Win.C1Command.C1CommandLink();
		this.lnkPrint = new C1.Win.C1Command.C1CommandLink();
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlToolbar = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.pnlPdfView = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlToolbar.SuspendLayout();
		base.SuspendLayout();
		this.c1ToolBar1.AccessibleName = "Tool Bar";
		this.c1ToolBar1.AutoSize = false;
		this.c1ToolBar1.ButtonLayoutHorz = C1.Win.C1Command.ButtonLayoutEnum.TextBelow;
		this.c1ToolBar1.ButtonLookHorz = C1.Win.C1Command.ButtonLookFlags.TextAndImage;
		this.c1ToolBar1.CommandHolder = this.c1CommandHolder1;
		this.c1ToolBar1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[6] { this.lnkNavigate, this.lnkPreviousPage, this.lnkNextPage, this.lnkFind, this.lnkExport, this.lnkPrint });
		this.c1ToolBar1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1ToolBar1.Location = new System.Drawing.Point(0, 0);
		this.c1ToolBar1.MinButtonSize = 42;
		this.c1ToolBar1.Movable = false;
		this.c1ToolBar1.Name = "c1ToolBar1";
		this.c1ToolBar1.Size = new System.Drawing.Size(784, 70);
		this.c1ToolBar1.Text = "c1ToolBar1";
		this.c1ToolBar1.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.c1ToolBar1.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.c1CommandHolder1.Commands.Add(this.cmdNextPage);
		this.c1CommandHolder1.Commands.Add(this.cmdPreviousPage);
		this.c1CommandHolder1.Commands.Add(this.cmdNavigate);
		this.c1CommandHolder1.Commands.Add(this.cmdFind);
		this.c1CommandHolder1.Commands.Add(this.cmdExport);
		this.c1CommandHolder1.Commands.Add(this.cmdPrint);
		this.c1CommandHolder1.Owner = this;
		this.cmdNextPage.Image = HelpView.Properties.Resources.forward32;
		this.cmdNextPage.Name = "cmdNextPage";
		this.cmdNextPage.ShortcutText = "";
		this.cmdNextPage.Text = "下一页";
		this.cmdNextPage.Click += new C1.Win.C1Command.ClickEventHandler(cmdNextPage_Click);
		this.cmdPreviousPage.Image = HelpView.Properties.Resources.back32;
		this.cmdPreviousPage.Name = "cmdPreviousPage";
		this.cmdPreviousPage.ShortcutText = "";
		this.cmdPreviousPage.Text = "上一页";
		this.cmdPreviousPage.Click += new C1.Win.C1Command.ClickEventHandler(cmdPreviousPage_Click);
		this.cmdNavigate.Image = HelpView.Properties.Resources.DocumentStructure;
		this.cmdNavigate.Name = "cmdNavigate";
		this.cmdNavigate.ShortcutText = "";
		this.cmdNavigate.Text = "导航栏";
		this.cmdNavigate.Click += new C1.Win.C1Command.ClickEventHandler(cmdNavigate_Click);
		this.cmdFind.Image = HelpView.Properties.Resources.Find;
		this.cmdFind.Name = "cmdFind";
		this.cmdFind.ShortcutText = "";
		this.cmdFind.Text = " 查找 ";
		this.cmdFind.Click += new C1.Win.C1Command.ClickEventHandler(cmdFind_Click);
		this.cmdExport.Image = HelpView.Properties.Resources.PdfExport;
		this.cmdExport.Name = "cmdExport";
		this.cmdExport.ShortcutText = "";
		this.cmdExport.Text = "导出pdf";
		this.cmdExport.Click += new C1.Win.C1Command.ClickEventHandler(cmdExport_Click);
		this.cmdPrint.Image = HelpView.Properties.Resources.Print;
		this.cmdPrint.Name = "cmdPrint";
		this.cmdPrint.ShortcutText = "";
		this.cmdPrint.Text = "打印";
		this.cmdPrint.Click += new C1.Win.C1Command.ClickEventHandler(cmdPrint_Click);
		this.lnkNavigate.Command = this.cmdNavigate;
		this.lnkPreviousPage.Command = this.cmdPreviousPage;
		this.lnkPreviousPage.SortOrder = 1;
		this.lnkPreviousPage.Text = "上一页";
		this.lnkNextPage.Command = this.cmdNextPage;
		this.lnkNextPage.SortOrder = 2;
		this.lnkNextPage.Text = "下一页";
		this.lnkFind.Command = this.cmdFind;
		this.lnkFind.SortOrder = 3;
		this.lnkFind.Text = "查找";
		this.lnkExport.Command = this.cmdExport;
		this.lnkExport.SortOrder = 4;
		this.lnkPrint.Command = this.cmdPrint;
		this.lnkPrint.SortOrder = 5;
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctnAll.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlToolbar);
		this.ctnAll.Panels.Add(this.pnlPdfView);
		this.ctnAll.Size = new System.Drawing.Size(784, 761);
		this.ctnAll.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 1;
		this.ctnAll.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlToolbar.Controls.Add(this.c1ToolBar1);
		this.pnlToolbar.Height = 70;
		this.pnlToolbar.KeepRelativeSize = false;
		this.pnlToolbar.Location = new System.Drawing.Point(0, 0);
		this.pnlToolbar.Name = "pnlToolbar";
		this.pnlToolbar.Resizable = false;
		this.pnlToolbar.Size = new System.Drawing.Size(784, 70);
		this.pnlToolbar.SizeRatio = 9.211;
		this.pnlToolbar.TabIndex = 0;
		this.pnlPdfView.Height = 690;
		this.pnlPdfView.Location = new System.Drawing.Point(0, 71);
		this.pnlPdfView.Name = "pnlPdfView";
		this.pnlPdfView.Size = new System.Drawing.Size(784, 690);
		this.pnlPdfView.TabIndex = 1;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		base.ClientSize = new System.Drawing.Size(784, 761);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "HelpViewerForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "帮助";
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlToolbar.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
