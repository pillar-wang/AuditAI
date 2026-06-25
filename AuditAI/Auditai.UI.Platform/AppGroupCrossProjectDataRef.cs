﻿using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

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
			&& Program.MainForm.CurrentProject.Kind == Auditai.DTO.ProjectType.Project
			&& state.ViewKind == MainFormView.Table;
	}
}