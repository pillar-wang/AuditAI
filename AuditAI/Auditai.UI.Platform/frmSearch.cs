using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class frmSearch : C1RibbonForm
{
	private const int CN_TREENODE = 0;

	private string _keyword = string.Empty;

	private LazyExcute lazyPopulateExcute = new LazyExcute();

	private bool _isNodesNameCacheGenerated;

	private List<Tuple<TreeNodeBase, string>> _nodesNameCacheList = new List<Tuple<TreeNodeBase, string>>();

	private Dictionary<TreeNodeBase, string> _nodesNameCacheDic = new Dictionary<TreeNodeBase, string>();

	private IContainer components;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlInput;

	private C1SplitterPanel pnlEditor;

	private C1FlexGrid grdEditor;

	private C1TextBox txtKeyword;

	public Project Project { get; set; }

	public event EventHandler<TreeNodeBase> SelectNode;

	public frmSearch()
	{
		InitializeComponent();
		base.ShowInTaskbar = false;
		base.TopMost = true;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormClosing += FrmSearch_FormClosing;
		base.KeyDown += FrmSearch_KeyDown;
		base.VisibleChanged += FrmSearch_VisibleChanged;
		txtKeyword.TextChanged += txtKeyword_TextChanged;
		txtKeyword.KeyDown += TxtKeyword_KeyDown;
		grdEditor.ExtendLastCol = true;
		grdEditor.Rows.DefaultSize = 30;
		grdEditor.AllowEditing = false;
		grdEditor.SelectionMode = SelectionModeEnum.Row;
		grdEditor.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdEditor.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdEditor.DrawFormBorder(e1.Graphics);
		};
		grdEditor.KeyDown += GrdEditor_KeyDown;
		grdEditor.Cols.Count = 1;
		grdEditor.Cols.Fixed = 0;
		grdEditor.Rows.Count = 0;
		lazyPopulateExcute.SetAction(delegate
		{
			grdEditor.BeginUpdate();
			try
			{
				_keyword = txtKeyword.Text.Trim();
				Popualte();
			}
			finally
			{
				grdEditor.EndUpdate();
			}
		});
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
	}

	private void FrmSearch_VisibleChanged(object sender, EventArgs e)
	{
		if (!base.Visible)
		{
			ClearNodeNameCache();
		}
	}

	public void SetKeyword(string keyword)
	{
		txtKeyword.TextChanged -= txtKeyword_TextChanged;
		txtKeyword.Text = keyword;
		txtKeyword.TextChanged += txtKeyword_TextChanged;
		_keyword = keyword;
	}

	public void UpdateDisplay()
	{
		grdEditor.BeginUpdate();
		try
		{
			Popualte();
			Auditai.UI.Controls.Theme.SetCurrentTree(this);
			base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(ContextResources.ctxSearch);
			ctnAll.SplitterWidth = 0;
			txtKeyword.Select();
			txtKeyword.SelectionStart = txtKeyword.TextLength;
			txtKeyword.SelectionLength = 0;
		}
		finally
		{
			grdEditor.EndUpdate();
		}
	}

	private void SetTheme()
	{
		ctnAll.SplitterWidth = 0;
	}

	private void ClearNodeNameCache()
	{
		_nodesNameCacheList.Clear();
		_nodesNameCacheDic.Clear();
		_isNodesNameCacheGenerated = false;
	}

	private void RebuildNodeNameCache()
	{
		ClearNodeNameCache();
		IEnumerable<TreeNodeBase> enumerable = Project.TreeGroups.SelectMany((TreeGroup g) => g.GetAllNodes());
		foreach (TreeNodeBase item in enumerable)
		{
			if (!_nodesNameCacheDic.ContainsKey(item))
			{
				string nodeDisplayName = GetNodeDisplayName(item);
				_nodesNameCacheList.Add(Tuple.Create(item, nodeDisplayName));
				_nodesNameCacheDic.Add(item, nodeDisplayName);
			}
		}
		_isNodesNameCacheGenerated = true;
	}

	private void PrepareNodeNameCache()
	{
		if (!_isNodesNameCacheGenerated)
		{
			RebuildNodeNameCache();
		}
	}

	private string GetNodeDisplayName(TreeNodeBase node)
	{
		List<string> list = new List<string> { GetDisplyaString(node) };
		TreeNodeBase treeNodeBase = node;
		for (TreeDirectoryNode treeDirectoryNode = node.Parent; treeDirectoryNode != null; treeDirectoryNode = treeDirectoryNode.Parent)
		{
			list.Add(GetDisplyaString(treeDirectoryNode));
			treeNodeBase = treeDirectoryNode;
		}
		if (Project.TreeGroups.Count > 1)
		{
			list.Add(treeNodeBase?.Group?.Name);
		}
		list.Reverse();
		return string.Join("\\", list);
		static string GetDisplyaString(TreeNodeBase node)
		{
			if (string.IsNullOrWhiteSpace(node.Number))
			{
				return node.Name ?? "";
			}
			return node.Number + " " + node.Name;
		}
	}

	private void Popualte()
	{
		grdEditor.Cols.Count = 1;
		grdEditor.Cols.Fixed = 0;
		grdEditor.Rows.Count = 0;
		if (Project == null)
		{
			return;
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		PrepareNodeNameCache();
		IEnumerable<TreeNodeBase> enumerable = from n in _nodesNameCacheList
			select Tuple.Create(n.Item1, FuzzySearch.Filter(n.Item2, _keyword), n.Item2) into n
			where n.Item2 > 0
			orderby n.Item2, n.Item3
			select n.Item1;
		stopwatch.Stop();
		stopwatch.Restart();
		foreach (TreeNodeBase item in enumerable)
		{
			if (!(item is TreeDocumentNode))
			{
				if (!(item is TreeTableNode))
				{
					if (!(item is TreeImageNode))
					{
						if (item is TreePdfNode)
						{
							AddRow(item, Resources.TreePdf);
						}
					}
					else
					{
						AddRow(item, Resources.TreeImage);
					}
				}
				else
				{
					AddRow(item, Resources.TreeTable);
				}
			}
			else
			{
				AddRow(item, Resources.TreeDoc);
			}
		}
		grdEditor.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
		stopwatch.Stop();
		void AddRow(TreeNodeBase node, System.Drawing.Image img)
		{
			string value = string.Empty;
			_nodesNameCacheDic.TryGetValue(node, out value);
			C1.Win.C1FlexGrid.Row row = grdEditor.Rows.Add();
			row.UserData = node;
			row[0] = value ?? "";
			grdEditor.SetCellImage(row.Index, 0, img);
		}
	}

	private void FrmSearch_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall)
		{
			e.Cancel = true;
			Hide();
		}
	}

	private void grdEditor_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		int mouseRow = grdEditor.MouseRow;
		if (mouseRow < 0 || mouseRow >= grdEditor.Rows.Count)
		{
			Hide();
			return;
		}
		TreeNodeBase treeNodeBase = grdEditor.Rows[mouseRow].UserData as TreeNodeBase;
		this.SelectNode?.Invoke(this, treeNodeBase);
		Hide();
	}

	private void txtKeyword_TextChanged(object sender, EventArgs e)
	{
		lazyPopulateExcute.Excute();
	}

	private void TxtKeyword_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.Return:
			if (grdEditor.Rows.Count <= 0)
			{
				break;
			}
			if (grdEditor.Row >= grdEditor.Rows.Fixed && grdEditor.Row < grdEditor.Rows.Count)
			{
				if (grdEditor.Rows[grdEditor.Row].UserData is TreeNodeBase treeNodeBase)
				{
					this.SelectNode?.Invoke(this, treeNodeBase);
					Hide();
				}
			}
			else if (grdEditor.Rows[0].UserData is TreeNodeBase treeNodeBase2)
			{
				this.SelectNode?.Invoke(this, treeNodeBase2);
				Hide();
			}
			break;
		case Keys.Escape:
			Close();
			break;
		}
	}

	private void FrmSearch_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode == Keys.Escape)
		{
			Close();
		}
	}

	private void GrdEditor_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode == Keys.Escape)
		{
			Close();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Auditai.UI.Platform.frmSearch));
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlInput = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txtKeyword = new C1.Win.C1Input.C1TextBox();
		this.pnlEditor = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.grdEditor = new C1.Win.C1FlexGrid.C1FlexGrid();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlInput.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtKeyword).BeginInit();
		this.pnlEditor.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdEditor).BeginInit();
		base.SuspendLayout();
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnAll.HeaderHeight = 27;
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlInput);
		this.ctnAll.Panels.Add(this.pnlEditor);
		this.ctnAll.Size = new System.Drawing.Size(612, 463);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		this.pnlInput.Controls.Add(this.txtKeyword);
		this.pnlInput.Height = 30;
		this.pnlInput.KeepRelativeSize = false;
		this.pnlInput.Location = new System.Drawing.Point(0, 0);
		this.pnlInput.MinHeight = 30;
		this.pnlInput.MinWidth = 52;
		this.pnlInput.Name = "pnlInput";
		this.pnlInput.Size = new System.Drawing.Size(612, 30);
		this.pnlInput.SizeRatio = 6.479;
		this.pnlInput.TabIndex = 0;
		this.pnlInput.Width = 612;
		this.txtKeyword.AutoSize = false;
		this.txtKeyword.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtKeyword.Location = new System.Drawing.Point(0, 0);
		this.txtKeyword.Name = "txtKeyword";
		this.txtKeyword.Size = new System.Drawing.Size(612, 30);
		this.txtKeyword.TabIndex = 0;
		this.txtKeyword.Tag = null;
		this.txtKeyword.TextDetached = true;
		this.txtKeyword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.pnlEditor.Controls.Add(this.grdEditor);
		this.pnlEditor.Height = 433;
		this.pnlEditor.Location = new System.Drawing.Point(0, 30);
		this.pnlEditor.MinHeight = 52;
		this.pnlEditor.MinWidth = 52;
		this.pnlEditor.Name = "pnlEditor";
		this.pnlEditor.Size = new System.Drawing.Size(612, 433);
		this.pnlEditor.TabIndex = 1;
		this.pnlEditor.Width = 612;
		this.grdEditor.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle;
		this.grdEditor.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this.grdEditor.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdEditor.Location = new System.Drawing.Point(0, 0);
		this.grdEditor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdEditor.Name = "grdEditor";
		this.grdEditor.Rows.DefaultSize = 20;
		this.grdEditor.Size = new System.Drawing.Size(612, 433);
		this.grdEditor.TabIndex = 1;
		this.grdEditor.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(grdEditor_MouseDoubleClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(612, 463);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmSearch";
		this.Text = "文件搜索";
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlInput.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txtKeyword).EndInit();
		this.pnlEditor.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdEditor).EndInit();
		base.ResumeLayout(false);
	}
}
