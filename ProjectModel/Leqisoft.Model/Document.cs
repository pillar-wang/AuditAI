using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;
using Leqisoft.DTO;
using Leqisoft.Model.Properties;
using Leqisoft.Util;

namespace Leqisoft.Model;

public class Document
{
	public bool _isLoaded;

	private static readonly Lazy<string> _fontTable;

	public static readonly XNamespace xmlns_a;

	public static readonly XNamespace xmlns_w;

	public static readonly XNamespace xmlns_r;

	public DocumentDirtyMask Dirty;

	public Project Project => TreeNode.Project;

	public TreeDocumentNode TreeNode { get; set; }

	public AutoIndexedCollection<Paragraph> Paragraphs { get; } = new AutoIndexedCollection<Paragraph>();


	public HashSet<Id64> RemovedParagraphs { get; } = new HashSet<Id64>();


	public HashSet<Id64> ParagraphsToDelete { get; } = new HashSet<Id64>();


	public string SectPr { get; set; }

	public Id64 Id => TreeNode.Id;

	public SyncStatus Status => TreeNode.Status;

	public int Version => TreeNode.Version;

	public long Locker { get; internal set; }

	public bool CanReload => Project.Dal.GetDocument(Id) != null;

	public bool IsCorrupted { get; private set; }

	public bool FromDuplicationButNotSaved { get; set; }

	public Id64 MergeTable { get; set; }

	public bool LocalExists => Version > -1;

	static Document()
	{
		_fontTable = new Lazy<string>(GetFontTable);
		xmlns_a = "http://schemas.openxmlformats.org/drawingml/2006/main";
		xmlns_w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
		xmlns_r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
	}

	public void UpdateLocker(long locker)
	{
		Locker = locker;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsLockerDirty = true;
		}
	}

	public void UpdateSectPr(string sectPr)
	{
		SectPr = sectPr;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsSectPrDirty = true;
		}
	}

	public void UpdateMergeTable(Id64 mt)
	{
		MergeTable = mt;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsMergeTableDirty = true;
		}
	}

	public void Save(IProgress<ProgressInfo> progress = null, TaskProgressValueUpdater taskProgressValueUpdater = null)
	{
		Project.Dal.BeginTransaction();
		progress?.Report(new ProgressInfo
		{
			MainCaption = "正在保存文档信息...",
			MainProgress = 60
		});
		Project.Dal.SaveDocument(ToDto());
		progress?.Report(new ProgressInfo
		{
			MainCaption = "正在保存段落信息...",
			MainProgress = 80
		});
		taskProgressValueUpdater?.UpdateProgress(10L, 100L);
		Project.Dal.SaveParagraphs(Paragraphs.Select((Paragraph p) => p.ToDto()));
		taskProgressValueUpdater?.UpdateProgress(60L, 100L);
		Project.Dal.RemoveParagraphs(RemovedParagraphs);
		taskProgressValueUpdater?.UpdateProgress(70L, 100L);
		Project.Dal.DeleteParagraphs(ParagraphsToDelete);
		taskProgressValueUpdater?.UpdateProgress(90L, 100L);
		Project.Dal.Commit();
		FromDuplicationButNotSaved = false;
		ParagraphsToDelete.Clear();
		progress?.Report(new ProgressInfo
		{
			MainProgress = 100
		});
		taskProgressValueUpdater?.UpdateProgress(100L, 100L);
	}

	public Document LoadAndReturn()
	{
		if (!_isLoaded)
		{
			_isLoaded = true;
			if (!LocalExists)
			{
				return this;
			}
			Leqisoft.DTO.Document document = Project.Dal.GetDocument(Id);
			if (document == null)
			{
				IsCorrupted = true;
				return this;
			}
			Locker = document.Locker;
			SectPr = document.SectPr;
			MergeTable = document.MergeTable;
			Dirty = new DocumentDirtyMask(document.Dirty);
			if (SectPr == null)
			{
				SectPr = Resource.DefaultSectPr;
			}
			foreach (Paragraph item in Project.Dal.GetParagraphs(Id).Select(Paragraph.FromDto))
			{
				item.Document = this;
				Paragraphs.Add(item);
			}
			if (Paragraphs.Count == 0)
			{
				return this;
			}
			foreach (Id64 localRemovedParagraph in Project.Dal.GetLocalRemovedParagraphs(Id))
			{
				RemovedParagraphs.Add(localRemovedParagraph);
			}
		}
		return this;
	}

	public void ReloadFromDb()
	{
		_isLoaded = false;
		Paragraphs.Clear();
		RemovedParagraphs.Clear();
		LoadAndReturn();
	}

	public Leqisoft.DTO.Document ToDto()
	{
		return new Leqisoft.DTO.Document
		{
			Id = Id,
			Version = Version,
			Locker = Locker,
			SectPr = SectPr,
			MergeTable = MergeTable,
			Dirty = Dirty.ToInt()
		};
	}

	public Tuple<string, List<DocumentLoadCellMerge>, List<Id64>> MakePackage(bool isExport = false)
	{
		string tempFileName = Path.GetTempFileName();
		List<DocumentLoadCellMerge> item = new List<DocumentLoadCellMerge>();
		List<Id64> item2 = new List<Id64>();
		XElement element = new XElement(xmlns_w + "tc", new XElement(xmlns_w + "tcPr", new XElement(xmlns_w + "hMerge", new XAttribute(xmlns_w + "val", "continue"))), new XElement(xmlns_w + "p"));
		using (Package package = Package.Open(tempFileName, FileMode.Create, FileAccess.ReadWrite))
		{
			package.CreateRelationship(new Uri("word/document.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");
			PackagePart packagePart = package.CreatePart(new Uri("/word/document.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml");
			packagePart.CreateRelationship(new Uri("styles.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles");
			PackagePart packagePart2 = package.CreatePart(new Uri("/word/styles.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml");
			using (Stream stream = packagePart2.GetStream())
			{
				using StreamWriter streamWriter = new StreamWriter(stream);
				streamWriter.Write(Resource.DefaultStyleXml);
			}
			PackagePart packagePart3 = package.CreatePart(new Uri("/word/fontTable.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.fontTable+xml");
			using (Stream stream2 = packagePart3.GetStream())
			{
				using StreamWriter streamWriter2 = new StreamWriter(stream2);
				streamWriter2.Write(_fontTable.Value);
			}
			XElement xElement = new XElement(xmlns_w + "document", new XAttribute(XNamespace.Xmlns + "w", xmlns_w.NamespaceName), new XAttribute(XNamespace.Xmlns + "r", xmlns_r.NamespaceName));
			XElement xElement2 = new XElement(xmlns_w + "body");
			xElement.Add(xElement2);
			int num = 0;
			XElement xElement10;
			XElement xElement13;
			for (int i = 0; i < Paragraphs.Count; i++)
			{
				Paragraph paragraph = Paragraphs[i];
				XElement xElement3 = XElement.Parse(paragraph.Stream);
				XElement xElement4 = xElement3.Descendants().FirstOrDefault((XElement e) => e.Name == xmlns_w + "bookmarkStart");
				if (xElement4 != null)
				{
					if (isExport)
					{
						xElement4.Remove();
					}
					else
					{
						xElement4.Attribute(xmlns_w + "id").SetValue(i);
					}
				}
				XElement xElement5 = xElement3.Descendants().FirstOrDefault((XElement e) => e.Name == xmlns_w + "bookmarkEnd");
				if (xElement5 != null)
				{
					if (isExport)
					{
						xElement5.Remove();
					}
					else
					{
						xElement5.Attribute(xmlns_w + "id").SetValue(i);
					}
				}
				foreach (XElement item3 in from e in xElement3.Descendants()
					where e.Name == xmlns_a + "blip" && !HasExceptionalAncestor(e)
					select e)
				{
					num++;
					XElement xElement6 = item3.Element("data");
					PackagePart packagePart4 = package.CreatePart(new Uri($"/word/media/{num}.bmp", UriKind.Relative), xElement6.Attribute("ContentType").Value);
					using (Stream stream3 = packagePart4.GetStream(FileMode.Create))
					{
						byte[] array = Convert.FromBase64String(xElement6.Value);
						stream3.Write(array, 0, array.Length);
					}
					packagePart.CreateRelationship(new Uri($"media/{num}.bmp", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image", $"rId{num}");
					item3.SetAttributeValue(xmlns_r + "embed", $"rId{num}");
					xElement6.Remove();
				}
				if (xElement3.Name.LocalName == "tbl")
				{
					List<int> list = (from e in xElement3.Element(xmlns_w + "tblGrid").Elements(xmlns_w + "gridCol")
						select (int)e.Attribute(xmlns_w + "w")).ToList();
					foreach (XElement item4 in xElement3.Elements(xmlns_w + "tr").ToList())
					{
						int? num2 = (int?)item4.Element(xmlns_w + "trPr")?.Element(xmlns_w + "gridAfter")?.Attribute(xmlns_w + "val");
						foreach (XElement item5 in item4.Elements(xmlns_w + "tc").ToList())
						{
							XElement xElement7 = item5.Element(xmlns_w + "tcPr");
							XElement xElement8 = xElement7.Element(xmlns_w + "gridSpan");
							if (xElement8 != null)
							{
								int num3 = (int)xElement8.Attribute(xmlns_w + "val");
								if (num3 > 1)
								{
									xElement8.Remove();
									xElement7.Add(new XElement(xmlns_w + "hMerge", new XAttribute(xmlns_w + "val", "restart")));
									item5.AddAfterSelf(Enumerable.Repeat(element, num3 - 1));
								}
							}
						}
						if (num2.HasValue)
						{
							item4.Add(Enumerable.Repeat(element, num2.Value));
						}
					}
				}
				xElement2.Add(xElement3);
				if (paragraph.Section == null)
				{
					continue;
				}
				XElement xElement9 = XElement.Parse(paragraph.Section);
				xElement9.Elements(xmlns_w + "headerReference").Skip(1).Remove();
				xElement10 = xElement9.Element(xmlns_w + "headerReference");
				if (xElement10 != null)
				{
					num++;
					packagePart.CreateRelationship(new Uri($"{num}.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/header", $"rId{num}");
					xElement10.SetAttributeValue(xmlns_r + "id", $"rId{num}");
					PackagePart packagePart5 = package.CreatePart(new Uri($"/word/{num}.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml");
					XElement xElement11 = xElement9.Element(xmlns_w + "hdr");
					foreach (XElement item6 in from e in xElement11.Descendants()
						where e.Name == xmlns_a + "blip"
						select e)
					{
						num++;
						XElement xElement12 = item6.Element("data");
						PackagePart packagePart6 = package.CreatePart(new Uri($"/word/media/{num}.bmp", UriKind.Relative), xElement12.Attribute("ContentType").Value);
						using (Stream stream4 = packagePart6.GetStream(FileMode.Create))
						{
							byte[] array2 = Convert.FromBase64String(xElement12.Value);
							stream4.Write(array2, 0, array2.Length);
						}
						packagePart5.CreateRelationship(new Uri($"media/{num}.bmp", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image", $"rId{num}");
						item6.SetAttributeValue(xmlns_r + "embed", $"rId{num}");
						xElement12.Remove();
					}
					xElement11.Save(packagePart5.GetStream());
					xElement11.Remove();
				}
				xElement9.Elements(xmlns_w + "footerReference").Skip(1).Remove();
				xElement13 = xElement9.Element(xmlns_w + "footerReference");
				if (xElement13 != null)
				{
					num++;
					packagePart.CreateRelationship(new Uri($"{num}.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer", $"rId{num}");
					xElement13.SetAttributeValue(xmlns_r + "id", $"rId{num}");
					PackagePart packagePart7 = package.CreatePart(new Uri($"/word/{num}.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml");
					XElement xElement14 = xElement9.Element(xmlns_w + "ftr");
					foreach (XElement item7 in from e in xElement14.Descendants()
						where e.Name == xmlns_a + "blip"
						select e)
					{
						num++;
						XElement xElement15 = item7.Element("data");
						PackagePart packagePart8 = package.CreatePart(new Uri($"/word/media/{num}.bmp", UriKind.Relative), xElement15.Attribute("ContentType").Value);
						using (Stream stream5 = packagePart8.GetStream(FileMode.Create))
						{
							byte[] array3 = Convert.FromBase64String(xElement15.Value);
							stream5.Write(array3, 0, array3.Length);
						}
						packagePart7.CreateRelationship(new Uri($"media/{num}.bmp", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image", $"rId{num}");
						item7.SetAttributeValue(xmlns_r + "embed", $"rId{num}");
						xElement15.Remove();
					}
					xElement14.Save(packagePart7.GetStream());
					xElement14.Remove();
				}
				XElement content = new XElement(xmlns_w + "p", new XAttribute(XNamespace.Xmlns + "w", xmlns_w.NamespaceName), new XElement(xmlns_w + "pPr", xElement9));
				xElement2.Add(content);
			}
			XElement xElement16 = XElement.Parse(SectPr);
			xElement2.Add(xElement16);
			if (isExport)
			{
				List<XElement> list2 = (from e in xElement2.Descendants()
					where e.Name.LocalName == "fldChar" || e.Name.LocalName == "instrText"
					select e).ToList();
				XElement xElement17 = null;
				XElement xElement18 = null;
				XElement xElement19 = null;
				bool flag = false;
				foreach (XElement item8 in list2)
				{
					if (item8.Name.LocalName == "fldChar" && (string)item8.Attribute(xmlns_w + "fldCharType") == "begin")
					{
						xElement17 = item8;
					}
					else if (item8.Name.LocalName == "instrText" && item8.Value.TrimStart().StartsWith("MERGEFIELD Formula", StringComparison.OrdinalIgnoreCase))
					{
						flag = true;
						xElement18 = item8;
					}
					else if (item8.Name.LocalName == "fldChar" && (string)item8.Attribute(xmlns_w + "fldCharType") == "separate")
					{
						xElement19 = item8;
					}
					else if (item8.Name.LocalName == "fldChar" && (string)item8.Attribute(xmlns_w + "fldCharType") == "end")
					{
						if (flag)
						{
							xElement17.Remove();
							xElement18.Remove();
							xElement19?.Remove();
							item8.Remove();
						}
						flag = false;
						xElement17 = null;
						xElement18 = null;
						xElement19 = null;
					}
				}
			}
			xElement16.Elements(xmlns_w + "headerReference").Skip(1).Remove();
			xElement10 = xElement16.Element(xmlns_w + "headerReference");
			if (xElement10 != null)
			{
				num++;
				packagePart.CreateRelationship(new Uri($"{num}.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/header", $"rId{num}");
				xElement10.SetAttributeValue(xmlns_r + "id", $"rId{num}");
				PackagePart packagePart9 = package.CreatePart(new Uri($"/word/{num}.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml");
				XElement xElement20 = xElement16.Element(xmlns_w + "hdr");
				foreach (XElement item9 in from e in xElement20.Descendants()
					where e.Name == xmlns_a + "blip"
					select e)
				{
					num++;
					XElement xElement21 = item9.Element("data");
					PackagePart packagePart10 = package.CreatePart(new Uri($"/word/media/{num}.bmp", UriKind.Relative), xElement21.Attribute("ContentType").Value);
					using (Stream stream6 = packagePart10.GetStream(FileMode.Create))
					{
						byte[] array4 = Convert.FromBase64String(xElement21.Value);
						stream6.Write(array4, 0, array4.Length);
					}
					packagePart9.CreateRelationship(new Uri($"media/{num}.bmp", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image", $"rId{num}");
					item9.SetAttributeValue(xmlns_r + "embed", $"rId{num}");
					xElement21.Remove();
				}
				xElement20.Save(packagePart9.GetStream());
				xElement20.Remove();
			}
			xElement16.Elements(xmlns_w + "footerReference").Skip(1).Remove();
			xElement13 = xElement16.Element(xmlns_w + "footerReference");
			if (xElement13 != null)
			{
				num++;
				packagePart.CreateRelationship(new Uri($"{num}.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer", $"rId{num}");
				xElement13.SetAttributeValue(xmlns_r + "id", $"rId{num}");
				PackagePart packagePart11 = package.CreatePart(new Uri($"/word/{num}.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml");
				XElement xElement22 = xElement16.Element(xmlns_w + "ftr");
				foreach (XElement item10 in from e in xElement22.Descendants()
					where e.Name == xmlns_a + "blip"
					select e)
				{
					num++;
					XElement xElement23 = item10.Element("data");
					PackagePart packagePart12 = package.CreatePart(new Uri($"/word/media/{num}.bmp", UriKind.Relative), xElement23.Attribute("ContentType").Value);
					using (Stream stream7 = packagePart12.GetStream(FileMode.Create))
					{
						byte[] array5 = Convert.FromBase64String(xElement23.Value);
						stream7.Write(array5, 0, array5.Length);
					}
					packagePart11.CreateRelationship(new Uri($"media/{num}.bmp", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image", $"rId{num}");
					item10.SetAttributeValue(xmlns_r + "embed", $"rId{num}");
					xElement23.Remove();
				}
				xElement22.Save(packagePart11.GetStream());
				xElement22.Remove();
			}
			xElement.Save(packagePart.GetStream());
		}
		return Tuple.Create(tempFileName, item, item2);
		static bool HasExceptionalAncestor(XElement xele)
		{
			return xele.Ancestors().Any((XElement x) => x.Name == xmlns_w + "hdr" || x.Name == xmlns_w + "ftr" || x.Name == xmlns_w + "sectPr");
		}
	}

	public void SetSynced()
	{
		TreeNode.IsEntityDirty = false;
		Dirty = default(DocumentDirtyMask);
		foreach (Paragraph paragraph in Paragraphs)
		{
			paragraph.SetSynced();
		}
		foreach (Id64 removedParagraph in RemovedParagraphs)
		{
			ParagraphsToDelete.Add(removedParagraph);
		}
		RemovedParagraphs.Clear();
	}

	public Document TemporaryClone()
	{
		Document document = new Document
		{
			TreeNode = TreeNode,
			_isLoaded = true,
			Locker = Locker,
			MergeTable = MergeTable,
			SectPr = SectPr
		};
		foreach (Paragraph paragraph2 in Paragraphs)
		{
			Paragraph paragraph = paragraph2.Clone();
			paragraph.Document = document;
			document.Paragraphs.Add(paragraph);
		}
		return document;
	}

	private static string GetFontTable()
	{
		XNamespace xNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
		using InstalledFontCollection installedFontCollection = new InstalledFontCollection();
		XElement xElement = new XElement(xNamespace + "fonts", new XAttribute(XNamespace.Xmlns + "w", xNamespace.NamespaceName));
		FontFamily[] families = installedFontCollection.Families;
		foreach (FontFamily fontFamily in families)
		{
			using (fontFamily)
			{
				try
				{
					using Font font = new Font(fontFamily, 9f);
					xElement.Add(new XElement(xNamespace + "font", new XAttribute(xNamespace + "name", fontFamily.Name), new XElement(xNamespace + "charset", new XAttribute(xNamespace + "val", font.GdiCharSet.ToString("x")))));
					font.FontFamily.Dispose();
				}
				catch (ArgumentException)
				{
				}
			}
		}
		return new XDocument(xElement).ToString();
	}
}
