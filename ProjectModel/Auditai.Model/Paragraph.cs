﻿using System.Linq;
using System.Text;
using System.Xml.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class Paragraph : IIndexable
{
	public ParagraphDirtyMask Dirty;

	public Id64 Id { get; set; }

	public SyncStatus Status { get; set; }

	public Document Document { get; set; }

	public int Index { get; set; }

	public string Stream { get; set; }

	public int ServerIndex { get; set; }

	public string Section { get; set; }

	public string Comment { get; set; }

	public bool IsIndexDirty => ServerIndex != Index;

	public void UpdateStream(string stream, string section)
	{
		Stream = stream;
		Section = section;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsStreamDirty = true;
		}
	}

	public void UpdateComment(string comment)
	{
		Comment = comment;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsCommentDirty = true;
		}
	}

	public static Paragraph FromDto(Auditai.DTO.Paragraph dto)
	{
		var stream = Encoding.UTF8.GetString(ZipCompressor.Decompress(dto.Stream));
		// 将旧格式书签 lsbm@ 转为 lsbm_（OOXML 安全，防止 @ 被截断导致 TableId 丢失）
		if (stream.Contains("lsbm@"))
			stream = UpgradeBookmarkFormat(stream);
		return new Paragraph
		{
			Id = dto.Id,
			Dirty = new ParagraphDirtyMask(dto.Dirty),
			Index = dto.Index,
			Stream = stream,
			Status = (SyncStatus)dto.Status,
			ServerIndex = dto.ServerIndex,
			Section = ((dto.Section == null) ? null : Encoding.UTF8.GetString(ZipCompressor.Decompress(dto.Section))),
			Comment = dto.Comment
		};
	}

	/// <summary>
	/// 将 Paragraph Stream 中的旧格式书签 lsbm@ 转为 lsbm_（OOXML 安全）。
	/// 旧格式：lsbm@0{ParaId}@1{Status}@2{TableId}@4{TableStyle}
	/// 新格式：lsbm_0_{ParaId}_1_{Status}_2_{TableId}_4_{TableStyle}
	/// </summary>
	private static string UpgradeBookmarkFormat(string stream)
	{
		try
		{
			var xElement = XElement.Parse(stream);
			var ns = xElement.GetNamespaceOfPrefix("w");
			bool changed = false;
			foreach (var bm in xElement.Descendants(ns + "bookmarkStart"))
			{
				var nameAttr = bm.Attribute(ns + "name");
				if (nameAttr != null && nameAttr.Value.StartsWith("lsbm@"))
				{
					nameAttr.SetValue(nameAttr.Value.Replace("@", "_"));
					changed = true;
				}
			}
			return changed ? xElement.ToString() : stream;
		}
		catch
		{
			return stream;
		}
	}

	public Auditai.DTO.Paragraph ToDto()
	{
		return new Auditai.DTO.Paragraph
		{
			Id = Id,
			DocumentId = Document.Id,
			Index = Index,
			Stream = ZipCompressor.Compress(Encoding.UTF8.GetBytes(Stream)),
			Dirty = Dirty.ToInt(),
			Status = (int)Status,
			ServerIndex = ServerIndex,
			Section = ((Section == null) ? null : ZipCompressor.Compress(Encoding.UTF8.GetBytes(Section))),
			Comment = Comment
		};
	}

	public void SetSynced()
	{
		Status = SyncStatus.Synced;
		Dirty = default(ParagraphDirtyMask);
		ServerIndex = Index;
	}

	internal Paragraph Duplicate()
	{
		Paragraph paragraph = new Paragraph();
		paragraph.Dirty = default(ParagraphDirtyMask);
		paragraph.Id = Project.Current.GetNextId();
		paragraph.Status = SyncStatus.New;
		paragraph.Stream = ReplaceIdInStream(Stream, paragraph.Id);
		paragraph.Comment = Comment;
		return paragraph;
	}

	private string ReplaceIdInStream(string stream, Id64 replaceWith)
	{
		XElement xElement = XElement.Parse(stream);
		XNamespace namespaceOfPrefix = xElement.GetNamespaceOfPrefix("w");
		XElement xElement2 = xElement.Descendants(namespaceOfPrefix + "bookmarkStart").FirstOrDefault();
		if (xElement2 != null)
		{
			string text = (string)xElement2.Attribute(namespaceOfPrefix + "name");
			if (text != null && AuditaiBookmark.TryParse(text, out var lsbm))
			{
				lsbm.ParaIdBase64 = replaceWith.ToBase64();
				xElement2.SetAttributeValue(namespaceOfPrefix + "name", lsbm.GetString());
				return xElement.ToString();
			}
		}
		return stream;
	}

	internal void RemoveBookmark()
	{
		XElement xElement = XElement.Parse(Stream);
		XNamespace namespaceOfPrefix = xElement.GetNamespaceOfPrefix("w");
		xElement.Descendants(namespaceOfPrefix + "bookmarkStart").FirstOrDefault()?.Remove();
		xElement.Descendants(namespaceOfPrefix + "bookmarkEnd").FirstOrDefault()?.Remove();
		Stream = xElement.ToString();
	}

	internal Paragraph Clone()
	{
		return (Paragraph)MemberwiseClone();
	}
}
