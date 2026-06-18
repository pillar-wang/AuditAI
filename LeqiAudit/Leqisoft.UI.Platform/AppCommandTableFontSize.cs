using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class AppCommandTableFontSize : AppCommandFontSizeSelector
{
	protected override void FontSizeSelected(FontSizeEventArgs e)
	{
		Program.MainForm.TableEditor.SetFontSize(e.FontSize);
	}
}
