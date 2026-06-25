using System;
using Auditai.DTO;

namespace Auditai.ExcelFormulaImporter;

public class ExcelFormulaImportContext
{
	public Id64 CurrentTableId { get; set; }

	public Func<string, Id64> SheetNameMapper { get; set; }

	public Func<string, int, int, Id64> CellMapper { get; set; }
}
