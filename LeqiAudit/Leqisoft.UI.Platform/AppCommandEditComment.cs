using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandEditComment : AppCommandButton
{
	public override Image LargeIcon => Resources.AuxEditComment;

	public override string Text => "编辑注释";

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.SetEditCommentDialog();
	}
}
