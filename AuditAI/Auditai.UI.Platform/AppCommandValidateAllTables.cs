using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandValidateAllTables : AppCommandButton
{
	public override string Text => "全部表校验";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ValidateAllTables;

	protected override string Tooltip => TipResource.全部表校验;

	protected override async void Clicked()
	{
		await Program.MainForm.ValidateAll();
	}
}
