using System;
using System.IO;
using C1.C1Excel;
using Auditai.DTO;
using Auditai.Model;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;

namespace Auditai.UI.Platform;

public class BigExcelExporter : IDisposable
{
	private const double DEFAULT_CHARACTER_WIDTH = 8.0;

	private readonly SXSSFWorkbook _wb;

	private readonly ICellStyle _csDate;

	public Auditai.Model.Table Table { get; set; }

	public BigExcelExporter()
	{
		_wb = new SXSSFWorkbook();
		_csDate = _wb.CreateCellStyle();
		_csDate.DataFormat = _wb.CreateDataFormat().GetFormat("yyyy/m/d");
	}

	public void Export(string path)
	{
		ISheet sheet = _wb.CreateSheet();
		for (int i = 0; i < Table.Columns.Count; i++)
		{
			sheet.SetColumnWidth(i, (int)((double)Table.Columns[i].Width / 8.0 * 256.0));
		}
		for (int j = 0; j < Table.Rows.Count; j++)
		{
			IRow row = sheet.CreateRow(j);
			row.Height = (short)C1XLBook.PixelsToTwips(Table.Rows[j].Height);
			for (int k = 0; k < Table.Columns.Count; k++)
			{
				ICell cell = row.CreateCell(k);
				Auditai.Model.Cell cell2 = Table[j, k];
				object value = cell2.Value;
				if (!(value is double cellValue))
				{
					if (!(value is string cellValue2))
					{
						if (!(value is DateTime cellValue3))
						{
							if (!(value is TimeSpan timeSpan))
							{
								if (!(value is bool cellValue4))
								{
									if (value is DateYearMonth)
									{
										cell.SetCellValue(cell2.GetDisplayValue());
									}
								}
								else
								{
									cell.SetCellValue(cellValue4);
								}
							}
							else
							{
								cell.SetCellValue(timeSpan.ToString());
							}
						}
						else if (cellValue3.Year < 1900)
						{
							cell.SetCellValue(cell2.GetDisplayValue());
						}
						else
						{
							cell.SetCellValue(cellValue3);
							cell.CellStyle = _csDate;
						}
					}
					else
					{
						cell.SetCellValue(cellValue2);
					}
				}
				else
				{
					cell.SetCellValue(cellValue);
				}
			}
		}
		using FileStream stream = new FileStream(path, FileMode.Create);
		_wb.Write(stream);
	}

	public void Dispose()
	{
		_wb.Close();
	}
}
