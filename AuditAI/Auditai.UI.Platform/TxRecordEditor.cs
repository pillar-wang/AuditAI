using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Auditai.Model;
using Auditai.UI.Controls;
using TXTextControl;

namespace Auditai.UI.Platform;

public class TxRecordEditor : IRecordEditor
{
	private TextControl _tx;

	private Color leftColor = Color.FromArgb(234, 108, 77);

	private Color rightColor = Color.FromArgb(40, 152, 248);

	private string _myId => User.Current.Id.ToString();

	public event EventHandler PreviousRecord;

	public TxRecordEditor()
	{
		_tx = new TextControl
		{
			AllowDrop = false,
			AllowUndo = false,
			Dock = DockStyle.Fill,
			BorderStyle = TXTextControl.BorderStyle.None,
			ViewMode = ViewMode.FloatingText,
			EditMode = EditMode.ReadAndSelect
		};
		_tx.MouseEnter += _tx_MouseEnter;
		_tx.MouseLeave += _tx_MouseLeave;
		AttachEvent();
	}

	public Control View()
	{
		return _tx;
	}

	public void Populate(IEnumerable<ChatRecord> records)
	{
		_tx.SuspendDrawing();
		EmptyView();
		if (records == null)
		{
			return;
		}
		foreach (ChatRecord item in records.OrderBy((ChatRecord r) => r.CreateTime))
		{
			Append(item);
		}
		ScrollEnd();
		_tx.ResumeDrawing();
	}

	public void Append(ChatRecord record)
	{
		if (record.FromId == _myId.ToString())
		{
			AppendImpl(record, TXTextControl.HorizontalAlignment.Right, rightColor);
		}
		else
		{
			AppendImpl(record, TXTextControl.HorizontalAlignment.Left, leftColor);
		}
	}

	public void Insert(IEnumerable<ChatRecord> records)
	{
		if (records == null || records.Count() == 0)
		{
			return;
		}
		_tx.SuspendDrawing();
		foreach (ChatRecord item in records.OrderByDescending((ChatRecord r) => r.CreateTime))
		{
			ServerTextControl tx = ServerTXWrapper.Instance.GetTx();
			tx.SelectAll();
			tx.Clear();
			if (item.FromId == _myId.ToString())
			{
				InsertImpl(tx, item, TXTextControl.HorizontalAlignment.Right, rightColor);
			}
			else
			{
				InsertImpl(tx, item, TXTextControl.HorizontalAlignment.Left, leftColor);
			}
			tx.Save(out var binaryData, BinaryStreamType.MSWord);
			_tx.Select(0, 0);
			_tx.Selection.Load(binaryData, BinaryStreamType.MSWord, new LoadSettings());
		}
		_tx.ResumeDrawing();
	}

	public void EmptyView()
	{
		_tx.SelectAll();
		if (_tx.Selection != null)
		{
			_tx.Selection.Text = string.Empty;
		}
	}

	public void ScrollEnd()
	{
		_tx.Select(GetEnd(), 0);
		int num = _tx.InputPosition.Location.Y - PixelToTwip(_tx.Height - 10);
		if (num > 0 && num > _tx.ScrollLocation.Y)
		{
			_tx.ScrollLocation = new Point(0, num);
		}
	}

	public void SetTheme()
	{
		try
		{
			_tx.BackColor = Color.White;
		}
		catch { }
	}

	private void _tx_MouseEnter(object sender, EventArgs e)
	{
		try
		{
			_tx.Cursor = Cursors.IBeam;
		}
		catch { }
	}

	private void _tx_MouseLeave(object sender, EventArgs e)
	{
		try
		{
			_tx.Cursor = Cursors.Default;
		}
		catch { }
	}

	private void _tx_MouseWheel(object sender, MouseEventArgs e)
	{
		if (_tx.ScrollLocation.Y == 0 && e.Delta > 0)
		{
			OnPreviousRecord();
		}
	}

	private int GetEnd()
	{
		return _tx.Text.Length;
	}

	private void AttachEvent()
	{
		_tx.MouseWheel += _tx_MouseWheel;
	}

	private void DettachEvent()
	{
		_tx.MouseWheel -= _tx_MouseWheel;
	}

	private int PixelToTwip(int pixel)
	{
		using Graphics graphics = _tx.CreateGraphics();
		return (int)((float)pixel / graphics.DpiX * 1440f);
	}

	private void OnPreviousRecord()
	{
		DettachEvent();
		this.PreviousRecord?.Invoke(this, EventArgs.Empty);
		AttachEvent();
	}

	private void AppendImpl(ChatRecord record, TXTextControl.HorizontalAlignment horzAlign, Color foreColor)
	{
		Member member = MemberManager.GetInstance().GetMember(record.FromId);
		if (member == null)
		{
			return;
		}
		updateRecordCutTime(record);
		int end = GetEnd();
		_tx.Select(end, 0);
		_tx.Selection.Text = $"{member.Name} {record.CreateTime}";
		_tx.Select(end, GetEnd());
		_tx.Selection.FontName = "微软雅黑";
		_tx.Selection.ForeColor = foreColor;
		_tx.Selection.ParagraphFormat.Alignment = horzAlign;
		_tx.Selection.ParagraphFormat.LeftIndent = 200;
		_tx.Selection.ParagraphFormat.RightIndent = 200;
		_tx.Select(GetEnd(), 0);
		_tx.Selection.Text = "\n";
		end = GetEnd();
		string pattern = "\\[:([^\\[\\]]+)\\]";
		string[] array = Regex.Split(record.Message, pattern);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.StartsWith("emoji"))
			{
				try
				{
					_tx.Select(GetEnd(), 0);
					string filename = ".//emojis//" + text + ".gif";
					TXTextControl.Image image = new TXTextControl.Image(System.Drawing.Image.FromFile(filename));
					_tx.Images.Add(image, _tx.InputPosition.TextPosition);
				}
				catch
				{
				}
			}
			else
			{
				_tx.Select(GetEnd(), 0);
				_tx.Selection.Text = text;
			}
		}
		_tx.Select(end, GetEnd());
		_tx.Selection.FontName = "微软雅黑";
		_tx.Selection.ForeColor = Color.Black;
		_tx.Selection.ParagraphFormat.Alignment = horzAlign;
		_tx.Selection.ParagraphFormat.LeftIndent = 200;
		_tx.Selection.ParagraphFormat.RightIndent = 200;
		_tx.Select(GetEnd(), 0);
		_tx.Selection.Text = "\n";
	}

	private void InsertImpl(ChatRecord record, TXTextControl.HorizontalAlignment horizontal, Color foreColor)
	{
		Member member = MemberManager.GetInstance().GetMember(record.FromId);
		if (member == null)
		{
			return;
		}
		updateRecordCutTime(record);
		_tx.Select(0, 0);
		_tx.Selection.Text = $"{member.Name} {record.CreateTime}";
		int start = _tx.Selection.Start;
		_tx.Select(0, start);
		_tx.Selection.FontName = "微软雅黑";
		_tx.Selection.ForeColor = foreColor;
		_tx.Selection.ParagraphFormat.Alignment = horizontal;
		_tx.Selection.ParagraphFormat.LeftIndent = 200;
		_tx.Selection.ParagraphFormat.RightIndent = 200;
		_tx.Select(start, 0);
		_tx.Selection.Text = "\n";
		int start2 = _tx.Selection.Start;
		start = _tx.Selection.Start;
		string pattern = "\\[:([^\\[\\]]+)\\]";
		string[] array = Regex.Split(record.Message, pattern);
		string[] array2 = array;
		foreach (string text in array2)
		{
			start = _tx.Selection.Start;
			if (text.StartsWith("emoji"))
			{
				try
				{
					_tx.Select(start, 0);
					string filename = ".//emojis//" + text + ".gif";
					TXTextControl.Image image = new TXTextControl.Image(System.Drawing.Image.FromFile(filename));
					_tx.Images.Add(image, _tx.InputPosition.TextPosition);
				}
				catch
				{
				}
			}
			else
			{
				_tx.Select(start, 0);
				_tx.Selection.Text = text;
			}
		}
		start = _tx.Selection.Start;
		_tx.Select(start2, start - start2);
		_tx.Selection.FontName = "微软雅黑";
		_tx.Selection.ForeColor = Color.Black;
		_tx.Selection.ParagraphFormat.Alignment = horizontal;
		_tx.Selection.ParagraphFormat.LeftIndent = 200;
		_tx.Selection.ParagraphFormat.RightIndent = 200;
		start = _tx.Selection.Start + _tx.Selection.Length;
		_tx.Select(start, 0);
		_tx.Selection.Text = "\n";
	}

	private void InsertImpl(ServerTextControl _tx, ChatRecord record, TXTextControl.HorizontalAlignment horizontal, Color foreColor)
	{
		MemberManager instance = MemberManager.GetInstance();
		Member member = instance.GetMember(record.FromId);
		if (member == null)
		{
			return;
		}
		updateRecordCutTime(record);
		_tx.Select(0, 0);
		_tx.Selection.Text = $"{member.Name} {record.CreateTime}";
		int start = _tx.Selection.Start;
		_tx.Select(0, start);
		_tx.Selection.FontName = "微软雅黑";
		_tx.Selection.ForeColor = foreColor;
		_tx.Selection.ParagraphFormat.Alignment = horizontal;
		_tx.Selection.ParagraphFormat.LeftIndent = 200;
		_tx.Selection.ParagraphFormat.RightIndent = 200;
		_tx.Select(start, 0);
		_tx.Selection.Text = "\n";
		int start2 = _tx.Selection.Start;
		start = _tx.Selection.Start;
		string pattern = "\\[:([^\\[\\]]+)\\]";
		string[] array = Regex.Split(record.Message, pattern);
		string[] array2 = array;
		foreach (string text in array2)
		{
			start = _tx.Selection.Start;
			if (text.StartsWith("emoji"))
			{
				try
				{
					_tx.Select(start, 0);
					string filename = ".//emojis//" + text + ".gif";
					TXTextControl.Image image = new TXTextControl.Image(System.Drawing.Image.FromFile(filename));
					_tx.Images.Add(image, _tx.InputPosition.TextPosition);
				}
				catch
				{
				}
			}
			else
			{
				_tx.Select(start, 0);
				_tx.Selection.Text = text;
			}
		}
		start = _tx.Selection.Start;
		_tx.Select(start2, start - start2);
		_tx.Selection.FontName = "微软雅黑";
		_tx.Selection.ForeColor = Color.Black;
		_tx.Selection.ParagraphFormat.Alignment = horizontal;
		_tx.Selection.ParagraphFormat.LeftIndent = 200;
		_tx.Selection.ParagraphFormat.RightIndent = 200;
		start = _tx.Selection.Start + _tx.Selection.Length;
		_tx.Select(start, 0);
		_tx.Selection.Text = "\n";
	}

	private static void updateRecordCutTime(ChatRecord record)
	{
		MemberManager instance = MemberManager.GetInstance();
		MemTab memTab = instance.GetMember(record.ChatId);
		if (memTab == null)
		{
			memTab = instance.GetGroup(record.ChatId);
		}
		if (memTab != null && record.CreateTime < memTab.TempMostEarly)
		{
			memTab.TempMostEarly = record.CreateTime;
		}
	}
}
