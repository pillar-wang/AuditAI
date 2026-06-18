using System.Reflection;

namespace Leqisoft.SignalR;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class UserState
{
	public string UserId { get; set; }

	public string ConnectionId { get; set; }

	public string ProjectId { get; set; }

	public string TeamId { get; set; }

	public string TreeNodeId { get; set; }

	public string TableCellId { get; set; }

	public string DocParagraphId { get; set; }

	public string TicketNavTreeNodePath { get; set; }
}
