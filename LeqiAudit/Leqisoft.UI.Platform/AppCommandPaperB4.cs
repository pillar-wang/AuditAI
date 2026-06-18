using System.Drawing.Printing;

namespace Leqisoft.UI.Platform;

public class AppCommandPaperB4 : AppCommandButton
{
	public override string Text => "B4";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangePaper(PaperKind.B4);
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetPaperKind(PaperKind.B4);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetPaperKind(PaperKind.B4);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetPaperKind(PaperKind.B4);
			break;
		}
		AppCommands.Paper.SelectPaper(PaperKind.B4);
	}
}
