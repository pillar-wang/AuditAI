namespace Leqisoft.UI.Platform;

public class AppCommandScalePageWidth : AppCommandCheckBox
{
	public override string Text => "一整页宽";

	protected override void Checked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.FitPageWidth(IsFit: true);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetFitWidth(true);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetFitWidth(true);
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
			Program.MainForm.Preview.FitPageWidth(IsFit: false);
			Program.MainForm.Preview.ChangeHorizZoom((double)AppCommands.WidthScale.Value);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetFitWidth(false);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetFitWidth(false);
			break;
		}
		bool enabled = !AppCommands.ScalePageWidth.IsChecked && !AppCommands.ScalePageHeight.IsChecked;
		AppCommands.WidthScale.Enabled = enabled;
		AppCommands.HeightScale.Enabled = enabled;
	}
}
