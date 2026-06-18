﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using C1.C1Excel;

namespace Leqisoft.UI.Controls;

public static class ClipboardUtil
{
	private const uint CF_UNICODETEXT = 13u;

	private static readonly C1XLBook xb = new C1XLBook();

	private static readonly XNamespace xmlns_ss = "urn:schemas-microsoft-com:office:spreadsheet";

	private const int EVERY_TIME_COLS_MAX_COUNT = 50;

	private const int CONTINUE_EMPTY_ROWS_MAX_COUNT = 500;

	public static bool IsStreamReady { get; private set; }

	public static int RowsCount { get; private set; }

	public static int RowsCountAlreadyRead { get; private set; }

	[DllImport("user32", SetLastError = true)]
	private static extern IntPtr GetClipboardData(uint uFormat);

	[DllImport("user32", SetLastError = true)]
	private static extern bool OpenClipboard(IntPtr hWndNewOwner);

	[DllImport("user32", SetLastError = true)]
	private static extern bool CloseClipboard();

	[DllImport("user32", SetLastError = true)]
	private static extern uint EnumClipboardFormats(uint format);

	[DllImport("user32")]
	private static extern int GetClipboardFormatName(uint format, [Out] StringBuilder lpszFormatName, int cchMaxCount);

	[DllImport("user32")]
	private static extern IntPtr GetOpenClipboardWindow();

	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int count);

	[DllImport("kernel32")]
	private static extern IntPtr GlobalLock(IntPtr hMem);

	[DllImport("kernel32")]
	private static extern bool GlobalUnlock(IntPtr hMem);

	[DllImport("kernel32")]
	private static extern UIntPtr GlobalSize(IntPtr hMem);

	private static List<Tuple<uint, string>> GetClipboardFormats()
	{
		List<Tuple<uint, string>> list = new List<Tuple<uint, string>>();
		uint num = 0u;
		StringBuilder stringBuilder = new StringBuilder(1000);
		OpenClipboard(IntPtr.Zero);
		while ((num = EnumClipboardFormats(num)) != 0)
		{
			int clipboardFormatName = GetClipboardFormatName(num, stringBuilder, stringBuilder.Capacity);
			list.Add(Tuple.Create(num, stringBuilder.ToString().Substring(0, clipboardFormatName)));
		}
		CloseClipboard();
		return list;
	}

	public static bool ContainsXmlSpreadsheet()
	{
		return Clipboard.GetDataObject().GetFormats().Contains("XML Spreadsheet");
	}

	public static List<List<object>> GetClipboardAsTable()
	{
		List<Tuple<uint, string>> clipboardFormats = GetClipboardFormats();
		Tuple<uint, string> tuple = clipboardFormats.FirstOrDefault((Tuple<uint, string> f) => f.Item2 == "XML Spreadsheet");
		if (tuple != null)
		{
			try
			{
				return GetXmlSpreadsheetClipboard(tuple.Item1, ReadXmlStream);
			}
			catch (Exception exception)
			{
				exception.Log("解析剪贴板上的XML Spreadsheet内容失败");
			}
		}
		tuple = clipboardFormats.FirstOrDefault((Tuple<uint, string> f) => f.Item1 == 13);
		if (tuple != null)
		{
			return GetXmlSpreadsheetClipboard(tuple.Item1, ReadUnicodeStream);
		}
		return null;
	}

	private unsafe static List<List<object>> GetXmlSpreadsheetClipboard(uint format, Func<Stream, List<List<object>>> processStream)
	{
		IsStreamReady = false;
		RowsCount = 0;
		RowsCountAlreadyRead = 0;
		try
		{
			if (!OpenClipboard(IntPtr.Zero))
			{
				return null;
			}
			IntPtr clipboardData = GetClipboardData(format);
			if (clipboardData == IntPtr.Zero)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32Exception(lastWin32Error, "要复制到剪贴板的内容过大，请减少复制内容。");
			}
			try
			{
				IntPtr intPtr = GlobalLock(clipboardData);
				if (intPtr == IntPtr.Zero)
				{
					return null;
				}
				long num = (long)(ulong)GlobalSize(intPtr);
				if (num == 1)
				{
					int lastWin32Error2 = Marshal.GetLastWin32Error();
					throw new Win32Exception(lastWin32Error2, "读取剪贴板内容失败，可能是复制到剪贴板的内容过大，或者剪贴板上的数据不是符合Excel标准格式的数据");
				}
				using UnmanagedMemoryStream arg = new UnmanagedMemoryStream((byte*)(void*)intPtr, num);
				return processStream(arg);
			}
			finally
			{
				GlobalUnlock(clipboardData);
			}
		}
		finally
		{
			CloseClipboard();
		}
	}

	public static List<List<object>> ReadXmlStream(Stream strm)
	{
		using XmlReader xmlReader = XmlReader.Create(strm, new XmlReaderSettings
		{
			CheckCharacters = false
		});
		xmlReader.MoveToContent();
		xmlReader.ReadToDescendant("Worksheet", xmlns_ss.NamespaceName);
		xmlReader.ReadToDescendant("Table", xmlns_ss.NamespaceName);
		xmlReader.MoveToAttribute("ExpandedRowCount", xmlns_ss.NamespaceName);
		int val = (RowsCount = xmlReader.ReadContentAsInt());
		IsStreamReady = true;
		xmlReader.MoveToAttribute("ExpandedColumnCount", xmlns_ss.NamespaceName);
		int colCount = xmlReader.ReadContentAsInt();
		bool flag = colCount > 50;
		colCount = Math.Min(colCount, 50);
		if (colCount == 0)
		{
			return new List<List<object>>();
		}
		int val2 = 1000000;
		val = (RowsCount = Math.Min(val, val2));
		List<List<object>> list = (from _ in Enumerable.Range(0, val)
			select Enumerable.Repeat<object>(null, colCount).ToList()).ToList();
		int num3 = 0;
		xmlReader.MoveToElement();
		bool flag2 = xmlReader.ReadToDescendant("Row", xmlns_ss.NamespaceName);
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		while (flag2)
		{
			if (xmlReader.MoveToAttribute("Index", xmlns_ss.NamespaceName))
			{
				num3 = xmlReader.ReadContentAsInt();
				xmlReader.MoveToElement();
			}
			else
			{
				num3++;
			}
			RowsCountAlreadyRead = num3;
			int num7 = 0;
			bool flag3 = xmlReader.ReadToDescendant("Cell", xmlns_ss.NamespaceName);
			bool flag4 = true;
			while (flag3)
			{
				if (xmlReader.MoveToAttribute("Index", xmlns_ss.NamespaceName))
				{
					num7 = xmlReader.ReadContentAsInt();
					xmlReader.MoveToElement();
				}
				else
				{
					num7++;
				}
				if (num7 > colCount)
				{
					break;
				}
				if (xmlReader.ReadToDescendant("Data", xmlns_ss.NamespaceName))
				{
					XElement xElement = (XElement)XNode.ReadFrom(xmlReader);
					string type = (string)xElement.Attribute(xmlns_ss + "Type");
					string value = xElement.Value;
					bool isEmpty;
					object data = GetData(type, value, out isEmpty, isEraseTabChar: true);
					list[num3 - 1][num7 - 1] = data;
					if (!isEmpty)
					{
						flag4 = false;
						num5 = ((num7 > num5) ? num7 : num5);
					}
					while ((xmlReader.NodeType != XmlNodeType.EndElement || !(xmlReader.LocalName == "Cell")) && xmlReader.Read())
					{
					}
				}
				flag3 = xmlReader.ReadToNextSibling("Cell", xmlns_ss.NamespaceName);
			}
			flag2 = xmlReader.ReadToNextSibling("Row", xmlns_ss.NamespaceName);
			if (flag4)
			{
				num4++;
			}
			else
			{
				num4 = 0;
				num6 = num3;
			}
			if (num4 > 500 || num3 >= val)
			{
				break;
			}
		}
		if (list.Count > num6 && list.Count - num6 > 500)
		{
			list.RemoveRange(num6, list.Count - num6);
		}
		if (flag && colCount > num5)
		{
			for (int num8 = list.Count - 1; num8 >= 0; num8--)
			{
				list[num8].RemoveRange(num5, colCount - num5);
			}
			colCount = num5;
		}
		RowsCount = list.Count;
		return list;
	}

	private static List<List<object>> GetExcelClipboard()
	{
		if (!(Clipboard.GetData("Biff8") is MemoryStream stream))
		{
			return null;
		}
		xb.Load(stream, FileFormat.Biff8);
		XLSheet xLSheet = xb.Sheets[0];
		XLCellRange xLCellRange = xLSheet.SelectedCells[0];
		List<List<object>> list = new List<List<object>>(xLCellRange.RowCount);
		for (int i = xLCellRange.RowFrom; i <= xLCellRange.RowTo; i++)
		{
			List<object> list2 = new List<object>(xLCellRange.ColumnCount);
			list.Add(list2);
			for (int j = xLCellRange.ColumnFrom; j <= xLCellRange.ColumnTo; j++)
			{
				list2.Add(xLSheet[i, j].Value);
			}
		}
		return list;
	}

	public static List<List<object>> GetTextClipboard()
	{
		if (!Clipboard.ContainsText())
		{
			return null;
		}
		string text = Clipboard.GetText();
		if (text.EndsWith("\r\n"))
		{
			text = text.Substring(0, text.Length - 2);
		}
		List<string[]> list = (from r in text.Split(new string[1] { "\r\n" }, StringSplitOptions.None)
			select r.Split('\t')).ToList();
		List<List<object>> list2 = new List<List<object>>(list.Count);
		int num = list.Select((string[] r) => r.Length).Max();
		for (int i = 0; i < list.Count; i++)
		{
			string[] array = list[i];
			List<object> list3 = new List<object>(num);
			list2.Add(list3);
			for (int j = 0; j < num; j++)
			{
				if (j < array.Length)
				{
					list3.Add(GetValueFromPaste(array[j]));
				}
				else
				{
					list3.Add(string.Empty);
				}
			}
		}
		return list2;
	}

	public static List<List<object>> ReadUnicodeStream(Stream strm)
	{
		using StreamReader streamReader = new StreamReader(strm, Encoding.Unicode);
		string text = streamReader.ReadToEnd();
		text = text.TrimEnd(default(char));
		IsStreamReady = true;
		if (text.EndsWith("\r\n"))
		{
			text = text.Substring(0, text.Length - 2);
		}
		List<string[]> list = (from r in text.Split(new string[1] { "\r\n" }, StringSplitOptions.None)
			select r.Split('\t')).ToList();
		RowsCount = list.Count;
		List<List<object>> list2 = new List<List<object>>(list.Count);
		int num = list.Select((string[] r) => r.Length).Max();
		for (int i = 0; i < list.Count; i++)
		{
			RowsCountAlreadyRead = i;
			string[] array = list[i];
			List<object> list3 = new List<object>(num);
			list2.Add(list3);
			for (int j = 0; j < num; j++)
			{
				if (j < array.Length)
				{
					list3.Add(GetValueFromPaste(array[j]));
				}
				else
				{
					list3.Add(string.Empty);
				}
			}
		}
		return list2;
	}

	public static string GetValueFromPaste(string s)
	{
		if (s.Contains("\n") && s.StartsWith("\"") && s.EndsWith("\""))
		{
			s = s.Substring(1, s.Length - 2);
			return s.Replace("\"\"", "\"");
		}
		return s;
	}

	public static string GetClipboardWindowText()
	{
		IntPtr openClipboardWindow = GetOpenClipboardWindow();
		if (openClipboardWindow == IntPtr.Zero)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder(501);
		GetWindowText(openClipboardWindow, stringBuilder, 500);
		return stringBuilder.ToString();
	}

	public static void SpinWaitSetClipboardText(string text)
	{
		SpinWait.SpinUntil(() => GetClipboardWindowText() == null, 2000);
		Clipboard.SetText(text);
	}

	private static List<List<object>> GetXmlSpreadsheetClipboard()
	{
		if (!(Clipboard.GetData("XML Spreadsheet") is MemoryStream memoryStream))
		{
			return null;
		}
		memoryStream.SetLength(memoryStream.Length - 1);
		XDocument xDocument = XDocument.Load(memoryStream);
		XElement xElement = xDocument.Root.Element(xmlns_ss + "Worksheet");
		XElement xElement2 = xElement.Element(xmlns_ss + "Table");
		int count = (int)xElement2.Attribute(xmlns_ss + "ExpandedRowCount");
		int colCount = (int)xElement2.Attribute(xmlns_ss + "ExpandedColumnCount");
		List<List<object>> list = (from _ in Enumerable.Range(0, count)
			select Enumerable.Repeat<object>(null, colCount).ToList()).ToList();
		int num = 0;
		foreach (XElement item in xElement2.Elements(xmlns_ss + "Row"))
		{
			XAttribute xAttribute = item.Attribute(xmlns_ss + "Index");
			num = ((xAttribute != null) ? ((int)xAttribute) : (num + 1));
			List<object> list2 = list[num - 1];
			int num2 = 0;
			foreach (XElement item2 in item.Elements(xmlns_ss + "Cell"))
			{
				XAttribute xAttribute2 = item2.Attribute(xmlns_ss + "Index");
				num2 = ((xAttribute2 != null) ? ((int)xAttribute2) : (num2 + 1));
				XElement xElement3 = item2.Element(xmlns_ss + "Data");
				if (xElement3 != null)
				{
					list2[num2 - 1] = GetData((string)xElement3.Attribute(xmlns_ss + "Type"), xElement3.Value, out var _);
				}
			}
		}
		return list;
	}

	private static object GetData(string type, string str, out bool isEmpty, bool isEraseTabChar = false)
	{
		isEmpty = false;
		switch (type)
		{
		case "Number":
			return double.Parse(str);
		case "Boolean":
			return int.Parse(str) == 1;
		case "DateTime":
			return DateTime.Parse(str);
		default:
			isEmpty = str == null || str.Length == 0;
			if (isEraseTabChar && !isEmpty && str.IndexOf('\t') != -1)
			{
				str = str.Replace("\t", "");
			}
			return str;
		}
	}
}
