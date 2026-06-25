﻿﻿﻿using System;
using System.Windows.Forms;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class LedgerCollectFormulaEditor
{
	private frmLedgerCollectFormulaEdit _view;

	public bool IsEditing { get; set; }

	public frmLedgerCollectFormulaEdit View
	{
		get
		{
			return _view;
		}
		set
		{
			_view = value;
		}
	}

	public event EventHandler Closed;

	public LedgerCollectFormulaEditor()
	{
		_view = new frmLedgerCollectFormulaEdit(this);
	}

	public void InsertRefTextAndFocus(string refText)
	{
		try
		{
			if (!string.IsNullOrEmpty(refText) && _view != null)
			{
				_view.rtbFormulaInput.SelectedText = refText;
				_view.rtbFormulaInput.Focus();
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex.Message);
		}
	}

	public void RemoveRefAtPos()
	{
		try
		{
			if (_view != null)
			{
				FormulaDisplay formulaDisplay = new FormulaDisplay(_view.rtbFormulaInput.Text);
				var refAtPos = formulaDisplay.GetRefAtPos(_view.rtbFormulaInput.SelectionStart);
				if (refAtPos != null)
				{
					_view.rtbFormulaInput.Select(refAtPos.Item1, refAtPos.Item2);
					_view.rtbFormulaInput.SelectedText = "";
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex.Message);
		}
	}

	public bool UseWildcard()
	{
		if (_view != null)
		{
			return _view.UseWildcard();
		}
		return false;
	}

	public void New()
	{
		IsEditing = true;
	}

	public void OnClosed()
	{
		IsEditing = false;
		Closed?.Invoke(this, EventArgs.Empty);
	}

	public DialogResult ShowEditor(IWin32Window owner, Table table, Column column)
	{
		try
		{
			IsEditing = true;
			if (_view != null)
			{
				_view.PopulateNavGrid(table, column);
				_view.PopulateBalanceGrid();
				_view.BuildVoucherGrid();
				_view.PrepareToShow();
				return _view.ShowDialog(owner);
			}
		}
		catch (Exception)
		{
		}
		return DialogResult.None;
	}
}
