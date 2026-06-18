using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Ribbon;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandBase
{
	public abstract RibbonItem RibbonItem { get; }

	public virtual bool Visible
	{
		get
		{
			return RibbonItem.Visible;
		}
		set
		{
			RibbonItem.Visible = value;
		}
	}

	public virtual bool Enabled
	{
		get
		{
			return RibbonItem.Enabled;
		}
		set
		{
			RibbonItem.Enabled = value;
		}
	}

	protected virtual string Tooltip { get; } = string.Empty;


	public virtual void OnAppStateChanged(AppState state)
	{
	}

	public AppCommandBase()
	{
	}

	protected void AttachTooltip()
	{
		RibbonItem.MouseEnter += RibbonItem_MouseEnter;
		RibbonItem.MouseLeave += RibbonItem_MouseLeave;
	}

	private void RibbonItem_MouseLeave(object sender, EventArgs e)
	{
		TooltipManager.Instance.Hide();
	}

	private void RibbonItem_MouseEnter(object sender, EventArgs e)
	{
		if (TooltipManager.Instance.ShouldDisplay && !(Tooltip == string.Empty))
		{
			Point point = RibbonItem.Ribbon.PointToClient(Control.MousePosition);
			TipInfo tipInfo = TipInfo.Parse(Tooltip);
			TooltipManager.Instance.Show(tipInfo, RibbonItem.Ribbon, point.X, point.Y);
		}
	}

	public abstract void GenerateRibbonItem();
}
