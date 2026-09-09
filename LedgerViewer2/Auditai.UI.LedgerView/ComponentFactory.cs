﻿﻿﻿﻿﻿using System.Drawing;
using System.Windows.Forms;
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
		C1SplitterPanel pnlContent = new C1SplitterPanel();
		pnlContent.Controls.Add(grid);
		pnlContent.Collapsible = false;
		pnlContent.KeepRelativeSize = false;
		pnlContent.SizeRatio = 100.0;
		pnlContent.Resizable = false;
		pnlContent.Dock = PanelDockStyle.Left;
		// 精修：去分割条、统一白底，避免侧栏与内容区之间出现灰色竖条
		C1SplitContainer container = new C1SplitContainer
		{
			Dock = DockStyle.Fill,
			FixedLineWidth = 0,
			SplitterWidth = 0,
			BackColor = Color.White
		};
		container.Panels.Add(pnlSidebar);
		container.Panels.Add(pnlContent);
		return container;
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
