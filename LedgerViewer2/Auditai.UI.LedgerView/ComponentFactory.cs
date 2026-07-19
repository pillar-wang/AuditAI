﻿﻿﻿using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1SplitContainer;

namespace Auditai.UI.LedgerView;

public static class ComponentFactory
{
	public static C1SplitContainer BuildSidebar(Control grid, C1ToolBar toolBar, out C1SplitterPanel pnlSidebar)
	{
		toolBar.HideFirstDelimiter = true;
		toolBar.ShowToolTips = false;
		toolBar.Horizontal = false;
		toolBar.Dock = DockStyle.Fill;
		toolBar.ButtonLookVert = ButtonLookFlags.TextAndImage;
		toolBar.MinButtonSize = 50;
		pnlSidebar = new C1SplitterPanel();
		pnlSidebar.Controls.Add(toolBar);
		pnlSidebar.Collapsible = false;
		pnlSidebar.KeepRelativeSize = false;
		pnlSidebar.Width = 110;
		pnlSidebar.Resizable = false;
		pnlSidebar.Dock = PanelDockStyle.Right;
		return new C1SplitContainer
		{
			Panels =
			{
				pnlSidebar,
				new C1SplitterPanel
				{
					Controls = { grid },
					Collapsible = false,
					KeepRelativeSize = false,
					SizeRatio = 100.0,
					Resizable = false,
					Dock = PanelDockStyle.Left
				}
			},
			Dock = DockStyle.Fill,
			FixedLineWidth = 0
		};
	}

	/// <summary>
	/// 在主题应用后恢复垂直工具栏的文字显示。
	/// 在每个编辑器的 SetTheme() 方法中调用。
	/// </summary>
	public static void RestoreSidebarToolBar(C1SplitterPanel pnlSidebar)
	{
		if (pnlSidebar == null) return;
		foreach (Control control in pnlSidebar.Controls)
		{
			if (control is C1ToolBar toolBar && !toolBar.Horizontal)
			{
				toolBar.ButtonLookVert = ButtonLookFlags.TextAndImage;
				foreach (C1CommandLink link in toolBar.CommandLinks)
				{
					link.ButtonLook = ButtonLookFlags.TextAndImage;
				}
				toolBar.Invalidate();
			}
		}
	}
}
