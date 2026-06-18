using System;
using System.Drawing;
using System.Windows.Forms;
using Leqisoft.Model;
using TXTextControl;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandRtfCombo : AppCommandComboBox
{
	protected virtual string PropName1 { get; }

	protected virtual string PropName2 { get; }

	protected virtual TXTextControl.HorizontalAlignment HorizontalAlignment { get; }

	public AppCommandRtfCombo()
	{
	}

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		base.ComboBox.ReadOnly = true;
		base.ComboBox.DropDown += ComboBox_DropDown;
	}

	private void ComboBox_DropDown(object sender, EventArgs e)
	{
		Rectangle itemBounds = RibbonItem.Ribbon.GetItemBounds(RibbonItem);
		FormHeaderEdit form = new FormHeaderEdit();
		form.HorizontalAlignment = HorizontalAlignment;
		form.Closed += delegate
		{
			if (form.DialogResult == DialogResult.OK)
			{
				base.Text = form.PlainText;
				switch (Program.MainForm.State.ViewKind)
				{
				case MainFormView.Table:
					SetProp(Program.MainForm.CurrentTable.PageSetup, form.Rtf);
					Program.MainForm.CurrentTable.TagPageSetupDirty();
					break;
				case MainFormView.TablePreview:
					SetProp(Program.MainForm.CurrentTable.PageSetup, form.Rtf);
					Program.MainForm.CurrentTable.TagPageSetupDirty();
					Program.MainForm.Preview.CreatePaper();
					break;
				case MainFormView.TicketInput:
					SetProp(Program.MainForm.TicketInputEditor.PageSetup, form.Rtf);
					break;
				case MainFormView.TicketPrint:
					SetProp(null, form.Rtf);
					Program.MainForm.TicketPrinter.Populate();
					break;
				}
			}
		};
		form.Show(RibbonItem.Ribbon.PointToScreen(new Point(itemBounds.Left, itemBounds.Bottom)));
		form.Rtf = GetInitRtf();
	}

	private string GetInitRtf()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			return GetProp(Program.MainForm.CurrentTable.PageSetup);
		case MainFormView.TicketPrint:
			return GetProp(null);
		case MainFormView.TicketInput:
			return GetProp(Program.MainForm.TicketInputEditor.PageSetup);
		default:
			return null;
		}
	}

	private void SetProp(PageSetup ps, string rtf)
	{
		Leqisoft.Model.HeaderFooter obj = (Leqisoft.Model.HeaderFooter)typeof(PageSetup).GetProperty(PropName1).GetValue(ps);
		typeof(Leqisoft.Model.HeaderFooter).GetProperty(PropName2).SetValue(obj, rtf);
	}

	private string GetProp(PageSetup ps)
	{
		Leqisoft.Model.HeaderFooter obj = (Leqisoft.Model.HeaderFooter)typeof(PageSetup).GetProperty(PropName1).GetValue(ps);
		return (string)typeof(Leqisoft.Model.HeaderFooter).GetProperty(PropName2).GetValue(obj);
	}
}
