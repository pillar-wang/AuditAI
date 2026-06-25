using System.Windows.Forms;
using C1.Win.C1Input;

namespace Auditai.UI.Controls;

public class SpecificalControlFactory
{
	public static C1DateEdit CreateDateInputTextBox(C1DateEdit c1DateEdit = null)
	{
		C1DateEdit dateInput = c1DateEdit ?? new C1DateEdit();
		dateInput.AllowSpinLoop = false;
		dateInput.Calendar.DayNameLength = 1;
		dateInput.CustomFormat = "yyyy-MM-dd";
		dateInput.FormatType = FormatTypeEnum.CustomFormat;
		dateInput.ImagePadding = new Padding(0);
		dateInput.VisibleButtons = DropDownControlButtonFlags.None;
		dateInput.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
		dateInput.KeyDown += delegate(object s1, KeyEventArgs e1)
		{
			if (e1.KeyCode == Keys.Return)
			{
				dateInput.Value = dateInput.Text;
			}
		};
		return dateInput;
	}
}
