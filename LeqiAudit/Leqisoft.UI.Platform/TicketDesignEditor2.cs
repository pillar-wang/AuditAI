﻿﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class TicketDesignEditor2
{
	private readonly Panel _view;
	private TicketDesignTitleFooterEditor2 _titleEditor;
	private TicketDesignTitleFooterEditor2 _footerEditor;
	private TicketDesignTitleFooterEditor2 _activeEditor;

	public Control EditorPanel { get; set; }
	public object TicketVM { get; set; }
	public Leqisoft.Model.Table Table { get; set; }
	public int DragingRow { get; set; }
	public Color NormalCellBorderColor { get; set; }
	public object View => _view;
	public object TitleEditor => _titleEditor;
	public object FooterEditor => _footerEditor;

	public TicketDesignEditor2()
	{
		_view = new Panel
		{
			Dock = DockStyle.Fill,
			AutoScroll = true
		};
	}

	public void Populate()
	{
		if (Table == null)
			return;

		_view.Controls.Clear();

		_titleEditor = new TicketDesignTitleFooterEditor2(this, TicketDesignTitleFooterEditor2.EditorType.Title);
		_footerEditor = new TicketDesignTitleFooterEditor2(this, TicketDesignTitleFooterEditor2.EditorType.Footer);

		var ticket = Table.Ticket;
		var titleVM = new TicketDesignTitleFooterVM(ticket, ticket.Title);
		var footerVM = new TicketDesignTitleFooterVM(ticket, ticket.Footer);

		_titleEditor._vm = titleVM;
		_footerEditor._vm = footerVM;

		_titleEditor.Populate();
		_footerEditor.Populate();

		_titleEditor.View.Dock = DockStyle.Top;
		_footerEditor.View.Dock = DockStyle.Top;

		_view.Controls.Add(_footerEditor.View);
		_view.Controls.Add(_titleEditor.View);
		_view.Controls.SetChildIndex(_titleEditor.View, 0);

		_activeEditor = _titleEditor;
	}

	private TicketDesignTitleFooterEditor2 GetActiveEditor()
	{
		if (_activeEditor != null)
			return _activeEditor;
		return _titleEditor;
	}

	public void AutoAdjustDataGridPosition()
	{
		if (_titleEditor != null)
			_titleEditor.AutoAdjustGridHeight();
		if (_footerEditor != null)
			_footerEditor.AutoAdjustGridHeight();
	}

	public int GetGridWidthWithoutFixedColumn()
	{
		if (EditorPanel != null)
			return EditorPanel.Width;
		return 0;
	}

	public int GetGridFixedColumnWidth()
	{
		return 0;
	}

	public Size GetEditorPanelSize()
	{
		if (EditorPanel != null)
			return EditorPanel.ClientSize;
		return Size.Empty;
	}

	public void EnterTitleDrag()
	{
		DragingRow = -1;
	}

	public void EnterFooterDrag()
	{
		DragingRow = -1;
	}

	public void EnterTitleEdit()
	{
		_activeEditor = _titleEditor;
	}

	public void EnterFootEdit()
	{
		_activeEditor = _footerEditor;
	}

	public void IncreaseGridWidth(params object[] args)
	{
		// Layout adjustment - no-op for now
	}

	public void BorderStyle2()
	{
		GetActiveEditor()?.BorderStyle2();
	}

	public void SetAlign(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is CellTextAlign align)
			GetActiveEditor()?.SetAlign(align);
	}

	public void SetBold(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is bool b)
			GetActiveEditor()?.SetBold(b);
	}

	public void BorderStyle1()
	{
		GetActiveEditor()?.BorderStyle1();
	}

	public void SetBackColor(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is Color c)
			GetActiveEditor()?.SetBackColor(c);
	}

	public void BorderBottom()
	{
		GetActiveEditor()?.BorderBottom();
	}

	public void BorderRight()
	{
		GetActiveEditor()?.BorderRight();
	}

	public void BorderLeft()
	{
		GetActiveEditor()?.BorderLeft();
	}

	public void BorderNone()
	{
		GetActiveEditor()?.BorderNone();
	}

	public void BorderTop()
	{
		GetActiveEditor()?.BorderTop();
	}

	public void MoveLeftColumn()
	{
		// Column move not directly supported in ticket design
	}

	public void MoveRightColumn()
	{
		// Column move not directly supported in ticket design
	}

	public string GetFormulaContextCellLabel(params object[] args)
	{
		return GetActiveEditor()?.GetFormulaContextCellLabel() ?? "";
	}

	public void AutoAdjustGridWidth()
	{
		_titleEditor?.AutoAdjustGridWidth();
		_footerEditor?.AutoAdjustGridWidth();
	}

	public void OnFormulaEditorBeganEditing()
	{
		// No special handling needed for ticket design
	}

	public void OnFormulaEditorFinishedEditing()
	{
		// No special handling needed for ticket design
	}

	public void BorderAll()
	{
		GetActiveEditor()?.BorderAll();
	}

	public void SetFontFamily(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is string ff)
			GetActiveEditor()?.SetFontFamily(ff);
	}

	public void SetFontSize(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is float fs)
			GetActiveEditor()?.SetFontSize(fs);
	}

	public void SetForeColor(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is Color c)
			GetActiveEditor()?.SetForeColor(c);
	}

	public void SetFormatDefault(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is DataFormatType dft)
			GetActiveEditor()?.SetFormatDefault(dft);
	}

	public void SetItalic(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is bool b)
			GetActiveEditor()?.SetItalic(b);
	}

	public void SetUnderline(params object[] args)
	{
		// TicketDesignCellVM does not support Underline property - no-op
	}

	public void SetDoubleUnderline(params object[] args)
	{
		// TicketDesignCellVM does not support DoubleUnderline property - no-op
	}

	public void GrowFont()
	{
		GetActiveEditor()?.GrowFont();
	}

	public void ShrinkFont()
	{
		GetActiveEditor()?.ShrinkFont();
	}

	public void SetFormatBoolean(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is DataFormatType dft)
			GetActiveEditor()?.SetFormatDefault(dft);
	}

	public void SetFormatNumeric(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is DataFormatType dft)
			GetActiveEditor()?.SetFormatNumeric(dft);
	}

	public void SetFormatDate(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is DataFormatType dft)
			GetActiveEditor()?.SetFormatDefault(dft);
	}

	public void SetFormatText(params object[] args)
	{
		GetActiveEditor()?.SetFormatText();
	}

	public void SetFormatTime(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is DataFormatType dft)
			GetActiveEditor()?.SetFormatDefault(dft);
	}

	public void SetComboListDialog()
	{
		// Not supported in ticket design mode
	}

	public void SetEditCommentDialog()
	{
		// Not supported in ticket design mode
	}

	public void ImportXlsx(params object[] args)
	{
		// Import Excel ticket style - placeholder
	}

	public void ImportTable(params object[] args)
	{
		// Import table ticket style - placeholder
	}

	public void MorePrecision(params object[] args)
	{
		GetActiveEditor()?.MorePrecision();
	}

	public void LessPrecision(params object[] args)
	{
		GetActiveEditor()?.LessPrecision();
	}

	public void Indent(params object[] args)
	{
		GetActiveEditor()?.Indent();
	}

	public void Unindent(params object[] args)
	{
		GetActiveEditor()?.Unindent();
	}

	public void SetZeroFormat(params object[] args)
	{
		if (args != null && args.Length > 0 && args[0] is ZeroFormat zf)
			GetActiveEditor()?.SetZeroFormat(zf);
	}
}
