using System;
using System.Windows.Forms;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AuxEditor
{
	public frmAuxEdit View { get; private set; }

	public bool IsEditing { get; set; }

	public event EventHandler Closed;

	public void New()
	{
		View = new frmAuxEdit(this)
		{
			Notice = "下拉列表公式：",
			DefaultNotice = "默认值：",
			CommentNotice = "批注："
		};
	}

	public void ShowList(IWin32Window owner)
	{
		View.Text = "下拉列表";
		View.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.ComboList);
		((dynamic)View.DockingTab).SelectedTab = View.tabDropList;
		View.Show(owner);
	}

	public DialogResult ShowComment()
	{
		View.Text = "编辑注释";
		View.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.AuxEditComment);
		((dynamic)View.DockingTab).SelectedTab = View.tabEdit;
		return View.ShowDialog();
	}

	public void OnClosed()
	{
		this.Closed?.Invoke(this, EventArgs.Empty);
	}

	public void InsertRefTextAndFocus(string t)
	{
		View.Activate();
		View.rtbDropInput.Selection.Text = t;
		View.rtbDropInput.Selection.Start += View.rtbDropInput.Selection.Length;
	}

	public void RemoveRefAtPos()
	{
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(View.rtbDropInput.Text);
			Tuple<int, int> refAtPos = formulaDisplay.GetRefAtPos(View.rtbDropInput.Selection.Start);
			if (refAtPos != null)
			{
				View.rtbDropInput.Select(refAtPos.Item1, refAtPos.Item2 - refAtPos.Item1 + 1);
			}
		}
		catch (FormulaException)
		{
		}
	}

	public bool UseWildcard()
	{
		return View.UseWildcard;
	}
}
