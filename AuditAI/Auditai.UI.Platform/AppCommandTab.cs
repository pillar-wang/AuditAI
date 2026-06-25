using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Ribbon;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Platform;

public abstract class AppCommandTab
{
	public abstract string Text { get; }

	public RibbonTab RibbonTab { get; private set; }

	public string Tooltip { get; } = TipResource.主标题菜单标题栏;


	public List<AppCommandGroup> Groups { get; } = new List<AppCommandGroup>();


	public bool Visible
	{
		get
		{
			return RibbonTab.Visible;
		}
		set
		{
			RibbonTab.Visible = value;
		}
	}

	public AppCommandTab()
	{
	}

	private void RibbonTab_MouseLeave(object sender, EventArgs e)
	{
		TooltipManager.Instance.Hide();
	}

	private void RibbonTab_MouseEnter(object sender, EventArgs e)
	{
		if (TooltipManager.Instance.ShouldDisplay && !(Tooltip == string.Empty))
		{
			Point point = RibbonTab.Ribbon.PointToClient(Control.MousePosition);
			TipInfo tipInfo = TipInfo.Parse(Tooltip);
			TooltipManager.Instance.Show(tipInfo, RibbonTab.Ribbon, point.X, point.Y);
		}
	}

	public virtual void OnAppStateChanged(AppState state)
	{
		foreach (AppCommandGroup group in Groups)
		{
			group.OnAppStateChanged(state);
		}
	}

	public void Select()
	{
		RibbonTab.Selected = true;
	}

	protected virtual void Selected()
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.RibbonAdded)
		{
			Program.MainForm.UpdateState(delegate(AppState s)
			{
				s.SelectedTab = this;
			});
			Program.MainForm.SwitchMainView();
		}
	}

	public virtual void GenerateRibbonTab()
	{
		RibbonTab = new RibbonTab(Text);
		RibbonTab.MouseEnter += RibbonTab_MouseEnter;
		RibbonTab.MouseLeave += RibbonTab_MouseLeave;
		RibbonTab.Select += delegate
		{
			Selected();
		};
	}
}
