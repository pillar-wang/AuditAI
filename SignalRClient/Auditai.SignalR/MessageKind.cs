﻿﻿﻿namespace Auditai.SignalR;

public enum MessageKind
{
	Unknown,
	PeerLogin,
	PeerLogout,
	MessageFromUser,
	LoginInit,
	PeerOpensProject,
	PeerOpensTreeNode,
	PeerTableCellChange,
	PeerParagraphChange,
	ProjectBroadcast,
	TeamBroadcast,
	ProjectSynced,
	PeerPushesTreeNode,
	PeerMemberInfoChanged,
	PeerTeamMembersChanged,
	PeerProjectMembersChanged,
	PeerStateUpload,
	PeerFileSectionArrived,
	PeerOpenTicketNavTreeNode,
	PeerTableChanged,
	PeerDocumentChanged,
	// 节点级强锁：其他客户端获取/释放表格锁时广播。
	// 参数: projectId, tableId, lockerUserId（"0" 表示释放）
	PeerTableLockChanged
}
