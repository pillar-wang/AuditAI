namespace Auditai.UI.Platform;

public class AppGroupBatchColumn : AppCommandGroup
{
	public override string Text => "跨表批量操作";

	public AppGroupBatchColumn()
	{
		base.Commands.Add(AppCommands.BatchColumnDuplicate);
		base.Commands.Add(AppCommands.BatchColumnRemove);
		base.Commands.Add(AppCommands.BatchColumnRename);
		base.Commands.Add(AppCommands.GenerateBatchFormula);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			base.Visible = false;
		}
		else
		{
			base.Visible = state.ViewKind == MainFormView.Table;
		}
	}
}
