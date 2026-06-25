namespace Auditai.UI.Platform;

public class AppCommandZoom75 : AppCommandButton
{
	public override string Text => "75%";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetZoomFactor(75);
			break;
		case MainFormView.Pdf:
		case MainFormView.PdfPreview:
			Program.MainForm.PdfViewer.SetZoomFactor(75);
			break;
		case MainFormView.Image:
		case MainFormView.ImagePreview:
			Program.MainForm.ImageEditor.SetZoomFactor(75);
			break;
		}
	}
}
