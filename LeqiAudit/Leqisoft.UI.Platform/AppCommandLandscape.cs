using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandLandscape : AppCommandButton
{
	public override string Text => "横向";

	public override System.Drawing.Image LargeIcon => Resources.Landscape16;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.Landscape();
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.Landscape();
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.Landscape();
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.Landscape();
			break;
		}
		AppCommands.PaperDirection.SelectPaperDirection(Direction.Horizontal);
	}
}
