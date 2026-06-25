using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class ShowColumnsSelector
{
	private frmShowColumns _form;

	private CheckedListBox _clb => _form._clb;

	public IEnumerable<Column> Selected { get; private set; }

	private void BtnOk_Click(object sender, EventArgs e)
	{
		Selected = _clb.CheckedItems.Cast<Column>();
	}

	public DialogResult ShowDialog(Table table)
	{
		_form = new frmShowColumns();
		_form.btnOk.Click += BtnOk_Click;
		foreach (Column item in table.Columns.Where((Column c) => !c.Visible))
		{
			_clb.Items.Add(item);
		}
		return _form.ShowDialog();
	}
}
