using Auditai.Model;

namespace Auditai.UI.Platform;

public interface DocumentImportHandler
{
	DocumentEditor Import(string file, TreeDocumentNode treeDoc);
}
