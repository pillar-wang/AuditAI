﻿using System;
using System.Drawing;
using Leqisoft.Model;
using TXTextControl;

namespace Leqisoft.UI.Controls;

public class RtfHelper
{
	private const string PAGENO = "[PageNo]";

	private const string PAGECOUNT = "[PageCount]";

	private static readonly ServerTextControl _tx;
	private static readonly bool _txAvailable;

	static RtfHelper()
	{
		try
		{
			_tx = new ServerTextControl();
			_tx.Create();
			_txAvailable = true;
		}
		catch
		{
			_tx = null;
			_txAvailable = false;
		}
	}

	public RtfHelper Load(string rtf)
	{
		if (!_txAvailable) return this;

		if (string.IsNullOrWhiteSpace(rtf))
		{
			_tx.ResetContents();
		}
		else
		{
			try
			{
				_tx.Load(rtf, StringStreamType.RichTextFormat);
			}
			catch (Exception exception)
			{
				try
				{
					exception.Log("加载页眉页脚时发生了未预期的异常, 发生错误的字符串内容:" + rtf);
				}
				catch
				{
				}
			}
		}
		return this;
	}

	public RtfHelper Monochrome()
	{
		if (!_txAvailable) return this;
		_tx.SelectAll();
		_tx.Selection.TextBackColor = Color.Transparent;
		_tx.Selection.ForeColor = Color.Black;
		return this;
	}

	public string Save()
	{
		if (!_txAvailable) return "";
		_tx.Save(out var stringData, StringStreamType.RichTextFormat);
		return stringData;
	}

	public RtfHelper ReplacePageNo(int pageNo)
	{
		if (!_txAvailable) return this;
		for (int num = _tx.Find("[PageNo]", 0, FindOptions.NoMessageBox); num >= 0; num = _tx.Find("[PageNo]", 0, FindOptions.NoMessageBox))
		{
			_tx.Select(num, "[PageNo]".Length);
			_tx.Selection.Text = pageNo.ToString();
		}
		return this;
	}

	public RtfHelper ReplacePageCount(int pageCount)
	{
		if (!_txAvailable) return this;
		for (int num = _tx.Find("[PageCount]", 0, FindOptions.NoMessageBox); num >= 0; num = _tx.Find("[PageCount]", 0, FindOptions.NoMessageBox))
		{
			_tx.Select(num, "[PageCount]".Length);
			_tx.Selection.Text = pageCount.ToString();
		}
		return this;
	}

	public RtfHelper ReplaceVariables(DataReferenceManager drm, DataReferenceEvaluationContext context)
	{
		if (!_txAvailable) return this;
		for (Tuple<int, int, string> tuple = drm.FindIn(_tx.Text, context); tuple != null; tuple = drm.FindIn(_tx.Text, context))
		{
			_tx.Select(tuple.Item1, tuple.Item2);
			_tx.Selection.Text = tuple.Item3 ?? "";
		}
		return this;
	}

	public string GetPlainText()
	{
		if (!_txAvailable) return "";
		return _tx.Text;
	}
}
