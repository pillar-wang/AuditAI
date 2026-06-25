using System;
using System.Collections.Generic;
using System.Linq;
using C1.Win.C1Input;

namespace Auditai.UI.Controls;

public class C1SplitButtonEx : C1SplitButton
{
	private Dictionary<string, object> _items = new Dictionary<string, object>();

	public event EventHandler<object> ItemClick;

	public void Initialize()
	{
		base.Items.Clear();
		_items.Clear();
		Text = string.Empty;
	}

	public void AddItem(string text, object tag)
	{
		_items.Add(text, tag);
	}

	public void FinishAdd()
	{
		if (_items.Count == 0)
		{
			return;
		}
		KeyValuePair<string, object> keyValuePair = _items.First();
		Text = keyValuePair.Key;
		base.Tag = keyValuePair.Value;
		base.Click += C1SplitButtonEx_Click;
		foreach (KeyValuePair<string, object> item in _items.Skip(1))
		{
			DropDownItem dropDownItem = new DropDownItem();
			dropDownItem.Click += Drop_Click;
			dropDownItem.Text = item.Key;
			dropDownItem.Tag = item.Value;
			base.Items.Add(dropDownItem);
		}
	}

	private void C1SplitButtonEx_Click(object sender, EventArgs e)
	{
		if (base.Items.Count == 0)
		{
			this.ItemClick?.Invoke(this, base.Tag);
			return;
		}
		int num = 0;
		object tag = base.Tag;
		string text = Text;
		int count = base.Items.Count;
		foreach (DropDownItem item in base.Items)
		{
			if (num == 0)
			{
				base.Tag = item.Tag;
				Text = item.Text;
				this.ItemClick?.Invoke(this, base.Tag);
			}
			if (num >= 0 && num < count - 1)
			{
				item.Tag = base.Items[num + 1].Tag;
				item.Text = base.Items[num + 1].Text;
			}
			if (num == count - 1)
			{
				item.Tag = tag;
				item.Text = text;
			}
			num++;
		}
	}

	private void Drop_Click(object sender, EventArgs e)
	{
		if (sender is DropDownItem dropDownItem)
		{
			object tag = base.Tag;
			string text = Text;
			base.Tag = dropDownItem.Tag;
			Text = dropDownItem.Text;
			dropDownItem.Tag = tag;
			dropDownItem.Text = text;
			this.ItemClick?.Invoke(this, base.Tag);
		}
	}

	public void SelectItem(object tag)
	{
		if (base.Tag.Equals(tag))
		{
			this.ItemClick?.Invoke(this, tag);
			return;
		}
		foreach (DropDownItem item in base.Items)
		{
			if (item.Tag.Equals(tag))
			{
				Drop_Click(item, EventArgs.Empty);
				break;
			}
		}
	}
}
