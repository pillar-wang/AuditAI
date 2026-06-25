namespace Auditai.UI.LedgerView;

public enum TileFlag
{
	Group = 1,
	LocalFile = 2,
	WaitSend = 4,
	Sending = 8,
	Recieving = 0x10,
	DirectoryTitle = 0x20,
	OtherPositionButton = 0x40
}
