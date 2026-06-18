﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using Leqisoft.DTO;
using Leqisoft.Model;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Leqisoft.UI.Platform;

public class TicketExportXlsx
{
	public dynamic Ticket { get; set; }
	public dynamic VM { get; set; }
	public object WaterMarkPageSetup { get; set; }

	private XSSFWorkbook _workbook;

	public void Generate(params object[] args)
	{
		try
		{
			_workbook = new XSSFWorkbook();
			ISheet sheet = _workbook.CreateSheet("Ticket");

			if (Ticket == null || VM == null) return;

			var ticketCells = Ticket.Cells as object[,];
			if (ticketCells == null) return;

			int rowCount = ticketCells.GetLength(0);
			int colCount = ticketCells.GetLength(1);

			for (int r = 0; r < rowCount; r++)
			{
				IRow row = sheet.CreateRow(r);
				for (int c = 0; c < colCount; c++)
				{
					ICell cell = row.CreateCell(c);
					var cellValue = ticketCells[r, c];
					if (cellValue != null)
					{
						var displayValue = cellValue.GetType().GetProperty("DisplayValue")?.GetValue(cellValue)?.ToString()
							?? cellValue.ToString();
						cell.SetCellValue(displayValue);
					}
				}
			}
		}
		catch
		{
			// 生成失败时静默处理
		}
	}

	public void Save(params object[] args)
	{
		try
		{
			if (_workbook == null || args.Length == 0) return;

			string filePath = args[0]?.ToString();
			if (string.IsNullOrEmpty(filePath)) return;

			using var stream = new FileStream(filePath, FileMode.Create);
			_workbook.Write(stream);
		}
		catch
		{
			// 保存失败时静默处理
		}
	}

	public void BatchExportToFile(params object[] args)
	{
		try
		{
			if (args.Length < 2) return;

			string filePath = args[0]?.ToString();
			if (string.IsNullOrEmpty(filePath)) return;

			var ticketRecordsList = args[1] as List<Tuple<TicketTable, TicketRecord, string>>;
			if (ticketRecordsList == null || ticketRecordsList.Count == 0) return;

			var wb = new XSSFWorkbook();
			ISheet sheet = wb.CreateSheet("Tickets");

			int rowIndex = 0;
			foreach (var item in ticketRecordsList)
			{
				IRow row = sheet.CreateRow(rowIndex);
				row.CreateCell(0).SetCellValue(item.Item3 ?? ""); // Name
				rowIndex++;
			}

			using var stream = new FileStream(filePath, FileMode.Create);
			wb.Write(stream);
		}
		catch
		{
			// 批量导出失败时静默处理
		}
	}
}