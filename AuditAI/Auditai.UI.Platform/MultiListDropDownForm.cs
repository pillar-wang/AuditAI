using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class MultiListDropDownForm : ListDropDownFormBase
{
	protected abstract class MultiListPage
	{
		protected MultiListDropDownForm _owner;

		public abstract Control View { get; }

		public virtual string Result { get; set; }

		public int Index { get; set; }

		public abstract Operand Op { get; set; }

		public abstract void Populate();

		public abstract bool OnTextChanged(string t);

		public abstract void OnKeyDown(KeyEventArgs e);

		public abstract void OnSetTheme();

		public abstract int GetTotalWidth();

		public MultiListPage(MultiListDropDownForm owner)
		{
			_owner = owner;
		}
	}

	private class MultiListSimplePage : MultiListPage
	{
		private ValueSetOperand _op;

		public override Control View => Grid;

		public C1FlexGridEx Grid { get; }

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

		private List<string> _validValues { get; set; }

		public MultiListSimplePage(MultiListDropDownForm owner)
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
				Font = new Font("微软雅黑", 9f)
			};
			Grid.Cols.Count = 1;
			Grid.Cols.Fixed = 0;
			Grid.Rows.Count = 0;
			Grid.Rows.Fixed = 0;
			Grid.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
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

		public override int GetTotalWidth()
		{
			return Grid.GetTotalWidth();
		}

		public override void Populate()
		{
			Grid.BeginUpdate();
			Grid.Rows.Count = 0;
			_validValues = _op.Set.Select((Tuple<Auditai.Model.Row, ValueOperand> tup) => tup.Item2.ToString()).ToList();
			foreach (string validValue in _validValues)
			{
				C1.Win.C1FlexGrid.Row row = Grid.Rows.Add();
				row[0] = validValue;
			}
			Grid.Row = -1;
			Grid.EndUpdate();
		}

		private void Grid_MouseClick(object sender, MouseEventArgs e)
		{
			HitTestInfo hitTestInfo = Grid.HitTest(e.Location);
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				_owner._anyEdit = true;
				Result = _validValues[hitTestInfo.Row];
				_owner.CommitPage();
			}
		}

		public override bool OnTextChanged(string t)
		{
			bool flag = false;
			Grid.BeginUpdate();
			for (int i = 0; i < _validValues.Count; i++)
			{
				bool flag2 = _validValues[i].Contains(t);
				Grid.Rows[i].Visible = flag2;
				if (!flag && flag2)
				{
					flag = true;
				}
			}
			Grid.EndUpdate();
			return flag;
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
					Result = _validValues[Grid.Row];
				}
				break;
			case Keys.Down:
				e.Handled = true;
				if (ToNextVisibleRow())
				{
					Result = _validValues[Grid.Row];
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

		public override void OnSetTheme()
		{
			Grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
		}
	}

	private class MultiListTreePage : MultiListPage
	{
		private TreeListOperand _op;

		private Dictionary<TreeListNode, int> _leaves = new Dictionary<TreeListNode, int>();

		public override Control View => Grid;

		public override Operand Op
		{
			get
			{
				return _op;
			}
			set
			{
				_op = (TreeListOperand)value;
			}
		}

		public C1FlexGridEx Grid { get; }

		public override void Populate()
		{
			_leaves.Clear();
			Grid.BeginUpdate();
			Grid.Rows.Count = 0;
			foreach (TreeListNode root in _op.Roots)
			{
				AddNode(null, root);
			}
			for (int i = 0; i < Grid.Rows.Count; i++)
			{
				Grid.Rows[i].Node.Collapsed = true;
			}
			Grid.EndUpdate();
		}

		private void AddNode(Node parent, TreeListNode n)
		{
			Node node;
			if (parent == null)
			{
				node = Grid.Rows.AddNode(0);
				node.Data = n.Text;
			}
			else
			{
				node = parent.AddNode(NodeTypeEnum.LastChild, n.Text);
			}
			if (n.Children.Count == 0)
			{
				node.Image = Resources.TreeListLeaf3;
				_leaves.Add(n, node.Row.Index);
				return;
			}
			node.Image = Resources.TreeListCollapsed;
			foreach (TreeListNode child in n.Children)
			{
				AddNode(node, child);
			}
		}

		public MultiListTreePage(MultiListDropDownForm owner)
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
				Font = new Font("微软雅黑", 9f)
			};
			Grid.Cols.Count = 1;
			Grid.Cols.Fixed = 0;
			Grid.Rows.Count = 0;
			Grid.Rows.Fixed = 0;
			Grid.Tree.Column = 0;
			Grid.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
			Grid.MouseClick += Grid_MouseClick;
			Grid.MouseWheel += Grid_MouseWheel;
			Grid.MouseMove += Grid_MouseMove;
		}

		public override int GetTotalWidth()
		{
			return Grid.GetTotalWidth();
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
				Node node = Grid.Rows[hitTestInfo.Row].Node;
				if (node.Children == 0)
				{
					_owner._anyEdit = true;
					Result = (string)Grid[hitTestInfo.Row, hitTestInfo.Column];
					_owner.CommitPage();
				}
				else
				{
					node.Collapsed = !node.Collapsed;
				}
			}
		}

		public bool Filter(string s)
		{
			bool flag = false;
			Grid.BeginUpdate();
			foreach (KeyValuePair<TreeListNode, int> leaf in _leaves)
			{
				bool flag2 = leaf.Key.Text.Contains(s);
				if (leaf.Key.Text.Contains(s))
				{
					Grid.Rows[leaf.Value].Node.EnsureVisible();
				}
				else
				{
					Grid.Rows[leaf.Value].Visible = false;
				}
				if (!flag && flag2)
				{
					flag = true;
				}
			}
			Node[] nodes = Grid.Nodes;
			foreach (Node node in nodes)
			{
				node.Row.Visible = SetNodeVisible(node);
				if (node.Row.Visible)
				{
					Grid.Row = node.Row.Index;
				}
			}
			Grid.EndUpdate();
			return flag;
		}

		private bool SetNodeVisible(Node n)
		{
			if (n.Children == 0)
			{
				return n.Row.Visible;
			}
			bool flag = false;
			Node[] nodes = n.Nodes;
			foreach (Node nodeVisible in nodes)
			{
				flag |= SetNodeVisible(nodeVisible);
			}
			return flag;
		}

		public void Up(int count)
		{
			int num = count;
			int num2 = Grid.Row;
			while (num > 0)
			{
				num2--;
				if (num2 < 0)
				{
					Grid.Row = 0;
					return;
				}
				if (Grid.Rows[num2].IsVisible)
				{
					num--;
				}
			}
			Grid.Row = num2;
		}

		public void Down(int count)
		{
			int num = count;
			int num2 = Grid.Row;
			while (num > 0)
			{
				num2++;
				if (num2 >= Grid.Rows.Count)
				{
					Grid.Row = Grid.Rows.Count - 1;
					return;
				}
				if (Grid.Rows[num2].IsVisible)
				{
					num--;
				}
			}
			Grid.Row = num2;
		}

		public bool IsLeaf()
		{
			if (Grid.IsCellValid(Grid.Row, 0))
			{
				return Grid.Rows[Grid.Row].Node.Children == 0;
			}
			return false;
		}

		public string GetText()
		{
			if (Grid.IsCellValid(Grid.Row, 0))
			{
				return (string)Grid[Grid.Row, 0];
			}
			return string.Empty;
		}

		public override bool OnTextChanged(string t)
		{
			return Filter(t);
		}

		public override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
			case Keys.Up:
				e.Handled = true;
				Up(1);
				if (IsLeaf())
				{
					Result = GetText();
				}
				break;
			case Keys.Down:
				e.Handled = true;
				Down(1);
				if (IsLeaf())
				{
					Result = GetText();
				}
				break;
			case Keys.Prior:
				e.Handled = true;
				Up(10);
				break;
			case Keys.Next:
				e.Handled = true;
				Down(10);
				break;
			}
		}

		public override void OnSetTheme()
		{
			Grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
		}
	}

	private class MultiListTablePage : MultiListPage
	{
		private List<string> _validValues = new List<string>();

		private TableListOperand _op;

		public override Control View => Grid;

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

		public MultiListTablePage(MultiListDropDownForm owner)
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

		private void Grid_MouseClick(object sender, MouseEventArgs e)
		{
			HitTestInfo hitTestInfo = Grid.HitTest(e.Location);
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				_owner._anyEdit = true;
				Result = _validValues[hitTestInfo.Row - Grid.Rows.Fixed];
				_owner.CommitPage();
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

		public override bool OnTextChanged(string t)
		{
			bool flag = false;
			Grid.BeginUpdate();
			for (int i = 0; i < _validValues.Count; i++)
			{
				bool flag2 = _validValues[i].Contains(t);
				Grid.Rows[i + Grid.Rows.Fixed].Visible = flag2;
				if (!flag && flag2)
				{
					flag = true;
				}
			}
			Grid.EndUpdate();
			return flag;
		}

		public override int GetTotalWidth()
		{
			return Grid.GetTotalWidth();
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
					Result = _validValues[Grid.Row];
				}
				break;
			case Keys.Down:
				e.Handled = true;
				if (ToNextVisibleRow())
				{
					Result = _validValues[Grid.Row];
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

		public override void OnSetTheme()
		{
			Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
		}
	}

	protected class MultiListTableCheckedPage : MultiListPage
	{
		private TableListOperand _op;

		private List<string> _validValues = new List<string>();

		public override Control View => Grid;

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

		public override string Result
		{
			get
			{
				List<string> list = new List<string>();
				for (int i = Grid.Rows.Fixed; i < Grid.Rows.Count; i++)
				{
					if (Grid.GetCellCheck(i, 0) == CheckEnum.Checked)
					{
						list.Add(_validValues[i - Grid.Rows.Fixed]);
					}
				}
				return string.Join("|", list);
			}
			set
			{
			}
		}

		public MultiListTableCheckedPage(MultiListDropDownForm owner)
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
			Grid.MouseClick += Grid_MouseClick;
			Grid.MouseMove += Grid_MouseMove;
			Grid.MouseWheel += Grid_MouseWheel;
			Grid.CellChecked += Grid_CellChecked;
		}

		private void Grid_CellChecked(object sender, RowColEventArgs e)
		{
			_owner._anyEdit = true;
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
			if (hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Column != 0)
			{
				_owner._anyEdit = true;
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

		public override int GetTotalWidth()
		{
			return Grid.GetTotalWidth();
		}

		public override void Populate()
		{
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

		public override bool OnTextChanged(string t)
		{
			bool flag = false;
			Grid.BeginUpdate();
			for (int i = 0; i < _validValues.Count; i++)
			{
				bool flag2 = _validValues[i].Contains(t);
				Grid.Rows[i + Grid.Rows.Fixed].Visible = flag2;
				if (!flag && flag2)
				{
					flag = true;
				}
			}
			Grid.EndUpdate();
			return flag;
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

		public override void OnSetTheme()
		{
			Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
		}
	}

	protected class MultiListTreeCheckedPage : MultiListPage
	{
		private Dictionary<TreeListNode, int> _leaves = new Dictionary<TreeListNode, int>();

		private TreeListOperand _op;

		public C1FlexGridEx Grid { get; }

		public override Control View => Grid;

		public override Operand Op
		{
			get
			{
				return _op;
			}
			set
			{
				_op = (TreeListOperand)value;
			}
		}

		public override string Result
		{
			get
			{
				List<string> list = new List<string>();
				for (int i = Grid.Rows.Fixed; i < Grid.Rows.Count; i++)
				{
					if (Grid.GetCellCheck(i, 0) == CheckEnum.Checked)
					{
						list.Add((string)Grid[i, 0]);
					}
				}
				return string.Join("|", list);
			}
			set
			{
			}
		}

		public MultiListTreeCheckedPage(MultiListDropDownForm owner)
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
				Font = new Font("微软雅黑", 9f)
			};
			Grid.Cols.Count = 1;
			Grid.Cols.Fixed = 0;
			Grid.Rows.Count = 0;
			Grid.Rows.Fixed = 0;
			Grid.Tree.Column = 0;
			Grid.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
			Grid.MouseClick += Grid_MouseClick;
			Grid.MouseMove += Grid_MouseMove;
			Grid.MouseWheel += Grid_MouseWheel;
			Grid.CellChecked += Grid_CellChecked;
		}

		private void Grid_CellChecked(object sender, RowColEventArgs e)
		{
			_owner._anyEdit = true;
		}

		private void Grid_MouseClick(object sender, MouseEventArgs e)
		{
			HitTestInfo hitTestInfo = Grid.HitTest(e.Location);
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				Node node = Grid.Rows[hitTestInfo.Row].Node;
				if (node.Children == 0)
				{
					(_owner as MultiCheckedListDropDownForm)._anyEdit = true;
					CheckEnum @checked = ((Grid.GetCellCheck(hitTestInfo.Row, 0) != CheckEnum.Checked) ? CheckEnum.Checked : CheckEnum.Unchecked);
					node.Checked = @checked;
				}
				else
				{
					node.Collapsed = !node.Collapsed;
				}
			}
		}

		private void SetCheck(Node n, CheckEnum check)
		{
			Node[] nodes = n.Nodes;
			foreach (Node n2 in nodes)
			{
				SetCheck(n2, check);
			}
			n.Checked = check;
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

		public override int GetTotalWidth()
		{
			return Grid.GetTotalWidth();
		}

		public override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
			case Keys.Up:
				e.Handled = true;
				Up(1);
				break;
			case Keys.Down:
				e.Handled = true;
				Down(1);
				break;
			case Keys.Prior:
				e.Handled = true;
				Up(10);
				break;
			case Keys.Next:
				e.Handled = true;
				Down(10);
				break;
			}
		}

		public void Up(int count)
		{
			int num = count;
			int num2 = Grid.Row;
			while (num > 0)
			{
				num2--;
				if (num2 < 0)
				{
					Grid.Row = 0;
					return;
				}
				if (Grid.Rows[num2].IsVisible)
				{
					num--;
				}
			}
			Grid.Row = num2;
		}

		public void Down(int count)
		{
			int num = count;
			int num2 = Grid.Row;
			while (num > 0)
			{
				num2++;
				if (num2 >= Grid.Rows.Count)
				{
					Grid.Row = Grid.Rows.Count - 1;
					return;
				}
				if (Grid.Rows[num2].IsVisible)
				{
					num--;
				}
			}
			Grid.Row = num2;
		}

		public override bool OnTextChanged(string t)
		{
			return Filter(t);
		}

		public override void Populate()
		{
			_leaves.Clear();
			Grid.BeginUpdate();
			Grid.Rows.Count = 0;
			foreach (TreeListNode root in _op.Roots)
			{
				AddNode(null, root);
			}
			for (int i = 0; i < Grid.Rows.Count; i++)
			{
				Grid.Rows[i].Node.Collapsed = true;
			}
			Grid.EndUpdate();
		}

		private void AddNode(Node parent, TreeListNode n)
		{
			Node node;
			if (parent == null)
			{
				node = Grid.Rows.AddNode(0);
				node.Data = n.Text;
			}
			else
			{
				node = parent.AddNode(NodeTypeEnum.LastChild, n.Text);
			}
			if (n.Children.Count == 0)
			{
				node.Checked = CheckEnum.Unchecked;
				_leaves.Add(n, node.Row.Index);
				return;
			}
			foreach (TreeListNode child in n.Children)
			{
				AddNode(node, child);
			}
		}

		public bool Filter(string s)
		{
			bool flag = false;
			Grid.BeginUpdate();
			foreach (KeyValuePair<TreeListNode, int> leaf in _leaves)
			{
				bool flag2 = leaf.Key.Text.Contains(s);
				if (leaf.Key.Text.Contains(s))
				{
					Grid.Rows[leaf.Value].Node.EnsureVisible();
				}
				else
				{
					Grid.Rows[leaf.Value].Visible = false;
				}
				if (!flag && flag2)
				{
					flag = true;
				}
			}
			Node[] nodes = Grid.Nodes;
			foreach (Node node in nodes)
			{
				node.Row.Visible = SetNodeVisible(node);
				if (node.Row.Visible)
				{
					Grid.Row = node.Row.Index;
				}
			}
			Grid.EndUpdate();
			return flag;
		}

		private bool SetNodeVisible(Node n)
		{
			if (n.Children == 0)
			{
				return n.Row.Visible;
			}
			bool flag = false;
			Node[] nodes = n.Nodes;
			foreach (Node nodeVisible in nodes)
			{
				flag |= SetNodeVisible(nodeVisible);
			}
			return flag;
		}

		public override void OnSetTheme()
		{
			Grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
		}
	}

	protected class MultiListSimpleCheckedPage : MultiListPage
	{
		private ValueSetOperand _op;

		public override Control View => Grid;

		public C1FlexGridEx Grid { get; }

		private List<string> _validValues { get; set; }

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

		public override string Result
		{
			get
			{
				List<string> list = new List<string>();
				for (int i = 0; i < Grid.Rows.Count; i++)
				{
					if (Grid.GetCellCheck(i, 0) == CheckEnum.Checked)
					{
						list.Add((string)Grid[i, 1]);
					}
				}
				return string.Join("|", list);
			}
			set
			{
			}
		}

		public MultiListSimpleCheckedPage(MultiListDropDownForm owner)
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
				BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None
			};
			Grid.Cols.Count = 2;
			Grid.Cols.Fixed = 0;
			Grid.Rows.Fixed = 0;
			Grid.Rows.Count = 0;
			Grid.Cols[0].DataType = typeof(bool);
			Grid.Cols[1].AllowEditing = false;
			Grid.Cols[1].TextAlign = TextAlignEnum.LeftCenter;
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
			if (hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Column != 0)
			{
				Grid.SetCellCheck(hitTestInfo.Row, 0, (Grid.GetCellCheck(hitTestInfo.Row, 0) != CheckEnum.Checked) ? CheckEnum.Checked : CheckEnum.Unchecked);
				(_owner as MultiCheckedListDropDownForm)._anyEdit = true;
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
			_validValues = _op.Set.Select((Tuple<Auditai.Model.Row, ValueOperand> tup) => tup.Item2.ToString()).ToList();
			foreach (string validValue in _validValues)
			{
				C1.Win.C1FlexGrid.Row row = Grid.Rows.Add();
				row[1] = validValue;
			}
			Grid.Row = -1;
			Grid.EndUpdate();
		}

		public override int GetTotalWidth()
		{
			return Grid.GetTotalWidth();
		}

		public override bool OnTextChanged(string t)
		{
			bool flag = false;
			Grid.BeginUpdate();
			for (int i = 0; i < _validValues.Count; i++)
			{
				bool flag2 = _validValues[i].Contains(t);
				Grid.Rows[i].Visible = flag2;
				if (!flag && flag2)
				{
					flag = true;
				}
			}
			Grid.EndUpdate();
			return flag;
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

		public override void OnSetTheme()
		{
			Grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			Grid.Font = _owner.Form.OwnerControl.Font;
			Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
		}
	}

	protected bool _anyEdit;

	protected MultiListOperand _op;

	protected MultiListPage _currentPage;

	protected bool _disableSelectedPageChanged;

	protected override int MinWidth => 200;

	public C1OutBarEx View { get; }

	protected List<MultiListPage> Pages { get; } = new List<MultiListPage>();


	public override Operand Op
	{
		get
		{
			return _op;
		}
		set
		{
			_op = (MultiListOperand)value;
		}
	}

	public MultiListDropDownForm(ListDropDown owner)
		: base(owner)
	{
		View = new C1OutBarEx
		{
			Dock = DockStyle.Fill,
			ShowScrollButtons = false,
			Font = new Font("微软雅黑", 9f)
		};
		View.SelectedPageChanged += View_SelectedPageChanged;
		base.Form.Controls.Add(View);
	}

	protected void View_SelectedPageChanged(object sender, EventArgs e)
	{
		if (_disableSelectedPageChanged)
		{
			return;
		}
		foreach (C1OutPage page in View.Pages)
		{
			page.Image = GetThemedImage(Resources.OutPageCollapsed);
		}
		View.SelectedPage.Image = GetThemedImage(Resources.OutPageExpanded);
		_currentPage = Pages[View.SelectedIndex];
	}

	protected override void OnBeforePostChanges()
	{
		if (_anyEdit)
		{
			base.Text = string.Join("|", from mlp in Pages
				select mlp.Result into s
				where !string.IsNullOrEmpty(s)
				select s);
		}
	}

	protected override int GetTotalWidth()
	{
		return Pages.Max((MultiListPage p) => p.GetTotalWidth());
	}

	public override void Populate()
	{
		_anyEdit = false;
		_disableSelectedPageChanged = true;
		View.Pages.Clear();
		Pages.Clear();
		for (int i = 0; i < _op.MultiList.Count; i++)
		{
			Tuple<string, Operand> tuple = _op.MultiList[i];
			C1OutPage c1OutPage = new C1OutPage();
			c1OutPage.Image = GetThemedImage(Resources.OutPageCollapsed);
			c1OutPage.Text = tuple.Item1;
			MultiListPage multiListPage = CreatePageForOp(tuple.Item2);
			multiListPage.Op = tuple.Item2;
			multiListPage.Index = i;
			multiListPage.Populate();
			Pages.Add(multiListPage);
			c1OutPage.Controls.Add(multiListPage.View);
			View.Pages.Add(c1OutPage);
		}
		_disableSelectedPageChanged = false;
		View_SelectedPageChanged(View, EventArgs.Empty);
	}

	private System.Drawing.Image GetThemedImage(System.Drawing.Image image)
	{
		if (Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			return new WhiteImageStrategy().ProcessImage(image);
		}
		return image;
	}

	protected virtual MultiListPage CreatePageForOp(Operand op)
	{
		if (!(op is ValueSetOperand))
		{
			if (!(op is TreeListOperand))
			{
				if (op is TableListOperand)
				{
					return new MultiListTablePage(this);
				}
				throw new ArgumentOutOfRangeException();
			}
			return new MultiListTreePage(this);
		}
		return new MultiListSimplePage(this);
	}

	private void CommitPage()
	{
		if (Pages.All((MultiListPage mlp) => mlp.Result != null))
		{
			OnBeforePostChanges();
			CloseDropDown();
			_owner.FinishEditing();
			return;
		}
		View.SelectedPage.Text = _op.MultiList[View.SelectedIndex].Item1 + " [已选择：" + _currentPage.Result + "]";
		System.Drawing.Image image = View.SelectedPage.Image;
		View.SelectedPage.Image = null;
		View.SelectedPage.Image = image;
		if (View.SelectedIndex < View.Pages.Count - 1)
		{
			View.SelectedIndex++;
		}
	}

	public override void OnTextChanged(string t)
	{
		foreach (MultiListPage page in Pages)
		{
			if (page.OnTextChanged(t))
			{
				View.SelectedIndex = page.Index;
			}
		}
	}

	public override void OnKeyDown(KeyEventArgs e)
	{
		_currentPage.OnKeyDown(e);
	}

	public override bool Validate()
	{
		return Pages.Any((MultiListPage mlp) => mlp.Result != null);
	}

	public override void OnSetTheme()
	{
		View.PageTitleHeight = 25;
		foreach (MultiListPage page in Pages)
		{
			page.OnSetTheme();
		}
	}
}
