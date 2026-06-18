namespace Leqisoft.UI.Platform;

public class AppCommandMarginRight : AppCommandNumericBox
{
	public override string Text => "右";

	public override int Width => 26;

	protected override void Changed(decimal value)
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangeRightMargin((double)value);
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetPageRightMargin((double)value);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetMarginRight((double)value);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetMarginRight((double)value);
			break;
		}
	}
}
