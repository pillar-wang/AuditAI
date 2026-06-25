using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandAutoNumber : AppCommandButton
{
	public override string Text => "自动编号";

	public override Image LargeIcon => Resources.IndexNumber;

	public override Image SmallIcon => Resources.IndexNumber;

	protected override string Tooltip => "自动重排文档中所有编号";

	protected override Func<Task> ClickedTask => async delegate
	{
		try
		{
			var editor = Program.MainForm?.CurrentDocumentEditor;
			if (editor == null) return;

			if (System.Windows.Forms.MessageBox.Show(
				"将自动重排文档中所有编号，是否继续？", "自动编号",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			var structure = editor.Structure;
			int changed = structure.AutoNumber();
			if (changed == 0)
			{
				System.Windows.Forms.MessageBox.Show(
					"编号已正确，无需调整。", "自动编号",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				editor.MakeIds();
				await structure.Populate();
			}
		}
		catch (Exception ex)
		{
			ex.Log("AppCommandAutoNumber.Clicked");
		}
	};
}
