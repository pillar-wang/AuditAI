﻿﻿﻿﻿﻿using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using C1.C1Preview;
using C1.C1Preview.Export;
using C1.Win.C1Preview;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class TicketPrinter : IDisposable
{
	protected class PagePrintSetting
	{
		public double HScale = 1.0;
		public double VScale = 1.0;
		public double FinallyWidth;
		public double DataTableFinallyHeight;
		public double TitleTableFinallyHeight;
		public double FooterTableFinallyHeight;
		public double PageDataAreaMaxWidth;
		public double PageDataAreaMaxHeight;
	}

	private readonly C1PreviewPane _pvw;
	private TicketInputTableVM _vm;
	private readonly C1PrintDocumentEx _pd;
	private readonly PdfExportProvider _pdfExporterProvider;
	private readonly RtfHelper _rtfHelper = new RtfHelper();
	private readonly Graphics ScreenGraphics = Graphics.FromHwnd(IntPtr.Zero);
	protected PagePrintSetting _currentPrintSetting;

	public C1PreviewPane View => _pvw;
	public TicketTable Ticket { get; set; }
	public PageSetup PageSetup => Ticket.PageSetup;

	static TicketPrinter()
	{
		C1PrintDocument.MeasurementDevice = MeasurementDeviceEnum.Screen;
	}

	public TicketPrinter()
	{
		_pvw = new C1PreviewPane
		{
			Dock = DockStyle.Fill
		};
		_pd = new C1PrintDocumentEx
		{
			CreationDevice = MeasurementDeviceEnum.Screen,
			AllowNonReflowableDocs = true,
			UseGdiPlusTextRendering = true
		};
		_pvw.Document = _pd;
		_pd.PageConfigure += _pd_PageConfigure;
		_pdfExporterProvider = new PdfExportProvider();
	}

	public void SetVm()
	{
		_vm = Program.MainForm.TicketInputEditor._vm;
	}

	public void SetVm(TicketTable ticket, TicketRecord ticketRecord)
	{
		_vm = new TicketInputTableVM(ticket, ticketRecord);
		_vm.CalculateTicket();
	}

	private void _pd_PageConfigure(C1PrintDocument sender, PageConfigureEventArgs e)
	{
		int pageNo = PageSetup.StartPageNo + e.Page.PageNo - 1;
		int pageCount = e.Page.PageCount;
		PageLayout pageLayout2 = (e.PageLayout = new PageLayout());
		pageLayout2.PageHeader = MakePageHeader(pageNo, pageCount);
		pageLayout2.PageFooter = MakePageFooter(pageNo, pageCount);
	}

	private RenderObject MakePageHeader(int pageNo, int pageCount)
	{
		RenderTable renderTable = new RenderTable(3, 3);
		RenderTable renderObject = MakeHeader(pageNo, pageCount);
		RenderTable renderObject2 = MakeTicketTitleOrFooter(_vm.Title);
		renderTable.Cells[0, 0].RenderObject = renderObject;
		renderTable.Cells[0, 0].SpanCols = 3;
		double num = PageSetup.TopMargin - PageSetup.HeaderMargin;
		renderTable.Rows[0].Height = new Unit((num > 0.0) ? num : 0.0, UnitTypeEnum.Mm);
		renderTable.Cells[1, 1].RenderObject = renderObject2;
		renderTable.Cols[0].Width = new Unit((_currentPrintSetting.PageDataAreaMaxWidth - _currentPrintSetting.FinallyWidth) * 0.5, UnitTypeEnum.Pixel);
		renderTable.Cols[1].Width = new Unit(_currentPrintSetting.FinallyWidth, UnitTypeEnum.Pixel);
		renderTable.Cols[2].Width = Unit.Auto;
		renderTable.Rows[1].Height = new Unit(_currentPrintSetting.TitleTableFinallyHeight, UnitTypeEnum.Pixel);
		renderTable.Rows[2].Height = new Unit(0.0, UnitTypeEnum.Pixel);
		return renderTable;
	}

	private RenderObject MakePageFooter(int pageNo, int pageCount)
	{
		RenderTable renderTable = MakeFooter(pageNo, pageCount);
		double num = PageSetup.BottomMargin - PageSetup.FooterMargin;
		renderTable.Height = new Unit((num > 0.0) ? num : 0.0, UnitTypeEnum.Mm);
		return renderTable;
	}

	private RenderTable MakeHeader(int pageNo, int pageCount)
	{
		RenderTable renderTable = new RenderTable(1, 3);
		RenderRichText renderRichText = new RenderRichText(ProcessRtf(PageSetup.PageHeader.LeftValue, pageNo, pageCount));
		renderRichText.Style.TextAlignVert = AlignVertEnum.Top;
		renderTable.Cells[0, 0].RenderObject = renderRichText;
		renderRichText = new RenderRichText(ProcessRtf(PageSetup.PageHeader.CenterValue, pageNo, pageCount));
		renderRichText.Style.TextAlignVert = AlignVertEnum.Top;
		renderTable.Cells[0, 1].RenderObject = renderRichText;
		renderRichText = new RenderRichText(ProcessRtf(PageSetup.PageHeader.RightValue, pageNo, pageCount));
		renderRichText.Style.TextAlignVert = AlignVertEnum.Top;
		renderTable.Cells[0, 2].RenderObject = renderRichText;
		return renderTable;
	}

	private RenderTable MakeFooter(int pageNo, int pageCount)
	{
		RenderTable renderTable = new RenderTable(1, 3);
		renderTable.SplitVertBehavior = SplitBehaviorEnum.Never;
		RenderRichText renderRichText = new RenderRichText(ProcessRtf(PageSetup.PageFooter.LeftValue, pageNo, pageCount));
		renderRichText.Style.TextAlignVert = AlignVertEnum.Top;
		renderTable.Cells[0, 0].RenderObject = renderRichText;
		renderRichText = new RenderRichText(ProcessRtf(PageSetup.PageFooter.CenterValue, pageNo, pageCount));
		renderRichText.Style.TextAlignVert = AlignVertEnum.Top;
		renderTable.Cells[0, 1].RenderObject = renderRichText;
		renderRichText = new RenderRichText(ProcessRtf(PageSetup.PageFooter.RightValue, pageNo, pageCount));
		renderRichText.Style.TextAlignVert = AlignVertEnum.Top;
		renderTable.Cells[0, 2].RenderObject = renderRichText;
		return renderTable;
	}

	private RenderTable MakeTicketTitleOrFooter(TicketInputTitleFooterVM setting)
	{
		if (setting.Rows.Count == 0)
		{
			return new RenderTable(0, 0);
		}
		int count = setting.Rows.Count;
		int count2 = setting.Columns.Count;
		RenderTable renderTable = new RenderTable(count, count2);
		for (int i = 0; i < count; i++)
		{
			double value = setting.Rows[i].TicketRow.Height;
			renderTable.Rows[i].Height = new Unit(value, UnitTypeEnum.Pixel);
			int j;
			for (j = 0; j < count2; j++)
			{
				TicketMerge ticketMerge = setting.Merges.FirstOrDefault((TicketMerge m) => m.Contains(i, j));
				TableCell tableCell = renderTable.Cells[i, j];
				TicketInputCellVM cellVM = setting.GetCellVM(i, j);
				tableCell.Text = cellVM.GetDisplayValue();
				tableCell.Style.Font = cellVM.Font;
				tableCell.Style.BackColor = (PageSetup.OneColor ? Color.Transparent : cellVM.BackColor);
				tableCell.Style.TextColor = (PageSetup.OneColor ? Color.Black : cellVM.ForeColor);
				tableCell.Style.TextAlignHorz = C1PrintDocumentEx.ToAlignHorz(cellVM.Align);
				tableCell.Style.TextAlignVert = C1PrintDocumentEx.ToAlignVert(cellVM.Align);
				if (tableCell.Style.TextAlignHorz == AlignHorzEnum.Right)
				{
					tableCell.CellStyle.Spacing.Right = new Unit(0.5, UnitTypeEnum.Mm);
				}
				if (tableCell.Style.TextAlignHorz == AlignHorzEnum.Left)
				{
					tableCell.CellStyle.Spacing.Left = new Unit(0.5, UnitTypeEnum.Mm);
				}
				if (cellVM.Indent != 0)
				{
					tableCell.CellStyle.Spacing.Left = new Unit(cellVM.Indent, UnitTypeEnum.Pixel);
				}
				TicketBorder top = cellVM.GetTop(isFirstDataRow: true);
				TicketBorder bottom = cellVM.GetBottom(isLastDataRow: true);
				if (i == 0 || (double)top.Width > renderTable.Cells[i - 1, j].Style.GridLines.Bottom.Width.Value)
				{
					tableCell.Style.GridLines.Top = new LineDef(new Unit(top.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				if (ticketMerge != null && ticketMerge.LeftColumn == j)
				{
					tableCell.Style.GridLines.Right = new LineDef(new Unit(setting.GetCellVM(i, ticketMerge.RightColumn).Right.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				else
				{
					tableCell.Style.GridLines.Right = new LineDef(new Unit(cellVM.Right.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				if (ticketMerge != null && ticketMerge.TopRow == i)
				{
					tableCell.Style.GridLines.Bottom = new LineDef(new Unit(setting.GetCellVM(ticketMerge.BottomRow, j).GetBottom(isLastDataRow: false).Width, UnitTypeEnum.Pixel), Color.Black);
				}
				else
				{
					tableCell.Style.GridLines.Bottom = new LineDef(new Unit(bottom.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				if (j == 0 || (double)cellVM.Left.Width > renderTable.Cells[i, j - 1].Style.GridLines.Right.Width.Value)
				{
					tableCell.Style.GridLines.Left = new LineDef(new Unit(cellVM.Left.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				if (cellVM.IsField && cellVM.Column == null)
				{
					tableCell.Text = "(无效列引用)";
				}
			}
		}
		for (int k = 0; k < count2; k++)
		{
			renderTable.Cols[k].Width = new Unit(setting.Columns[k].TicketColumn.Width, UnitTypeEnum.Pixel);
		}
		foreach (TicketMerge merge in setting.Merges)
		{
			TableCell tableCell2 = renderTable.Cells[merge.TopRow, merge.LeftColumn];
			tableCell2.SpanRows = merge.BottomRow - merge.TopRow + 1;
			tableCell2.SpanCols = merge.RightColumn - merge.LeftColumn + 1;
		}
		double num = Math.Min(_currentPrintSetting.HScale, _currentPrintSetting.VScale);
		for (int l = 0; l < count; l++)
		{
			for (int n = 0; n < count2; n++)
			{
				TableCell tableCell3 = renderTable.Cells[l, n];
				tableCell3.Style.FontSize = (float)((double)tableCell3.Style.FontSize * num);
			}
		}
		renderTable.Style.FlowAlign = FlowAlignEnum.Center;
		return renderTable;
	}

	private string ProcessRtf(string rtf, int pageNo, int pageCount)
	{
		if (string.IsNullOrEmpty(rtf))
		{
			return rtf;
		}
		_rtfHelper.Load(rtf);
		if (PageSetup.OneColor)
		{
			_rtfHelper.Monochrome();
		}
		return _rtfHelper.ReplacePageNo(pageNo).ReplacePageCount(pageCount).ReplaceVariables(Project.Current.DataReferenceManager, new DataReferenceEvaluationContext
		{
			CurrentTreeNode = Ticket.Table.TreeNode,
			Project = Project.Current
		})
			.Save();
	}

	private RenderObject CreateWaterMark(bool isHorizontalDir)
	{
		string platformName = Program.MainForm.CurrentEdition.PlatformName;
		string empty = string.Empty;
		empty = ((!SoftwareLicenseManager.IsPayByProject()) ? "非正式版用户" : ((!SoftwareLicenseManager.IsFreeTeam()) ? ("体验" + StringConstBase.Current.Project) : "非正式版用户"));
		int num = 6;
		int num2 = 4;
		if (isHorizontalDir)
		{
			num = 4;
			num2 = 6;
		}
		RenderTable renderTable = new RenderTable();
		for (int i = 0; i < num; i++)
		{
			TableRow tableRow = renderTable.Rows[i];
			for (int j = 0; j < num2; j++)
			{
				RenderText renderText = null;
				renderText = ((j % 2 != 0) ? new RenderText(empty) : new RenderText(platformName));
				TableCell tableCell = renderTable.Cells[i, j];
				tableCell.RenderObject = renderText;
				tableCell.Style.FontName = "微软雅黑";
				tableCell.Style.FontSize = 14f;
				tableCell.Style.TextColor = Color.LightGray;
				tableCell.Style.TextAngle = 45f;
				tableCell.Style.TextAlignHorz = AlignHorzEnum.Center;
				tableCell.Style.TextAlignVert = AlignVertEnum.Center;
			}
		}
		renderTable.ZOrder = 1000;
		return renderTable;
	}

	private void ResetCurrentPrintSetting()
	{
		_currentPrintSetting = new PagePrintSetting();
		double num = Ticket.Columns.Sum((TicketColumn c) => c.Width);
		double num2 = _vm.GetAllRowsTotalHeight();
		double num3 = _vm.Title.Rows.Sum((TicketInputRowVM r) => r.TicketRow.Height);
		double num4 = _vm.Footer.Rows.Sum((TicketInputRowVM r) => r.TicketRow.Height);
		double num5 = _pd.UnitSubtract(_pd.PageLayout.PageSettings.Width, new Unit(PageSetup.LeftMargin + PageSetup.RightMargin, UnitTypeEnum.Mm)).ConvertUnit(UnitTypeEnum.Pixel, ScreenGraphics.DpiX);
		double num6 = _pd.UnitSubtract(_pd.PageLayout.PageSettings.Height, new Unit(Math.Max(PageSetup.HeaderMargin, PageSetup.TopMargin) + Math.Max(PageSetup.FooterMargin, PageSetup.BottomMargin), UnitTypeEnum.Mm)).ConvertUnit(UnitTypeEnum.Pixel, ScreenGraphics.DpiY);
		double num7 = num3 + num2 + num4;
		_currentPrintSetting.PageDataAreaMaxWidth = num5;
		_currentPrintSetting.PageDataAreaMaxHeight = num6;
		if (PageSetup.FitPageWidth || PageSetup.FitPageHeight)
		{
			double val = 1.0;
			double val2 = 1.0;
			if (num <= num5)
			{
				val = 1.0;
			}
			else if (PageSetup.FitPageWidth)
			{
				val = num5 / num;
			}
			if (num7 <= num6)
			{
				val2 = 1.0;
			}
			else if (PageSetup.FitPageHeight)
			{
				val2 = num6 / num7;
			}
			double num8 = Math.Min(val, val2);
			_currentPrintSetting.HScale = num8;
			_currentPrintSetting.VScale = num8;
			_currentPrintSetting.FinallyWidth = num * num8;
			_currentPrintSetting.TitleTableFinallyHeight = num3 * num8;
			_currentPrintSetting.DataTableFinallyHeight = num2 * num8;
			_currentPrintSetting.FooterTableFinallyHeight = num4 * num8;
		}
		else
		{
			_currentPrintSetting.HScale = Math.Max(PageSetup.HorizontalZoom, 0.0010000000474974513);
			_currentPrintSetting.VScale = Math.Max(PageSetup.VerticalZoom, 0.0010000000474974513);
			_currentPrintSetting.FinallyWidth = num * _currentPrintSetting.HScale;
			_currentPrintSetting.TitleTableFinallyHeight = num3 * _currentPrintSetting.VScale;
			_currentPrintSetting.DataTableFinallyHeight = num2 * _currentPrintSetting.VScale;
			_currentPrintSetting.FooterTableFinallyHeight = num4 * _currentPrintSetting.VScale;
		}
		double num9 = num5 - _currentPrintSetting.FinallyWidth;
		if (num9 == 0.0)
		{
			_currentPrintSetting.FinallyWidth -= 3.0;
		}
		_currentPrintSetting.FinallyWidth = (int)_currentPrintSetting.FinallyWidth;
		_currentPrintSetting.FinallyWidth = Math.Max(0.0, _currentPrintSetting.FinallyWidth);
	}

	public void Populate(bool isUpdateAppCommandStatus = true)
	{
		if (isUpdateAppCommandStatus)
		{
			AppCommands.Paper.SelectPaper(PageSetup.PaperKind);
			AppCommands.WidthScale.Value = (decimal)PageSetup.HorizontalZoom;
			AppCommands.HeightScale.Value = (decimal)PageSetup.VerticalZoom;
			AppCommands.MarginTop.Value = (decimal)PageSetup.TopMargin;
			AppCommands.MarginBottom.Value = (decimal)PageSetup.BottomMargin;
			AppCommands.MarginLeft.Value = (decimal)PageSetup.LeftMargin;
			AppCommands.MarginRight.Value = (decimal)PageSetup.RightMargin;
			AppCommands.HeaderMargin.Enabled = true;
			AppCommands.HeaderMargin.Value = (decimal)PageSetup.HeaderMargin;
			AppCommands.FooterMargin.Enabled = true;
			AppCommands.FooterMargin.Value = (decimal)PageSetup.FooterMargin;
			AppCommands.HeaderLeft.Text = _rtfHelper.Load(PageSetup.PageHeader.LeftValue).GetPlainText();
			AppCommands.HeaderCenter.Text = _rtfHelper.Load(PageSetup.PageHeader.CenterValue).GetPlainText();
			AppCommands.HeaderRight.Text = _rtfHelper.Load(PageSetup.PageHeader.RightValue).GetPlainText();
			AppCommands.FooterLeft.Text = _rtfHelper.Load(PageSetup.PageFooter.LeftValue).GetPlainText();
			AppCommands.FooterCenter.Text = _rtfHelper.Load(PageSetup.PageFooter.CenterValue).GetPlainText();
			AppCommands.FooterRight.Text = _rtfHelper.Load(PageSetup.PageFooter.RightValue).GetPlainText();
			AppCommands.Monochrome.IsChecked = PageSetup.OneColor;
			AppCommands.ScalePageWidth.IsChecked = PageSetup.FitPageWidth;
			AppCommands.ScalePageHeight.IsChecked = PageSetup.FitPageHeight;
		}
		_pd.PageLayout.PageSettings.PaperKind = PageSetup.PaperKind;
		_pd.PageLayout.PageSettings.Landscape = PageSetup.Direction == Direction.Horizontal;
		_pd.PageLayout.PageSettings.UsePrinterPaperSize = false;
		if (PageSetup.PaperKind == PaperKind.Custom)
		{
			_pd.PageLayout.PageSettings.SetPaperSizes(new Unit(PageSetup.PaperWidth, UnitTypeEnum.Mm), new Unit(PageSetup.PaperHeight, UnitTypeEnum.Mm));
		}
		_pd.PageLayout.PageSettings.LeftMargin = new Unit(PageSetup.LeftMargin, UnitTypeEnum.Mm);
		_pd.PageLayout.PageSettings.TopMargin = new Unit(PageSetup.HeaderMargin, UnitTypeEnum.Mm);
		_pd.PageLayout.PageSettings.RightMargin = new Unit(PageSetup.RightMargin, UnitTypeEnum.Mm);
		_pd.PageLayout.PageSettings.BottomMargin = new Unit(PageSetup.FooterMargin, UnitTypeEnum.Mm);
		_pd.PageLayout.PageSettings.Color = !PageSetup.OneColor;
		ResetCurrentPrintSetting();
		if (SoftwareLicenseManager.IsNeedPrintWaterMark())
		{
			_pd.PageLayout.Watermark = CreateWaterMark(PageSetup.Direction == Direction.Horizontal);
		}
		else
		{
			_pd.PageLayout.Watermark = null;
		}
		_pd.StartDoc();
		int num = 0;
		int rowsCount = _vm.GetRowsCount();
		RenderTable renderTable = new RenderTable(num + rowsCount, Ticket.Columns.Count);
		renderTable.SplitHorzBehavior = SplitBehaviorEnum.SplitIfNeeded;
		for (int k = 0; k < Ticket.Columns.Count; k++)
		{
			TableCol tableCol = renderTable.Cols[k];
			int num2 = ((!Ticket.Columns[k].IsHiddenColumn) ? Ticket.Columns[k].Width : 0);
			tableCol.Width = new Unit(num2, UnitTypeEnum.Pixel);
		}
		for (int i = 0; i < rowsCount; i++)
		{
			TableRow tableRow = renderTable.Rows[num + i];
			tableRow.Height = new Unit(_vm.GetRowHeight(i), UnitTypeEnum.Pixel);
			int j;
			for (j = 0; j < Ticket.Columns.Count; j++)
			{
				TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(i, j));
				TableCell tableCell = renderTable.Cells[num + i, j];
				TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
				tableCell.Text = (Ticket.Columns[j].IsHiddenColumn ? string.Empty : cellVM.GetDisplayValue());
				tableCell.Style.Font = cellVM.Font;
				tableCell.Style.BackColor = (PageSetup.OneColor ? Color.Transparent : cellVM.BackColor);
				tableCell.Style.TextColor = (PageSetup.OneColor ? Color.Black : cellVM.ForeColor);
				tableCell.Style.TextAlignHorz = C1PrintDocumentEx.ToAlignHorz(cellVM.Align);
				tableCell.Style.TextAlignVert = C1PrintDocumentEx.ToAlignVert(cellVM.Align);
				if (tableCell.Style.TextAlignHorz == AlignHorzEnum.Right)
				{
					tableCell.CellStyle.Spacing.Right = new Unit(0.5, UnitTypeEnum.Mm);
				}
				if (tableCell.Style.TextAlignHorz == AlignHorzEnum.Left)
				{
					tableCell.CellStyle.Spacing.Left = new Unit(0.5, UnitTypeEnum.Mm);
				}
				if (cellVM.Indent != 0)
				{
					tableCell.CellStyle.Spacing.Left = new Unit(cellVM.Indent, UnitTypeEnum.Pixel);
				}
				TicketBorder cellTopBorder = _vm.GetCellTopBorder(cellVM, i, j);
				TicketBorder cellBottomBorder = _vm.GetCellBottomBorder(cellVM, i, j);
				if (i == 0 || (double)cellTopBorder.Width > renderTable.Cells[num + i - 1, j].Style.GridLines.Bottom.Width.Value)
				{
					tableCell.Style.GridLines.Top = new LineDef(new Unit(cellTopBorder.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				if (ticketMerge != null && ticketMerge.LeftColumn == j)
				{
					tableCell.Style.GridLines.Right = new LineDef(new Unit(_vm.GetCellVM(i, ticketMerge.RightColumn).Right.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				else
				{
					tableCell.Style.GridLines.Right = new LineDef(new Unit(cellVM.Right.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				if (ticketMerge != null && ticketMerge.TopRow == i)
				{
					tableCell.Style.GridLines.Bottom = new LineDef(new Unit(_vm.GetCellVM(ticketMerge.BottomRow, j).GetBottom(isLastDataRow: false).Width, UnitTypeEnum.Pixel), Color.Black);
				}
				else
				{
					tableCell.Style.GridLines.Bottom = new LineDef(new Unit(cellBottomBorder.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				if (j == 0 || (double)cellVM.Left.Width > renderTable.Cells[num + i, j - 1].Style.GridLines.Right.Width.Value)
				{
					tableCell.Style.GridLines.Left = new LineDef(new Unit(cellVM.Left.Width, UnitTypeEnum.Pixel), Color.Black);
				}
				if (cellVM.IsField && cellVM.Column == null)
				{
					tableCell.Text = "(无效列引用)";
				}
			}
		}
		foreach (TicketMerge merge in _vm.Merges)
		{
			TableCell tableCell2 = renderTable.Cells[num + merge.TopRow, merge.LeftColumn];
			tableCell2.SpanRows = merge.BottomRow - merge.TopRow + 1;
			tableCell2.SpanCols = merge.RightColumn - merge.LeftColumn + 1;
		}
		double num3 = Math.Min(_currentPrintSetting.HScale, _currentPrintSetting.VScale);
		for (int l = 0; l < rowsCount; l++)
		{
			for (int n = 0; n < Ticket.Columns.Count; n++)
			{
				TableCell tableCell3 = renderTable.Cells[num + l, n];
				tableCell3.Style.FontSize = (float)((double)tableCell3.Style.FontSize * num3);
			}
		}
		renderTable.Style.FlowAlign = FlowAlignEnum.Center;
		renderTable.Width = new Unit(_currentPrintSetting.FinallyWidth, UnitTypeEnum.Pixel);
		renderTable.Height = new Unit(_currentPrintSetting.DataTableFinallyHeight, UnitTypeEnum.Pixel);
		int num4 = Math.Min(Ticket.ColumnHeaderRowsCount + num, rowsCount);
		if (num4 > 0)
		{
			renderTable.RowGroups[0, num4].PageHeader = true;
		}
		_pd.RenderBlock(renderTable);
		if (_vm.Footer.Rows.Count > 0)
		{
			RenderTable renderTable2 = MakeTicketTitleOrFooter(_vm.Footer);
			renderTable2.Width = new Unit(_currentPrintSetting.FinallyWidth, UnitTypeEnum.Pixel);
			renderTable2.Height = new Unit(_currentPrintSetting.FooterTableFinallyHeight, UnitTypeEnum.Pixel);
			renderTable2.SplitHorzBehavior = SplitBehaviorEnum.SplitIfNeeded;
			_pd.AdvanceBlockFlow(new Unit(-1.0, UnitTypeEnum.Pixel));
			_pd.RenderBlock(renderTable2);
		}
		_pd.EndDoc();
	}

	public void SetPaperKind(PaperKind kind)
	{
		PageSetup.PaperKind = kind;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetPaperCustom(double width, double height)
	{
		PageSetup.PaperKind = PaperKind.Custom;
		PageSetup.PaperWidth = Math.Max(width, PageSetup.LeftMargin + PageSetup.RightMargin + 50.0);
		PageSetup.PaperHeight = Math.Max(height, PageSetup.TopMargin + PageSetup.BottomMargin + 50.0);
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void Portrait()
	{
		PageSetup.Direction = Direction.Vertical;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void Landscape()
	{
		PageSetup.Direction = Direction.Horizontal;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetMarginLeft(double mm)
	{
		PageSetup.LeftMargin = mm;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetMarginTop(double mm)
	{
		PageSetup.TopMargin = mm;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetMarginRight(double mm)
	{
		PageSetup.RightMargin = mm;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetMarginBottom(double mm)
	{
		PageSetup.BottomMargin = mm;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetStartPage(int p)
	{
		PageSetup.StartPageNo = p;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetOneColor(bool b)
	{
		PageSetup.OneColor = b;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void Print()
	{
		PrinterSettings printerSettings = new PrinterSettings();
		printerSettings.DefaultPageSettings.PaperSize = _pd.PageLayout.PageSettings.ToPageSettings().PaperSize;
		_pd.PrintDialog(printerSettings, null, showProgress: false);
	}

	public void Print(PrinterSettings printerSettings)
	{
		_pd.Print(printerSettings, showProgress: false);
	}

	public void ExportPdf()
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "PDF|*.pdf",
			DefaultExt = "pdf"
		};
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			using (FileStream stream = File.OpenWrite(saveFileDialog.FileName))
			{
				_pd.Export(stream, _pdfExporterProvider, showProgress: true);
			}
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, Ticket.GetLevelString() + " 导出成功");
		}
	}

	public void SetFitWidth(bool value)
	{
		PageSetup.FitPageWidth = value;
		Populate();
	}

	public void SetFitHeight(bool value)
	{
		PageSetup.FitPageHeight = value;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetWidthScale(double value)
	{
		PageSetup.HorizontalZoom = value;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetHeightScale(double value)
	{
		PageSetup.VerticalZoom = value;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetHeaderMargin(double value)
	{
		PageSetup.HeaderMargin = value;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void SetFooterMargin(double value)
	{
		PageSetup.FooterMargin = value;
		Ticket.Table.TagTicketDirty();
		Populate();
	}

	public void Dispose()
	{
		ScreenGraphics.Dispose();
	}
}
