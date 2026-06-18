using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowFormula : AppCommandToggleButton
{
	public override string Text => "运算公式";

	public override Image LargeIcon => Resources.ToggleFormula;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		base.IsPressed = true;
	}

	protected override void Pressed()
	{
		Program.MainForm.ShowFormulaPane();
	}

	protected override void Unpressed()
	{
		Program.MainForm.HideFormulaPane();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.Document;
	}
}
