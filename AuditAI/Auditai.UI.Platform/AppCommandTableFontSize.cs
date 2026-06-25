using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class AppCommandTableFontSize : AppCommandFontSizeSelector
{
	protected override void FontSizeSelected(FontSizeEventArgs e)
	{
		Program.MainForm.TableEditor.SetFontSize(e.FontSize);
	}
}
