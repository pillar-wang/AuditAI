using System;
using System.Windows.Forms;
using C1.Win.C1Input;
using Auditai.Model;

namespace Auditai.UI.Platform;

public abstract class ListDropDownFormBase
{
	protected ListDropDown _owner;

	public DropDownForm Form { get; }

	public abstract Operand Op { get; set; }

	protected virtual int MinWidth => 50;

	protected string Text
	{
		get
		{
			return _owner.DropDown.Text;
		}
		set
		{
			_owner.DropDown.Value = value;
		}
	}

	public virtual void Populate()
	{
	}

	public abstract void OnTextChanged(string t);

	public abstract void OnKeyDown(KeyEventArgs e);

	public abstract bool Validate();

	public abstract void OnSetTheme();

	protected virtual void OnBeforePostChanges()
	{
	}

	protected abstract int GetTotalWidth();

	protected ListDropDownFormBase(ListDropDown owner)
	{
		_owner = owner;
		Form = new DropDownForm
		{
			Options = DropDownFormOptionsFlags.AlwaysPostChanges,
			BorderStyle = BorderStyle.Fixed3D
		};
		Form.CancelChanges += Form_CancelChanges;
		Form.PostChanges += Form_PostChanges;
		Form.Open += Form_Open;
	}

	private void Form_Open(object sender, EventArgs e)
	{
		SetWidth();
	}

	private void SetWidth()
	{
		int num = GetTotalWidth();
		int num2 = Math.Max(_owner.DropDown.Width, MinWidth);
		if (num < num2)
		{
			num = num2;
		}
		int num3 = 800;
		if (num > num3)
		{
			num = num3;
		}
		Form.Width = num;
	}

	protected void CloseDropDown()
	{
		_owner.DropDown.CloseDropDown();
	}

	private void Form_PostChanges(object sender, EventArgs e)
	{
		OnBeforePostChanges();
		CloseDropDown();
		_owner.FinishEditing();
	}

	private void Form_CancelChanges(object sender, EventArgs e)
	{
		_owner.FinishEditing(cancel: true);
	}
}
