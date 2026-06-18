namespace Leqisoft.UI.Platform;

public class AppCommandScalePageHeight : AppCommandCheckBox
{
	public override string Text => "一整页高";

	protected override void Checked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.FitPageHeight(IsFit: true);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetFitHeight(true);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetFitHeight(true);
			break;
		}
		AppCommands.WidthScale.Enabled = false;
		AppCommands.HeightScale.Enabled = false;
	}

	protected override void Unchecked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.FitPageHeight(IsFit: false);
			Program.MainForm.Preview.ChangeVertZoom((double)AppCommands.HeightScale.NumericBox.Value);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetFitHeight(false);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetFitHeight(false);
			break;
		}
		bool enabled = !AppCommands.ScalePageWidth.IsChecked && !AppCommands.ScalePageHeight.IsChecked;
		AppCommands.WidthScale.Enabled = enabled;
		AppCommands.HeightScale.Enabled = enabled;
	}
}
