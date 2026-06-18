using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandDoubleUnderline : AppCommandToggleButton
{
	public override string Text => "双下划线";

	public override Image LargeIcon => Resources.DoubleUnderline;

	protected override void Pressed()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetDoubleUnderline(underline: true);
		}
	}

	protected override void Unpressed()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetDoubleUnderline(underline: false);
		}
	}
}
