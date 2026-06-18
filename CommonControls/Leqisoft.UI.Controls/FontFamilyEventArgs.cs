using System;

namespace Leqisoft.UI.Controls;

public class FontFamilyEventArgs : EventArgs
{
	public string FontFamily { get; set; }

	public FontFamilyEventArgs(string ff)
	{
		FontFamily = ff;
	}
}
