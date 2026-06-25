using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class AppCommandDocumentFont : AppCommandFontSelector
{
	protected override void FontSelected(FontFamilyEventArgs e)
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetFont(e.FontFamily);
		}
	}
}
