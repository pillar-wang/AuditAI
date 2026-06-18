using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketFont : AppCommandFontSelector
{
	protected override void FontSelected(FontFamilyEventArgs e)
	{
		Program.MainForm.TicketDesignEditor.SetFontFamily(e.FontFamily);
	}
}
