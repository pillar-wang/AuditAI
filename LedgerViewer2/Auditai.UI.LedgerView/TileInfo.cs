namespace Auditai.UI.LedgerView;

public class TileInfo
{
	public TileFlag TileFlag { get; set; }

	public string LocalFile { get; set; }

	public string FileId { get; set; }

	public FileTransferProgressInfo ProgressInfo { get; set; } = new FileTransferProgressInfo();

}
