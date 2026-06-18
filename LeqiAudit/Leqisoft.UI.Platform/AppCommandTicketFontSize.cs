using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketFontSize : AppCommandFontSizeSelector
{
	protected override void FontSizeSelected(FontSizeEventArgs e)
	{
		Program.MainForm.TicketDesignEditor.SetFontSize(e.FontSize);
	}
}
