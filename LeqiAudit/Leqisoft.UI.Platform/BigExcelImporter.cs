using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ExcelDataReader;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class BigExcelImporter : IDisposable
{
	private const double DEFAULT_CHARACTER_WIDTH = 8.0;

	private const int DEFAULT_ROW_HEIGHT = 20;

	private readonly FileStream _stream;

	private readonly IExcelDataReader _reader;

	private readonly string _fileName;

	public int SheetCount => _reader.ResultsCount;

	public string SheetName => _reader.Name;

	public int RowCount => _reader.RowCount;

	public BigExcelImporter(string path)
	{
		_fileName = path;
		_stream = File.OpenRead(path);
		_reader = ExcelReaderFactory.CreateReader(_stream);
	}

	public void Dispose()
	{
		_reader.Dispose();
		_stream.Close();
	}

	public void Import(Table table)
	{
		int num = Math.Min(_reader.FieldCount, 100);
		int num2 = RowCount;
		int tableMaxRowsCountLimit = SoftwareLicenseManager.GetTableMaxRowsCountLimit();
		if (RowCount > tableMaxRowsCountLimit)
		{
			string text = (SoftwareLicenseManager.IsNoTableRowsLimitLicense() ? $"单表格的最大行数限制为{tableMaxRowsCountLimit}行，您导入的文件【{Path.GetFileName(_fileName)}】中的工作表【{SheetName}】行数超过该限制，超过部分将被忽略!" : ((!SoftwareLicenseManager.IsCurrentProjectBeFreeProjectOnPayByProject()) ? $"尊敬的用户：\r\n您是{SoftwareLicenseManager.GetCurrentLicenseDisplayName()}用户，单表格的最大行数限制为{tableMaxRowsCountLimit}行，您导入的文件【{Path.GetFileName(_fileName)}】中的工作表【{SheetName}】行数超过该限制，超过部分将被忽略，请联系官方客服升级为{SoftwareLicenseManager.GetUnlimitLicenseDisplayName()}用户，不受该限制!" : ((!SoftwareLicenseManager.IsFreeTeam()) ? $"尊敬的用户：\r\n当前{StringConstBase.Current.Project}为体验{StringConstBase.Current.Project}，单表格的最大行数限制为{tableMaxRowsCountLimit}行，您导入的文件【{Path.GetFileName(_fileName)}】中的工作表【{SheetName}】行数超过该限制，超过部分将被忽略，请联系官方客服升级为正式{StringConstBase.Current.Project}，不受该限制!" : $"尊敬的用户：\r\n您是免费版用户，单表格的最大行数限制为{tableMaxRowsCountLimit}行，您导入的文件【{Path.GetFileName(_fileName)}】中的工作表【{SheetName}】行数超过该限制，超过部分将被忽略，请联系官方客服升级为正式版用户，不受该限制!")));
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text);
			num2 = tableMaxRowsCountLimit;
		}
		table.Rows.Append(num2 - table.Rows.Count);
		table.Columns.Append(num - table.Columns.Count);
		using Control control = new Control();
		using Graphics graphics = control.CreateGraphics();
		for (int i = 0; i < num; i++)
		{
			table.Columns[i].UpdateWidth((int)(_reader.GetColumnWidth(i) * 8.0));
		}
		int num3 = 0;
		while (_reader.Read())
		{
			for (int j = 0; j < num; j++)
			{
				Type fieldType = _reader.GetFieldType(j);
				Cell cell = table[num3, j];
				if (fieldType == typeof(double))
				{
					cell.UpdateValue(_reader.GetDouble(j));
				}
				else if (fieldType == typeof(DateTime))
				{
					cell.UpdateValue(_reader.GetDateTime(j).ToString("yyyy-M-d"));
				}
				else if (fieldType == typeof(string))
				{
					string @string = _reader.GetString(j);
					cell.UpdateValue(@string.Replace("\r\n", "\n"));
				}
			}
			table.Rows[num3].UpdateHeight((_reader.RowHeight == 0.0) ? 20 : ((int)(_reader.RowHeight / 72.0 * (double)graphics.DpiY)));
			num3++;
			if (num3 >= num2)
			{
				break;
			}
		}
	}

	public void NextSheet()
	{
		_reader.NextResult();
	}

	public static int GetMaxRowCount(string path)
	{
		int num = 0;
		using BigExcelImporter bigExcelImporter = new BigExcelImporter(path);
		for (int i = 0; i < bigExcelImporter.SheetCount; i++)
		{
			if (bigExcelImporter.RowCount > num)
			{
				num = bigExcelImporter.RowCount;
			}
			bigExcelImporter.NextSheet();
		}
		return num;
	}
}
