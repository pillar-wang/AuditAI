using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandFind : AppCommandButton
{
	public override string Text => "查找替换";

	public override Image LargeIcon => Resources.Replace;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.Replace();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.Replace();
			break;
		}
	}
}
