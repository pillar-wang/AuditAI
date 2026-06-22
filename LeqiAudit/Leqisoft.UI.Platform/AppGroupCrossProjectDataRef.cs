﻿﻿﻿using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupCrossProjectDataRef : AppCommandGroup
{
	public override string Text => "跨项目引用";

	public override System.Drawing.Image Image => Resources.Intelliref;

	public AppGroupCrossProjectDataRef()
	{
		base.Commands.Add(AppCommands.CrossProjectDataRef);
		base.Commands.Add(AppCommands.RefreshCrossProjectRefs);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = Program.MainForm.CurrentProject != null
			&& Program.MainForm.CurrentProject.Kind == Leqisoft.DTO.ProjectType.Project
			&& state.ViewKind == MainFormView.Table;
	}
}