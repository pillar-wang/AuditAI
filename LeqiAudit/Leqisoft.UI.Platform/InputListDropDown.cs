using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class InputListDropDown
{
	private const int IDX_KEY = 0;

	private const int IDX_VALUE = 1;

	private const int MIN_WIDTH = 200;

	private readonly C1FlexGrid _owner;

	private InputListOperand _op;

	private readonly C1SplitContainer _spl;

	private readonly C1SplitterPanel _pnlGrid;

	private readonly C1SplitterPanel _pnlButtons;

	private readonly C1Button _btnOk;

	private readonly C1Button _btnCancel;

	private readonly C1FlexGridEx _grid;

	private readonly DropDownForm _dropDownForm;

	private readonly Dictionary<string, int> _dicKeyRow = new Dictionary<string, int>();

	public C1DropDownControlEx DropDown { get; }

	public Operand Op
	{
		get
		{
			return _op;
		}
		set
		{
			_op = (InputListOperand)value;
		}
	}

	public bool CanInputTextbox
	{
		get
		{
			return DropDown.DropDownStyle == DropDownStyle.Default;
		}
		set
		{
			DropDown.DropDownStyle = ((!value) ? DropDownStyle.DropDownList : DropDownStyle.Default);
		}
	}

	public InputListDropDown(C1FlexGrid owner)
	{
		_owner = owner;
		DropDown = new C1DropDownControlEx
		{
			ShowUpDownButtons = false,
			MouseClickPassThrough = true,
			GapHeight = 0,
			TrimEnd = false,
			TrimStart = false
		};
		DropDown.DropDownOpened += DropDown_DropDownOpened;
		_dropDownForm = new DropDownForm
		{
			Options = (DropDownFormOptionsFlags.Focusable | DropDownFormOptionsFlags.AlwaysPostChanges | DropDownFormOptionsFlags.NoPostOnEnter),
			BorderStyle = BorderStyle.Fixed3D
		};
		_dropDownForm.Font = new Font("微软雅黑", 9f);
		DropDown.DropDownForm = _dropDownForm;
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.None,
			AllowMergingFixed = AllowMergingEnum.None,
			AllowResizing = AllowResizingEnum.Columns,
			AllowSorting = AllowSortingEnum.None,
			SelectionMode = SelectionModeEnum.CellRange,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			ScrollBars = ScrollBars.Vertical,
			ExtendLastCol = true,
			AutoClipboard = true
		};
		_grid.Rows.DefaultSize = 30;
		_grid.ScrollBars = ScrollBars.Both;
		_spl = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_dropDownForm.CancelChanges += DropDownForm_CancelChanges;
		_dropDownForm.PostChanges += DropDownForm_PostChanges;
		_dropDownForm.Controls.Add(_spl);
		_pnlButtons = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Bottom,
			MinHeight = 0,
			Height = 40,
			KeepRelativeSize = false,
			Resizable = false,
			Collapsible = false
		};
		_spl.Panels.Add(_pnlButtons);
		_pnlGrid = new C1SplitterPanel
		{
			Resizable = false,
			Collapsible = false
		};
		_spl.Panels.Add(_pnlGrid);
		_pnlGrid.Controls.Add(_grid);
		_btnOk = new C1Button
		{
			Text = "确定",
			Anchor = (AnchorStyles.Top | AnchorStyles.Right),
			Location = new Point(-160, 8),
			Size = new Size(70, 25)
		};
		_btnOk.Click += _btnOk_Click;
		_pnlButtons.Controls.Add(_btnOk);
		_btnCancel = new C1Button
		{
			Text = "取消",
			Anchor = (AnchorStyles.Top | AnchorStyles.Right),
			Location = new Point(-80, 8),
			Size = new Size(70, 25)
		};
		_btnCancel.Click += _btnCancel_Click;
		_pnlButtons.Controls.Add(_btnCancel);
		_grid.Cols.Count = 2;
		_grid.Cols.Fixed = 1;
		_grid.Rows.Count = 0;
		_grid.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
		_grid.Cols[0].AllowEditing = false;
		_grid.Cols[0].Width = 150;
		_grid.Cols[1].TextAlign = TextAlignEnum.LeftCenter;
	}

	public void Clear()
	{
		_dicKeyRow.Clear();
		_grid.Rows.Count = 0;
	}

	public void Populate()
	{
		_grid.BeginUpdate();
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols[0].Caption = _op.KeyName;
		_grid.Cols[1].Caption = "输入值";
		foreach (Tuple<Leqisoft.Model.Row, ValueOperand> item in _op.Set)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
			string key = (string)(row[0] = item.Item2.ToString());
			_dicKeyRow.Add(key, row.Index);
		}
		_grid.EndUpdate();
	}

	public void SetInitValue(string s)
	{
		MatchCollection matchCollection = Regex.Matches(s, "\\[([^\\]]*):([^\\]]*)\\]");
		foreach (Match item in matchCollection)
		{
			if (_dicKeyRow.TryGetValue(item.Groups[1].Value, out var value))
			{
				_grid[value, 1] = item.Groups[2].Value;
			}
		}
	}

	public void PopulateError()
	{
		_dicKeyRow.Clear();
		_grid.Rows.Count = 1;
		_grid.Cols[0].Caption = "(公式错误)";
	}

	private void DropDown_DropDownOpened(object sender, EventArgs e)
	{
		SetWidth();
		Theme.SetCurrentTree(_dropDownForm);
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		if (_grid.BodyRow >= 0 && _grid.BodyRow < _grid.BodyRowsCount)
		{
			Commit();
		}
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		Cancel();
	}

	private void DropDownForm_PostChanges(object sender, EventArgs e)
	{
		Commit();
	}

	private void DropDownForm_CancelChanges(object sender, EventArgs e)
	{
		Cancel();
	}

	private void Cancel()
	{
		DropDown.CloseDropDown(doPost: false);
		_owner.FinishEditing(cancel: true);
	}

	private void Commit()
	{
		DropDown.Value = GetValue();
		DropDown.CloseDropDown();
		_owner.FinishEditing();
	}

	private string GetValue()
	{
		_grid.FinishEditing();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			string value = (string)_grid[i, 1];
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.Append("[");
				stringBuilder.Append(((string)_grid[i, 0]) ?? string.Empty);
				stringBuilder.Append(":");
				stringBuilder.Append(value);
				stringBuilder.Append("]");
			}
		}
		return stringBuilder.ToString();
	}

	private void SetWidth()
	{
		int num = _grid.GetTotalWidth(delegate
		{
			if (_grid.Cols[1].WidthDisplay < 120)
			{
				_grid.Cols[1].Width = 120;
			}
		});
		int num2 = Math.Max(DropDown.Width, 200);
		if (num < num2)
		{
			num = num2;
		}
		int num3 = 800;
		if (num > num3)
		{
			num = num3;
		}
		_dropDownForm.Width = num;
	}
}
