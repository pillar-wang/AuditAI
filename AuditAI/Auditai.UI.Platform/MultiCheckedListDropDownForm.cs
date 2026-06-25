using System;
using System.Linq;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class MultiCheckedListDropDownForm : MultiListDropDownForm
{
	public MultiCheckedListDropDownForm(ListDropDown owner)
		: base(owner)
	{
	}

	protected override MultiListPage CreatePageForOp(Operand op)
	{
		if (!(op is ValueSetOperand))
		{
			if (!(op is TreeListOperand))
			{
				if (op is TableListOperand)
				{
					return new MultiListTableCheckedPage(this);
				}
				throw new ArgumentOutOfRangeException();
			}
			return new MultiListTreeCheckedPage(this);
		}
		return new MultiListSimpleCheckedPage(this);
	}

	protected override void OnBeforePostChanges()
	{
		base.Text = string.Join("|", from p in base.Pages
			select p.Result into s
			where !string.IsNullOrEmpty(s)
			select s);
	}

	public override bool Validate()
	{
		return _anyEdit;
	}
}
