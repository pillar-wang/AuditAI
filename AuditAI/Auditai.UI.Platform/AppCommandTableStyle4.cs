using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTableStyle4 : AppCommandGalleryItem
{
	public override string Text => "样式4";

	public override Image LargeImage => Resources.TableStyle4;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.SetSidaStyle();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document;
	}
}
