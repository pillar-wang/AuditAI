using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class MergeTemplateSelector : C1RibbonForm
{
	private ProjectTreeGrid _projectTree;

	private static MergeTemplateSelector _instance;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlTree;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancle;

	private C1Button btnConfirm;

	public TreeDocumentNode SelectedTemplete { get; set; }

	public TreeTableNode CurrentTable { get; set; }

	public static MergeTemplateSelector GetInstance()
	{
		if (_instance == null || _instance.IsDisposed)
		{
			_instance = new MergeTemplateSelector();
		}
		return _instance;
	}

	public MergeTemplateSelector()
	{
		InitializeComponent();
		base.Shown += MergeTemplateSelector_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.TopMost = true;
	}

	private void MergeTemplateSelector_Shown(object sender, EventArgs e)
	{
		Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.ConfirmationGenerate);
	}

	private void _projectTree_TreeNodeDoubleClicked(object sender, TreeNodeEventArgs e)
	{
		if (e.TreeNode is TreeDocumentNode selectedTemplete)
		{
			SelectedTemplete = selectedTemplete;
			base.DialogResult = DialogResult.OK;
			Close();
		}
		else
		{
			System.Windows.Forms.MessageBox.Show("选择节点不是模板文档");
		}
	}

	public DialogResult ShowDialog(Project project)
	{
		_projectTree = new ProjectTreeGrid();
		_projectTree.TreeNodeDoubleClicked += _projectTree_TreeNodeDoubleClicked;
		_projectTree.View.MouseClick += View_MouseClick;
		_projectTree.Project = project;
		_projectTree.Populate();
		pnlTree.Controls.Clear();
		pnlTree.Controls.Add(_projectTree.View);
		List<C1.Win.C1FlexGrid.Row> list = new List<C1.Win.C1FlexGrid.Row>();
		for (int i = 0; i < _projectTree.View.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = _projectTree.View.Rows[i];
			row.Node.Collapsed = true;
			object userData = row.UserData;
			if (!(userData is TreeDirectoryNode treeDir2))
			{
				if (!(userData is TreeDocumentNode treeDocumentNode))
				{
					if (!(userData is TreeTableNode))
					{
						TreeGroup treeGroup = userData as TreeGroup;
						if (treeGroup != null)
						{
						}
					}
					else
					{
						list.Add(row);
					}
				}
				else if (!CurrentTable.Table.Id.Equals(treeDocumentNode.Document.MergeTable))
				{
					list.Add(row);
				}
			}
			else if (!hasDocTemplate(treeDir2))
			{
				list.Add(row);
			}
		}
		foreach (C1.Win.C1FlexGrid.Row item in list)
		{
			_projectTree.View.Rows.Remove(item);
		}
		Theme.SetCurrentTree(this);
		return ShowDialog();
		bool hasDocTemplate(TreeDirectoryNode treeDir)
		{
			foreach (TreeNodeBase child in treeDir.Children)
			{
				if (!(child is TreeDirectoryNode treeDir3))
				{
					if (!(child is TreeDocumentNode treeDocumentNode2))
					{
						TreeTableNode treeTableNode2 = child as TreeTableNode;
						if (treeTableNode2 != null)
						{
						}
					}
					else if (CurrentTable.Table.Id.Equals(treeDocumentNode2.Document.MergeTable))
					{
						return true;
					}
				}
				else if (hasDocTemplate(treeDir3))
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
		if (e.Button == MouseButtons.Left)
		{
			_ = hitTestInfo.Type;
			_ = 1;
		}
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		if (_projectTree.SelectedNode is TreeDocumentNode selectedTemplete)
		{
			SelectedTemplete = selectedTemplete;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void btnCancle_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void MergeTemplateSelector_FormClosing(object sender, FormClosingEventArgs e)
	{
		_ = e.CloseReason;
		_ = 6;
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
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.pnlTree = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancle).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
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
		this.ctnAll.Panels.Add(this.pnlTree);
		this.ctnAll.Size = new System.Drawing.Size(306, 480);
		this.ctnAll.SplitterWidth = 5;
		this.ctnAll.TabIndex = 0;
		this.pnlButtons.Controls.Add(this.btnCancle);
		this.pnlButtons.Controls.Add(this.btnConfirm);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 52;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point(0, 428);
		this.pnlButtons.MinHeight = 52;
		this.pnlButtons.MinWidth = 52;
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Resizable = false;
		this.pnlButtons.Size = new System.Drawing.Size(306, 52);
		this.pnlButtons.SizeRatio = 8.176;
		this.pnlButtons.TabIndex = 1;
		this.pnlButtons.Width = 306;
		this.btnCancle.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancle.Location = new System.Drawing.Point(206, 13);
		this.btnCancle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancle.Name = "btnCancle";
		this.btnCancle.Size = new System.Drawing.Size(70, 26);
		this.btnCancle.TabIndex = 1;
		this.btnCancle.Text = "取消";
		this.btnCancle.UseVisualStyleBackColor = true;
		this.btnCancle.Click += new System.EventHandler(btnCancle_Click);
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Location = new System.Drawing.Point(104, 13);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 0;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.pnlTree.Height = 427;
		this.pnlTree.Location = new System.Drawing.Point(0, 0);
		this.pnlTree.MinHeight = 52;
		this.pnlTree.MinWidth = 52;
		this.pnlTree.Name = "pnlTree";
		this.pnlTree.Size = new System.Drawing.Size(306, 427);
		this.pnlTree.TabIndex = 0;
		this.pnlTree.Width = 306;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(306, 480);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "MergeTemplateSelector";
		base.ShowInTaskbar = false;
		this.Text = "选择模板";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(MergeTemplateSelector_FormClosing);
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancle).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		base.ResumeLayout(false);
	}
}
