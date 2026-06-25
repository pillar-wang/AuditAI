using C1.Win.C1Ribbon;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public abstract class AppCommandNumericBox : AppCommandBase
{
	public RibbonNumericBoxEx NumericBox { get; private set; }

	public virtual string Text { get; }

	public virtual int Width { get; }

	public sealed override RibbonItem RibbonItem => NumericBox;

	public decimal Value
	{
		get
		{
			return NumericBox.Value;
		}
		set
		{
			NumericBox.Value = value;
		}
	}

	public AppCommandNumericBox()
	{
	}

	protected abstract void Changed(decimal value);

	public override void GenerateRibbonItem()
	{
		if (NumericBox == null)
		{
			NumericBox = new RibbonNumericBoxEx
			{
				Label = Text,
				TextAreaWidth = Width,
				Maximum = 2147483647m
			};
			NumericBox.ValueChanged += delegate
			{
				Changed(NumericBox.Value);
			};
			AttachTooltip();
		}
	}
}
