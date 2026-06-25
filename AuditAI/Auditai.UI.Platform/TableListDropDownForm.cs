using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class TableListDropDownForm : ListDropDownFormBase
{
	private TableListOperand _op;

	private List<string> _validValues = new List<string>();

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

	public TableListDropDownForm(ListDropDown owner)
		: base(owner)
	{
		Grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			AllowResizing = AllowResizingEnum.None,
			AllowEditing = false,
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
		Grid.Cols.Count = 0;
		Grid.Cols.Fixed = 0;
		Grid.Rows.Count = 1;
		Grid.Rows.Fixed = 1;
		base.Form.Controls.Add(Grid);
		Grid.MouseClick += Grid_MouseClick;
		Grid.MouseMove += Grid_MouseMove;
		Grid.MouseWheel += Grid_MouseWheel;
	}

	private void Grid_MouseWheel(object sender, MouseEventArgs e)
	{
		Point scrollPosition = Grid.ScrollPosition;
		scrollPosition.Offset(0, e.Delta);
		Grid.ScrollPosition = scrollPosition;
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
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			base.Text = _validValues[hitTestInfo.Row - Grid.Rows.Fixed];
			CloseDropDown();
			_owner.FinishEditing();
		}
	}

	public override void Populate()
	{
		_validValues.Clear();
		Grid.BeginUpdate();
		Grid.Cols.Count = 0;
		Grid.Rows.Count = 1;
		for (int i = 0; i < _op.DataTable.Columns.Count; i++)
		{
			C1.Win.C1FlexGrid.Column column = Grid.Cols.Add();
			column.Caption = _op.DataTable.Columns[i].Caption;
			column.TextAlign = (TextAlignEnum)_op.Aligns[i];
			if (i > 0)
			{
				column.StyleNew.ForeColor = Color.DarkGray;
			}
		}
		for (int j = 0; j < _op.DataTable.Rows.Count; j++)
		{
			C1.Win.C1FlexGrid.Row row = Grid.Rows.Add();
			for (int k = 0; k < _op.DataTable.Columns.Count; k++)
			{
				row[k] = _op.DataTable.Rows[j][k];
				if (k == 0)
				{
					_validValues.Add((string)row[0]);
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
			Grid.BodyGetRow(i).Visible = SearchMatches(t, i);
		}
		Grid.EndUpdate();
	}

	private bool SearchMatches(string t, int row)
	{
		for (int i = 0; i < _op.DataTable.Columns.Count; i++)
		{
			if (((string)_op.DataTable.Rows[row][i]).Contains(t))
			{
				return true;
			}
		}
		return false;
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
		while (num - Grid.Rows.Fixed < _validValues.Count - 1)
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
			if (ToPreviousVisibleRow())
			{
				_owner.SkipTextChanged = true;
				base.Text = _validValues[Grid.BodyRow];
				_owner.DropDown.SelectAll();
				_owner.SkipTextChanged = false;
			}
			break;
		case Keys.Down:
			e.Handled = true;
			if (ToNextVisibleRow())
			{
				_owner.SkipTextChanged = true;
				base.Text = _validValues[Grid.BodyRow];
				_owner.DropDown.SelectAll();
				_owner.SkipTextChanged = false;
			}
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
		return _validValues.Contains(base.Text);
	}

	public override void OnSetTheme()
	{
		Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
	}
}
