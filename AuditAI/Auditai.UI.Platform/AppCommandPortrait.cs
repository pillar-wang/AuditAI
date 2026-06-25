using System.Drawing;
using Auditai.Model;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandPortrait : AppCommandButton
{
	public override string Text => "纵向";

	public override System.Drawing.Image LargeIcon => Resources.Portrait16;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.Portrait();
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.Portrait();
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.Portrait();
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.Portrait();
			break;
		}
		AppCommands.PaperDirection.SelectPaperDirection(Direction.Vertical);
	}
}
