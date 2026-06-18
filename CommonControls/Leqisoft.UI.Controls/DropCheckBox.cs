using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;

namespace Leqisoft.UI.Controls;

public class DropCheckBox<T> : C1ComboBox
{
	private const int WM_LBUTTONDOWN = 513;

	private const int WM_LBUTTONDBLCLK = 515;

	private ToolStripDropDown dropDown;

	private ToolStripControlHost controlHost;

	private C1FlexGrid ValueGrid;

	private IEnumerable<KeyValuePair<T, bool>> _value;

	private Func<T, string> _listDisplay;

	private Func<IEnumerable<T>, string> _textDisplay;

	public new IEnumerable<KeyValuePair<T, bool>> Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			UpdateGrid();
		}
	}

	public List<T> SelectedValue { get; private set; }

	public Func<T, string> ValueDisplay
	{
		get
		{
			return _listDisplay;
		}
		set
		{
			_listDisplay = value;
			UpdateValue();
		}
	}

	public Func<IEnumerable<T>, string> TextDisplay
	{
		get
		{
			return _textDisplay;
		}
		set
		{
			_textDisplay = value;
			UpdateText();
		}
	}

	public DropCheckBox()
	{
		base.TextDetached = true;
		SelectedValue = new List<T>();
		ValueGrid = new C1FlexGrid
		{
			Dock = DockStyle.Fill,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None
		};
		ValueGrid.CellChecked += delegate(object s, RowColEventArgs e)
		{
			if (ValueGrid.GetCellCheck(e.Row, e.Col) == CheckEnum.Checked)
			{
				SelectedValue.Add((T)ValueGrid.Rows[e.Row].UserData);
			}
			else
			{
				SelectedValue.Remove((T)ValueGrid.Rows[e.Row].UserData);
			}
			if (SelectedValue.Count == 0)
			{
				Text = string.Empty;
			}
			else
			{
				Text = TextDisplay?.Invoke(SelectedValue);
			}
		};
		controlHost = new ToolStripControlHost(ValueGrid);
		dropDown = new ToolStripDropDown();
		dropDown.Items.Clear();
		dropDown.Items.Add(controlHost);
	}

	private void UpdateValue()
	{
		if (Value != null)
		{
			for (int i = 0; i < Value.Count(); i++)
			{
				ValueGrid[i, 0] = ValueDisplay?.Invoke(Value.ToArray()[i].Key);
			}
		}
	}

	private void UpdateText()
	{
		if (SelectedValue.Count > 0)
		{
			Text = TextDisplay?.Invoke(SelectedValue);
		}
	}

	private void UpdateGrid()
	{
		ValueGrid.Rows.Count = 0;
		ValueGrid.Cols.Count = 0;
		ValueGrid.Rows.Count = Value.Count();
		ValueGrid.Cols.Count = 1;
		ValueGrid.Cols[0].Width = base.Width - 2;
		SelectedValue.Clear();
		int num = 0;
		if (ValueDisplay == null)
		{
			ValueDisplay = (T t) => t.ToString();
		}
		if (TextDisplay == null)
		{
			TextDisplay = (IEnumerable<T> t) => string.Join(";", t.ToString());
		}
		foreach (KeyValuePair<T, bool> item in Value)
		{
			ValueGrid[num, 0] = ValueDisplay?.Invoke(item.Key);
			if (item.Value)
			{
				ValueGrid.SetCellCheck(num, 0, CheckEnum.Checked);
				SelectedValue.Add(item.Key);
			}
			else
			{
				ValueGrid.SetCellCheck(num, 0, CheckEnum.Unchecked);
			}
			ValueGrid.Rows[num].UserData = item.Key;
			num++;
		}
		if (SelectedValue.Count > 0)
		{
			Text = TextDisplay?.Invoke(SelectedValue);
		}
	}

	private void DropShow()
	{
		controlHost.Size = new Size(base.Width - 2, DropDownForm.Height);
		dropDown.Show(this, new Point(0, base.Height));
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
