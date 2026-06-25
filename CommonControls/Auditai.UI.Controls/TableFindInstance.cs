using System;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class TableFindInstance
{
	private FindReplace form;

	public event EventHandler<FindNextEventArgs> findnexthandler;

	public event EventHandler<ReplaceEventArgs> replacehandler;

	public event EventHandler<FindNextEventArgs> FindNextHandler
	{
		add
		{
			this.findnexthandler = value;
		}
		remove
		{
			findnexthandler -= value;
		}
	}

	public event EventHandler<ReplaceEventArgs> ReplaceHandler
	{
		add
		{
			this.replacehandler = value;
		}
		remove
		{
			replacehandler -= value;
		}
	}

	public void UpdateForm()
	{
		if (form == null || form.IsDisposed)
		{
			form = new FindReplace(this);
			form.FormClosed += ReplaceForm_FormClosed;
			form.FindNextHandler += delegate(object s, FindNextEventArgs e)
			{
				this.findnexthandler?.Invoke(s, e);
			};
			form.ReplaceHandler += delegate(object s, ReplaceEventArgs e)
			{
				this.replacehandler?.Invoke(s, e);
			};
		}
	}

	private void ReplaceForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		form.Dispose();
		form = null;
	}

	public void ShowFind()
	{
		Theme.SetCurrentTree(form);
		form.Show(IsReplace: false);
	}

	public void ShowReplace()
	{
		Theme.SetCurrentTree(form);
		form.Show(IsReplace: true);
	}

	public void HideScopeControls()
	{
		form.lblScope.Hide();
		form.cboScope.Hide();
	}
}
