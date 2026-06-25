namespace Auditai.UI.Platform;

public class AppCommandZoom100 : AppCommandButton
{
	public override string Text => "100%";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetZoomFactor(100);
			break;
		case MainFormView.Pdf:
		case MainFormView.PdfPreview:
			Program.MainForm.PdfViewer.SetZoomFactor(100);
			break;
		case MainFormView.Image:
		case MainFormView.ImagePreview:
			Program.MainForm.ImageEditor.SetZoomFactor(100);
			break;
		}
	}
}
