using System.Drawing.Printing;

namespace Leqisoft.UI.Platform;

public class AppCommandPaperB5 : AppCommandButton
{
	public override string Text => "B5";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangePaper(PaperKind.B5);
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetPaperKind(PaperKind.B5);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetPaperKind(PaperKind.B5);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetPaperKind(PaperKind.B5);
			break;
		}
		AppCommands.Paper.SelectPaper(PaperKind.B5);
	}
}
