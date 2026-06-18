using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class DefaultDocumentImportHandler : DocumentImportHandler
{
	private delegate DocumentEditor InvokeDelegate();

	public DocumentEditor Import(string file, TreeDocumentNode treeDoc)
	{
		return (DocumentEditor)Program.MainForm.View.Invoke((InvokeDelegate)delegate
		{
			ImportWord importWord = new ImportWord();
			return importWord.Import(file, treeDoc.Document);
		});
	}
}
