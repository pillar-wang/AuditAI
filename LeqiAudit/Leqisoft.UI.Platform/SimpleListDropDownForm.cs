﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class SimpleListDropDownForm : ListDropDownFormBase
{
	private ValueSetOperand _op;

	public override Operand Op
	{
		get
		{
			return _op;
		}
		set
		{
			_op = (ValueSetOperand)value;
		}
	}

	public C1FlexGridEx Grid { get; }

	private List<string> _validValues { get; set; }

	public SimpleListDropDownForm(ListDropDown owner)
		: base(owner)
	{
		Grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.None,
			AllowMergingFixed = AllowMergingEnum.None,
			AllowResizing = AllowResizingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			SelectionMode = SelectionModeEnum.Row,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			ScrollBars = ScrollBars.Vertical
		};
		Grid.Cols.Count = 1;
		Grid.Cols.Fixed = 0;
		Grid.Rows.Fixed = 0;
		Grid.Rows.Count = 0;
		Grid.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
		base.Form.Controls.Add(Grid);
		Grid.MouseMove += Grid_MouseMove;
		Grid.MouseClick += Grid_MouseClick;
		Grid.MouseWheel += Grid_MouseWheel;
	}

	private void Grid_MouseWheel(object sender, MouseEventArgs e)
	{
		Point scrollPosition = Grid.ScrollPosition;
		scrollPosition.Offset(0, e.Delta);
		Grid.ScrollPosition = scrollPosition;
	}

	private void Grid_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Grid.HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			base.Text = _validValues[hitTestInfo.Row];
			CloseDropDown();
			_owner.FinishEditing();
		}
	}

	private void Grid_MouseMove(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Grid.HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			Grid.Row = hitTestInfo.Row;
		}
	}

	public override void Populate()
	{
		Grid.BeginUpdate();
		Grid.Rows.Count = 0;
		_validValues = _op.Set.Select((Tuple<Leqisoft.Model.Row, ValueOperand> tup) => tup.Item2.ToString()).ToList();
		foreach (string validValue in _validValues)
		{
			C1.Win.C1FlexGrid.Row row = Grid.Rows.Add();
			row[0] = validValue;
		}
		Grid.Row = -1;
		Grid.EndUpdate();
		base.Populate();
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
			Grid.Rows[i].Visible = _validValues[i].Contains(t);
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
		while (num < _validValues.Count - 1)
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
				base.Text = _validValues[Grid.Row];
				_owner.DropDown.SelectAll();
				_owner.SkipTextChanged = false;
			}
			break;
		case Keys.Down:
			e.Handled = true;
			if (ToNextVisibleRow())
			{
				_owner.SkipTextChanged = true;
				base.Text = _validValues[Grid.Row];
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
		Grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		Grid.Font = _owner.DropDown.Font;
		Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
	}
}
