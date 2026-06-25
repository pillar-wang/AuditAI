using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class TableCheckedListDropDownForm : ListDropDownFormBase
{
	private TableListOperand _op;

	private List<string> _validValues = new List<string>();

	private bool _anyEdit;

	public C1FlexGridEx Grid { get; }

	public override Operand Op
	{
		get
		{
			return _op;
		}
		set
		{
			_op = (TableListOperand)value;
		}
	}

	public TableCheckedListDropDownForm(ListDropDown owner)
		: base(owner)
	{
		Grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			AllowResizing = AllowResizingEnum.None,
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.None,
			AllowMergingFixed = AllowMergingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			SelectionMode = SelectionModeEnum.Row,
			Font = new Font("微软雅黑", 9f)
		};
		Grid.Cols.Count = 1;
		Grid.Cols.Fixed = 0;
		Grid.Rows.Count = 1;
		Grid.Rows.Fixed = 1;
		Grid.Cols[0].DataType = typeof(bool);
		base.Form.Controls.Add(Grid);
		Grid.MouseClick += Grid_MouseClick;
		Grid.MouseMove += Grid_MouseMove;
		Grid.MouseWheel += Grid_MouseWheel;
		Grid.CellChecked += Grid_CellChecked;
	}

	private void Grid_CellChecked(object sender, RowColEventArgs e)
	{
		_anyEdit = true;
	}

	private void Grid_MouseWheel(object sender, MouseEventArgs e)
	{
		Point scrollPosition = Grid.ScrollPosition;
		scrollPosition.Offset(0, e.Delta);
		Grid.ScrollPosition = scrollPosition;
	}

	protected override void OnBeforePostChanges()
	{
		List<string> list = new List<string>();
		for (int i = Grid.Rows.Fixed; i < Grid.Rows.Count; i++)
		{
			if (Grid.GetCellCheck(i, 0) == CheckEnum.Checked)
			{
				list.Add(_validValues[i - Grid.Rows.Fixed]);
			}
		}
		base.Text = string.Join("|", list);
	}

	private void Grid_MouseMove(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Grid.HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			Grid.Row = hitTestInfo.Row;
		}
	}

	private void Grid_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Grid.HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Column != 0)
		{
			_anyEdit = true;
			Grid.SetCellCheck(hitTestInfo.Row, 0, (Grid.GetCellCheck(hitTestInfo.Row, 0) != CheckEnum.Checked) ? CheckEnum.Checked : CheckEnum.Unchecked);
		}
	}

	public void SetInitValue(string initValue)
	{
		Grid.BeginUpdate();
		List<string> list = ((initValue == null) ? new List<string>() : initValue.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList());
		for (int i = 0; i < _validValues.Count; i++)
		{
			string item = _validValues[i];
			if (list.Contains(item))
			{
				Grid.SetCellCheck(i + 1, 0, CheckEnum.Checked);
			}
		}
		Grid.EndUpdate();
	}

	public override void Populate()
	{
		_anyEdit = false;
		Grid.BeginUpdate();
		Grid.Cols.Count = 1;
		Grid.Rows.Count = 1;
		for (int i = 0; i < _op.DataTable.Columns.Count; i++)
		{
			C1.Win.C1FlexGrid.Column column = Grid.Cols.Add();
			column.AllowEditing = false;
			column.Caption = _op.DataTable.Columns[i].Caption;
			column.TextAlign = (TextAlignEnum)_op.Aligns[i];
			if (i > 0)
			{
				column.StyleNew.ForeColor = Color.DarkGray;
			}
		}
		_validValues.Clear();
		for (int j = 0; j < _op.DataTable.Rows.Count; j++)
		{
			string text = (string)_op.DataTable.Rows[j][0];
			if (text != "")
			{
				C1.Win.C1FlexGrid.Row row = Grid.Rows.Add();
				_validValues.Add(text);
				for (int k = 0; k < _op.DataTable.Columns.Count; k++)
				{
					text = (string)_op.DataTable.Rows[j][k];
					row[k + 1] = text;
				}
			}
		}
		Grid.EndUpdate();
	}

	protected override int GetTotalWidth()
	{
		return Grid.GetTotalWidth();
	}

	public override void OnTextChanged(string t)
	{
		Grid.BeginUpdate();
		for (int i = 0; i < _validValues.Count; i++)
		{
			Grid.BodyGetRow(i).Visible = _validValues[i].Contains(t);
		}
		Grid.EndUpdate();
	}

	private bool ToPreviousVisibleRow()
	{
		int num = Grid.Row;
		while (num > Grid.Rows.Fixed)
		{
			num--;
			if (Grid.Rows[num].Visible)
			{
				Grid.Row = num;
				return true;
			}
		}
		return false;
	}

	private bool ToNextVisibleRow()
	{
		int num = Grid.Row;
		while (num - Grid.Rows.Count < _validValues.Count - 1)
		{
			num++;
			if (Grid.Rows[num].Visible)
			{
				Grid.Row = num;
				return true;
			}
		}
		return false;
	}

	public override void OnKeyDown(KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.Up:
			e.Handled = true;
			ToPreviousVisibleRow();
			break;
		case Keys.Down:
			e.Handled = true;
			ToNextVisibleRow();
			break;
		case Keys.Prior:
		{
			e.Handled = true;
			Point scrollPosition = Grid.ScrollPosition;
			scrollPosition.Offset(0, 100);
			Grid.ScrollPosition = scrollPosition;
			break;
		}
		case Keys.Next:
		{
			e.Handled = true;
			Point scrollPosition = Grid.ScrollPosition;
			scrollPosition.Offset(0, -100);
			Grid.ScrollPosition = scrollPosition;
			break;
		}
		}
	}

	public override bool Validate()
	{
		return _anyEdit;
	}

	public override void OnSetTheme()
	{
		Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
	}
}
