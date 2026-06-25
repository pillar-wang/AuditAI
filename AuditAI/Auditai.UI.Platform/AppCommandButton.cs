using System;
using System.Drawing;
using System.Threading.Tasks;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Platform;

public abstract class AppCommandButton : AppCommandBase
{
	public RibbonButton Button { get; private set; }

	public virtual string Text { get; }

	public virtual Image LargeIcon { get; }

	public virtual Image SmallIcon { get; }

	protected virtual Func<Task> ClickedTask { get; }

	public sealed override RibbonItem RibbonItem => Button;

	protected virtual void Clicked()
	{
	}

	public override void GenerateRibbonItem()
	{
		if (!(Button == null))
		{
			return;
		}
		Button = new RibbonButton
		{
			Text = Text,
			LargeImage = LargeIcon,
			SmallImage = SmallIcon,
			TextImageRelation = ((SmallIcon == null) ? TextImageRelation.ImageAboveText : TextImageRelation.ImageBeforeText)
		};
		if (ClickedTask == null)
		{
			Button.Click += delegate
			{
				Clicked();
			};
		}
		else
		{
			Button.Click += async delegate
			{
				await ClickedTask();
			};
		}
		AttachTooltip();
	}
}
