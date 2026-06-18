namespace Leqisoft.UI.Platform;

public class AppTabTable : AppCommandTab
{
	public override string Text => "表格编辑";

	public AppTabTable()
	{
		base.Groups.Add(AppCommandGroups.DocTableStyle);
		base.Groups.Add(AppCommandGroups.TableLock);
		base.Groups.Add(AppCommandGroups.FormatBrush);
		base.Groups.Add(AppCommandGroups.Row);
		base.Groups.Add(AppCommandGroups.Column);
		base.Groups.Add(AppCommandGroups.MergeCell);
		base.Groups.Add(AppCommandGroups.TableCharFormat);
		base.Groups.Add(AppCommandGroups.Align);
		base.Groups.Add(AppCommandGroups.Indent);
		base.Groups.Add(AppCommandGroups.TitleRow);
		base.Groups.Add(AppCommandGroups.TitleColumn);
		base.Groups.Add(AppCommandGroups.DataFormat);
		base.Groups.Add(AppCommandGroups.AuxEdit);
		base.Groups.Add(AppCommandGroups.Find);
		base.Groups.Add(AppCommandGroups.TicketMode);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.EditingColHeader || state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot || state.ViewKind == MainFormView.FormatBrush;
	}

	protected override void Selected()
	{
		base.Selected();
		if (Program.MainForm.TableEditor != null)
		{
			Program.MainForm.TableEditor.RefreshTableLockShowStatus();
		}
	}
}
