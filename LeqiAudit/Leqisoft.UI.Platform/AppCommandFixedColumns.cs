namespace Leqisoft.UI.Platform;

public class AppCommandFixedColumns : AppCommandNumericBox
{
	public override string Text => "固定列数";

	public override int Width => 20;

	protected override void Changed(decimal value)
	{
		Program.MainForm.Preview.SetFixedColCount((int)value);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview;
	}
}
