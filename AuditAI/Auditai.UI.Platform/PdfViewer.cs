using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using PdfiumViewer;

namespace Auditai.UI.Platform;

public class PdfViewer
{
	private PdfiumViewer.PdfViewer _pv = new PdfiumViewer.PdfViewer
	{
		Dock = DockStyle.Fill,
		ShowToolbar = false,
		ShowBookmarks = true
	};

	private readonly C1Command cmdBack = new C1Command
	{
		Image = Resources.back32,
		Text = "后退"
	};

	private readonly C1CommandLink lnkBack = new C1CommandLink();

	private readonly C1Command cmdForward = new C1Command
	{
		Image = Resources.forward32,
		Text = "前进"
	};

	private readonly C1CommandLink lnkForward = new C1CommandLink();

	private readonly C1Command cmdHideToolbar = new C1Command
	{
		Text = "隐藏侧边栏",
		Image = Resources.HideSideToolbar
	};

	private readonly C1CommandLink lnkZoomIn = new C1CommandLink();

	private readonly C1Command cmdZoomIn = new C1Command
	{
		Text = "放大显示",
		Image = Resources.ZoomIn
	};

	private readonly C1CommandLink lnkZoomOut = new C1CommandLink();

	private readonly C1Command cmdZoomOut = new C1Command
	{
		Text = "缩小显示",
		Image = Resources.ZoomOut
	};

	private readonly C1CommandLink lnkExportPdf = new C1CommandLink();

	private readonly C1Command cmdExportPdf = new C1Command
	{
		Text = "导出Pdf",
		Image = Resources.PdfExport
	};

	private readonly C1CommandLink lnkHideToolbar = new C1CommandLink();

	private readonly C1Command _cmdHelpCenter;

	private readonly C1CommandLink _lnkHelpCenter;

	private C1SplitterPanel _pnlToolbar = new C1SplitterPanel
	{
		Collapsible = false,
		Dock = PanelDockStyle.Right,
		Width = 80,
		KeepRelativeSize = false,
		Resizable = false
	};

	private C1ToolBar _toolBar = new C1ToolBar
	{
		Horizontal = false,
		Dock = DockStyle.Fill,
		ButtonLookVert = ButtonLookFlags.TextAndImage,
		MinButtonSize = 40,
		ShowToolTips = false
	};

	private int _onVisibleViewOffsetX;

	private int _onVisibleViewOffsetY;

	private double _onVisibleViewZoomFactor = 1.5;

	public C1SplitContainer View { get; } = new C1SplitContainer
	{
		Dock = DockStyle.Fill
	};


	public Pdf Pdf { get; set; }

	public PdfViewer()
	{
		View.Panels.Add(_pnlToolbar);
		_pnlToolbar.Controls.Add(_toolBar);
		View.Panels.Add(new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left
		});
		_pv.Renderer.DisplayRectangleChanged += PdfRenderer_OnDisplayRectangleChanged;
		_pv.Renderer.ZoomChanged += PdfRenderer_OnZoomChanged;
		View.Panels[1].Controls.Add(_pv);
		cmdBack.Click += CmdBack_Click;
		lnkBack.Command = cmdBack;
		lnkBack.Delimiter = true;
		cmdForward.Click += CmdForward_Click;
		lnkForward.Command = cmdForward;
		lnkHideToolbar.Delimiter = true;
		cmdHideToolbar.Click += CmdHideToolbar_Click;
		lnkHideToolbar.Command = cmdHideToolbar;
		cmdZoomIn.Click += CmdZoomIn_Click;
		lnkZoomIn.Command = cmdZoomIn;
		cmdZoomOut.Click += CmdZoomOut_Click;
		lnkZoomOut.Command = cmdZoomOut;
		cmdExportPdf.Click += CmdExportPdf_Click;
		lnkExportPdf.Command = cmdExportPdf;
		_cmdHelpCenter = new C1Command
		{
			Text = "帮助中心",
			Image = Resources.HelpCenter,
			Visible = SoftwareLicenseManager.IsShowHelpDocumentButton()
		};
		_cmdHelpCenter.Click += _cmdHelpCenter_Click;
		_lnkHelpCenter = new C1CommandLink(_cmdHelpCenter);
		_lnkHelpCenter.Delimiter = true;
		_toolBar.CommandLinks.AddRange(new C1CommandLink[4] { lnkZoomIn, lnkZoomOut, lnkExportPdf, _lnkHelpCenter });
		RibbonImageProcess imageProcess = MainForm.ImageProcess;
		foreach (C1CommandLink commandLink in _toolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
	}

	private void PdfRenderer_OnZoomChanged(object sender, EventArgs e)
	{
		if (Pdf != null)
		{
			Pdf.ZoomFactor = _pv.Renderer.Zoom;
		}
	}

	private void PdfRenderer_OnDisplayRectangleChanged(object sender, EventArgs e)
	{
		if (Pdf != null)
		{
			Rectangle displayRectangle = _pv.Renderer.DisplayRectangle;
			Pdf.DisplayAreaOffsetX = displayRectangle.X;
			Pdf.DisplayAreaOffsetY = displayRectangle.Y;
		}
	}

	public void AfterBecomeVisible()
	{
		if (Pdf == null)
		{
			return;
		}
		_pv.Renderer.Zoom = _onVisibleViewZoomFactor;
		_pv.Renderer.SetDisplayRectLocation(new Point(_onVisibleViewOffsetX, _onVisibleViewOffsetY));
		if (Pdf._isFirstOpened)
		{
			Pdf._isFirstOpened = false;
			int width = _pv.Renderer.DisplayRectangle.Width;
			int num = 100;
			if (_pv.Width > num && width > _pv.Width - num)
			{
				float num2 = (float)(_pv.Width - num) * 1f / (float)width;
				_pv.Renderer.Zoom = _onVisibleViewZoomFactor * (double)num2;
			}
		}
	}

	public void Populate()
	{
		string path = Pdf.Project.FileCacheManager.GetPath(Pdf.FileId);
		try
		{
			_onVisibleViewOffsetX = Pdf.DisplayAreaOffsetX;
			_onVisibleViewOffsetY = Pdf.DisplayAreaOffsetY;
			_onVisibleViewZoomFactor = Pdf.ZoomFactor;
			if (_pv.Document != null)
			{
				_pv.Document.Dispose();
			}
			_pv.Document = PdfDocument.Load(new FileStream(path, FileMode.Open, FileAccess.Read));
			_pv.Renderer.Zoom = _onVisibleViewZoomFactor;
			_pv.Renderer.SetDisplayRectLocation(new Point(_onVisibleViewOffsetX, _onVisibleViewOffsetY));
		}
		catch (PdfException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void SetZoomFactor(int percent)
	{
		_pv.Renderer.Zoom = (float)percent / 100f;
	}

	public void Print()
	{
		Print(null);
	}

	public void Print(PrinterSettings ps)
	{
		using PrintDocument printDocument = _pv.Document.CreatePrintDocument();
		if (ps != null)
		{
			printDocument.PrinterSettings = ps;
		}
		printDocument.Print();
	}

	public async Task<DialogResult> SaveDialog()
	{
		if (Pdf == null)
		{
			return DialogResult.No;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "PDF文档 (*.pdf)|*.pdf",
			FileName = Pdf.TreeNode.Number + " " + Pdf.TreeNode.Name,
			DefaultExt = ".pdf"
		};
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			return await Save(saveFileDialog.FileName);
		}
		return DialogResult.Cancel;
	}

	public async Task<DialogResult> Save(string file)
	{
		Pdf.LoadAndReturn();
		if (Pdf.FileId == Guid.Empty)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件损坏");
			return DialogResult.No;
		}
		try
		{
			await Pdf.Project.FileCacheManager.DownloadIfNotExist(Pdf.FileId);
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			return DialogResult.No;
		}
		string path = Pdf.Project.FileCacheManager.GetPath(Pdf.FileId);
		if (!File.Exists(path))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "未发现该文件");
			return DialogResult.No;
		}
		File.Copy(path, file, overwrite: true);
		return DialogResult.OK;
	}

	public void HideToolbar()
	{
		_pnlToolbar.Hide();
	}

	public void ShowToolbar()
	{
		_pnlToolbar.Show();
	}

	private void CmdHideToolbar_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ToggleSideToolbar();
	}

	private void CmdForward_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.Forward();
	}

	private void CmdBack_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.Back();
	}

	private void _cmdHelpCenter_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ShowHelpCenter();
	}

	private void CmdZoomOut_Click(object sender, ClickEventArgs e)
	{
		_pv.Renderer.ZoomOut();
	}

	private void CmdZoomIn_Click(object sender, ClickEventArgs e)
	{
		_pv.Renderer.ZoomIn();
	}

	private void CmdExportPdf_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ExportPdfDocumentDialog();
	}
}
