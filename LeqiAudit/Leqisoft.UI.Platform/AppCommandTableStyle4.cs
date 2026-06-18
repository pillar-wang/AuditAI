using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
