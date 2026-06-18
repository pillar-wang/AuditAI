using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandExportPdf : AppCommandButton
{
	public override string Text => "Pdf文件";

	public override Image LargeIcon => Resources.PdfExport;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.TableExportPdf();
			break;
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.ExportPdfDocumentDialog();
			break;
		case MainFormView.Pdf:
		case MainFormView.PdfPreview:
			Program.MainForm.ExportPdfDocumentDialog();
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.ExportPdf();
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.ExportPdf();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.Pdf || state.ViewKind == MainFormView.PdfPreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
	}
}
