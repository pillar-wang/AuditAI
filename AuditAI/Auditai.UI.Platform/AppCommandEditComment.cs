using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandEditComment : AppCommandButton
{
	public override Image LargeIcon => Resources.AuxEditComment;

	public override string Text => "编辑注释";

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.SetEditCommentDialog();
	}
}
