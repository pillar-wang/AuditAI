using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandUnderline : AppCommandSplitButton
{
	public override Image SmallIcon => Resources.Underline;

	public AppCommandUnderline()
		: base(new AppCommandBase[1] { AppCommands.DoubleUnderline })
	{
	}

	protected override void Pressed()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetUnderline(true);
		}
	}

	protected override void Unpressed()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetUnderline(false);
		}
	}
}
