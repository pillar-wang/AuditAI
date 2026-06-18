using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class FormSelectNode
{
	private readonly C1RibbonForm _form;

	private readonly ProjectTreeGrid _projectTreeGrid;

	private readonly C1ContextMenu ctxMenu = new C1ContextMenu();

	private readonly C1CommandLink lnkCollaspe = new C1CommandLink();

	private readonly C1Command cmdCollaspe = new C1Command();

	private readonly C1CommandLink lnkExpand = new C1CommandLink();

	private readonly C1Command cmdExpand = new C1Command();

	public Project Project
	{
		get
		{
			return _projectTreeGrid.Project;
		}
		set
		{
			_projectTreeGrid.Project = value;
			_projectTreeGrid.Populate();
			foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)_projectTreeGrid.View.Rows)
			{
				if (item.IsNode)
				{
					item.Node.Collapsed = true;
				}
			}
		}
	}

	public TreeTableNode SelectedTableNode { get; private set; }

	public FormSelectNode()
	{
		_projectTreeGrid = new ProjectTreeGrid();
		_projectTreeGrid.TreeNodeDoubleClicked += _projectTreeGrid_TreeNodeDoubleClicked;
		C1Button c1Button = new C1Button
		{
			Text = "确定",
			Left = 100,
			Top = 5,
			Height = 30,
			Width = 80
		};
		c1Button.Click += BtnOk_Click;
		C1Button c1Button2 = new C1Button
		{
			Text = "取消",
			Left = 200,
			Top = 5,
			Height = 30,
			Width = 80
		};
		c1Button2.Click += BtnCancel_Click;
		_form = FormFactory.Create();
		_form.Size = new Size(300, 400);
		_form.FormBorderStyle = FormBorderStyle.FixedSingle;
		_form.ShowIcon = true;
		_form.ShowInTaskbar = false;
		_form.MaximizeBox = true;
		_form.MinimizeBox = true;
		_form.Controls.Add(new C1SplitContainer
		{
			Panels = 
			{
				new C1SplitterPanel
				{
					Dock = PanelDockStyle.Bottom,
					Height = 40,
					KeepRelativeSize = false,
					Resizable = false,
					Controls = 
					{
						(Control)c1Button,
						(Control)c1Button2
					}
				},
				new C1SplitterPanel
				{
					Dock = PanelDockStyle.Top,
					Controls = { (Control)_projectTreeGrid.View }
				}
			},
			Dock = DockStyle.Fill,
			FixedLineWidth = 0
		});
		c1Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		c1Button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		cmdExpand.Text = "全部展开";
		lnkExpand.Command = cmdExpand;
		cmdExpand.Click += delegate
		{
			_projectTreeGrid?.View.ExpandAll();
		};
		cmdCollaspe.Text = "全部收缩";
		lnkCollaspe.Command = cmdCollaspe;
		cmdCollaspe.Click += delegate
		{
			_projectTreeGrid?.View.CollapseAll();
		};
		ctxMenu.CommandLinks.Add(lnkExpand);
		ctxMenu.CommandLinks.Add(lnkCollaspe);
		C1CommandHolder c1CommandHolder = new C1CommandHolder
		{
			Owner = _form
		};
		c1CommandHolder.SetC1ContextMenu(_projectTreeGrid.View, ctxMenu);
		Theme.SetCurrentTree(_form);
	}

	public DialogResult ShowImportTable()
	{
		_form.Text = "引用表格";
		_form.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.Intelliref16);
		return _form.ShowDialog();
	}

	public DialogResult ShowImportTicket()
	{
		_form.Text = "导入他表" + Program.MainForm.TicketDesignEditor.Table.Ticket.GetLevelString() + "样式";
		_form.Icon = ((Program.MainForm.TicketDesignEditor.Table.Ticket.Level == TicketLevel.Report) ? Theme.SelectedLeqiTheme.GetThemedIcon(Resources.TicketReport16) : Theme.SelectedLeqiTheme.GetThemedIcon(Resources.Ticket16));
		return _form.ShowDialog();
	}

	private void BtnCancel_Click(object sender, EventArgs e)
	{
		_form.DialogResult = DialogResult.Cancel;
	}

	private void BtnOk_Click(object sender, EventArgs e)
	{
		if (_projectTreeGrid.SelectedNode is TreeTableNode selectedTableNode)
		{
			SelectedTableNode = selectedTableNode;
			_form.DialogResult = DialogResult.OK;
		}
		else if (_projectTreeGrid.SelectedNode is TreeDocumentNode)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择表格。");
		}
	}

	private void _projectTreeGrid_TreeNodeDoubleClicked(object sender, TreeNodeEventArgs e)
	{
		if (e.TreeNode is TreeTableNode selectedTableNode)
		{
			SelectedTableNode = selectedTableNode;
			_form.DialogResult = DialogResult.OK;
		}
	}
}
