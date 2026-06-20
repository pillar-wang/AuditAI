using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupParagraphFormat : AppCommandGroup
{
	public override string Text => "段落格式";

	public override Image Image => Resources.LineSpacing;

	public override bool HasLauncherButton => true;

	public AppGroupParagraphFormat()
	{
		base.Commands.Add(AppCommands.LineSpacing);
		base.Commands.Add(AppCommands.AboveSpacing);
		base.Commands.Add(AppCommands.BelowSpacing);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.ParagraphAlign);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.IndentFirstLine);
		base.Commands.Add(AppCommands.UnindentFirstLine);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.IndentParagraph);
		base.Commands.Add(AppCommands.UnindentParagraph);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.AutoNumber);
	}

	protected override void LauncherButtonClicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.ParaFormatDialog();
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind != MainFormView.DocFormatBrush;
	}
}
