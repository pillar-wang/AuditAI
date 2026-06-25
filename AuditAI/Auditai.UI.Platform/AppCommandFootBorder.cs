namespace Auditai.UI.Platform;

public class AppCommandFootBorder : AppCommandCheckBox
{
	public override string Text => "表底框线";

	protected override void Checked()
	{
		Program.MainForm.Preview.ToggleNoteBorder();
	}

	protected override void Unchecked()
	{
		Program.MainForm.Preview.ToggleNoteBorder();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview;
	}
}
