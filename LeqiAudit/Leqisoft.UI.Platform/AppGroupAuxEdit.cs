using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupAuxEdit : AppCommandGroup
{
	public override string Text => "辅助编辑";

	public override Image Image => Resources.ComboList;

	public AppGroupAuxEdit()
	{
		base.Commands.Add(AppCommands.AuxEdit);
		base.Commands.Add(AppCommands.InsertSymbol);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct() && (state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot))
		{
			base.Visible = false;
		}
	}
}
