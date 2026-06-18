using System;
using System.Collections.Generic;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class CellPropManager
{
	public Table Table { get; private set; }

	public Dictionary<Id64, CellAttachments> DicCellAttachments { get; } = new Dictionary<Id64, CellAttachments>();


	public CellPropManager(Table table)
	{
		Table = table;
	}

	public void AddAttachment(Cell cell, Guid fileId, string fileName)
	{
		if (!DicCellAttachments.TryGetValue(cell.Id, out var value))
		{
			value = new CellAttachments
			{
				Status = SyncStatus.New
			};
			DicCellAttachments.Add(cell.Id, value);
		}
		value.Attachments.Add(new CellAttachment
		{
			FileId = fileId,
			Name = fileName
		});
		value.Dirty = true;
		Table.NeedSave = true;
	}

	public void RemoveAllAttachment(Cell cell)
	{
		if (DicCellAttachments.TryGetValue(cell.Id, out var value) && value.Attachments.Count > 0)
		{
			value.Dirty = true;
			value.Attachments.Clear();
			Table.NeedSave = true;
		}
	}

	public void RemoveAttachment(Cell cell, CellAttachment attachment)
	{
		if (DicCellAttachments.TryGetValue(cell.Id, out var value))
		{
			value.Attachments.Remove(attachment);
			value.Dirty = true;
			Table.NeedSave = true;
		}
	}

	public void RemoveAttachmentAt(Cell cell, int i)
	{
		if (DicCellAttachments.TryGetValue(cell.Id, out var value))
		{
			value.Attachments.RemoveAt(i);
			value.Dirty = true;
			Table.NeedSave = true;
		}
	}

	public void RenameAttachmentAt(Cell cell, int i, string name)
	{
		if (DicCellAttachments.TryGetValue(cell.Id, out var value))
		{
			CellAttachment cellAttachment = value.Attachments[i];
			cellAttachment.Name = name;
			value.Dirty = true;
			Table.NeedSave = true;
		}
	}

	public bool TryGetAttachments(Cell cell, out CellAttachments attachments)
	{
		if (DicCellAttachments.TryGetValue(cell.Id, out attachments))
		{
			return attachments.Attachments.Count > 0;
		}
		return false;
	}

	public void SetSynced()
	{
		foreach (KeyValuePair<Id64, CellAttachments> dicCellAttachment in DicCellAttachments)
		{
			dicCellAttachment.Value.Status = SyncStatus.Synced;
			dicCellAttachment.Value.Dirty = false;
		}
	}

	public void UpdateAttachments(Cell cell, CellAttachments _new)
	{
		if (_new == null || _new.Attachments.Count == 0)
		{
			if (DicCellAttachments.TryGetValue(cell.Id, out var value))
			{
				value.Attachments.Clear();
				value.Dirty = true;
				Table.NeedSave = true;
			}
			return;
		}
		if (!DicCellAttachments.TryGetValue(cell.Id, out var value2))
		{
			value2 = new CellAttachments();
			DicCellAttachments.Add(cell.Id, value2);
		}
		value2.Deserialize(_new.Serialize());
		value2.Dirty = true;
		Table.NeedSave = true;
	}
}
