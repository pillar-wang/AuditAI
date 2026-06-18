using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandExportImage : AppCommandButton
{
	public override string Text => "图片文件";

	public override Image LargeIcon => Resources.ExportImage;

	protected override void Clicked()
	{
		Program.MainForm.ExportImageDialog();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.ImagePreview;
	}
}
