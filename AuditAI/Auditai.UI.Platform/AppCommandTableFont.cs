using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class AppCommandTableFont : AppCommandFontSelector
{
	protected override void FontSelected(FontFamilyEventArgs e)
	{
		Program.MainForm.TableEditor.SetFontFamily(e.FontFamily);
	}
}
