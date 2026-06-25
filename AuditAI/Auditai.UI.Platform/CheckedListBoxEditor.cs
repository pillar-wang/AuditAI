using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;

namespace Auditai.UI.Platform;

public class CheckedListBoxEditor : CheckedListBox, IC1EmbeddedEditor
{
	private C1FlexGrid _owner;

	public CheckedListBoxEditor(C1FlexGrid owner)
	{
		_owner = owner;
		base.CheckOnClick = true;
	}

	public string C1EditorFormat(object value, string mask)
	{
		return string.Empty;
	}

	public UITypeEditorEditStyle C1EditorGetStyle()
	{
		return UITypeEditorEditStyle.DropDown;
	}

	public object C1EditorGetValue()
	{
		return string.Join("|", base.CheckedItems.Cast<string>());
	}

	public void C1EditorInitialize(object value, IDictionary editorAttributes)
	{
		if (value is string text && !string.IsNullOrWhiteSpace(text))
		{
			string[] array = text.Split('|');
			foreach (string s in array)
			{
				int num = FindStringExact(s);
				if (num >= 0)
				{
					SetItemChecked(num, value: true);
				}
			}
		}
		base.Height = base.PreferredSize.Height;
	}

	public bool C1EditorKeyDownFinishEdit(KeyEventArgs e)
	{
		return true;
	}

	public void C1EditorUpdateBounds(Rectangle rc)
	{
		base.Width = Math.Max(rc.Width, base.PreferredSize.Width);
		int num = _owner.Height - rc.Bottom;
		if (base.Height < num)
		{
			base.Location = new Point(rc.Left, rc.Bottom);
		}
		else if (num > rc.Top)
		{
			base.Height = num;
			base.Location = new Point(rc.Left, rc.Bottom);
		}
		else
		{
			base.Height = rc.Top;
			base.Location = new Point(rc.Left, 0);
		}
	}

	public bool C1EditorValueIsValid()
	{
		return true;
	}
}
