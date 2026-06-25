using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using C1.Win.C1Editor;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class C1EditorRecordEditor : IRecordEditor
{
	private const string TEMPLETE_ROOT = "<html><body id='rootbody'></body></html>";

	private const string TEMPLETE_RECORD_SELF = "<p align='right' style='color:rgb(40,131,243);line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{0}({1})</p><p align='right' style='line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{2}</p>";

	private const string TEMPLETE_RECORD_Member = "<p align='left' style='color:rgb(230,108,33);line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{0}({1})</p><p align='left' style='line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{2}</p>";

	private C1Editor _editor;

	private long _myId => User.Current.Id;

	public event EventHandler PreviousRecord;

	public C1EditorRecordEditor()
	{
		_editor = new C1Editor
		{
			ReadOnly = true,
			Dock = DockStyle.Fill,
			BackColor = Color.White,
			BorderStyle = BorderStyle.None
		};
		_editor.LoadXml("<html><body id='rootbody'></body></html>", null);
		_editor.ReadOnly = true;
	}

	public void EmptyView()
	{
		XmlElement elementById = _editor.Document.GetElementById("rootbody");
		elementById.InnerXml = string.Empty;
	}

	public void Append(ChatRecord record)
	{
		if (record == null)
		{
			return;
		}
		Member member = MemberManager.GetInstance().GetMember(record.FromId);
		if (member == null)
		{
			return;
		}
		XmlElement elementById = _editor.Document.GetElementById("rootbody");
		XmlNode xmlNode = _editor.Document.CreateNode("element", "div", "");
		string arg = DealEmoji(ToXml(record.Message));
		if (record.FromId != _myId.ToString())
		{
			try
			{
				xmlNode.InnerXml = $"<p align='left' style='color:rgb(230,108,33);line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{ToXml(member.Name)}({ToXml(record.CreateTime.ToString())})</p><p align='left' style='line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{arg}</p>";
			}
			catch (XmlException)
			{
				xmlNode.InnerXml = string.Format("<p align='left' style='color:rgb(230,108,33);line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{0}({1})</p><p align='left' style='line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{2}</p>", ToXml(member.Name), ToXml(record.CreateTime.ToString()), ToXml("[消息包含特殊字符，不能正常显示]"));
			}
		}
		else
		{
			try
			{
				xmlNode.InnerXml = $"<p align='right' style='color:rgb(40,131,243);line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{ToXml(member.Name)}({ToXml(record.CreateTime.ToString())})</p><p align='right' style='line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{arg}</p>";
			}
			catch (XmlException)
			{
				xmlNode.InnerXml = string.Format("<p align='right' style='color:rgb(40,131,243);line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{0}({1})</p><p align='right' style='line-height:1px;margin:0 auto;font: normal 10pt 微软雅黑'>{2}</p>", ToXml(member.Name), ToXml(record.CreateTime.ToString()), ToXml("[消息包含特殊字符，不能正常显示]"));
			}
		}
		elementById.AppendChild(xmlNode);
	}

	private string DealEmoji(string message)
	{
		return Regex.Replace(message, "\\[:([^\\[\\]]+)\\]", "<img src='.//emojis//$1.gif'/>");
	}

	private string ToXml(string str)
	{
		string text = str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
			.Replace("\"", "&quot;")
			.Replace("'", "&apos;");
		return text.Replace("&lt;img src=&apos;", "<img src='").Replace("&apos;/&gt;", "'/>");
	}

	public void Populate(IEnumerable<ChatRecord> records)
	{
		EmptyView();
		if (records == null)
		{
			return;
		}
		foreach (ChatRecord record in records)
		{
			Append(record);
		}
		ScrollEnd();
	}

	public void ScrollEnd()
	{
		try
		{
			_editor.Select(_editor.Text.Length, 0);
			_editor.ScrollIntoView();
		}
		catch (Exception)
		{
		}
	}

	public Control View()
	{
		return _editor;
	}

	public void SetTheme()
	{
		_editor.BackColor = Color.White;
	}

	public void Insert(IEnumerable<ChatRecord> records)
	{
		throw new NotImplementedException();
	}
}
