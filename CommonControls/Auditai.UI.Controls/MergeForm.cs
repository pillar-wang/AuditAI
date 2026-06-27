using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class MergeForm : C1RibbonForm
{
	private C1ContextMenu ctxMenu = new C1ContextMenu();

	private C1CommandLink lnkCollaspe = new C1CommandLink();

	private C1Command cmdCollaspe = new C1Command();

	private C1CommandLink lnkExpand = new C1CommandLink();

	private C1Command cmdExpand = new C1Command();

	private ProjectTreeGrid _projectTree;

	private static MergeForm _instance;

	private C1CommandHolder holder;

	private bool canTriggerMouse = true;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlTables;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancle;

	private C1Button btnInsert;

	private event EventHandler<Auditai.Model.Column> _afterSelected;

	public event EventHandler<Auditai.Model.Column> AfterSelected
	{
		add
		{
			this._afterSelected = value;
		}
		remove
		{
			_afterSelected -= value;
		}
	}

	public static MergeForm GetInstance()
	{
		if (_instance == null || _instance.IsDisposed)
		{
			_instance = new MergeForm();
		}
		return _instance;
	}

	private MergeForm()
	{
		InitializeComponent();
		base.Shown += MergeForm_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.TopMost = true;
		cmdExpand.Text = "全部展开";
		lnkExpand.Command = cmdExpand;
		cmdExpand.Click += delegate
		{
			_projectTree?.View.ExpandAll();
		};
		cmdCollaspe.Text = "全部收缩";
		lnkCollaspe.Command = cmdCollaspe;
		cmdCollaspe.Click += delegate
		{
			_projectTree?.View.CollapseAll();
		};
		ctxMenu.CommandLinks.Add(lnkExpand);
		ctxMenu.CommandLinks.Add(lnkCollaspe);
		holder = new C1CommandHolder
		{
			Owner = this
		};
	}

	private void MergeForm_Shown(object sender, EventArgs e)
	{
		base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.SelectColumn);
	}

	public void Show(Project project)
	{
		_projectTree = new ProjectTreeGrid();
		_projectTree.View.MouseClick += View_MouseClick;
		_projectTree.View.DoubleClick += View_DoubleClick;
		_projectTree.TreeNodeClicked += _projectTree_TreeNodeClicked;
		_projectTree.Project = project;
		_projectTree.Populate();
		holder.SetC1ContextMenu(_projectTree.View, ctxMenu);
		pnlTables.Controls.Clear();
		pnlTables.Controls.Add(_projectTree.View);
		List<C1.Win.C1FlexGrid.Row> list = new List<C1.Win.C1FlexGrid.Row>();
		for (int i = 0; i < _projectTree.View.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = _projectTree.View.Rows[i];
			row.Node.Collapsed = true;
			object userData = row.UserData;
			if (!(userData is TreeDirectoryNode treeDir2))
			{
				if (!(userData is TreeDocumentNode))
				{
					TreeTableNode treeTableNode = userData as TreeTableNode;
					if (treeTableNode == null)
					{
						TreeGroup treeGroup = userData as TreeGroup;
						if (treeGroup != null)
						{
						}
					}
				}
				else
				{
					list.Add(row);
				}
			}
			else if (!hasTable(treeDir2))
			{
				list.Add(row);
			}
		}
		foreach (C1.Win.C1FlexGrid.Row item in list)
		{
			_projectTree.View.Rows.Remove(item);
		}
		Theme.SetCurrentTree(this);
		Show();
		static bool hasTable(TreeDirectoryNode treeDir)
		{
			foreach (TreeNodeBase child in treeDir.Children)
			{
				if (!(child is TreeDirectoryNode treeDir3))
				{
					TreeDocumentNode treeDocumentNode2 = child as TreeDocumentNode;
					if (treeDocumentNode2 == null && child is TreeTableNode)
					{
						return true;
					}
				}
				else if (hasTable(treeDir3))
				{
					return true;
				}
			}
			return false;
		}
	}

	private void View_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _projectTree.View.HitTest(e.Location);
		if (e.Button == MouseButtons.Left && canTriggerMouse)
		{
		}
		canTriggerMouse = true;
	}

	private void View_DoubleClick(object sender, EventArgs e)
	{
		int row = _projectTree.View.Row;
		C1.Win.C1FlexGrid.Row row2 = _projectTree.View.Rows[row];
		if (row2.UserData is Auditai.Model.Column column)
		{
			this._afterSelected?.Invoke(this, column);
		}
	}

	private void _projectTree_TreeNodeClicked(object sender, TreeNodeEventArgs e)
	{
		int row = _projectTree.View.Row;
		Node node = _projectTree.View.Rows[row].Node;
		if (!(node.Key is TreeTableNode treeTableNode) || node.Nodes.Count() != 0)
		{
			return;
		}
		treeTableNode.Table.LoadAndReturn();
		foreach (Auditai.Model.Column column in treeTableNode.Table.Columns)
		{
			Node node2 = node.AddNode(NodeTypeEnum.LastChild, column.CaptionDisplay, column, Resources.SelectColumn);
		}
		canTriggerMouse = false;
	}

	private void btnInsert_Click(object sender, EventArgs e)
	{
		int row = _projectTree.View.Row;
		C1.Win.C1FlexGrid.Row row2 = _projectTree.View.Rows[row];
		if (row2.UserData is Auditai.Model.Column column)
		{
			this._afterSelected?.Invoke(this, column);
		}
	}

	private void btnCancle_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void MergeForm_FormClosing(object sender, FormClosingEventArgs e)
	{
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
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCancle = new C1.Win.C1Input.C1Button();
		this.btnInsert = new C1.Win.C1Input.C1Button();
		this.pnlTables = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancle).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnInsert).BeginInit();
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
		this.ctnAll.Panels.Add(this.pnlButtons);
		this.ctnAll.Panels.Add(this.pnlTables);
		this.ctnAll.Size = new System.Drawing.Size(279, 509);
		this.ctnAll.SplitterWidth = 1;
		this.ctnAll.TabIndex = 0;
		this.pnlButtons.Controls.Add(this.btnCancle);
		this.pnlButtons.Controls.Add(this.btnInsert);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 50;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point(0, 459);
		this.pnlButtons.MinHeight = 50;
		this.pnlButtons.MinWidth = 53;
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Resizable = false;
		this.pnlButtons.Size = new System.Drawing.Size(279, 50);
		this.pnlButtons.SizeRatio = 9.843;
		this.pnlButtons.TabIndex = 1;
		this.pnlButtons.Width = 279;
		this.btnCancle.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancle.Location = new System.Drawing.Point(187, 11);
		this.btnCancle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancle.Name = "btnCancle";
		this.btnCancle.Size = new System.Drawing.Size(70, 26);
		this.btnCancle.TabIndex = 1;
		this.btnCancle.Text = "关闭";
		this.btnCancle.UseVisualStyleBackColor = true;
		this.btnCancle.Click += new System.EventHandler(btnCancle_Click);
		this.btnInsert.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnInsert.Location = new System.Drawing.Point(75, 11);
		this.btnInsert.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnInsert.Name = "btnInsert";
		this.btnInsert.Size = new System.Drawing.Size(70, 26);
		this.btnInsert.TabIndex = 0;
		this.btnInsert.Text = "插入";
		this.btnInsert.UseVisualStyleBackColor = true;
		this.btnInsert.Click += new System.EventHandler(btnInsert_Click);
		this.pnlTables.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlTables.Height = 458;
		this.pnlTables.Location = new System.Drawing.Point(0, 0);
		this.pnlTables.MinHeight = 53;
		this.pnlTables.MinWidth = 53;
		this.pnlTables.Name = "pnlTables";
		this.pnlTables.Size = new System.Drawing.Size(279, 458);
		this.pnlTables.SizeRatio = 68.293;
		this.pnlTables.TabIndex = 0;
		this.pnlTables.Width = 279;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(279, 509);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "MergeForm";
		base.ShowInTaskbar = false;
		this.Text = "选择列";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(MergeForm_FormClosing);
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancle).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnInsert).EndInit();
		base.ResumeLayout(false);
	}
}
