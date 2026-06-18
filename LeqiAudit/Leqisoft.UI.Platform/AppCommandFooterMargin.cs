namespace Leqisoft.UI.Platform;

public class AppCommandFooterMargin : AppCommandNumericBox
{
	public override string Text => "页脚边距";

	public override int Width => 26;

	protected override void Changed(decimal value)
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangeFooterMargin((double)value);
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetFooterMargin((int)value);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetFooterMargin((double)value);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetFooterMargin((double)value);
			break;
		}
	}
}
