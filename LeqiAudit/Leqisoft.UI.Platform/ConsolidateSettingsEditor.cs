﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using Leqisoft.LocalDataStore;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;
using Leqisoft.Util;
using Newtonsoft.Json.Linq;

namespace Leqisoft.UI.Platform;

public class ConsolidateSettingsEditor
{
	private class ProjectTreeColumnEditor : Control, IC1EmbeddedEditor
	{
		private ProjectTreeGrid _editor = new ProjectTreeGrid();

		private Rectangle _cellRect;

		private C1FlexGrid _owner;

		private object _previousValue;

		public ProjectTreeColumnEditor(C1FlexGrid owner)
		{
			_owner = owner;
			_editor.View.ScrollBars = ScrollBars.Both;
			_editor.TreeNodeClicked += _editor_TreeNodeClicked;
			_editor.View.AfterCollapse += View_AfterCollapse;
			base.Controls.Add(_editor.View);
		}

		public void SetProject(Leqisoft.Model.Project p)
		{
			_editor.Project = p;
			_editor.Populate();
			SetHeight();
		}

		public void FindAndSelectNodeById(Id64 nodeId)
		{
			_editor.FindAndSelectNodeById(nodeId);
		}

		public void C1EditorInitialize(object value, IDictionary editorAttributes)
		{
			_previousValue = value;
		}

		public object C1EditorGetValue()
		{
			if (!(_editor.SelectedNode is TreeTableNode treeTableNode))
			{
				return _previousValue;
			}
			return treeTableNode.Table;
		}

		public string C1EditorFormat(object value, string mask)
		{
			return string.Empty;
		}

		public UITypeEditorEditStyle C1EditorGetStyle()
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public bool C1EditorValueIsValid()
		{
			return true;
		}

		public void C1EditorUpdateBounds(Rectangle rc)
		{
			_cellRect = rc;
		}

		public bool C1EditorKeyDownFinishEdit(KeyEventArgs e)
		{
			return true;
		}

		private void _editor_TreeNodeClicked(object sender, TreeNodeEventArgs e)
		{
			if (e.TreeNode is TreeTableNode)
			{
				_owner.FinishEditing();
			}
		}

		private void View_AfterCollapse(object sender, RowColEventArgs e)
		{
			SetHeight();
		}

		private void SetHeight()
		{
			if (_editor.View.Rows.Count != 0)
			{
				C1.Win.C1FlexGrid.Row row = _editor.View.Rows[_editor.View.Rows.Count - 1];
				int bottom = row.Bottom;
				if (bottom > 300)
				{
					bottom = 300;
				}
				bottom = 300;
				int num = _owner.Height - _cellRect.Bottom;
				if (bottom < num)
				{
					base.Location = new Point(_cellRect.Left, _cellRect.Bottom);
				}
				else
				{
					int num2 = _cellRect.Top - bottom;
					base.Location = new Point(_cellRect.Left, (num2 >= 0) ? num2 : 0);
				}
				base.Height = bottom;
			}
		}
	}

	private class MultiColumnEditor : CheckedListBox, IC1EmbeddedEditor
	{
		private C1FlexGrid _owner;

		public MultiColumnEditor(C1FlexGrid owner)
		{
			_owner = owner;
			base.CheckOnClick = true;
			base.DisplayMember = "Caption";
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
			List<KeyValuePair<Leqisoft.DTO.Column, bool>> list = new List<KeyValuePair<Leqisoft.DTO.Column, bool>>();
			for (int i = 0; i < base.Items.Count; i++)
			{
				list.Add(new KeyValuePair<Leqisoft.DTO.Column, bool>((Leqisoft.DTO.Column)base.Items[i], GetItemChecked(i)));
			}
			return list;
		}

		public void C1EditorInitialize(object value, IDictionary editorAttributes)
		{
		}

		public void InitializeValue(object value)
		{
			if (!(value is IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>> enumerable))
			{
				return;
			}
			base.Items.Clear();
			foreach (KeyValuePair<Leqisoft.DTO.Column, bool> item in enumerable)
			{
				base.Items.Add(item.Key, item.Value);
			}
		}

		public bool C1EditorKeyDownFinishEdit(KeyEventArgs e)
		{
			return true;
		}

		public void C1EditorUpdateBounds(Rectangle rc)
		{
			C1.Win.C1FlexGrid.Row row = _owner.Rows[_owner.Row];
			base.Top = row.Top + row.HeightDisplay;
			base.Height = 300;
		}

		public bool C1EditorValueIsValid()
		{
			return true;
		}
	}

	private class ProjectSelectorEditor : C1FlexGrid, IC1EmbeddedEditor
	{
		private ConsolidateSettingsEditor _owner;

		public ProjectSelectorEditor(ConsolidateSettingsEditor owner)
		{
			_owner = owner;
			base.Rows.Count = 0;
			base.Rows.Fixed = 0;
			base.Cols.Count = 1;
			base.Cols.Fixed = 0;
			base.Tree.Column = 0;
			base.AllowEditing = false;
			base.ExtendLastCol = true;
			base.Cols[0].StyleNew.TextAlign = TextAlignEnum.LeftCenter;
			base.Rows.DefaultSize = 30;
			base.MouseClick += ProjectSelectorEditor_MouseClick;
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
			if (base.Row < 0 || base.Row >= base.Rows.Count)
			{
				return null;
			}
			return base.Rows[base.Row].UserData;
		}

		public void C1EditorInitialize(object value, IDictionary editorAttributes)
		{
		}

		public bool C1EditorKeyDownFinishEdit(KeyEventArgs e)
		{
			return true;
		}

		public void C1EditorUpdateBounds(Rectangle rc)
		{
			C1.Win.C1FlexGrid.Row row = _owner._grid.Rows[_owner._grid.Row];
			base.Top = row.Top + row.HeightDisplay;
			base.Height = 300;
		}

		public bool C1EditorValueIsValid()
		{
			return true;
		}

		public void Initialize()
		{
			if (_owner._projects == null || _owner._projects.Count == 0)
			{
				return;
			}
			AddProject(_owner._projects[0], null);
			void AddProject(Leqisoft.Model.Project p, Node parentNode)
			{
				Node node = ((parentNode != null) ? parentNode.AddNode(NodeTypeEnum.LastChild, null) : base.Rows.AddNode(0));
				node.Key = p;
				node.Data = p.Name;
				node.Row.UserData = p;
				foreach (Leqisoft.Model.Project item in _owner._projects.Where((Leqisoft.Model.Project c) => c.ParentId == p.Id))
				{
					AddProject(item, node);
				}
			}
		}

		private void ProjectSelectorEditor_MouseClick(object sender, MouseEventArgs e)
		{
			_owner._grid.FinishEditing();
		}
	}

	private const string CN_NUMBER = "Number";

	private const string CN_SELECTED = "Selected";

	private const string CN_PROJECT = "Project";

	private const string CN_TABLE = "Table";

	private const string CN_GROUP = "Group";

	private const string CN_DATA = "Data";

	private frmConsolidateSettings _form = new frmConsolidateSettings();

	private ProjectTreeColumnEditor _editorTable;

	private MultiColumnEditor _editorColumn;

	private ProjectSelectorEditor _projectSelector;

	private C1ContextMenu _ctxCell = new C1ContextMenu();

	private C1ContextMenu _ctxEmpty = new C1ContextMenu();

	private C1Command cmdAdd = new C1Command
	{
		Text = "新增行",
		Image = ContextResources.ctxAppendRow
	};

	private C1CommandLink lnkAdd = new C1CommandLink();

	private C1CommandLink lnkAdd2 = new C1CommandLink();

	private C1Command cmdRemove = new C1Command
	{
		Text = "删除行",
		Image = ContextResources.ctxDeleteRow
	};

	private C1CommandLink lnkRemove = new C1CommandLink();

	private List<Leqisoft.Model.Project> _projects;

	private C1FlexGrid _grid => _form._grid;

	private DropCheckBox<Leqisoft.Model.Column> _dcbGroup => _form._dcbGroup;

	private DropCheckBox<Leqisoft.Model.Column> _dcbAggregate => _form._dcbAggregate;

	private C1ComboBox _cmbMode => _form._cmbMode;

	private C1Button _btnOk => _form._btnOk;

	private C1Button _btnCancel => _form._btnCancel;

	public Leqisoft.Model.Table Table { get; set; }

	public ConsolidateSettingsEditor()
	{
		_projectSelector = new ProjectSelectorEditor(this);
		Initialize();
	}

	public async Task<DialogResult> ShowDialog()
	{
		_projects = await GetProjects();
		_projectSelector.Initialize();
		await Populate();
		return _form.ShowDialog();
	}

	public static async Task<List<Leqisoft.DTO.Column>> GetTableColumns(Leqisoft.Model.Table table)
	{
		try
		{
			if (StorageRouter.IsLocalMode)
			{
				return await GetTableColumnsLocal(table);
			}
			JObject jObject = new JObject();
			jObject.Add("ProjectId", table.Project.Id);
			jObject.Add("TableId", table.Id.Value);
			return (await WebApiClient.GetTableColumns(jObject)).Select((JToken j) => new Leqisoft.DTO.Column
			{
				Id = new Id64(j.Value<long>("Id")),
				Caption = j.Value<string>("Caption")
			}).ToList();
		}
		catch (HttpRequestException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
			return null;
		}
	}

	private static async Task<List<Leqisoft.DTO.Column>> GetTableColumnsLocal(Leqisoft.Model.Table table)
	{
		var projectDAL = new ProjectDAL(MainForm.GetDbPathByGuid(table.Project.Id));
		var columns = await projectDAL.GetTableColumns(table.Id.Value);
		return columns.Select(c => new Leqisoft.DTO.Column
		{
			Id = new Id64(c.Id.Value),
			Caption = c.Caption
		}).ToList();
	}

	public async Task Populate()
	{
		foreach (ConsolidateEntry source in Table.ConsolidateSettings.Sources)
		{
			Leqisoft.Model.Project project2 = _projects.FirstOrDefault((Leqisoft.Model.Project p) => p.Id == source.ProjectId);
			if (project2 == null)
			{
				continue;
			}
			C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
			row["Project"] = project2;
			_grid.SetCellCheck(row.Index, _grid.Cols["Selected"].Index, source.Selected ? CheckEnum.Checked : CheckEnum.Unchecked);
			Leqisoft.Model.Table table = (Leqisoft.Model.Table)(row["Table"] = project2.GetTableById(source.TableId));
			if (table != null)
			{
				List<Leqisoft.DTO.Column> source2 = await GetTableColumns(table);
				row["Group"] = source2.ToDictionary((Leqisoft.DTO.Column c) => c, (Leqisoft.DTO.Column c) => source.GroupSrcId.Contains(c.Id));
				row["Data"] = source2.ToDictionary((Leqisoft.DTO.Column c) => c, (Leqisoft.DTO.Column c) => source.DataSrcId.Contains(c.Id));
				row["OwnershipRatio"] = source.OwnershipRatio;
				row["Level"] = source.Level;
				var intercompanyModelCols = new HashSet<Id64>(table.Columns.Where(c => c.ConsolidateAttributes != null && c.ConsolidateAttributes.Role == ConsolidateRole.Intercompany).Select(c => c.Id));
				row["IntercompanyCols"] = source2.Where(c => intercompanyModelCols.Contains(c.Id)).ToDictionary((Leqisoft.DTO.Column c) => c, (Leqisoft.DTO.Column c) => source.IntercompanyCols.Contains(c.Id));
			}
		}
		foreach (Leqisoft.Model.Project project in _projects)
		{
			if (!Table.ConsolidateSettings.Sources.Any((ConsolidateEntry s) => s.ProjectId == project.Id))
			{
				C1.Win.C1FlexGrid.Row row2 = _grid.Rows.Add();
				row2["Project"] = project;
			}
		}
		_grid.AutoSizeCol(_grid.Cols["Number"].Index);
		_grid.AutoSizeCol(_grid.Cols["Selected"].Index);
		_grid_Resize(null, EventArgs.Empty);
		_dcbGroup.Value = Table.Columns.Where((Leqisoft.Model.Column c) => c.ConsolidateAttributes == null || c.ConsolidateAttributes.Role == ConsolidateRole.None || c.ConsolidateAttributes.Role == ConsolidateRole.GroupBy).ToDictionary((Leqisoft.Model.Column c) => c, (Leqisoft.Model.Column c) => Table.ConsolidateSettings.GroupDestId.Contains(c.Id));
		_dcbAggregate.Value = Table.Columns.Where((Leqisoft.Model.Column c) => c.ConsolidateAttributes == null || c.ConsolidateAttributes.Role == ConsolidateRole.None || c.ConsolidateAttributes.Role == ConsolidateRole.Aggregate).ToDictionary((Leqisoft.Model.Column c) => c, (Leqisoft.Model.Column c) => Table.ConsolidateSettings.AggregateDestId.Contains(c.Id));
		_cmbMode.SelectedIndex = (int)Table.ConsolidateSettings.Mode;
		try { _form._txtConsolidationName.Text = Table.ConsolidateSettings.ConsolidationName; } catch { }
		_form._chkShowDetail.Checked = Table.ConsolidateSettings.ShowDetail;
		_grid.CellChanged += _grid_CellChanged;
		_grid.AfterEdit += _grid_AfterEdit;
	}

	private void _grid_CellChanged(object sender, RowColEventArgs e)
	{
		if (_grid.Cols[e.Col].Name == "Project")
		{
			_grid[e.Row, "Table"] = null;
		}
		else
		{
			_ = _grid.Cols[e.Col].Name == "Table";
		}
	}

	private async void _grid_AfterEdit(object sender, RowColEventArgs e)
	{
		if (_grid.Cols[e.Col].Name == "Selected")
		{
			if (_grid.GetCellCheck(e.Row, e.Col) != CheckEnum.Checked || e.Row - _grid.Rows.Fixed <= 0)
			{
				return;
			}
			int previousRow = -1;
			for (int num = e.Row - 1; num >= _grid.Rows.Fixed; num--)
			{
				if (_grid.GetCellCheck(num, e.Col) == CheckEnum.Checked)
				{
					previousRow = num;
					break;
				}
			}
			if (previousRow <= -1)
			{
				return;
			}
			Leqisoft.Model.Table previousRowTable = (Leqisoft.Model.Table)_grid[previousRow, "Table"];
			if (previousRowTable == null)
			{
				return;
			}
			Leqisoft.Model.Project project = (Leqisoft.Model.Project)_grid[e.Row, "Project"];
			TreeTableNode treeTableNode = project.GetAllTableNodes().FirstOrDefault((TreeTableNode t) => t.Name == previousRowTable.TreeNode.Name);
			if (treeTableNode != null)
			{
				_grid[e.Row, "Table"] = treeTableNode.Table;
				List<Leqisoft.DTO.Column> list = await GetTableColumns(treeTableNode.Table);
				IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>> source = (IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>>)_grid[previousRow, "Group"];
				List<string> previousGroupCaptions = (from kv in source
					where kv.Value
					select kv.Key.Caption).ToList();
				Dictionary<Leqisoft.DTO.Column, bool> value = list?.ToDictionary((Leqisoft.DTO.Column c) => c, (Leqisoft.DTO.Column c) => previousGroupCaptions.Contains(c.Caption));
				_grid[e.Row, "Group"] = value;
				IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>> source2 = (IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>>)_grid[previousRow, "Data"];
				List<string> previousDataCaptions = (from kv in source2
					where kv.Value
					select kv.Key.Caption).ToList();
				Dictionary<Leqisoft.DTO.Column, bool> value2 = list?.ToDictionary((Leqisoft.DTO.Column c) => c, (Leqisoft.DTO.Column c) => previousDataCaptions.Contains(c.Caption));
				_grid[e.Row, "Data"] = value2;
			}
		}
		else if (_grid.Cols[e.Col].Name == "Table" && _grid[e.Row, e.Col] is Leqisoft.Model.Table table)
		{
			Dictionary<Leqisoft.DTO.Column, bool> value3 = (await GetTableColumns(table))?.ToDictionary((Leqisoft.DTO.Column c) => c, (Leqisoft.DTO.Column c) => false);
			_grid[e.Row, "Group"] = value3;
			_grid[e.Row, "Data"] = value3;
			var intercompanyModelCols = new HashSet<Id64>(table.Columns.Where(c => c.ConsolidateAttributes != null && c.ConsolidateAttributes.Role == ConsolidateRole.Intercompany).Select(c => c.Id));
			_grid[e.Row, "IntercompanyCols"] = value3?.Where(kv => intercompanyModelCols.Contains(kv.Key.Id)).ToDictionary(kv => kv.Key, kv => kv.Value);
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Row < _grid.Rows.Fixed)
		{
			return;
		}
		if (e.Col == _grid.Cols["Project"].Index && _grid[e.Row, e.Col] is Leqisoft.Model.Project project)
		{
			e.Text = project.Name;
		}
		if (e.Col == _grid.Cols["Table"].Index && _grid[e.Row, e.Col] is Leqisoft.Model.Table table)
		{
			e.Text = table.TreeNode.Name;
		}
		if ((e.Col == _grid.Cols["Group"].Index || e.Col == _grid.Cols["Data"].Index || e.Col == _grid.Cols["IntercompanyCols"].Index) && _grid[e.Row, e.Col] is IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>> source)
		{
			e.Text = string.Join("|", from kv in source
				where kv.Value
				select kv.Key.Caption);
		}
	}

	private void _grid_OwnerDrawCell_Number(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col == 0 && e.Row >= _grid.Rows.Fixed)
		{
			e.Text = (e.Row - _grid.Rows.Fixed + 1).ToString();
		}
	}

	private void _grid_SetupEditor(object sender, RowColEventArgs e)
	{
		if (e.Row < _grid.Rows.Fixed)
		{
			return;
		}
		object obj;
		if (e.Col == _grid.Cols["Table"].Index)
		{
			obj = _grid[e.Row, "Project"];
			Leqisoft.Model.Project p = obj as Leqisoft.Model.Project;
			if (p != null)
			{
				_editorTable.SetProject(p);
				ConsolidateEntry consolidateEntry = Table.ConsolidateSettings.Sources.FirstOrDefault((ConsolidateEntry s) => s.ProjectId == p.Id);
				if (consolidateEntry != null)
				{
					_editorTable.FindAndSelectNodeById(consolidateEntry.TableId);
				}
				return;
			}
		}
		if ((e.Col != _grid.Cols["Group"].Index && e.Col != _grid.Cols["Data"].Index && e.Col != _grid.Cols["IntercompanyCols"].Index) || !(_grid[e.Row, "Project"] is Leqisoft.Model.Project))
		{
			return;
		}
		obj = _grid[e.Row, "Table"];
		Leqisoft.Model.Table t = obj as Leqisoft.Model.Table;
		if (t == null)
		{
			return;
		}
		ConsolidateEntry consolidateEntry2 = Table.ConsolidateSettings.Sources.FirstOrDefault((ConsolidateEntry s) => s.TableId == t.Id);
		try
		{
			if (e.Col == _grid.Cols["Group"].Index)
			{
				_editorColumn.InitializeValue(_grid[e.Row, "Group"]);
			}
			else if (e.Col == _grid.Cols["Data"].Index)
			{
				_editorColumn.InitializeValue(_grid[e.Row, "Data"]);
			}
			else if (e.Col == _grid.Cols["IntercompanyCols"].Index)
			{
				var val = _grid[e.Row, "IntercompanyCols"];
				if (val != null)
				{
					_editorColumn.InitializeValue(val);
				}
				else
				{
					var intercompanyCols = t.Columns.Where(c => c.ConsolidateAttributes != null && c.ConsolidateAttributes.Role == ConsolidateRole.Intercompany)
						.Select(c => new Leqisoft.DTO.Column { Id = c.Id, Caption = c.Caption })
						.ToDictionary(c => (Leqisoft.DTO.Column)c, c => false);
					_editorColumn.InitializeValue(intercompanyCols);
				}
			}
		}
		catch (NullReferenceException)
		{
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		switch (_grid.HitTest(e.Location).Type)
		{
		case HitTestTypeEnum.Cell:
		case HitTestTypeEnum.RowHeader:
			if (_grid.MouseRow >= _grid.Rows.Fixed)
			{
				_ctxCell.ShowContextMenu(_grid, e.Location);
			}
			break;
		case HitTestTypeEnum.None:
			_ctxEmpty.ShowContextMenu(_grid, e.Location);
			break;
		}
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		switch (hitTestInfo.Type)
		{
		case HitTestTypeEnum.RowHeader:
			if (hitTestInfo.Row >= _grid.Rows.Fixed)
			{
				_grid.Select(hitTestInfo.Row, 0, hitTestInfo.Row, _grid.Cols.Count - 1);
			}
			break;
		case HitTestTypeEnum.Cell:
			if (!_grid.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
			{
				_grid.Select(hitTestInfo.Row, hitTestInfo.Column);
			}
			break;
		}
	}

	private void _grid_Resize(object sender, EventArgs e)
	{
		int width = (_grid.Width - _grid.Cols["Number"].WidthDisplay - _grid.Cols["Selected"].WidthDisplay) / 7;
		_grid.Cols["Project"].Width = width;
		_grid.Cols["Table"].Width = width;
		_grid.Cols["Group"].Width = width;
		_grid.Cols["Data"].Width = width;
		_grid.Cols["OwnershipRatio"].Width = width;
		_grid.Cols["Level"].Width = width;
		_grid.Cols["IntercompanyCols"].Width = width;
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		if (Validate())
		{
			Apply();
			_form.DialogResult = DialogResult.OK;
			_form.Close();
		}
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		_form.DialogResult = DialogResult.Cancel;
		_form.Close();
	}

	private void CmdRemove_Click(object sender, ClickEventArgs e)
	{
		_grid.Rows.Remove(_grid.Row);
	}

	private void CmdAdd_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
		_grid.SetCellCheck(row.Index, _grid.Cols["Selected"].Index, CheckEnum.Checked);
	}

	private void _form_FormClosed(object sender, FormClosedEventArgs e)
	{
		_grid.CellChanged -= _grid_CellChanged;
	}

	private void Initialize()
	{
		_editorColumn = new MultiColumnEditor(_grid);
		_editorTable = new ProjectTreeColumnEditor(_grid);
		_form.FormClosed += _form_FormClosed;
		_grid.DrawMode = DrawModeEnum.OwnerDraw;
		_grid.ExtendLastCol = true;
		_grid.Rows.DefaultSize = 30;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Count = 0;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		_grid.Cols.Fixed = 1;
		column.Name = "Number";
		column.Caption = "序号";
		column = _grid.Cols.Add();
		column.Name = "Selected";
		column.Caption = "选择";
		column.DataType = typeof(bool);
		column = _grid.Cols.Add();
		column.Name = "Project";
		column.Caption = "来源单体";
		column.Editor = _projectSelector;
		column = _grid.Cols.Add();
		column.Name = "Table";
		column.Caption = "来源报表";
		column.Editor = _editorTable;
		column = _grid.Cols.Add();
		column.Name = "Group";
		column.Caption = "合并维度";
		column.Editor = _editorColumn;
		column = _grid.Cols.Add();
		column.Name = "Data";
		column.Caption = "合并金额";
		column.Editor = _editorColumn;
		column = _grid.Cols.Add();
		column.Name = "OwnershipRatio";
		column.Caption = "持股比例";
		column.DataType = typeof(decimal);
		column.Format = "0.00'%'";
		column = _grid.Cols.Add();
		column.Name = "Level";
		column.Caption = "合并层级";
		column.DataType = typeof(int);
		column = _grid.Cols.Add();
		column.Name = "IntercompanyCols";
		column.Caption = "内部交易列";
		column.Editor = _editorColumn;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell_Number;
		_grid.SetupEditor += _grid_SetupEditor;
		_grid.MouseClick += _grid_MouseClick;
		_grid.MouseDown += _grid_MouseDown;
		_grid.Resize += _grid_Resize;
		_btnOk.Click += _btnOk_Click;
		_btnCancel.Click += _btnCancel_Click;
		cmdAdd.Click += CmdAdd_Click;
		lnkAdd.Command = cmdAdd;
		_ctxCell.CommandLinks.Add(lnkAdd);
		cmdRemove.Click += CmdRemove_Click;
		lnkRemove.Command = cmdRemove;
		_ctxCell.CommandLinks.Add(lnkRemove);
		lnkAdd2.Command = cmdAdd;
		_ctxEmpty.CommandLinks.Add(lnkAdd2);
		Theme.SetCurrentTree(_form);
		_grid.Cols["Project"].TextAlign = TextAlignEnum.LeftCenter;
	}

	private async Task<List<Leqisoft.Model.Project>> GetProjects()
	{
		List<Leqisoft.Model.Project> ret = new List<Leqisoft.Model.Project>();
		try
		{
			if (StorageRouter.IsLocalMode)
			{
				var dtos = await StorageRouter.GetProjects();
				foreach (var dto in dtos)
				{
					if (dto.Id == Program.MainForm.CurrentProject.Id)
						continue;
					ret.Add(new Leqisoft.Model.Project
					{
						Id = dto.Id,
						Name = dto.Name,
						ParentId = dto.ParentId
					});
				}
			}
			else
			{
				ret.Add(Program.MainForm.CurrentProject);
				foreach (Leqisoft.DTO.Project item in await WebApiClient.GetProjectDescendants(Program.MainForm.CurrentProject.Id))
				{
					Leqisoft.Model.Project proj = new Leqisoft.Model.Project
					{
						Id = item.Id,
						Name = item.Name,
						ParentId = item.ParentId
					};
					await Syncer.Pull(proj);
					ret.Add(proj);
				}
			}
		}
		catch (HttpRequestException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		return ret;
	}

	private bool Validate()
	{
		List<int> list = new List<int>();
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			if (_grid.GetCellCheck(i, _grid.Cols["Selected"].Index) == CheckEnum.Checked)
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			return true;
		}
		HashSet<Tuple<Leqisoft.Model.Project, Leqisoft.Model.Table>> hashSet = new HashSet<Tuple<Leqisoft.Model.Project, Leqisoft.Model.Table>>();
		foreach (int item2 in list)
		{
			if (_grid[item2, "Project"] == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "来源单体不能为空");
				return false;
			}
			if (_grid[item2, "Table"] == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "来源报表不能为空");
				return false;
			}
			if (_grid[item2, "Table"] == Table)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "来源报表不能为当前表");
				return false;
			}
			Tuple<Leqisoft.Model.Project, Leqisoft.Model.Table> item = Tuple.Create(_grid[item2, "Project"] as Leqisoft.Model.Project, _grid[item2, "Table"] as Leqisoft.Model.Table);
			if (hashSet.Contains(item))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "来源报表不能有重复");
				return false;
			}
			hashSet.Add(item);
			if (!(_grid[item2, "Group"] is IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>> source) || !source.Any((KeyValuePair<Leqisoft.DTO.Column, bool> kv) => kv.Value))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "合并维度不能为空");
				return false;
			}
			if (!(_grid[item2, "Data"] is IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>> source2) || !source2.Any((KeyValuePair<Leqisoft.DTO.Column, bool> kv) => kv.Value))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "合并金额不能为空");
				return false;
			}
			HashSet<Id64> hashSet2 = new HashSet<Id64>(from kv in source
				where kv.Value
				select kv.Key.Id);
			IEnumerable<Id64> other = from kv in source2
				where kv.Value
				select kv.Key.Id;
			if (hashSet2.Overlaps(other))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "合并维度和合并金额不能重复");
				return false;
			}
			decimal ownershipRatio = (_grid[item2, "OwnershipRatio"] as decimal?) ?? 100m;
			if (ownershipRatio < 0m || ownershipRatio > 100m)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "持股比例必须在0~100之间");
				return false;
			}
			int level = (_grid[item2, "Level"] as int?) ?? 1;
			if (level < 1)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "合并层级必须为正整数");
				return false;
			}
		}
		int num = (_grid[list[0], "Group"] as IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>>).Where((KeyValuePair<Leqisoft.DTO.Column, bool> kv) => kv.Value).Count();
		foreach (int item3 in list.Skip(1))
		{
			if ((_grid[item3, "Group"] as IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>>).Where((KeyValuePair<Leqisoft.DTO.Column, bool> kv) => kv.Value).Count() != num)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "各来源报表的合并维度数量必须相同");
				return false;
			}
		}
		if (num != _dcbGroup.SelectedValue.Count)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "来源报表合并维度数量与目标合并维度数量必须相同");
			return false;
		}
		int num2 = (_grid[list[0], "Data"] as IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>>).Where((KeyValuePair<Leqisoft.DTO.Column, bool> kv) => kv.Value).Count();
		foreach (int item4 in list.Skip(1))
		{
			if ((_grid[item4, "Data"] as IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>>).Where((KeyValuePair<Leqisoft.DTO.Column, bool> kv) => kv.Value).Count() != num2)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "各来源报表的合并金额数量必须相同");
				return false;
			}
		}
		if (num2 != _dcbAggregate.SelectedValue.Count)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "来源报表合并金额数量与目标合并金额数量必须相同");
			return false;
		}
		if (_dcbAggregate.SelectedValue.Intersect(_dcbGroup.SelectedValue).Any())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "目标合并维度与目标合并金额不得重复");
			return false;
		}
		return true;
	}

	private void Apply()
	{
		Table.ConsolidateSettings.Sources = new List<ConsolidateEntry>();
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			Leqisoft.Model.Project project = _grid[i, "Project"] as Leqisoft.Model.Project;
			Leqisoft.Model.Table table = _grid[i, "Table"] as Leqisoft.Model.Table;
			IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>> enumerable = _grid[i, "Group"] as IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>>;
			IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>> enumerable2 = _grid[i, "Data"] as IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>>;
			if (project != null && table != null && enumerable != null && enumerable2 != null)
			{
				ConsolidateEntry item = new ConsolidateEntry
			{
				Selected = (_grid.GetCellCheck(i, _grid.Cols["Selected"].Index) == CheckEnum.Checked),
				ProjectId = project.Id,
				TableId = table.Id,
				GroupSrcId = (from kv in enumerable
					where kv.Value
					select kv.Key.Id).ToList(),
				DataSrcId = (from kv in enumerable2
					where kv.Value
					select kv.Key.Id).ToList(),
				OwnershipRatio = (_grid[i, "OwnershipRatio"] as decimal?) ?? 100m,
				Level = (_grid[i, "Level"] as int?) ?? 1,
				IntercompanyCols = (_grid[i, "IntercompanyCols"] is IEnumerable<KeyValuePair<Leqisoft.DTO.Column, bool>> intercompanyEnum
					? (from kv in intercompanyEnum
						where kv.Value
						select kv.Key.Id).ToList()
					: new List<Id64>())
			};
				Table.ConsolidateSettings.Sources.Add(item);
			}
		}
		if (Table.ConsolidateSettings.Sources.Count != 0)
		{
			Table.ConsolidateSettings.GroupDest = _dcbGroup.SelectedValue.ToList();
			Table.ConsolidateSettings.AggregateDest = _dcbAggregate.SelectedValue.ToList();
			Table.ConsolidateSettings.GroupDestId = Table.ConsolidateSettings.GroupDest.Select((Leqisoft.Model.Column c) => c.Id).ToList();
			Table.ConsolidateSettings.AggregateDestId = Table.ConsolidateSettings.AggregateDest.Select((Leqisoft.Model.Column c) => c.Id).ToList();
			Table.ConsolidateSettings.Mode = (MergeMode)_cmbMode.SelectedIndex;
			Table.ConsolidateSettings.ConsolidationName = _form._txtConsolidationName.Text;
			Table.ConsolidateSettings.ShowDetail = _form._chkShowDetail.Checked;
			Table.TagConsolidateSettingsDirty();
		}
	}
}
