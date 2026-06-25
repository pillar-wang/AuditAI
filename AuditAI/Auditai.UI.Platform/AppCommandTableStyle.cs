using System;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTableStyle : AppCommandGallery
{
	public TableBorderStyle SelectedStyle
	{
		get
		{
			if (base.Gallery.SelectedItem == null)
			{
				return null;
			}
			if (base.Gallery.SelectedItem == AppCommands.TableStyle0.RibbonItem)
			{
				return TableBorderStyles.Grid;
			}
			if (base.Gallery.SelectedItem == AppCommands.TableStyleNoLine.RibbonItem)
			{
				return TableBorderStyles.NoLine;
			}
			if (base.Gallery.SelectedItem == AppCommands.TableStyle1.RibbonItem)
			{
				return TableBorderStyles.ThickUpDownThinBody;
			}
			if (base.Gallery.SelectedItem == AppCommands.TableStyle2.RibbonItem)
			{
				return TableBorderStyles.ThickUpDownDashBody;
			}
			if (base.Gallery.SelectedItem == AppCommands.TableStyle3.RibbonItem)
			{
				return TableBorderStyles.ThickBorderThinBody;
			}
			throw new ArgumentOutOfRangeException();
		}
	}

	public AppCommandTableStyle()
		: base(new AppCommandBase[6]
		{
			AppCommands.TableStyle0,
			AppCommands.TableStyleNoLine,
			AppCommands.TableStyle1,
			AppCommands.TableStyle2,
			AppCommands.TableStyle3,
			AppCommands.TableStyle4
		})
	{
	}
}
