using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTablePrintPreview : AppCommandToggleButton
{
	public override string Text => "打印预览";

	public override Image LargeIcon => Resources.PrintPreview;

	protected override void Pressed()
	{
		base.ToggleButton.Text = "退出预览";
		Program.MainForm.SwitchToPreview();
	}

	protected override void Unpressed()
	{
		base.ToggleButton.Text = "打印预览";
		Program.MainForm.SwitchToNormalView();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
		base.IsPressed = state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.TicketPrint;
	}
}
