using System;

namespace Auditai.UI.Controls;

public class TextEventArgs : EventArgs
{
	public string Text { get; set; }

	public TextEventArgs(string text)
	{
		Text = text;
	}
}
