using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.LedgerView;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandLedgerPrintPreview : AppCommandToggleButton
{
	public override string Text => "打印预览";

	public override Image LargeIcon => Resources.PrintPreview;

	protected override void Pressed()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
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
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.LedgerPreview(preview: false);
		}
	}
}
