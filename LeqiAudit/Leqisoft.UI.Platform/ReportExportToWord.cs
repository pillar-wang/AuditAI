using System;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

[Obsolete]
internal class ReportExportToWord
{
	private DocumentEditor documentEditor;

	public Document Document { get; set; }

	internal ReportExportToWord()
	{
		documentEditor = new DocumentEditor();
	}

	public void Save(string path)
	{
		documentEditor.Document = Document;
		documentEditor.PopulateDocument(false, false);
		documentEditor.ExportDocument(path);
	}
}
