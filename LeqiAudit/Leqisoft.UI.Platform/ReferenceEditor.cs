﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using Leqisoft.Model;
using Newtonsoft.Json;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class ReferenceEditor
{
	private class ViewModel
	{
		public C1.Win.C1FlexGrid.Row View { get; set; }

		public Cell Cell { get; set; }

		public DataReference Model { get; set; }

		public CrossProjectCellInfo CrossProjectInfo { get; set; }
	}

	private const string CN_NUMBER = "Number";

	private const string CN_KEY = "Key";

	private const string CN_VALUE = "Value";

	private const string CN_REF = "Ref";

	private const string CS_HYPERLINK = "Hyperlink";

	private frmReferenceEditor _form = new frmReferenceEditor();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	private C1CommandLink lnkAppend = new C1CommandLink();

	private C1Command cmdAppend = new C1Command();

	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1CommandLink lnkAppend2 = new C1CommandLink();

	private C1CommandLink lnkRemove = new C1CommandLink();

	private C1Command cmdRemove = new C1Command();

	private C1Command cmdCrossProject = new C1Command
	{
		Text = "跨项目引用单元格",
		Image = ContextResources.ctxAppendRow
	};

	private C1CommandLink lnkCrossProject = new C1CommandLink();

	private C1Command cmdBatchRef = new C1Command { Text = "批量引用多列", Image = ContextResources.ctxAppendRow };
	private C1CommandLink lnkBatchRef = new C1CommandLink();
	private C1Command cmdRangeRef = new C1Command { Text = "区域引用", Image = ContextResources.ctxAppendRow };
	private C1CommandLink lnkRangeRef = new C1CommandLink();
	private C1Command cmdComputeRef = new C1Command { Text = "公式运算引用", Image = ContextResources.ctxAppendRow };
	private C1CommandLink lnkComputeRef = new C1CommandLink();

	private C1.Win.C1FlexGrid.CellStyle _csHyperlink;

	private readonly DataReferenceManager _dataReferenceManager;

	private readonly Project _project;

	public EventHandler<DataReference> ReferenceClicked;

	private C1FlexGridEx _grid => _form._grid;

	private C1Button _btnOk => _form.btnOk;

	private C1Button _btnCancel => _form.btnCancel;

	public DataReference SelectedReference { get; private set; }

	public ReferenceEditor()
	{
		_form.FormClosing += _form_FormClosing;
		_btnCancel.Click += _btnCancel_Click;
		_grid.Cols.Count = 0;
		_grid.ExtendLastCol = true;
		_grid.Rows.DefaultSize = 30;
		_grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		_grid.AllowSorting = AllowSortingEnum.None;
		_grid.SelectionMode = SelectionModeEnum.Row;
		_csHyperlink = _grid.Styles.Add("Hyperlink");
		_csHyperlink.ForeColor = Color.Blue;
		_csHyperlink.Font = new Font(_csHyperlink.Font, FontStyle.Underline);
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		column.Name = "Number";
		column.Caption = "序号";
		column = _grid.Cols.Add();
		column.Name = "Key";
		column.Caption = "变量名";
		column.TextAlign = TextAlignEnum.LeftCenter;
		column = _grid.Cols.Add();
		column.Name = "Value";
		column.Caption = "变量值";
		column.TextAlign = TextAlignEnum.LeftCenter;
		column = _grid.Cols.Add();
		column.Name = "Ref";
		column.Caption = "变量来源";
		column.TextAlign = TextAlignEnum.LeftCenter;
		column.AllowEditing = false;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Fixed = 1;
		_grid.DrawMode = DrawModeEnum.OwnerDraw;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.MouseMove += _grid_MouseMove;
		_grid.BeforeEdit += _grid_BeforeEdit;
		lnkAppend.Command = cmdAppend;
		cmdAppend.CommandStateQuery += CmdAppend_CommandStateQuery;
		cmdAppend.Click += CmdAppend_Click;
		ctxEmpty.CommandLinks.Add(lnkAppend);
		lnkAppend2.Command = cmdAppend;
		ctxCell.CommandLinks.Add(lnkAppend2);
		lnkRemove.Command = cmdRemove;
		cmdRemove.CommandStateQuery += CmdRemove_CommandStateQuery;
		cmdRemove.Click += CmdRemove_Click;
		ctxCell.CommandLinks.Add(lnkRemove);
		lnkCrossProject.Command = cmdCrossProject;
		cmdCrossProject.CommandStateQuery += CmdCrossProject_CommandStateQuery;
		cmdCrossProject.Click += CmdCrossProject_Click;
		ctxCell.CommandLinks.Add(lnkCrossProject);
		lnkBatchRef.Command = cmdBatchRef;
		cmdBatchRef.Click += CmdBatchRef_Click;
		ctxCell.CommandLinks.Add(lnkBatchRef);
		lnkRangeRef.Command = cmdRangeRef;
		cmdRangeRef.Click += CmdRangeRef_Click;
		ctxCell.CommandLinks.Add(lnkRangeRef);
		lnkComputeRef.Command = cmdComputeRef;
		cmdComputeRef.Click += CmdComputeRef_Click;
		ctxCell.CommandLinks.Add(lnkComputeRef);
		Theme.SetCurrentTree(_form);
		_grid.Styles.SelectedColumnHeader.Clear();
		_project = Project.Current;
		_dataReferenceManager = _project.DataReferenceManager;
	}

	public void ShowEdit(string focus = null)
	{
		_btnOk.Click += _btnOk_Edit_Click;
		_grid.KeyPress += _grid_KeyPress;
		_grid.MouseClick += _grid_MouseClick;
		_grid.MouseClick += _grid_ClickReference;
		_form.Text = "变量管理";
		Populate();
		FocusKey(focus);
		_form.ShowDialog();
	}

	public DialogResult ShowSelect()
	{
		_grid.MouseDoubleClick += _grid_MouseDoubleClick;
		_btnOk.Click += _btnOk_Select_Click;
		_form.Text = "插入变量";
		Populate();
		return _form.ShowDialog();
	}

	public void ShowAddCellRef(Cell cell)
	{
		_btnOk.Click += _btnOk_Edit_Click;
		_grid.KeyPress += _grid_KeyPress;
		_grid.MouseClick += _grid_MouseClick;
		_form.Text = "变量管理";
		Populate();
		C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
		ViewModel userData = new ViewModel
		{
			View = row,
			Cell = cell
		};
		row.UserData = userData;
		row["Value"] = cell.GetDisplayValue();
		row["Ref"] = $"{cell.Column.CaptionDisplay},{cell.Row.Index}";
		_grid.StartEditing(row.Index, GetColIndexFromName("Key"));
		_form.ShowDialog();
	}

	private void _form_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall)
		{
			_btnOk.Click -= _btnOk_Edit_Click;
			_btnOk.Click -= _btnOk_Select_Click;
			_grid.KeyPress -= _grid_KeyPress;
			_grid.MouseClick -= _grid_MouseClick;
			_grid.MouseDoubleClick -= _grid_MouseDoubleClick;
			_grid.MouseClick -= _grid_ClickReference;
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col == 0 && e.Row > 0)
		{
			e.Text = e.Row.ToString();
		}
	}

	private void _grid_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (_grid.Row != _grid.Rows.Count - 1)
			{
				_grid.Row++;
			}
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (_grid.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.None:
				ctxEmpty.ShowContextMenu(_grid, e.Location);
				break;
			case HitTestTypeEnum.Cell:
			case HitTestTypeEnum.RowHeader:
				ctxCell.ShowContextMenu(_grid, e.Location);
				break;
			}
		}
	}

	private void _grid_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (_grid.Row >= _grid.Rows.Fixed && _grid.Row < _grid.Rows.Count)
		{
			SelectedReference = (_grid.Rows[_grid.Row].UserData as ViewModel).Model;
			_form.DialogResult = DialogResult.OK;
		}
		else
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择");
		}
	}

	private void _grid_ClickReference(object sender, EventArgs e)
	{
		if (_grid.Row <= _grid.Rows.Fixed || _grid.Row >= _grid.Rows.Count)
		{
			return;
		}
		try
		{
			if (_grid.Rows[_grid.Row].UserData is ViewModel viewModel && _grid.Cols[_grid.MouseCol].Name == "Ref")
			{
				DoubleClickReference(viewModel.Model);
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.Cell && object.Equals(_grid.GetCellStyle(hitTestInfo.Row, hitTestInfo.Column), _csHyperlink))
		{
			_grid.Cursor = Cursors.Hand;
		}
		else
		{
			_grid.Cursor = Cursors.Default;
		}
	}

	private void _grid_BeforeEdit(object sender, RowColEventArgs e)
	{
		if (_grid.Rows[_grid.Row].UserData is ViewModel { Model: not null } viewModel && (viewModel.Model.Kind == DataReferenceKind.CellRef || viewModel.Model.Kind == DataReferenceKind.CrossProjectCellRef))
		{
			string name = _grid.Cols[e.Col].Name;
			if (name == "Value" || name == "Ref")
			{
				e.Cancel = true;
			}
		}
	}

	private void CmdAppend_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
		row["Value"] = "";
		ViewModel userData = new ViewModel
		{
			View = row
		};
		row.UserData = userData;
		_grid.StartEditing(_grid.Rows.Count - 1, GetColIndexFromName("Key"));
	}

	private void CmdAppend_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAppend.Text = "新增行";
		cmdAppend.Image = ContextResources.ctxAppendRow;
	}

	private void CmdRemove_Click(object sender, ClickEventArgs e)
	{
		_grid.Rows.Remove(_grid.Row);
	}

	private void CmdRemove_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRemove.Text = "删除行";
		cmdRemove.Image = ContextResources.ctxDeleteRow;
		if (_grid.MouseRow >= 0 && _grid.MouseCol >= 0 && _grid.Rows[_grid.MouseRow].UserData is ViewModel { Model: not null } viewModel && (viewModel.Model.Kind == DataReferenceKind.Text || viewModel.Model.Kind == DataReferenceKind.CellRef))
		{
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		_form.DialogResult = DialogResult.Cancel;
	}

	private void _btnOk_Edit_Click(object sender, EventArgs e)
	{
		List<ViewModel> list = Validate();
		if (list == null)
		{
			return;
		}
		HashSet<string> keys = GetKeys();
		List<string> list2 = new List<string>();
		foreach (DataReference item in _dataReferenceManager.Enumerate())
		{
			if (!keys.Contains(item.Key))
			{
				list2.Add(item.Key);
			}
		}
		foreach (string item2 in list2)
		{
			_dataReferenceManager.Remove(item2);
		}
		foreach (ViewModel item3 in list)
		{
			string key = (string)item3.View["Key"];
			string text = (string)item3.View["Value"];
			if (_dataReferenceManager.Exists(key))
			{
				DataReference dataReference = _dataReferenceManager.Get(key);
				if (dataReference.Kind == DataReferenceKind.Text && dataReference.Value != text)
				{
					dataReference.UpdateValue(text);
				}
			}
			else if (item3.Cell == null)
			{
				// 检查是否是跨项目引用
				var crossInfo = item3.CrossProjectInfo;
				if (crossInfo != null)
				{
					_dataReferenceManager.AddCrossProject(key, crossInfo.ProjectId, crossInfo.TableId, crossInfo.CellId);
				}
				else
				{
					_dataReferenceManager.Add(key, text);
				}
			}
			else
			{
				_dataReferenceManager.Add(key, item3.Cell);
			}
		}
		_form.DialogResult = DialogResult.OK;
	}

	private void _btnOk_Select_Click(object sender, EventArgs e)
	{
		if (_grid.Row >= _grid.Rows.Fixed && _grid.Row < _grid.Rows.Count)
		{
			SelectedReference = (_grid.Rows[_grid.Row].UserData as ViewModel).Model;
			_form.DialogResult = DialogResult.OK;
		}
		else
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择");
		}
	}

	private void Populate()
	{
		_grid.Rows.Count = 1;
		foreach (DataReference item in _dataReferenceManager.Enumerate())
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
			ViewModel userData = new ViewModel
			{
				View = row,
				Model = item,
				Cell = null
			};
			row.UserData = userData;
			row["Key"] = item.Key;
			row["Value"] = item.GetValue(new DataReferenceEvaluationContext
			{
				Project = _project,
				CurrentTreeNode = Program.MainForm.ProjectHierarchy.SelectedNode
			});
			if (item.Kind == DataReferenceKind.BuiltIn)
			{
				row["Ref"] = "内置变量";
				row.AllowEditing = false;
			}
			else if (item.Kind == DataReferenceKind.Text)
			{
				row["Ref"] = string.Empty;
				row.AllowEditing = true;
			}
			else if (item.Kind == DataReferenceKind.CellRef)
			{
				row["Ref"] = _project.DataReferenceManager.GetCellRefDisplay(item.Value, DataReferenceKind.CellRef);
				_grid.SetCellStyle(row.Index, "Ref", _csHyperlink);
			}
			else if (item.Kind == DataReferenceKind.CrossProjectCellRef)
			{
				row["Ref"] = _project.DataReferenceManager.GetCellRefDisplay(item.Value, DataReferenceKind.CrossProjectCellRef);
				_grid.SetCellStyle(row.Index, "Ref", _csHyperlink);
			}
		}
		_grid.AutoSizeCols();
	}

	private void FocusKey(string focus)
	{
		if (focus == null)
		{
			return;
		}
		int index = _grid.Cols["Value"].Index;
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			if (focus.Equals(_grid[i, "Key"]))
			{
				_grid.Select(new CellRange
				{
					r1 = i,
					r2 = i,
					c1 = index,
					c2 = index
				}, show: true);
				break;
			}
		}
	}

	private int GetColIndexFromName(string name)
	{
		return _grid.Cols[name].Index;
	}

	private List<ViewModel> Validate()
	{
		HashSet<string> hashSet = new HashSet<string>();
		List<ViewModel> list = new List<ViewModel>();
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			string text = (string)_grid[i, "Key"];
			if (string.IsNullOrWhiteSpace(text))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "变量名不能为空");
				_grid.Select(i, GetColIndexFromName("Key"));
				return null;
			}
			if (hashSet.Contains(text))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "变量名不能重复");
				_grid.Select(i, GetColIndexFromName("Key"));
				return null;
			}
			hashSet.Add(text);
			list.Add((ViewModel)_grid.Rows[i].UserData);
		}
		return list;
	}

	private HashSet<string> GetKeys()
	{
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			string item = (string)_grid[i, "Key"];
			hashSet.Add(item);
		}
		return hashSet;
	}

	private void DoubleClickReference(DataReference e)
	{
		if (e != null && (e.Kind == DataReferenceKind.CellRef || e.Kind == DataReferenceKind.CrossProjectCellRef))
		{
			if (e.Kind == DataReferenceKind.CrossProjectCellRef)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "跨项目引用，无法直接跳转");
				return;
			}
			ProjectHierarchy projectHierarchy = Program.MainForm.ProjectHierarchy;
			Cell cell = e.GetCell(new DataReferenceEvaluationContext
			{
				Project = _project,
				CurrentTreeNode = projectHierarchy.SelectedNode
			});
			if (cell != null && projectHierarchy.FindAndSelectNode(cell.Row.Table.TreeNode))
			{
				Program.MainForm.TableEditor.Select(cell.Row.Index, cell.Column.Index);
			}
		}
	}

	private void CmdCrossProject_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCrossProject.Text = "跨项目引用单元格";
		cmdCrossProject.Image = ContextResources.ctxAppendRow;
	}

	private void CmdCrossProject_Click(object sender, ClickEventArgs e)
	{
		// 弹出项目选择对话框
		using (var frm = new frmSelectProject(Program.MainForm.CurrentProject.Id))
		{
			if (frm.ShowDialog() != DialogResult.OK || frm.SelectedProject == null)
				return;
			
			var selectedProject = frm.SelectedProject;
			
			// 创建外部项目实例并加载表格结构
			var externalProject = new Leqisoft.Model.Project
			{
				Id = selectedProject.Id,
				Name = selectedProject.Name
			};
			
			// 本地模式下加载外部项目数据
			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
			{
				try
				{
					string dbPath = MainForm.GetDbPathByGuid(selectedProject.Id);
					var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
					var dto = dal.GetProject();
					if (dto != null)
					{
						externalProject.Name = dto.Name;
					}
				}
				catch { }
			}
			
			// 获取该项目的表格列表
			var tableNodes = externalProject.GetAllTableNodes();
			if (tableNodes == null || !tableNodes.Any())
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该项目没有可用的表格");
				return;
			}
			
			// 弹出表格选择对话框
			using (var tableForm = new Form())
			{
				tableForm.Text = "选择表格 - " + selectedProject.Name;
				tableForm.Size = new Size(400, 300);
				tableForm.StartPosition = FormStartPosition.CenterParent;
				
				var treeView = new TreeView();
				treeView.Dock = DockStyle.Fill;
				tableForm.Controls.Add(treeView);
				
				foreach (var node in tableNodes)
				{
					var tn = treeView.Nodes.Add(node.Name);
					tn.Tag = node;
				}
				
				var btnOk = new Button();
				btnOk.Text = "选择此表格";
				btnOk.Dock = DockStyle.Bottom;
				btnOk.Click += (s, ev) =>
				{
					if (treeView.SelectedNode?.Tag is TreeTableNode tableNode)
					{
						// 用户选择了表格，现在添加跨项目引用变量
						var row = _grid.Rows.Add();
						var viewModel = new ViewModel
						{
							View = row
						};
						row.UserData = viewModel;
						
						// 存储跨项目引用信息
						var projectId = selectedProject.Id;
						var tableId = tableNode.Table.Id;
						
						// 使用第一个单元格作为默认引用
						var firstCell = tableNode.Table.Rows.FirstOrDefault()?.GetCells().FirstOrDefault();
						if (firstCell != null)
						{
							var cellId = firstCell.Id;
							row["Value"] = firstCell.GetDisplayValue();
							row["Ref"] = $"{selectedProject.Name}::{tableNode.Name}[{firstCell.Column.CaptionDisplay},{firstCell.Row.Index + 1}]";
							
							// 存储 Value 格式: {ProjectId}|{TableId}.{CellId}
							row["Key"] = ""; // 用户需要输入变量名
							viewModel.CrossProjectInfo = new CrossProjectCellInfo
							{
								ProjectId = projectId,
								TableId = tableId,
								CellId = cellId
							};
						}
						
						_grid.SetCellStyle(row.Index, "Ref", _csHyperlink);
						_grid.StartEditing(row.Index, GetColIndexFromName("Key"));
						
						tableForm.DialogResult = DialogResult.OK;
						tableForm.Close();
					}
					else
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择一个表格");
					}
				};
				tableForm.Controls.Add(btnOk);
				tableForm.ShowDialog();
			}
		}
	}

	private void CmdBatchRef_Click(object sender, ClickEventArgs e)
	{
		using (var frm = new frmSelectProject(Program.MainForm.CurrentProject.Id))
		{
			if (frm.ShowDialog() != DialogResult.OK || frm.SelectedProject == null)
				return;
			var selectedProject = frm.SelectedProject;

			// 创建外部项目实例并加载表格结构
			var externalProject = new Leqisoft.Model.Project
			{
				Id = selectedProject.Id,
				Name = selectedProject.Name
			};

			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
			{
				try
				{
					string dbPath = MainForm.GetDbPathByGuid(selectedProject.Id);
					var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
					var dto = dal.GetProject();
					if (dto != null)
					{
						externalProject.Name = dto.Name;
					}
				}
				catch { }
			}

			var tableNodes = externalProject.GetAllTableNodes();
			if (tableNodes == null || !tableNodes.Any())
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该项目没有可用的表格");
				return;
			}

			using (var tableForm = new Form())
			{
				tableForm.Text = "选择表格 - " + selectedProject.Name;
				tableForm.Size = new Size(400, 300);
				tableForm.StartPosition = FormStartPosition.CenterParent;
				var treeView = new TreeView();
				treeView.Dock = DockStyle.Fill;
				tableForm.Controls.Add(treeView);
				foreach (var node in tableNodes)
				{
					var tn = treeView.Nodes.Add(node.Name);
					tn.Tag = node;
				}
				var btnOk = new Button();
				btnOk.Text = "选择此表格";
				btnOk.Dock = DockStyle.Bottom;
				btnOk.Click += (s, ev) =>
				{
					if (treeView.SelectedNode?.Tag is TreeTableNode)
					{
						tableForm.DialogResult = DialogResult.OK;
						tableForm.Close();
					}
				};
				tableForm.Controls.Add(btnOk);
				if (tableForm.ShowDialog() != DialogResult.OK)
					return;
				var selectedTableNode = treeView.SelectedNode.Tag as TreeTableNode;
				if (selectedTableNode == null) return;

				// 获取列列表
				var columns = selectedTableNode.Table.Columns.ToList();
				using (var colForm = new Form())
				{
					colForm.Text = "选择要引用的列（批量）";
					colForm.Size = new Size(350, 400);
					colForm.StartPosition = FormStartPosition.CenterParent;
					var chkList = new CheckedListBox();
					chkList.Dock = DockStyle.Fill;
					foreach (var col in columns)
						chkList.Items.Add(col, false);
					colForm.Controls.Add(chkList);
					var btnColOk = new Button();
					btnColOk.Text = "确定";
					btnColOk.Dock = DockStyle.Bottom;
					btnColOk.Click += (s, ev) => { colForm.DialogResult = DialogResult.OK; colForm.Close(); };
					colForm.Controls.Add(btnColOk);
					if (colForm.ShowDialog() != DialogResult.OK) return;

					// 创建批量引用公式
					var selectedColIds = new List<Leqisoft.DTO.Id64>();
					var colCaptions = new List<string>();
					for (int i = 0; i < chkList.Items.Count; i++)
					{
						if (chkList.GetItemChecked(i))
						{
							var col = (Leqisoft.Model.Column)chkList.Items[i];
							selectedColIds.Add(col.Id);
							colCaptions.Add(col.CaptionDisplay);
						}
					}
					if (selectedColIds.Count == 0) return;

					// 添加变量行
					var row = _grid.Rows.Add();
					var viewModel = new ViewModel { View = row };
					row.UserData = viewModel;
					row["Key"] = "";
					row["Value"] = $"批量引用 {selectedProject.Name}::{selectedTableNode.Name} [{string.Join(", ", colCaptions)}]";
					row["Ref"] = $"批量引用 {selectedProject.Name}::{selectedTableNode.Name}";
					_grid.SetCellStyle(row.Index, "Ref", _csHyperlink);
					_grid.StartEditing(row.Index, GetColIndexFromName("Key"));

					// 保存公式到 FormulaStore
					var formula = new CrossProjectFormula
					{
						Id = _project.GetNextId(),
						SourceProjectId = selectedProject.Id,
						SourceTableId = selectedTableNode.Table.Id,
						FormulaType = CrossProjectFormulaType.Batch,
						FormulaExpression = JsonConvert.SerializeObject(colCaptions),
						SourceColumnIds = selectedColIds
					};
					_project.FormulaStore?.Save(formula).Wait();
				}
			}
		}
	}

	private void CmdRangeRef_Click(object sender, ClickEventArgs e)
	{
		using (var frm = new frmSelectProject(Program.MainForm.CurrentProject.Id))
		{
			if (frm.ShowDialog() != DialogResult.OK || frm.SelectedProject == null)
				return;
			var selectedProject = frm.SelectedProject;

			// 创建外部项目实例并加载表格结构
			var externalProject = new Leqisoft.Model.Project
			{
				Id = selectedProject.Id,
				Name = selectedProject.Name
			};

			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
			{
				try
				{
					string dbPath = MainForm.GetDbPathByGuid(selectedProject.Id);
					var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
					var dto = dal.GetProject();
					if (dto != null)
					{
						externalProject.Name = dto.Name;
					}
				}
				catch { }
			}

			var tableNodes = externalProject.GetAllTableNodes();
			if (tableNodes == null || !tableNodes.Any())
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该项目没有可用的表格");
				return;
			}

			using (var tableForm = new Form())
			{
				tableForm.Text = "选择表格 - " + selectedProject.Name;
				tableForm.Size = new Size(400, 300);
				tableForm.StartPosition = FormStartPosition.CenterParent;
				var treeView = new TreeView();
				treeView.Dock = DockStyle.Fill;
				tableForm.Controls.Add(treeView);
				foreach (var node in tableNodes)
				{
					var tn = treeView.Nodes.Add(node.Name);
					tn.Tag = node;
				}
				var btnOk = new Button();
				btnOk.Text = "选择此表格";
				btnOk.Dock = DockStyle.Bottom;
				btnOk.Click += (s, ev) =>
				{
					if (treeView.SelectedNode?.Tag is TreeTableNode)
					{
						tableForm.DialogResult = DialogResult.OK;
						tableForm.Close();
					}
				};
				tableForm.Controls.Add(btnOk);
				if (tableForm.ShowDialog() != DialogResult.OK) return;
				var selectedTableNode = treeView.SelectedNode.Tag as TreeTableNode;
				if (selectedTableNode == null) return;

				// 输入区域范围
				using (var rangeForm = new Form())
				{
					rangeForm.Text = "输入区域范围";
					rangeForm.Size = new Size(300, 200);
					rangeForm.StartPosition = FormStartPosition.CenterParent;
					var lblStartRow = new Label { Text = "起始行:", Location = new Point(10, 15), AutoSize = true };
					var txtStartRow = new TextBox { Location = new Point(80, 12), Width = 50, Text = "0" };
					var lblEndRow = new Label { Text = "结束行:", Location = new Point(150, 15), AutoSize = true };
					var txtEndRow = new TextBox { Location = new Point(220, 12), Width = 50, Text = "0" };
					var lblStartCol = new Label { Text = "起始列:", Location = new Point(10, 50), AutoSize = true };
					var txtStartCol = new TextBox { Location = new Point(80, 47), Width = 50, Text = "0" };
					var lblEndCol = new Label { Text = "结束列:", Location = new Point(150, 50), AutoSize = true };
					var txtEndCol = new TextBox { Location = new Point(220, 47), Width = 50, Text = "0" };
					var btnRangeOk = new Button { Text = "确定", Location = new Point(100, 90), Width = 80 };
					btnRangeOk.Click += (s, ev) => { rangeForm.DialogResult = DialogResult.OK; rangeForm.Close(); };
					rangeForm.Controls.AddRange(new Control[] { lblStartRow, txtStartRow, lblEndRow, txtEndRow, lblStartCol, txtStartCol, lblEndCol, txtEndCol, btnRangeOk });
					if (rangeForm.ShowDialog() != DialogResult.OK) return;
					int startRow = int.Parse(txtStartRow.Text);
					int endRow = int.Parse(txtEndRow.Text);
					int startCol = int.Parse(txtStartCol.Text);
					int endCol = int.Parse(txtEndCol.Text);

					// 添加变量行
					var row = _grid.Rows.Add();
					var viewModel = new ViewModel { View = row };
					row.UserData = viewModel;
					row["Key"] = "";
					row["Value"] = $"区域引用 {selectedProject.Name}::{selectedTableNode.Name} [R{startRow}-R{endRow}, C{startCol}-C{endCol}]";
					row["Ref"] = $"区域引用 {selectedProject.Name}::{selectedTableNode.Name}";
					_grid.SetCellStyle(row.Index, "Ref", _csHyperlink);
					_grid.StartEditing(row.Index, GetColIndexFromName("Key"));

					// 保存公式
					var rangeExpr = JsonConvert.SerializeObject(new { StartRow = startRow, EndRow = endRow, StartCol = startCol, EndCol = endCol });
					var formula = new CrossProjectFormula
					{
						Id = _project.GetNextId(),
						SourceProjectId = selectedProject.Id,
						SourceTableId = selectedTableNode.Table.Id,
						FormulaType = CrossProjectFormulaType.Range,
						FormulaExpression = rangeExpr
					};
					_project.FormulaStore?.Save(formula).Wait();
				}
			}
		}
	}

	private void CmdComputeRef_Click(object sender, ClickEventArgs e)
	{
		using (var frm = new frmSelectProject(Program.MainForm.CurrentProject.Id))
		{
			if (frm.ShowDialog() != DialogResult.OK || frm.SelectedProject == null)
				return;
			var selectedProject = frm.SelectedProject;

			// 创建外部项目实例并加载表格结构
			var externalProject = new Leqisoft.Model.Project
			{
				Id = selectedProject.Id,
				Name = selectedProject.Name
			};

			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
			{
				try
				{
					string dbPath = MainForm.GetDbPathByGuid(selectedProject.Id);
					var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
					var dto = dal.GetProject();
					if (dto != null)
					{
						externalProject.Name = dto.Name;
					}
				}
				catch { }
			}

			var tableNodes = externalProject.GetAllTableNodes();
			if (tableNodes == null || !tableNodes.Any())
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该项目没有可用的表格");
				return;
			}

			using (var tableForm = new Form())
			{
				tableForm.Text = "选择表格 - " + selectedProject.Name;
				tableForm.Size = new Size(400, 300);
				tableForm.StartPosition = FormStartPosition.CenterParent;
				var treeView = new TreeView();
				treeView.Dock = DockStyle.Fill;
				tableForm.Controls.Add(treeView);
				foreach (var node in tableNodes)
				{
					var tn = treeView.Nodes.Add(node.Name);
					tn.Tag = node;
				}
				var btnOk = new Button();
				btnOk.Text = "选择此表格";
				btnOk.Dock = DockStyle.Bottom;
				btnOk.Click += (s, ev) =>
				{
					if (treeView.SelectedNode?.Tag is TreeTableNode)
					{
						tableForm.DialogResult = DialogResult.OK;
						tableForm.Close();
					}
				};
				tableForm.Controls.Add(btnOk);
				if (tableForm.ShowDialog() != DialogResult.OK) return;
				var selectedTableNode = treeView.SelectedNode.Tag as TreeTableNode;
				if (selectedTableNode == null) return;

				var columns = selectedTableNode.Table.Columns.ToList();
				using (var colForm = new Form())
				{
					colForm.Text = "选择列并输入公式";
					colForm.Size = new Size(450, 350);
					colForm.StartPosition = FormStartPosition.CenterParent;
					var lblHint = new Label { Text = "选择列（按顺序对应{0},{1}...）:", Location = new Point(10, 10), AutoSize = true };
					var chkList = new CheckedListBox { Location = new Point(10, 35), Size = new Size(200, 200) };
					foreach (var col in columns)
						chkList.Items.Add(col, false);
					var lblFormula = new Label { Text = "公式:", Location = new Point(220, 35), AutoSize = true };
					var txtFormula = new TextBox { Location = new Point(220, 60), Width = 200, Text = "{0}+{1}" };
					var btnColOk = new Button { Text = "确定", Location = new Point(180, 260), Width = 80 };
					btnColOk.Click += (s, ev) => { colForm.DialogResult = DialogResult.OK; colForm.Close(); };
					colForm.Controls.AddRange(new Control[] { lblHint, chkList, lblFormula, txtFormula, btnColOk });
					if (colForm.ShowDialog() != DialogResult.OK) return;

					var selectedColIds = new List<Leqisoft.DTO.Id64>();
					var colCaptions = new List<string>();
					for (int i = 0; i < chkList.Items.Count; i++)
					{
						if (chkList.GetItemChecked(i))
						{
							var col = (Leqisoft.Model.Column)chkList.Items[i];
							selectedColIds.Add(col.Id);
							colCaptions.Add(col.CaptionDisplay);
						}
					}
					if (selectedColIds.Count == 0) return;
					string formulaExpr = txtFormula.Text;

					// 添加变量行
					var row = _grid.Rows.Add();
					var viewModel = new ViewModel { View = row };
					row.UserData = viewModel;
					row["Key"] = "";
					row["Value"] = $"公式引用 {selectedProject.Name}::{selectedTableNode.Name} [{formulaExpr}]";
					row["Ref"] = $"公式运算 {selectedProject.Name}::{selectedTableNode.Name} [{string.Join(", ", colCaptions)}]";
					_grid.SetCellStyle(row.Index, "Ref", _csHyperlink);
					_grid.StartEditing(row.Index, GetColIndexFromName("Key"));

					// 保存公式
					var formula = new CrossProjectFormula
					{
						Id = _project.GetNextId(),
						SourceProjectId = selectedProject.Id,
						SourceTableId = selectedTableNode.Table.Id,
						FormulaType = CrossProjectFormulaType.Compute,
						FormulaExpression = formulaExpr,
						SourceColumnIds = selectedColIds
					};
					_project.FormulaStore?.Save(formula).Wait();
				}
			}
		}
	}

	// 跨项目单元格信息辅助类
	private class CrossProjectCellInfo
	{
		public Guid ProjectId { get; set; }
		public Leqisoft.DTO.Id64 TableId { get; set; }
		public Leqisoft.DTO.Id64 CellId { get; set; }
	}
}
