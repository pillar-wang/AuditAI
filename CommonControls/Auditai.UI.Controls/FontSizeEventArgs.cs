using System;

namespace Auditai.UI.Controls;

public class FontSizeEventArgs : EventArgs
{
	public float FontSize { get; set; }

	public FontSizeEventArgs(float fontSize)
	{
		FontSize = fontSize;
	}
}
