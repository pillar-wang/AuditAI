using Auditai.Model;

namespace Auditai.UI.Platform;

public class ImportWord
{
	public DocumentEditor Import(string path, Document document)
	{
		DocumentEditor documentEditor = new DocumentEditor
		{
			Document = document
		};
		documentEditor.PopulateDocument();
		documentEditor.Import(path);
		return documentEditor;
	}
}
