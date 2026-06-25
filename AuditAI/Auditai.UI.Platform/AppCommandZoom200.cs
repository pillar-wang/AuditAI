namespace Auditai.UI.Platform;

public class AppCommandZoom200 : AppCommandButton
{
	public override string Text => "200%";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetZoomFactor(200);
			break;
		case MainFormView.Pdf:
		case MainFormView.PdfPreview:
			Program.MainForm.PdfViewer.SetZoomFactor(200);
			break;
		case MainFormView.Image:
		case MainFormView.ImagePreview:
			Program.MainForm.ImageEditor.SetZoomFactor(200);
			break;
		}
	}
}
