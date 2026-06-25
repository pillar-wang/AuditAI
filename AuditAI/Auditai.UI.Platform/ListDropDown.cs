using System;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class ListDropDown
{
	private C1FlexGrid _owner;

	private DropDownViewKind _viewKind;

	private ListDropDownFormBase _currentForm;

	public bool SkipTextChanged { get; set; } = true;


	public C1DropDownControlEx DropDown { get; } = new C1DropDownControlEx
	{
		ShowUpDownButtons = false,
		MouseClickPassThrough = true,
		GapHeight = 0,
		TrimEnd = false,
		TrimStart = false
	};


	public DropDownViewKind ViewKind
	{
		get
		{
			return _viewKind;
		}
		set
		{
			_viewKind = value;
			switch (_viewKind)
			{
			case DropDownViewKind.SimpleList:
				_currentForm = SimpleList;
				break;
			case DropDownViewKind.TreeList:
				_currentForm = TreeList;
				break;
			case DropDownViewKind.SimpleCheckList:
				_currentForm = SimpleCheckedList;
				break;
			case DropDownViewKind.TableList:
				_currentForm = TableList;
				break;
			case DropDownViewKind.TableCheckList:
				_currentForm = TableCheckedList;
				break;
			case DropDownViewKind.MultiList:
				_currentForm = MultiList;
				break;
			case DropDownViewKind.TreeCheckList:
				_currentForm = TreeCheckedList;
				break;
			case DropDownViewKind.MultiCheckList:
				_currentForm = MultiCheckedList;
				break;
			}
			DropDown.DropDownForm = _currentForm.Form;
		}
	}

	public SimpleListDropDownForm SimpleList { get; }

	public TreeListDropDownForm TreeList { get; }

	public SimpleCheckedListDropDownForm SimpleCheckedList { get; }

	public TableListDropDownForm TableList { get; }

	public TableCheckedListDropDownForm TableCheckedList { get; }

	public MultiListDropDownForm MultiList { get; }

	public TreeCheckedListDropDownForm TreeCheckedList { get; }

	public MultiCheckedListDropDownForm MultiCheckedList { get; }

	public ListDropDown(C1FlexGrid owner)
	{
		_owner = owner;
		SimpleList = new SimpleListDropDownForm(this);
		TreeList = new TreeListDropDownForm(this);
		SimpleCheckedList = new SimpleCheckedListDropDownForm(this);
		TableList = new TableListDropDownForm(this);
		TableCheckedList = new TableCheckedListDropDownForm(this);
		MultiList = new MultiListDropDownForm(this);
		TreeCheckedList = new TreeCheckedListDropDownForm(this);
		MultiCheckedList = new MultiCheckedListDropDownForm(this);
		DropDown.KeyDown += DropDown_KeyDown;
		DropDown.TextChanged += DropDown_TextChanged;
		DropDown.DropDownOpened += DropDown_DropDownOpened;
	}

	private void DropDown_DropDownOpened(object sender, EventArgs e)
	{
		Theme.SetCurrentTree(_currentForm.Form);
		_currentForm.OnSetTheme();
	}

	public bool Validate()
	{
		return _currentForm.Validate();
	}

	public void FinishEditing(bool cancel = false)
	{
		_owner.FinishEditing(cancel);
	}

	private void DropDown_TextChanged(object sender, EventArgs e)
	{
		if (!SkipTextChanged)
		{
			string text = DropDown.Text;
			_currentForm.OnTextChanged(text);
			DropDown.OpenDropDown();
		}
	}

	private void DropDown_KeyDown(object sender, KeyEventArgs e)
	{
		_currentForm.OnKeyDown(e);
	}
}
