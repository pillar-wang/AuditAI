using FileTransferModel;

namespace Leqisoft.SignalR;

public class MessageReceivedEventArgs
{
	public MessageKind Kind { get; set; }

	public string FromId { get; set; }

	public string Message { get; set; }

	public string ProjectId { get; set; }

	public string NodeId { get; set; }

	public string TableCellId { get; set; }

	public string ParagraphId { get; set; }

	public FileInfo FileInfo { get; set; }

	public FileSection FileSection { get; set; }

	public string TicketNavTreeNodePath { get; set; }
}
