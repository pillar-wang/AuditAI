using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandLockTable : AppCommandToggleButton
{
	public override string Text => "锁定表格";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ToggleLockTable;

	protected override string Tooltip => TipResource.锁定表格;

	protected override void Pressed()
	{
		Program.MainForm.TableEditor.LockTable();
	}

	protected override void Unpressed()
	{
		Program.MainForm.TableEditor.UnlockTable();
	}
}
