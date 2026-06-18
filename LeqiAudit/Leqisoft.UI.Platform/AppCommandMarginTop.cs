namespace Leqisoft.UI.Platform;

public class AppCommandMarginTop : AppCommandNumericBox
{
	public override string Text => "上";

	public override int Width => 26;

	protected override void Changed(decimal value)
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangeTopMargin((double)value);
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetPageTopMargin((double)value);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetMarginTop((double)value);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetMarginTop((double)value);
			break;
		}
	}
}
