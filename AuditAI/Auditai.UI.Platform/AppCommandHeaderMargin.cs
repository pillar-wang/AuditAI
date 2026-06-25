namespace Auditai.UI.Platform;

public class AppCommandHeaderMargin : AppCommandNumericBox
{
	public override string Text => "页眉边距";

	public override int Width => 26;

	protected override void Changed(decimal value)
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangeHeaderMargin((double)value);
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetHeaderMargin((int)value);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetHeaderMargin((double)value);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetHeaderMargin((double)value);
			break;
		}
	}
}
