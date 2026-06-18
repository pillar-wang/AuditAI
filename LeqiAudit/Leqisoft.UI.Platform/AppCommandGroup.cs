using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Ribbon;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandGroup
{
	public abstract string Text { get; }

	public RibbonGroup RibbonGroup { get; private set; }

	public virtual Image Image { get; }

	public virtual bool HasLauncherButton { get; }

	public List<AppCommandBase> Commands { get; } = new List<AppCommandBase>();


	protected virtual string Tooltip { get; } = string.Empty;


	public bool Visible
	{
		get
		{
			return RibbonGroup.Visible;
		}
		set
		{
			RibbonGroup.Visible = value;
		}
	}

	public bool Enabled
	{
		get
		{
			return RibbonGroup.Enabled;
		}
		set
		{
			RibbonGroup.Enabled = value;
		}
	}

	public AppCommandGroup()
	{
	}

	public virtual void OnAppStateChanged(AppState state)
	{
		foreach (AppCommandBase command in Commands)
		{
			command.OnAppStateChanged(state);
		}
	}

	public virtual void GenerateRibbonGroup()
	{
		RibbonGroup = new RibbonGroup(Text)
		{
			HasLauncherButton = HasLauncherButton,
			Image = Image
		};
		RibbonGroup.MouseEnter += RibbonGroup_MouseEnter;
		RibbonGroup.MouseLeave += RibbonGroup_MouseLeave;
		RibbonGroup.DialogLauncherClick += delegate
		{
			LauncherButtonClicked();
		};
	}

	protected virtual void LauncherButtonClicked()
	{
	}

	private void RibbonGroup_MouseLeave(object sender, EventArgs e)
	{
		TooltipManager.Instance.Hide();
	}

	private void RibbonGroup_MouseEnter(object sender, EventArgs e)
	{
		if (TooltipManager.Instance.ShouldDisplay && !(Tooltip == string.Empty))
		{
			Point point = RibbonGroup.Ribbon.PointToClient(Control.MousePosition);
			TipInfo tipInfo = TipInfo.Parse(Tooltip);
			TooltipManager.Instance.Show(tipInfo, RibbonGroup.Ribbon, point.X, point.Y);
		}
	}
}
