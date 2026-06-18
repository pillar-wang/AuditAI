using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;

namespace Leqisoft.UI.Controls;

public class ComboTree : C1ComboBox
{
	private const int WM_LBUTTONDOWN = 513;

	private const int WM_LBUTTONDBLCLK = 515;

	private ToolStripControlHost treeViewHost;

	public ToolStripDropDown dropDown;

	public int DropWidth { get; set; } = -1;


	public int DropHeight { get; set; } = -1;


	public TreeView TreeView { get; set; }

	public TreeNode SelectedNode
	{
		get
		{
			return TreeView.SelectedNode;
		}
		set
		{
			if (value != null)
			{
				TreeView.SelectedNode = value;
				Text = TreeView.SelectedNode.Text;
				this.SelectNodeChanged?.Invoke(TreeView, new TreeViewEventArgs(value));
			}
			else
			{
				TreeView.SelectedNode = null;
				Text = string.Empty;
				this.SelectNodeChanged?.Invoke(TreeView, new TreeViewEventArgs(value));
			}
		}
	}

	public event EventHandler<TreeViewEventArgs> SelectNodeChanged;

	public ComboTree()
	{
		TreeView = new TreeView
		{
			BorderStyle = BorderStyle.None,
			Dock = DockStyle.Fill
		};
		TreeView.AfterSelect += delegate(object s, TreeViewEventArgs e)
		{
			Text = e.Node.Text;
			dropDown.Close();
		};
		TreeView.AfterSelect += delegate(object s, TreeViewEventArgs e)
		{
			this.SelectNodeChanged?.Invoke(TreeView, e);
		};
		treeViewHost = new ToolStripControlHost(TreeView);
		dropDown = new ToolStripDropDown();
		dropDown.Width = base.Width;
		dropDown.Items.Add(treeViewHost);
		dropDown.Font = new Font("微软雅黑", 9f);
		TreeView.Font = new Font("微软雅黑", 9f);
		Font = new Font("微软雅黑", 9f);
		base.ReadOnly = false;
		base.TextDetached = true;
	}

	private void ShowDropDown()
	{
		if (dropDown != null)
		{
			treeViewHost.Size = new Size((DropWidth > 0) ? DropWidth : (base.Width - 2), (DropHeight > 0) ? DropHeight : DropDownForm.Height);
			dropDown.Show(this, 0, base.Height);
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 515 || m.Msg == 513)
		{
			ShowDropDown();
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && dropDown != null)
		{
			dropDown.Dispose();
			dropDown = null;
		}
		base.Dispose(disposing);
	}
}
