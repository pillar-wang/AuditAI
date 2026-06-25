using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class AppCommandDocumentFontSize : AppCommandFontSizeSelector
{
	protected override void FontSizeSelected(FontSizeEventArgs e)
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetFontSize(e.FontSize);
		}
	}
}
