using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class AppCommandTicketFontSize : AppCommandFontSizeSelector
{
	protected override void FontSizeSelected(FontSizeEventArgs e)
	{
		Program.MainForm.TicketDesignEditor.SetFontSize(e.FontSize);
	}
}
