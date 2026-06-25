using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

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
