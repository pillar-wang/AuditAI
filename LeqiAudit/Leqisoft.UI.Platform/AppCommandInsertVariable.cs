using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandInsertVariable : AppCommandButton
{
	public override string Text => "引用变量";

	public override Image LargeIcon => Resources.ReferenceManager;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.InsertVariable();
	}
}
