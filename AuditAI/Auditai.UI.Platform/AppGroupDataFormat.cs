using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupDataFormat : AppCommandGroup
{
	public override string Text => "数据格式";

	public override Image Image => Resources.DataFormat;

	public AppGroupDataFormat()
	{
		base.Commands.Add(AppCommands.DataFormat);
		base.Commands.Add(AppCommands.ZeroFormat);
		base.Commands.Add(AppCommands.MorePrecision);
		base.Commands.Add(AppCommands.LessPrecision);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
