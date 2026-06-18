using System;
using System.Windows.Forms;

namespace Leqisoft.UI.Controls;

public class DocFindInstance
{
	private ReplaceForm form;

	private event EventHandler<FindReplaceEventArgs> find_nextkeydown;

	private event EventHandler<FindReplaceEventArgs> replace_nextkeydown;

	private event EventHandler<FindReplaceEventArgs> replace_replacekeydown;

	private event EventHandler<FindReplaceEventArgs> replace_replaceallkeydown;

	public event EventHandler<FindReplaceEventArgs> Find_NextKeyDown
	{
		add
		{
			this.find_nextkeydown = value;
		}
		remove
		{
			find_nextkeydown -= value;
		}
	}

	public event EventHandler<FindReplaceEventArgs> Replace_NextKeyDown
	{
		add
		{
			this.replace_nextkeydown = value;
		}
		remove
		{
			replace_nextkeydown -= value;
		}
	}

	public event EventHandler<FindReplaceEventArgs> Replace_ReplaceKeyDown
	{
		add
		{
			this.replace_replacekeydown = value;
		}
		remove
		{
			replace_replacekeydown -= value;
		}
	}

	public event EventHandler<FindReplaceEventArgs> Replace_ReplaceAllKeyDown
	{
		add
		{
			this.replace_replaceallkeydown = value;
		}
		remove
		{
			replace_replaceallkeydown -= value;
		}
	}

	public void UpdateForm()
	{
		if (form == null || form.IsDisposed)
		{
			form = new ReplaceForm();
			form.Find_NextKeyDown += delegate(object s, FindReplaceEventArgs e)
			{
				this.find_nextkeydown?.Invoke(s, e);
			};
			form.Replace_NextKeyDown += delegate(object s, FindReplaceEventArgs e)
			{
				this.replace_nextkeydown?.Invoke(s, e);
			};
			form.Replace_ReplaceKeyDown += delegate(object s, FindReplaceEventArgs e)
			{
				this.replace_replacekeydown?.Invoke(s, e);
			};
			form.Replace_ReplaceAllKeyDown += delegate(object s, FindReplaceEventArgs e)
			{
				this.replace_replaceallkeydown?.Invoke(s, e);
			};
			form.FormClosed += Form_FormClosed;
		}
	}

	private void Form_FormClosed(object sender, FormClosedEventArgs e)
	{
		form.Dispose();
		form = null;
	}

	public void ShowFind()
	{
		form.ShowFind();
	}

	public void ShowReplace()
	{
		form.ShowReplace();
	}

	public void ShowOnlyFind()
	{
		form.ShowFind();
		form.SetCanReplace(canReplace: false);
	}
}
