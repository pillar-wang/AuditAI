namespace Leqisoft.UI.Platform;

public class AppCommandMarginBottom : AppCommandNumericBox
{
	public override string Text => "下";

	public override int Width => 26;

	protected override void Changed(decimal value)
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangeBottomMargin((double)value);
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetPageBottomMargin((double)value);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetMarginBottom((double)value);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetMarginBottom((double)value);
			break;
		}
	}
}
