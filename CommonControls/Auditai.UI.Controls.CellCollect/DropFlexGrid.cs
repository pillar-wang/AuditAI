using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using Auditai.Model;

namespace Auditai.UI.Controls.CellCollect;

public class DropFlexGrid : C1ComboBox
{
	private const int WM_LBUTTONDOWN = 513;

	private const int WM_LBUTTONDBLCLK = 515;

	private ToolStripDropDown dropDown;

	private ToolStripControlHost controlHost;

	public C1FlexGrid flex;

	private Voucher selectVoucher;

	private int DropWidth = -1;

	private int DropHeight = -1;

	public Voucher SelectVoucher
	{
		get
		{
			return selectVoucher;
		}
		set
		{
			selectVoucher = value;
			OnSelectChanged(value);
		}
	}

	public event EventHandler<Voucher> SelectChanged;

	public void OnSelectChanged(Voucher voucher)
	{
		this.SelectChanged?.Invoke(this, voucher);
	}

	public DropFlexGrid(int width = -1, int height = -1)
	{
		flex = new C1FlexGrid
		{
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None
		};
		flex.Dock = DockStyle.Fill;
		flex.DoubleClick += Flex_DoubleClick;
		controlHost = new ToolStripControlHost(flex);
		dropDown = new ToolStripDropDown();
		dropDown.Items.Clear();
		dropDown.Items.Add(controlHost);
		DropWidth = width;
		DropHeight = height;
	}

	private void Flex_DoubleClick(object sender, EventArgs e)
	{
		if (flex.Rows[flex.MouseRow].UserData is Voucher voucher)
		{
			Text = voucher.Type.Name + voucher.Number;
			SelectVoucher = voucher;
			dropDown.Close();
		}
	}

	private void DropShow()
	{
		controlHost.Size = new Size((DropWidth > 0) ? DropWidth : (base.Width - 2), (DropHeight > 0) ? DropHeight : DropDownForm.Height);
		dropDown.Show(this, new Point(0, base.Height));
	}

	public new void Clear()
	{
		SelectedText = string.Empty;
		flex.Rows.Count = 0;
		flex.Cols.Count = 0;
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 515 || m.Msg == 513)
		{
			DropShow();
		}
		else
		{
			base.WndProc(ref m);
		}
	}
}
