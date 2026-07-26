﻿﻿﻿using FileTransferModel;

namespace Auditai.SignalR;

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

	public string TableId { get; set; }

	public string DocumentId { get; set; }

	public string Version { get; set; }

	/// <summary>
	/// 节点级强锁：PeerTableLockChanged 广播时携带的 lockerUserId。
	/// "0" 表示锁已释放；其他值为持有锁的用户 Id。
	/// </summary>
	public string LockerUserId { get; set; }
}
