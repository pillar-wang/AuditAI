using System;
using Auditai.DTO;

namespace Auditai.ExcelFormulaImporter;

internal static class Test
{
	private static void Main()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		ExcelFormulaImportContext context = new ExcelFormulaImportContext
		{
			CurrentTableId = new Id64(123L),
			CellMapper = (string sn, int row, int col) => new Id64(678L),
			SheetNameMapper = (string sn) => new Id64(999L)
		};
		ExcelFormulaImporter excelFormulaImporter = new ExcelFormulaImporter(context);
		string value = excelFormulaImporter.Convert("=1+A2+B3:C4+abs(abc!A2-abc!B3:C4)");
		Console.WriteLine(value);
		Console.ReadLine();
	}
}
