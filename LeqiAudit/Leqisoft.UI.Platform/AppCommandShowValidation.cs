using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowValidation : AppCommandToggleButton
{
	public override string Text => "校验公式";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ToggleValidation;

	protected override string Tooltip => TipResource.校验公式;

	protected override void Pressed()
	{
		Program.MainForm.TableEditor.ShowValidationPane();
	}

	protected override void Unpressed()
	{
		Program.MainForm.TableEditor.HideValidationPane();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table;
	}
}
