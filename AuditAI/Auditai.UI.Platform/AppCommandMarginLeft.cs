namespace Auditai.UI.Platform;

public class AppCommandMarginLeft : AppCommandNumericBox
{
	public override string Text => "左";

	public override int Width => 26;

	protected override void Changed(decimal value)
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangeLeftMargin((double)value);
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetPageLeftMargin((double)value);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetMarginLeft((double)value);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetMarginLeft((double)value);
			break;
		}
	}
}
