namespace Auditai.UI.Platform;

public class AppCommandMonochrome : AppCommandCheckBox
{
	public override string Text => "单色打印";

	protected override void Checked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.OneColor(OneColor: true);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetOneColor(true);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetOneColor(true);
			break;
		}
	}

	protected override void Unchecked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.OneColor(OneColor: false);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetOneColor(false);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetOneColor(false);
			break;
		}
	}
}
