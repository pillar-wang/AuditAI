using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupAlign : AppCommandGroup
{
	public override string Text => "对齐设置";

	public override Image Image => Resources.tb_AlignTopLeft;

	public AppGroupAlign()
	{
		base.Commands.Add(AppCommands.AlignTopLeft);
		base.Commands.Add(AppCommands.AlignMiddleLeft);
		base.Commands.Add(AppCommands.AlignBottomLeft);
		base.Commands.Add(AppCommands.AlignTopCenter);
		base.Commands.Add(AppCommands.AlignMiddleCenter);
		base.Commands.Add(AppCommands.AlignBottomCenter);
		base.Commands.Add(AppCommands.AlignTopRight);
		base.Commands.Add(AppCommands.AlignMiddleRight);
		base.Commands.Add(AppCommands.AlignBottomRight);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.EditingColHeader || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
