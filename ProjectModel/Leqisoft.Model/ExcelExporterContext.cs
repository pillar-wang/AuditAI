using System;

namespace Leqisoft.Model;

public class ExcelExporterContext
{
	public Table CurrentTable { get; set; }

	public int CurrentRowIndex { get; set; }

	public Func<Table, string> TablePathMapper { get; set; }

	public Func<Cell, Tuple<int, int>> CellOffset { get; set; }

	public FormulaReferenceResolver Resolver { get; set; }

	public Cell CurrentCell { get; set; }

	public int DataRowStartIndex { get; set; }

	public int DataRowsCount { get; set; }
}
