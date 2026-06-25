﻿using System.Drawing;
using Auditai.PlatformResource;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupCustomFill : AppCommandGroup
{
	public override string Text => "自定义填充";

	public override Image Image => Resources.TableCollect;

	public AppGroupCustomFill()
	{
		base.Commands.Add(AppCommands.CustomFillConfig);
		base.Commands.Add(AppCommands.ExecuteCustomFill);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (SoftwareLicenseManager.IsLedgerModuleEnable() && Program.ClientPlatformType == PlatformType.AuditPlatform)
		{
			base.Visible = state.ViewKind == MainFormView.Table;
		}
		else
		{
			base.Visible = false;
		}
	}
}