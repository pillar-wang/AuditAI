using System;

namespace FileTransferModel;

public class TranferCompleteEventArgs : EventArgs
{
	public string Id { get; set; }

	public string FileName { get; set; }

	public byte[] Contents { get; set; }

	public TranferCompleteEventArgs(string id, string filename, byte[] contents)
	{
		Id = id;
		FileName = filename;
		Contents = contents;
	}
}
