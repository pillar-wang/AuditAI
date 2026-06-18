using C1.Win.C1Ribbon;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandCheckBox : AppCommandBase
{
	public RibbonCheckBox CheckBox { get; private set; }

	public virtual string Text { get; }

	public sealed override RibbonItem RibbonItem => CheckBox;

	public bool IsChecked
	{
		get
		{
			return CheckBox.Checked;
		}
		set
		{
			CheckBox.Checked = value;
		}
	}

	public AppCommandCheckBox()
	{
	}

	protected abstract void Checked();

	protected abstract void Unchecked();

	public override void GenerateRibbonItem()
	{
		if (!(CheckBox == null))
		{
			return;
		}
		CheckBox = new RibbonCheckBox
		{
			Text = Text
		};
		CheckBox.Click += delegate
		{
			if (CheckBox.Checked)
			{
				Checked();
			}
			else
			{
				Unchecked();
			}
		};
		AttachTooltip();
	}
}
