using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandShowValidation : AppCommandToggleButton
{
	public override string Text => "校验公式";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ToggleValidation;

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
