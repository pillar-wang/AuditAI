﻿﻿﻿﻿﻿﻿namespace Leqisoft.UI.Platform;

public class AppTabAdvanced : AppCommandTab
{
	public override string Text => "高级功能";

	public AppTabAdvanced()
	{
		base.Groups.Add(AppCommandGroups.CollectLedger);
		base.Groups.Add(AppCommandGroups.Consolidate);
		base.Groups.Add(AppCommandGroups.CrossProjectDataRef);
		base.Groups.Add(AppCommandGroups.CustomFill);
		base.Groups.Add(AppCommandGroups.Reference);
		base.Groups.Add(AppCommandGroups.Confirmation);
		base.Groups.Add(AppCommandGroups.BatchColumn);
		base.Groups.Add(AppCommandGroups.ControlFormula);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			if (!(Program.MainForm.CurrentEdition is AppEditionGeneral) && state.ViewKind == MainFormView.Document)
			{
				base.Visible = true;
			}
			else
			{
				base.Visible = false;
			}
		}
		else
		{
			base.Visible = state.ViewKind == MainFormView.Table || (!(Program.MainForm.CurrentEdition is AppEditionGeneral) && state.ViewKind == MainFormView.Document);
		}
	}
}
