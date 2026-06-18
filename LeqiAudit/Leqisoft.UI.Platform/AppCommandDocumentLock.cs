using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandDocumentLock : AppCommandToggleButton
{
	public override string Text => "文档锁定";

	public override Image LargeIcon => Resources.ToggleDocLock;

	protected override void Pressed()
	{
		Program.MainForm.CurrentDocumentEditor.LockDocument();
	}

	protected override void Unpressed()
	{
		Program.MainForm.CurrentDocumentEditor.UnlockDocument();
	}
}
