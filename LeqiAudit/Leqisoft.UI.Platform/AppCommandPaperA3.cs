using System.Drawing.Printing;

namespace Leqisoft.UI.Platform;

public class AppCommandPaperA3 : AppCommandButton
{
	public override string Text => "A3";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangePaper(PaperKind.A3);
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetPaperKind(PaperKind.A3);
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetPaperKind(PaperKind.A3);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetPaperKind(PaperKind.A3);
			break;
		}
		AppCommands.Paper.SelectPaper(PaperKind.A3);
	}
}
