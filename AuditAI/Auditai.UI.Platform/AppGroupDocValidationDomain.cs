using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

/// <summary>
/// "校验域" Ribbon 组：包含添加校验点、解除校验点、校验域管理等命令。
/// 仅在 Document 视图下可见。
/// </summary>
public class AppGroupDocValidationDomain : AppCommandGroup
{
	public override string Text => "校验域";

	public override Image Image => Resources.ValidationSettings;

	public AppGroupDocValidationDomain()
	{
		base.Commands.Add(AppCommands.AddValidationPoint);
		base.Commands.Add(AppCommands.RemoveValidationPoint);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.DocValidationMgmt);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Document;
	}
}