using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.LedgerView;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class frmSelectImport : C1RibbonForm
{
	private const string CN_INDEX = "index";

	private const string CN_CHECK = "check";

	private const string CN_TREE = "tree";

	private Leqisoft.Model.Project _project;

	private int _selectedItemIndex = -1;

	private bool _suspendCheckEvent;

	private IContainer components;

	private C1SplitContainer c1SplitContainer1;

	private C1SplitterPanel pnlTree;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancel;

	private C1Button btnCertain;

	private C1FlexGrid grdTree;

	public List<TreeTableNode> Selects { get; set; }

	public frmSelectImport(Leqisoft.Model.Project project)
	{
		_project = project;
		InitializeComponent();
		base.Shown += FrmSelectImport_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		grdTree.ScrollBars = ScrollBars.Vertical;
		grdTree.SizeChanged += GrdTree_SizeChanged;
		grdTree.CellChecked += GrdTree_CellChecked;
		grdTree.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdTree.DrawFormBorder(e1.Graphics);
		};
	}

	private void GrdTree_CellChecked(object sender, RowColEventArgs e)
	{
		if (_suspendCheckEvent || e.Col != grdTree.Cols["check"].Index)
		{
			return;
		}
		int selectedItemIndex = _selectedItemIndex;
		_selectedItemIndex = -1;
		if (grdTree.GetCellCheck(e.Row, e.Col) != CheckEnum.Checked)
		{
			return;
		}
		_selectedItemIndex = e.Row;
		if (selectedItemIndex == -1)
		{
			return;
		}
		_suspendCheckEvent = true;
		try
		{
			grdTree.SetCellCheck(selectedItemIndex, e.Col, CheckEnum.Unchecked);
		}
		catch (Exception)
		{
		}
		finally
		{
			_suspendCheckEvent = false;
		}
	}

	private void FrmSelectImport_Shown(object sender, EventArgs e)
	{
		base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(Resources.batchFill);
	}

	private void GrdTree_SizeChanged(object sender, EventArgs e)
	{
		grdTree.BeginUpdate();
		grdTree.Cols["check"].Width = 80;
		grdTree.Cols["tree"].Width = base.Width - 100;
		grdTree.EndUpdate();
	}

	public DialogResult ShowDialog(LedgerCollectEventArgs e)
	{
		PopulateTree(e);
		if (grdTree.Rows.Count == grdTree.Rows.Fixed)
		{
			throw new InvalidOperationException("未能智能识别出填充的底稿对象，请检查底稿体系中是否含有匹配的底稿。");
		}
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
		return ShowDialog();
	}

	private void InitializeGrdTree()
	{
		grdTree.Rows.DefaultSize = 30;
		grdTree.Rows.Count = 1;
		grdTree.Rows.Fixed = 1;
		grdTree.Cols.Count = 0;
		C1.Win.C1FlexGrid.Column column = grdTree.Cols.Add();
		column.Name = "tree";
		column.Caption = "表格";
		column.TextAlign = TextAlignEnum.LeftCenter;
		column.ImageAlign = ImageAlignEnum.LeftCenter;
		column.AllowEditing = false;
		column = grdTree.Cols.Add();
		column.Name = "check";
		column.Caption = "选择";
		column.Width = 40;
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.ImageAlign = ImageAlignEnum.CenterCenter;
		column.AllowEditing = true;
		grdTree.Tree.Column = 0;
		grdTree.ExtendLastCol = true;
		grdTree.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		grdTree.Styles.Fixed.ImageAlign = ImageAlignEnum.CenterCenter;
	}

	private void PopulateTree(LedgerCollectEventArgs e)
	{
		grdTree.BeginUpdate();
		try
		{
			InitializeGrdTree();
			Account account = e.Account;
			if (account == null)
			{
				if (!e.IsShowSubsidiaryAllAccountTypeTable)
				{
					return;
				}
			}
			else
			{
				while (account.Parent != null)
				{
					account = account.Parent;
				}
			}
			CollectObjectEnum collectObject = e.CollectObject;
			foreach (Leqisoft.Model.TreeGroup treeGroup in _project.TreeGroups)
			{
				foreach (TreeTableNode item in from n in treeGroup.GetAllNodes()
					where n is TreeTableNode
					select n into t
					select t as TreeTableNode)
				{
					string name = item.Name;
					if (string.IsNullOrWhiteSpace(name))
					{
						continue;
					}
					CollectObjectEnum? collectObjectEnum = DictionarySync.TableCollector.IntellegenceObject(name);
					if (collectObjectEnum != collectObject)
					{
						continue;
					}
					bool flag = false;
					if (e.IsShowSubsidiaryAllAccountTypeTable && collectObjectEnum.GetValueOrDefault() == CollectObjectEnum.Subsidiary)
					{
						flag = DictionarySync.TableCollector.IntellegenceIsNeedSelectAllAccount(name);
					}
					if (e.IsShowBalanceAllAccountTypeTable && collectObjectEnum == CollectObjectEnum.Balance)
					{
						flag = DictionarySync.TableCollector.IntellegenceIsNeedSelectAllAccount(name);
					}
					if (!flag && account != null)
					{
						Tuple<string, List<string>> tuple = DictionarySync.TableCollector.IntellegenceLogic(name);
						if (tuple != null && tuple.Item2.Any((string a) => Regex.IsMatch(account.Name, a)))
						{
							flag = true;
						}
					}
					if (flag)
					{
						Node node = grdTree.Rows.AddNode(0);
						node.Key = item;
						node.Data = item.Name;
						node.Image = ContextResources.TreeTable;
						grdTree.SetCellCheck(node.Row.Index, grdTree.Cols["check"].Index, CheckEnum.Unchecked);
					}
				}
			}
			if (grdTree.Rows.Count - grdTree.Rows.Fixed == 1)
			{
				grdTree.SetCellCheck(grdTree.Rows.Fixed, grdTree.Cols["check"].Index, CheckEnum.Checked);
			}
		}
		finally
		{
			grdTree.EndUpdate();
		}
	}

	private async Task<Leqisoft.Model.Table> OpenTableImpl(Leqisoft.Model.Table table)
	{
		ProgressForm<Leqisoft.Model.Table> progressForm = new ProgressForm<Leqisoft.Model.Table>(async delegate(IProgress<ProgressInfo> progress)
		{
			progress.Report(new ProgressInfo
			{
				MainCaption = "正在打开表格，请稍候......",
				MainProgress = 0
			});
			Application.DoEvents();
			table.LoadAndReturn();
			return await Task.FromResult(table);
		});
		progressForm.ShowDialog();
		return await progressForm.Task;
	}

	private void btnCertain_Click(object sender, EventArgs e)
	{
		List<TreeTableNode> selectedNodes = GetSelectedNodes();
		if (selectedNodes.Count == 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选中要填充的表格后再进行该操作!");
			return;
		}
		Leqisoft.Model.Table table = selectedNodes[0].Table;
		if (!table._loaded)
		{
			table = OpenTableImpl(table).Result;
		}
		if (table.IsCorrupted)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格 " + table.TreeNode.Name + " 损坏了，无法打开表格!");
			return;
		}
		table.TryApplyTitleFootFormula();
		base.DialogResult = DialogResult.OK;
		Selects = GetSelectedNodes();
		Close();
	}

	private List<TreeTableNode> GetSelectedNodes()
	{
		List<TreeTableNode> list = new List<TreeTableNode>();
		int index = grdTree.Cols["check"].Index;
		for (int i = grdTree.Rows.Fixed; i < grdTree.Rows.Count; i++)
		{
			if (grdTree.GetCellCheck(i, index) == CheckEnum.Checked)
			{
				list.Add(grdTree.Rows[i].UserData as TreeTableNode);
			}
		}
		return list;
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnCertain = new C1.Win.C1Input.C1Button();
		this.pnlTree = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.grdTree = new C1.Win.C1FlexGrid.C1FlexGrid();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.pnlButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).BeginInit();
		this.pnlTree.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdTree).BeginInit();
		base.SuspendLayout();
		this.c1SplitContainer1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1SplitContainer1.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.c1SplitContainer1.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.c1SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1SplitContainer1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.c1SplitContainer1.HeaderHeight = 27;
		this.c1SplitContainer1.Location = new System.Drawing.Point(0, 0);
		this.c1SplitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.c1SplitContainer1.Name = "c1SplitContainer1";
		this.c1SplitContainer1.Panels.Add(this.pnlButtons);
		this.c1SplitContainer1.Panels.Add(this.pnlTree);
		this.c1SplitContainer1.Size = new System.Drawing.Size(333, 537);
		this.c1SplitContainer1.SplitterWidth = 5;
		this.c1SplitContainer1.TabIndex = 0;
		this.pnlButtons.Controls.Add(this.btnCancel);
		this.pnlButtons.Controls.Add(this.btnCertain);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 40;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point(0, 497);
		this.pnlButtons.MinHeight = 39;
		this.pnlButtons.MinWidth = 52;
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Resizable = false;
		this.pnlButtons.Size = new System.Drawing.Size(333, 40);
		this.pnlButtons.SizeRatio = 7.463;
		this.pnlButtons.TabIndex = 1;
		this.pnlButtons.Width = 333;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(236, 7);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnCertain.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCertain.Location = new System.Drawing.Point(146, 7);
		this.btnCertain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCertain.Name = "btnCertain";
		this.btnCertain.Size = new System.Drawing.Size(70, 26);
		this.btnCertain.TabIndex = 0;
		this.btnCertain.Text = "确定";
		this.btnCertain.UseVisualStyleBackColor = true;
		this.btnCertain.Click += new System.EventHandler(btnCertain_Click);
		this.pnlTree.Controls.Add(this.grdTree);
		this.pnlTree.Height = 496;
		this.pnlTree.Location = new System.Drawing.Point(0, 0);
		this.pnlTree.MinHeight = 52;
		this.pnlTree.MinWidth = 52;
		this.pnlTree.Name = "pnlTree";
		this.pnlTree.Size = new System.Drawing.Size(333, 496);
		this.pnlTree.TabIndex = 0;
		this.pnlTree.Width = 333;
		this.grdTree.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdTree.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this.grdTree.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdTree.Location = new System.Drawing.Point(0, 0);
		this.grdTree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdTree.Name = "grdTree";
		this.grdTree.Rows.DefaultSize = 20;
		this.grdTree.Size = new System.Drawing.Size(333, 496);
		this.grdTree.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(333, 537);
		base.Controls.Add(this.c1SplitContainer1);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmSelectImport";
		this.Text = "填充至底稿";
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.pnlButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).EndInit();
		this.pnlTree.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdTree).EndInit();
		base.ResumeLayout(false);
	}
}
