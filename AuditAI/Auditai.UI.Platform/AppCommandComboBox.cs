using C1.Win.C1Ribbon;

namespace Auditai.UI.Platform;

public abstract class AppCommandComboBox : AppCommandBase
{
	public RibbonComboBox ComboBox { get; private set; }

	public sealed override RibbonItem RibbonItem => ComboBox;

	public string Text
	{
		get
		{
			return ComboBox.Text;
		}
		set
		{
			ComboBox.Text = value;
		}
	}

	public AppCommandComboBox()
	{
	}

	public override void GenerateRibbonItem()
	{
		if (ComboBox == null)
		{
			ComboBox = new RibbonComboBox();
			AttachTooltip();
		}
	}
}
