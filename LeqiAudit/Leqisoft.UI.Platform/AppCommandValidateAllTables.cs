using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandValidateAllTables : AppCommandButton
{
	public override string Text => "全部表校验";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ValidateAllTables;

	protected override string Tooltip => TipResource.全部表校验;

	protected override async void Clicked()
	{
		await Program.MainForm.ValidateAll();
	}
}
