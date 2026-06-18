using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandValidateDocument : AppCommandButton
{
	public override string Text => "当前文档校验";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ValidateDocument;

	protected override Func<Task> ClickedTask => delegate
	{
		Program.MainForm.CurrentDocumentEditor.StartValidate();
		return Task.CompletedTask;
	};

	protected override string Tooltip => TipResource.当前文档校验;
}
