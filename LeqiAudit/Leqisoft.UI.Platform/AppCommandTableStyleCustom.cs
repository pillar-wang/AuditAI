using System.Windows.Forms;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

/// <summary>
/// 自定义表格样式命令按钮。
/// 点击弹出 frmTableStyleConfig 对话框配置自定义边框样式，
/// 配置结果写入 AppCommands.PendingCustomStyle 供 InsertModelTable 消费。
/// </summary>
public class AppCommandTableStyleCustom : AppCommandButton
{
	/// <summary>最后一次配置的自定义样式</summary>
	public TableBorderStyle LastConfiguredStyle { get; private set; }

	public AppCommandTableStyleCustom()
	{
		LastConfiguredStyle = TableBorderStyles.CreateCustom();
	}

	public override string Text => "自定义样式";

	protected override void Clicked()
	{
		using (var dlg = new frmTableStyleConfig())
		{
			dlg.LoadFromStyle(LastConfiguredStyle ?? TableBorderStyles.CreateCustom());
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				LastConfiguredStyle = dlg.GetConfiguredStyle();
				AppCommands.PendingCustomStyle = LastConfiguredStyle;
			}
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document;
	}
}
