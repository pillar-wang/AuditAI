namespace FileTransferModel;

public enum FileState
{
	SendWaitAccept,
	Sending,
	SendComplete,
	SendTimeout,
	Recieving,
	RecieveComplete,
	RecieveTimeout,
	SendCancel,
	RecieveCancel
}
