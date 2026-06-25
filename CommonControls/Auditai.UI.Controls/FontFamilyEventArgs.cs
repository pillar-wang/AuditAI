using System;

namespace Auditai.UI.Controls;

public class FontFamilyEventArgs : EventArgs
{
	public string FontFamily { get; set; }

	public FontFamilyEventArgs(string ff)
	{
		FontFamily = ff;
	}
}
