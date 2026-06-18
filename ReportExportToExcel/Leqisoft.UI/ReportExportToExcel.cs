﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using C1.C1Excel;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI;

public class ReportExportToExcel
{
	private C1XLBook xlBook;

	private XLSheet xlSheet;

	public PageSetup PageSetup { get; set; }

	public Leqisoft.Model.Table Table { get; set; }

	public PageSetupWaterMark WaterMarkPageSetup { get; set; }

	public ReportExportToExcel()
	{
		xlBook = new C1XLBook();
		xlSheet = xlBook.Sheets[0];
	}

	private string standardSheetName(string fileName)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char c in invalidFileNameChars)
		{
			fileName = fileName.Replace(c.ToString(), "");
		}
		fileName = fileName.Replace("、", "");
		if (fileName.Length > 30)
		{
			fileName = fileName.Substring(0, 30);
		}
		return fileName;
	}

	public void SaveValue(string path, ExcelContex contex)
	{
		xlBook = new C1XLBook();
		xlSheet = xlBook.Sheets[0];
		string text = standardSheetName(Table.TreeNode.Number + " " + Table.TreeNode.Name);
		if (xlSheet.Name != text)
		{
			xlSheet.Name = nonRepetitiveSheetName(xlBook, text);
		}
		Build(xlSheet, out var rowOffSet, out var columnOffSet);
		contex.Add(xlSheet, new BookInfo
		{
			SavePath = path,
			SheetName = xlSheet.Name,
			Table = Table,
			ColumnOffSet = columnOffSet,
			RowOffSet = rowOffSet,
			Exporter = this
		});
		contex.dataRowStartIndex = rowOffSet;
		contex.dataRowsCount = Table.Rows.Count;
	}

	public void SaveValue(string path, C1XLBook xLBook, XLSheet xLSheet, ExcelContex contex)
	{
		xlBook = xLBook;
		xlSheet = xLSheet;
		string text = standardSheetName(Table.TreeNode.Number + " " + Table.TreeNode.Name);
		if (xlSheet.Name != text)
		{
			xlSheet.Name = nonRepetitiveSheetName(xlBook, text);
		}
		Build(xlSheet, out var rowOffSet, out var columnOffSet);
		contex.Add(xlSheet, new BookInfo
		{
			SavePath = path,
			SheetName = xlSheet.Name,
			Table = Table,
			ColumnOffSet = columnOffSet,
			RowOffSet = rowOffSet,
			Exporter = this
		});
		contex.dataRowStartIndex = rowOffSet;
		contex.dataRowsCount = Table.Rows.Count;
	}

	public void SetFormula(ExcelContex context)
	{
		try
		{
			CannotExportExcelContext cannotExportExcelContext = new CannotExportExcelContext();
			foreach (Leqisoft.Model.Row row in Table.Rows)
			{
				if (row.Role != 0)
				{
					cannotExportExcelContext.IsExistUnNormalRow = true;
					break;
				}
			}
			cannotExportExcelContext.CurrentTableId = Table.Id.Value;
			foreach (Leqisoft.Model.Column column in Table.Columns)
			{
				if (!column.HasFormula)
				{
					continue;
				}
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(column.Formula);
				if (formulaEvaluator.CannotExportExcel(cannotExportExcelContext))
				{
					continue;
				}
				BookInfo bookInfo = context[xlSheet];
				foreach (Leqisoft.Model.Row row2 in Table.Rows)
				{
					try
					{
						Leqisoft.Model.Cell cell = Table[row2.Index, column.Index];
						if (cell.IsAllowUseColumnFormulaResultUpdateCellValue)
						{
							XLSheet xLSheet = xlSheet;
							XLCell cell2 = xLSheet.GetCell(row2.Index + bookInfo.RowOffSet, column.Index + bookInfo.ColumnOffSet);
							string formula = formulaEvaluator.ExcelExport(new ExcelExporterContext
							{
								CurrentTable = Table,
								CellOffset = (Leqisoft.Model.Cell c) => Tuple.Create(context[c.Row.Table].RowOffSet, context[c.Row.Table].ColumnOffSet),
								CurrentRowIndex = row2.Index,
								Resolver = new FormulaReferenceModelResolver(Table.Project),
								TablePathMapper = (Leqisoft.Model.Table t) => context[t].SheetName,
								CurrentCell = cell,
								DataRowStartIndex = context.dataRowStartIndex,
								DataRowsCount = context.dataRowsCount
							});
							cell2.Formula = formula;
						}
					}
					catch
					{
					}
				}
			}
			foreach (Leqisoft.Model.Row row3 in Table.Rows)
			{
				if (row3.Role == RowRole.Fixed || row3.Role == RowRole.Header)
				{
					continue;
				}
				foreach (Leqisoft.Model.Column column2 in Table.Columns)
				{
					try
					{
						Leqisoft.Model.Cell cell3 = Table[row3.Index, column2.Index];
						if (!cell3.HasFormula)
						{
							continue;
						}
						FormulaEvaluator formulaEvaluator2 = new FormulaEvaluator(cell3.Formula);
						if (!formulaEvaluator2.CannotExportExcel(cannotExportExcelContext))
						{
							BookInfo bookInfo2 = context[xlSheet];
							XLSheet xLSheet2 = xlSheet;
							XLCell cell4 = xLSheet2.GetCell(row3.Index + bookInfo2.RowOffSet, column2.Index + bookInfo2.ColumnOffSet);
							string formula2 = formulaEvaluator2.ExcelExport(new ExcelExporterContext
							{
								CurrentTable = Table,
								CellOffset = (Leqisoft.Model.Cell c) => Tuple.Create(context[c.Row.Table].RowOffSet, context[c.Column.Table].ColumnOffSet),
								CurrentRowIndex = row3.Index,
								Resolver = new FormulaReferenceModelResolver(Table.Project),
								TablePathMapper = (Leqisoft.Model.Table t) => context[t].SheetName,
								CurrentCell = cell3,
								DataRowStartIndex = context.dataRowStartIndex,
								DataRowsCount = context.dataRowsCount
							});
							cell4.Formula = formula2;
						}
					}
					catch (FormulaNotApplicableException)
					{
					}
					catch (Exception)
					{
					}
				}
			}
		}
		catch
		{
		}
	}

	public void Save(ExcelContex contex)
	{
		string savePath = contex[xlSheet].SavePath;
		using FileStream stream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		xlBook.Save(stream, FileFormat.OpenXml);
	}

	public static void BatchExportToFile(string filename, List<Tuple<Leqisoft.Model.Table, PageSetup>> batchs)
	{
		List<ReportExportToExcel> list = new List<ReportExportToExcel>();
		ExcelContex excelContex = new ExcelContex();
		C1XLBook c1XLBook = new C1XLBook();
		for (int i = 0; i < batchs.Count; i++)
		{
			try
			{
				Tuple<Leqisoft.Model.Table, PageSetup> tuple = batchs[i];
				tuple.Item1.CalculateRecursive();
				ReportExportToExcel reportExportToExcel = new ReportExportToExcel
				{
					Table = tuple.Item1,
					PageSetup = tuple.Item2
				};
				XLSheet xLSheet = null;
				if (i >= c1XLBook.Sheets.Count)
				{
					xLSheet = c1XLBook.Sheets.Add();
				}
				xLSheet = c1XLBook.Sheets[i];
				reportExportToExcel.SaveValue(filename, c1XLBook, xLSheet, excelContex);
				list.Add(reportExportToExcel);
			}
			catch (Exception)
			{
			}
		}
		foreach (ReportExportToExcel item in list)
		{
			try
			{
				item.SetFormula(excelContex);
			}
			catch (Exception)
			{
			}
		}
		if (list.Count > 0)
		{
			list.First().Save(excelContex);
		}
	}

	private static string nonRepetitiveSheetName(C1XLBook xLBook, string sheetName)
	{
		string arg = sheetName;
		int num = 1;
		while (xLBook.Sheets.IndexOf(sheetName) != -1)
		{
			sheetName = $"{arg}({num++})";
		}
		return sheetName;
	}

	private void Build(XLSheet sheet, out int rowOffSet, out int columnOffSet)
	{
		int titleLayers = 0;
		int captionLayers = 0;
		bool ifSetNote = false;
		setHeader();
		setFooter();
		setMainTitle();
		setSubTitle();
		setCaption();
		rowOffSet = titleLayers + captionLayers;
		columnOffSet = 0;
		setContent();
		ApplySubTotal(sheet, titleLayers + captionLayers);
		setBottomMark();
		SetC1ExcleStyle(Table.BorderStyle ?? TableBorderStyles.Grid, sheet, captionLayers, ifSetNote);
		static object ConvertValue(Leqisoft.Model.Cell cell)
		{
			if (cell == null)
			{
				return string.Empty;
			}
			object value = cell.Value;
			if (value is bool)
			{
				return cell.GetDisplayValue();
			}
			if (value is TimeSpan timeSpan)
			{
				return timeSpan.TotalDays;
			}
			if (value is DateYearMonth)
			{
				return cell.GetDisplayValue();
			}
			if (value is DateTime dateTime)
			{
				if (dateTime.Year < 1900)
				{
					return cell.GetDisplayValue();
				}
				return cell.Value;
			}
			return cell.Value;
		}
		static void convertAlign(CellTextAlign cellTextAlign, out XLAlignHorzEnum horizTextAlign, out XLAlignVertEnum vertTextAlign)
		{
			switch (cellTextAlign)
			{
			case CellTextAlign.TopLeft:
				vertTextAlign = XLAlignVertEnum.Top;
				horizTextAlign = XLAlignHorzEnum.Left;
				break;
			case CellTextAlign.TopCenter:
				vertTextAlign = XLAlignVertEnum.Top;
				horizTextAlign = XLAlignHorzEnum.Center;
				break;
			case CellTextAlign.TopRight:
				vertTextAlign = XLAlignVertEnum.Top;
				horizTextAlign = XLAlignHorzEnum.Right;
				break;
			case CellTextAlign.MiddleLeft:
				vertTextAlign = XLAlignVertEnum.Center;
				horizTextAlign = XLAlignHorzEnum.Left;
				break;
			case CellTextAlign.MiddleCenter:
				vertTextAlign = XLAlignVertEnum.Center;
				horizTextAlign = XLAlignHorzEnum.Center;
				break;
			case CellTextAlign.MiddleRight:
				vertTextAlign = XLAlignVertEnum.Center;
				horizTextAlign = XLAlignHorzEnum.Right;
				break;
			case CellTextAlign.BottomLeft:
				vertTextAlign = XLAlignVertEnum.Bottom;
				horizTextAlign = XLAlignHorzEnum.Left;
				break;
			case CellTextAlign.BottomCenter:
				vertTextAlign = XLAlignVertEnum.Bottom;
				horizTextAlign = XLAlignHorzEnum.Center;
				break;
			case CellTextAlign.BottomRight:
				vertTextAlign = XLAlignVertEnum.Bottom;
				horizTextAlign = XLAlignHorzEnum.Right;
				break;
			default:
				vertTextAlign = XLAlignVertEnum.Center;
				horizTextAlign = XLAlignHorzEnum.Left;
				break;
			}
		}
		void customMerge(int r1, int c1, int r2, int c2)
		{
			try
			{
				XLCellRange cr = new XLCellRange(r1, r2, c1, c2);
				sheet.MergedCells.Add(cr);
			}
			catch
			{
			}
		}
		void setBottomMark()
		{
			TableFoot foot = Table.Foot;
			int[] array = new int[foot.Columns.Count + 1];
			if (foot.Columns.Count < Table.Columns.Count)
			{
				int result;
				int num = Math.DivRem(Table.Columns.Count, foot.Columns.Count, out result);
				int num2 = 0;
				for (int i = 0; i <= foot.Columns.Count; i++)
				{
					array[i] = num2;
					num2 += num;
					if (result > 0)
					{
						num2++;
						result--;
					}
				}
			}
			else
			{
				for (int j = 0; j <= foot.Columns.Count; j++)
				{
					array[j] = j;
				}
			}
			int num3 = Table.Rows.Count + titleLayers + captionLayers + (ifSetNote ? 1 : 0);
			for (int k = 0; k < foot.Rows.Count; k++)
			{
				int num4 = num3 + k;
				TableTitleRow tableTitleRow = foot.Rows[k];
				int num5 = ((tableTitleRow.Height > 0) ? tableTitleRow.Height : 40);
				sheet.Rows[num4].Height = C1XLBook.PixelsToTwips(num5);
				for (int l = 0; l < tableTitleRow.Cells.Count; l++)
				{
					TableTitleCell tableTitleCell = tableTitleRow.Cells[l];
					XLStyle titleStyle = GetTitleStyle(tableTitleCell);
					sheet[num4, array[l]].Style = titleStyle;
					sheet[num4, array[l]].Value = tableTitleCell.GetDisplayValue();
				}
			}
			if (foot.Columns.Count < Table.Columns.Count)
			{
				for (int m = 0; m < foot.Rows.Count; m++)
				{
					int num6 = num3 + m;
					for (int n = 0; n < foot.Columns.Count; n++)
					{
						customMerge(num6, array[n], num6, array[n + 1] - 1);
					}
				}
			}
			foreach (TicketMerge merge in foot.Merges)
			{
				int r3 = num3 + merge.TopRow;
				int r4 = num3 + merge.BottomRow;
				customMerge(r3, array[merge.LeftColumn], r4, array[merge.RightColumn + 1] - 1);
			}
		}
		void setCaption()
		{
			titleLayers = Table.Title.Rows.Count + 1;
			if (Table.HeaderMode == TableHeaderMode.Custom)
			{
				Dictionary<int, List<string>> dictionary = Table.Columns.ToDictionary((Leqisoft.Model.Column t) => t.Index, (Leqisoft.Model.Column t) => t.CaptionDisplay.Split('_').ToList());
				captionLayers = ((dictionary.Count != 0) ? dictionary.Max((KeyValuePair<int, List<string>> t) => t.Value.Count) : 0);
				for (int num10 = 0; num10 < captionLayers; num10++)
				{
					sheet.Rows[num10 + titleLayers].Height = C1XLBook.PixelsToTwips(30.0);
					for (int num11 = 0; num11 < Table.Columns.Count; num11++)
					{
						Leqisoft.Model.CellStyle captionStyle = Table.Columns[num11].CaptionStyle;
						XLStyle xLStyle = new XLStyle(xlBook);
						XLAlignHorzEnum horizTextAlign2 = XLAlignHorzEnum.Center;
						XLAlignVertEnum vertTextAlign2 = XLAlignVertEnum.Center;
						if (captionStyle.Align.HasValue)
						{
							convertAlign(captionStyle.Align.Value, out horizTextAlign2, out vertTextAlign2);
						}
						xLStyle.AlignHorz = horizTextAlign2;
						xLStyle.AlignVert = vertTextAlign2;
						xLStyle.BackColor = captionStyle.BackColor ?? Color.White;
						xLStyle.ForeColor = captionStyle.ForeColor ?? Color.Black;
						FontStyle fontStyle = (captionStyle.Bold.GetValueOrDefault() ? FontStyle.Bold : FontStyle.Regular);
						FontStyle fontStyle2 = (captionStyle.Italic.GetValueOrDefault() ? FontStyle.Italic : FontStyle.Regular);
						FontStyle fontStyle3 = (captionStyle.Underline.GetValueOrDefault() ? FontStyle.Underline : FontStyle.Regular);
						xLStyle.Font = new Font(captionStyle.FontFamily, captionStyle.FontSize.GetValueOrDefault(9f), fontStyle | fontStyle2 | fontStyle3);
						sheet[num10 + titleLayers, num11].Style = xLStyle;
						sheet.Columns[num11].Width = C1XLBook.PixelsToTwips(100.0);
						sheet[num10 + titleLayers, num11].Value = ((num10 >= dictionary[num11].Count) ? string.Empty : dictionary[num11][num10].ToString());
					}
				}
			}
			List<CellRange> mergeInfo = Table.GetMergeInfo(visibleOnly: false);
			foreach (CellRange item in mergeInfo)
			{
				customMerge(item.r1 + titleLayers, item.c1, item.r2 + titleLayers, item.c2);
			}
		}
		void setContent()
		{
			for (int num7 = 0; num7 < Table.Rows.Count; num7++)
			{
				int num8 = num7 + titleLayers + captionLayers;
				sheet.Rows[num8].Height = C1XLBook.PixelsToTwips(Table.Rows[num7].Height);
				for (int num9 = 0; num9 < Table.Columns.Count; num9++)
				{
					Leqisoft.Model.Cell cell2 = Table[num7, num9];
					if (cell2 != null)
					{
						XLStyle cellStyle = GetCellStyle(cell2);
						sheet[num8, num9].Style = cellStyle;
						sheet[num8, num9].Value = ConvertValue(cell2);
						sheet.Columns[num9].Width = C1XLBook.PixelsToTwips(cell2.Column.Width);
					}
					sheet.Columns[num9].Visible = Table.Columns[num9].Visible;
				}
			}
			foreach (CellMerge mergedCell in Table.MergedCells)
			{
				customMerge(mergedCell.TopLeft.Row.Index + titleLayers + captionLayers, mergedCell.TopLeft.Column.Index, mergedCell.BottomRight.Row.Index + titleLayers + captionLayers, mergedCell.BottomRight.Column.Index);
			}
		}
		void setFooter()
		{
			sheet.PrintSettings.Footer = CreateFooter();
		}
		void setHeader()
		{
			sheet.PrintSettings.Header = CreateHeader();
		}
		void setMainTitle()
		{
			XLStyle titleStyle3 = GetTitleStyle(Table.Title.TitleCell);
			titleStyle3.AlignHorz = ConvertHorz(Table.Title.TitleCell.Align);
			titleStyle3.AlignVert = ConvertVert(Table.Title.TitleCell.Align);
			customMerge(0, 0, 0, Math.Max(Table.Title.Columns.Count, Table.Columns.Count) - 1);
			int num21 = ((Table.Title.TitleHeight > 0) ? Table.Title.TitleHeight : 40);
			sheet.Rows[0].Height = C1XLBook.PixelsToTwips(num21);
			sheet[0, 0].Style = titleStyle3;
			sheet[0, 0].Value = Table.Title.TitleCell.GetDisplayValue();
		}
		void setSubTitle()
		{
			TableTitle title = Table.Title;
			int[] array2 = new int[title.Columns.Count + 1];
			if (title.Columns.Count < Table.Columns.Count)
			{
				int result2;
				int num12 = Math.DivRem(Table.Columns.Count, title.Columns.Count, out result2);
				int num13 = 0;
				for (int num14 = 0; num14 <= title.Columns.Count; num14++)
				{
					array2[num14] = num13;
					num13 += num12;
					if (result2 > 0)
					{
						num13++;
						result2--;
					}
				}
			}
			else
			{
				for (int num15 = 0; num15 <= title.Columns.Count; num15++)
				{
					array2[num15] = num15;
				}
			}
			for (int num16 = 0; num16 < title.Rows.Count; num16++)
			{
				TableTitleRow tableTitleRow2 = title.Rows[num16];
				int num17 = ((tableTitleRow2.Height > 0) ? tableTitleRow2.Height : 40);
				sheet.Rows[num16 + 1].Height = C1XLBook.PixelsToTwips(num17);
				for (int num18 = 0; num18 < tableTitleRow2.Cells.Count; num18++)
				{
					TableTitleCell tableTitleCell2 = tableTitleRow2.Cells[num18];
					XLStyle titleStyle2 = GetTitleStyle(tableTitleCell2);
					sheet[num16 + 1, array2[num18]].Style = titleStyle2;
					sheet[num16 + 1, array2[num18]].Value = tableTitleCell2.GetDisplayValue();
				}
			}
			if (title.Columns.Count < Table.Columns.Count)
			{
				for (int num19 = 0; num19 < title.Rows.Count; num19++)
				{
					for (int num20 = 0; num20 < title.Columns.Count; num20++)
					{
						customMerge(num19 + 1, array2[num20], num19 + 1, array2[num20 + 1] - 1);
					}
				}
			}
			foreach (TicketMerge merge2 in title.Merges)
			{
				customMerge(merge2.TopRow + 1, array2[merge2.LeftColumn], merge2.BottomRow + 1, array2[merge2.RightColumn + 1] - 1);
			}
		}
	}

	private string GetWaterMarkHeaderOrFooterSectionPrintSetting(PageSetupWaterMark.WaterMarkSetting setting, string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text2 = "微软雅黑";
		string text3 = "12";
		if (!string.IsNullOrWhiteSpace(setting.FontName))
		{
			text2 = setting.FontName;
		}
		if (setting.Height > 0.0)
		{
			text3 = setting.Height.ToString();
		}
		stringBuilder.Append("&\"" + text2 + "\"");
		stringBuilder.Append("&" + text3);
		stringBuilder.Append(text);
		return stringBuilder.ToString();
	}

	private string CreateHeader()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (WaterMarkPageSetup != null && WaterMarkPageSetup.Header != null)
		{
			PageSetupWaterMark.WaterMarkSetting header = WaterMarkPageSetup.Header;
			stringBuilder.Append("&L");
			stringBuilder.Append(GetWaterMarkHeaderOrFooterSectionPrintSetting(header, header.LeftText));
			stringBuilder.Append("&C");
			stringBuilder.Append(GetWaterMarkHeaderOrFooterSectionPrintSetting(header, header.CenterText));
			stringBuilder.Append("&R");
			stringBuilder.Append(GetWaterMarkHeaderOrFooterSectionPrintSetting(header, header.RightText));
			return stringBuilder.ToString();
		}
		stringBuilder.Append("&L");
		stringBuilder.Append(ConvertText(ConvertTag(PageSetup.PageHeader.LeftValue)));
		stringBuilder.Append("&C");
		stringBuilder.Append(ConvertText(ConvertTag(PageSetup.PageHeader.CenterValue)));
		stringBuilder.Append("&R");
		stringBuilder.Append(ConvertText(ConvertTag(PageSetup.PageHeader.RightValue)));
		return stringBuilder.ToString();
	}

	private string CreateFooter()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (WaterMarkPageSetup != null && WaterMarkPageSetup.Footer != null)
		{
			PageSetupWaterMark.WaterMarkSetting footer = WaterMarkPageSetup.Footer;
			stringBuilder.Append("&L");
			stringBuilder.Append(GetWaterMarkHeaderOrFooterSectionPrintSetting(footer, footer.LeftText));
			stringBuilder.Append("&C");
			stringBuilder.Append(GetWaterMarkHeaderOrFooterSectionPrintSetting(footer, footer.CenterText));
			stringBuilder.Append("&R");
			stringBuilder.Append(GetWaterMarkHeaderOrFooterSectionPrintSetting(footer, footer.RightText));
			return stringBuilder.ToString();
		}
		stringBuilder.Append("&L");
		stringBuilder.Append(ConvertText(ConvertTag(PageSetup.PageFooter.LeftValue)));
		stringBuilder.Append("&C");
		stringBuilder.Append(ConvertText(ConvertTag(PageSetup.PageFooter.CenterValue)));
		stringBuilder.Append("&R");
		stringBuilder.Append(ConvertText(ConvertTag(PageSetup.PageFooter.RightValue)));
		return stringBuilder.ToString();
	}

	private XLStyle GetTitleStyle(TableTitleCell cell)
	{
		XLStyle xLStyle = new XLStyle(xlBook);
		FontStyle fontStyle = (cell.Bold ? FontStyle.Bold : FontStyle.Regular);
		FontStyle fontStyle2 = (cell.Italic ? FontStyle.Italic : FontStyle.Regular);
		FontStyle fontStyle3 = (cell.Underline ? FontStyle.Underline : FontStyle.Regular);
		FontStyle fontStyle4 = (cell.Strikeout ? FontStyle.Strikeout : FontStyle.Regular);
		xLStyle.Font = new Font(cell.FontFamily, cell.FontSize, fontStyle | fontStyle2 | fontStyle3 | fontStyle4);
		xLStyle.ForeColor = cell.ForeColor;
		xLStyle.BackColor = cell.BackColor;
		xLStyle.WordWrap = true;
		return xLStyle;
	}

	private XLStyle GetCellStyle(Leqisoft.Model.Cell cell)
	{
		XLStyle xLStyle = new XLStyle(xlBook);
		xLStyle.Font = cell.GetFont();
		xLStyle.BackColor = cell.DisplayBackColor;
		xLStyle.ForeColor = cell.DisplayForeColor;
		xLStyle.AlignHorz = ConvertHorz(cell.DisplayAlign);
		xLStyle.AlignVert = ConvertVert(cell.DisplayAlign);
		xLStyle.Indent = C1XLBookEx.PixelToIndent(cell.DisplayMargin);
		xLStyle.WordWrap = true;
		try
		{
			string text = cell.DisplayFormat.GetFormatString();
			if (cell.DisplayFormat.FormatType == DataFormatType.DateSlash)
			{
				text = "yyyy/MM/dd";
			}
			string text2 = text;
			if (text2.EndsWith("."))
			{
				text2 = text2.Substring(0, text2.Length - 1);
			}
			string format = XLStyle.FormatDotNetToXL(text2, cell.DisplayDataType);
			xLStyle.Format = format;
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		return xLStyle;
	}

	private void ApplySubTotal(XLSheet sheet, int FixedCols)
	{
		Dictionary<Tuple<int, int>, int> dictionary = new Dictionary<Tuple<int, int>, int>();
		if (Table.SubTotal.GroupColumns.Count <= 0)
		{
			return;
		}
		int num = Table.SubTotal.GroupColumns.Min((Id64 t) => Table.Columns.GetById(t).Index);
		int count = Table.SubTotal.GroupColumns.Count;
		int num2 = 0;
		Dictionary<Tuple<int, int, Id64>, double> dictionary2 = Table.SubTotal.Apply();
		foreach (KeyValuePair<Tuple<int, int, Id64>, double> item in dictionary2)
		{
			if (dictionary.ContainsKey(Tuple.Create(item.Key.Item1, item.Key.Item2)))
			{
				num2 = dictionary[Tuple.Create(item.Key.Item1, item.Key.Item2)];
			}
			else
			{
				if (Table.SubTotal.Direction == DirectionEnum.Top)
				{
					num2 = item.Key.Item1 + dictionary.Count + FixedCols;
					if (num2 > 0)
					{
						sheet.Rows.Insert(num2);
						sheet.Rows[num2].Height = sheet.Rows[num2 + 1].Height;
					}
				}
				else if (Table.SubTotal.Direction == DirectionEnum.Bottom)
				{
					num2 = item.Key.Item2 + 1 + dictionary.Count + FixedCols;
					if (num2 < sheet.Rows.Count)
					{
						sheet.Rows.Insert(num2);
					}
					sheet.Rows[num2].Height = sheet.Rows[num2 - 1].Height;
				}
				dictionary.Add(Tuple.Create(item.Key.Item1, item.Key.Item2), num2);
				XLCellRange cr = new XLCellRange(num2, num2, num + 1, num + 1 + count);
				sheet.MergedCells.Add(cr);
				sheet[num2, num + 1].Style = sheet[num2, num + 1].Style ?? new XLStyle(xlBook);
				sheet[num2, num + 1].Value = Table.SubTotal.TotalName;
				sheet[num2, num + 1].Style.AlignVert = XLAlignVertEnum.Center;
				sheet[num2, num + 1].Style.AlignHorz = XLAlignHorzEnum.Center;
			}
			int colIndex = Table.Columns.GetById(item.Key.Item3).Index + 1;
			sheet[num2, colIndex].Value = item.Value.ToString();
			sheet[num2, colIndex].Style = sheet[num2, colIndex].Style ?? new XLStyle(xlBook);
			sheet[num2, colIndex].Style.AlignVert = XLAlignVertEnum.Center;
		}
	}

	private XLAlignHorzEnum ConvertHorz(CellTextAlign align)
	{
		switch (align)
		{
		case CellTextAlign.TopCenter:
		case CellTextAlign.MiddleCenter:
		case CellTextAlign.BottomCenter:
			return XLAlignHorzEnum.Center;
		case CellTextAlign.TopLeft:
		case CellTextAlign.MiddleLeft:
		case CellTextAlign.BottomLeft:
			return XLAlignHorzEnum.Left;
		case CellTextAlign.TopRight:
		case CellTextAlign.MiddleRight:
		case CellTextAlign.BottomRight:
			return XLAlignHorzEnum.Right;
		default:
			throw new ArgumentOutOfRangeException("align", align, null);
		}
	}

	private XLAlignVertEnum ConvertVert(CellTextAlign align)
	{
		switch (align)
		{
		case CellTextAlign.BottomLeft:
		case CellTextAlign.BottomCenter:
		case CellTextAlign.BottomRight:
			return XLAlignVertEnum.Bottom;
		case CellTextAlign.MiddleLeft:
		case CellTextAlign.MiddleCenter:
		case CellTextAlign.MiddleRight:
			return XLAlignVertEnum.Center;
		case CellTextAlign.TopLeft:
		case CellTextAlign.TopCenter:
		case CellTextAlign.TopRight:
			return XLAlignVertEnum.Top;
		default:
			throw new ArgumentOutOfRangeException("align", align, null);
		}
	}

	private XLLineStyleEnum StyleToXL(LineStyle line)
	{
		return line switch
		{
			LineStyle.Thick => XLLineStyleEnum.Medium, 
			LineStyle.Thin => XLLineStyleEnum.Thin, 
			LineStyle.Dash => XLLineStyleEnum.Dotted, 
			LineStyle.None => XLLineStyleEnum.None, 
			_ => XLLineStyleEnum.None, 
		};
	}

	private string ConvertTag(string value)
	{
		return value?.Replace("[PageCount]", "&N")?.Replace("[PageNo]", "&P");
	}

	private string ConvertText(string Rtf)
	{
		RichTextBox richTextBox = new RichTextBox();
		richTextBox.Rtf = Rtf;
		return richTextBox.Text;
	}

	private void DefaultStyle(XLCell cell)
	{
		if (cell.Style == null)
		{
			XLStyle style = new XLStyle(xlBook);
			cell.Style = style;
		}
	}

	private void SetC1ExcleStyle(TableBorderStyle tableStyle, XLSheet sheet, int CaptionRow, bool Rtf = true)
	{
		int num = Table.Title.Rows.Count + 1;
		for (int i = 0; i < Table.Rows.Count + CaptionRow + (Rtf ? 1 : 0); i++)
		{
			for (int j = 0; j < Table.Columns.Count; j++)
			{
				int rowIndex = i + num;
				DefaultStyle(sheet[rowIndex, j]);
				sheet[rowIndex, j].Style.BorderTop = StyleToXL(tableStyle.BodyLine);
				sheet[rowIndex, j].Style.BorderBottom = StyleToXL(tableStyle.BodyLine);
				sheet[rowIndex, j].Style.BorderLeft = StyleToXL(tableStyle.BodyLine);
				sheet[rowIndex, j].Style.BorderRight = StyleToXL(tableStyle.BodyLine);
			}
		}
		for (int k = 0; k < Table.Columns.Count; k++)
		{
			DefaultStyle(sheet[num, k]);
			sheet[num, k].Style.BorderTop = StyleToXL(tableStyle.UpDownLine);
			DefaultStyle(sheet[num + CaptionRow - 1, k]);
			sheet[num + CaptionRow - 1, k].Style.BorderBottom = StyleToXL(tableStyle.SecondLine);
			DefaultStyle(sheet[CaptionRow + Table.Rows.Count + num, k]);
			sheet[CaptionRow + Table.Rows.Count + num - ((!Rtf) ? 1 : 0), k].Style.BorderBottom = StyleToXL(tableStyle.UpDownLine);
		}
		for (int l = 0; l < Table.Rows.Count + CaptionRow + (Rtf ? 1 : 0); l++)
		{
			DefaultStyle(sheet[l + num, 0]);
			sheet[l + num, 0].Style.BorderLeft = StyleToXL(tableStyle.LeftRightLine);
			int num2 = Table.Columns.Count - 1;
			if (num2 >= 0)
			{
				DefaultStyle(sheet[l + num, num2]);
				sheet[l + num, num2].Style.BorderRight = StyleToXL(tableStyle.LeftRightLine);
			}
		}
	}
}
