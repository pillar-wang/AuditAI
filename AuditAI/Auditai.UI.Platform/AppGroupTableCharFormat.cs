using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupTableCharFormat : AppCommandGroup
{
	public override string Text => "字体样式";

	public override Image Image => Resources.ForeColor;

	public AppGroupTableCharFormat()
	{
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[1] { AppCommands.TableFont }));
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[1] { AppCommands.TableFontSize }));
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.ForeColor);
		base.Commands.Add(AppCommands.Bold);
		base.Commands.Add(AppCommands.GrowFont);
		base.Commands.Add(AppCommands.BackColor);
		base.Commands.Add(AppCommands.Italic);
		base.Commands.Add(AppCommands.ShrinkFont);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.EditingColHeader || state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
