using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupDocumentCharFormat : AppCommandGroup
{
	public override string Text => "字体样式";

	public override Image Image => Resources.ForeColor;

	public override bool HasLauncherButton => true;

	public AppGroupDocumentCharFormat()
	{
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[1] { AppCommands.DocumentFont }));
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[1] { AppCommands.DocumentFontSize }));
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.Underline);
		base.Commands.Add(AppCommands.DocForeColor);
		base.Commands.Add(AppCommands.DocBackColor);
		base.Commands.Add(AppCommands.Bold);
		base.Commands.Add(AppCommands.GrowFont);
		base.Commands.Add(AppCommands.Subscript);
		base.Commands.Add(AppCommands.Italic);
		base.Commands.Add(AppCommands.ShrinkFont);
		base.Commands.Add(AppCommands.Superscript);
	}

	protected override void LauncherButtonClicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.FontDialog();
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind != MainFormView.DocFormatBrush;
	}
}
