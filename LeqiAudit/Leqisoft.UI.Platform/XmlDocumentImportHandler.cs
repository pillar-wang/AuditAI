﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Leqisoft.Model;
using Leqisoft.Model.Properties;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class XmlDocumentImportHandler : DocumentImportHandler
{
	private class ListLevel
	{
		public string NumFmt { get; set; }

		public string LvlText { get; set; }

		public string Font { get; set; }

		public string PStyle { get; set; }

		public int Level { get; set; }

		public int Start { get; set; }
	}

	private class ParagraphStyle
	{
		public int? NumId { get; set; }

		public string BasedOn { get; set; }

		public List<XElement> PPr { get; set; }

		public List<XElement> RPr { get; set; }
	}

	private static readonly XNamespace xmlns_w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

	private static readonly XNamespace xmlns_wps = "http://schemas.microsoft.com/office/word/2010/wordprocessingShape";

	private static readonly XNamespace xmlns_a = "http://schemas.openxmlformats.org/drawingml/2006/main";

	private static readonly XNamespace xmlns_pic = "http://schemas.openxmlformats.org/drawingml/2006/picture";

	private static readonly XNamespace xmlns_r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

	private static readonly XNamespace xmlns_wp = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing";

	private static readonly XNamespace xmlns_v = "urn:schemas-microsoft-com:vml";

	private const string rel_header = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/header";

	private const string rel_footer = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer";

	private static readonly XElement hMergeCont = new XElement(xmlns_w + "tc", new XElement(xmlns_w + "tcPr", new XElement(xmlns_w + "hMerge", new XAttribute(xmlns_w + "val", "continue"))), new XElement(xmlns_w + "p"));

	private readonly Dictionary<string, ParagraphStyle> _dicPStyles = new Dictionary<string, ParagraphStyle>();

	private string _defaultStyleId;

	public DocumentEditor Import(string file, TreeDocumentNode treeDoc)
	{
		_dicPStyles.Clear();
		string text = Path.GetExtension(file).ToLowerInvariant();
		if (text != ".docx")
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "只支持导入.docx文件");
		}
		string tempFileName = Path.GetTempFileName();
		File.Copy(file, tempFileName, overwrite: true);
		using (Package package = Package.Open(tempFileName))
		{
			GetParagraphStyles(package);
			PackagePart part = package.GetPart(new Uri("/word/document.xml", UriKind.Relative));
			using (Stream stream = part.GetStream())
			{
				XDocument xDocument = XDocument.Load(stream);
				FixNumbering(package, xDocument);
				FixGb2312FontPart(xDocument);
				FixPageBreak(xDocument);
				FixHMerge(xDocument);
				FixBookmarks(xDocument);
				FixFields(xDocument);
				FixImagesPart(part, xDocument);
				FixParagraphStylesPart(xDocument);
				RemoveVImageDataElementPart(xDocument);
				stream.SetLength(0L);
				using (var writer = System.Xml.XmlWriter.Create(stream, new System.Xml.XmlWriterSettings { OmitXmlDeclaration = true }))
					xDocument.Save(writer);
			}
			foreach (PackageRelationship item in part.GetRelationshipsByType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/header"))
			{
				Uri partUri = new Uri($"/word/{item.TargetUri}", UriKind.Relative);
				PackagePart part2 = package.GetPart(partUri);
				using Stream stream2 = part2.GetStream();
				XDocument xDocument2 = XDocument.Load(stream2);
				FixGb2312FontPart(xDocument2);
				FixFooterTextboxPart(xDocument2);
				FixImagesPart(part2, xDocument2);
				FixParagraphStylesPart(xDocument2);
				RemoveVImageDataElementPart(xDocument2);
				stream2.SetLength(0L);
				using (var writer = System.Xml.XmlWriter.Create(stream2, new System.Xml.XmlWriterSettings { OmitXmlDeclaration = true }))
					xDocument2.Save(writer);
			}
			foreach (PackageRelationship item2 in part.GetRelationshipsByType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer"))
			{
				Uri partUri2 = new Uri($"/word/{item2.TargetUri}", UriKind.Relative);
				PackagePart part3 = package.GetPart(partUri2);
				using Stream stream3 = part3.GetStream();
				XDocument xDocument3 = XDocument.Load(stream3);
				FixGb2312FontPart(xDocument3);
				FixFooterTextboxPart(xDocument3);
				FixImagesPart(part3, xDocument3);
				FixParagraphStylesPart(xDocument3);
				RemoveVImageDataElementPart(xDocument3);
				stream3.SetLength(0L);
				using (var writer = System.Xml.XmlWriter.Create(stream3, new System.Xml.XmlWriterSettings { OmitXmlDeclaration = true }))
					xDocument3.Save(writer);
			}
		}
		DocumentEditor documentEditor = new DocumentEditor
		{
			Document = treeDoc.Document
		};
		documentEditor.PopulateDocument();
		documentEditor.Import(tempFileName);
		File.Delete(tempFileName);
		return documentEditor;
	}

	private void GetParagraphStyles(Package pkg)
	{
		Uri partUri = new Uri("/word/styles.xml", UriKind.Relative);
		PackagePart part = pkg.GetPart(partUri);
		using Stream stream = part.GetStream();
		XElement xElement = XElement.Load(System.Xml.XmlReader.Create(stream));
		foreach (XElement item in (from ele in xElement.Elements(xmlns_w + "style")
			where (string)ele.Attribute(xmlns_w + "type") == "paragraph"
			select ele).ToList())
		{
			string text = (string)item.Attribute(xmlns_w + "styleId");
			if ((string)item.Attribute(xmlns_w + "default") == "1")
			{
				_defaultStyleId = text;
			}
			ParagraphStyle paragraphStyle = new ParagraphStyle();
			paragraphStyle.BasedOn = (string)item.Element(xmlns_w + "basedOn")?.Attribute(xmlns_w + "val");
			paragraphStyle.RPr = item.Element(xmlns_w + "rPr")?.Elements()?.ToList();
			XElement xElement2 = item.Element(xmlns_w + "pPr");
			if (xElement2 != null)
			{
				XElement xElement3 = xElement2.Element(xmlns_w + "numPr");
				if (xElement3 != null)
				{
					int value = (int)xElement3.Element(xmlns_w + "numId").Attribute(xmlns_w + "val");
					paragraphStyle.NumId = value;
					xElement3.Remove();
				}
			}
			paragraphStyle.PPr = xElement2?.Elements()?.ToList();
			_dicPStyles.Add(text, paragraphStyle);
		}
		stream.SetLength(0L);
		using StreamWriter streamWriter = new StreamWriter(stream);
		streamWriter.Write(Resource.DefaultStyleXml);
	}

	private void FixParagraphStylesPart(XDocument root)
	{
		foreach (XElement item in root.Descendants(xmlns_w + "p").ToList())
		{
			XElement xElement = item.Element(xmlns_w + "pPr");
			if (xElement == null)
			{
				continue;
			}
			XElement xElement2 = xElement.Element(xmlns_w + "pStyle");
			string text;
			if (xElement2 != null)
			{
				xElement2.Remove();
				text = (string)xElement2.Attribute(xmlns_w + "val");
			}
			else
			{
				text = _defaultStyleId;
			}
			while (text != null)
			{
				if (_dicPStyles.TryGetValue(text, out var value))
				{
					if (value.PPr != null)
					{
						foreach (XElement item2 in value.PPr)
						{
							XElement xElement3 = xElement.Element(item2.Name);
							if (xElement3 == null)
							{
								xElement.Add(item2);
								continue;
							}
							foreach (XAttribute item3 in item2.Attributes())
							{
								XAttribute xAttribute = xElement3.Attribute(item3.Name);
								if (xAttribute == null)
								{
									xElement3.Add(item3);
								}
							}
						}
					}
					if (value.RPr != null)
					{
						foreach (XElement item4 in value.RPr)
						{
							foreach (XElement item5 in item.Descendants(xmlns_w + "r").ToList())
							{
								XElement xElement4 = item5.Element(xmlns_w + "rPr");
								if (xElement4 == null)
								{
									xElement4 = new XElement(xmlns_w + "rPr");
									item5.AddFirst(xElement4);
								}
								XElement xElement5 = xElement4.Element(item4.Name);
								if (xElement5 == null)
								{
									xElement4.Add(item4);
								}
								else
								{
									if (!(xElement5.Name.LocalName != "b") || !(xElement5.Name.LocalName != "i"))
									{
										continue;
									}
									foreach (XAttribute item6 in item4.Attributes())
									{
										XAttribute xAttribute2 = xElement5.Attribute(item6.Name);
										if (xAttribute2 == null)
										{
											xElement5.Add(item6);
										}
									}
								}
							}
						}
					}
					text = value.BasedOn;
				}
				else
				{
					text = null;
				}
			}
		}
	}

	private void FixNumbering(Package pkg, XDocument root)
	{
		Uri partUri = new Uri("/word/numbering.xml", UriKind.Relative);
		if (!pkg.PartExists(partUri))
		{
			return;
		}
		Dictionary<int, List<ListLevel>> dictionary = new Dictionary<int, List<ListLevel>>();
		PackagePart part = pkg.GetPart(partUri);
		using (Stream stream = part.GetStream())
		{
			XElement xElement = XElement.Load(System.Xml.XmlReader.Create(stream));
			Dictionary<int, Dictionary<int, ListLevel>> dictionary2 = new Dictionary<int, Dictionary<int, ListLevel>>();
			foreach (XElement item in xElement.Elements(xmlns_w + "abstractNum"))
			{
				int key = (int)item.Attribute(xmlns_w + "abstractNumId");
				Dictionary<int, ListLevel> dictionary3 = new Dictionary<int, ListLevel>();
				dictionary2.Add(key, dictionary3);
				foreach (XElement item2 in item.Elements(xmlns_w + "lvl"))
				{
					int num = (int)item2.Attribute(xmlns_w + "ilvl");
					dictionary3.Add(num, new ListLevel
					{
						NumFmt = (string)item2.Element(xmlns_w + "numFmt").Attribute(xmlns_w + "val"),
						LvlText = (string)item2.Element(xmlns_w + "lvlText").Attribute(xmlns_w + "val"),
						Font = (string)item2.Element(xmlns_w + "rPr")?.Element(xmlns_w + "rFonts")?.Attribute(xmlns_w + "ascii"),
						PStyle = (string)item2.Element(xmlns_w + "pStyle")?.Attribute(xmlns_w + "val"),
						Level = num,
						Start = ((int?)item2.Element(xmlns_w + "start")?.Attribute(xmlns_w + "val")).GetValueOrDefault(1)
					});
				}
			}
			foreach (XElement item3 in xElement.Elements(xmlns_w + "num"))
			{
				int key2 = (int)item3.Element(xmlns_w + "abstractNumId").Attribute(xmlns_w + "val");
				List<ListLevel> value = (from kv in dictionary2[key2]
					orderby kv.Key
					select kv.Value).ToList();
				dictionary.Add((int)item3.Attribute(xmlns_w + "numId"), value);
			}
		}
		Dictionary<int, List<int>> dictionary4 = dictionary.ToDictionary((KeyValuePair<int, List<ListLevel>> kv) => kv.Key, (KeyValuePair<int, List<ListLevel>> kv) => kv.Value.Select((ListLevel ll) => 0).ToList());
		foreach (XElement item4 in root.Root.Element(xmlns_w + "body").Descendants(xmlns_w + "p"))
		{
			XElement xElement2 = item4.Element(xmlns_w + "pPr");
			if (xElement2 == null)
			{
				continue;
			}
			ListLevel listLevel = null;
			int num2 = 0;
			XElement xElement3 = xElement2.Element(xmlns_w + "numPr");
			if (xElement3 != null)
			{
				XElement xElement4 = xElement3.Element(xmlns_w + "ilvl");
				if (xElement4 == null)
				{
					continue;
				}
				int index = (int)xElement4.Attribute(xmlns_w + "val");
				num2 = (int)xElement3.Element(xmlns_w + "numId").Attribute(xmlns_w + "val");
				if (num2 == 0)
				{
					continue;
				}
				listLevel = dictionary[num2][index];
				xElement3.Remove();
			}
			else
			{
				XElement xElement5 = xElement2.Element(xmlns_w + "pStyle");
				if (xElement5 != null)
				{
					string pStyle = (string)xElement5.Attribute(xmlns_w + "val");
					if (_dicPStyles.TryGetValue(pStyle, out var value2) && value2.NumId.HasValue)
					{
						num2 = value2.NumId.Value;
						if (dictionary.TryGetValue(value2.NumId.Value, out var value3))
						{
							listLevel = value3.FirstOrDefault((ListLevel l) => l.PStyle == pStyle);
						}
					}
				}
			}
			if (listLevel != null)
			{
				List<int> list = dictionary4[num2];
				for (int i = listLevel.Level + 1; i < list.Count; i++)
				{
					list[i] = 0;
				}
				if (list[listLevel.Level] == 0)
				{
					list[listLevel.Level] = listLevel.Start;
				}
				else
				{
					list[listLevel.Level]++;
				}
				string text = ((!(listLevel.NumFmt == "bullet")) ? listLevel.LvlText.Replace($"%{listLevel.Level + 1}", GetNumberString(listLevel.NumFmt, list[listLevel.Level])) : ((!listLevel.Font.Equals("Wingdings", StringComparison.InvariantCultureIgnoreCase)) ? listLevel.LvlText : NumberingHelper.WingdingsToUnicode(listLevel.LvlText)));
				XElement xElement6 = item4.Element(xmlns_w + "r")?.Element(xmlns_w + "t");
				xElement6?.SetValue(text + xElement6.Value);
			}
		}
	}

	private static void FixGb2312FontPart(XDocument xd)
	{
		foreach (XElement item in xd.Root.Descendants(xmlns_w + "rFonts"))
		{
			FixRFontAttribute(item.Attribute(xmlns_w + "ascii"));
			FixRFontAttribute(item.Attribute(xmlns_w + "hAnsi"));
			FixRFontAttribute(item.Attribute(xmlns_w + "eastAsia"));
			FixRFontAttribute(item.Attribute(xmlns_w + "cs"));
		}
	}

	private static void FixRFontAttribute(XAttribute xa)
	{
		if (xa != null)
		{
			if (xa.Value == "楷体_GB2312")
			{
				xa.Value = "楷体";
			}
			else if (xa.Value == "仿宋_GB2312")
			{
				xa.Value = "仿宋";
			}
		}
	}

	private static void FixPageBreak(XDocument xd)
	{
		foreach (XElement item in from ele in xd.Root.Descendants(xmlns_w + "br")
			where (string)ele.Attribute(xmlns_w + "type") == "page"
			select ele)
		{
			XElement xElement = item.Ancestors(xmlns_w + "p").FirstOrDefault();
			XElement xElement2 = xElement.Element(xmlns_w + "pPr");
			if (xElement2 == null)
			{
				xElement2 = new XElement(xmlns_w + "pPr");
				xElement.Add(xElement2);
			}
			XElement xElement3 = xElement2.Element(xmlns_w + "jc");
			if (xElement3 == null)
			{
				xElement3 = new XElement(xmlns_w + "jc");
				xElement2.Add(xElement3);
			}
			XAttribute xAttribute = xElement3.Attribute(xmlns_w + "val");
			if (xAttribute == null)
			{
				xAttribute = new XAttribute(xmlns_w + "val", "start");
				xElement3.Add(xAttribute);
			}
			if ((string)xAttribute == "both")
			{
				xAttribute.SetValue("start");
			}
		}
	}

	private static void FixFooterTextboxPart(XDocument xd)
	{
		foreach (XElement item in xd.Root.Descendants(xmlns_wps + "txbx").ToList())
		{
			if (item.Parent.Element(xmlns_wps + "bodyPr")?.Element(xmlns_a + "spAutoFit") == null)
			{
				continue;
			}
			XElement xElement = item.Ancestors(xmlns_w + "r").FirstOrDefault();
			XElement xElement2 = xElement.Ancestors(xmlns_w + "p").FirstOrDefault();
			string text = item.Ancestors(xmlns_wp + "anchor").FirstOrDefault()?.Element(xmlns_wp + "positionH")?.Element(xmlns_wp + "align")?.Value;
			string text2 = null;
			switch (text)
			{
			case "left":
				text2 = "start";
				break;
			case "center":
				text2 = "center";
				break;
			case "right":
				text2 = "end";
				break;
			}
			List<XElement> list = item.Descendants(xmlns_w + "p").ToList();
			if (text2 != null)
			{
				foreach (XElement item2 in list)
				{
					XElement xElement3 = item2.Element(xmlns_w + "pPr");
					if (xElement3 == null)
					{
						xElement3 = new XElement(xmlns_w + "pPr");
						item2.Add(xElement3);
					}
					XElement xElement4 = xElement3.Element(xmlns_w + "jc");
					if (xElement4 == null)
					{
						xElement4 = new XElement(xmlns_w + "jc");
					}
					xElement4.SetValue(text2);
				}
			}
			xElement2.AddAfterSelf(list);
			xElement.Remove();
		}
	}

	private static void FixHMerge(XDocument xd)
	{
		foreach (XElement item in xd.Root.Descendants(xmlns_w + "tbl"))
		{
			List<int> list = (from e in item.Element(xmlns_w + "tblGrid").Elements(xmlns_w + "gridCol")
				select (int)e.Attribute(xmlns_w + "w")).ToList();
			foreach (XElement item2 in item.Elements(xmlns_w + "tr").ToList())
			{
				foreach (XElement item3 in item2.Elements(xmlns_w + "tc").ToList())
				{
					XElement xElement = item3.Element(xmlns_w + "tcPr");
					XElement xElement2 = xElement.Element(xmlns_w + "gridSpan");
					if (xElement2 != null)
					{
						int num = (int)xElement2.Attribute(xmlns_w + "val");
						if (num > 1)
						{
							xElement2.Remove();
							xElement.Add(new XElement(xmlns_w + "hMerge", new XAttribute(xmlns_w + "val", "restart")));
							item3.AddAfterSelf(Enumerable.Repeat(hMergeCont, num - 1));
						}
					}
				}
			}
		}
	}

	private static void FixBookmarks(XDocument xd)
	{
		xd.Root.Descendants(xmlns_w + "bookmarkStart").Remove();
		xd.Root.Descendants(xmlns_w + "bookmarkEnd").Remove();
	}

	private static void FixFields(XDocument xd)
	{
		xd.Root.Descendants(xmlns_w + "fldChar").Remove();
		xd.Root.Descendants(xmlns_w + "instrText").Remove();
	}

	private static void FixImagesPart(PackagePart part, XDocument xd)
	{
		foreach (XElement item in xd.Root.Descendants(xmlns_pic + "pic").ToList())
		{
			string text = item.Element(xmlns_pic + "blipFill")?.Element(xmlns_a + "blip")?.Attribute(xmlns_r + "link")?.Value;
			if (text != null && part.GetRelationship(text).TargetMode == TargetMode.External)
			{
				part.DeleteRelationship(text);
				item.Ancestors(xmlns_w + "drawing").FirstOrDefault().Remove();
			}
		}
	}

	private static void RemoveVImageDataElementPart(XDocument xd)
	{
		xd.Root.Descendants(xmlns_v + "imagedata").Remove();
	}

	private static string GetNumberString(string numFmt, int number)
	{
		switch (numFmt)
		{
		case "chineseCountingThousand":
		case "chineseCounting":
		case "japaneseCounting":
		case "taiwaneseCounting":
		case "japaneseDigitalTenThousand":
		case "taiwaneseCountingThousand":
		case "japaneseLegal":
			return NumberingHelper.NumToChinese(number);
		case "decimalFullWidth":
		case "decimalFullWidth2":
			return NumberingHelper.NumFullwidth(number);
		case "upperRoman":
			return NumberingHelper.NumToUpperRoman(number);
		case "lowerRoman":
			return NumberingHelper.NumToUpperRoman(number).ToLowerInvariant();
		case "upperLetter":
			return char.ConvertFromUtf32(65 + number % 26 - 1);
		case "lowerLetter":
			return char.ConvertFromUtf32(97 + number % 26 - 1);
		case "hex":
			return number.ToString("X");
		case "decimalEnclosedCircle":
			return NumberingHelper.NumEncircled(number);
		case "decimalEnclosedFullstop":
			return NumberingHelper.NumFullstop(number);
		case "decimalEnclosedParen":
			return NumberingHelper.NumEnclosedParen(number);
		case "ideographEnclosedCircle":
		case "decimalEnclosedCircleChinese":
			return NumberingHelper.NumEncircledChinese(number);
		case "ideographTraditional":
			return NumberingHelper.NumIdeographTraditional(number);
		case "ideographZodiac":
			return NumberingHelper.NumIdeographZodiac(number);
		case "ideographZodiacTraditional":
			return NumberingHelper.NumIdeographZodiacTraditional(number);
		case "numberInDash":
			return $"- {number} -";
		default:
			return number.ToString();
		}
	}
}
