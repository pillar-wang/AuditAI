using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandLockTable : AppCommandToggleButton
{
	public override string Text => "锁定表格";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ToggleLockTable;

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
