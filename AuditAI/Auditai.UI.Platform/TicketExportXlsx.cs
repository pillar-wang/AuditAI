using System;
using System.Collections.Generic;
using System.IO;
using Auditai.DTO;
using Auditai.Model;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Auditai.UI.Platform;

/// <summary>
/// 单据导出 Excel。表头（Title）、表体（数据区）、表尾（Footer）依次写入工作表，
/// 单元格显示内容统一通过 TicketInputTableVM.GetDisplayValue() 获取（与打印预览一致）。
/// 导出过程不再静默吞异常：失败时向上抛出，由调用方弹窗提示用户。
/// </summary>
public class TicketExportXlsx
{
	public TicketTable Ticket { get; set; }
	public TicketInputTableVM VM { get; set; }
	public object WaterMarkPageSetup { get; set; }

	private XSSFWorkbook _workbook;

	/// <summary>把当前单据（表头 + 表体 + 表尾）生成到工作簿的第一个工作表。</summary>
	public void Generate(params object[] args)
	{
		if (Ticket == null || VM == null)
		{
			throw new InvalidOperationException("单据数据未初始化，无法导出。");
		}
		_workbook = new XSSFWorkbook();
		ISheet sheet = _workbook.CreateSheet("Ticket");
		int rowIndex = 0;
		rowIndex = WriteGrid(sheet, VM.Title, rowIndex);
		rowIndex = WriteGrid(sheet, VM, rowIndex);
		WriteGrid(sheet, VM.Footer, rowIndex);
	}

	public void Save(params object[] args)
	{
		if (_workbook == null)
		{
			throw new InvalidOperationException("没有可保存的工作簿，请先执行导出。");
		}
		if (args == null || args.Length == 0 || args[0] == null)
		{
			throw new ArgumentException("未指定导出文件路径。");
		}
		string filePath = args[0].ToString();
		using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
		{
			_workbook.Write(stream);
		}
	}

	/// <summary>批量导出多个单据记录，每个记录导出为一个独立工作表。</summary>
	public void BatchExportToFile(params object[] args)
	{
		if (args == null || args.Length < 2 || args[0] == null)
		{
			throw new ArgumentException("批量导出参数不足。");
		}
		string filePath = args[0].ToString();
		List<Tuple<TicketTable, TicketRecord, string>> ticketRecordsList = args[1] as List<Tuple<TicketTable, TicketRecord, string>>;
		if (ticketRecordsList == null || ticketRecordsList.Count == 0)
		{
			throw new ArgumentException("没有可导出的单据记录。");
		}
		XSSFWorkbook workbook = new XSSFWorkbook();
		HashSet<string> usedSheetNames = new HashSet<string>();
		int sheetIndex = 1;
		foreach (Tuple<TicketTable, TicketRecord, string> item in ticketRecordsList)
		{
			string sheetName = SanitizeSheetName(item.Item3);
			if (!usedSheetNames.Add(sheetName))
			{
				sheetName = SanitizeSheetName(item.Item3 + "_" + (++sheetIndex));
			}
			TicketInputTableVM vm = new TicketInputTableVM(item.Item1, item.Item2);
			vm.CalculateTicket();
			ISheet sheet = workbook.CreateSheet(sheetName);
			int rowIndex = 0;
			rowIndex = WriteGrid(sheet, vm.Title, rowIndex);
			rowIndex = WriteGrid(sheet, vm, rowIndex);
			WriteGrid(sheet, vm.Footer, rowIndex);
		}
		using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
		{
			workbook.Write(stream);
		}
	}

	private static int WriteGrid(ISheet sheet, TicketInputTitleFooterVM grid, int startRow)
	{
		return WriteGrid(sheet, grid.GetRowsCount(), grid.GetColumnsCount(), grid.GetCellVM, grid.GetRowHeight, grid.GetColumnWidth, startRow);
	}

	private static int WriteGrid(ISheet sheet, TicketInputTableVM grid, int startRow)
	{
		return WriteGrid(sheet, grid.GetRowsCount(), grid.GetColumnsCount(), grid.GetCellVM, grid.GetRowHeight, grid.GetColumnWidth, startRow);
	}

	private static int WriteGrid(ISheet sheet, int rowCount, int colCount, Func<int, int, TicketInputCellVM> getCellVM, Func<int, int> getRowHeight, Func<int, int> getColumnWidth, int startRow)
	{
		for (int i = 0; i < rowCount; i++)
		{
			IRow row = sheet.CreateRow(startRow + i);
			row.Height = PxToPoints(getRowHeight(i));
			for (int j = 0; j < colCount; j++)
			{
				ICell cell = row.CreateCell(j);
				cell.SetCellValue(SafeDisplay(getCellVM(i, j)));
			}
		}
		for (int j2 = 0; j2 < colCount; j2++)
		{
			sheet.SetColumnWidth(j2, PxToChar256(getColumnWidth(j2)));
		}
		return startRow + rowCount;
	}

	/// <summary>获取单元格显示值；单个异常单元格回退为空串，避免一条脏数据中断整单导出。</summary>
	private static string SafeDisplay(TicketInputCellVM cellVM)
	{
		try
		{
			return cellVM?.GetDisplayValue() ?? string.Empty;
		}
		catch
		{
			return string.Empty;
		}
	}

	/// <summary>像素按 0.75 折算为 Excel 行高（磅）。</summary>
	private static short PxToPoints(int px)
	{
		int points = (int)Math.Round(px * 0.75);
		return (short)Math.Max(1, points);
	}

	/// <summary>像素折算为 Excel 列宽单位（1/256 字符宽，按 7px 一字符估算）。</summary>
	private static int PxToChar256(int px)
	{
		int width = (int)Math.Round(px / 7.0 * 256.0);
		return Math.Max(1, width);
	}

	/// <summary>处理 Excel 工作表名非法字符与长度限制（最长 31 字符）。</summary>
	private static string SanitizeSheetName(string name)
	{
		string result = (name ?? string.Empty);
		foreach (char c in new[] { '\\', '/', '?', '*', '[', ']', ':' })
		{
			result = result.Replace(c, '_');
		}
		if (result.Length > 31)
		{
			result = result.Substring(0, 31);
		}
		return string.IsNullOrWhiteSpace(result) ? "Sheet" : result;
	}
}