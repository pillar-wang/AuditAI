namespace Leqisoft.UI.Platform;

public class HandlerContext
{
	public DocumentImportHandler docHandler { get; set; }

	public HandlerContext()
	{
		docHandler = new XmlDocumentImportHandler();
	}
}
