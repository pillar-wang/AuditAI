namespace Auditai.SignalR;

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
	PeerOpenTicketNavTreeNode
}
