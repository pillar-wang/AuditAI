namespace Leqisoft.UI.Platform;

public class AppCommandZoom25 : AppCommandButton
{
	public override string Text => "25%";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Document:
		case MainFormView.DocumentPreview:
			Program.MainForm.CurrentDocumentEditor.SetZoomFactor(25);
			break;
		case MainFormView.Pdf:
		case MainFormView.PdfPreview:
			Program.MainForm.PdfViewer.SetZoomFactor(25);
			break;
		case MainFormView.Image:
		case MainFormView.ImagePreview:
			Program.MainForm.ImageEditor.SetZoomFactor(25);
			break;
		}
	}
}
