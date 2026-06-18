using System;
using System.Collections.Generic;

namespace FileTransferModel;

public class FileCache
{
	public string SendUserId { get; set; }

	public string RecieveUserId { get; set; }

	public string LocalFile { get; set; }

	public FileInfo FileInfo { get; set; }

	public DateTime TransferStart { get; set; }

	public FileState FileState { get; set; }

	public List<FileSection> RecievedSections { get; set; }
}
