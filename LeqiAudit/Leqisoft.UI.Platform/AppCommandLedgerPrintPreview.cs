using System.Drawing;
using System.Windows.Forms;
using Leqisoft.UI.Controls;
using Leqisoft.UI.LedgerView;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandLedgerPrintPreview : AppCommandToggleButton
{
	public override string Text => "打印预览";

	public override Image LargeIcon => Resources.PrintPreview;

	protected override void Pressed()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
			return;
		}
		try
		{
			Program.MainForm.LedgerPreview(preview: true);
		}
		catch (PreviewNotSupport)
		{
			base.ToggleButton.Pressed = false;
		}
	}

	protected override void Unpressed()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.LedgerPreview(preview: false);
		}
	}
}
