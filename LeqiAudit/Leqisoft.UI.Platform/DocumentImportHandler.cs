using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public interface DocumentImportHandler
{
	DocumentEditor Import(string file, TreeDocumentNode treeDoc);
}
