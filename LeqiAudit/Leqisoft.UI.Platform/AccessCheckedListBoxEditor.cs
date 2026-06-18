using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using Leqisoft.DTO;

namespace Leqisoft.UI.Platform;

public class AccessCheckedListBoxEditor : CheckedListBox, IC1EmbeddedEditor
{
	private const int ROWINDEX_CHECKALL = 0;

	private CellValue Value;

	private C1FlexGrid _owner;

	private bool CanTriggerCheckItemEvent = true;

	public event EventHandler<CellChangeEventArgs> AfterItemCheckedChange;

	public event EventHandler<CellValue> CellEditFinished;

	public AccessCheckedListBoxEditor(C1FlexGrid owner)
	{
		_owner = owner;
		base.CheckOnClick = true;
		base.DisplayMember = "Name";
		base.ItemCheck += AccessCheckedListBoxEditor_ItemCheck;
	}

	public void C1EditorInitialize(object value, IDictionary editorAttributes)
	{
		base.Height = Math.Min(base.PreferredSize.Height, 300);
		Value = value as CellValue;
		if (Value == null)
		{
			return;
		}
		CanTriggerCheckItemEvent = false;
		if (Value.Permission.GrantAll)
		{
			for (int i = 0; i < base.Items.Count; i++)
			{
				SetItemChecked(i, value: true);
			}
			foreach (object item in base.Items)
			{
				User user = item as User;
				if (user.Id != 0L && !Value.Permission.Users.Contains(user.Id))
				{
					Value.Permission.Users.Add(user.Id);
				}
			}
		}
		else
		{
			int r = 0;
			Dictionary<long, int> dictionary = base.Items.Cast<User>().ToDictionary((User u) => u.Id, (User u) => r++);
			for (int j = 0; j < base.Items.Count; j++)
			{
				SetItemChecked(j, value: false);
			}
			foreach (long user2 in Value.Permission.Users)
			{
				if (dictionary.ContainsKey(user2))
				{
					SetItemChecked(dictionary[user2], value: true);
				}
			}
		}
		CanTriggerCheckItemEvent = true;
		SelectedIndex = -1;
		base.TopIndex = 0;
	}

	public object C1EditorGetValue()
	{
		this.CellEditFinished?.Invoke(this, Value);
		return Value;
	}

	private void AccessCheckedListBoxEditor_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		bool flag = e.NewValue == CheckState.Checked;
		CanTriggerCheckItemEvent = false;
		if (e.Index == 0)
		{
			for (int i = 1; i < base.Items.Count; i++)
			{
				SetItemChecked(i, flag);
			}
			if (flag)
			{
				foreach (object item in base.Items)
				{
					User user = item as User;
					if (user.Id != 0L && !Value.Permission.Users.Contains(user.Id))
					{
						Value.Permission.Users.Add(user.Id);
					}
				}
			}
			else
			{
				Value.Permission.Users = new List<long>();
			}
			Value.Permission.GrantAll = flag;
			this.AfterItemCheckedChange?.Invoke(this, new CellChangeEventArgs
			{
				Cell = Value,
				GrantAll = flag
			});
		}
		else
		{
			long id = (base.Items[e.Index] as User).Id;
			if (flag)
			{
				Value.Permission.Users.Add(id);
				this.AfterItemCheckedChange?.Invoke(this, new CellChangeEventArgs
				{
					Cell = Value,
					Add = id
				});
			}
			else
			{
				if (Value.Permission.GrantAll)
				{
					SetItemChecked(0, value: false);
					Value.Permission.GrantAll = false;
				}
				Value.Permission.Users.Remove(id);
				this.AfterItemCheckedChange?.Invoke(this, new CellChangeEventArgs
				{
					Cell = Value,
					Remove = id
				});
			}
		}
		CanTriggerCheckItemEvent = true;
	}

	protected override void OnItemCheck(ItemCheckEventArgs ice)
	{
		if (CanTriggerCheckItemEvent)
		{
			base.OnItemCheck(ice);
		}
	}

	public bool C1EditorKeyDownFinishEdit(KeyEventArgs e)
	{
		return true;
	}

	public bool C1EditorValueIsValid()
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
			return;
		}
		int num2 = rc.Top - base.Height;
		if (base.Height > _owner.Height - 20)
		{
			base.Height = _owner.Height - 20;
		}
		base.Location = new Point(rc.Left, (num2 >= 0) ? num2 : 0);
	}

	public UITypeEditorEditStyle C1EditorGetStyle()
	{
		return UITypeEditorEditStyle.DropDown;
	}

	public string C1EditorFormat(object value, string mask)
	{
		return string.Empty;
	}
}
