﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Platform.Properties;
using TXTextControl;

namespace Auditai.UI.Platform;

public class FormHeaderEdit
{
	private const string EMPTY_RTF = "{\\rtf1";

	private bool _preventClosing;

	private readonly TextControlEx _tx;

	private readonly Form _form;

	private readonly C1Button _btnOk;

	private readonly C1Button _btnCancel;

	private readonly C1SplitContainer _splc;

	private readonly C1SplitterPanel _pnlBottom;

	private readonly C1SplitterPanel _pnlMain;

	private readonly C1ContextMenu _ctx;

	private readonly C1Command _cmdFont;

	private readonly C1CommandLink _lnkFont;

	private readonly C1Command _cmdParagraph;

	private readonly C1CommandLink _lnkParagraph;

	private readonly C1Command _cmdInsertPageNo;

	private readonly C1CommandLink _lnkInsertPageNo;

	private readonly C1Command _cmdInsertPageCount;

	private readonly C1CommandLink _lnkInsertPageCount;

	private readonly C1Command _cmdInsertIndex;

	private readonly C1CommandLink _lnkInsertIndex;

	private readonly C1Command _cmdInsertVariable;

	private readonly C1CommandLink _lnkInsertVariable;

	private readonly C1Command _cmdInsertTable;

	private readonly C1CommandLink _lnkInsertTable;

	private readonly C1Command _cmdInsertImage;

	private readonly C1CommandLink _lnkInsertImage;

	public DialogResult DialogResult { get; private set; }

	public string PlainText => _tx.Text;

	public TXTextControl.HorizontalAlignment HorizontalAlignment { get; set; } = TXTextControl.HorizontalAlignment.Left;


	public string Rtf
	{
		get
		{
			_tx.SelectAll();
			_tx.Selection.ParagraphFormat.Alignment = HorizontalAlignment;
			_tx.Save(out var stringData, StringStreamType.RichTextFormat);
			return stringData;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				_tx.Load("{\\rtf1", StringStreamType.RichTextFormat);
				return;
			}
			try
			{
				_tx.Load(value, StringStreamType.RichTextFormat);
			}
			catch
			{
				_tx.Load("{\\rtf1", StringStreamType.RichTextFormat);
			}
		}
	}

	public event FormClosedEventHandler Closed;

	public FormHeaderEdit()
	{
		_form = FormFactory.Create();
		_form.StartPosition = FormStartPosition.Manual;
		_form.FormBorderStyle = FormBorderStyle.None;
		_form.Size = new Size(300, 200);
		_form.ShowInTaskbar = false;
		_form.Load += _form_Load;
		_form.Deactivate += _form_Deactivate;
		_form.FormClosed += _form_FormClosed;
		_splc = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_form.Controls.Add(_splc);
		_pnlBottom = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Bottom,
			Resizable = false,
			Collapsible = false,
			KeepRelativeSize = false,
			MinHeight = 0,
			Height = 30
		};
		_splc.Panels.Add(_pnlBottom);
		_btnOk = new C1Button
		{
			Text = "确定",
			Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right),
			Dock = DockStyle.None,
			Location = new Point(-200, 0)
		};
		_btnOk.Click += _btnOk_Click;
		_pnlBottom.Controls.Add(_btnOk);
		_btnCancel = new C1Button
		{
			Text = "取消",
			Anchor = AnchorStyles.Right,
			Dock = DockStyle.None
		};
		_btnCancel.Click += _btnCancel_Click;
		_pnlBottom.Controls.Add(_btnCancel);
		_pnlMain = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Resizable = false,
			Collapsible = false,
			KeepRelativeSize = false,
			MinHeight = 0,
			SizeRatio = 100.0
		};
		_splc.Panels.Add(_pnlMain);
		try
		{
			_tx = new TextControlEx
			{
				BorderStyle = TXTextControl.BorderStyle.FixedSingle,
				Dock = DockStyle.Fill,
				ViewMode = ViewMode.SimpleControl
			};
			_tx.TextContextMenuOpening += _tx_TextContextMenuOpening;
			_pnlMain.Controls.Add(_tx);
		}
		catch (Exception)
		{
		}
		_ctx = new C1ContextMenu();
		_cmdFont = new C1Command
		{
			Text = "字体",
			Image = ContextResources.ctxFont
		};
		_cmdFont.Click += _cmdFont_Click;
		_cmdParagraph = new C1Command
		{
			Text = "段落",
			Image = ContextResources.ctxParagraph
		};
		_cmdParagraph.Click += _cmdParagraph_Click;
		_cmdInsertPageNo = new C1Command
		{
			Text = "插入页码"
		};
		_cmdInsertPageNo.Click += _cmdInsertPageNo_Click;
		_cmdInsertPageCount = new C1Command
		{
			Text = "插入页数"
		};
		_cmdInsertPageCount.Click += _cmdInsertPageCount_Click;
		_cmdInsertIndex = new C1Command
		{
			Text = "插入索引号"
		};
		_cmdInsertIndex.Click += _cmdInsertIndex_Click;
		_cmdInsertVariable = new C1Command
		{
			Text = "插入变量"
		};
		_cmdInsertVariable.Click += _cmdInsertVariable_Click;
		_cmdInsertTable = new C1Command
		{
			Text = "插入表格",
			Image = ContextResources.ctxInsertTable
		};
		_cmdInsertTable.Click += _cmdInsertTable_Click;
		_cmdInsertImage = new C1Command
		{
			Text = "插入图片",
			Image = ContextResources.ctxInsertImage
		};
		_cmdInsertImage.Click += _cmdInsertImage_Click;
		_lnkFont = new C1CommandLink(_cmdFont);
		_ctx.CommandLinks.Add(_lnkFont);
		_lnkParagraph = new C1CommandLink(_cmdParagraph);
		_ctx.CommandLinks.Add(_lnkParagraph);
		_lnkInsertPageNo = new C1CommandLink(_cmdInsertPageNo);
		_ctx.CommandLinks.Add(_lnkInsertPageNo);
		_lnkInsertPageCount = new C1CommandLink(_cmdInsertPageCount);
		_ctx.CommandLinks.Add(_lnkInsertPageCount);
		_lnkInsertIndex = new C1CommandLink(_cmdInsertIndex);
		_ctx.CommandLinks.Add(_lnkInsertIndex);
		_lnkInsertVariable = new C1CommandLink(_cmdInsertVariable);
		_ctx.CommandLinks.Add(_lnkInsertVariable);
		_lnkInsertTable = new C1CommandLink(_cmdInsertTable);
		_ctx.CommandLinks.Add(_lnkInsertTable);
		_lnkInsertImage = new C1CommandLink(_cmdInsertImage);
		_ctx.CommandLinks.Add(_lnkInsertImage);
	}

	public void Show(Point location)
	{
		_form.Location = location;
		_form.Show();
		_tx.Focus();
	}

	public void SetFont()
	{
		_preventClosing = true;
		_tx.FontDialog();
		_preventClosing = false;
	}

	public void SetParagraphFormat()
	{
		_preventClosing = true;
		_tx.ParagraphFormatDialog();
		_preventClosing = false;
	}

	public void InsertPageNo()
	{
		_tx.Selection.Text = "[PageNo]";
	}

	public void InsertPageCount()
	{
		_tx.Selection.Text = "[PageCount]";
	}

	public void InsertIndex()
	{
		_tx.Selection.Text = "[索引号]";
	}

	public void InsertVariable()
	{
		_preventClosing = true;
		ReferenceEditor referenceEditor = new ReferenceEditor();
		if (referenceEditor.ShowSelect() == DialogResult.OK)
		{
			DataReference selectedReference = referenceEditor.SelectedReference;
			_tx.Selection.Text = "[" + selectedReference.Key + "]";
		}
		_preventClosing = false;
	}

	public void InsertTable()
	{
		_preventClosing = true;
		_tx.Tables.Add();
		_preventClosing = false;
	}

	public void InsertImage()
	{
		_preventClosing = true;
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Title = "插入图像",
			Filter = "图像文件|*.bmp;*.jpg;*.tif;*.tiff;*.wmf;*.png;*.jpeg;*.gif;*.emf;*.ico;"
		};
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			Bitmap bitmap = new Bitmap(openFileDialog.FileName);
			using MemoryStream stream = new MemoryStream();
			bitmap.Save(stream, ImageFormat.Bmp);
			System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
			TXTextControl.Image image2 = new TXTextControl.Image(image);
			_tx.Images.Add(image2, _tx.InputPosition.TextPosition);
		}
		_preventClosing = false;
	}

	private void _tx_TextContextMenuOpening(object sender, TextContextMenuEventArgs e)
	{
		e.Cancel = true;
		_ctx.ShowContextMenu(_tx, _tx.PointToClient(e.Location));
	}

	private void _form_Load(object sender, EventArgs e)
	{
		_btnOk.Location = new Point(100, 2);
		_btnOk.Size = new Size(80, 26);
		_btnCancel.Location = new Point(200, 2);
		_btnCancel.Size = new Size(80, 26);
	}

	private void _form_Deactivate(object sender, EventArgs e)
	{
		if (!_preventClosing)
		{
			_ctx.CloseContextMenu();
			DialogResult = DialogResult.OK;
			_form.Close();
		}
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		_ctx.CloseContextMenu();
		DialogResult = DialogResult.Cancel;
		_form.Close();
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		_ctx.CloseContextMenu();
		DialogResult = DialogResult.OK;
		_form.Close();
	}

	private void _form_FormClosed(object sender, FormClosedEventArgs e)
	{
		_form.Deactivate -= _form_Deactivate;
		this.Closed?.Invoke(sender, e);
	}

	private void _cmdInsertImage_Click(object sender, ClickEventArgs e)
	{
		InsertImage();
	}

	private void _cmdInsertTable_Click(object sender, ClickEventArgs e)
	{
		InsertTable();
	}

	private void _cmdInsertVariable_Click(object sender, ClickEventArgs e)
	{
		InsertVariable();
	}

	private void _cmdInsertIndex_Click(object sender, ClickEventArgs e)
	{
		InsertIndex();
	}

	private void _cmdInsertPageCount_Click(object sender, ClickEventArgs e)
	{
		InsertPageCount();
	}

	private void _cmdInsertPageNo_Click(object sender, ClickEventArgs e)
	{
		InsertPageNo();
	}

	private void _cmdParagraph_Click(object sender, ClickEventArgs e)
	{
		SetParagraphFormat();
	}

	private void _cmdFont_Click(object sender, ClickEventArgs e)
	{
		SetFont();
	}
}
