using System;
using System.Collections.Generic;
using Google.Protobuf;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class CellAttachments
{
	public bool Dirty;

	public List<CellAttachment> Attachments { get; } = new List<CellAttachment>();


	public SyncStatus Status { get; set; }

	public void Deserialize(byte[] bytes)
	{
		Attachments.Clear();
		Leqisoft.DTO.CellAttachments cellAttachments = Leqisoft.DTO.CellAttachments.Parser.ParseFrom(bytes);
		foreach (CellAttachmentEntry entry in cellAttachments.Entries)
		{
			Attachments.Add(new CellAttachment
			{
				FileId = new Guid(entry.Id.ToByteArray()),
				Name = entry.Name
			});
		}
	}

	public byte[] Serialize()
	{
		Leqisoft.DTO.CellAttachments cellAttachments = new Leqisoft.DTO.CellAttachments();
		foreach (CellAttachment attachment in Attachments)
		{
			cellAttachments.Entries.Add(new CellAttachmentEntry
			{
				Id = ByteString.CopyFrom(attachment.FileId.ToByteArray()),
				Name = attachment.Name
			});
		}
		return cellAttachments.ToByteArray();
	}

	public CellAttachments Clone()
	{
		CellAttachments cellAttachments = new CellAttachments();
		cellAttachments.Deserialize(Serialize());
		return cellAttachments;
	}
}
