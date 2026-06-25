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
		toolBar.MinButtonSize = 40;
		pnlSidebar = new C1SplitterPanel();
		pnlSidebar.Controls.Add(toolBar);
		pnlSidebar.Collapsible = false;
		pnlSidebar.KeepRelativeSize = false;
		pnlSidebar.Width = 80;
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
}
