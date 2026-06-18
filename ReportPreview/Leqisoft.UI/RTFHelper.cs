using System;
using System.Drawing;
using Leqisoft.Model;
using TXTextControl;

namespace Leqisoft.UI;

public static class RTFHelper
{
	private static readonly ServerTextControl _tx;

	static RTFHelper()
	{
		_tx = new ServerTextControl();
		_tx.Create();
	}

	public static string ParsePlainText(string Rtf)
	{
		if (string.IsNullOrWhiteSpace(Rtf))
		{
			return Rtf;
		}
		_tx.Load(Rtf, StringStreamType.RichTextFormat);
		return _tx.Text;
	}

	public static string MakeBlackWhite(string rtf)
	{
		_tx.Load(rtf, StringStreamType.RichTextFormat);
		_tx.SelectAll();
		_tx.Selection.TextBackColor = Color.Transparent;
		_tx.Selection.ForeColor = Color.Black;
		_tx.Save(out var stringData, StringStreamType.RichTextFormat);
		return stringData;
	}

	public static string SystemVariable(DataReferenceManager drm, string rtf, DataReferenceEvaluationContext context)
	{
		_tx.Load(rtf, StringStreamType.RichTextFormat);
		Tuple<int, int, string> tuple = null;
		while ((tuple = drm.FindIn(_tx.Text, context)) != null)
		{
			_tx.Select(tuple.Item1, tuple.Item2);
			_tx.Selection.Text = tuple.Item3 ?? string.Empty;
		}
		_tx.Save(out var stringData, StringStreamType.RichTextFormat);
		return stringData;
	}
}
