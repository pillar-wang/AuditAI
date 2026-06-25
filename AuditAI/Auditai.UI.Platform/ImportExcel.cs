using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using C1.C1Excel;
using Auditai.DTO;
using Auditai.ExcelFormulaImporter;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class ImportExcel
{
	private const int MAX_FORMULA_COUNT = 1000;

	private readonly C1XLBook xLBook = new C1XLBook();

	private Dictionary<string, TempTable> SheetMap = new Dictionary<string, TempTable>();

	private List<Auditai.Model.Table> tables = new List<Auditai.Model.Table>();

	private int _currentIndex = -1;

	private int _currentRow;

	private bool _fixHead;

	private int bodyOffSet;

	private Auditai.Model.Table _currentTable;

	private XLSheet _currentSheet;

	private string file;

	private int _formulaCount;

	private const string COMMENT_MAINTITLE = "主标题";

	private const string COMMENT_SUBTITLE_LEFT = "左副标题";

	private const string COMMENT_SUBTITLE_CENTER = "中副标题";

	private const string COMMENT_SUBTITLE_RIGHT = "右副标题";

	private const string COMMENT_COLUMN_CAPTION = "列头";

	private const string COMMENT_NOTE = "审计说明";

	private const string COMMENT_BOTTOMMARK_LEFT = "左表底签名";

	private const string COMMENT_BOTTOMMARK_CENTER = "中表底签名";

	private const string COMMENT_BOTTOMMARK_RIGHT = "右表底签名";

	private const int MAX_IMPORT_ROWCOUNT = 50000;

	private Tuple<XLSheet, int> cachedSheetValidRow;

	private Tuple<XLSheet, int> cachedSheetValidCol;

	public int SheetCount => xLBook.Sheets.Count;

	public XLSheet CurrentSheet => _currentSheet;

	public bool CurrentEmpty()
	{
		return IsEmptySheet(xLBook.Sheets[_currentIndex]);
	}

	private bool IsEmptySheet(XLSheet sheet)
	{
		if (sheet == null)
		{
			return true;
		}
		if (sheet.Rows.Count == 0 || sheet.Columns.Count == 0)
		{
			return true;
		}
		return false;
	}

	public void Load(string file)
	{
		xLBook.Load(file);
		this.file = file;
		_fixHead = false;
		_currentRow = 0;
		_currentIndex = -1;
		_currentSheet = null;
		_currentTable = null;
		SheetMap = new Dictionary<string, TempTable>();
		tables = new List<Auditai.Model.Table>();
	}

	public bool HasNext()
	{
		_currentIndex++;
		return _currentIndex < xLBook.Sheets.Count;
	}

	public void Next()
	{
		_currentIndex++;
	}

	public void Import(Auditai.Model.Table table)
	{
		try
		{
			if (_currentIndex >= xLBook.Sheets.Count)
			{
				throw new SheetOutOfRangeException("Sheet超出范围");
			}
			_currentSheet = xLBook.Sheets[_currentIndex];
			_currentTable = table;
			if (PreDeal(file))
			{
				SheetMap.Add(_currentSheet.Name, new TempTable
				{
					TableId = table.Id.Value
				});
				tables.Add(table);
				_currentRow = 0;
				_fixHead = false;
				GetCaption();
				GetBody();
			}
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	public void GenerateFormula()
	{
		try
		{
			foreach (KeyValuePair<string, TempTable> sheet in SheetMap)
			{
				try
				{
					Auditai.Model.Table table = tables.Find((Auditai.Model.Table t) => t.Id.Value == sheet.Value.TableId);
					ExcelFormulaImportContext context = new ExcelFormulaImportContext
					{
						CurrentTableId = new Id64(sheet.Value.TableId),
						SheetNameMapper = (string sn) => new Id64(SheetMap[sn].TableId),
						CellMapper = (string sn, int row, int col) => new Id64((sn == null) ? sheet.Value[row - 1 - sheet.Value.SheetOffset][col - 1].CellId : SheetMap[sn][row - 1 - SheetMap[sn].SheetOffset][col - 1].CellId)
					};
					Auditai.ExcelFormulaImporter.ExcelFormulaImporter excelFormulaImporter = new Auditai.ExcelFormulaImporter.ExcelFormulaImporter(context);
					foreach (KeyValuePair<int, TempRow> item in sheet.Value)
					{
						foreach (KeyValuePair<int, TempCell> item2 in item.Value)
						{
							string formula = item2.Value.Formula;
							if (string.IsNullOrEmpty(formula) || _formulaCount >= 1000)
							{
								continue;
							}
							_formulaCount++;
							if (item.Key < table.Rows.Count && item2.Key < table.Columns.Count)
							{
								Auditai.Model.Cell cell = table.Cells.Get(item.Key, item2.Key);
								try
								{
									string text = excelFormulaImporter.Convert(formula);
									cell.UpdateFormula(text);
								}
								catch (Exception exception)
								{
									exception.Log();
									cell.UpdateFormula(string.Empty);
								}
							}
						}
					}
				}
				catch (Exception exception2)
				{
					exception2.Log();
				}
			}
		}
		catch (Exception exception3)
		{
			exception3.Log();
		}
	}

	private void GetMainTitle()
	{
		Auditai.Model.Table currentTable = _currentTable;
		XLSheet currentSheet = _currentSheet;
		if (GetValidRowCount(currentSheet) > 0)
		{
			XLCell xLCell = FindCommentCell(currentSheet, 0, (string comment) => (comment?.Trim()?.EndsWith("主标题")).GetValueOrDefault());
			if (xLCell != null)
			{
				currentTable.Title.TitleCell.Value = xLCell.Text;
				SetTitleStyle(currentTable.Title.TitleCell, xLCell.Style);
				currentTable.TagTitleDirty();
				_currentRow++;
			}
		}
	}

	private XLCell FindCommentCell(XLSheet sheet, int row, Predicate<string> preCommentText)
	{
		foreach (XLComment item in (IEnumerable)sheet.Comments)
		{
			if (item.RowIndex == row && preCommentText(GetText(item)))
			{
				return item.Cell;
			}
		}
		return null;
	}

	private void SetTitleStyle(TableTitleCell titleCell, XLStyle style)
	{
		if (style != null)
		{
			titleCell.Bold = style.Font.Bold;
			titleCell.Italic = style.Font.Italic;
			titleCell.Underline = style.Font.Underline;
			titleCell.Strikeout = style.Font.Strikeout;
			titleCell.FontSize = style.Font.Size;
			titleCell.ForeColor = style.ForeColor;
			titleCell.BackColor = style.BackColor;
			titleCell.FontFamily = style.Font.FontFamily.Name;
			titleCell.Align = ConvertAlign(style.AlignHorz, style.AlignVert);
		}
	}

	private CellTextAlign ConvertAlign(XLAlignHorzEnum alignHorzEnum, XLAlignVertEnum alignVertEnum)
	{
		switch (alignVertEnum)
		{
		case XLAlignVertEnum.Top:
			switch (alignHorzEnum)
			{
			case XLAlignHorzEnum.Left:
				return CellTextAlign.TopLeft;
			case XLAlignHorzEnum.Center:
				return CellTextAlign.TopCenter;
			case XLAlignHorzEnum.Right:
				return CellTextAlign.TopRight;
			}
			break;
		case XLAlignVertEnum.Center:
			switch (alignHorzEnum)
			{
			case XLAlignHorzEnum.Left:
				return CellTextAlign.MiddleLeft;
			case XLAlignHorzEnum.Center:
				return CellTextAlign.MiddleCenter;
			case XLAlignHorzEnum.Right:
				return CellTextAlign.MiddleRight;
			}
			break;
		case XLAlignVertEnum.Bottom:
			switch (alignHorzEnum)
			{
			case XLAlignHorzEnum.Left:
				return CellTextAlign.BottomLeft;
			case XLAlignHorzEnum.Center:
				return CellTextAlign.BottomCenter;
			case XLAlignHorzEnum.Right:
				return CellTextAlign.BottomRight;
			}
			break;
		}
		return CellTextAlign.MiddleLeft;
	}

	private void GetSubTitle()
	{
		try
		{
			if (_currentRow != 0)
			{
				Auditai.Model.Table currentTable = _currentTable;
				XLSheet currentSheet = _currentSheet;
				while (_currentRow < GetValidRowCount(currentSheet))
				{
					_currentRow++;
				}
			}
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private string GetText(XLComment comment)
	{
		if (IsRtf(comment.TextBox.Text))
		{
			RichTextBox richTextBox = new RichTextBox
			{
				Rtf = comment.TextBox.Text
			};
			return richTextBox.Text;
		}
		return comment.TextBox.Text;
	}

	private void GetCaption()
	{
		int tableRowHeight = UserSet.Config.TableStyle.TableRowHeight;
		Auditai.Model.Table currentTable = _currentTable;
		XLSheet sheet = _currentSheet;
		currentTable.Columns.Append(GetValidColCount(sheet));
		if (_fixHead)
		{
			currentTable.UpdateHeaderMode(TableHeaderMode.Fixed);
			currentTable.UpdateHeaderRowHeight(0, tableRowHeight);
			return;
		}
		IEnumerable<XLCellRange> cellMerges = sheet.MergedCells.Cast<XLCellRange>();
		int num = 0;
		if (num == 0)
		{
			_fixHead = true;
			currentTable.UpdateHeaderMode(TableHeaderMode.Fixed);
			currentTable.UpdateHeaderRowHeight(0, tableRowHeight);
			return;
		}
		for (int k = 0; k < GetValidColCount(sheet); k++)
		{
			List<string> list = new List<string>();
			for (int l = 0; l < num; l++)
			{
				int num2 = _currentRow + l;
				list.Add(getCellText(num2, k));
				int num3 = C1XLBook.TwipsToPixels(sheet.Rows[num2].Height);
				currentTable.UpdateHeaderRowHeight(l, (num3 > tableRowHeight) ? num3 : tableRowHeight);
			}
			XLCell cell = sheet.GetCell(_currentRow, k);
			if (cell != null && cell.Style != null)
			{
				XLStyle style = cell.Style;
				Auditai.Model.CellStyle captionStyle = currentTable.Columns[k].CaptionStyle;
				captionStyle.Bold = style.Font.Bold;
				captionStyle.Italic = style.Font.Italic;
				captionStyle.Underline = style.Font.Underline;
				captionStyle.FontSize = style.Font.Size;
				captionStyle.ForeColor = style.ForeColor;
				captionStyle.BackColor = style.BackColor;
				captionStyle.FontFamily = style.Font.FontFamily.Name;
				captionStyle.Align = ConvertAlign(style.AlignHorz, style.AlignVert);
			}
			string caption = string.Join("_", list.Where((string c) => c != string.Empty));
			currentTable.Columns[k].UpdateCaption(caption);
			currentTable.Columns[k].UpdateWidth(C1XLBook.TwipsToPixels(sheet.Columns[k].Width));
		}
		_currentRow += num;
		string getCellText(int i, int j)
		{
			XLCell cell2 = sheet.GetCell(i, j);
			if (cell2 == null)
			{
				return string.Empty;
			}
			XLCellRange xLCellRange = cellMerges.FirstOrDefault((XLCellRange t) => i >= t.RowFrom && i <= t.RowTo && j >= t.ColumnFrom && j <= t.ColumnTo);
			if (xLCellRange == null)
			{
				return cell2.Text ?? string.Empty;
			}
			XLCell cell3 = sheet.GetCell(xLCellRange.RowFrom, xLCellRange.ColumnFrom);
			if (cell3 == null)
			{
				return string.Empty;
			}
			if (xLCellRange.RowFrom == i && xLCellRange.ColumnFrom == j)
			{
				return cell3.Text;
			}
			if (xLCellRange.ColumnFrom == j)
			{
				return string.Empty;
			}
			return cell3.Text;
		}
	}

	private void GetBody()
	{
		Auditai.Model.Table currentTable = _currentTable;
		XLSheet currentSheet = _currentSheet;
		bodyOffSet = _currentRow;
		SheetMap[currentSheet.Name].SheetOffset = bodyOffSet;
		int num = 0;
		int num2 = NoteAndMarkRows();
		Dictionary<XLStyle, Auditai.Model.CellStyle> dictionary = new Dictionary<XLStyle, Auditai.Model.CellStyle>();
		int validRowCount = GetValidRowCount(currentSheet);
		int validColCount = GetValidColCount(currentSheet);
		currentTable.Rows.Append(validRowCount - _currentRow - num2);
		int num3 = _currentRow;
		while (num3 < validRowCount)
		{
			if (num3 >= validRowCount - num2)
			{
				GetMark(num3);
			}
			else
			{
				_currentRow++;
				TempRow tempRow = new TempRow(num);
				SheetMap[currentSheet.Name].Add(tempRow);
				int num4 = ((currentSheet.Rows[num3].Height != -1) ? currentSheet.Rows[num3].Height : ((currentSheet.DefaultRowHeight == -1) ? 100 : currentSheet.DefaultRowHeight));
				currentTable.Rows[num].UpdateHeight(C1XLBook.TwipsToPixels(num4));
				for (int i = 0; i < validColCount; i++)
				{
					int width = C1XLBook.TwipsToPixels((currentSheet.Columns[i].Width != -1) ? currentSheet.Columns[i].Width : ((currentSheet.DefaultColumnWidth == -1) ? 100 : currentSheet.DefaultColumnWidth));
					currentTable.Columns[i].UpdateWidth(width);
					XLCell sheetCell = currentSheet.GetCell(num3, i);
					if (sheetCell == null)
					{
						continue;
					}
					object value2 = getCellValue(sheetCell);
					Auditai.Model.Cell cell2 = currentTable.Cells.Get(num, i);
					cell2.UpdateValue(typeConvert(value2));
					TempCell cell3 = new TempCell(i)
					{
						CellId = cell2.Id.Value,
						Formula = sheetCell.Formula
					};
					tempRow.Add(cell3);
					if (sheetCell.Style != null)
					{
						KeyValuePair<XLStyle, Auditai.Model.CellStyle> keyValuePair = dictionary.FirstOrDefault((KeyValuePair<XLStyle, Auditai.Model.CellStyle> s) => s.Key.Equals(sheetCell.Style));
						if (default(KeyValuePair<XLStyle, Auditai.Model.CellStyle>).Equals(keyValuePair))
						{
							Auditai.Model.CellStyle cellStyle = C1XLBookEx.ToCellStyle(sheetCell.Style);
							dictionary.Add(sheetCell.Style, cellStyle);
							cell2.Column.Table.CellStyles.Add(cellStyle);
							cell2.UpdateStyle(cellStyle);
						}
						else
						{
							cell2.UpdateStyle(dictionary[keyValuePair.Key]);
						}
					}
				}
			}
			num3++;
			num++;
		}
		int num5 = validRowCount - num2;
		foreach (XLCellRange item in (IEnumerable)currentSheet.MergedCells)
		{
			if (item.RowTo < num5 && item.RowFrom < num5 && item.RowFrom >= bodyOffSet)
			{
				currentTable.MergeCells(item.RowFrom - bodyOffSet, item.ColumnFrom, item.RowTo - bodyOffSet, item.ColumnTo);
			}
		}
		object getCellValue(XLCell cell)
		{
			if (cell.Value != null)
			{
				if (!IsRtf(cell.Value.ToString()))
				{
					return cell.Value;
				}
				return cell.Text;
			}
			return string.Empty;
		}
		static object typeConvert(object value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			if (value is string text)
			{
				return text.Replace("\r\n", "\n");
			}
			if (value is int || value is double || value is bool)
			{
				return value;
			}
			if (value is DateTime dateTime)
			{
				return dateTime.ToString("yyyy-M-d");
			}
			if (value is byte[])
			{
				return value;
			}
			if (value is Exception ex)
			{
				return ex.Message;
			}
			if (value is byte b)
			{
				return (double)(int)b;
			}
			if (value is float num8)
			{
				return (double)num8;
			}
			if (value is decimal num9)
			{
				return num9;
			}
			return value.ToString();
		}
	}

	private bool IsRtf(string value)
	{
		return Regex.IsMatch(value, "rtf.*}");
	}

	private int NoteAndMarkRows()
	{
		int validRowCount = GetValidRowCount(_currentSheet);
		if (validRowCount == 0)
		{
			return 0;
		}
		int num = 0;
		for (int num2 = validRowCount - 1; num2 >= 0; num2--)
		{
			XLCell xLCell = FindCommentCell(_currentSheet, num2, delegate(string comment)
			{
				string text = comment?.Trim();
				if (text == null)
				{
					return false;
				}
				return text.EndsWith("左表底签名") || text.EndsWith("中表底签名") || text.EndsWith("右表底签名") || text.EndsWith("审计说明");
			});
			if (xLCell == null)
			{
				break;
			}
			num++;
		}
		return num;
	}

	private void GetMark(int r)
	{
		Auditai.Model.Table currentTable = _currentTable;
		XLSheet currentSheet = _currentSheet;
		if (GetValidRowCount(currentSheet) != 0)
		{
			_ = _currentRow;
		}
	}

	private bool PreDeal(string file)
	{
		XLSheet currentSheet = _currentSheet;
		Auditai.Model.Table currentTable = _currentTable;
		if (currentSheet.Rows.Count == 0 || currentSheet.Columns.Count == 0)
		{
			currentTable.UpdateHeaderRowHeight(0, 30);
			currentTable.UpdateHeaderMode(TableHeaderMode.Fixed);
			currentTable.Rows.Append(10);
			currentTable.Columns.Append(5);
			return false;
		}
		if (currentSheet.Rows.Count > 50000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"文件【{Path.GetFileName(file)}】的工作表【{currentSheet.Name}】行数过多，最大导入{50000}行，超出部分不导入");
		}
		else if (currentSheet.Rows.Count > SoftwareLicenseManager.GetTableMaxRowsCountLimit())
		{
			string text = (SoftwareLicenseManager.IsNoTableRowsLimitLicense() ? $"单表格的最大行数限制为{SoftwareLicenseManager.GetTableMaxRowsCountLimit()}行，您导入的文件【{Path.GetFileName(file)}】中的工作表【{currentSheet.Name}】行数超过该限制，超过部分将被忽略!" : ((!SoftwareLicenseManager.IsCurrentProjectBeFreeProjectOnPayByProject()) ? $"尊敬的用户：\r\n您是{SoftwareLicenseManager.GetCurrentLicenseDisplayName()}用户，单表格的最大行数限制为{SoftwareLicenseManager.GetTableMaxRowsCountLimit()}行，您导入的文件【{Path.GetFileName(file)}】中的工作表【{currentSheet.Name}】行数超过该限制，超过部分将被忽略，请联系官方客服升级为{SoftwareLicenseManager.GetUnlimitLicenseDisplayName()}用户，不受该限制!" : ((!SoftwareLicenseManager.IsFreeTeam()) ? $"尊敬的用户：\r\n当前{StringConstBase.Current.Project}为体验{StringConstBase.Current.Project}，单表格的最大行数限制为{SoftwareLicenseManager.GetTableMaxRowsCountLimit()}行，您导入的文件【{Path.GetFileName(file)}】中的工作表【{currentSheet.Name}】行数超过该限制，超过部分将被忽略，请联系官方客服升级为正式{StringConstBase.Current.Project}，不受该限制!" : $"尊敬的用户：\r\n您是免费版用户，单表格的最大行数限制为{SoftwareLicenseManager.GetTableMaxRowsCountLimit()}行，您导入的文件【{Path.GetFileName(file)}】中的工作表【{currentSheet.Name}】行数超过该限制，超过部分将被忽略，请联系官方客服升级为正式版用户，不受该限制!")));
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text);
		}
		for (int i = 0; i < GetValidRowCount(currentSheet); i++)
		{
			for (int j = 0; j < GetValidColCount(currentSheet); j++)
			{
				XLCell cell = currentSheet.GetCell(i, j);
				if (cell != null && !string.IsNullOrEmpty(cell.Formula) && (cell.Formula.ToLower().Contains(".xls") || cell.Formula.ToLower().Contains(".xlsx")))
				{
					cell.Formula = string.Empty;
				}
			}
		}
		return true;
	}

	private string ToRtf(string text)
	{
		RichTextBox richTextBox = new RichTextBox
		{
			Text = text
		};
		return richTextBox.Rtf;
	}

	private XLCellRange GetCellMerge(XLSheet sheet, XLCell xlCell)
	{
		getCellPosition(sheet, xlCell, out var outrow2, out var outcol2);
		foreach (XLCellRange item in (IEnumerable)sheet.MergedCells)
		{
			if (item.RowFrom == outrow2 && item.ColumnFrom == outcol2)
			{
				return item;
			}
		}
		return new XLCellRange(outrow2, outrow2, outcol2, outcol2);
		bool getCellPosition(XLSheet xlSheet, XLCell cell, out int outrow, out int outcol)
		{
			outrow = -1;
			outcol = -1;
			for (int i = 0; i < GetValidRowCount(sheet); i++)
			{
				for (int j = 0; j < GetValidColCount(sheet); j++)
				{
					if (xlSheet.GetCell(i, j) == cell)
					{
						outrow = i;
						outcol = j;
						return true;
					}
				}
			}
			return false;
		}
	}

	private int GetValidRowCount(XLSheet xLSheet)
	{
		if (cachedSheetValidRow == null || cachedSheetValidRow.Item1 != xLSheet)
		{
			int validRowCount = C1XLBookEx.GetValidRowCount(xLSheet);
			cachedSheetValidRow = Tuple.Create(xLSheet, (validRowCount > 50000) ? 50000 : validRowCount);
			if (cachedSheetValidRow.Item2 > SoftwareLicenseManager.GetTableMaxRowsCountLimit())
			{
				cachedSheetValidRow = Tuple.Create(cachedSheetValidRow.Item1, SoftwareLicenseManager.GetTableMaxRowsCountLimit());
			}
			return cachedSheetValidRow.Item2;
		}
		return cachedSheetValidRow.Item2;
	}

	private int GetValidColCount(XLSheet xLSheet)
	{
		if (cachedSheetValidCol == null || cachedSheetValidCol.Item1 != xLSheet)
		{
			cachedSheetValidCol = Tuple.Create(xLSheet, C1XLBookEx.GetValidColCount(xLSheet));
			return cachedSheetValidCol.Item2;
		}
		return cachedSheetValidCol.Item2;
	}
}
