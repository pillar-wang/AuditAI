using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class frmSearchTicketRecord : C1RibbonForm
{
	[CompilerGenerated]
	private sealed class _003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Node _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private C1FlexGrid grid;

		public C1FlexGrid _003C_003E3__grid;

		private int _003CrowsCount_003E5__2;

		private int _003Ci_003E5__3;

		Node IEnumerator<Node>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_008e;
			}
			_003C_003E1__state = -1;
			_003CrowsCount_003E5__2 = grid.Rows.Count;
			_003Ci_003E5__3 = grid.Rows.Fixed;
			goto IL_009e;
			IL_008e:
			_003Ci_003E5__3++;
			goto IL_009e;
			IL_009e:
			if (_003Ci_003E5__3 < _003CrowsCount_003E5__2)
			{
				Row row = grid.Rows[_003Ci_003E5__3];
				if (row.IsNode && row.Node.Nodes.Length == 0)
				{
					_003C_003E2__current = row.Node;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_008e;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		{
			_003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed _003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed = this;
			}
			else
			{
				_003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed = new _003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed(0);
			}
			_003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed.grid = _003C_003E3__grid;
			return _003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private const int CN_TREENODE = 0;

	private string _keyword = string.Empty;

	private LazyExcute lazyPopulateExcute = new LazyExcute();

	private bool _isNodesNameCacheGenerated;

	private List<Tuple<Node, TicketNavGrid, string>> _nodesNameCacheList = new List<Tuple<Node, TicketNavGrid, string>>();

	private Dictionary<Node, string> _nodesNameCacheDic = new Dictionary<Node, string>();

	private IContainer components;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlInput;

	private C1SplitterPanel pnlEditor;

	private C1FlexGrid grdEditor;

	private C1TextBox txtKeyword;

	public List<Tuple<TicketNavGrid, string>> TicketNavGrids { get; set; }

	public event EventHandler<Tuple<Node, TicketNavGrid>> SelectNode;

	public frmSearchTicketRecord()
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
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
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
			Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
			base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(ContextResources.ctxSearch);
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
		if (TicketNavGrids == null)
		{
			return;
		}
		foreach (Tuple<TicketNavGrid, string> ticketNavGrid in TicketNavGrids)
		{
			IEnumerable<Node> enumerable = GetAllNodes(ticketNavGrid.Item1.View);
			foreach (Node item in enumerable)
			{
				if (!_nodesNameCacheDic.ContainsKey(item))
				{
					string nodeDisplayName = GetNodeDisplayName(item, ticketNavGrid.Item1);
					_nodesNameCacheList.Add(Tuple.Create(item, ticketNavGrid.Item1, nodeDisplayName));
					_nodesNameCacheDic.Add(item, nodeDisplayName);
				}
			}
		}
		_isNodesNameCacheGenerated = true;
		[IteratorStateMachine(typeof(_003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed))]
		static IEnumerable<Node> GetAllNodes(C1FlexGrid grid)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003C_003CRebuildNodeNameCache_003Eg__GetAllNodes_007C19_0_003Ed(-2)
			{
				_003C_003E3__grid = grid
			};
		}
	}

	private void PrepareNodeNameCache()
	{
		if (!_isNodesNameCacheGenerated)
		{
			RebuildNodeNameCache();
		}
	}

	private string GetNodeDisplayName(Node node, TicketNavGrid navGrid)
	{
		List<string> list = new List<string> { GetDisplyaString(node) };
		for (Node node2 = node.Parent; node2 != null; node2 = node2.Parent)
		{
			list.Add(GetDisplyaString(node2));
		}
		if (TicketNavGrids.Count > 1)
		{
			list.Add(TicketNavGrids.First((Tuple<TicketNavGrid, string> n) => n.Item1 == navGrid).Item2);
		}
		list.Reverse();
		return string.Join("\\", list);
		static string GetDisplyaString(Node node)
		{
			if (!(node.Key is TicketNavGrid.NavNode navNode))
			{
				return "空";
			}
			if (string.IsNullOrEmpty(navNode.Text))
			{
				return "空";
			}
			return navNode.Text;
		}
	}

	private void Popualte()
	{
		grdEditor.Cols.Count = 1;
		grdEditor.Cols.Fixed = 0;
		grdEditor.Rows.Count = 0;
		if (TicketNavGrids == null || TicketNavGrids.Count == 0)
		{
			return;
		}
		PrepareNodeNameCache();
		IEnumerable<Tuple<Node, TicketNavGrid>> enumerable = from n in _nodesNameCacheList
			select Tuple.Create(n.Item1, n.Item2, n.Item3, FuzzySearch.Filter(n.Item3, _keyword)) into n
			where n.Item4 > 0
			orderby n.Item4, n.Item3
			select Tuple.Create(n.Item1, n.Item2);
		foreach (Tuple<Node, TicketNavGrid> item in enumerable)
		{
			AddRow(item.Item1, item.Item2);
		}
		grdEditor.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
		void AddRow(Node node, TicketNavGrid navGrid)
		{
			string value = string.Empty;
			_nodesNameCacheDic.TryGetValue(node, out value);
			Row row = grdEditor.Rows.Add();
			row.UserData = Tuple.Create(node, navGrid);
			row[0] = value ?? "";
			grdEditor.SetCellImage(row.Index, 0, Resources.Ticket16);
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
		Tuple<Node, TicketNavGrid> tuple = grdEditor.Rows[mouseRow].UserData as Tuple<Node, TicketNavGrid>;
		this.SelectNode?.Invoke(this, tuple);
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
				if (grdEditor.Rows[grdEditor.Row].UserData is Tuple<Node, TicketNavGrid> tuple)
				{
					this.SelectNode?.Invoke(this, tuple);
					Hide();
				}
			}
			else if (grdEditor.Rows[0].UserData is Tuple<Node, TicketNavGrid> tuple2)
			{
				this.SelectNode?.Invoke(this, tuple2);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Leqisoft.UI.Platform.frmSearch));
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
		this.Text = "表单搜索";
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlInput.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txtKeyword).EndInit();
		this.pnlEditor.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdEditor).EndInit();
		base.ResumeLayout(false);
	}
}
