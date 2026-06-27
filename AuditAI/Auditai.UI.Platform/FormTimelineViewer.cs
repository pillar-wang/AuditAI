﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using C1.Win.C1Tile;
using Auditai.LocalDataStore;
using Auditai.DTO;
using Auditai.Model;
using Auditai.SignalR;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using Auditai.Util;
using Newtonsoft.Json.Linq;
using TXTextControl;

namespace Auditai.UI.Platform;

public class FormTimelineViewer
{
	private readonly C1RibbonForm _form;

	private readonly C1FlexGridEx _grid;

	private readonly TextControlEx _tx;

	private readonly C1SplitContainer _ctn;

	private readonly C1SplitContainer _ctnTable;

	private readonly C1SplitterPanel _pnlToolbar;

	private readonly C1SplitterPanel _pnlEntity;

	private readonly C1SplitterPanel _pnlList;

	private readonly C1SplitterPanel _pnlGrid;

	private readonly C1SplitterPanel _pnlTitle;

	private readonly C1TileControlEx _tile;

	private readonly C1FlexGridEx _gridTitle;

	private readonly C1ToolBar _tbr;

	private readonly C1Command _cmdPrevious;

	private readonly C1CommandLink _lnkPrevious;

	private readonly C1Command _cmdNext;

	private readonly C1CommandLink _lnkNext;

	private readonly C1Command _cmdRevert;

	private readonly C1CommandLink _lnkRevert;

	private readonly C1CommandDock _dock;

	private readonly Template _template;

	private bool _isControlCreated;

	private bool _suspendTileClickEnvent;

	public Auditai.Model.Table TemporaryTable { get; set; }

	public Auditai.Model.Document TemporaryDocument { get; set; }

	public int LatestVersion { get; set; }

	public Auditai.Model.Table RevertedTable { get; set; }

	public Auditai.Model.Document RevertedDocument { get; set; }

	public List<PushEntityMeta> Metas { get; } = new List<PushEntityMeta>();


	public int SelectedIndex { get; private set; }

	public FormTimelineViewer()
	{
		_form = FormFactory.Create();
		_form.WindowState = FormWindowState.Maximized;
		_form.Text = "历史版本";
		_pnlToolbar = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Height = 30,
			KeepRelativeSize = false,
			Resizable = false
		};
		_tbr = new C1ToolBar
		{
			Dock = DockStyle.Fill,
			ButtonLayoutHorz = ButtonLayoutEnum.TextBelow,
			ButtonLookHorz = ButtonLookFlags.TextAndImage,
			MinButtonSize = 42,
			AutoSize = true
		};
		_cmdPrevious = new C1Command
		{
			Text = "上一个版本",
			Image = Resources.PreviousError
		};
		_cmdPrevious.Click += _cmdPrevious_Click;
		_lnkPrevious = new C1CommandLink(_cmdPrevious);
		_tbr.CommandLinks.Add(_lnkPrevious);
		_cmdNext = new C1Command
		{
			Text = "下一个版本",
			Image = Resources.NextError
		};
		_cmdNext.Click += _cmdNext_Click;
		_lnkNext = new C1CommandLink(_cmdNext);
		_tbr.CommandLinks.Add(_lnkNext);
		_cmdRevert = new C1Command
		{
			Text = "恢复当前历史版本",
			Image = Resources.RevertTable
		};
		_cmdRevert.Click += _cmdRevert_Click;
		_lnkRevert = new C1CommandLink(_cmdRevert);
		_tbr.CommandLinks.Add(_lnkRevert);
		_dock = new C1CommandDock();
		_dock.Controls.Add(_tbr);
		_pnlToolbar.Controls.Add(_dock);
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowEditing = false,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.Custom,
			AllowMergingFixed = AllowMergingEnum.Custom,
			AllowResizing = AllowResizingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			DrawMode = DrawModeEnum.OwnerDraw,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			Visible = true
		};
		_grid.Rows.Count = 0;
		_grid.Cols.Count = 1;
		_grid.Cols.Fixed = 1;
		_ctn = new C1SplitContainer
		{
			Dock = DockStyle.Fill,
			FixedLineWidth = 0
		};
		_ctn.Panels.Add(_pnlToolbar);
		_pnlList = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			Width = 210,
			KeepRelativeSize = false
		};
		_ctn.Panels.Add(_pnlList);
		_tile = new C1TileControlEx
		{
			CellWidth = 200,
			Dock = DockStyle.Fill,
			Padding = new Padding(0),
			AutomaticLayout = true,
			SurfacePadding = new Padding(10, 10, 0, 0),
			GroupPadding = new Padding(0),
			GroupSpacing = 10,
			Orientation = LayoutOrientation.Vertical
		};
		_pnlList.Controls.Add(_tile);
		_template = new Template();
		_tile.TileClicked += _tile_TileClicked;
		_tile.Resize += _tile_Resize;
		_tile.Templates.Add(_template);
		PanelElement item = new PanelElement
		{
			Dock = DockStyle.Right,
			AlignmentOfContents = ContentAlignment.MiddleCenter,
			Padding = new Padding(0, 0, 10, 0)
		};
		ImageElement imageElement = new ImageElement
		{
			ImageSelector = ImageSelector.Image1
		};
		_template.Elements.Add(item);
		PanelElement panelElement = new PanelElement
		{
			AlignmentOfContents = ContentAlignment.TopLeft,
			Padding = new Padding(10, 20, 0, 0),
			Dock = DockStyle.Top
		};
		TextElement item2 = new TextElement
		{
			TextSelector = TextSelector.Text1
		};
		ImageElement item3 = new ImageElement
		{
			ImageSelector = ImageSelector.Image2
		};
		TextElement item4 = new TextElement
		{
			TextSelector = TextSelector.Text2
		};
		TextElement item5 = new TextElement
		{
			TextSelector = TextSelector.Text3,
			Alignment = ContentAlignment.BottomLeft,
			Margin = new Padding(10, 0, 0, 20)
		};
		panelElement.Children.Add(item2);
		panelElement.Children.Add(item3);
		panelElement.Children.Add(item4);
		_template.Elements.Add(panelElement);
		_template.Elements.Add(item5);
		_pnlTitle = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Resizable = false,
			KeepRelativeSize = false
		};
		_gridTitle = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowSorting = AllowSortingEnum.None,
			AllowResizing = AllowResizingEnum.None,
			ScrollBars = ScrollBars.None,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			AllowMerging = AllowMergingEnum.Custom,
			AllowEditing = false,
			Visible = true
		};
		_gridTitle.Rows.Count = 1;
		_gridTitle.Rows.Fixed = 0;
		_gridTitle.Cols.Count = 0;
		_gridTitle.Cols.Fixed = 0;
		_pnlTitle.Controls.Add(_gridTitle);
		_pnlTitle.Resize += _pnlTitle_Resize;
		_ctnTable = new C1SplitContainer
		{
			Dock = DockStyle.Fill,
			Visible = false,
			FixedLineWidth = 1
		};
		_ctnTable.Panels.Add(_pnlTitle);
		_pnlGrid = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			SizeRatio = 100.0,
			Resizable = false
		};
		_ctnTable.Panels.Add(_pnlGrid);
		_pnlGrid.Controls.Add(_grid);
		_pnlEntity = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Resizable = false,
			KeepRelativeSize = false
		};
		_ctn.Panels.Add(_pnlEntity);
		_pnlEntity.Controls.Add(_ctnTable);

		// 确保 TXTextControl 许可证已注入，避免授权失败
		Program.EnsureTXTextControlLicense();

		_tx = new TextControlEx
		{
			Dock = DockStyle.Fill,
			Visible = false
		};
		_pnlEntity.Controls.Add(_tx);
		_form.Controls.Add(_ctn);
		_form.Shown += _form_Shown;
		_form.Layout += _form_Layout;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
	}

	public DialogResult ShowDialog()
	{
		return _form.ShowDialog();
	}

	public async Task PopulateTableTimeline()
	{
		JObject request = new JObject();
		request.Add("TableId", TemporaryTable.Id.Value);
		request.Add("ProjectId", Auditai.Model.Project.Current.Id);
		JArray jarray = null;
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		progressRuntimeData.NextStep("正在获取历史版本...");
		progressRuntimeData.UpdateProgress(0.9f);
		await progressForm.ShowDialog(progressRuntimeData, async delegate
		{
			await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
			if (!StorageRouter.IsLocalMode)
			{
				jarray = await WebApiClient.GetTableTimeline(request);
			}
		}, 1000);
		if (jarray == null)
		{
			jarray = new JArray();
		}
		Metas.Clear();
		for (int i = 0; i < jarray.Count; i++)
		{
			JToken jToken = jarray[i];
			PushEntityMeta pushEntityMeta = new PushEntityMeta
			{
				Version = jToken.Value<int>("Version"),
				Length = jToken.Value<int>("Length"),
				UserId = jToken.Value<long>("UserId"),
				Time = jToken.Value<DateTime>("Time")
			};
			Metas.Add(pushEntityMeta);
			C1.Win.C1Tile.Group group = new C1.Win.C1Tile.Group();
			_tile.Groups.Add(group);
			Member member = MemberManager.GetInstance().GetMember(pushEntityMeta.UserId.ToString());
			Tile item = new Tile
			{
				Template = _template,
				Text1 = $"第{i + 1}版",
				Image1 = Resources.RevertTableDark,
				Image2 = member?.Image?.ToSize(16, 16),
				Text2 = (member?.Name ?? "[未知用户]") + "编辑",
				Text3 = $"{pushEntityMeta.Time:G}  {Auditai.Model.Util.GetReadableFileSize(pushEntityMeta.Length)}",
				IntValue1 = i
			};
			group.Tiles.Add(item);
		}
		if (Metas.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该表格没有历史版本。");
			_form.DialogResult = DialogResult.Cancel;
		}
		else
		{
			SelectedIndex = Metas.Count - 1;
			SelectTile(SelectedIndex);
			ShowTileToSelectedStatus(SelectedIndex);
		}
		await Task.Delay(1);
	}

	public async Task PopulateDocumentTimeline()
	{
		JObject request = new JObject();
		request.Add("DocId", TemporaryDocument.Id.Value);
		request.Add("ProjectId", Auditai.Model.Project.Current.Id);
		JArray jarray = null;
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		progressRuntimeData.NextStep("正在获取历史版本...");
		progressRuntimeData.UpdateProgress(0.9f);
		await progressForm.ShowDialog(progressRuntimeData, async delegate
		{
			await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
			if (!StorageRouter.IsLocalMode)
			{
				jarray = await WebApiClient.GetDocumentTimeline(request);
			}
		}, 1000);
		if (jarray == null)
		{
			jarray = new JArray();
		}
		Metas.Clear();
		for (int i = 0; i < jarray.Count; i++)
		{
			JToken jToken = jarray[i];
			PushEntityMeta pushEntityMeta = new PushEntityMeta
			{
				Version = jToken.Value<int>("Version"),
				Length = jToken.Value<int>("Length"),
				UserId = jToken.Value<long>("UserId"),
				Time = jToken.Value<DateTime>("Time")
			};
			Metas.Add(pushEntityMeta);
			C1.Win.C1Tile.Group group = new C1.Win.C1Tile.Group();
			_tile.Groups.Add(group);
			Member member = MemberManager.GetInstance().GetMember(pushEntityMeta.UserId.ToString());
			Tile item = new Tile
			{
				Template = _template,
				Text1 = $"第{i + 1}版",
				Image1 = Resources.RevertTableDark,
				Image2 = member?.Image?.ToSize(16, 16),
				Text2 = (member?.Name ?? "[未知用户]") + "编辑",
				Text3 = $"{pushEntityMeta.Time:G}  {Auditai.Model.Util.GetReadableFileSize(pushEntityMeta.Length)}",
				IntValue1 = i
			};
			group.Tiles.Add(item);
		}
		if (Metas.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该文档没有历史版本。");
			_form.DialogResult = DialogResult.Cancel;
		}
		else
		{
			SelectedIndex = Metas.Count - 1;
			SelectTile(SelectedIndex);
			ShowTileToSelectedStatus(SelectedIndex);
		}
		await Task.Delay(1);
	}

	public void PopulateTable()
	{
		_grid.BeginUpdate();
		_grid.Cols[0].StyleNew.TextAlign = TextAlignEnum.CenterCenter;
		_grid.BodyRowsCount = RevertedTable.Rows.Count;
		_grid.BodyColsCount = RevertedTable.Columns.Count;
		_grid.Cols.Frozen = RevertedTable.FrozenCols;
		for (int i = 0; i < RevertedTable.Rows.Count; i++)
		{
			PopulateRow(RevertedTable.Rows[i], _grid.BodyGetRow(i));
		}
		_grid.AutoSizeCol(0);
		PopulateColumns();
		_grid.AutoSizeCol(0);
		PopulateMerges();
		if (_grid.Cols[0].Width < 56)
		{
			_grid.Cols[0].Width = 56;
		}
		PopulateTitle();
		_grid.EndUpdate();
	}

	public async Task<object> PopulateDocument()
	{
		try
		{
			if (!_isControlCreated)
			{
				_isControlCreated = true;
				_tx.CreateControl();
			}
			Tuple<string, List<DocumentLoadCellMerge>, List<Id64>> tup = RevertedDocument.MakePackage();
			await Task.Delay(100);
			_tx.Load(File.ReadAllBytes(tup.Item1), BinaryStreamType.WordprocessingML);
			_tx.EditMode = EditMode.ReadAndSelect;
			File.Delete(tup.Item1);
			return null;
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因内存加载资源过多，历史文档未能成功打开，可尝试重新登录再打开历史文档！");
			return null;
		}
	}

	public void PopulateColumns()
	{
		for (int i = 0; i < RevertedTable.Columns.Count; i++)
		{
			PopulateColumn(RevertedTable.Columns[i], _grid.BodyGetCol(i));
		}
		for (int num = _grid.MergedRanges.Count - 1; num >= 0; num--)
		{
			if (_grid.MergedRanges[num].TopRow < _grid.Rows.Fixed)
			{
				_grid.MergedRanges.RemoveAt(num);
			}
		}
		int numCaptionRows = RevertedTable.GetNumCaptionRows();
		_grid.Rows.RemoveRange(0, _grid.Rows.Fixed);
		_grid.Rows.InsertRange(0, numCaptionRows);
		_grid.Rows.Fixed = numCaptionRows;
		for (int j = 0; j < numCaptionRows; j++)
		{
			_grid.Rows[j].StyleNew.WordWrap = true;
		}
		foreach (Auditai.Model.Column column2 in RevertedTable.Columns)
		{
			string[] array = column2.CaptionDisplay.Split('_');
			for (int k = 0; k < array.Length; k++)
			{
				_grid.SetData(k, column2.Index + _grid.Cols.Fixed, array[k]);
			}
		}
		List<Auditai.DTO.CellRange> mergeInfo = RevertedTable.GetMergeInfo(visibleOnly: false);
		foreach (Auditai.DTO.CellRange item in mergeInfo)
		{
			_grid.MergedRanges.Add(item.r1, item.c1 + _grid.Cols.Fixed, item.r2, item.c2 + _grid.Cols.Fixed);
		}
		for (int l = 0; l < numCaptionRows; l++)
		{
			try
			{
				_grid.Rows[l].Height = RevertedTable.GetHeaderHeight(l);
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
		for (int m = 0; m < RevertedTable.Columns.Count; m++)
		{
			Auditai.Model.Column column = RevertedTable.Columns[m];
			FontStyle fontStyle = FontStyle.Regular;
			if (column.CaptionStyle.Bold.GetValueOrDefault())
			{
				fontStyle |= FontStyle.Bold;
			}
			if (column.CaptionStyle.Italic.GetValueOrDefault())
			{
				fontStyle |= FontStyle.Italic;
			}
			if (column.CaptionStyle.Underline.GetValueOrDefault())
			{
				fontStyle |= FontStyle.Underline;
			}
			C1.Win.C1FlexGrid.CellRange cellRange = _grid.GetCellRange(0, m + _grid.Cols.Fixed, _grid.Rows.Fixed - 1, m + _grid.Cols.Fixed);
			cellRange.StyleNew.ForeColor = column.CaptionStyle.ForeColor.Value;
			cellRange.StyleNew.Font = new Font(column.CaptionStyle.FontFamily, column.CaptionStyle.FontSize.Value, fontStyle);
			cellRange.StyleNew.TextAlign = (TextAlignEnum)column.CaptionStyle.Align.Value;
		}
	}

	private void PopulateMerges()
	{
		List<C1.Win.C1FlexGrid.CellRange> list = (from C1.Win.C1FlexGrid.CellRange merge in _grid.MergedRanges
			where merge.TopRow >= _grid.Rows.Fixed && merge.LeftCol >= _grid.Cols.Fixed
			select merge).ToList();
		foreach (C1.Win.C1FlexGrid.CellRange item in list)
		{
			_grid.MergedRanges.Remove(item);
		}
		foreach (CellMerge mergedCell in RevertedTable.MergedCells)
		{
			_grid.BodyAddMergedRange(mergedCell.TopLeft.Row.Index, mergedCell.TopLeft.Column.Index, mergedCell.BottomRight.Row.Index, mergedCell.BottomRight.Column.Index);
			for (int i = mergedCell.TopLeft.Row.Index; i <= mergedCell.BottomRight.Row.Index; i++)
			{
				for (int j = mergedCell.TopLeft.Column.Index; j <= mergedCell.BottomRight.Column.Index; j++)
				{
					_grid.BodyGetCell(i, j).StyleNew.DataType = null;
				}
			}
		}
	}

	private async void _cmdRevert_Click(object sender, ClickEventArgs e)
	{
		if (TemporaryTable != null)
		{
			await DoRevertTableByIndex(SelectedIndex);
		}
		else
		{
			await DoRevertDocumentByIndex(SelectedIndex);
		}
	}

	private void _cmdNext_Click(object sender, ClickEventArgs e)
	{
		if (SelectedIndex < Metas.Count - 1)
		{
			SelectedIndex++;
			SelectTile(SelectedIndex);
		}
	}

	private void _cmdPrevious_Click(object sender, ClickEventArgs e)
	{
		if (SelectedIndex > 0)
		{
			SelectedIndex--;
			SelectTile(SelectedIndex);
		}
	}

	private void _form_Layout(object sender, LayoutEventArgs e)
	{
		_pnlToolbar.Height = _dock.Height;
	}

	private async void _form_Shown(object sender, EventArgs e)
	{
		Theme.SetCurrentTree(_form);
		_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Snapshots);
		_cmdPrevious.Image = Theme.SelectedAuditaiTheme.GetThemedBitmap(Resources.PreviousError);
		_cmdNext.Image = Theme.SelectedAuditaiTheme.GetThemedBitmap(Resources.NextError);
		_cmdRevert.Image = Theme.SelectedAuditaiTheme.GetThemedBitmap(Resources.RevertTable);
		_tile.TileBorderColor = Color.Transparent;
		if (TemporaryTable != null)
		{
			await PopulateTableTimeline();
			_ctnTable.Show();
		}
		else
		{
			await PopulateDocumentTimeline();
			_tx.Show();
		}
	}

	private void _pnlTitle_Resize(object sender, EventArgs e)
	{
		if (RevertedTable != null)
		{
			TableTitle title = RevertedTable.Title;
			float num = title.Columns.Sum((TableTitleColumn c) => c.Width);
			for (int i = 0; i < title.Columns.Count; i++)
			{
				_gridTitle.Cols[i].Width = (int)(title.Columns[i].Width / num * (float)_gridTitle.Width);
			}
		}
	}

	private void _tile_Resize(object sender, EventArgs e)
	{
		_tile.CellWidth = Math.Min(_tile.Width - 30, 900);
	}

	private void ShowTileToSelectedStatus(int selectIndex)
	{
		_suspendTileClickEnvent = true;
		try
		{
			Tile tile = _tile.Groups[selectIndex].Tiles[0];
			_tile.SelectTile(tile);
			_tile.Invalidate();
		}
		catch (Exception)
		{
		}
		finally
		{
			_suspendTileClickEnvent = false;
		}
	}

	private async void _tile_TileClicked(object sender, TileEventArgs e)
	{
		if (!_suspendTileClickEnvent)
		{
			SelectedIndex = e.Tile.IntValue1;
			if (TemporaryTable != null)
			{
				await PopulateTableByIndex(SelectedIndex);
			}
			else
			{
				await PopulateDocumentByIndex(SelectedIndex);
			}
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col == 0 && e.Row >= _grid.Rows.Fixed)
		{
			e.Text = (e.Row - _grid.Rows.Fixed + 1).ToString();
		}
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		Auditai.Model.Cell cell = RevertedTable[e.Row, e.Col];
		C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
		styleNew.DataType = cell.DisplayDataType;
		DataFormat displayFormat = cell.DisplayFormat;
		if (displayFormat.FormatType == DataFormatType.BoolCheckBox)
		{
			e.Text = string.Empty;
			e.Image = (cell.Value.Equals(true) ? _grid.Glyphs[GlyphEnum.Checked] : _grid.Glyphs[GlyphEnum.Unchecked]);
			styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cell.DisplayAlign);
		}
		else if (displayFormat.FormatType == DataFormatType.BoolOnOff)
		{
			e.Text = string.Empty;
			e.Image = (cell.Value.Equals(true) ? Resources.On : Resources.Off);
			styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cell.DisplayAlign);
		}
		else
		{
			e.Image = null;
			e.Text = cell.GetDisplayValue();
		}
		switch (displayFormat.FormatType)
		{
		case DataFormatType.General:
		case DataFormatType.Number:
		case DataFormatType.Percentage:
		case DataFormatType.NumDollar:
		case DataFormatType.NumRmb:
		case DataFormatType.DateSlash:
		case DataFormatType.DateDash:
		case DataFormatType.DateChinese:
		case DataFormatType.Comma:
		case DataFormatType.BoolCheckBox:
		case DataFormatType.BoolOnOff:
		case DataFormatType.DateYearMonthChinese:
		case DataFormatType.DateYearMonthDash:
		case DataFormatType.DateYearMonthSlash:
		case DataFormatType.DateYearMonthDot:
		case DataFormatType.DateDot:
			styleNew.DataMap = null;
			break;
		case DataFormatType.BoolYesNo:
		case DataFormatType.BoolRightWrong:
		case DataFormatType.BoolTickCross:
			styleNew.DataMap = cell.DisplayFormat.GetFormatDictForBool();
			break;
		case DataFormatType.ComboList:
			styleNew.DataMap = null;
			break;
		}
		styleNew.BackColor = GetBackColor();
		if (cell.HasFormula || cell.Column.HasFormula || cell.Row.Role == RowRole.Subtotal || cell.Row.Role == RowRole.Total || (cell.Column.ConsolidateAttributes != null && cell.Column.ConsolidateAttributes.Role != 0) || cell.TryGetHeaderCellFormulaCell(out var _))
		{
			if (cell.IsExistManualInputValue && cell.IsAllowManualInputOnFormula)
			{
				styleNew.BackColor = Color.White;
			}
			else if (cell.IsAllowManualInputOnFormula)
			{
				styleNew.BackColor = UserSet.Config.TableStyle.AllowManualInputFormulaColor;
			}
			else
			{
				styleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
			}
		}
		bool flag = false;
		if (cell.Row.Role == RowRole.Among)
		{
			flag = true;
			styleNew.BackColor = UserSet.Config.TableStyle.RowAmongColor;
		}
		else if (cell.Row.Role == RowRole.Minus)
		{
			flag = true;
			styleNew.BackColor = UserSet.Config.TableStyle.RowMinusColor;
		}
		else if (cell.Row.Role == RowRole.Subtotal || cell.Row.Role == RowRole.Total)
		{
			flag = true;
			styleNew.BackColor = UserSet.Config.TableStyle.RowTotalColor;
		}
		else if (cell.Row.Role == RowRole.Fixed)
		{
			styleNew.BackColor = UserSet.Config.TableStyle.RowFixedColor;
		}
		if (flag && cell.IsExistManualInputValue && cell.IsAllowManualInputOnFormula)
		{
			styleNew.BackColor = Color.White;
		}
		if (cell.DisplayLocked != 0L)
		{
			styleNew.BackColor = UserSet.Config.TableStyle.LockAreaColor;
		}
		if (cell.Row.Role == RowRole.Header)
		{
			styleNew.BackColor = _grid.Styles[CellStyleEnum.Fixed].BackColor;
		}
		styleNew.ForeColor = cell.DisplayForeColor;
		styleNew.Font = cell.GetFont();
		styleNew.TextAlign = (TextAlignEnum)cell.DisplayAlign;
		Margins margins = (Margins)styleNew.Margins.Clone();
		margins.Left = cell.DisplayMargin;
		styleNew.Margins = margins;
		styleNew.WordWrap = cell.Value is string;
		Color GetBackColor()
		{
			Color displayBackColor = cell.DisplayBackColor;
			if (displayBackColor.ToArgb() == -1)
			{
				return Color.Transparent;
			}
			return displayBackColor;
		}
	}

	private async Task PopulateTableByIndex(int index)
	{
		PushEntityMeta meta = Metas[index];
		_grid.BeginUpdate();
		RevertedTable = TemporaryTable.TemporaryClone();
		if (LatestVersion != meta.Version)
		{
			try
			{
				ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
				ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
				progressRuntimeData.NextStep("正在预览历史版本，请稍候...");
				progressForm.ShowDialog(progressRuntimeData, async delegate
				{
					await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
					await Syncer.RevertTemporary(RevertedTable, LatestVersion, meta.Version, progressRuntimeData.UpdateProgress);
				});
			}
			catch (HttpRequestException ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
			}
		}
		PopulateTable();
		_grid.EndUpdate();
		await Task.Delay(1);
	}

	private async Task PopulateDocumentByIndex(int index)
	{
		PushEntityMeta meta = Metas[index];
		RevertedDocument = TemporaryDocument.TemporaryClone();
		if (LatestVersion != meta.Version)
		{
			try
			{
				ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
				ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
				progressRuntimeData.NextStep("正在预览历史版本，请稍候...");
				progressForm.ShowDialog(progressRuntimeData, async delegate
				{
					await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
					await Syncer.RevertTemporary(RevertedDocument, LatestVersion, meta.Version, progressRuntimeData.UpdateProgress);
				});
			}
			catch (HttpRequestException ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
			}
		}
		await Task.Yield();
		await PopulateDocument();
	}

	private async Task DoRevertTableByIndex(int index)
	{
		PushEntityMeta meta = Metas[index];
		try
		{
			ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStep("正在恢复历史版本，请稍候...");
			Auditai.Model.Table table = RevertedTable;
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				await Syncer.Revert(table, LatestVersion, meta.Version, progressRuntimeData.UpdateProgress);
			});
			table.TreeNode.SetTable(table);
			Auditai.Model.Project.Current.Dal.DeleteTable(table.Id);
			table.NeedSave = true;
			foreach (Auditai.Model.Row row in table.Rows)
			{
				row.NeedSave = true;
			}
			foreach (Auditai.Model.Cell cell in table.Cells)
			{
				cell.NeedSave = true;
			}
			table.Save();
			_form.DialogResult = DialogResult.OK;
			Program.MainForm.TableEditor.RecoverHistoryTable(table);
			if (!StorageRouter.IsLocalMode)
			{
				await SignalRClient.SyncProject(Auditai.Model.Project.Current.Id.ToString());
			}
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, string.Format(ex.InnerException.Message, StringConstBase.Current.Manager));
		}
	}

	private async Task DoRevertDocumentByIndex(int index)
	{
		PushEntityMeta meta = Metas[index];
		Auditai.Model.Document document = Program.MainForm.CurrentDocumentEditor.Document;
		if (document.Version != LatestVersion)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文档数据不是最新版本，需要同步后才可执行。");
			return;
		}
		try
		{
			ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStep("正在恢复历史版本，请稍候...");
			progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				await Syncer.Revert(document, meta.Version, progressRuntimeData.UpdateProgress);
			});
			document.Save();
			_form.DialogResult = DialogResult.OK;
			Program.MainForm.CurrentDocumentEditor.PopulateDocument();
			if (!StorageRouter.IsLocalMode)
			{
				await SignalRClient.SyncProject(Auditai.Model.Project.Current.Id.ToString());
			}
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
	}

	private void PopulateRow(Auditai.Model.Row model, C1.Win.C1FlexGrid.Row view)
	{
		view.Height = model.Height;
		view.Visible = model.Visible;
	}

	private void PopulateColumn(Auditai.Model.Column model, C1.Win.C1FlexGrid.Column view)
	{
		view.Width = model.Width;
		view.Visible = model.Visible;
		view.DataType = null;
	}

	private void PopulateTitle()
	{
		TableTitle title = RevertedTable.Title;
		_gridTitle.Rows.Count = 1 + title.Rows.Count;
		_gridTitle.Cols.Count = title.Columns.Count;
		_pnlTitle_Resize(_pnlTitle, EventArgs.Empty);
		_gridTitle.SetCellStyle(0, 0, (C1.Win.C1FlexGrid.CellStyle)null);
		_gridTitle.Rows[0].Height = title.TitleHeight;
		_gridTitle.MergedRanges.Clear();
		_gridTitle.BodyAddMergedRange(0, 0, 0, title.Columns.Count - 1);
		foreach (TicketMerge merge in title.Merges)
		{
			_gridTitle.BodyAddMergedRange(merge.TopRow + 1, merge.LeftColumn, merge.BottomRow + 1, merge.RightColumn);
		}
		PopulateTitleCell(0, 0, title.TitleCell);
		for (int i = 0; i < title.Rows.Count; i++)
		{
			TableTitleRow tableTitleRow = title.Rows[i];
			C1.Win.C1FlexGrid.Row row = _gridTitle.Rows[i + 1];
			row.Height = tableTitleRow.Height;
			for (int j = 0; j < tableTitleRow.Cells.Count; j++)
			{
				PopulateTitleCell(i + 1, j, tableTitleRow.Cells[j]);
			}
		}
		_pnlTitle.Height = GetTitleTotalHeight();
	}

	private void PopulateTitleCell(int row, int col, TableTitleCell cell)
	{
		_gridTitle[row, col] = cell.GetDisplayValue();
		C1.Win.C1FlexGrid.CellStyle styleNew = _gridTitle.GetCellRange(row, col).StyleNew;
		FontStyle fontStyle = FontStyle.Regular;
		if (cell.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (cell.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (cell.Strikeout)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (cell.Underline)
		{
			fontStyle |= FontStyle.Underline;
		}
		styleNew.Font = new Font(cell.FontFamily, cell.FontSize, fontStyle);
		styleNew.ForeColor = cell.ForeColor;
		styleNew.BackColor = cell.BackColor;
		if (row == 0)
		{
			styleNew.TextAlign = TextAlignEnum.CenterCenter;
		}
		else
		{
			styleNew.TextAlign = C1FlexGridEx.ToTextAlign(cell.Align);
		}
		styleNew.Margins = new Margins(cell.Margin, 0, 0, 0);
	}

	private int GetTitleTotalHeight()
	{
		int num = 0;
		for (int i = 0; i < _gridTitle.Rows.Count; i++)
		{
			num += _gridTitle.Rows[i].HeightDisplay;
		}
		return num;
	}

	private void SelectTile(int index)
	{
		Tile tile = _tile.Groups[index].Tiles[0];
		tile.PerformClick();
		_tile.Refresh();
		_tile.ScrollToTile(tile, immediate: true);
	}
}
