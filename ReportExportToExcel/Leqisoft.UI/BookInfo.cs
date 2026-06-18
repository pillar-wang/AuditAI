using Leqisoft.Model;

namespace Leqisoft.UI;

public class BookInfo
{
	public Table Table { get; set; }

	public int RowOffSet { get; set; }

	public int ColumnOffSet { get; set; }

	public string SavePath { get; set; }

	public string SheetName { get; set; }

	public ReportExportToExcel Exporter { get; set; }
}
